using System;
using System.Diagnostics;
using System.Threading;
using Enterprise.ServiceManager.Host.Http;
using Enterprise.ServiceManager.Host.Queue;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing.Core.Http
{
	class HttpListenerTaskTest : TestCase
	{
		protected override void SetUp()
		{
			hostLoggerMock = new Mock<IHostLogger>();
			queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
			cancellationTokenProviderMock = new Mock<ICancellationTokenProvider>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			requestQueue = new RequestQueue();
			task = new HttpListenerTask(hostLoggerMock.Object, new HttpListenerWrapperFactory(), requestQueue, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);
		}

		protected override void TearDown()
		{
			requestQueue.Dispose();
		}

		[ExpectNoExceptions]
		public void TestDisposeMultipleTimesDoesNotThrow()
		{
			// Arrange
			task.Initialise(CancellationToken.None);

			// Act
			// Assert
			task.Dispose();
			task.Dispose();
		}

		[ExpectNoExceptions]
		public void TestDisposeWithoutConfiguredListenerDoesNotThrow()
		{
			// Arrange
			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>());

			// Act
			// Assert
			task.Dispose();
		}

		[ExpectNoExceptions]
		public void TestDisposeDoesNotWaitOnRequestQueue()
		{
			// Arrange
			var requestQueueMock = new Mock<IRequestQueue>();
			var waitingTask = new HttpListenerTask(hostLoggerMock.Object, new HttpListenerWrapperFactory(), requestQueueMock.Object, new HttpListenerExceptionHandler(), cancellationTokenProviderMock.Object, errorReporterProxyMock.Object);
			requestQueueMock
				.Setup(q => q.Count)
				.Returns(1);
			var stopwatch = Stopwatch.StartNew();

			// Act
			cancellationTokenProviderMock
				.Setup(p => p.Token)
				.Returns(new CancellationToken(true));
			waitingTask.Dispose();

			// Assert
			stopwatch.Stop();
			NUnit.Framework.Assert.That(stopwatch.ElapsedMilliseconds, NUnit.Framework.Is.LessThan(TimeSpan.FromSeconds(5).TotalMilliseconds).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(stopwatch.ElapsedMilliseconds, NUnit.Framework.Is.EqualTo(0).Within(1000));
		}

		HttpListenerTask task;
		RequestQueue requestQueue;
		Mock<IHostLogger> hostLoggerMock;
		Mock<IQueueStatusProviderFactory> queueStatusProviderFactoryMock;
		Mock<ICancellationTokenProvider> cancellationTokenProviderMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
	}
}
