using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert.Testing
{
	public class SqlExecutionPlanRetrieverTest : TestCase
	{
		public void TestException_WhenRetrievingTimeout()
		{
			var httpClient = MockHttpClient(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(200),
				new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent("This is a timeout."),
				});
			var retriever = new SqlExecutionPlanRetriever(httpClient);
			var stopwatch = new Stopwatch();
			var searchLink = "https://whatever.website.link.hahaha";

			try
			{
				stopwatch.Start();
				retriever.RetrieveExecutionPlan(searchLink).GetAwaiter().GetResult();
			}
			catch (Exception e)
			{
				stopwatch.Stop();
				Assert(e is HttpRequestException);
				AssertEquals($"Failed to retrieve execution plan from {searchLink}.", e.Message);
				var timeCost = stopwatch.ElapsedMilliseconds;
				Assert($"elapsed time should greater than or equal to timeout setting, but it's {timeCost}", timeCost >= 100);
				Assert($"elapsed time should less that the task waiting time setting, but it's {timeCost}", timeCost < 200);
			}
		}

		public void TestException_WhenReturningFailureStatusCode()
		{
			var httpClient = MockHttpClient(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(200),
				new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.InternalServerError,
					Content = new StringContent("This is a failure."),
				});
			var retriever = new SqlExecutionPlanRetriever(httpClient);
			var searchLink = "https://whatever.website.link.hahaha";

			AssertExceptionThrown<HttpRequestException>("Exception should be thrown when response status does not represent success.",
				$"Failed to retrieve execution plan from {searchLink}. Status code: {HttpStatusCode.InternalServerError}",
				() => retriever.RetrieveExecutionPlan(searchLink).GetAwaiter().GetResult());
		}

		public void TestRetrieveSuccessfully()
		{
			var response = "This is a success. ^_^";
			var httpClient = MockHttpClient(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(200),
				new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(response),
				});
			var retriever = new SqlExecutionPlanRetriever(httpClient);

			var result = retriever.RetrieveExecutionPlan("https://whatever.website.link.hahaha").GetAwaiter().GetResult();
			AssertEquals(response, result);
		}

		HttpClient MockHttpClient(TimeSpan timeout, TimeSpan delayTime, HttpResponseMessage response)
		{
			var handler = new Mock<HttpMessageHandler>();
			handler.Protected()
				.Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync((HttpRequestMessage _, CancellationToken cancellationToken) =>
				{
					Task.Delay(delayTime, cancellationToken).GetAwaiter().GetResult();
					return response;
				});

			return new HttpClient(handler.Object) { Timeout = timeout };
		}
	}
}
