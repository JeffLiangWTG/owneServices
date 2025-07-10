using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.DocumentEngine.DigitalSignature.EMudhra.V1;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.DocumentEngine.DigitalSignature.Testing
{
	sealed class EMudhraClientTest : TransactionedTestCase
	{
		public void TestWebException()
		{
			var client = new EMudhraClientForTest();
			client.ThrowOnWeb = true;
			client.ThrowWebException = true;
			var list = new List<byte[]>();
			list.Add(new byte[] { 1, 2, 3 });
			var result = client.Sign(list);
			CombineAssertions(() =>
			{
				AssertEquals("WEB_EXCEPTION", result.errorCode);

				Assert(@"Should contain exception messages and stack trace:
System.Net.WebException: Operation is not valid due to the current state of the object.
", result.errorMessage.Contains(@"System.Net.WebException: Operation is not valid due to the current state of the object."));
			});
		}

		public void TestBadResponse()
		{
			var client = new EMudhraClientForTest();
			client.ResponseAsString = "whoa there, slow down";
			client.WebApiUrlSource = "source";
			var list = new List<byte[]>();
			list.Add(new byte[] { 1, 2, 3 });
			var result = client.Sign(list);
			CombineAssertions(() =>
			{
				AssertEquals("BAD_RESPONSE", result.errorCode);

				Assert("Should mention checking the link in the registry. Error message: " + result.errorMessage, result.errorMessage.Contains("Incorrect response received from the API. Please verify the endpoint url in the registry: source"));

				Assert(@"Should contain exception messages and stack trace:
System.InvalidOperationException: Response: whoa there, slow down ---> System.InvalidOperationException: There is an error in XML document (1, 1). ---> System.Xml.XmlException: Data at the root level is invalid. Line 1, position 1.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
   at System.Xml.XmlTextReaderImpl.ParseRootLevelWhitespace()
   at System.Xml.XmlTextReaderImpl.ParseDocumentContent()
", result.errorMessage.Contains(@"Response: whoa there, slow down ---> System.InvalidOperationException: There is an error in XML document (1, 1). ---> System.Xml.XmlException: Data at the root level is invalid. Line 1, position 1.
   at System.Xml.XmlTextReaderImpl.Throw(Exception e)
   at System.Xml.XmlTextReaderImpl.ParseRootLevelWhitespace()
   at System.Xml.XmlTextReaderImpl.ParseDocumentContent()"));
			}
			);
		}

		public void TestNoUnhandled()
		{
			var client = new EMudhraClientForTest();
			var list = new List<byte[]>();
			for (int i = 0; i < 20; i++)
			{
				list.Add(new byte[] { (byte)i });
			}

			client.ThrowOnWeb = true;
			var result = client.Sign(list);
			AssertEquals("UNHANDLED_EXCEPTION", result.errorCode);
		}

		public void TestDoesNotThrow()
		{
			var client = new EMudhraClientForTest();
			var list = new List<byte[]>();
			for (int i = 0; i < 20; i++)
			{
				list.Add(new byte[] { (byte)i });
			}

			client.Response = new SignDocResp();
			var result = client.Sign(list);
			Assert(client.LastSignDocReqAsString.Contains("ts="));
			Assert(client.LastSignDocReqAsString.Contains("txn="));
			Assert(client.LastSignDocReqAsString.Contains("accessKeyhash="));
			AssertEquals("", result.errorCode);
		}

		public void TestSignDocReqAsString()
		{
			var client = new EMudhraClientForTest();
			client.Response = new SignDocResp();
			client.WebApiUri = "http://www.api.com/s.do";
			client.AccessKey = "akey";
			client.ClientID = "cid";
			client.KeyID = "kid";
			client.PartnerID = "pid";
			client.PartnerAccessKey = "pakey";

			client.Sign(new byte[10]);
			AssertContains(@" partnerID=""pid"" partnerAccessKey=""pakey"" sessionKey=""0"">
  <Docs>PEJhc2U2ND4NCiAgPElucHV0SGFzaCBpZD0iMSIgaGFzaEFsZ29yaXRobT0iU0hBMjU2IiByZXNwb25zZVNpZ1R5cGU9IlBLQ1M3Ij4wMDAwMDAwMDAwMDAwMDAwMDAwMDwvSW5wdXRIYXNoPg0KPC9CYXNlNjQ+</Docs>
</SignDocReq>", client.LastSignDocReqAsString);
		}

		public void TestHttpClient()
		{
			var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
			var httpResponseMessage = new HttpResponseMessage()
			{
				Content = new StringContent("Your response text")
			};

			Expression<Func<HttpRequestMessage, bool>> matchUri = x => true; // x.RequestUri == new Uri("https://web.example.com/api");
			Expression<Func<CancellationToken, bool>> anyToken = x => true;

			// SendAsync(matchUri, anyToken) will return a canned response message.
			httpMessageHandlerMock
				.Protected() // required as SendAsync is protected
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is(matchUri), ItExpr.Is(anyToken))
				.ReturnsAsync(httpResponseMessage);

			var httpClientFactory = new Mock<IHttpClientFactory>();
			httpClientFactory.Setup(x => x.Create())
				.Returns(new HttpClient(httpMessageHandlerMock.Object));

			using var temp = ObjectFactory.Substitute(httpClientFactory.Object);

			const string signDocReqAsString = "<SignDocReq\u00a0version=\"1.0\"\u00a0ts=\"2024-07-18T18:26:55+05:30\"\u00a0txn=\"a83a4954ee3245e4bf35b307887b73c3\"\u00a0accessKeyhash=\"a51117b5cae2ec4acec6bbfd1a1cd6e0653d4b3fb51dfaed2a448145e56f116c\"\u00a0sessionKey=\"0\"";
			var client = new EMudhraClient();
			client.WebApiUri = "https://web.example.com/api";

			var response = client.GetHttpResponseString(signDocReqAsString);
			AssertEquals("Your response text", response);
		}
	}
}
