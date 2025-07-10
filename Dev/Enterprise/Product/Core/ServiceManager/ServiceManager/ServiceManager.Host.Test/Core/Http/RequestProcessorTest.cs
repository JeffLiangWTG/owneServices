using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Host.Queue;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http
{
	class ManualActionQueue : IActionQueue
	{
		readonly List<Action> actions = new List<Action>();

		public void InvokeActions()
		{
			actions.ForEach((a) => a.Invoke());
			actions.Clear();
		}

		public void Enqueue(Action action) => actions.Add(action);

		public void Enqueue(TimeSpan delaySpan, Action action) => throw new NotImplementedException();
	}

	class RequestProcessorTest : TestCaseWithFactory
	{
		class ShouldProduce404Test : TestCase
		{
			[ExpectNoExceptions]
			public void TestProcessRequestShouldProduce404ForCommandWithEmptyTasks()
			{
				TestGetWebResponse("cargowise/processcontroller/a/b/command?action=bla&tasks=,");
			}

			[ExpectNoExceptions]
			public void TestProcessRequestShouldProduce404ForUnknownRequest()
			{
				TestGetWebResponse("blahblah");
			}

			[ExpectNoExceptions]
			public void TestProcessRequestShouldProduce404ForUnknownCommand()
			{
				TestGetWebResponse("cargowise/processcontroller/a/b/command?action=bla&tasks=xxx");
			}

			void TestGetWebResponse(string urlPath)
			{
				// Arrange
				var responseStringBuilder = new ResponseStringBuilder("a", "b", new Mock<ITaskScheduler>().Object, new Mock<ITaskStatusProvider>().Object, CreateMockQueueStatusProviderFactory(), new Mock<IHostServiceStatusProvider>(MockBehavior.Strict).Object, Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>());

				var requestProcessor = new RequestProcessor(responseStringBuilder, new Mock<IHostLogger>().Object, new Mock<IActionQueue>().Object);
				var httpReqListener = new HttpRequestListenerForTest(requestProcessor);

				// Act
				var response = httpReqListener.GetWebResponse(urlPath);

				// Assert
				NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
			}
		}

		[ExpectNoExceptions]
		public void TestProcessRequestShouldProduce500InCaseOfUnhandledException()
		{
			ProcessRequestShouldProduce500InCaseOfException(new InvalidOperationException());
		}

		[ExpectNoExceptions]
		public void TestProcessRequestShouldProduce500InCaseOfUnhandledHttpListenerException()
		{
			ProcessRequestShouldProduce500InCaseOfException(new HttpListenerException(5));
		}

		void ProcessRequestShouldProduce500InCaseOfException(Exception ex)
		{
			// Arrange
			var statusProvider = new Mock<ITaskStatusProvider>();
			statusProvider.Setup(sp => sp.GetTasksStatus()).Throws(ex);
			var responseStringBuilder = new ResponseStringBuilder("a", "b", new Mock<ITaskScheduler>().Object, statusProvider.Object, CreateMockQueueStatusProviderFactory(), new Mock<IHostServiceStatusProvider>(MockBehavior.Strict).Object, Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>());

			var requestProcessor = new RequestProcessor(responseStringBuilder, new Mock<IHostLogger>().Object, new Mock<IActionQueue>().Object);
			var httpReqListener = new HttpRequestListenerForTest(requestProcessor);
			// Act
			var response = httpReqListener.GetWebResponse($"cargowise/processcontroller/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/status");

			// Assert
			NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
			for (var i = 0; i < 100 && httpReqListener.LastProcessRequestException == null; i++)
			{
				Thread.Sleep(10);
			}
			NUnit.Framework.Assert.That(httpReqListener.LastProcessRequestException, Is.Not.EqualTo(default(Exception)), "Exception should have been rethrown - should not be [null]");
			NUnit.Framework.Assert.That(httpReqListener.LastProcessRequestException, Is.EqualTo(ex), string.Format("Unexpected last exception: {0}", httpReqListener.LastProcessRequestException.ToString()));
			statusProvider.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestProcessRequestShouldNotReportNetWorkConnectionError()
		{
			// Arrange
			var logger = new Mock<IHostLogger>();
			var statusProvider = new Mock<ITaskStatusProvider>();
			statusProvider.Setup(sp => sp.GetTasksStatus()).Throws(new HttpListenerException(1229));
			var responseStringBuilder = new ResponseStringBuilder("a", "b", new Mock<ITaskScheduler>().Object, statusProvider.Object, CreateMockQueueStatusProviderFactory(), new Mock<IHostServiceStatusProvider>(MockBehavior.Strict).Object, Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>());

			var requestProcessor = new RequestProcessor(responseStringBuilder, logger.Object, new Mock<IActionQueue>().Object);
			var httpReqListener = new HttpRequestListenerForTest(requestProcessor);
			// Act
			var response = httpReqListener.GetWebResponse($"cargowise/processcontroller/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/status");

			// Assert
			NUnit.Framework.Assert.That(httpReqListener.LastProcessRequestException, Is.EqualTo(default(Exception)), "Should not report this error - should be [null]");
			logger.Verify(l => l.Log(LogLevel.Warning, It.Is<string>(message => message.Contains("RequestProcessor failed to send the reply to a client due to reason: An operation was attempted on a nonexistent network connection"))));
			statusProvider.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestProcessRequestShouldNotReportSqlException()
		{
			AssertProcessRequestShouldNotReportException(SqlExceptionBuilder.CreateSqlException(-2, "Execution timeout expired"), new Mock<IActionQueue>().Object);
		}

		[ExpectNoExceptions]
		public void TestProcessRequestShouldNotReportDatabaseUpgradingException()
		{
			var actionQueue = new ManualActionQueue();
			AssertProcessRequestShouldNotReportException(new DatabaseUpgradeInProgressException(), actionQueue);
			AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => actionQueue.InvokeActions());
		}

		void AssertProcessRequestShouldNotReportException(Exception ex, IActionQueue actionQueue)
		{
			// Arrange
			var statusProvider = new Mock<ITaskStatusProvider>();
			statusProvider.Setup(sp => sp.GetTasksStatus()).Throws(ex);
			var responseStringBuilder = new ResponseStringBuilder("a", "b", new Mock<ITaskScheduler>().Object, statusProvider.Object, CreateMockQueueStatusProviderFactory(), new Mock<IHostServiceStatusProvider>(MockBehavior.Strict).Object, Mock.Of<IJsonConverter>(), Mock.Of<IServiceHostRequestProvider>());

			var requestProcessor = new RequestProcessor(responseStringBuilder, new Mock<IHostLogger>().Object, actionQueue);
			var httpReqListener = new HttpRequestListenerForTest(requestProcessor);
			// Act
			var response = httpReqListener.GetWebResponse($"cargowise/processcontroller/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/status");

			// Assert
			NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.ServiceUnavailable));
			NUnit.Framework.Assert.That(httpReqListener.LastProcessRequestException, Is.EqualTo(default(Exception)), "Should not report this error - should be [null]");
			statusProvider.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestProcessRequestShouldProduceOkForNormalRequest()
		{
			// Arrange
			var responseStringBuilder = new ResponseStringBuilder("a", "b", new Mock<ITaskScheduler>().Object, new Mock<ITaskStatusProvider>().Object, CreateMockQueueStatusProviderFactory(), new Mock<IHostServiceStatusProvider>(MockBehavior.Strict).Object, new JsonNetConverter(), Mock.Of<IServiceHostRequestProvider>());

			var requestProcessor = new RequestProcessor(responseStringBuilder, new Mock<IHostLogger>().Object, new Mock<IActionQueue>().Object);
			var httpReqListener = new HttpRequestListenerForTest(requestProcessor);
			// Act
			var response = httpReqListener.GetWebResponse($"cargowise/processcontroller/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/status");

			// Assert
			NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
		}

		[ExpectNoExceptions]
		public void TestProcessRequestInNewThreadShouldNotReportError()
		{
			var originalLoggingValue = SystemDataRegistry.Instance.ProcessControllerVerboseLogging.GetValueWithoutFallback(Guid.Empty,
				Guid.Empty, Guid.Empty);
			var responseStringBuilder = new ResponseStringBuilder("a", "b", new Mock<ITaskScheduler>().Object, new Mock<ITaskStatusProvider>().Object, CreateMockQueueStatusProviderFactory(), new Mock<IHostServiceStatusProvider>(MockBehavior.Strict).Object, new JsonNetConverter(), Mock.Of<IServiceHostRequestProvider>());

			var requestProcessor = new RequestProcessor(responseStringBuilder, new Mock<IHostLogger>().Object, new Mock<IActionQueue>().Object);
			var httpReqListener = new HttpRequestListenerForTest(requestProcessor);

			try
			{
				SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetValue(Guid.Empty,
					Guid.Empty,
					Guid.Empty,
					new VerboseLoggingCollection
					{
						new VerboseLoggingBusinessObject
						{
							Code = "HOST",
							Value = ZDateTime.UtcNow.AddDays(1),
							SystemDefined = true,
						},
					});

				// Act
				var response = httpReqListener.GetWebResponse($"cargowise/processcontroller/{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}/status");

				// Assert
				NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
			}
			finally
			{
				SystemDataRegistry.Instance.ProcessControllerVerboseLogging.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalLoggingValue);
			}
		}

		[ExpectNoExceptions]
		public void TestHttpClientUsesContentTypeHeaderToInterpretHttpPostBody_WI00253839()
		{
			var content = "Joe Bloggs|Jõëí BĹōğĝŞ|啊芭擦|ابةتثجخد";
			var contentAsUtf8 = Encoding.UTF8.GetBytes(content);

			void AssertContentForEncoding(string message, string charset, string expected)
			{
				// Arrange
				var httpReqListener = new HttpEchoRequestListenerForTest(contentAsUtf8, charset);

				// Act
				var response = httpReqListener.GetWebResponse("echo");

				// Assert
				NUnit.Framework.Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
				var echoBody = Encoding.UTF8.GetString(httpReqListener.LastResponseBytes);
				NUnit.Framework.Assert.That(echoBody, Is.EqualTo(expected), message);
			}

			AssertContentForEncoding("UTF8 bytes interpreted as UTF8 should round trip without loss", "utf-8", content);
			AssertContentForEncoding("UTF8 bytes interpreted as ASCII should use fallback characters", "ascii", "Joe Bloggs|J?????? B??????????|?????????|????????????????");
			AssertContentForEncoding("UTF8 bytes interpreted as CP1252 / latin1 should be mangled", "CP1252", "Joe Bloggs|JÃµÃ«Ã­ BÄ¹ÅÄŸÄÅž|å•ŠèŠ­æ“¦|Ø§Ø¨Ø©ØªØ«Ø¬Ø®Ø¯");
		}

		static IQueueStatusProviderFactory CreateMockQueueStatusProviderFactory()
		{
			var queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>());

			return queueStatusProviderFactoryMock.Object;
		}
	}

	abstract class RoundTripHttpRequestListenerForTest
	{
		public Exception LastProcessRequestException { get; private set; }
		public byte[] LastResponseBytes { get; private set; }

		protected abstract void ProcessRequest(HttpListenerContext context, CancellationToken cancellationToken);

		protected virtual HttpWebRequest CreateRequest(string requestCommand)
		{
			var request = WebRequest.CreateHttp(requestCommand);
			request.Timeout = 30000;
			return request;
		}

		public HttpWebResponse GetWebResponse(string requestCommand)
		{
#if NET
			using
#endif
			var l = new TcpListener(IPAddress.Loopback, 0);
			l.Start();
			var port = ((IPEndPoint)l.LocalEndpoint).Port;
			l.Stop();

			// Create a listener.
			var prefix = "http://localhost:" + port + "/";
			using var listener = new HttpListener();
			listener.Prefixes.Add(prefix);
			listener.Start();
			try
			{
				listener.BeginGetContext((ar) =>
				{
					var context = listener.EndGetContext(ar);
					try
					{
						ProcessRequest(context, new CancellationToken());
					}
					catch (Exception ex)
					{
						LastProcessRequestException = ex;
					}
				}, null);

				using var client = new WebClient();
				try
				{
					var request = CreateRequest(prefix + requestCommand);
					var response = (HttpWebResponse)request.GetResponse();
					LastResponseBytes = response.GetResponseStream().ReadFully();
					return response;
				}
				catch (WebException e)
				{
					return (HttpWebResponse)e.Response;
				}
			}
			finally
			{
				listener.Stop();
			}
		}
	}

	class HttpRequestListenerForTest : RoundTripHttpRequestListenerForTest
	{
		readonly RequestProcessor requestProcessor;

		public HttpRequestListenerForTest(RequestProcessor requestProcessor)
		{
			this.requestProcessor = requestProcessor;
		}

		protected override void ProcessRequest(HttpListenerContext context, CancellationToken cancellationToken)
		{
			requestProcessor.ProcessRequest(context, cancellationToken);
		}
	}

	class HttpEchoRequestListenerForTest : RoundTripHttpRequestListenerForTest
	{
		readonly byte[] bodyContent;
		readonly string charset;

		public HttpEchoRequestListenerForTest(byte[] bodyContent, string charset)
		{
			this.bodyContent = bodyContent;
			this.charset = charset;
		}

		protected override HttpWebRequest CreateRequest(string requestCommand)
		{
			var request = base.CreateRequest(requestCommand);
			request.Method = "POST";
			request.ContentType = "text/plain; charset=" + charset;     // Request encoding varies for each test.
			request.ContentLength = bodyContent.Length;
			using (var stream = request.GetRequestStream())
			{
				stream.Write(bodyContent, 0, bodyContent.Length);
			}
			return request;
		}

		protected override void ProcessRequest(HttpListenerContext context, CancellationToken cancellationToken)
		{
			var request = context.Request;
			var responseString = new WebRequestInfo(request).Body;
			var buffer = Encoding.UTF8.GetBytes(responseString);        // Response encoding is always UTF8

			using (var output = context.Response.OutputStream)
			{
				output.Write(buffer, 0, buffer.Length);
			}
		}
	}
}
