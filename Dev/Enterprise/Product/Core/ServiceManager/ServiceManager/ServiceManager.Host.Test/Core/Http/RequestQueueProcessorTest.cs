using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.ServiceManager.Host.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http
{
	class RequestQueueProcessorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestMultipleRequestsAreProcessedInParallel()
		{
			// Arrange
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			var requestProcessorMock = new Mock<IRequestProcessor>();

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessorMock.Object);

			using var requestQueue = GetDefaultRequestQueue();
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object);
			using var firstThreadWait = new ManualResetEvent(false);
			using var secondThreadWait = new ManualResetEvent(false);
			using var firstThreadEntered = new ManualResetEvent(false);
			using var secondThreadEntered = new ManualResetEvent(false);
			using var firstThreadExiting = new ManualResetEvent(false);
			using var secondThreadExiting = new ManualResetEvent(false);
			using var cancellationTokenSource = new CancellationTokenSource();

			var calls = 0;
			requestProcessorMock
				.Setup(r => r.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					if (calls == 0)
					{
						calls++;
						firstThreadEntered.Set();
						firstThreadWait.WaitOne(TimeSpan.FromSeconds(5));
						secondThreadWait.Set();
						firstThreadExiting.Set();
					}
					else
					{
						secondThreadEntered.Set();
						secondThreadWait.WaitOne(TimeSpan.FromSeconds(5));
						secondThreadExiting.Set();
					}
				});

			requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
			requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

			// Act
			var task = Task.Run(() =>
			{
				requestQueueProcessor.Run(cancellationTokenSource.Token);
				firstThreadEntered.WaitOne(TimeSpan.FromSeconds(5));
				requestQueueProcessor.Run(cancellationTokenSource.Token);
			});

			// Assert
			NUnit.Framework.Assert.That(firstThreadEntered.WaitOne(TimeSpan.FromSeconds(5)), Is.True);
			NUnit.Framework.Assert.That(secondThreadEntered.WaitOne(TimeSpan.FromSeconds(5)), Is.True);
			NUnit.Framework.Assert.That(firstThreadExiting.WaitOne(0), Is.False);
			NUnit.Framework.Assert.That(secondThreadExiting.WaitOne(0), Is.False);
			firstThreadWait.Set();
			cancellationTokenSource.Cancel();

			NUnit.Framework.Assert.That(firstThreadExiting.WaitOne(TimeSpan.FromSeconds(5)), Is.True);
			NUnit.Framework.Assert.That(secondThreadExiting.WaitOne(TimeSpan.FromSeconds(5)), Is.True);
			NUnit.Framework.Assert.That(requestQueue.Count, Is.EqualTo(0), "Queue should be empty");
		}

		[ExpectNoExceptions]
		public void TestParallelThreadsCreatedDoNotExceedThreadLimit()
		{
			// Arrange
			var threadTracker = new ConcurrentBag<int>();
			var httpListenerMock1 = new Mock<IHttpListener>();
			var httpListenerMock2 = new Mock<IHttpListener>();
			var httpListenerMock3 = new Mock<IHttpListener>();
			var requestProcessorMock = new Mock<IRequestProcessor>();
			var currentThread = 0;
			var manualResetEventsToSignalEndGetContextComplete = new List<ManualResetEvent>() { new ManualResetEvent(false), new ManualResetEvent(false), new ManualResetEvent(false) };

			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessorMock.Object);

				using var requestQueue = GetDefaultRequestQueue();
				using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object, Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 2));
				using var cancellationTokenSource = new CancellationTokenSource();
				using var eventToSimulateRequestProcessing = new ManualResetEvent(false);

			using (new DisposableAction(() => manualResetEventsToSignalEndGetContextComplete.ForEach(manualResetEvent => manualResetEvent.Dispose())))
			{
				requestProcessorMock
				.Setup(r => r.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					threadTracker.Add(Thread.CurrentThread.ManagedThreadId);
					manualResetEventsToSignalEndGetContextComplete[currentThread++].Set();
					eventToSimulateRequestProcessing.WaitOne();
				});

				requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
				requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
				requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
				requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

				// Act
				var task = Task.Run(() =>
				{
					Enumerable
						.Range(1, 3)
						.ForEach(i => requestQueueProcessor.Run(cancellationTokenSource.Token));
				});

				Thread.Sleep(TimeSpan.FromSeconds(5));

				// Assert
				NUnit.Framework.Assert.That(threadTracker.Count, Is.EqualTo(2), "Should be a maximum of 2 threads processing");
				NUnit.Framework.Assert.That(manualResetEventsToSignalEndGetContextComplete.Count(e => e.WaitOne(TimeSpan.FromSeconds(5))), Is.EqualTo(2), "Should be only 2 tasks signalled for processing");

				eventToSimulateRequestProcessing.Set();
				Thread.Sleep(TimeSpan.FromSeconds(2));

				cancellationTokenSource.Cancel();
				requestQueueProcessor.Dispose();

				task.Wait(TimeSpan.FromSeconds(5));
			}
		}

		[ExpectNoExceptions]
		public void TestQueueProcessorWaitsWhileQueueIsEmpty()
		{
			// Arrange
			using var eventToSimulateWaiting = new ManualResetEvent(false);

			var requestQueue = new Mock<IRequestQueueConsumable>();
			requestQueue
				.Setup(q => q.Take(It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					eventToSimulateWaiting.WaitOne();
				});

			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue.Object, new Mock<IRequestProcessorFactory>().Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
			var task = AsyncHelper.RunTask(() =>
			{
				try
				{
					requestQueueProcessor.Run(cancellationTokenSource.Token);
				}
				catch (OperationCanceledException)
				{ }
			}, "Run request queue processor in background");

			Thread.Sleep(TimeSpan.FromMilliseconds(250));

			// Assert
			NUnit.Framework.Assert.That(task.Status, Is.EqualTo(TaskStatus.Running));

			cancellationTokenSource.Cancel();
			eventToSimulateWaiting.Set();
			task.Wait();

			requestQueue.Verify(q => q.Take(cancellationTokenSource.Token), Times.Once);
			requestQueue.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestQueueProcessorWaitsForOutOfOrderConfiguration()
		{
			// Arrange
			var requestProcessor = new Mock<IRequestProcessor>();
			var requestQueue = new Mock<IRequestQueueConsumable>();
			requestQueue
				.Setup(q => q.Take(It.IsAny<CancellationToken>()))
				.Returns(Mock.Of<IHttpListenerContext>());
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue.Object, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			var task = AsyncHelper.RunTask(() => requestQueueProcessor.Run(cancellationTokenSource.Token), "process functionality in background");
			Task.Delay(100).Wait();
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
			task.Wait(TimeSpan.FromSeconds(10));

			// Assert
			cancellationTokenSource.Cancel();
			requestQueue.Verify(q => q.Take(cancellationTokenSource.Token), Times.Once);
			requestProcessor.Verify(p => p.ProcessRequest(It.IsAny<HttpListenerContext>(), cancellationTokenSource.Token), Times.Once);
			requestProcessorFactory.Verify(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()), Times.Once);
			requestProcessorFactory.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestQueueProcessorCancellationWhileWaitingForConfigurationExits()
		{
			// Arrange
			var loggerMock = new Mock<IHostLogger>();
			var requestProcessor = new Mock<IRequestProcessor>();
			var requestQueue = new Mock<IRequestQueueConsumable>();
			requestQueue
				.Setup(q => q.Take(It.IsAny<CancellationToken>()))
				.Returns(Mock.Of<IHttpListenerContext>());
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(loggerMock.Object, requestQueue.Object, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			var task = AsyncHelper.RunTask(() => requestQueueProcessor.Run(cancellationTokenSource.Token), "process functionality in background");
			Task.Delay(100).Wait();
			cancellationTokenSource.Cancel();
			task.Wait(TimeSpan.FromSeconds(10));

			// Assert
			requestQueue.VerifyNoOtherCalls();
			requestProcessorFactory.VerifyNoOtherCalls();
			requestProcessor.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestQueueProcessorCancellationWithConfigurationInitializedExits()
		{
			// Arrange
			var loggerMock = new Mock<IHostLogger>();
			var requestProcessor = new Mock<IRequestProcessor>();
			var requestQueue = new Mock<IRequestQueueConsumable>();
			requestQueue
				.Setup(q => q.Take(It.IsAny<CancellationToken>()))
				.Returns(Mock.Of<IHttpListenerContext>());
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(loggerMock.Object, requestQueue.Object, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
			cancellationTokenSource.Cancel();
			var task = AsyncHelper.RunTask(() => requestQueueProcessor.Run(cancellationTokenSource.Token), "process functionality in background");
			task.Wait(TimeSpan.FromSeconds(10));

			// Assert
			requestQueue.VerifyNoOtherCalls();
			requestProcessorFactory.Verify(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()), Times.Once);
			requestProcessorFactory.VerifyNoOtherCalls();
			requestProcessor.VerifyNoOtherCalls();
		}

		[ExpectNoExceptions]
		public void TestQueueProcessorResumesAfterRequestsAddedToEmptyQueue()
		{
			// Arrange
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			var requestProcessorMock = new Mock<IRequestProcessor>();

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessorMock.Object);

			using var requestQueue = GetDefaultRequestQueue();
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object);
			using var cancellationTokenSource = new CancellationTokenSource();
			using var asyncResultManualResetEvent = new ManualResetEvent(false);
			using var manualResetEventToSignalRequestProcessed = new ManualResetEvent(false);

			requestProcessorMock
				.Setup(r => r.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					manualResetEventToSignalRequestProcessed.Set();
				});

			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());
			var task = Task.Run(() => requestQueueProcessor.Run(cancellationTokenSource.Token));
			Thread.Sleep(TimeSpan.FromMilliseconds(250));

			NUnit.Framework.Assert.That(task.Status, Is.EqualTo(TaskStatus.Running));

			// Act
			requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);

			// Assert
			NUnit.Framework.Assert.That(manualResetEventToSignalRequestProcessed.WaitOne(TimeSpan.FromSeconds(15)), Is.True, "Request should be processed from queue.");

			cancellationTokenSource.Cancel();
			requestQueueProcessor.Dispose();

			task.Wait(TimeSpan.FromSeconds(5));
		}

		[ExpectNoExceptions]
		public void TestQueueProcessorResumesAfterThreadLimitWasReachedAndSomeThreadsFinish()
		{
			// Arrange
			var httpListenerMock1 = new Mock<IHttpListener>();
			var httpListenerMock2 = new Mock<IHttpListener>();
			var httpListenerMock3 = new Mock<IHttpListener>();
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			var requestProcessorMock = new Mock<IRequestProcessor>();
			var currentThread = 0;

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessorMock.Object);

			var manualResetEventsToSignalStartProcessRequest = new List<ManualResetEvent>() { new ManualResetEvent(false), new ManualResetEvent(false), new ManualResetEvent(false) };

				using var cancellationTokenSource = new CancellationTokenSource();
				using var requestQueue = GetDefaultRequestQueue();
				using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), new Mock<IErrorReporterProxy>().Object, Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 2));
				using (new DisposableAction(() => manualResetEventsToSignalStartProcessRequest.ForEach(manualResetEvent => manualResetEvent.Dispose())))
				using (var asyncResultManualResetEvent = new ManualResetEvent(false))
				using (var manualResetEventToSignalProcessingComplete = new ManualResetEvent(false))
				{
					requestProcessorMock
						.Setup(r => r.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							manualResetEventsToSignalStartProcessRequest[currentThread++].Set();
							manualResetEventToSignalProcessingComplete.WaitOne();
						});

				Enumerable
					.Range(1, 3)
					.ForEach(i => requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None));

				requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

				// Act
				var task = Task.Run(() =>
				{
					Enumerable
						.Range(1, 3)
						.ForEach(i => requestQueueProcessor.Run(cancellationTokenSource.Token));
				});

				Thread.Sleep(TimeSpan.FromMilliseconds(1000));

				NUnit.Framework.Assert.That(manualResetEventsToSignalStartProcessRequest.Count(e => e.WaitOne(TimeSpan.FromSeconds(5))), Is.EqualTo(2), "Only 2 tasks should be processing");

				manualResetEventToSignalProcessingComplete.Set();

				// Assert
				NUnit.Framework.Assert.That(manualResetEventsToSignalStartProcessRequest[2].WaitOne(TimeSpan.FromSeconds(15)), Is.EqualTo(true), "Tasks beyond max thread limit should eventually be processed");

				cancellationTokenSource.Cancel();
				requestQueueProcessor.Dispose();

				task.Wait(TimeSpan.FromSeconds(5));
			}
		}

		public void TestProcessRequestsObservesRequestExceptions()
		{
			// Arrange
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			var requestProcessor = new Mock<IRequestProcessor>();

			using var taskStartedEvent = new ManualResetEvent(false);

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);

			requestProcessor
				.Setup(rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
				.Callback(() => taskStartedEvent.Set())
				.Throws(new InvalidOperationException());

			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

			using var requestQueue = GetDefaultRequestQueue();
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), errorReporterProxyMock.Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

			var task = Task.Run(() => requestQueueProcessor.Run(cancellationTokenSource.Token));
			taskStartedEvent.WaitOne();
			Thread.Sleep(TimeSpan.FromMilliseconds(500));

			cancellationTokenSource.Cancel();
			requestQueueProcessor.Dispose();
			Thread.Sleep(TimeSpan.FromSeconds(2));

			// Assert
			AssertNoExceptionThrown(() =>
			{
				requestProcessor.Verify(
					rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()),
					Times.Once);

				errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<InvalidOperationException>()), Times.Once);
			});
		}

		public void TestProcessRequestThatAccessesDb()
		{
			// Arrange
			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			var requestProcessor = new Mock<IRequestProcessor>();

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);

			requestProcessor
				.Setup(rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
				.Callback(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						Db.Connection.EnsureIsOpen();
					}
				});

			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

			using var requestQueue = GetDefaultRequestQueue();
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), errorReporterProxyMock.Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

			var task = Task.Run(() => requestQueueProcessor.Run(cancellationTokenSource.Token));
			Thread.Sleep(TimeSpan.FromMilliseconds(500));

			// Assert
			AssertNoExceptionThrown(() =>
			{
				requestProcessor.Verify(rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()), Times.Once);
				errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			});

			cancellationTokenSource.Cancel();
		}

		[ExpectNoExceptions]
		public void TestHandleHttpExceptionDuringProcessRequest()
		{
			Test(new HttpListenerException(64));
			Test(new HttpListenerException(99));

			void Test(Exception exception)
			{
				// Arrange
				var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
				var requestProcessor = new Mock<IRequestProcessor>();

				var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
				errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

				using var requestQueue = GetDefaultRequestQueue();
				using var requestQueueProcessor = GetDefaultRequestQueueProcessor(Mock.Of<IHostLogger>(), requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), errorReporterProxyMock.Object);
				using var cancellationTokenSource = new CancellationTokenSource();
				using (new DisposableAction(() => AsyncHelper.WaitAllActiveTasksForTest()))
				using (var manualResetEvent = new ManualResetEvent(false))
				{
					requestProcessorFactory
					.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
					.Returns(requestProcessor.Object);

					requestProcessor
						.Setup(rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
						.Callback(() => manualResetEvent.Set())
						.Throws(exception);

					// Act
					requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
					requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

					var task = Task.Run(() => requestQueueProcessor.Run(cancellationTokenSource.Token));
					Thread.Sleep(TimeSpan.FromMilliseconds(500));

					// Assert
					NUnit.Framework.Assert.That(manualResetEvent.WaitOne(TimeSpan.FromSeconds(15)), Is.EqualTo(true), "requestProcessor.ProcessRequest method should have been called.");
					errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);

					cancellationTokenSource.Cancel();
					requestQueueProcessor.Dispose();

					task.Wait(TimeSpan.FromSeconds(5));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestHandleHttpExceptionDuringProcessRequests_NoNetwork()
		{
			// Arrange
			using var manualRequirementsChecker = new ManualResetEvent(false);
			var timeout = TimeSpan.FromSeconds(90);

			var requestProcessorFactory = new Mock<IRequestProcessorFactory>();
			var requestProcessor = new Mock<IRequestProcessor>();
			var hostLoggerMock = new Mock<IHostLogger>();

			requestProcessorFactory
				.Setup(f => f.CreateRequestProcessor(It.IsAny<IActionQueue>(), It.IsAny<ITaskScheduler>(), It.IsAny<ITaskStatusProvider>()))
				.Returns(requestProcessor.Object);

			requestProcessor
				.Setup(rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()))
				.Callback(() => manualRequirementsChecker.Set())
				.Throws(new HttpListenerException(1229));

			var errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));

			using var requestQueue = GetDefaultRequestQueue();
			using var requestQueueProcessor = GetDefaultRequestQueueProcessor(hostLoggerMock.Object, requestQueue, requestProcessorFactory.Object, new HttpListenerExceptionHandler(), errorReporterProxyMock.Object);
			using var cancellationTokenSource = new CancellationTokenSource();

			// Act
			requestQueue.Add(Mock.Of<IHttpListenerContext>(), CancellationToken.None);
			requestQueueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>());

			var task = Task.Run(() => requestQueueProcessor.Run(cancellationTokenSource.Token));
			Thread.Sleep(TimeSpan.FromMilliseconds(500));

			try
			{
				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					NUnit.Framework.Assert.That(manualRequirementsChecker.WaitOne(timeout), Is.EqualTo(true), "Signal to release service task lock was triggered");

					requestProcessor.Verify(
						rp => rp.ProcessRequest(It.IsAny<HttpListenerContext>(), It.IsAny<CancellationToken>()),
						Times.Once);

					errorReporterProxyMock.Verify(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);

					hostLoggerMock.Verify(
						cs => cs.Log(
							LogLevel.Warning,
							It.IsAny<string>()),
						Times.AtLeastOnce);
				});
			}
			finally
			{
				cancellationTokenSource.Cancel();
				requestQueueProcessor.Dispose();
				task.Wait(TimeSpan.FromSeconds(5));
			}
		}

		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				using var queueProcessor = new RequestQueueProcessor(Mock.Of<IHostLogger>(), Mock.Of<IRequestQueueConsumable>(), Mock.Of<IRequestProcessorFactory>(), Mock.Of<IHttpListenerExceptionHandler>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(r => r.ServiceTaskHttpProcessorMaxThreads == 1));

				var result = AssertExceptionThrown<ArgumentNullException>(() => queueProcessor.ConfigureHttpRequestProcessor(null, Mock.Of<ITaskStatusProvider>(), Mock.Of<IActionQueue>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("scheduler"));

				result = AssertExceptionThrown<ArgumentNullException>(() => queueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), null, Mock.Of<IActionQueue>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("statusProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => queueProcessor.ConfigureHttpRequestProcessor(Mock.Of<ITaskScheduler>(), Mock.Of<ITaskStatusProvider>(), null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("actionQueue"));
			});
		}

		RequestQueueProcessor GetDefaultRequestQueueProcessor(IHostLogger hostLogger, IRequestQueueConsumable testQueue, IRequestProcessorFactory testRequestProcessorFactory, IHttpListenerExceptionHandler httpListenerExceptionHandler, IErrorReporterProxy errorReporterProxy, IHostRegistrySettings hostRegistry = null)
		{
			return new RequestQueueProcessor(hostLogger, testQueue, testRequestProcessorFactory, httpListenerExceptionHandler, errorReporterProxy, hostRegistry ?? Mock.Of<IHostRegistrySettings>(o => o.ServiceTaskHttpProcessorMaxThreads == 20));
		}

		RequestQueue GetDefaultRequestQueue()
		{
			return new RequestQueue();
		}
	}
}
