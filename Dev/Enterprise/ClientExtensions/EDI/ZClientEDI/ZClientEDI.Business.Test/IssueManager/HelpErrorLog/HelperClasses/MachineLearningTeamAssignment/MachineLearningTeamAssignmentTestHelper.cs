using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Client.EDI.IssueManager.Business.Tests
{
	public static class MachineLearningTeamAssignmentTestHelper
	{
		public static HttpMessageHandler GetHttpMessageHandler(HttpStatusCode? httpStatusCode, string responseJson = "", Exception exceptionToThrow = null, Action<HttpRequestMessage> assertSendAsyncAction = null)
		{
			var httpResponse = httpStatusCode.HasValue ? new HttpResponseMessage(httpStatusCode.Value)
			{
				Content = new StringContent(responseJson)
				{
					Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
				}
			} : null;

			return new MockHttpHandler(httpResponse, exceptionToThrow, assertSendAsyncAction);
		}
	}

	public class MockHttpHandler : HttpClientHandler
	{
		readonly HttpResponseMessage response;
		readonly Exception exceptionToThrow;
		readonly Action<HttpRequestMessage> assertSendAsyncAction;

		public MockHttpHandler(HttpResponseMessage response, Exception exceptionToThrow = null, Action<HttpRequestMessage> assertSendAsyncAction = null)
		{
			this.response = response;
			this.exceptionToThrow = exceptionToThrow;
			this.assertSendAsyncAction = assertSendAsyncAction;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			assertSendAsyncAction?.Invoke(request);

			if (exceptionToThrow != null)
			{
				throw exceptionToThrow;
			}

			return Task.FromResult(response);
		}
	}
}
