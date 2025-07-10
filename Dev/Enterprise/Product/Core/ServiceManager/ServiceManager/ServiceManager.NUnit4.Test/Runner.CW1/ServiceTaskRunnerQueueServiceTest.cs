using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Async;
using Enterprise.ServiceManager.Host;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManagerProto;

namespace CargoWise.ServiceManager.Runner.Test
{
	public class ServiceTaskRunnerQueueServiceTest
	{
		[SetUp]
		public void SetUp()
		{
			cancellationTokenSource = new CancellationTokenSource();
			eventHandleNames = new GrpcEventHandleNames();
			hostCommunicationStrategyMock = new Mock<IHostCommunicationStrategy>();
			serviceTaskRunnerCancelerMock = new Mock<IServiceTaskRunnerCanceler>();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()));
			registrySettingsMock = new Mock<IRunnerRegistrySettings>();
			service = new ServiceTaskRunnerQueueService(hostCommunicationStrategyMock.Object, serviceTaskRunnerCancelerMock.Object, runnerLoggerMock.Object, new GrpcLoggerProxy(runnerLoggerMock.Object), registrySettingsMock.Object);
		}

		[TearDown]
		public void TearDown()
		{
			cancellationTokenSource.Cancel();
			AsyncHelper.WaitAllActiveTasksForTest();
		}

		[Test]
		public void TestStart()
		{
			// Arrange
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			// Act
			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));

			// Assert
			hostCommunicationStrategyMock.Verify(s => s.GrpcPortOpened(It.IsAny<int>()), Times.Once);
		}

		[Test]
		public void TestGetNextCommandScheduledRun()
		{
			// Arrange
			var assembly = "someAssembly";
			var code = "ASD";
			var guid = Guid.Parse("138813B4-C0AE-4E20-94AD-911B74265841");
			var dateTime = new DateTime(2020, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
			var config = "someConfig";
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), (grpcPort!.Value), Mock.Of<IErrorReporterProxy>());

			// Act
			queueProvider.Run(assembly, code, guid, dateTime, dateTime, config);

			// Assert
			ICommandInfo? commandInfo = null;
			var timer = new Stopwatch();
			timer.Start();
			while (commandInfo == null
					&& timer.Elapsed < TimeSpan.FromSeconds(10))
			{
				commandInfo = queue.GetNextCommand();
			}

			Assert.That(commandInfo, Is.Not.Null);
			Assert.That(commandInfo, Is.TypeOf<ScheduledRunCommandInfo>());
			Assert.That(((ScheduledRunCommandInfo)commandInfo!).Code, Is.EqualTo(code));
			Assert.That(((ScheduledRunCommandInfo)commandInfo).AssemblyName, Is.EqualTo(assembly));
			Assert.That(((ScheduledRunCommandInfo)commandInfo).Id, Is.EqualTo(guid));
			Assert.That(((ScheduledRunCommandInfo)commandInfo).NextRunTime, Is.EqualTo(dateTime));
			Assert.That(((ScheduledRunCommandInfo)commandInfo).ExpectedNextRunTime, Is.EqualTo(dateTime));
			Assert.That(((ScheduledRunCommandInfo)commandInfo).ConfigString, Is.EqualTo(config));
			queueProvider.Close();
		}

		[Test]
		public void TestGetNextCommandDirectRun()
		{
			// Arrange
			var assembly = "someAssembly2";
			var code = "ASD2";
			var guid = Guid.Parse("34DA921F-319C-4E67-8B8E-87C86D4BD129");
			var config = "someConfig2";
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort!.Value, Mock.Of<IErrorReporterProxy>());

			// Act
			queueProvider.Run(assembly, code, guid, config);

			// Assert
			ICommandInfo? commandInfo = null;
			var timer = new Stopwatch();
			timer.Start();
			while (commandInfo == null
					&& timer.Elapsed < TimeSpan.FromSeconds(10))
			{
				commandInfo = queue.GetNextCommand();
			}

			Assert.That(commandInfo, Is.Not.Null);
			Assert.That(commandInfo, Is.TypeOf<DirectRunCommandInfo>());
			Assert.That(((DirectRunCommandInfo)commandInfo!).Code, Is.EqualTo(code));
			Assert.That(((DirectRunCommandInfo)commandInfo).AssemblyName, Is.EqualTo(assembly));
			Assert.That(((DirectRunCommandInfo)commandInfo).Id, Is.EqualTo(guid));
			Assert.That(((DirectRunCommandInfo)commandInfo).ConfigString, Is.EqualTo(config));
			queueProvider.Close();
		}

		[Test]
		public void TestGetNextCommandStop()
		{
			// Arrange
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort!.Value, Mock.Of<IErrorReporterProxy>());

			// Act
			queueProvider.Stop();

			// Assert
			ICommandInfo? commandInfo = null;
			var timer = new Stopwatch();
			timer.Start();
			while (commandInfo == null
					&& timer.Elapsed < TimeSpan.FromSeconds(10))
			{
				commandInfo = queue.GetNextCommand();
			}

			Assert.That(commandInfo, Is.Not.Null);
			Assert.That(commandInfo, Is.TypeOf<StopCommandInfo>());
			queueProvider.Close();
		}

		[Test]
		public void TestStopCommandSendsNoQueuedResponse()
		{
			// Arrange
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort!.Value, Mock.Of<IErrorReporterProxy>());

			// Arrange
			ServiceTaskRunResponse? result = null;
			queueProvider.StartResponseTask((response) => result = response, cancellationTokenSource.Token);

			// Act
			queueProvider.Stop();
			Task.Delay(TimeSpan.FromSeconds(5)).Wait();

			// Assert
			Assert.That(result, Is.Null);
			queueProvider.Close();
			queueProvider.Dispose();
		}

		[Test]
		public void TestGetNextCommandMessageTimeoutReturnsNull()
		{
			// Arrange
			var timeout = TimeSpan.FromSeconds(1);
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort!.Value, Mock.Of<IErrorReporterProxy>());
			registrySettingsMock
				.Setup(r => r.ServiceTaskUnloadTimeout)
				.Returns(TimeSpan.FromSeconds(1));

			// Act
			var stopwatch = Stopwatch.StartNew();
			var commandInfo = queue.GetNextCommand();
			stopwatch.Stop();

			// Assert
			Assert.That(commandInfo, Is.Null);
			Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThan(timeout.TotalMilliseconds));
			queueProvider.Close();
		}

		[Test]
		public void TestGetNextCommandMessageTimeoutWaitsOnSecondMessage()
		{
			// Arrange
			var assembly = "someAssembly";
			var code = "ASD";
			var guid = Guid.Parse("138813B4-C0AE-4E20-94AD-911B74265841");
			var dateTime = new DateTime(2020, 1, 2, 3, 4, 5, 6, DateTimeKind.Utc);
			var config = "someConfig";
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), (grpcPort!.Value), Mock.Of<IErrorReporterProxy>());
			registrySettingsMock
				.Setup(r => r.ServiceTaskUnloadTimeout)
				.Returns(TimeSpan.FromSeconds(10));

			// Act
			queueProvider.Run(assembly, code, guid, dateTime, dateTime, config);
			var commandInfo1 = queue.GetNextCommand();
			Assert.That(commandInfo1, Is.Not.Null);
			ICommandInfo? commandInfo2 = null;
			var secondCommandTask = Task.Run(() =>
			{
				commandInfo2 = queue.GetNextCommand();
			});
			Task.Delay(1000).Wait();
			queueProvider.Run(assembly, code, guid, dateTime, dateTime, config);
			secondCommandTask.Wait();

			// Assert
			Assert.That(commandInfo2, Is.Not.Null);
			queueProvider.Close();
		}

		[Test]
		public void TestStreamClose()
		{
			// Arrange
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort!.Value, Mock.Of<IErrorReporterProxy>());
			ServiceTaskRunResponse? result = null;
			queueProvider.StartResponseTask((response) => result = response, cancellationTokenSource.Token);

			// Act
			var closeTask = Task.Run(() => queue.CloseStream(false, new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token));
			var stopwatch = Stopwatch.StartNew();
			while (result is null && stopwatch.ElapsedMilliseconds < 1000)
			{
			}
			queueProvider.Close();

			// Assert
			closeTask.Wait();
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Requesting grpc message stream closure"), Times.Once);
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Waiting for grpc message stream closure"), Times.Once);
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Grpc stream did not close prior to cancellation"), Times.Never);
			Assert.That(result, Is.Not.Null);
			Assert.That(result!.Status, Is.EqualTo(Status.RunnerExiting));
			Assert.That(result.HasFailureReason, Is.False);
		}

		[Test]
		public void TestRunnerExitingWithCancellationReQueuesRequest()
		{
			// Arrange
			int? grpcPort = null;
			using var synchronizer = new GrpcClientSynchronizer(eventHandleNames);
			hostCommunicationStrategyMock
				.Setup(s => s.GrpcPortOpened(It.IsAny<int>()))
				.Callback((int port) => grpcPort = port);

			var task = Task.Run(() => queue = service.Start(eventHandleNames.BaseEventWaitHandleName));
			synchronizer.WaitForServerReadySignal(Mock.Of<IRunnableServiceTask>(), Mock.Of<IGrpcPortResolver>(r => r.PortOpened == (grpcPort != null)), Mock.Of<IProcess>(p => !p.HasExited), TimeSpan.FromSeconds(10));
			task.Wait(TimeSpan.FromSeconds(5));
			if (grpcPort == null)
			{
				Assert.Fail("Server should have opened port");
			}
			var queueProvider = new RunnerCommandQueueProvider(Mock.Of<IHostLogger>(), grpcPort!.Value, Mock.Of<IErrorReporterProxy>());
			ServiceTaskRunResponse? result = null;
			queueProvider.StartResponseTask((response) =>
			{
				result = response;
			}, cancellationTokenSource.Token);

			// Act
			var closeTask = Task.Run(() => queue.CloseStream(true, new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token));
			var stopwatch = Stopwatch.StartNew();
			while (result is null && stopwatch.ElapsedMilliseconds < 1000)
			{
			}
			queueProvider.Close();

			// Assert
			closeTask.Wait();
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Requesting grpc message stream closure"), Times.Once);
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Waiting for grpc message stream closure"), Times.Once);
			runnerLoggerMock.Verify(l => l.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Grpc stream did not close prior to cancellation"), Times.Never);
			Assert.That(result, Is.Not.Null);
			Assert.That(result!.Status, Is.EqualTo(Status.RunnerExiting));
			Assert.That(result.HasFailureReason, Is.True);
			Assert.That(result.FailureReason, Is.EqualTo(FailureReasonType.RunnerWasCancelled));
		}

		CancellationTokenSource cancellationTokenSource = null!;
		Mock<IHostCommunicationStrategy> hostCommunicationStrategyMock = null!;
		Mock<IServiceTaskRunnerCanceler> serviceTaskRunnerCancelerMock = null!;
		Mock<IRunnerLogger> runnerLoggerMock = null!;
		Mock<IErrorReporterProxy> errorReporterProxyMock = null!;
		Mock<IRunnerRegistrySettings> registrySettingsMock = null!;
		GrpcEventHandleNames eventHandleNames = null!;
		ServiceTaskRunnerQueueService service = null!;
		IServiceTaskRunnerQueue queue = null!;
	}
}
