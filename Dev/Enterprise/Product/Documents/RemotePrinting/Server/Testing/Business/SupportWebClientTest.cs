using System.Globalization;
using System.Net;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.RemotePrinting.Server.Support;
using Enterprise.RemotePrinting.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	public class SupportWebClientTest : TransactionedTestCase
	{
		[TestDate(2021, 08, 27, 12, 56, 24)]
		public void TestGetSignalRClients()
		{
			var printers = new[] { "print1", "print2", "print3" };
			RemoteHub.Controller.RegisterClientForTest("client1", "server1", printers, "1.0");

			var webClient = new SupportWebClientForTest();
			webClient.ResponseContent =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfSignalRClientInfo xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/"">
  <SignalRClientInfo>
    <ClientId>client1</ClientId>
    <ServerName>server1</ServerName>
    <PrintersList>print1</PrintersList>
    <PrintersList>print2</PrintersList>
    <PrintersList>print3</PrintersList>
    <WebPrintClientVersionNumber>1.0</WebPrintClientVersionNumber>
  </SignalRClientInfo>
</ArrayOfSignalRClientInfo>";

			var signalRClients = webClient.GetSignalRClients("http://10.61.224.25:4545/|host:test.wtg.zone");

			AssertIsValidWebRequest(webClient.LastWebRequest, webClient.LastWebRequestArguments, nameof(SupportService.GetSignalRClients), "http://10.61.224.25:4545/", "test.wtg.zone", "");

			AssertEquals(1, signalRClients.Length);
			var signalRClient = signalRClients[0];
			AssertNotNull(signalRClient);
			AssertEquals("client1", signalRClient.ClientId);
			AssertEquals("server1", signalRClient.ServerName);
			AssertEquals(3, signalRClient.PrintersList.Count);
			AssertEquals("1.0", signalRClient.WebPrintClientVersionNumber);
			AssertEquals("http://10.61.224.25:4545", signalRClient.WebServerAddress);
			AssertEquals("test.wtg.zone", signalRClient.WebServerHostName);
		}

		[TestDate(2021, 08, 27, 12, 56, 24)]
		public void TestRequestClientLogs()
		{
			var webClient = new SupportWebClientForTest();

			var requestDetails = new ClientLogRequestDetails
			{
				EmailAddress = "aaa@bbb.ccc",
				FromDate = ZDateTime.Today.ToDateTime().AddDays(-3),
				ToDate = ZDateTime.Today.ToDateTime().AddDays(1),
				LogTypes = LogTypes.Log | LogTypes.ServiceLog,
			};

			var result = webClient.RequestClientLogs("http://10.61.224.25:4545/", "test.wtg.zone", "client1", requestDetails);
			AssertEquals("Test web request does not work", "Web response status unknown.", result);

			var expectedArguments = new StringBuilder();
			expectedArguments.Append("clientId=").Append(WebUtility.UrlEncode("client1"));
			expectedArguments.Append("&emailAddress=").Append(WebUtility.UrlEncode(requestDetails.EmailAddress));
			expectedArguments.Append("&fromDateString=").Append(WebUtility.UrlEncode(requestDetails.FromDate.ToString(SupportService.DateFormat, CultureInfo.InvariantCulture)));
			expectedArguments.Append("&toDateString=").Append(WebUtility.UrlEncode(requestDetails.ToDate.ToString(SupportService.DateFormat, CultureInfo.InvariantCulture)));
			expectedArguments.Append("&logTypes=").Append(WebUtility.UrlEncode(((int)requestDetails.LogTypes).ToString()));

			AssertIsValidWebRequest(webClient.LastWebRequest, webClient.LastWebRequestArguments, nameof(SupportService.RequestClientLogs), "http://10.61.224.25:4545/", "test.wtg.zone", expectedArguments.ToString());
		}

		public void AssertIsValidWebRequest(HttpWebRequest webRequest, string webRequestArguments, string method, string serviceUrl, string hostName, string expectedArguments)
		{
			AssertNotNull(webRequest);
			AssertEquals(serviceUrl.TrimEnd('/') + "/support/SupportService.asmx/" + method, webRequest.RequestUri);
			AssertEquals(hostName, webRequest.Host);

			AssertEquals("POST", webRequest.Method);

			AssertEquals("CWSupport-" + CWSupportLoginToken.TokenForTest, ((NetworkCredential)webRequest.Credentials).UserName);
			AssertEquals("The password is empty because support user login with token which is in username", "", ((NetworkCredential)webRequest.Credentials).Password);
			AssertEquals("application/x-www-form-urlencoded", webRequest.ContentType);

			if (!expectedArguments.IsNullOrEmpty())
			{
				AssertEquals(expectedArguments.Length, webRequest.ContentLength);
				AssertEquals("WebRequest query string arguments should be correct", expectedArguments, webRequestArguments);
			}
			else
			{
				AssertEquals(0, webRequest.ContentLength);
				Assert("WebRequest query string arguments should be empty", string.IsNullOrEmpty(webRequestArguments));
			}
		}

		public void TestGetSignalRClientsWithWebPrintForceToUseHTTPSForWebPrintRequests()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("IsHostedWithCargowise", true, EnvProxy.IsHostedWithCargowise);

			var webRequest = new Mock<HttpWebRequest>();
			var mockWebClient = new Mock<SupportWebClient>();
			mockWebClient.Protected().Setup<HttpWebRequest>("GetWebRequest", "https://jerry.hostname.com:443/WebPrint", "jerry.hostname.com", "GetSignalRClients", ItExpr.IsNull<string>())
				.Returns((string serviceUrl, string hostName, string methodName, string arguments) => webRequest.Object);

			mockWebClient.Protected().Setup<string>("GetResponseContent", webRequest.Object)
				.Returns(() =>
				{
					return @"<?xml version=""1.0"" encoding=""utf-8""?>
<ArrayOfSignalRClientInfo xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""http://www.cargowise.com/"">
  <SignalRClientInfo>
    <ClientId>client1</ClientId>
    <ServerName>server1</ServerName>
    <PrintersList>print1</PrintersList>
    <PrintersList>print2</PrintersList>
    <PrintersList>print3</PrintersList>
    <WebPrintClientVersionNumber>1.0</WebPrintClientVersionNumber>
  </SignalRClientInfo>
</ArrayOfSignalRClientInfo>";
				});

			var signalRClients = mockWebClient.Object.GetSignalRClients("https://10.61.224.25:443/WebPrint/|host:jerry.hostname.com");

			AssertEquals(1, signalRClients.Length);
			var signalRClient = signalRClients[0];
			AssertNotNull(signalRClient);
			AssertEquals("client1", signalRClient.ClientId);
			AssertEquals("server1", signalRClient.ServerName);
			AssertEquals(3, signalRClient.PrintersList.Count);
			AssertEquals("1.0", signalRClient.WebPrintClientVersionNumber);
			AssertEquals("https://jerry.hostname.com:443/WebPrint", signalRClient.WebServerAddress);
			AssertEquals("jerry.hostname.com", signalRClient.WebServerHostName);
		}

		public void TestGetSignalRClientsShouldNoErrorIfServerAddressIsInvalid()
		{
			var client = new SupportWebClient();
			AssertNoExceptionThrown(() => client.GetSignalRClients(string.Empty));
		}
	}

	public class SupportWebClientForTest : SupportWebClient
	{
		public HttpWebRequest LastWebRequest { get; private set; }

		public string LastWebRequestArguments { get; set; }

		public string ResponseContent { get; set; }

		protected override HttpWebRequest GetWebRequest(string serviceUrl, string hostName, string methodName, string arguments = null)
		{
			LastWebRequestArguments = arguments;
			var webRequest = base.GetWebRequest(serviceUrl, hostName, methodName, arguments);
			LastWebRequest = webRequest;
			return webRequest;
		}

		protected override string GetResponseContent(HttpWebRequest webRequest)
		{
			return ResponseContent;
		}

		protected override WebResponse GetResponse(HttpWebRequest webRequest)
		{
			return null;
		}

		protected override string GetUserName()
		{
			return "CWSupport-" + CWSupportLoginToken.TokenForTest;
		}
	}
}
