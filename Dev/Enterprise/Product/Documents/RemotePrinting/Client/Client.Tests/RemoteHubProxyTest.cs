using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.RemotePrinting.Types;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	public class RemoteHubProxyTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestRegisterClient_RegisterClientForReconnecting()
		{
			var eventName = nameof(IRemoteClient.RegisterClientForReconnecting);
			var mockConnection = new Mock<IHubConnection>();
			var hubProxy = new HubProxy(mockConnection.Object, eventName);
			var proxy = new RemoteHubProxy(mockConnection.Object, hubProxy, new WebClientConfiguration());
			var client = new Mock<IRemoteClient>();
			proxy.RegisterClient(client.Object);
			hubProxy.InvokeEvent(eventName, Array.Empty<JToken>());

			client.Verify(c => c.RegisterClientForReconnecting(), Times.Once);
		}

		public void TestHandleHttpRequestException()
		{
			AssertHandleHttpRequestException(new HttpRequestException("Response status code does not indicate success: 401 (Authorization Required)."));
			AssertHandleHttpRequestException(new HttpRequestException("Response status code does not indicate success: 403 (Forbidden)."));
			AssertHandleHttpRequestException(new HttpRequestException("Response status code does not indicate success: 404 (Not Found)."));
			AssertHandleHttpRequestException(new HttpRequestException("Response status code does not indicate success: 408 (Request Time-out)."));
			AssertHandleHttpRequestException(new HttpRequestException("Response status code does not indicate success: 503 (Service Unavailable)."));

			AssertHandleHttpRequestException(new HttpRequestException("Error while copying content to a stream.",
				new IOException("Unable to read data from the transport connection: The connection was closed.", ErrorReporter.ConnectionDroppedWhileSending)));
		}

		void AssertHandleHttpRequestException(HttpRequestException requestException)
		{
			var mockHubProxy = GetMockHubProxyWithException(requestException);
			var mockConnection = new Mock<IHubConnection>();

			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration());

			var logs = new StringBuilder();
			proxy.LogError += message => logs.Append(message);

			proxy.Initialise("test", "0.0", new[] { "MyPrinterForTest" });
			proxy.LastInvokedTaskForTest?.Wait(100);

			var exceptionMessage = requestException.InnerException != null ? requestException.InnerException.Message : requestException.Message;
			var expectedMessage = $"Error in SignalR Hub Proxy on Initialise. Error message: {exceptionMessage}";

			AssertEquals(expectedMessage, logs.ToString());
		}

		public void TestHandleHubProxyException()
		{
			var mockHubProxy = GetMockHubProxyWithException(new InvalidOperationException("Connection was disconnected before invocation result was received"));
			var mockConnection = new Mock<IHubConnection>();

			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration());

			var logs = new StringBuilder();
			proxy.LogError += message => logs.Append(message);

			proxy.Initialise("test", "0.0", new[] { "MyPrinterForTest" });
			proxy.LastInvokedTaskForTest?.Wait(100);

			AssertEquals("Error in SignalR Hub Proxy on Initialise. Error message: Connection was disconnected before invocation result was received", logs.ToString());
		}

		public void TestUpdatePrinters_BadRequest_DoNotThrow()
		{
			const string errorMessage = @"StatusCode: 400, ReasonPhrase: 'Bad Request', Version: 1.1, Content: System.Net.Http.StreamContent, Headers:
{
 Pragma: no-cache
 Transfer-Encoding: chunked
 Strict-Transport-Security: max-age=31536000; includeSubdomains; preload
 X-Content-Type-Options: nosniff
 Cache-Control: no-cache
 Date: Sun, 07 Mar 2021 23:52:41 GMT
 Set-Cookie: WEBSVC=bc89fc7369372b00|YEVmn|YEVmh; path=/
 Server: Microsoft-IIS/8.5
 X-AspNet-Version: 4.0.30319
 X-Powered-By: ASP.NET
 Content-Type: text/html
 Expires: -1
}";

			var mockHubProxy = GetMockHubProxyWithException(new HttpClientException(errorMessage));
			var mockConnection = new Mock<IHubConnection>();

			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration());

			var logs = new StringBuilder();
			proxy.LogError += message => logs.Append(message);

			proxy.UpdatePrinters("test", new[] { "MyPrinterForTest" });
			proxy.LastInvokedTaskForTest?.Wait(100);

			AssertEquals("Error in SignalR Hub Proxy on UpdatePrinters. Error message: Bad Request (Status code 400).", logs.ToString());
		}

		public void TestUpdatePrinters_BadRequest_DoNotThrow2()
		{
			var mockHubProxy = GetMockHubProxyWithException(new HttpClientException(new HttpResponseMessage(HttpStatusCode.BadRequest)));
			var mockConnection = new Mock<IHubConnection>();

			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration());

			var logs = new StringBuilder();
			proxy.LogError += message => logs.Append(message);

			proxy.UpdatePrinters("test", new[] { "MyPrinterForTest" });
			proxy.LastInvokedTaskForTest?.Wait(100);

			AssertEquals("Error in SignalR Hub Proxy on UpdatePrinters. Error message: Bad Request (Status code 400).", logs.ToString());
		}

		public void TestUpdatePrinters_GatewayTimeoutt_DoNotThrow()
		{
			var mockHubProxy = GetMockHubProxyWithException(new HttpClientException(new HttpResponseMessage(HttpStatusCode.GatewayTimeout)));
			var mockConnection = new Mock<IHubConnection>();

			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration());

			var logs = new StringBuilder();
			proxy.LogError += message => logs.Append(message);

			proxy.UpdatePrinters("test", new[] { "MyPrinterForTest" });
			proxy.LastInvokedTaskForTest?.Wait(100);

			AssertEquals("Error in SignalR Hub Proxy on UpdatePrinters. Error message: WebPrint suspended due to a slow connection or connection break to gateway. (Status code 504)", logs.ToString());
		}

		public void TestUpdatePrinters_DoNotInvokeForSamePrinters()
		{
			var invokedMethods = new List<string>();
			var mockHubProxy = GetMockHubProxyWithAction(method => invokedMethods.Add(method));
			var mockConnection = new Mock<IHubConnection>();
			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration());

			proxy.Initialise("S1", "1.0.0", new[] { "p1", "p2" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should invoke Initialise", 1, invokedMethods.Count);
			AssertEquals("Initialise", invokedMethods[0]);

			proxy.Initialise("S1", "1.0.0", new[] { "p1", "p2" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should always invoke Initialise", 2, invokedMethods.Count);
			AssertEquals("Initialise", invokedMethods[1]);

			proxy.UpdatePrinters("S1", new[] { "p1", "p2" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should not invoke UpdatePrinters for same printers", 2, invokedMethods.Count);

			proxy.UpdatePrinters("S1", new[] { "p2", "p1" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should not invoke UpdatePrinters for same printers", 2, invokedMethods.Count);

			proxy.UpdatePrinters("S1", new[] { "p1" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should invoke UpdatePrinters for different printers", 3, invokedMethods.Count);
			AssertEquals("UpdatePrinters", invokedMethods[2]);

			proxy.UpdatePrinters("S2", new[] { "p1" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should invoke UpdatePrinters for different server", 4, invokedMethods.Count);
			AssertEquals("UpdatePrinters", invokedMethods[3]);

			proxy.UpdatePrinters("S2", new[] { "p1" });
			proxy.LastInvokedTaskForTest?.Wait(100);
			AssertEquals("Should not invoke UpdatePrinters for same printers", 4, invokedMethods.Count);
		}

		public void TestInitialise_ShouldWaitTaskToComplete()
		{
			AssertInvokeMethod_ShouldWaitTaskToComplete(proxy => proxy.Initialise("S1", "1.0.0", new[] { "p1", "p2" }), nameof(RemoteHubProxy.Initialise), true);
		}

		public void TestSetPrintStatus_ShouldWaitTaskToComplete()
		{
			AssertInvokeMethod_ShouldWaitTaskToComplete(proxy => proxy.SetPrintStatus(Guid.Empty, ProcessedStatus.Processed, string.Empty), nameof(RemoteHubProxy.SetPrintStatus), true);
		}

		public void TestUpdatePrinters_ShouldWaitTaskToComplete()
		{
			AssertInvokeMethod_ShouldWaitTaskToComplete(proxy => proxy.UpdatePrinters("S1", new[] { "p1", "p2" }), nameof(RemoteHubProxy.UpdatePrinters), true);
		}

		public void TestRequestRefreshCNSWClientSetting_ShouldNotWaitTaskToComplete()
		{
			AssertInvokeMethod_ShouldWaitTaskToComplete(proxy => proxy.RequestRefreshCNSWClientSetting("S1"), nameof(RemoteHubProxy.RequestRefreshCNSWClientSetting), false);
		}

		void AssertInvokeMethod_ShouldWaitTaskToComplete(Action<RemoteHubProxy> invokeAction, string methodName, bool shouldComplete)
		{
			var invokedMethods = new List<string>();
			var mockHubProxy = GetMockHubProxyWithAction(method =>
			{
				Thread.Sleep(100);
				invokedMethods.Add(method);
			});
			var mockConnection = new Mock<IHubConnection>();
			var proxy = new RemoteHubProxy(mockConnection.Object, mockHubProxy, new WebClientConfiguration { RemotePrintingServiceTimeoutInSeconds = 10 });

			AssertNull("Precondition: LastInvokedTaskForTest should not be set yet", proxy.LastInvokedTaskForTest);

			invokeAction.Invoke(proxy);

			AssertNotNull("LastInvokedTaskForTest should be set", proxy.LastInvokedTaskForTest);

			if (shouldComplete)
			{
				Assert("Initialise should complete before returning", proxy.LastInvokedTaskForTest.IsCompleted);
				AssertEquals("Should execute action and store invoked method name", 1, invokedMethods.Count);
				AssertEquals(methodName, invokedMethods[0]);
			}
			else
			{
				Assert("Initialise should not complete before returning", !proxy.LastInvokedTaskForTest.IsCompleted);

				proxy.LastInvokedTaskForTest.Wait(200);
			}
		}

		static IHubProxy GetMockHubProxyWithException(Exception exception)
		{
			var mockHubProxy = new Mock<IHubProxy>();
			mockHubProxy.Setup(m => m.Invoke(It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(new Func<string, object[], Task>((method, args) =>
				{
					var tcs = new TaskCompletionSource<string>();
					Task.Run(() =>
					{
						tcs.TrySetException(exception);
					});
					return tcs.Task;
				}));

			return mockHubProxy.Object;
		}

		static IHubProxy GetMockHubProxyWithAction(Action<string> action)
		{
			var mockHubProxy = new Mock<IHubProxy>();
			mockHubProxy.Setup(m => m.Invoke(It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(new Func<string, object[], Task>((method, args) =>
				{
					return Task.Run(() =>
					{
						action?.Invoke(method);
					});
				}));

			return mockHubProxy.Object;
		}
		static IHubProxy GetMockHubProxyWithAsyncAction(Action<string> action)
		{
			var mockHubProxy = new Mock<IHubProxy>();
			mockHubProxy.Setup(m => m.Invoke(It.IsAny<string>(), It.IsAny<object[]>()))
				.Returns(new Func<string, object[], Task>((method, args) =>
				{
					var task = Task.Run(() =>
					{
						action?.Invoke(method);
					});
					return task;
				}));

			return mockHubProxy.Object;
		}
	}
}
