using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.XPath;
using HtmlAgilityPack;
using ScrapySharp.Network;
using System.Collections;
using System.Net;
using System.Collections.Specialized;

namespace pachong
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string key;
        private string region;
        private string url;
        private string page;
        private ArrayList threads = new ArrayList();
        private string username = "";
        private string password = "";
        public MainWindow()
        {
            InitializeComponent();
        }
        

        /// <summary>
        /// 单击查询按钮出发事件
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.key = this.keyBox.Text.Trim();
            this.region = this.regionBox.Text.Trim();
            this.page = this.pageBox.Text.Trim();
            int web =this.webBox.SelectedIndex;
            if (web==0)
            {
                this.url = "http://sou.zhaopin.com/jobs/searchresult.ashx";
                Thread spider = new Thread(new ThreadStart(zhilianwork));
                spider.Name = "智联招聘线程-"+System.DateTime.Now.ToLocalTime();
                spider.Start();
                
            }
            if (web==1)
            {
                this.url = "51job";
            }


       
            //开启线程
            

        }

        /// <summary>
        /// 智联招聘线程
        /// </summary>
        private void zhilianwork()
        {
            //string url = "http://sou.zhaopin.com/jobs/searchresult.ashx?jl=江苏&kw=python";
            string url = this.url;
            Dictionary<string, string> searchParm = new Dictionary<string, string>();
            searchParm.Add("kw",this.key);
            searchParm.Add("jl",this.region);
            searchParm.Add("p", this.page);
            url=Url_Get(url, searchParm);
            //爬取数据
            var uri = new Uri(url);
            var browser1 = new ScrapingBrowser();
            //browser1.Proxy = new WebProxy(new Uri("http://59.173.13.153:8081"));//代理ip
            browser1.Encoding = Encoding.UTF8;
            var html1 = browser1.DownloadString(uri);
            var doc = new HtmlDocument();
            doc.LoadHtml(html1);
            var html = doc.DocumentNode;

            //处理数据
            foreach ( HtmlNode item  in html.SelectNodes("//table[@class='newlist']"))//一条记录
            {
                if (item.SelectNodes("tr/td") == null)
                {
                    continue;
                }

                var tds= item.SelectNodes("tr/td").ToArray();//每一列
                string name=tds[0].SelectSingleNode("div/a").InnerText;
                string link = tds[0].SelectSingleNode("div/a").Attributes["href"].Value;
                string company = tds[2].SelectSingleNode("a").InnerText;
                string salary = tds[3].InnerText;
                string place = tds[4].InnerText;
                string[] timestr=tds[5].SelectSingleNode("span").InnerText.Split('-');
                DateTime date;
                if (timestr.Count() < 3)
                {
                    date = new DateTime(System.DateTime.Now.Year, int.Parse(timestr[0]), int.Parse(timestr[1].ToString()));
                }
                else
                {
                    date = new DateTime(int.Parse(timestr[0]), int.Parse(timestr[1]), int.Parse(timestr[2]));
                }
                using (dbDataContext db = new dbDataContext())
                {
                    workinfo data = new workinfo();
                    data.name = name;
                    data.url = link;
                    data.company = company;
                    data.salary = salary;
                    data.place = place;
                    data.date = date;
                    db.workinfo.InsertOnSubmit(data);
                    db.SubmitChanges();
                }
                

            }

          
        }






        /// <summary>
        /// 拼接url与参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parm"></param>
        /// <returns>返回get字符串</returns>
        public string Url_Get(string url, Dictionary<string, string> parm)
        {
            int i = 0;
            foreach (KeyValuePair<string, string> item in parm)
            {
                if (i == 0)
                {
                    url += string.Format("?{0}={1}", item.Key, item.Value);
                }
                else
                {
                    url += string.Format("&{0}={1}", item.Key, item.Value);
                }
                i++;
            }
            return url;
        }

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            using (dbDataContext db = new dbDataContext())
            {
                this.datashow.ItemsSource = db.workinfo.ToList();
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            //using (dbDataContext db = new dbDataContext())
            //{
            //    this.datashow.ItemsSource = db.workinfo.ToList();
            //}

           // //个人用户登录=========================================================
           // string url = "https://passport.zhaopin.com/";
           // var uri = new Uri(url);
           // var browser1 = new ScrapingBrowser();
           // browser1.UserAgent = new FakeUserAgent("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:42.0) Gecko/20100101 Firefox/42.0");
           // //browser1.Proxy = new WebProxy(new Uri("http://59.173.13.153:8081"));//代理ip
           // browser1.Encoding = Encoding.UTF8;
           // NameValueCollection data=new NameValueCollection();
           // data.Add("LoginName",this.username);
           // data.Add("Password",this.password);
           // data.Add("RememberMe","false");
           // data.Add("bkurl","");
           // string response = browser1.NavigateTo(uri, HttpVerb.Post, data);
           // url="http://i.zhaopin.com";
           // uri=new Uri(url);
           // string html1 = browser1.DownloadString(uri);
           // Console.WriteLine(html1);
           ////==========================================================================


            var uri = new Uri("http://99.zhaopin.com/my/index.jsp");
            var browser1 = new ScrapingBrowser();
            browser1.UserAgent = new FakeUserAgent("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:42.0) Gecko/20100101 Firefox/42.0");
            browser1.Encoding = Encoding.Default;
            

            string cookie=@"pcc=r=308254826&t=0; path=/";
            cookie += @";dywea=95841923.4198357813664735000.1447403203.1447403203.1447403203.1; path=/";
            cookie += @";dyweb=95841923.5.9.1447403213266; path=/";
            cookie += @";dywec=95841923; path=/";
            cookie += @";dywez=95841923.1447403203.1.1.dywecsr=(direct)|dyweccn=(direct)|dywecmd=(none)|dywectr=undefined; path=/";
            cookie += @";__utma=269921210.211437922.1447403204.1447403204.1447403204.1; path=/";
            cookie += @";__utmb=269921210.6.9.1447403213282; path=/";
            cookie += @";__utmc=269921210; path=/";
            cookie += @";__utmz=269921210.1447403204.1.1.utmcsr=(direct)|utmccn=(direct)|utmcmd=(none); path=/";
            cookie += @";JsOrglogin=1982127353; path=/";
            cookie += @";RDsUserInfo=3473207359664564507343675D77556858735573556644645D733F67247758680B7317730166076400731C6701775E6834732A735966551519E60039517727682573597353664664547342675377566851735C735F66316428734E67351BAAEA3C3D213DC02869F4D8F8CE02631035FE0D229C205F6627642B734E67517720682C735973556647645373476753775568547351732466426454734567447706680E7309735F66206432734E675B775E68207330735966426457735E675B775668417355735266496454734667517721682573597353664664547342675377566851735C735F66376428734E67351BAAEA3C3D213DC02869F4D8F8CE02631035FE0D229C205F663F642B734E675A775568517355735F66306422734E675C77566859735F73316627645B7342675A7754685A73277325664E64257330675D775068537355735D66406456734B675D775E682573257359663064257344675F77576850735D73576643645E73446751772168227359735466486435733A6757775D685A732D7334664E645473476758774B68507355735666486427733F67577755685A733; path=/";
            cookie += @";xychkcontr=75219308%2c0; path=/";
            cookie += @";lastchannelurl=http%3A//rd2.zhaopin.com/portal/myrd/regnew.asp%3Fza%3D2; path=/";
            cookie += @";cgmark=2; path=/";
            cookie += @";JsNewlogin=660709117; path=/";
            cookie += @";isNewUser=0; path=/";
            cookie += @";JSESSIONID=453A7D3608AE04E712B011D48EE250EF; path=/";
            cookie += @";__utmt=1; path=/";
            browser1.SetCookies(uri, cookie);
            var html1 = browser1.DownloadString(uri);
            Console.WriteLine(html1);

        }


       
    }
}
