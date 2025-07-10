using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Enterprise.RemotePrinting.Client.RemotePrintServer;
using Microsoft.AspNet.SignalR.Client;
using Moq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class ErrorResponseWebRequestProcessorTest : TestCase
	{
		#region TestShouldHandleRedirectResponse

		public void TestShouldHandleRedirectResponse()
		{
			AssertShouldHandleRedirectResponse(HttpStatusCode.MovedPermanently, "http://new.service.url", true, "http://new.service.url");
			AssertShouldHandleRedirectResponse((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), "308"), "http://new.service.url", true, "http://new.service.url");
			AssertShouldHandleRedirectResponse(HttpStatusCode.Found, "http://new.service.url", true, "http://new.service.url");
			AssertShouldHandleRedirectResponse(HttpStatusCode.TemporaryRedirect, "http://new.service.url", true, "http://new.service.url");

			AssertShouldHandleRedirectResponse(HttpStatusCode.MovedPermanently, "test.url", false, "test.url");
			AssertShouldHandleRedirectResponse((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), "308"), "test.url", false, "test.url");
			AssertShouldHandleRedirectResponse(HttpStatusCode.Found, "test.url", false, "test.url");
			AssertShouldHandleRedirectResponse(HttpStatusCode.TemporaryRedirect, "test.url", false, "test.url");
		}

		void AssertShouldHandleRedirectResponse(HttpStatusCode code, string location, bool expectedResult, string expectedUrl)
		{
			var headers = new WebHeaderCollection();
			headers.Add("Location", location);

			var mockHttpWebResponse = new Mock<HttpWebResponse>();
			mockHttpWebResponse.Setup(o => o.StatusCode).Returns(code);
			mockHttpWebResponse.Setup(o => o.Headers).Returns(headers);

			var responseProcessor = new ErrorResponseWebRequestProcessor(new RemotePrintingService());
			var result = responseProcessor.ShouldHandleRedirectResponse(mockHttpWebResponse.Object, out var newUrl);

			AssertEquals(expectedResult, result);
			AssertEquals(expectedUrl, newUrl);
		}

		#endregion

		#region TestProcess

		public void TestProcessWithTemporaryRetryAction()
		{
			AssertProcessWithWebException(HttpStatusCode.Found, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[302]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
			AssertProcessWithWebException(HttpStatusCode.TemporaryRedirect, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://oldurl/RemotePrintingService.asmx", "[307]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);

			AssertProcessWithHttpClientException(HttpStatusCode.Found, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[302]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
			AssertProcessWithHttpClientException(HttpStatusCode.TemporaryRedirect, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://oldurl/RemotePrintingService.asmx", "[307]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
		}

		public void TestProcessWithWebException()
		{
			AssertProcessWithWebException(HttpStatusCode.MovedPermanently, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[301]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", true);
			AssertProcessWithWebException((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), "308"), "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[308]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", true);

			AssertProcessWithWebException(HttpStatusCode.Found, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[302]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
			AssertProcessWithWebException(HttpStatusCode.TemporaryRedirect, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://oldurl/RemotePrintingService.asmx", "[307]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
		}

		public void TestProcessWithHttpClientException()
		{
			AssertProcessWithHttpClientException(HttpStatusCode.MovedPermanently, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[301]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", true);
			AssertProcessWithHttpClientException((HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), "308"), "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[308]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", true);

			AssertProcessWithHttpClientException(HttpStatusCode.Found, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://newurl/RemotePrintingService.asmx", "[302]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
			AssertProcessWithHttpClientException(HttpStatusCode.TemporaryRedirect, "http://oldurl/RemotePrintingService.asmx", "http://newurl", "http://oldurl/RemotePrintingService.asmx", "[307]Retrying operation with new Url: http://newurl:80/RemotePrintingService.asmx", false);
		}

		void AssertProcessWithWebException(HttpStatusCode code, string oldServiceUrl, string newServiceUrl, string expectedServiceUrl, string expectedMessage, bool isPermanentRedirect, bool isTemporaryRetryAction = false)
		{
			var headers = new WebHeaderCollection();
			headers.Add("Location", newServiceUrl);

			var mockHttpWebResponse = new Mock<HttpWebResponse>();
			mockHttpWebResponse.Setup(o => o.StatusCode).Returns(code);
			mockHttpWebResponse.Setup(o => o.Headers).Returns(headers);
			var webException = new WebException("Jerry For Test", null, WebExceptionStatus.ProtocolError, mockHttpWebResponse.Object);

			AssertProcessWithException(webException, oldServiceUrl, expectedServiceUrl, expectedMessage, isPermanentRedirect, isTemporaryRetryAction);
		}

		void AssertProcessWithHttpClientException(HttpStatusCode code, string oldServiceUrl, string newServiceUrl, string expectedServiceUrl, string expectedMessage, bool isPermanentRedirect, bool isTemporaryRetryAction = false)
		{
			var pesponseMessage = new HttpResponseMessage(code);
			pesponseMessage.Headers.Add("Location", newServiceUrl);
			var clientException = new HttpClientException(pesponseMessage);

			AssertProcessWithException(clientException, oldServiceUrl, expectedServiceUrl, expectedMessage, isPermanentRedirect, isTemporaryRetryAction);
		}

		void AssertProcessWithException(Exception exception, string oldServiceUrl, string expectedServiceUrl, string expectedMessage, bool isPermanentRedirect, bool isTemporaryRetryAction)
		{
			var throwException = true;
			var expectedUrl = string.Empty;
			var logs = new List<string>();
			MethodDelegate processAction = () => MethodForTest(exception);
			MethodDelegate retryAction = null;
			Exception outException = null;

			var printingService = new RemotePrintingService();
			var processor = new ErrorResponseWebRequestProcessor(printingService);
			processor.LogInfo = LogInfo;
			printingService.Url = oldServiceUrl;

			AssertNoExceptionThrown(() => processor.Process(processAction, out retryAction, out outException, isTemporaryRetryAction));
			AssertEquals(exception, outException);
			AssertEquals(expectedServiceUrl, processor.ServiceUrl);
			AssertEquals(expectedMessage, string.Join("\r\n", logs));
			AssertNotNull(retryAction);

			if (isPermanentRedirect || isTemporaryRetryAction)
			{
				AssertEquals(processAction, retryAction);
			}

			bool MethodForTest(Exception ex)
			{
				if (throwException)
				{
					throwException = false;
					throw ex;
				}
				return true;
			}

			void LogInfo(string message) => logs.Add(message);
		}

		#endregion
	}
}
