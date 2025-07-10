using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.Abstractions.ServiceHostRequestInterfaces;
using ServiceManager.Shared.CW;
using WTG.NUnit;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http
{
	class HttpRequestListenerTaskTest : TestCase
	{
		readonly Mock<IRequestProcessor> requestProcessor = new Mock<IRequestProcessor>();
		readonly Mock<IHttpListenerFactory> oneRequestListenerFactory = new Mock<IHttpListenerFactory>();
		readonly LimitedRequestHttpListener oneRequestListener = new LimitedRequestHttpListener(1);
		readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
		readonly Mock<IHostLogger> hostLoggerMock = new Mock<IHostLogger>();
		readonly Mock<IRequestProcessorFactory> requestProcessorFactory = new Mock<IRequestProcessorFactory>();
		readonly Mock<IQueueStatusProviderFactory> queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
		readonly Mock<ICancellationTokenProvider> cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
		readonly Mock<IErrorReporterProxy> errorReporterProxyMock = new Mock<IErrorReporterProxy>();
		readonly Mock<IHostServiceStatusProvider> hostServiceStatusProviderMock = new Mock<IHostServiceStatusProvider>(MockBehavior.Strict);
		readonly Mock<IServiceHostRequestProvider> serviceHostRequestProviderMock = new Mock<IServiceHostRequestProvider>();

		protected override void SetUp()
		{
			base.SetUp();
			oneRequestListenerFactory.Setup(lf => lf.Create(It.IsAny<string>())).Returns(oneRequestListener);
			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);

			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>());
		}

		protected override void TearDown()
		{
			oneRequestListener.Dispose();
			base.TearDown();
		}

		[UseSnapshotProtection]
		[TestRequiresAdministrativePrivileges("Http requires Admin to launch without prior configuration")]
		[ExpectNoExceptions]
		public void TestRunAsyncStartsHttpListener()
		{
			// Arrange
			using var requestQueue = new RequestQueue();
			hostServiceStatusProviderMock.Setup(m => m.IsReady()).Returns(true);

			using var requestQueueProcessor = new RequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, new RequestProcessorFactory(hostLoggerMock.Object, queueStatusProviderFactoryMock.Object, hostServiceStatusProviderMock.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object), new HttpListenerExceptionHandler(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 20));

			using (SystemDataRegistry.Instance.ServiceHostHttpRequestTimeoutInMilliseconds.SetTemporaryValue(
				Guid.Empty, Guid.Empty, Guid.Empty, (int)TimeSpan.FromSeconds(5).TotalMilliseconds))
			using (var listenerTask = new HttpListenerTask(hostLoggerMock.Object,
				new HttpListenerWrapperFactory(),
				requestQueue,
				new HttpListenerExceptionHandler(),
				cancellationTokenProviderMock.Object,
				errorReporterProxyMock.Object))
			{
				requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
				var processorThread = new Thread(() =>
				{
					requestQueueProcessor.Run(cancellationTokenSource.Token);
				});
				processorThread.Start();

				listenerTask.Initialise(cancellationTokenSource.Token);
				using var task = Task.Run(() => listenerTask.Run(cancellationTokenSource.Token));
				Thread.Sleep(TimeSpan.FromSeconds(5));

				var productRegistrationKey = ObjectFactory.Get<IProductRegistration>().Key;
				using (var serviceHostClientFactory = new EDIServiceHostClientFactory())
				{
					// Act
					var requestResult = serviceHostClientFactory
						.GetNewServiceHostClient("localhost", productRegistrationKey.EnterpriseCode, productRegistrationKey.ServerCode)
						.IsAlive();

					// Assert
					NUnit.Framework.Assert.That(requestResult, Is.EqualTo(true));
				}

				cancellationTokenSource.Cancel();
				NUnit.Framework.Assert.That(task.Wait(TimeSpan.FromSeconds(15)), Is.EqualTo(true));
			}
			hostServiceStatusProviderMock.Verify(m => m.IsReady(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestCancelsInitialisationWhenCancellationTokenIsCancelled()
		{
			// Arrange
			var httpListenerWrapperFactoryMock = new Mock<HttpListenerWrapperFactory>();
			using var cancellationTokenSource = new CancellationTokenSource();
			using var listenerTask = new HttpListenerTask(hostLoggerMock.Object,
				httpListenerWrapperFactoryMock.Object,
				Mock.Of<IRequestQueueProduceable>(),
				new HttpListenerExceptionHandler(),
				cancellationTokenProviderMock.Object,
				errorReporterProxyMock.Object);
			using var taskStartedAndReadyEvent = new ManualResetEvent(false);
			using var waitToInitialise = new ManualResetEvent(false);
			using var task = Task.Run(() =>
			{
				taskStartedAndReadyEvent.Set();
				waitToInitialise.WaitOne();
				listenerTask.Initialise(cancellationTokenSource.Token);
			});
			taskStartedAndReadyEvent.WaitOne();
			NUnit.Framework.Assert.That(task.Status, Is.EqualTo(TaskStatus.Running));

			// Act
			cancellationTokenSource.Cancel();
			waitToInitialise.Set();

			// Assert
			NUnit.Framework.Assert.That(task.Wait(TimeSpan.FromMinutes(1)), Is.EqualTo(true), "The listenerTask must have exited as we have invoked cancel on the cancellationTokenSource.");
			httpListenerWrapperFactoryMock.Verify((factory) => factory.Create(It.IsAny<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestInitialiseWillCompleteAfterConfigureHttpListenerIsComplete()
		{
			// Arrange
			var httpListenerWrapperFactoryMock = new Mock<HttpListenerWrapperFactory>();
			using var requestQueue = new RequestQueue();
			var httpListenerFactory = new HttpListenerWrapperFactory();
			using var queueProcessor = new RequestQueueProcessor(new Mock<IHostLogger>().Object, requestQueue, new RequestProcessorFactory(new Mock<IHostLogger>().Object, queueStatusProviderFactoryMock.Object, hostServiceStatusProviderMock.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object), new HttpListenerExceptionHandler(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(r => r.ServiceTaskHttpProcessorMaxThreads == 1));
			using var listenerTask = new HttpListenerTask(hostLoggerMock.Object, httpListenerWrapperFactoryMock.Object, requestQueue, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);
			using var initialiseTask = Task.Run(() => listenerTask.Initialise(CancellationToken.None));
			NUnit.Framework.Assert.That(initialiseTask.Wait(0), Is.EqualTo(false));

			// Act
			queueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

			// Assert
			NUnit.Framework.Assert.That(initialiseTask.Wait(TimeSpan.FromMinutes(1)), Is.EqualTo(true), "The listenerTask.InitialiseAsync must have completed once ConfigureHttpListener is completed.");
			httpListenerWrapperFactoryMock.Verify(factory => factory.Create("http://+:7070/cargowise/processController/EDIDAT/"), Times.Once);
		}

		public void TestDisposeTaskBeforeStartListen()
		{
			// Arrange
			using (var requestListener = new HttpListenerTask(hostLoggerMock.Object, Mock.Of<IHttpListenerFactory>(), Mock.Of<IRequestQueueProduceable>(), new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object))
			{
				// Act
				// Assert
				AssertNoExceptionThrown(() => requestListener.Dispose());
			}
		}

		public void TestInvalidHandleHttpExceptionDuringBeginGetContext()
		{
			// Arrange
			using var requestQueue = new RequestQueue();
			var httpListenerFactory = new HttpListenerWrapperFactory();
			using var queueProcessor = new RequestQueueProcessor(new Mock<IHostLogger>().Object, requestQueue, new RequestProcessorFactory(new Mock<IHostLogger>().Object, queueStatusProviderFactoryMock.Object, hostServiceStatusProviderMock.Object, new JsonNetConverter(), serviceHostRequestProviderMock.Object), new HttpListenerExceptionHandler(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 20));

			var exception = new HttpListenerException(6);
			var listenerWithInvalidHandle = new Mock<IHttpListener>();
			listenerWithInvalidHandle
				.Setup(l => l.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()))
				.Throws(exception);

			oneRequestListenerFactory
				.Setup(lf => lf.Create(It.IsAny<string>()))
				.Returns(listenerWithInvalidHandle.Object);

			using (var requestListener = new HttpListenerTask(hostLoggerMock.Object, oneRequestListenerFactory.Object, requestQueue, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object))
			{
				queueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
				requestListener.Initialise(cancellationTokenSource.Token);

				// Act
				var result = AssertExceptionThrown<InitialisationRequestException>(() =>
					requestListener.Run(cancellationTokenSource.Token)
				);

				// Assert
				NUnit.Framework.Assert.That(result.InnerException, Is.EqualTo(exception).Using(CustomComparers.TypeComparison));
			}
			oneRequestListenerFactory.Verify(factory => factory.Create("http://+:7070/cargowise/processController/EDIDAT/"), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestRunAsyncTaskCanBeCancelledWhenEndGetContextBlocks()
		{
			// Arrange
			var httpListenerFactoryMock = new Mock<IHttpListenerFactory>();
			var httpListenerMock = new Mock<IHttpListener>();
			var asyncResultMock = new Mock<IAsyncResult>();

			using var requestQueue = new RequestQueue();
			using var queueProcessor = new RequestQueueProcessor(new Mock<IHostLogger>().Object, requestQueue, new RequestProcessorFactory(new Mock<IHostLogger>().Object, queueStatusProviderFactoryMock.Object, hostServiceStatusProviderMock.Object, Mock.Of<IJsonConverter>(), serviceHostRequestProviderMock.Object), new HttpListenerExceptionHandler(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 20));

			using (new DisposableAction(() => AsyncHelper.WaitAllActiveTasksForTest()))
			using (var cancellationTokenSource = new CancellationTokenSource())
			using (var asyncResultManualResetEvent = new ManualResetEvent(false))
			using (var manualResetEventToSignalBeginGetContextComplete = new ManualResetEvent(false))
			{
				asyncResultMock.Setup(asyncResult => asyncResult.AsyncWaitHandle)
					.Returns(asyncResultManualResetEvent);

				httpListenerFactoryMock
					.Setup(factory => factory.Create(It.IsAny<string>())).Returns(httpListenerMock.Object);

				httpListenerMock
					.Setup(listener => listener.BeginGetContext(It.IsAny<AsyncCallback>(), It.IsAny<object>()))
					.Callback(() => manualResetEventToSignalBeginGetContextComplete.Set())
					.Returns(asyncResultMock.Object);

				using var requestListener = new HttpListenerTask(hostLoggerMock.Object, httpListenerFactoryMock.Object, requestQueue, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);
				queueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
				requestListener.Initialise(cancellationTokenSource.Token);

				using var task = Task.Run(() =>
				{
					requestListener.Run(cancellationTokenSource.Token);
				});

				NUnit.Framework.Assert.That(manualResetEventToSignalBeginGetContextComplete.WaitOne(TimeSpan.FromSeconds(15)), Is.EqualTo(true), "BeginGetContext should have completed.");

				// Act
				cancellationTokenSource.Cancel();

				// Assert
				NUnit.Framework.Assert.That(task.Wait(TimeSpan.FromSeconds(15)), Is.EqualTo(true), "Task created by RunAsync has been cancelled and hence it should be able to complete even if AsyncResult.WaitHandle still blocks.");

				asyncResultManualResetEvent.Set(); // unblock the handle to make sure internal task is not left uncomplete
			}
		}

		[TestRequiresAdministrativePrivileges("Http requires Admin to launch without prior configuration")]
		public void TestDisposeStopsListenerGracefully()
		{
			// Arrange
			const string dbServerName = "HttpRequestListenerTestServer";
			const string dbDatabaseName = "HttpRequestListenerTestDb";
			const int countClients = 10;

			AllowHttpConnection("http://+:7070/cargowise/processController/EDIDAT/");

			using var listenerReady = new ManualResetEvent(false);

			var clientsStarted = 0L;
			var clientsExited = 0L;

			var clients = new Thread[countClients];
			for (int i = 0; i < clients.Length; i++)
			{
				clients[i] = new Thread(() =>
				{
					var request = WebRequest.CreateHttp(ServiceManagerHelper.GetIsAliveUri(null));
					request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
					request.Timeout = 1000;
					request.ContentType = "application/json; charset=utf-8";
					request.Proxy = null;

					try
					{
						Interlocked.Increment(ref clientsStarted);
						listenerReady.WaitOne();
						request.GetResponse();
					}
					catch (WebException)
					{
					}

					Interlocked.Increment(ref clientsExited);
				});
			}

			clients.ForEach(x => x.Start());

			var listenerWrapperFactory = new Mock<IHttpListenerFactory>();
			listenerWrapperFactory
				.Setup(f => f.Create(It.IsAny<string>()))
				.Returns((string uriPrefix) => new HttpListenerWrapperWithDelay(uriPrefix));
			var responseStringBuilder = new ResponseStringBuilder(dbServerName, dbDatabaseName, Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), CreateMockQueueStatusProviderFactory(), hostServiceStatusProviderMock.Object, Mock.Of<IJsonConverter>(), serviceHostRequestProviderMock.Object);
			var localRequestProcessor = new RequestProcessor(responseStringBuilder, Mock.Of<IHostLogger>(), Mock.Of<IActionQueue>());

			var localRequestProcessorFactory = new Mock<IRequestProcessorFactory>();
			localRequestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(localRequestProcessor);

			using var requestQueue = new RequestQueue();

			using var requestQueueProcessor = new RequestQueueProcessor(new Mock<IHostLogger>().Object, requestQueue, localRequestProcessorFactory.Object, new HttpListenerExceptionHandler(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 20));
			using var cancellationTokenSource = new CancellationTokenSource();

			try
			{
				AssertNoExceptionThrown(() =>
				{
					requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

					var processorThread = new Thread(() =>
					{
						Enumerable
							.Range(1, clients.Length)
							.ForEach(i => requestQueueProcessor.Run(cancellationTokenSource.Token));
					});
					processorThread.Start();

					using var requestListener = new HttpListenerTask(hostLoggerMock.Object, listenerWrapperFactory.Object, requestQueue, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);
					requestListener.Initialise(cancellationTokenSource.Token);

					// Act
					var taskThread = new Thread(() =>
					{
						for (int i = 0; i < clients.Length; i++)
						{
							requestListener.Run(cancellationTokenSource.Token);
						}
					});
					taskThread.Start();

					while (Interlocked.Read(ref clientsStarted) < clients.Length)
					{
						Thread.Sleep(10);
					}

					listenerReady.Set();
					Thread.Sleep(10);

					Assert("Failed to join thread deadlock.", taskThread.Join(TimeSpan.FromSeconds(30)));
				});
			}
			finally
			{
				clients.ForEach(x => x.Join());
				cancellationTokenSource.Cancel();
			}

			// Assert
			NUnit.Framework.Assert.That(Interlocked.Read(ref clientsExited), Is.EqualTo(countClients).Using(CustomComparers.TypeComparison));

			void AllowHttpConnection(string url)
			{
				var process = Process.Start("cmd", Invariant($"/C netsh http add urlacl url={url} user=everyone"));
				process?.WaitForExit();
			}
		}

		[ExpectNoExceptions]
		public void TestMultipleSimultaneousRequestsAreQueued()
		{
			// Arrange
			NUnit.Framework.Assert.Multiple(() =>
			{
				Test(3);
				Test(100);
				Test(1000);
			});

			void Test(int numberOfRequests)
			{
				var requestQueueMock = new Mock<IRequestQueueProduceable>();
				requestQueueMock.Setup(q => q.Add(It.IsAny<IHttpListenerContext>(), It.IsAny<CancellationToken>()));

				using var httpListenerMock = new LimitedRequestHttpListener(numberOfRequests);

				var multipleRequestslistenerFactory = new Mock<IHttpListenerFactory>();
				multipleRequestslistenerFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(httpListenerMock);

				using var requestListener = new HttpListenerTask(hostLoggerMock.Object, multipleRequestslistenerFactory.Object, requestQueueMock.Object, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);

				requestListener.Initialise(cancellationTokenSource.Token);

				// Act
				for (int i = 0; i < numberOfRequests; i++)
				{
					requestListener.Run(cancellationTokenSource.Token);
				}

				Thread.Sleep(TimeSpan.FromMilliseconds(100));

				// Assert
				NUnit.Framework.Assert.That(requestQueueMock.Invocations.Count, Is.EqualTo(numberOfRequests), "All requests should be queued");
			}
		}

		[ExpectNoExceptions]
		public void TestQueuesMultipleRequestsAndDoesNotStartNewThread()
		{
			// Arrange
			var nRequests = 3;
			var workerThreadsBefore = 0;
			var workerThreadsAfter = 0;
			var completionPortThreadsBefore = 0;
			var completionPortThreadsAfter = 0;
			using var multipleRequestsListener = new LimitedRequestHttpListener(nRequests);
			var multipleRequestslistenerFactory = new Mock<IHttpListenerFactory>();
			multipleRequestslistenerFactory.Setup(f => f.Create(It.IsAny<string>())).Returns(multipleRequestsListener);

			var requestQueueMock = new Mock<IRequestQueueProduceable>();
			requestQueueMock.Setup(q => q.Add(It.IsAny<IHttpListenerContext>(), It.IsAny<CancellationToken>()));

			using (new DisposableAction(() => AsyncHelper.WaitAllActiveTasksForTest()))
			{
				using var requestListener = new HttpListenerTask(hostLoggerMock.Object, multipleRequestslistenerFactory.Object, requestQueueMock.Object, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);
				requestListener.Initialise(cancellationTokenSource.Token);

				ThreadPool.GetAvailableThreads(out workerThreadsBefore, out completionPortThreadsBefore);
				// Act
				for (int i = 0; i < nRequests; i++)
				{
					requestListener.Run(cancellationTokenSource.Token);
				}

				ThreadPool.GetAvailableThreads(out workerThreadsAfter, out completionPortThreadsAfter);

				Thread.Sleep(TimeSpan.FromMilliseconds(100));

				// Assert
				NUnit.Framework.Assert.That(requestQueueMock.Invocations.Count, Is.EqualTo(nRequests), "All requests should be queued");
				NUnit.Framework.Assert.That(workerThreadsAfter, Is.EqualTo(workerThreadsBefore), "Available worker threads shouldn't change");
				NUnit.Framework.Assert.That(completionPortThreadsAfter, Is.EqualTo(completionPortThreadsBefore), "Available completion port threads shouldn't change");
			}
		}

		[ExpectNoExceptions]
		public void TestStartListenLogsServerUrl()
		{
			// Arrange
			using (var requestListener = new HttpListenerTask(hostLoggerMock.Object, oneRequestListenerFactory.Object, Mock.Of<IRequestQueueProduceable>(), new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object))
			{
				// Act
				requestListener.Initialise(cancellationTokenSource.Token);
			}

			// Assert
			hostLoggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Once);
			hostLoggerMock.Verify(logger => logger.Log(
				LogLevel.Information,
				$"Http server is initialized at [http://+:7070/cargowise/processController/EDIDAT/]"),
				Times.Once);
		}

		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HttpListenerTask(null, Mock.Of<IHttpListenerFactory>(), Mock.Of<IRequestQueueProduceable>(), Mock.Of<IHttpListenerExceptionHandler>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HttpListenerTask(Mock.Of<IHostLogger>(), null, Mock.Of<IRequestQueueProduceable>(), Mock.Of<IHttpListenerExceptionHandler>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("httpListenerFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HttpListenerTask(Mock.Of<IHostLogger>(), Mock.Of<IHttpListenerFactory>(), null, Mock.Of<IHttpListenerExceptionHandler>(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("requestQueue"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HttpListenerTask(Mock.Of<IHostLogger>(), Mock.Of<IHttpListenerFactory>(), Mock.Of<IRequestQueueProduceable>(), null, cancellationTokenProviderMock.Object, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("httpListenerExceptionHandler"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HttpListenerTask(Mock.Of<IHostLogger>(), Mock.Of<IHttpListenerFactory>(), Mock.Of<IRequestQueueProduceable>(), Mock.Of<IHttpListenerExceptionHandler>(), null, errorReporterProxyMock.Object));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("cancellationTokenProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new HttpListenerTask(Mock.Of<IHostLogger>(), Mock.Of<IHttpListenerFactory>(), Mock.Of<IRequestQueueProduceable>(), Mock.Of<IHttpListenerExceptionHandler>(), cancellationTokenProviderMock.Object, null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("errorReporterProxy"));
			});
		}

		public void TestListenerThrowsCriticalExceptionForHttpErrors()
		{
			Test(new HttpListenerException(5));
			Test(new HttpListenerException(183));
			Test(new HttpListenerException(6));

			void Test(HttpListenerException exception)
			{
				// Arrange
				oneRequestListenerFactory
					.Setup(f => f.Create(It.IsAny<string>()))
					.Throws(exception);

				using (var requestListener = new HttpListenerTask(hostLoggerMock.Object, oneRequestListenerFactory.Object, Mock.Of<IRequestQueueProduceable>(), new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object))
				{
					// Act
					var result = AssertExceptionThrown<Exception>(() =>
						requestListener.Initialise(cancellationTokenSource.Token));

					// Assert
					NUnit.Framework.Assert.That(result.InnerException, Is.EqualTo(exception).Using(CustomComparers.TypeComparison));
					NUnit.Framework.Assert.That(result.IsCriticalException(), Is.EqualTo(true));
				}
			}
		}

		static IQueueStatusProviderFactory CreateMockQueueStatusProviderFactory()
		{
			var queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>());

			return queueStatusProviderFactoryMock.Object;
		}

		class HttpListenerWrapperWithDelay : HttpListenerWrapper
		{
			internal HttpListenerWrapperWithDelay(string uriPrefix) : base(uriPrefix) { }

			public override IAsyncResult BeginGetContext(AsyncCallback callback, object state)
			{
				var result = base.BeginGetContext(callback, state);
				Thread.Sleep(TimeSpan.FromMilliseconds(50));
				return result;
			}
		}

		class DummyAsyncResult : IAsyncResult, IDisposable
		{
			readonly ManualResetEvent completed = new ManualResetEvent(false);

			public bool IsCompleted => throw new NotImplementedException();
			public WaitHandle AsyncWaitHandle => completed;
			public object AsyncState => throw new NotImplementedException();
			public bool CompletedSynchronously => throw new NotImplementedException();

			public void Dispose() => completed.Dispose();
			public void Complete() => completed.Set();
		}

		internal class LimitedRequestHttpListener : IHttpListener
		{
			readonly List<DummyAsyncResult> contextResults = new List<DummyAsyncResult>();
			int requestsRemaining;

			public LimitedRequestHttpListener(int numberOfRequests)
			{
				requestsRemaining = numberOfRequests;
			}

			public IAsyncResult BeginGetContext(AsyncCallback callback, object state)
			{
				var contextResult = new DummyAsyncResult();
				contextResults.Add(contextResult);

				if (requestsRemaining > 0)
				{
					--requestsRemaining;
					contextResult.Complete();
					callback(contextResult);
				}
				return contextResult;
			}

			public void Dispose() => contextResults.ForEach((dr) => dr.Dispose());

			public HttpListenerContext EndGetContext(IAsyncResult result) => null;
		}
	}
}
