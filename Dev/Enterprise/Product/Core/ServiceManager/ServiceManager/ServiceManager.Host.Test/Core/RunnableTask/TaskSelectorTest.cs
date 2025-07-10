using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Testing.Core.RunnableTask;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class TaskSelectorTest : TestCaseWithFactory
	{
		[TestDate(2012, 4, 11, 9, 0, 0)]
		[TestUtcOffset(0, 0, 0)]
		[ExpectNoExceptions]
		public void TestSelectTasksToRun()
		{
			var host1 = Factory.New<StmServiceHost>();
			host1.SH_HostName = "host1";

			var schedule1 = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));

			var schedule2 = TaskSchedulerTest.CreateSchedule("BBB", Factory);
			schedule2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Now.AddMinutes(1);
			var task2 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule2));

			var schedule3 = TaskSchedulerTest.CreateSchedule("HHH", Factory);
			var taskThatFailedToSchedule = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule3));

			var request = new Mock<ITaskRunRequest>();
			request
				.Setup(x => x.FormatRequestToLogMessage(It.IsAny<LogMessageStage>(), It.IsAny<object[]>()))
				.Returns(string.Empty);

			taskThatFailedToSchedule.HandleUnableToRun(UnableToRunReason.ConfigurationError, request.Object, retry: true, failedPostScheduleUpdate: true);

			var scheduleInactive = TaskSchedulerTest.CreateSchedule("CCC", Factory);
			var taskInactive = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(scheduleInactive));
			scheduleInactive.S5_IsActive = false;
			var mockHostLogger = Mock.Of<IHostLogger>();

			var taskWithoutSchedule = TaskSchedulerTest.CreateTaskWithNoSchedule("FFF");
			var scheduleAlreadyRunning = TaskSchedulerTest.CreateSchedule("GGG", Factory);
			var taskAlreadyRunning = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(scheduleAlreadyRunning));

			var processRunningPool = new Mock<IProcessRunnerPool>();
			processRunningPool.Setup(prp => prp.RunningCount(taskAlreadyRunning)).Returns(1);
			var taskSelector = new TaskSelector(processRunningPool.Object, mockHostLogger);

			IRunnableServiceTask[] allTasks = { task1, task2, taskThatFailedToSchedule, taskInactive, taskWithoutSchedule, taskAlreadyRunning };
			var tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "tasksToRun.Count");
			NUnit.Framework.Assert.That(tasksToRun.ElementAt(0).Task, NUnit.Framework.Is.EqualTo(task1), "task1");

			task1.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddMinutes(-1), SetNextRuntimeReason.ScheduledToRun);
			task2.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset(), SetNextRuntimeReason.ScheduledToRun);
			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(2), "tasksToRun.Count");
			NUnit.Framework.Assert.That(tasksToRun.ElementAt(0).Task, NUnit.Framework.Is.EqualTo(task1), "task1 earlier");
			NUnit.Framework.Assert.That(tasksToRun.ElementAt(1).Task, NUnit.Framework.Is.EqualTo(task2), "task2 later");

			task2.SetNextRunTime(ZDateTimeOffset.UtcNow.ToDateTimeOffset().AddMinutes(-2), SetNextRuntimeReason.ScheduledToRun);
			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(2), "tasksToRun.Count");
			NUnit.Framework.Assert.That(tasksToRun.ElementAt(1).Task, NUnit.Framework.Is.EqualTo(task1), "task1 later");
			NUnit.Framework.Assert.That(tasksToRun.ElementAt(0).Task, NUnit.Framework.Is.EqualTo(task2), "task2 earlier");

			// fix all the problem tasks
			scheduleInactive.S5_IsActive = true;
			taskWithoutSchedule = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(TaskSchedulerTest.CreateSchedule("FFF", Factory)));
			allTasks = new[] { task1, task2, taskThatFailedToSchedule, taskInactive, taskWithoutSchedule, taskAlreadyRunning };
			scheduleAlreadyRunning.SecondaryProcessesMaxCount = 2;
			var scheduleAttemptTime = ZDateTime.UtcNow.ToDateTime();

			TestDateAttribute.Date = scheduleAttemptTime.AddSeconds(TaskScheduler.FailedScheduleRetryTime.TotalSeconds / 2);
			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(allTasks.Length - 1), "tasksToRun.Count");

			TestDateAttribute.Date = scheduleAttemptTime.Add(TaskScheduler.FailedScheduleRetryTime);
			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(allTasks.Length), "tasksToRun.Count");
		}

		[TestDate(2012, 4, 11, 9, 0, 0)]
		[TestUtcOffset(0, 0, 0)]
		[ExpectNoExceptions]
		public void TestSelectTasksToRun_SecondaryProcesses()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_HostName = "host1";

			var schedule1 = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));
			schedule1.SecondaryProcessesMaxCount = 3;
			schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow;
			IRunnableServiceTask[] allTasks = { task1 };

			var mockHostLogger = Mock.Of<IHostLogger>();
			var processRunningPool = new Mock<IProcessRunnerPool>();
			processRunningPool.SetupSequence(prp => prp.RunningCount(task1)).Returns(0).CallBase();
			var taskSelector = new TaskSelector(processRunningPool.Object, mockHostLogger);

			var tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");
			processRunningPool.SetupSequence(prp => prp.RunningCount(task1)).Returns(1).CallBase();

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");
			processRunningPool.SetupSequence(prp => prp.RunningCount(task1)).Returns(2).CallBase();

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");
			processRunningPool.SetupSequence(prp => prp.RunningCount(task1)).Returns(3).CallBase();

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");
			processRunningPool.SetupSequence(prp => prp.RunningCount(task1)).Returns(4).CallBase();

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(0), "runner not started");
		}

		[TestDate(2014, 8, 18, 13, 56, 0)]
		[TestUtcOffset(0, 0, 0)]
		[ExpectNoExceptions]
		public void TestSelectTasksToRun_BacklogClearing()
		{
			var host = Factory.New<StmServiceHost>();
			host.SH_HostName = "host1";

			var schedule1 = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));
			schedule1.SecondaryProcessesMaxCount = 2;
			var allTasks = new[] { task1 };

			var mockHostLogger = Mock.Of<IHostLogger>();
			var processRunningPool = new Mock<IProcessRunnerPool>();
			processRunningPool.Setup(prp => prp.RunningCount(task1)).Returns(0);

			var taskSelector = new TaskSelector(processRunningPool.Object, mockHostLogger);

			var tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			// put the task in future and set startTime (usual scheduling)
			schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow + TimeSpan.FromDays(1);
			processRunningPool.Setup(prp => prp.RunningCount(task1)).Returns(1);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");
			processRunningPool.Setup(prp => prp.RunningCount(task1)).Returns(2);

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(1), "runner started");
			processRunningPool.Setup(prp => prp.RunningCount(task1)).Returns(3);

			tasksToRun = taskSelector.SelectTasksToRun(allTasks);
			NUnit.Framework.Assert.That(tasksToRun.Count(), NUnit.Framework.Is.EqualTo(0), "runner not started"); //exceeded max secondary
		}

		[ExpectNoExceptions]
		public void TestSelectTasksToRun_ScheduledTaskReturnsScheduledRequest()
		{
			// Arrange
			var taskMock = new Mock<IRunnableServiceTask>();
			taskMock
				.SetupGet(task => task.IsActive)
				.Returns(true);
			taskMock
				.SetupGet(task => task.NextRunTimeAllowingForLocalSchedulingFailures)
				.Returns(new DateTimeOffset(2019, 12, 23, 20, 20, 0, TimeSpan.FromHours(0)));

			var hostLoggerMock = new Mock<IHostLogger>();
			var processRunningPoolMock = new Mock<IProcessRunnerPool>();
			var taskSelector = new TaskSelector(processRunningPoolMock.Object, hostLoggerMock.Object);

			// Act
			var result = taskSelector.SelectTasksToRun(new[] { taskMock.Object })
				.Single();

			// Assert
			NUnit.Framework.Assert.That(result is IScheduledTaskRunRequest, NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestLogMessageWhenNewScheduledRequest()
		{
			// Arrange
			const string code = "LWM";
			var sequence = new MockSequence();
			var logger = new Mock<IHostLogger>();
			logger
				.InSequence(sequence)
				.Setup(x => x.LogSection(It.IsAny<LogLevel>(), It.IsAny<string>(), It.IsAny<string>()));
			logger
				.InSequence(sequence)
				.Setup(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<string>()));

			var runnerPool = new Mock<IProcessRunnerPool>();
			runnerPool
				.Setup(x => x.RunningCount(It.IsAny<IRunnableServiceTask>()))
				.Returns(int.MinValue);
			var runner = new TaskSelector(runnerPool.Object, logger.Object);
			var randomAncientDate = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.FromHours(0));

			var taskList = new List<IRunnableServiceTask>()
			{
				Mock.Of<IRunnableServiceTask>(x =>
					x.Code == code
				&& x.IsActive
				&& x.NextRunTimeAllowingForLocalSchedulingFailures == randomAncientDate),
			};

			// Act
			runner.SelectTasksToRun(taskList);

			// Assert
			logger.Verify(x => x.LogSection(LogLevel.Debug, "Creating Scheduled run requests for startable tasks.", It.Is<Func<string>>(y => y.Invoke() == "1 Scheduled run requests for startable tasks are created and enqueued.")), Times.Once);
			logger.Verify(x => x.Log(LogLevel.Debug, It.IsRegex($"\\[{code}/{TaskRunRequestTest<ScheduledTaskRunRequest>.GuidRegExTemplate}\\] Scheduled run request is created.")), Times.Once);
			logger.VerifyNoOtherCalls();
		}

		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TaskSelector(null, Mock.Of<IHostLogger>()));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("runnerPool"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TaskSelector(Mock.Of<IProcessRunnerPool>(), null));
				NUnit.Framework.Assert.That(result.ParamName, NUnit.Framework.Is.EqualTo("hostLogger"));
			});
		}
	}
}
