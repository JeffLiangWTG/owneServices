using System.Net;
using CargoWise.Common;
using Enterprise.MailManager.FileDownload;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class PackagePathGeneratorTest : TransactionedTestCase
	{
		public void TestGenerateWebServerRootPath()
		{
			EDIDataRegistry.Instance.WebServerClientSpecificPath = "WebCSP\\";
			EDIDataRegistry.Instance.WebServerGenericPath = "WebGeneric\\";
			AssertEquals("Client-Specific Path", "WebCSP\\x", PackagePathGenerator.GenerateWebServerRootPath("x"));
			AssertEquals("Generic Path", "WebGeneric\\", PackagePathGenerator.GenerateWebServerRootPath(""));
		}

		public void TestGenerateHttpPath()
		{
			EDIDataRegistry.Instance.HttpDownloadUserName = "";
			EDIDataRegistry.Instance.HttpClientSpecificBaseUrl = "HttpCSP/";
			EDIDataRegistry.Instance.HttpGenericBaseUrl = "HttpGeneric/";
			AssertEquals("Client-Specific Path", "HttpCSP/x/y", PackagePathGenerator.GenerateHttpPath("x", "y"));
			AssertEquals("Generic Path", "HttpGeneric/y", PackagePathGenerator.GenerateHttpPath("", "y"));

			EDIDataRegistry.Instance.HttpDownloadUserName = "SecureUpdate";
			EDIDataRegistry.Instance.HttpDownloadPassword = "xyzzy";
			SecureQueryString qs = new SecureQueryString();
			qs.Add(WebProtocolSupport.UserQueryStringKey, UpgradeConstants.HttpDownloadUserName);
			qs.Add(WebProtocolSupport.PasswordQueryStringKey, UpgradeConstants.HttpDownloadPassword);
			string query = SecureQueryString.QueryStringKey + '=' + WebUtility.UrlEncode(qs.ToString());
			AssertEquals("Client-Specific Path", "HttpCSP/x/y?" + query, PackagePathGenerator.GenerateHttpPath("x", "y"));
			AssertEquals("Generic Path", "HttpGeneric/y?" + query, PackagePathGenerator.GenerateHttpPath("", "y"));
		}
	}
}
