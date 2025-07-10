using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ServiceManager.Host.Queue;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.ServiceHostClient.DataContracts;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class QueueMonitorTaskTest : TransactionedTestCase
	{
		public void TestWrongConstructorParams()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueMonitorTask(null, queueStatusProviderFactoryMock.Object, hostLoggerMock.Object, lockProviderMock.Object, Mock.Of<ISharedRegistrySettings>()));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("queueMonitorLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueMonitorTask(queueMonitorLoggerMock.Object, null, hostLoggerMock.Object, lockProviderMock.Object, Mock.Of<ISharedRegistrySettings>()));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("queueStatusProviderFactory"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueMonitorTask(queueMonitorLoggerMock.Object, queueStatusProviderFactoryMock.Object, null, lockProviderMock.Object, Mock.Of<ISharedRegistrySettings>()));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("hostLogger"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueMonitorTask(queueMonitorLoggerMock.Object, queueStatusProviderFactoryMock.Object, hostLoggerMock.Object, null, Mock.Of<ISharedRegistrySettings>()));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("lockProvider"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new QueueMonitorTask(queueMonitorLoggerMock.Object, queueStatusProviderFactoryMock.Object, hostLoggerMock.Object, lockProviderMock.Object, null));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("sharedRegistry"));
			});
		}

		[ExpectNoExceptions]
		public void TestConstructorWithConfiguredFrequencySetsRunDelay()
		{
			// Arrange
			var expectedResult = TimeSpan.FromMinutes(1);
			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringFrequency)
				.Returns(expectedResult);

			// Act
			using var task = CreateTask();

			// Assert
			NUnit.Framework.Assert.That(task.RunDelay, NUnit.Framework.Is.EqualTo(expectedResult));
		}

		[ExpectNoExceptions]
		public void TestConstructorSetsErrorDelay()
		{
			// Arrange
			using var task = CreateTask();

			// Assert
			NUnit.Framework.Assert.That(task.ErrorDelay, NUnit.Framework.Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		[ExpectNoExceptions]
		public void TestRunWithMonitoringDisabledDoesntLogQueueStatus()
		{
			// Arrange
			using var task = CreateTask();
			task.ConfigureQueueMonitor(Mock.Of<ITaskStatusProvider>());

			// Act
			task.Run(CancellationToken.None);

			// Assert
			queueMonitorLoggerMock.Verify(o => o.Log(It.IsAny<QueueDTO>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestRunWithSuccessfulLockAttemptLogsQueueStatus()
		{
			// Arrange
			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringEnabled)
				.Returns(true);
			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringFrequency)
				.Returns(TimeSpan.FromMinutes(1));

			var queueStatus = Enumerable.Range(0, 1)
				.Select(o => new QueueDTO($"queue{o}", $"task{o}", true, o, o, o, o))
				.ToArray();

			lockProviderMock.Setup(provider => provider.GetUnobservedLock(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<int?>()))
				.Returns(true);

			var queueStatusProviderMock = new Mock<IQueueStatusProvider>();
			queueStatusProviderMock
				.Setup(o => o.GetQueueStatus())
				.Returns(new QueueListDTO(queueStatus));

			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(queueStatusProviderMock.Object);

			using var task = CreateTask();
			task.ConfigureQueueMonitor(Mock.Of<ITaskStatusProvider>());

			// Act
			task.Run(CancellationToken.None);

			// Assert
			queueMonitorLoggerMock.Verify(o => o.Log(It.IsAny<QueueDTO>()), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestRunWithFailedLockAttemptDoesntLogsQueueStatus()
		{
			// Arrange
			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringEnabled)
				.Returns(true);
						sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringFrequency)
				.Returns(TimeSpan.FromMinutes(1));

			lockProviderMock.Setup(provider => provider.GetUnobservedLock(
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<TimeSpan>(),
						It.IsAny<string>(),
						It.IsAny<string>(),
						It.IsAny<int?>()))
					.Returns(false);

			var queueStatus = Enumerable.Range(0, 1)
				.Select(o => new QueueDTO($"queue{o}", $"task{o}", true, o, o, o, o))
				.ToArray();

			var queueStatusProviderMock = new Mock<IQueueStatusProvider>();
			queueStatusProviderMock
				.Setup(o => o.GetQueueStatus())
				.Returns(new QueueListDTO(queueStatus));

			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(queueStatusProviderMock.Object);

			using var task = CreateTask();
			task.ConfigureQueueMonitor(Mock.Of<ITaskStatusProvider>());

			// Act
			task.Run(CancellationToken.None);

			// Assert
			queueMonitorLoggerMock.Verify(o => o.Log(It.IsAny<QueueDTO>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestRunWithQueueStatusesLogsEachQueueStatus()
		{
			// Arrange
			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringEnabled)
				.Returns(true);

			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringFrequency)
				.Returns(TimeSpan.FromMinutes(1));

			lockProviderMock.Setup(provider => provider.GetUnobservedLock(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<int?>()))
				.Returns(true);

			var queueStatus = Enumerable.Range(0, 5)
				.Select(o => new QueueDTO($"queue{o}", $"task{o}", true, o, o, o, o))
				.ToArray();

			var queueStatusProviderMock = new Mock<IQueueStatusProvider>();
			queueStatusProviderMock
				.Setup(o => o.GetQueueStatus())
				.Returns(new QueueListDTO(queueStatus));

			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(queueStatusProviderMock.Object);

			using var task = CreateTask();
			task.ConfigureQueueMonitor(Mock.Of<ITaskStatusProvider>());

			// Act
			task.Run(CancellationToken.None);

			// Assert
			queueMonitorLoggerMock.Verify(o => o.Log(It.IsAny<QueueDTO>()), Times.Exactly(5));
		}

		[ExpectNoExceptions]
		public void TestRunWithNoQueueStatusesLogsToHost()
		{
			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringEnabled)
				.Returns(true);

			sharedRegistryMock
				.SetupGet(o => o.ProcessControllerQueueMonitoringFrequency)
				.Returns(TimeSpan.FromMinutes(1));

				// Arrange
			lockProviderMock.Setup(provider => provider.GetUnobservedLock(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<TimeSpan>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<int?>()))
				.Returns(true);

			queueStatusProviderFactoryMock
				.Setup(o => o.Create(It.IsAny<ITaskStatusProvider>()))
				.Returns(Mock.Of<IQueueStatusProvider>(provider => provider.GetQueueStatus() == new QueueListDTO(Enumerable.Empty<QueueDTO>())));

			using var task = CreateTask();
			task.ConfigureQueueMonitor(Mock.Of<ITaskStatusProvider>());

			// Act
			task.Run(CancellationToken.None);

			// Assert
			hostLoggerMock.Verify(o => o.Log(It.IsAny<LogLevel>(), It.IsAny<string>()), Times.Once);
		}

		public void TestRunWithNoInitializationThrowsException()
		{
			// Arrange
			using var task = CreateTask();

			// Act, Assert
			AssertExceptionThrown<QueueuMonitorInitializationException>(() => task.Run(CancellationToken.None));
		}

		[ExpectNoExceptions]
		public void TestInitialiseExitsOnCancellationToken()
		{
			// Arrange
			var maxTimeout = TimeSpan.FromSeconds(10);
			using var cancellationTokenSource = new CancellationTokenSource();
			using var task = CreateTask();

			// Act
			var initialiseTask = Task.Run(() => task.Initialise(cancellationTokenSource.Token));
			cancellationTokenSource.Cancel();

			// Assert
			NUnit.Framework.Assert.That(initialiseTask.Wait(maxTimeout), NUnit.Framework.Is.True);
		}

		public void TestConfigureQueueMonitorWithWrongArgumentsThrowsException()
		{
			// Arrange
			using var task = CreateTask();

			// Act, Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => task.ConfigureQueueMonitor(null));
			NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("taskStatusProvider"));
		}

		protected override void SetUp()
		{
			queueStatusProviderFactoryMock = new Mock<IQueueStatusProviderFactory>();
			queueMonitorLoggerMock = new Mock<IQueueMonitorLogger>();
			hostLoggerMock = new Mock<IHostLogger>();
			lockProviderMock = new Mock<ISqlMutexLockProvider>();
			sharedRegistryMock = new Mock<ISharedRegistrySettings>();
		}

		QueueMonitorTask CreateTask()
		{
			return new QueueMonitorTask(queueMonitorLoggerMock.Object, queueStatusProviderFactoryMock.Object, hostLoggerMock.Object, lockProviderMock.Object, sharedRegistryMock.Object);
		}

		Mock<IQueueStatusProviderFactory> queueStatusProviderFactoryMock;
		Mock<IQueueMonitorLogger> queueMonitorLoggerMock;
		Mock<IHostLogger> hostLoggerMock;
		Mock<ISqlMutexLockProvider> lockProviderMock;
		Mock<ISharedRegistrySettings> sharedRegistryMock;
	}
}

