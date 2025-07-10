using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace Enterprise.ServiceManager.Host.Testing
{
	class TaskRunnerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestRunTask_ShouldResetLastRunStatus()
		{
			// Arrange
			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(
					false,
					ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention,
					new TimeSpan(1),
					0m, 0m, 0m));

			var taskRunner = new TaskRunner(
				new Mock<IProcessRunnerPool>().Object,
				new Mock<IHostLogger>().Object);
			var serviceRunnerMock = new Mock<IServiceRunner>();
			serviceRunnerMock
				.Setup(r => r.Run(It.IsAny<ITaskRunRequest>()))
				.Returns(true);
			runners.Add(serviceRunnerMock.Object);
			var taskToRun = new Mock<IRunnableServiceTask>();
			taskToRun
				.SetupGet(t => t.Info)
				.Returns(new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(a => a.TypeName == "someType")));
			taskToRun
				.SetupGet(t => t.IsActive)
				.Returns(true);
			taskToRun
				.Setup(t => t.ValidateForRun())
				.Returns(TaskRunRequestResult.Success);
			taskToRun
				.Setup(t => t.TimeSinceLastStarted)
				.Returns(new Stopwatch());
			taskToRun
				.Setup(t => t.TimeRunning)
				.Returns(new Stopwatch());

			// Act
			var result = taskRunner.ProcessRunRequest(new DirectTaskRunRequest(taskToRun.Object), serviceRunnerMock.Object);

			// Assert
			NUnit.Framework.Assert.That(taskToRun.Object.Info.ErrorOnLastRun, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.Success));
		}

		[ExpectNoExceptions]
		public void TestRunTask_ShouldRecordExceptionAndNotRunIfTypeNameIsEmpty()
		{
			// Arrange
			var procRunPool = new Mock<IProcessRunnerPool>();
			var serviceRunner = new Mock<IServiceRunner>();
			procRunPool
				.Setup(prp => prp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(serviceRunner.Object);
			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
				0m, 0m, 0m));

			var logger = Mock.Of<IHostLogger>();
			var taskRunner = new TaskRunner(procRunPool.Object, logger);
			var taskToRun = new Mock<IRunnableServiceTask>();
			var getSchedule = TaskSchedulerTest.CreateTask("AAA", Factory, allowsMultiple: true);
			var serviceConfig = new Mock<IHostedServiceAttribute>();

			serviceConfig.Setup(sc => sc.TypeName).Returns((string)null);
			taskToRun.Setup(t => t.Info).Returns(new ServiceTaskInfo(serviceConfig.Object));
			taskToRun.Setup(t => t.RecordLastRunError());

			// Act
			var result = taskRunner.ProcessRunRequest(new ScheduledTaskRunRequest(taskToRun.Object), serviceRunner.Object);

			// Assert
			Mock
				.Get(logger)
				.Verify(x => x.Log(LogLevel.Error, It.IsAny<string>()), Times.Once);
			serviceRunner.Verify(sr => sr.Run(It.IsAny<TaskRunRequest>()), Times.Never);
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.ConfigurationError));
			taskToRun.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestRunTask_ShouldStartStartTimeTimer_ForFirstTaskInstance()
		{
			// Arrange
			var procRunPool = new Mock<IProcessRunnerPool>();
			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
				0m, 0m, 0m));
			var logger = new Mock<IHostLogger>();
			var serviceRunner = new ProcessServiceRunner(Mock.Of<ITaskScheduler>(), backgroundThreadActionQueue, processFactory, processRunnerRemotingServicesMock.Object, hostLoggerMock.Object, grpcClientSynchronizerFactoryMock.Object, serviceTaskLocksCleanerMock.Object, Mock.Of<IDateTimeProvider>(), errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), string.Empty, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")));
			runners.Add(serviceRunner);

			var taskRunner = new TaskRunner(procRunPool.Object, logger.Object);

			var runningStopwatch = new Stopwatch();
			var lastStartedStopwatch = new Stopwatch();

			var task = TaskSchedulerTest.CreateTask("AAA", Factory, allowsMultiple: true);

			var taskToRun = new Mock<IRunnableServiceTask>();
			taskToRun.SetupSequence(ttr => ttr.HasSchedule).Returns(true).CallBase();
			procRunPool.Setup(prp => prp.RunningCount(taskToRun.Object)).Returns(0);
			procRunPool
				.Setup(prp => prp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(new Mock<IServiceRunner>().Object);

			taskToRun.Setup(t => t.TimeSinceLastStarted).Returns(lastStartedStopwatch);
			taskToRun.Setup(t => t.TimeRunning).Returns(runningStopwatch);
			taskToRun.Setup(t => t.Info).Returns(task.Info);

			// Act
			taskRunner.ProcessRunRequest(new ScheduledTaskRunRequest(taskToRun.Object), serviceRunner);

			// Assert
			NUnit.Framework.Assert.That(lastStartedStopwatch.IsRunning, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(runningStopwatch.IsRunning, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestRunTask_ShouldNotRestartStartTimeRunning_ForSubsequentTaskInstances()
		{
			// Arrange
			var processRunnerRemotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			processRunnerRemotingServicesMock
				.Setup(p => p.CreateRunnerCommandQueueProxy(It.IsAny<int?>()))
				.Returns(Mock.Of<IRunnerCommandQueueProvider>());
			var procRunPool = new Mock<IProcessRunnerPool>();
			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
				0m, 0m, 0m));

			using (var actionQueue = new BackgroundThreadActionQueue(CancellationToken.None))
			{
				var taskRunner = new TaskRunner(procRunPool.Object, new Mock<IHostLogger>().Object);
				var serviceRunner = new ProcessServiceRunner(Mock.Of<ITaskScheduler>(), backgroundThreadActionQueue, processFactory, processRunnerRemotingServicesMock.Object, hostLoggerMock.Object, grpcClientSynchronizerFactoryMock.Object, serviceTaskLocksCleanerMock.Object, Mock.Of<IDateTimeProvider>(), errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(o => o.RunnerProcessPriorityValue == ProcessPriorityClass.BelowNormal), string.Empty, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")));
				runners.Add(serviceRunner);
				var runningStopwatch = new Stopwatch();
				var lastStartedStopwatch = new Stopwatch();
				var taskToRun = new Mock<IRunnableServiceTask>();
				taskToRun.Setup(task => task.HasSchedule).Returns(true);
				taskToRun.Setup(task => task.IsActive).Returns(true);
				taskToRun.Setup(task => task.ValidateForRun()).Returns(TaskRunRequestResult.Success);
				procRunPool.Setup(prp => prp.RunningCount(taskToRun.Object)).Returns(2);
				taskToRun.Setup(t => t.TimeRunning).Returns(runningStopwatch);
				taskToRun.Setup(t => t.TimeSinceLastStarted).Returns(lastStartedStopwatch);
				var serviceConfig = new Mock<IHostedServiceAttribute>();
				serviceConfig.Setup(sc => sc.TypeName).Returns("DummyType");
				taskToRun.Setup(t => t.Info).Returns(new ServiceTaskInfo(serviceConfig.Object));

				// Act
				var result = taskRunner.ProcessRunRequest(new DirectTaskRunRequest(taskToRun.Object), serviceRunner);

				// Assert
				NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.Success));
				NUnit.Framework.Assert.That(runningStopwatch.IsRunning, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(lastStartedStopwatch.IsRunning, NUnit.Framework.Is.EqualTo(true));
			}
		}

		public void TestRunTask_ShouldRun_IfScheduleInitialized()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var processRunnerRemotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			var jobObj = new Mock<IJobObject>();

			var procRunner = new Mock<IServiceRunner>();

			var procRunPool = new Mock<IProcessRunnerPool>();
			procRunPool
				.Setup(prp => prp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(procRunner.Object);
			var taskScheduler = new Mock<ITaskScheduler>();
			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
				0m, 0m, 0m));
			var logger = new Mock<IHostLogger>();

			var taskRunner = new TaskRunner(procRunPool.Object, logger.Object);

			var schedule = TaskSchedulerTest.CreateSchedule("C01", Factory);
			var taskToRun = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), hostRegistry: Mock.Of<IHostRegistrySettings>(o => o.ForcefullyDisabledTasks == string.Empty), transactionAdapter: transactionAdapter);

			// Act
			taskRunner.ProcessRunRequest(new ScheduledTaskRunRequest(taskToRun), procRunner.Object);

			// Assert
			procRunner.Verify(pr => pr.Run(It.IsAny<TaskRunRequest>()), Times.Once());
			Assert(true);
		}

		[ExpectNoExceptions]
		public void TestRunTask_ShouldBeDeactivated_IfScheduleIsInactive()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var jobObj = new Mock<IJobObject>();

			var procRunner = new Mock<IServiceRunner>();
			var taskScheduler = new Mock<ITaskScheduler>();
			var procRunPool = new Mock<IProcessRunnerPool>();
			procRunPool.Setup(prp => prp.RunningCount(null)).Returns(1);

			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
				0m, 0m, 0m));
			var logger = new Mock<IHostLogger>();

			var taskRunner = new TaskRunner(procRunPool.Object, logger.Object);

			var schedule = TaskSchedulerTest.CreateSchedule("C01", Factory, active: false);
			var taskToRun = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), transactionAdapter: transactionAdapter);

			schedule.ConfigString = "CONFIG_STRING";

			// Act
			var result = taskRunner.ProcessRunRequest(new ScheduledTaskRunRequest(taskToRun), procRunner.Object);

			// Assert
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.Inactive));
			procRunPool.Verify(pr => pr.StopAllRunners(taskToRun), Times.Once());
		}

		[ExpectNoExceptions]
		public void TestRunTask_TaskShouldNotRun_IfRunAttemptsHaveBeenCompleted()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var procRunner = new Mock<IServiceRunner>();
			var procRunPool = new Mock<IProcessRunnerPool>();
			procRunPool
				.Setup(prp => prp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(procRunner.Object);
			var resThrottle = new Mock<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
					0m, 0m, 0m));
			var logger = new Mock<IHostLogger>();

			var taskRunner = new TaskRunner(procRunPool.Object, logger.Object);

			var schedule = TaskSchedulerTest.CreateSchedule("C01", Factory);
			var taskToRun = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), hostRegistry: Mock.Of<IHostRegistrySettings>(o => o.ForcefullyDisabledTasks == string.Empty), transactionAdapter: transactionAdapter);

			// Act
			var result = taskRunner.ProcessRunRequest(Mock.Of<IDirectTaskRunRequest>(t => !t.HasRunsRemaining && t.Task == taskToRun), procRunner.Object);

			// Assert
			NUnit.Framework.Assert.That(result, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.TaskAlreadyCompleted));
			procRunner.Verify(pr => pr.Run(It.IsAny<TaskRunRequest>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestProcessRunRequest_WhenRunFails()
		{
			// Arrange
			using var transactionAdapter = new SchedulerServiceTaskTransactionAdapter();
			var repo = new MockRepository(MockBehavior.Default);
			var processRunnerRemotingServicesMock = repo.Create<IProcessRunnerRemotingServices>();
			var jobObj = repo.Create<IJobObject>();

			var procRunner = repo.Create<IServiceRunner>();
			var taskScheduler = repo.Create<ITaskScheduler>();
			var procRunPool = repo.Create<IProcessRunnerPool>();
			procRunPool
				.Setup(prp => prp.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(procRunner.Object);

			var resThrottle = repo.Create<IResourceThrottler>();
			resThrottle.Setup(rt => rt.WaitForResource())
				.Returns(new ResourceThrottlerResult(false, ResourceThrottlerResult.ResourceThrottlerResults.NoResourceContention, new TimeSpan(1),
				0m, 0m, 0m));
			var logger = repo.Create<IHostLogger>();

			var taskRunner = new TaskRunner(procRunPool.Object, logger.Object);

			var schedule = TaskSchedulerTest.CreateSchedule("C01", Factory);
			var taskToRun = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule), hostRegistry: Mock.Of<IHostRegistrySettings>(o => o.ForcefullyDisabledTasks == string.Empty), transactionAdapter: transactionAdapter);

			// Act
			procRunner.SetupSequence(pr => pr.Run(It.IsAny<TaskRunRequest>())).Returns(true).Returns(false);
			var result1 = taskRunner.ProcessRunRequest(new DirectTaskRunRequest(taskToRun), procRunner.Object);
			var result2 = taskRunner.ProcessRunRequest(new DirectTaskRunRequest(taskToRun), procRunner.Object);

			// Assert
			NUnit.Framework.Assert.That(result1, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.Success));
			NUnit.Framework.Assert.That(result2, NUnit.Framework.Is.EqualTo(TaskRunRequestResult.ProcessDidNotStart));
		}

		public class TestLogMessageTestCase : TestCase
		{
			[ExpectNoExceptions]
			public void TestValidateServiceTask()
			{
				// Arrange
				var serviceRunner = Mock.Of<IServiceRunner>();
				var request = Mock.Of<ITaskRunRequest>(
					x => x.Task == Mock.Of<IRunnableServiceTask>(
						y => y.Code == code
						&& y.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(
							z => z.TypeName == "TestTypeName"))
						&& y.IsActive
						&& y.TimeSinceLastStarted == new Stopwatch()
						&& y.TimeRunning == new Stopwatch()));

				// Act
				taskRunner.ProcessRunRequest(request, serviceRunner);

				// Assert
				var requestMock = Mock.Get(request);
				requestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.ValidatingRequest), Times.Once);
				requestMock.Verify(x => x.FormatRequestToLogMessage(LogMessageStage.ValidatedRequest), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestInactiveServiceTask()
			{
				// Arrange
				var serviceRunner = Mock.Of<IServiceRunner>();
				var task = Mock.Of<IRunnableServiceTask>(
					y => y.Code == code
						&& y.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>(
							z => z.TypeName == "TestTypeName"))
						&& !y.IsActive);
				var request = Mock.Of<ITaskRunRequest>(x => x.Task == task);
				Mock
					.Get(task)
					.Setup(x => x.ValidateForRun())
					.Returns(TaskRunRequestResult.Inactive);

				// Act
				taskRunner.ProcessRunRequest(request, serviceRunner);

				// Assert
				Mock.Get(request).Verify(x => x.FormatRequestToLogMessage(LogMessageStage.StoppingAccociatedRunners), Times.Once);
			}

			protected override void SetUp()
			{
				base.SetUp();
				logger = new Mock<IHostLogger>();
				runner = Mock.Of<IServiceRunner>(
					x => x.ProcessId == processId);
				factory = new Mock<IServiceRunnerFactory>();
				factory
					.Setup(x => x.Create(It.IsAny<ITaskScheduler>(), string.Empty))
					.Returns(runner);
				runnerPool = new Mock<IProcessRunnerPool>();
				runnerPool
					.Setup(x => x.GetOrCreateRunnerAsync(It.IsAny<ITaskRunRequest>(), It.IsAny<ITaskScheduler>(), It.IsAny<CancellationToken>()))
					.ReturnsAsync(runner);
				taskRunner = new TaskRunner(
					runnerPool.Object,
					logger.Object);
			}

			const int processId = 1234;
			const string code = "LWM";
			Mock<IHostLogger> logger;
			Mock<IProcessRunnerPool> runnerPool;
			TaskRunner taskRunner;
			IServiceRunner runner;
			Mock<IServiceRunnerFactory> factory;
		}

		protected override void SetUp()
		{
			base.SetUp();

			hostLoggerMock = new Mock<IHostLogger>();
			backgroundThreadActionQueue = new BackgroundThreadActionQueue(CancellationToken.None);
			processFactory = new ProcessWrapperFactory();
			processRunnerRemotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			runnerCommandQueueProviderMock = new Mock<IRunnerCommandQueueProvider>();
			grpcClientSynchronizerFactoryMock = new Mock<IGrpcClientSynchronizerFactory>();
			grpcClientSynchronizerMock = new Mock<IGrpcClientSynchronizer>();
			processServiceFactoryMock = new Mock<IServiceRunnerFactory>();
			serviceTaskLocksCleanerMock = new Mock<IServiceTaskLocksCleaner>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();

			grpcClientSynchronizerFactoryMock
				.Setup(f => f.Create(It.IsAny<GrpcEventHandleNames>()))
				.Returns(grpcClientSynchronizerMock.Object);
			grpcClientSynchronizerMock
				.Setup(s => s.WaitForServerReadySignal(It.IsAny<IRunnableServiceTask>(), It.IsAny<IGrpcPortResolver>(), It.IsAny<IProcess>(), It.IsAny<TimeSpan>()))
				.Returns(true);
			processRunnerRemotingServicesMock
				.Setup(s => s.CreateRunnerCommandQueueProxy(It.IsAny<int?>()))
				.Returns(runnerCommandQueueProviderMock.Object);

			runners = new List<IServiceRunner>();
			processServiceFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), string.Empty))
				.Returns((ITaskScheduler taskScheduler, string taskGroup) =>
				{
					var runner = new ProcessServiceRunner(taskScheduler, backgroundThreadActionQueue, processFactory, processRunnerRemotingServicesMock.Object, hostLoggerMock.Object, grpcClientSynchronizerFactoryMock.Object, Mock.Of<IServiceTaskLocksCleaner>(), Mock.Of<IDateTimeProvider>(), errorReporterProxyMock.Object, Mock.Of<IHostRegistrySettings>(), taskGroup, Mock.Of<IProductRegistration>(o => o.Key == Mock.Of<IProductRegistrationKey>(x => x.EnterpriseCode == "TST" && x.ServerCode == "TST")));
					runners.Add(runner);
					return runner;
				});
		}

		protected override void TearDown()
		{
			foreach (var runner in runners)
			{
				runner?.Dispose();
			}

			AsyncHelper.WaitAllActiveTasksForTest();
			base.TearDown();
		}

		ProcessWrapperFactory processFactory;
		List<IServiceRunner> runners;
		BackgroundThreadActionQueue backgroundThreadActionQueue;
		Mock<IRunnerCommandQueueProvider> runnerCommandQueueProviderMock;
		Mock<IGrpcClientSynchronizerFactory> grpcClientSynchronizerFactoryMock;
		Mock<IGrpcClientSynchronizer> grpcClientSynchronizerMock;
		Mock<IProcessRunnerRemotingServices> processRunnerRemotingServicesMock;
		Mock<IServiceRunnerFactory> processServiceFactoryMock;
		Mock<IServiceTaskLocksCleaner> serviceTaskLocksCleanerMock;
		Mock<IHostLogger> hostLoggerMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
	}
}
