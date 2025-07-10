using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Authentication.Glow.Client;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.Foundation.Http;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class GlowServiceClientTest : TestCase
	{
		public void TestSendAsync_HandlesWebExceptionAsyncWithNullResponse()
		{
			HandlesWebExceptionAsyncWithNullResponseAsync(() => client.SendAsync(new HttpRequestMessage(), HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult();
		}

		public void TestGetAsync_HandlesWebExceptionAsyncWithNullResponse()
		{
			HandlesWebExceptionAsyncWithNullResponseAsync(() => client.GetAsync("https://www.wisetechglobal.com")).GetAwaiter().GetResult();
		}

		public void TestPostAsync_HandlesWebExceptionAsyncWithNullResponse()
		{
			HandlesWebExceptionAsyncWithNullResponseAsync(() => client.PostAsync("https://www.wisetechglobal.com", new StringContent("Test"))).GetAwaiter().GetResult();
		}

		public void TestPostAsJsonAsync_HandlesWebExceptionAsyncWithNullResponse()
		{
			HandlesWebExceptionAsyncWithNullResponseAsync(() => client.PostAsJsonAsync("https://www.wisetechglobal.com", new StringContent("JSON"))).GetAwaiter().GetResult();
		}

		async Task HandlesWebExceptionAsyncWithNullResponseAsync(Func<Task> codeToRun)
		{
			var inner = new WebException("Web Exception", null, WebExceptionStatus.ConnectFailure, null);
			var requestException = new HttpRequestException("Something went wrong", inner);
			mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).Throws(requestException);

			var glowException = await AssertGlowHttpRequestExceptionAsync(codeToRun);
			AssertEquals(0, (int)glowException.StatusCode);
			AssertEquals(requestException, glowException.InnerException);
		}

		public void TestSendAsync_HandlesWebExceptionAsync()
		{
			HandlesWebExceptionAsync(() => client.SendAsync(new HttpRequestMessage(), HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult();
		}

		public void TestGetAsync_HandlesWebExceptionAsync()
		{
			HandlesWebExceptionAsync(() => client.GetAsync("https://www.wisetechglobal.com")).GetAwaiter().GetResult();
		}

		public void TestPostAsync_HandlesWebExceptionAsync()
		{
			HandlesWebExceptionAsync(() => client.PostAsync("https://www.wisetechglobal.com", new StringContent("Test"))).GetAwaiter().GetResult();
		}

		public void TestPostAsJsonAsync_HandlesWebExceptionAsync()
		{
			HandlesWebExceptionAsync(() => client.PostAsJsonAsync("https://www.wisetechglobal.com", new StringContent("JSON"))).GetAwaiter().GetResult();
		}

		async Task HandlesWebExceptionAsync(Func<Task> codeToRun)
		{
			var mockResponse = new Mock<HttpWebResponse>();
			mockResponse.SetupGet(m => m.StatusCode).Returns(HttpStatusCode.BadRequest);
			var inner = new WebException("Web Exception", null, WebExceptionStatus.ConnectFailure, mockResponse.Object);
			var requestException = new HttpRequestException("Something went wrong", inner);
			mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).Throws(requestException);

			var glowException = await AssertGlowHttpRequestExceptionAsync(codeToRun);
			AssertEquals(HttpStatusCode.BadRequest, glowException.StatusCode);
			AssertEquals(requestException, glowException.InnerException);
		}

		public void TestSendAsync_HandlesSocketExceptionAsync()
		{
			HandlesSocketExceptionAsync(() => client.SendAsync(new HttpRequestMessage(), HttpCompletionOption.ResponseHeadersRead)).GetAwaiter().GetResult();
		}

		public void TestGetAsync_HandlesSocketExceptionAsync()
		{
			HandlesSocketExceptionAsync(() => client.GetAsync("https://www.wisetechglobal.com")).GetAwaiter().GetResult();
		}

		public void TestPostAsync_HandlesSocketExceptionAsync()
		{
			HandlesSocketExceptionAsync(() => client.PostAsync("https://www.wisetechglobal.com", new StringContent("Test"))).GetAwaiter().GetResult();
		}

		public void TestPostAsJsonAsync_HandlesSocketExceptionAsync()
		{
			HandlesSocketExceptionAsync(() => client.PostAsJsonAsync("https://www.wisetechglobal.com", new StringContent("JSON"))).GetAwaiter().GetResult();
		}

		async Task HandlesSocketExceptionAsync(Func<Task> codeToRun)
		{
			var inner = new SocketException();
			var requestException = new HttpRequestException("Something went wrong", inner);
			mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>()).Throws(requestException);

			var glowException = await AssertGlowHttpRequestExceptionAsync(codeToRun);
			AssertEquals(0, (int)glowException.StatusCode);
			AssertEquals(requestException, glowException.InnerException);
		}

		public void TestGetAsync_GivenMultipleCalls_ShouldNotAccumulateAcceptHeader()
		{
			IList<string> actualHeaders = new List<string>();
			mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
				{
					actualHeaders.Add(req.Headers.Accept.ToString());
					return new Mock<HttpResponseMessage>().Object;
				});

			client.GetAsync("https://www.wisetechglobal.com").GetAwaiter().GetResult();
			client.GetAsync("https://www.wisetechglobal.com").GetAwaiter().GetResult();
			client.GetAsync("https://www.wisetechglobal.com").GetAwaiter().GetResult();

			var request = new HttpRequestMessage(HttpMethod.Post, "https://www.wisetechglobal.com");
			request.Headers.Add("Accept", "application/xml");
			// we set default Accept header when client is created, SendAsync should be able to override it if user wants something else
			client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();

			AssertArrayEqualsByElements(
				// before WI00431099 we got Accept header accumulated every time GetAsync is called: {'application/json', 'application/json,application/json', ...}
				new[] { "application/json", "application/json", "application/json", "application/xml" },
				actualHeaders.ToArray());
		}

		public void TestGetAsync_GivenMultipleCalls_ShouldNotAccumulateAdditionalHeaders()
		{
			IList<string> actualHeaders = new List<string>();
			mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
				.ReturnsAsync((HttpRequestMessage req, CancellationToken _) =>
				{
					actualHeaders.Add(req.Headers.GetValues("X-Test").First());
					return new Mock<HttpResponseMessage>().Object;
				});

			client.SetAdditionalHeader("X-Test", "Value1");
			client.GetAsync("https://www.wisetechglobal.com").GetAwaiter().GetResult();

			AssertArrayEqualsByElements(["Value1"], actualHeaders.ToArray());
		}

		async Task<GlowHttpRequestException> AssertGlowHttpRequestExceptionAsync(Func<Task> codeToRun)
		{
			try
			{
				await codeToRun();
				throw new Exception($"Expected exception of type {nameof(GlowHttpRequestException)}");
			}
			catch (GlowHttpRequestException ex)
			{
				return ex;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockHandler = new Mock<HttpClientHandler>();

			var mockUserSession = new Mock<IUserSession>();
			mockUserSession.SetupGet(m => m.AuthenticationToken).Returns("TrustMeImADolphin");

			var mockAuthenticationController = new Mock<IAuthenticationController>();

			client = new GlowServiceClient(new Uri("http://test.tst"), mockHandler.Object, new HttpClientFactory(() => mockHandler.Object), mockUserSession.Object, mockAuthenticationController.Object);
		}

		GlowServiceClient client;
		Mock<HttpClientHandler> mockHandler;

		protected override void TearDown()
		{
			base.TearDown();

			if (client != null)
			{
				client.Dispose();
			}
		}
	}
}
