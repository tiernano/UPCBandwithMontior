using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UPCBandwidthMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            var cookies = Login(ConfigurationManager.AppSettings["username"], ConfigurationManager.AppSettings["password"]);
            string json = StreamToString(Request("https://service.upc.ie/cckservices/myupcmyusage", null, cookies, "https://service.upc.ie/cckservices/myupc/").GetResponseStream());
            try
            {
                UPCObject obj = JsonConvert.DeserializeObject<UPCObject>(json);
                Console.WriteLine("Last Updated: {3} Downloaded: {0} Uploaded: {1} total: {2}", obj.downloaded, obj.uploaded, obj.total, obj.lastmoddt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }

        public static CookieContainer Login(string userName, string password)
        {
            CookieContainer tmp = new CookieContainer();
            string LoginURL = "https://service.upc.ie/pkmslogin.form?REDIRURL=https://service.upc.ie/cckservices/myupc//handle-login";
            string ToPost = String.Format("login-form-type=pwd&hid_username=unauthenticated&hid_tamop=login&hid_errorcode=0x00000000&hid_referer=null&form_action=https%3A%2F%2Fservice.upc.ie%2Fpkmslogin.form%3FREDIRURL%3Dhttps%3A%2F%2Fservice.upc.ie%2Fcckservices%2Fmyupc%2F%2Fhandle-login&username={0}&password={1}", userName, password);
            HttpWebResponse Response = Request(LoginURL, ToPost, null, "http://www.upc.ie");
            tmp.Add(Response.Cookies);
            Response.Close();
            return tmp;
        }

        private static HttpWebResponse Request(string URL, string ToPost, CookieContainer Cookies, string Referrer)
        {
            System.Net.ServicePointManager.Expect100Continue = false;
            HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(URL);
            Request.CookieContainer = new CookieContainer();
            Request.Referer = Referrer;
            Request.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 5.1; en-US; rv:1.8.0.6) Gecko/20060728 Firefox/1.5.0.6";

            if (Cookies != null)
                Request.CookieContainer.Add(Cookies.GetCookies(Request.RequestUri));
            if (ToPost != null)
            {
                Request.Method = "POST";
                Request.ContentType = "application/x-www-form-urlencoded";
                byte[] data = System.Text.Encoding.UTF8.GetBytes(ToPost);
                Request.ContentLength = data.Length;
                System.IO.Stream writeStream = Request.GetRequestStream();
                writeStream.Write(data, 0, data.Length);
                writeStream.Close();
            }
            else
            {
                Request.Method = "GET";
            }
            HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            Response.Cookies = Request.CookieContainer.GetCookies(Request.RequestUri);
            return Response;
        }
        private static string StreamToString(System.IO.Stream readStream)
        {
            string result = null;
            string tempstring = null;
            int count = 0;
            byte[] buffer = new byte[8192];
            do
            {
                count = readStream.Read(buffer, 0, buffer.Length);
                if (count != 0)
                {
                    tempstring = System.Text.Encoding.ASCII.GetString(buffer, 0, count);
                    result = result + tempstring;
                }
            }
            while (count > 0);
            readStream.Close();
            return result;
        }
    }

    public class UPCObject
    {
        public string result { get; set; }
        public string lastmoddt { get; set; }
        public string lastmodtm { get; set; }
        public string curr { get; set; }
        public string nxt { get; set; }
        public string downloaded { get; set; }
        public string uploaded { get; set; }
        public string total { get; set; }
        public string cap { get; set; }
    }
}
