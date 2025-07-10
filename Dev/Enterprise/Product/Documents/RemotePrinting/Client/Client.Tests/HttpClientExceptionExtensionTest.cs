using System.Net;
using System.Net.Http;
using Microsoft.AspNet.SignalR.Client;
using NUnit.Framework;
using WTG.ErrorReporting.Extensibility;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class HttpClientExceptionExtensionTest : TestCase
	{
		public void TestAnalyze()
		{
			var response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
			response.ReasonPhrase = "Bad reason";

			response.Headers.Add("name1", new[] { "value11", "value12" });
			response.Headers.Add("name2", "value2");

			response.RequestMessage = new HttpRequestMessage(HttpMethod.Post, "http://some.url/address");
			response.RequestMessage.Headers.Add("name3", "value3");
			response.RequestMessage.Headers.Add("name4", new[] { "value41", "value42" });

			var httpClientException = new HttpClientException(response);

			var writer = new ExceptionDetailsWriterForTest();
			var context = new ExceptionContext(httpClientException, writer);

			var httpClientExceptionExtension = new HttpClientExceptionExtension();
			((IAdditionalDetailContributor)httpClientExceptionExtension).Analyze(context);

			AssertEquals("http://some.url/address", writer.WrittenDetails["RequestUri"]);
			AssertEquals(nameof(HttpStatusCode.InternalServerError), writer.WrittenDetails["ResponseStatusCode"]);
			AssertEquals("Bad reason", writer.WrittenDetails["ResponseReasonPhrase"]);

			const string expectedResponseHeaders =
@"name1: value11,value12
name2: value2";
			AssertEquals(expectedResponseHeaders, writer.WrittenDetails["ResponseHeaders"]);

			const string expectedRequestHeaders =
@"name3: value3
name4: value41,value42";
			AssertEquals(expectedRequestHeaders, writer.WrittenDetails["RequestHeaders"]);
		}
	}
}
