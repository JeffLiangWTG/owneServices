using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Shared;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Testing
{
	class ProcessRunnerPoolTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<IHostLogger>();
			remotingServicesMock = new Mock<IProcessRunnerRemotingServices>();
			taskSchedulerMock = new Mock<ITaskScheduler>();
			serviceRunnerFactoryMock = new Mock<IServiceRunnerFactory>();
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(Mock.Of<IServiceRunner>(r => r.TaskGroup == string.Empty));
			jobObjectMock = new Mock<IJobObject>();
			grpcClientSynchronizerFactoryMock = new Mock<IGrpcClientSynchronizerFactory>();
			grpcClientSynchronizerMock = new Mock<IGrpcClientSynchronizer>();
			grpcClientSynchronizerFactoryMock
				.Setup(f => f.Create(It.IsAny<GrpcEventHandleNames>()))
				.Returns(grpcClientSynchronizerMock.Object);
			actionQueue = new BackgroundThreadActionQueue(CancellationToken.None);
			backgroundThreadActionQueueFactoryMock = new Mock<IBackgroundThreadActionQueueFactory>();
			backgroundThreadActionQueueFactoryMock
				.Setup(f => f.BackgroundThreadActionQueue)
				.Returns(actionQueue);
			hostRegistryMock = new Mock<IHostRegistrySettings>();
			hostRegistryMock.SetupGet(o => o.RunnerProcessPriorityValue).Returns(ProcessPriorityClass.BelowNormal);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns(5);
			hostRegistryMock.SetupGet(o => o.ServiceTaskUnloadTimeoutInSeconds).Returns(60);
			hostRegistryMock.SetupGet(o => o.SecondaryProcessSpinUpDelayInSeconds).Returns(5);
			hostRegistryMock.SetupGet(o => o.ServiceTaskRunnerSpecificGroup).Returns(new Dictionary<string, string>());
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			nudgingControllerMock = new Mock<INudgingController>();
		}

		protected override void TearDown()
		{
			actionQueue?.Dispose();
			disposeContainerAction?.Dispose();

			base.TearDown();
		}

		[ExpectNoExceptions]
		public void TestCheckIdle()
		{
			hostRegistryMock.SetupGet(o => o.ServiceTaskUnloadTimeoutInSeconds).Returns(3);

			var request = Mock.Of<ITaskRunRequest>(
				x => x.Task == Mock.Of<IRunnableServiceTask>(
					y => y.Info == new ServiceTaskInfo(Mock.Of<IHostedServiceAttribute>())));

			using (var pool = new ProcessRunnerPoolForTest(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, Mock.Of<IServiceRunnerFactory>(), errorReporterProxyMock.Object, hostRegistry: hostRegistryMock.Object))
			{
				using var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskSchedulerMock.Object, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object) { StubOutRunCore = true };
				pool.Add(runner);

				runner.Run(request);
				runner.ProcessExposed = new Mock<IProcess>().Object;
				runner.OverrideTaskRunning(true);
				Thread.Sleep(TimeSpan.FromSeconds(1.5));
				actionQueue.InvokeActions();
				NUnit.Framework.Assert.That(!runner.IsIdle, Is.True, "We are running, so should not be considered idle.");

				runner.OverrideTaskRunning(false);
				Thread.Sleep(TimeSpan.FromSeconds(0.5));
				actionQueue.InvokeActions();
				NUnit.Framework.Assert.That(runner.IsIdle, Is.True, "We are no longer running, should be idle.");

				Thread.Sleep(TimeSpan.FromSeconds(3));
				actionQueue.InvokeActions();
				NUnit.Framework.Assert.That(runner.StopCalledCount, Is.EqualTo(1));
				NUnit.Framework.Assert.That(!runner.IsIdle, Is.True, "idle timer has expired, we should be stopping, which is not idle.");

				Thread.Sleep(TimeSpan.FromSeconds(1));
				actionQueue.InvokeActions();
				NUnit.Framework.Assert.That(runner.StopCalledCount, Is.EqualTo(1), "Stop should only be called once as if we are stopping we are not idle");
			}
		}

		[ExpectNoExceptions]
		public void TestCheckRunnersRequiringBacklogHelp()
		{
			var nudgingController = new Mock<INudgingController>();
			hostRegistryMock.SetupGet(o => o.SecondaryProcessSpinUpDelayInSeconds).Returns(1);

			using (var pool = new ProcessRunnerPoolForTest(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, Mock.Of<IServiceRunnerFactory>(), errorReporterProxyMock.Object, nudgingController.Object, hostRegistryMock.Object))
			{
				using var runner1 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskSchedulerMock.Object, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object) { StubOutRunCore = true };
				pool.Add(runner1);
				var task1 = new Mock<IRunnableServiceTask>();
				pool.Associate(runner1, task1.Object);
				task1.Setup(t => t.Code).Returns("TA1");
				task1.Setup(t => t.MaxSecondaryRunningCount).Returns(0);
				runner1.Run(new ScheduledTaskRunRequest(task1.Object));
				runner1.ProcessExposed = new Mock<IProcess>().Object;
				runner1.OverrideTaskRunning(true);

				Thread.Sleep(TimeSpan.FromSeconds(1.5));
				using var runner2 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskSchedulerMock.Object, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object) { StubOutRunCore = true };
				pool.Add(runner2);
				var task2 = new Mock<IRunnableServiceTask>();
				pool.Associate(runner2, task2.Object);
				task2.Setup(t => t.Code).Returns("TA2");
				task2.Setup(t => t.MaxSecondaryRunningCount).Returns(1);
				var timeSinceTask2Started = Stopwatch.StartNew();
				task2.Setup(t => t.TimeSinceLastStarted).Returns(timeSinceTask2Started);
				runner2.Run(new ScheduledTaskRunRequest(task2.Object));
				runner2.ProcessExposed = new Mock<IProcess>().Object;
				runner2.OverrideTaskRunning(true);
				actionQueue.InvokeActions();
				nudgingController.Verify(nc => nc.ScheduleTasks(It.IsAny<IEnumerable<string>>(), It.IsAny<bool?>(), null), Times.Never);

				Thread.Sleep(TimeSpan.FromSeconds(1.5));
				actionQueue.InvokeActions();
				nudgingController.Verify(nc => nc.ReportNudgeStarted(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), It.IsNotNull<StackTrace>()), Times.Once());
				nudgingController.Verify(nc => nc.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), false, null), Times.Once());

				Thread.Sleep(TimeSpan.FromSeconds(0.75));
				actionQueue.InvokeActions();
				nudgingController.Verify(nc => nc.ReportNudgeStarted(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), It.IsNotNull<StackTrace>()), Times.Once());
				nudgingController.Verify(nc => nc.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), false, null), Times.Once());

				Thread.Sleep(TimeSpan.FromSeconds(0.75));
				actionQueue.InvokeActions();
				nudgingController.Verify(nc => nc.ReportNudgeStarted(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), It.IsNotNull<StackTrace>()), Times.AtLeast(2));
				nudgingController.Verify(nc => nc.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), false, null), Times.AtLeast(2));

				runner2.OverrideTaskRunning(false);
				Thread.Sleep(TimeSpan.FromSeconds(1.5));
				actionQueue.InvokeActions();
				nudgingController.Verify(nc => nc.ReportNudgeStarted(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), It.IsNotNull<StackTrace>()), Times.AtLeast(2));
				nudgingController.Verify(nc => nc.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA2"), false, null), Times.AtLeast(2));

				nudgingController.Verify(nc => nc.ReportNudgeStarted(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA1"), It.IsAny<StackTrace>()), Times.Never);
				nudgingController.Verify(nc => nc.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA1"), It.IsAny<bool?>(), It.IsAny<TimeSpan?>()), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestCheckRunnersRequiringBacklogHelp_SendsOneNudgePerUniqueTask()
		{
			var nudgingController = new Mock<INudgingController>();
			hostRegistryMock.SetupGet(o => o.SecondaryProcessSpinUpDelayInSeconds).Returns(1);
			using (var pool = new ProcessRunnerPoolForTest(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, Mock.Of<IServiceRunnerFactory>(), errorReporterProxyMock.Object, nudgingController.Object))
			{
				using var runner1 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskSchedulerMock.Object, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object) { StubOutRunCore = true };
				pool.Add(runner1);
				var task = new Mock<IRunnableServiceTask>();
				pool.Associate(runner1, task.Object);
				task.Setup(t => t.Code).Returns("TA1");
				task.Setup(t => t.MaxSecondaryRunningCount).Returns(1);
				var timeSinceTaskStarted = Stopwatch.StartNew();
				task.Setup(t => t.TimeSinceLastStarted).Returns(timeSinceTaskStarted);
				runner1.Run(new ScheduledTaskRunRequest(task.Object));
				runner1.ProcessExposed = new Mock<IProcess>().Object;
				runner1.OverrideTaskRunning(true);
				using var runner2 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskSchedulerMock.Object, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object) { StubOutRunCore = true };
				pool.Add(runner2);
				runner2.Run(new ScheduledTaskRunRequest(task.Object));
				runner2.ProcessExposed = new Mock<IProcess>().Object;
				runner2.OverrideTaskRunning(true);

				Thread.Sleep(TimeSpan.FromSeconds(1.5));
				actionQueue.InvokeActions();
				nudgingController.Verify(nc => nc.ScheduleTasks(It.Is<IEnumerable<string>>(t => t.Count() == 1 && t.First() == "TA1"), false, null), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestRemoveRunnerOnExit()
		{
			SystemDataRegistry.Instance.ServiceTaskUnloadTimeoutInSeconds.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);

			var runner = new Mock<IServiceRunner>();
			runner
				.SetupGet(r => r.TaskGroup)
				.Returns(string.Empty);
			runner.Setup(r => r.IsIdle).Returns(true);
			runner.SetupAdd(r => r.Exited += null);

			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				pool.Add(runner.Object);
				serviceRunnerFactoryMock
					.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
					.Returns(Mock.Of<IServiceRunner>(r => r.TaskGroup == string.Empty));

				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.EqualTo(runner.Object));
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Never);

				runner.Raise(r => r.Exited += null, EventArgs.Empty);

				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.Not.EqualTo(default(IServiceRunner)));
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Once);
			}
		}

		public void TestStopAllRunnersForTask_CallsStop_OnEachRunner()
		{
			// Arrange
			using (var processPool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var task = new Mock<IRunnableServiceTask>();
				var runner1 = new Mock<IServiceRunner>();
				var runner2 = new Mock<IServiceRunner>();
				runner1
					.SetupGet(r => r.TaskGroup)
					.Returns(string.Empty);
				runner2
					.SetupGet(r => r.TaskGroup)
					.Returns(string.Empty);
				processPool.Add(runner1.Object);
				processPool.Add(runner2.Object);
				processPool.Associate(runner1.Object, task.Object);
				processPool.Associate(runner2.Object, task.Object);

				runner1.SetupSequence(r1 => r1.TaskRunning).Returns(true).CallBase();
				runner2.SetupSequence(r2 => r2.TaskRunning).Returns(true).CallBase();

				// Act
				processPool.StopAllRunners(task.Object);

				// Assert
				runner1.Verify(r => r.Stop());
				runner2.Verify(r => r.Stop());
				Assert(true);
			}
		}

		[ExpectNoExceptions]
		public void TestStop_CallsStop_OnEachRunner()
		{
			// Arrange
			using (var processPool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, Mock.Of<IHostRegistrySettings>(), errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var task = new Mock<IRunnableServiceTask>();
				var runner1 = new Mock<IServiceRunner>();
				var runner2 = new Mock<IServiceRunner>();
				runner1
					.SetupGet(r => r.TaskGroup)
					.Returns(string.Empty);
				runner2
					.SetupGet(r => r.TaskGroup)
					.Returns(string.Empty);
				processPool.Add(runner1.Object);
				processPool.Add(runner2.Object);
				processPool.Associate(runner1.Object, task.Object);
				runner1.SetupSequence(r1 => r1.TaskRunning).Returns(true).CallBase();

				// Act
				processPool.Stop(TimeSpan.Zero);

				// Assert
				runner1.Verify(r => r.Stop());
				runner2.Verify(r => r.Stop());
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnersSnapshotIsThreadSafe()
		{
			hostRegistryMock.SetupGet(o => o.ServiceTaskUnloadTimeoutInSeconds).Returns(0);
			using (var processPool = new ProcessRunnerPoolForTest(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, Mock.Of<IServiceRunnerFactory>(), errorReporterProxy: errorReporterProxyMock.Object))
			{
				var runner = new Mock<IServiceRunner>();
				runner
					.SetupGet(r => r.TaskGroup)
					.Returns(string.Empty);
				var task = new Mock<IRunnableServiceTask>();
				var stopwatch = Stopwatch.StartNew();
				var timeout = TimeSpan.FromSeconds(1);

				var getRunnersTask = ExecuteActionContinuously(stopwatch, timeout, () =>
				{
					processPool.GetRunnersSnapshot();
				});
				var addRemoveTask = ExecuteActionContinuously(stopwatch, timeout, () =>
				{
					processPool.AddRunnerThreadSafeAsync(runner.Object, CancellationToken.None).Wait();
					processPool.CheckIdleRunnersExposed();
				});

				Task.WaitAll(getRunnersTask, addRemoveTask);
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnersSnapshotOnlyIncludesRunningTasks()
		{
			using (var processPool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var runner = new Mock<IServiceRunner>();
				runner
					.SetupGet(r => r.TaskGroup)
					.Returns(string.Empty);
				var task = new Mock<IRunnableServiceTask>();
				task.Setup(t => t.HasSchedule).Returns(true);
				task.Setup(t => t.Code).Returns("BLA");

				var snapshot = processPool.GetRunnersSnapshot();
				NUnit.Framework.Assert.That(snapshot.Count(), Is.EqualTo(0));

				processPool.Associate(runner.Object, task.Object);
				snapshot = processPool.GetRunnersSnapshot();
				NUnit.Framework.Assert.That(snapshot.Count(), Is.EqualTo(0));

				runner.Setup(r => r.TaskRunning).Returns(true);
				snapshot = processPool.GetRunnersSnapshot();
				NUnit.Framework.Assert.That(snapshot.Count(), Is.EqualTo(1));
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnerCompletesImmediatelyWhenIdleRunnerExists()
		{
			// Arrange
			const int waitTimeoutInSeconds = 60;
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns(waitTimeoutInSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				using var busyRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				busyRunner.OverrideTaskRunning(true);

				using var idleRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);

				pool.Add(busyRunner);
				pool.Add(idleRunner);

				var releaseBusyRunnerTask = Task
					.Delay(TimeSpan.FromSeconds(10))
					.ContinueWith(t => busyRunner.OverrideTaskRunning(false));
				using (new DisposableAction(releaseBusyRunnerTask.Wait))
				{
					var stopwatch = Stopwatch.StartNew();

					// Act
					var result = Task.Run(async () => await pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None)).Result;

					// Assert
					NUnit.Framework.Assert.Multiple(() =>
					{
						NUnit.Framework.Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(waitTimeoutInSeconds)), "Should return before timeout expires");
						NUnit.Framework.Assert.That(busyRunner.TaskRunning, Is.EqualTo(true), $"{nameof(busyRunner)} should still be running");
						NUnit.Framework.Assert.That(result, Is.EqualTo(idleRunner).Using(CustomComparers.TypeComparison), $"{nameof(idleRunner)} should be returned");
					});
				}
			}
		}

		public void TestGetRunnerCompletesWhenBusyRunnerBecomesIdleAndLogContainsTime()
		{
			// Arrange
			const int waitTimeoutInSeconds = 60;
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns(waitTimeoutInSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				using var busyRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				busyRunner.OverrideTaskRunning(true);

				using var toBeIdleRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				toBeIdleRunner.OverrideTaskRunning(true);

				var task = Task.Run(() =>
				{
					Task.Delay(TimeSpan.FromSeconds(5)).Wait();
					toBeIdleRunner.OverrideTaskRunning(false);
				});

				pool.Add(busyRunner);
				pool.Add(toBeIdleRunner);

				var releaseBusyRunnerTask = Task
					.Delay(TimeSpan.FromSeconds(10))
					.ContinueWith(t => busyRunner.OverrideTaskRunning(false));
				using (new DisposableAction(releaseBusyRunnerTask.Wait))
				{
					var stopwatch = Stopwatch.StartNew();

					// Act
					var result = Task.Run(async () => await pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None)).Result;

					// Assert
					NUnit.Framework.Assert.Multiple(() =>
					{
						NUnit.Framework.Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(waitTimeoutInSeconds)), "Should return before timeout expires");
						NUnit.Framework.Assert.That(busyRunner.TaskRunning, Is.EqualTo(true), $"{nameof(busyRunner)} should still be running");
						NUnit.Framework.Assert.That(result, Is.EqualTo(toBeIdleRunner).Using(CustomComparers.TypeComparison), $"{nameof(toBeIdleRunner)} should be returned");
						AssertNoExceptionThrown(() => loggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.IsRegex(@"Found idle runner \[PID=None, State=Idle\]. \[Wait time: \d{2}:\d{2}:\d{2}.\d{3}\]")), Times.Once));
					});
				}
			}
		}

		public void TestGetRunnerCompletesImmediatelyWhenNoRunnersExist()
		{
			// Arrange
			const int waitTimeoutInSeconds = 60;
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns(waitTimeoutInSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var stopwatch = Stopwatch.StartNew();

				// Act
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Never);
				var result = Task.Run(async () => await pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None)).Result;
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Once);

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
					{
						NUnit.Framework.Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(waitTimeoutInSeconds)), "Should return before timeout expires");
						NUnit.Framework.Assert.That(result, Is.Not.EqualTo(default(IServiceRunner)));
						AssertNoExceptionThrown(() => loggerMock.Verify(logger => logger.Log(LogLevel.Debug, "No processes exist"), Times.Once));
					});
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnerCompletesImmediatelyWhenCancellationRequested()
		{
			// Arrange
			const int waitTimeoutInSeconds = 60;
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns(waitTimeoutInSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				using var busyRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				busyRunner.OverrideTaskRunning(true);

				using var toBeIdleRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				toBeIdleRunner.OverrideTaskRunning(true);

				actionQueue.Enqueue(TimeSpan.FromSeconds(5), () => toBeIdleRunner.OverrideTaskRunning(false));

				pool.Add(busyRunner);
				pool.Add(toBeIdleRunner);

				var releaseBusyRunnerTask = Task
					.Delay(TimeSpan.FromSeconds(10))
					.ContinueWith(t => busyRunner.OverrideTaskRunning(false));
				using (new DisposableAction(releaseBusyRunnerTask.Wait))
				{
					var stopwatch = Stopwatch.StartNew();

					// Act
					using var cancellationTokenSource = new CancellationTokenSource();
					cancellationTokenSource.Cancel();
					IServiceRunner result = null;
					try
					{
						result = pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, cancellationTokenSource.Token).Result;
					}
					catch (Exception e)
					{
						NUnit.Framework.Assert.That(e.InnerException, Is.TypeOf<TaskCanceledException>());
					}

					// Assert
					NUnit.Framework.Assert.Multiple(() =>
					{
						NUnit.Framework.Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(waitTimeoutInSeconds)), "Should return before timeout expires");
						NUnit.Framework.Assert.That(busyRunner.TaskRunning, Is.EqualTo(true), $"{nameof(busyRunner)} should still be running");
						NUnit.Framework.Assert.That(result, Is.EqualTo(default(IServiceRunner)));
					});
				}
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnerCompletesImmediatelyWhenBusyRunnerBecomesIdle()
		{
			// Arrange
			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				using var busyRunner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				busyRunner.OverrideTaskRunning(true);
				pool.Add(busyRunner);

				var task = Task.Run(() =>
				{
					Task.Delay(TimeSpan.FromSeconds(5)).Wait();
					busyRunner.OverrideTaskRunning(false);
				});

				var stopwatch = Stopwatch.StartNew();

				// Act
				var result = Task.Run(async () => await pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None)).Result;

				// Assert
				NUnit.Framework.Assert.Multiple(() =>
				{
					NUnit.Framework.Assert.That(stopwatch.Elapsed, Is.LessThan(waitTimeout), "Should return before timeout expires");
					NUnit.Framework.Assert.That(result, Is.EqualTo(busyRunner).Using(CustomComparers.TypeComparison));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerNoRunner()
		{
			// Arrange
			var runnableServiceTask = Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO");
			var taskRunRequestMock = new Mock<ITaskRunRequest>();
			taskRunRequestMock
				.SetupGet(t => t.Task)
				.Returns(runnableServiceTask);
			var serviceRunnerMock = new Mock<IServiceRunner>();
			serviceRunnerMock
				.SetupGet(r => r.TaskGroup)
				.Returns(string.Empty);
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(serviceRunnerMock.Object);
			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				// Act
				var serviceRunner = Task.Run(async () => await pool.GetOrCreateRunnerAsync(taskRunRequestMock.Object, taskSchedulerMock.Object, CancellationToken.None)).Result;

				// Assert
				NUnit.Framework.Assert.That(serviceRunner, Is.EqualTo(serviceRunnerMock.Object));
			}

			serviceRunnerFactoryMock.Verify(f => f.Create(taskSchedulerMock.Object, It.IsAny<string>()), Times.Once);
			serviceRunnerMock.VerifySet(s => s.IsAllocatingTask = true, Times.Once);
			taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.ObtainingRunner), Times.Once);
			taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.RunnerIsCreated, serviceRunnerMock.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerCalledTwiceIdleRunner() => AssertGetOrCreateRunnerCalledTwice(true, 1);
		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerCalledTwiceBusyRunner() => AssertGetOrCreateRunnerCalledTwice(false, 2);

		void AssertGetOrCreateRunnerCalledTwice(bool idle, int expectedCreateCalls)
		{
			// Arrange
			var runnableServiceTask = Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO");
			var taskRunRequestMock = new Mock<ITaskRunRequest>();
			taskRunRequestMock
				.SetupGet(t => t.Task)
				.Returns(runnableServiceTask);
			var serviceRunnerMock = new Mock<IServiceRunner>();
			serviceRunnerMock
				.SetupGet(r => r.IsIdle)
				.Returns(idle);
			serviceRunnerMock
				.SetupGet(r => r.TaskGroup)
				.Returns(string.Empty);
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(serviceRunnerMock.Object);
			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				// Act
				var serviceRunner1 = Task.Run(async () => await pool.GetOrCreateRunnerAsync(taskRunRequestMock.Object, taskSchedulerMock.Object, CancellationToken.None)).Result;
				var serviceRunner2 = Task.Run(async () => await pool.GetOrCreateRunnerAsync(taskRunRequestMock.Object, taskSchedulerMock.Object, CancellationToken.None)).Result;

				// Assert
				NUnit.Framework.Assert.That(serviceRunner1, Is.EqualTo(serviceRunnerMock.Object));
				NUnit.Framework.Assert.That(serviceRunner2, Is.EqualTo(serviceRunner1));
			}

			serviceRunnerFactoryMock.Verify(f => f.Create(taskSchedulerMock.Object, It.IsAny<string>()), Times.Exactly(expectedCreateCalls));
			serviceRunnerMock.VerifySet(s => s.IsAllocatingTask = true, Times.Exactly(2));
			taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.ObtainingRunner), Times.Exactly(2));
		}

		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerExistingIdleRunner() => AssertGetOrCreateRunnerExistingRunner(true, 0);
		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerExistingBusyRunner() => AssertGetOrCreateRunnerExistingRunner(false, 1);

		void AssertGetOrCreateRunnerExistingRunner(bool idle, int expectedCreateCalls)
		{
			// Arrange
			var runnableServiceTask = Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO");
			var taskRunRequestMock = new Mock<ITaskRunRequest>();
			taskRunRequestMock
				.SetupGet(t => t.Task)
				.Returns(runnableServiceTask);
			var serviceRunnerMock = new Mock<IServiceRunner>();
			serviceRunnerMock
				.SetupGet(r => r.IsIdle)
				.Returns(idle);
			serviceRunnerMock
				.SetupGet(r => r.TaskGroup)
				.Returns(string.Empty);
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(serviceRunnerMock.Object);
			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				pool.Add(serviceRunnerMock.Object);
				pool.Associate(serviceRunnerMock.Object, runnableServiceTask);

				// Act
				var serviceRunner = Task.Run(async () => await pool.GetOrCreateRunnerAsync(taskRunRequestMock.Object, taskSchedulerMock.Object, CancellationToken.None)).Result;

				// Assert
				NUnit.Framework.Assert.That(serviceRunner, Is.EqualTo(serviceRunnerMock.Object));
			}

			serviceRunnerFactoryMock.Verify(f => f.Create(taskSchedulerMock.Object, It.IsAny<string>()), Times.Exactly(expectedCreateCalls));
			serviceRunnerMock.VerifySet(s => s.IsAllocatingTask = true, Times.Once);
			taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.ObtainingRunner), Times.Once);
			if (idle)
			{
				taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.RunnerIsFound, serviceRunnerMock.Object), Times.Once);
			}
			else
			{
				taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.RunnerIsCreated, serviceRunnerMock.Object), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerIgnoresRunnersBeingAllocated()
		{
			// Arrange
			var runnableServiceTask = Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO");
			var taskRunRequestMock = new Mock<ITaskRunRequest>();
			taskRunRequestMock
				.SetupGet(t => t.Task)
				.Returns(runnableServiceTask);
			var serviceRunnerMock = new Mock<IServiceRunner>();
			serviceRunnerMock
				.SetupGet(r => r.IsIdle)
				.Returns(true);
			serviceRunnerMock
				.SetupGet(r => r.TaskGroup)
				.Returns(string.Empty);
			serviceRunnerMock
				.SetupGet(r => r.IsAllocatingTask)
				.Returns(true);
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(serviceRunnerMock.Object);
			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				pool.Add(serviceRunnerMock.Object);
				pool.Associate(serviceRunnerMock.Object, runnableServiceTask);

				// Act
				var serviceRunner = Task.Run(async () => await pool.GetOrCreateRunnerAsync(taskRunRequestMock.Object, taskSchedulerMock.Object, CancellationToken.None)).Result;

				// Assert
				NUnit.Framework.Assert.That(serviceRunner, Is.EqualTo(serviceRunnerMock.Object));
			}

			serviceRunnerFactoryMock.Verify(f => f.Create(taskSchedulerMock.Object, It.IsAny<string>()), Times.Once);
			serviceRunnerMock.VerifySet(s => s.IsAllocatingTask = true, Times.Once);
			taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.ObtainingRunner), Times.Once);
			taskRunRequestMock.Verify(r => r.FormatRequestToLogMessage(LogMessageStage.RunnerIsCreated, serviceRunnerMock.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestGetOrCreateRunnerParallel()
		{
			// Arrange
			var numberOfRunners = 100;
			var requests = Enumerable.Range(0, numberOfRunners)
				.Select(i =>
				{
					var runnableServiceTask = Mock.Of<IRunnableServiceTask>(r => r.Code == $"{i}");
					var taskRunRequestMock = new Mock<ITaskRunRequest>();
					taskRunRequestMock
						.SetupGet(t => t.Task)
						.Returns(runnableServiceTask);
					var serviceRunnerMock = new Mock<IServiceRunner>();
					serviceRunnerMock
						.SetupGet(r => r.IsIdle)
						.Returns(false);
					serviceRunnerMock
						.SetupGet(r => r.TaskGroup)
						.Returns(string.Empty);
					serviceRunnerFactoryMock
						.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
						.Returns(serviceRunnerMock.Object);
					return taskRunRequestMock;
				});

			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				// Act
				var tasks = requests
					.Select(async r => await pool.GetOrCreateRunnerAsync(r.Object, taskSchedulerMock.Object, CancellationToken.None))
					.ToArray();
				Task.WhenAll(tasks);
				var results = tasks.Select(t => t.Result).ToArray();

				// Assert
				NUnit.Framework.Assert.That(results.Distinct().Count(t => t != null), Is.EqualTo(numberOfRunners));
			}
		}

		[ExpectNoExceptions]
		public void TestDissociateTasksFromRunnerAfterFinished()
		{
			// Arrange
			var runnableServiceTask = Mock.Of<IRunnableServiceTask>(r => r.Code == "PRC" && r.HasSchedule);
			var taskRunRequestMock = new Mock<ITaskRunRequest>();
			taskRunRequestMock
				.SetupGet(t => t.Task)
				.Returns(runnableServiceTask);
			var serviceRunnerMock = new Mock<IServiceRunner>();
			serviceRunnerMock
				.SetupGet(r => r.IsIdle)
				.Returns(true);
			serviceRunnerMock
				.SetupGet(r => r.TaskGroup)
				.Returns(string.Empty);
			serviceRunnerMock
				.SetupGet(r => r.TaskRunning)
				.Returns(false);
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(serviceRunnerMock.Object);

			var waitTimeout = TimeSpan.FromSeconds(60);
			hostRegistryMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns((int)waitTimeout.TotalSeconds);
			using (var pool = new ProcessRunnerPool(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var task = Task.Run(async () => await pool.GetOrCreateRunnerAsync(taskRunRequestMock.Object, taskSchedulerMock.Object, CancellationToken.None));
				task.Wait();
				serviceRunnerMock
					.SetupGet(r => r.TaskRunning)
					.Returns(true);

				var serviceRunner = task.Result;

				var associationBeforeTaskCompletion = pool.GetRunnersSnapshot().ToList();
				NUnit.Framework.Assert.That(associationBeforeTaskCompletion.Count, Is.EqualTo(1));
				NUnit.Framework.Assert.That(associationBeforeTaskCompletion[0].Item2, Is.EqualTo(serviceRunner.ProcessId));
				NUnit.Framework.Assert.That(associationBeforeTaskCompletion[0].Item1, Is.EqualTo(runnableServiceTask.Code));

				// Act
				serviceRunnerMock
					.SetupGet(r => r.TaskRunning)
					.Returns(false);
				serviceRunnerMock.Raise(runner => runner.TaskRunRequestCompleted += null, EventArgs.Empty);

				// Assert both state where TaskRunning is true and false. So that we could conclude that it has been removed from the Association.
				serviceRunnerMock
					.SetupGet(r => r.TaskRunning)
					.Returns(true);
				var associationAfterTaskCompletion1 = pool.GetRunnersSnapshot().ToList();
				NUnit.Framework.Assert.That(associationAfterTaskCompletion1.Count, Is.EqualTo(0));

				serviceRunnerMock
					.SetupGet(r => r.TaskRunning)
					.Returns(false);
				var associationAfterTaskCompletion2 = pool.GetRunnersSnapshot().ToList();
				NUnit.Framework.Assert.That(associationAfterTaskCompletion2.Count, Is.EqualTo(0));
			}
		}

		[ExpectNoExceptions]
		public void TestStopWhenExitEventFiresReentrantly()
		{
			using (var pool = new ProcessRunnerPool(new Mock<IHostLogger>().Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var mockRepository = new MockRepository(MockBehavior.Default);
				var process1 = mockRepository.Create<IProcess>();
				process1.SetupAdd(r => r.Exited += null);
				process1.Setup(p => p.Id).Callback(() => process1.Raise(r => r.Exited += null, EventArgs.Empty)).Returns(5);
				var process2 = mockRepository.Create<IProcess>();
				var process3 = mockRepository.Create<IProcess>();
				using var runner1 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process1.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				runner1.OverrideTaskRunning(true);
				using var runner2 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process2.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				runner2.OverrideTaskRunning(true);
				using var runner3 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process3.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				runner3.OverrideTaskRunning(true);

				pool.Add(runner1);
				pool.Add(runner2);
				pool.Add(runner3);
				pool.Stop(TimeSpan.FromSeconds(1));
			}
		}

		[ExpectNoExceptions]
		public void TestStopDoesNotKillRunnersThatExit()
		{
			using (var pool = new ProcessRunnerPool(new Mock<IHostLogger>().Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var process1 = new Mock<IProcess>();
				process1.SetupAdd(r => r.Exited += null);
				process1.Setup(p => p.Id).Callback(() => process1.Raise(r => r.Exited += null, EventArgs.Empty)).Returns(5);
				using var runner1 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process1.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				runner1.OverrideTaskRunning(true);

				pool.Add(runner1);
				pool.Stop(TimeSpan.FromSeconds(5));
				process1.Verify(p => p.Kill(), Times.Never);
			}
		}

		[ExpectNoExceptions]
		public void TestStopDoesKillRunnersThatDoNotExit()
		{
			using (var pool = new ProcessRunnerPool(new Mock<IHostLogger>().Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var process1 = new Mock<IProcess>();
				using var runner1 = new ProcessServiceRunnerForTesting(Guid.NewGuid(), null, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process1.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				runner1.OverrideTaskRunning(true);

				pool.Add(runner1);
				pool.Stop(TimeSpan.FromSeconds(1));
				process1.Verify(p => p.Kill());
			}
		}

		Task ExecuteActionContinuously(Stopwatch stopWatch, TimeSpan timeout, Action action)
		{
			return Task.Run(() =>
			{
				while (stopWatch.Elapsed < timeout)
				{
					action();
				}
			});
		}

		[ExpectNoExceptions]
		public void TestGetRunnerDoesNotReturnRunnerThatIsExiting()
		{
			using (var pool = new ProcessRunnerPool(new Mock<IHostLogger>().Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var process = new Mock<IProcess>();
				process.Setup(p => p.HasExited).Returns(true);
				using var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskScheduler: null, actionQueue: actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);

				runner.OverrideTaskRunning(false);
				pool.Add(runner);

				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Never);
				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.Not.EqualTo(default(IServiceRunner)));
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnerDoesNotReturnRunnerThatIsStopping()
		{
			using (var pool = new ProcessRunnerPool(new Mock<IHostLogger>().Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				var process = new Mock<IProcess>();
				using var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskScheduler: null, actionQueue: actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), process.Object, grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);

				runner.OverrideTaskRunning(false);
				runner.Stop();
				pool.Add(runner);

				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Never);
				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.Not.EqualTo(default(IServiceRunner)));
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Once);
			}
		}

		[ExpectNoExceptions]
		public void TestGetRunnerReturnsRunnerWithNoProcess()
		{
			using (var pool = new ProcessRunnerPool(new Mock<IHostLogger>().Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, jobObjectMock.Object, hostRegistryMock.Object, errorReporterProxyMock.Object, nudgingControllerMock.Object))
			{
				using var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskScheduler: null, actionQueue: actionQueue, remotingServicesMock.Object, null, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				pool.Add(runner);
				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.EqualTo(runner).Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestCheckIdleRemovesExpiredRunnerWithNoProcess()
		{
			hostRegistryMock.SetupGet(o => o.ServiceTaskUnloadTimeoutInSeconds).Returns(1);
			var serviceRunnerMock = Mock.Of<IServiceRunner>(r => r.IsIdle && r.TaskGroup == string.Empty);
			serviceRunnerFactoryMock
				.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
				.Returns(serviceRunnerMock);
			using (var pool = new ProcessRunnerPoolForTest(loggerMock.Object, backgroundThreadActionQueueFactoryMock.Object, serviceRunnerFactoryMock.Object, hostRegistry: hostRegistryMock.Object, errorReporterProxy: errorReporterProxyMock.Object))
			{
				using var runner = new ProcessServiceRunnerForTesting(Guid.NewGuid(), taskSchedulerMock.Object, actionQueue, remotingServicesMock.Object, loggerMock.Object, new ServiceManagerDateTimeProvider(), grpcClientSynchronizerFactory: grpcClientSynchronizerFactoryMock.Object, hostRegistrySettings: hostRegistryMock.Object);
				pool.Add(runner);

				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.EqualTo(runner).Using(CustomComparers.TypeComparison));

				Thread.Sleep(TimeSpan.FromSeconds(1.5));
				actionQueue.InvokeActions();
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Never);
				NUnit.Framework.Assert.That(pool.GetOrCreateRunnerAsync(Mock.Of<ITaskRunRequest>(t => t.Task == Mock.Of<IRunnableServiceTask>(r => r.Code == "FOO")), taskSchedulerMock.Object, CancellationToken.None).Result, Is.Not.EqualTo(default(IServiceRunner)));
				serviceRunnerFactoryMock.Verify(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()), Times.Once);
				NUnit.Framework.Assert.That(runner.StopCalledCount, Is.EqualTo(0), "Runner should be removed rather than attempting to send a runner that is not running a stop command");
			}
		}

		Mock<IHostLogger> loggerMock;
		Mock<IProcessRunnerRemotingServices> remotingServicesMock;
		Mock<ITaskScheduler> taskSchedulerMock;
		Mock<IGrpcClientSynchronizerFactory> grpcClientSynchronizerFactoryMock;
		Mock<IGrpcClientSynchronizer> grpcClientSynchronizerMock;
		Mock<IServiceRunnerFactory> serviceRunnerFactoryMock;
		Mock<IJobObject> jobObjectMock;
		Mock<IHostRegistrySettings> hostRegistryMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<INudgingController> nudgingControllerMock;
		BackgroundThreadActionQueue actionQueue;
		Mock<IBackgroundThreadActionQueueFactory> backgroundThreadActionQueueFactoryMock;
		readonly IDisposable disposeContainerAction;

		class ProcessRunnerPoolTaskGroupSpecificPoolTest : TestCase
		{
			protected override void SetUp()
			{
				base.SetUp();
				hostLoggerMock = new Mock<IHostLogger>();
				backgroundThreadActionQueueMock = new Mock<IBackgroundThreadActionQueue>();
				backgroundThreadActionQueueFactoryMock = new Mock<IBackgroundThreadActionQueueFactory>();
				backgroundThreadActionQueueFactoryMock.Setup(f => f.BackgroundThreadActionQueue).Returns(backgroundThreadActionQueueMock.Object);
				serviceRunnerFactoryMock = new Mock<IServiceRunnerFactory>();
				jobObjectMock = new Mock<IJobObject>();
				hostRegistrySettingsMock = new Mock<IHostRegistrySettings>();
				nudgingControllerMock = new Mock<INudgingController>();
				errorReporterProxyMock = new Mock<IErrorReporterProxy>();

				hostRegistrySettingsMock.SetupGet(o => o.RunnerProcessPriorityValue).Returns(ProcessPriorityClass.BelowNormal);
				hostRegistrySettingsMock.SetupGet(o => o.BusyRunnerWaitTimeInSeconds).Returns(5);
				hostRegistrySettingsMock.SetupGet(o => o.ServiceTaskUnloadTimeoutInSeconds).Returns(60);
				hostRegistrySettingsMock.SetupGet(o => o.SecondaryProcessSpinUpDelayInSeconds).Returns(5);

				processRunnerPool = new ProcessRunnerPool(
					hostLoggerMock.Object,
					backgroundThreadActionQueueFactoryMock.Object,
					serviceRunnerFactoryMock.Object,
					jobObjectMock.Object,
					hostRegistrySettingsMock.Object,
					errorReporterProxyMock.Object,
					nudgingControllerMock.Object);
			}

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_1Runner_1BusyGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
				},
				new[]
				{
					"FOO",
				},
				new[]
				{
					"Task:FOO-Group:BAR-IsIdle:False",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", false },
				});

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_3Runners_1BusyGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
					{ "BAZ", "BAR" },
					{ "QWE", "BAR" }
				},
				new[]
				{
					"FOO",
					"BAZ",
					"QWE",
				},
				new[]
				{
					"Task:FOO-Group:BAR-IsIdle:False",
					"Task:BAZ-Group:BAR-IsIdle:False",
					"Task:QWE-Group:BAR-IsIdle:False",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", false },
				});

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_1Runners_1IdleGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
				},
				new[]
				{
					"FOO",
				},
				new[]
				{
					"Task:FOO-Group:BAR-IsIdle:True",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", true },
				});

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_3Runners_1IdleGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
					{ "BAZ", "BAR" },
					{ "QWE", "BAR" }
				},
				new[]
				{
					"FOO",
					"BAZ",
					"QWE",
				},
				new[]
				{
					"Task:QWE-Group:BAR-IsIdle:True",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", true },
				});

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_3Runners1BusyGroup_3RunnersDefaultBusyGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
					{ "BAZ", "BAR" },
					{ "QWE", "BAR" }
				},
				new[]
				{
					"FOO",
					"BAZ",
					"QWE",
					"ASD",
					"DSA",
					"DON",
				},
				new[]
				{
					"Task:FOO-Group:BAR-IsIdle:False",
					"Task:BAZ-Group:BAR-IsIdle:False",
					"Task:QWE-Group:BAR-IsIdle:False",
					"Task:ASD-Group:-IsIdle:False",
					"Task:DSA-Group:-IsIdle:False",
					"Task:DON-Group:-IsIdle:False",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", false },
					{ string.Empty, false },
				});

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_3Runners1BusyGroup_3RunnersDefaultIdleGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
					{ "BAZ", "BAR" },
					{ "QWE", "BAR" }
				},
				new[]
				{
					"FOO",
					"BAZ",
					"QWE",
					"ASD",
					"DSA",
					"DON",
				},
				new[]
				{
					"Task:FOO-Group:BAR-IsIdle:False",
					"Task:BAZ-Group:BAR-IsIdle:False",
					"Task:QWE-Group:BAR-IsIdle:False",
					"Task:DON-Group:-IsIdle:True",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", false },
					{ string.Empty, true },
				});

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupNewRunners_3Runners1BusyGroup_3Runners1IdleGroup_3RunnersDefaultIdleGroup() => AssertTaskSpecificGroup(
				new Dictionary<string, string>()
				{
					{ "FOO", "BAR" },
					{ "BAZ", "BAR" },
					{ "QWE", "BAR" },
					{ "123", "UPG" },
					{ "234", "UPG" },
					{ "345", "UPG" },
				},
				new[]
				{
					"FOO",
					"BAZ",
					"QWE",
					"ASD",
					"DSA",
					"DON",
					"123",
					"234",
					"345",
				},
				new[]
				{
					"Task:FOO-Group:BAR-IsIdle:False",
					"Task:BAZ-Group:BAR-IsIdle:False",
					"Task:QWE-Group:BAR-IsIdle:False",
					"Task:DON-Group:-IsIdle:True",
					"Task:345-Group:UPG-IsIdle:True",
				},
				new Dictionary<string, bool>()
				{
					{ "BAR", false },
					{ "UPG", true },
					{ string.Empty, true },
				});

			void AssertTaskSpecificGroup(
				IReadOnlyDictionary<string, string> taskSpecificGroupRegistry,
				string[] tasksToRun,
				string[] expectedTaskAndRunnerMapping,
				IDictionary<string, bool> groupIdle)
			{
				// Arrange
				hostRegistrySettingsMock
					.Setup(h => h.ServiceTaskRunnerSpecificGroup)
					.Returns(taskSpecificGroupRegistry);
				var taskRunRequests = tasksToRun
					.Select(ttr => Mock.Of<ITaskRunRequest>(r => r.Task == Mock.Of<IRunnableServiceTask>(t => t.Code == ttr && t.HasSchedule)));

				var processId = 0;
				var runnerDict = new Dictionary<int, Mock<IServiceRunner>>();
				serviceRunnerFactoryMock
					.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
					.Returns((ITaskScheduler scheduler, string taskGroup) =>
					{
						var runnerMock = new Mock<IServiceRunner>();
						runnerMock.Setup(r => r.TaskGroup).Returns(taskGroup);
						runnerMock.Setup(r => r.IsIdle).Returns(groupIdle[taskGroup]);
						runnerMock.Setup(r => r.ProcessId).Returns(processId);
						runnerDict.Add(processId, runnerMock);
						processId++;
						return runnerMock.Object;
					});

				// Act
				foreach (var runRequest in taskRunRequests)
				{
					processRunnerPool.GetOrCreateRunnerAsync(runRequest, Mock.Of<ITaskScheduler>(), CancellationToken.None).Wait();
				}

				// Assert
				runnerDict.Values.ForEach(r => r.Setup(r => r.TaskRunning).Returns(true));
				var runnerToTaskMapping = processRunnerPool.GetRunnersSnapshot();
				var result = runnerToTaskMapping
					.Select(m =>
					{
						var runner = runnerDict[m.Item2];
						return $"Task:{m.Item1}-Group:{runner.Object.TaskGroup}-IsIdle:{runner.Object.IsIdle}";
					});
				NUnit.Framework.Assert.That(result, Is.EquivalentTo(expectedTaskAndRunnerMapping));
				hostLoggerMock.Verify(logger => logger.Log(LogLevel.Debug, It.Is<string>(s => s != null && s.StartsWith("Adding runner") && s.Contains("to runner pool group"))), Times.Exactly(expectedTaskAndRunnerMapping.Length));
			}

			[ExpectNoExceptions]
			public void TestTaskSpecificGroupWithDeserializeFailureLogsErrorAndDefaultsToNoGroupBehaviour()
			{
				AssertTaskSpecificGroup(
					new Dictionary<string, string>()
					{
						{ "ERROR", "some error message" },
						{ "FOO", "BAR" },
						{ "BAZ", "BAR" },
						{ "QWE", "BAR" },
						{ "123", "UPG" },
						{ "234", "UPG" },
						{ "345", "UPG" },
					},
					new[]
					{
						"FOO",
						"BAZ",
						"QWE",
						"ASD",
						"DSA",
						"DON",
						"123",
						"234",
						"345",
					},
					new[]
					{
						"Task:FOO-Group:-IsIdle:False",
						"Task:BAZ-Group:-IsIdle:False",
						"Task:QWE-Group:-IsIdle:False",
						"Task:ASD-Group:-IsIdle:False",
						"Task:DSA-Group:-IsIdle:False",
						"Task:DON-Group:-IsIdle:False",
						"Task:123-Group:-IsIdle:False",
						"Task:234-Group:-IsIdle:False",
						"Task:345-Group:-IsIdle:False",
					},
					new Dictionary<string, bool>()
					{
						{ "BAR", false },
						{ "UPG", true },
						{ string.Empty, false },
					});
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "Task specific group could not be deserialized from registry, defaulting to no grouping: [some error message]"));
			}

			[ExpectNoExceptions]
			public void TestLastRunnerRemovedFromPoolDeletesPool()
			{
				// Arrange
				var serviceRunnerMock = new Mock<IServiceRunner>();
				serviceRunnerMock.Setup(r => r.TaskGroup).Returns(string.Empty);
				serviceRunnerMock.Setup(r => r.ProcessId).Returns(100);
				processRunnerPool.Add(serviceRunnerMock.Object);

				// Act
				serviceRunnerMock.Raise(r => r.Exited += null, EventArgs.Empty);

				// Assert
				hostLoggerMock.Verify(l => l.Log(LogLevel.Debug, "Removing last runner from task Group pool []"), Times.Once);
			}

			[ExpectNoExceptions]
			public void TestRunnerGroupingPoolMultithreadedAccess()
			{
				// Arrange
				var taskNamesAndGroups = Enumerable.Range(0, 100)
					.Select(i => ($"{i}", $"{i}"))
					.ToArray();
				var taskSpecificGroupRegistry = taskNamesAndGroups
					.ToDictionary(t => t.Item1, t => t.Item2);
				var tasksToRun = taskNamesAndGroups.Select(t => t.Item1).ToArray();

				hostRegistrySettingsMock
					.Setup(h => h.ServiceTaskRunnerSpecificGroup)
					.Returns(taskSpecificGroupRegistry);
				var taskRunRequests = tasksToRun
					.Select(ttr => Mock.Of<ITaskRunRequest>(r => r.Task == Mock.Of<IRunnableServiceTask>(t => t.Code == ttr && t.HasSchedule)));

				var runnerDict = new ConcurrentDictionary<int, Mock<IServiceRunner>>();
				serviceRunnerFactoryMock
					.Setup(f => f.Create(It.IsAny<ITaskScheduler>(), It.IsAny<string>()))
					.Returns((ITaskScheduler scheduler, string taskGroup) =>
					{
						var processId = int.Parse(taskGroup);
						var runnerMock = new Mock<IServiceRunner>();
						runnerMock.Setup(r => r.TaskGroup).Returns(taskGroup);
						runnerMock.Setup(r => r.IsIdle).Returns(false);
						runnerMock.Setup(r => r.ProcessId).Returns(processId);
						runnerDict.TryAdd(processId, runnerMock);
						return runnerMock.Object;
					});

				// Act
				var tasks = new List<Task>();
				foreach (var runRequest in taskRunRequests)
				{
					Mock.Get(runRequest)
						.Setup(r => r.FormatRequestToLogMessage(LogMessageStage.RunnerIsCreated, It.IsAny<object[]>()))
						.Returns((LogMessageStage s, object[] array) =>
						{
							if (runnerDict.TryGetValue(int.Parse(runRequest.Task.Code), out var runner))
							{
								runner.Raise(r => r.Exited += null, EventArgs.Empty);
							}
							return string.Empty;
						});
					tasks.Add(Task.Run(() => processRunnerPool.GetOrCreateRunnerAsync(runRequest, Mock.Of<ITaskScheduler>(), CancellationToken.None)));
				}

				// Assert
				AssertNoExceptionThrown(() => Task.WaitAll(tasks.ToArray()));
				hostLoggerMock.Verify(logger => logger.Log(LogLevel.Error, It.IsAny<string>()), Times.Never);
			}

			[ExpectNoExceptions]
			public void TestNoPoolFoundOnExitReportsError()
			{
				// Arrange
				var serviceRunnerMock = new Mock<IServiceRunner>();
				serviceRunnerMock.Setup(r => r.TaskGroup).Returns(string.Empty);
				serviceRunnerMock.Setup(r => r.ProcessId).Returns(100);
				processRunnerPool.Add(serviceRunnerMock.Object);
				serviceRunnerMock.Setup(r => r.TaskGroup).Returns("SomeDifferentGroup");

				// Act
				serviceRunnerMock.Raise(r => r.Exited += null, EventArgs.Empty);

				// Assert
				errorReporterProxyMock.Verify(l => l.ReportOnce("Could not find runner for removal from task group pool [SomeDifferentGroup]", It.IsAny<Exception>()), Times.Once);
			}

			ProcessRunnerPool processRunnerPool;
			Mock<IHostLogger> hostLoggerMock;
			Mock<IBackgroundThreadActionQueue> backgroundThreadActionQueueMock;
			Mock<IBackgroundThreadActionQueueFactory> backgroundThreadActionQueueFactoryMock;
			Mock<IServiceRunnerFactory> serviceRunnerFactoryMock;
			Mock<IJobObject> jobObjectMock;
			Mock<IHostRegistrySettings> hostRegistrySettingsMock;
			Mock<INudgingController> nudgingControllerMock;
			Mock<IErrorReporterProxy> errorReporterProxyMock;
		}

		class ProcessRunnerPoolForTest : ProcessRunnerPool
		{
			public ProcessRunnerPoolForTest(IHostLogger hostLogger, IBackgroundThreadActionQueueFactory actionQueueFactory, IServiceRunnerFactory serviceRunnerFactory, IErrorReporterProxy errorReporterProxy, INudgingController nudgingController = null, IHostRegistrySettings  hostRegistry = null)
				: base(hostLogger, actionQueueFactory, serviceRunnerFactory, Mock.Of<IJobObject>(), hostRegistry ?? Mock.Of<IHostRegistrySettings>(), errorReporterProxy, nudgingController)
			{
			}

			public void CheckIdleRunnersExposed()
			{
				CheckIdleRunners();
			}
		}
	}
}
