using System.Net;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MailManager.FileDownload
{
	sealed class WebProtocolSupportConcreteTest : TestCaseWithFactory
	{
		public void TestUserAndPassword()
		{
			SecureQueryString qs = new SecureQueryString();
			qs.UrlEncodeNameAndValue = true;
			qs.Add(WebProtocolSupport.UserQueryStringKey, "jim");
			qs.Add(WebProtocolSupport.PasswordQueryStringKey, "jimspw=&;");
			string url = "http://www.cargowise.com/ftpmirror/ediEnterprise/nosuchfile.txt?"
				+ SecureQueryString.QueryStringKey
				+ '='
				+ WebUtility.UrlEncode(qs.ToString());
			WebProtocolSupportTestSubclass web = new WebProtocolSupportTestSubclass(url);
			WebRequest request = web.GetWebRequestForDownload();
			NetworkCredential credential = (NetworkCredential)request.Credentials;
			AssertEquals("jim", credential.UserName);
			AssertEquals("jimspw=&;", credential.Password);
			AssertEquals("http://www.cargowise.com/ftpmirror/ediEnterprise/nosuchfile.txt", request.RequestUri.OriginalString);
		}
	}
}
