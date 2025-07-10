using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing
{
	class DispatchingLoopThrottlingTest : TestCaseWithFactory
	{
		[TestDate(2016, 04, 13, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestGetTimeToSleepIfQueueIsEmpty_SelectsTimeUntilTheNextTaskIsToBeRun()
		{
			// Arrange
			var schedule1 = TaskSchedulerTest.CreateSchedule("ONE", Factory);
			schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(5); // later
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));
			var schedule2 = TaskSchedulerTest.CreateSchedule("TWO", Factory);
			schedule2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(1); // earlier
			var task2 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule2));
			var allTasks = new List<IRunnableServiceTask>() { task1, task2 };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottling(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, Mock.Of<IHostRegistrySettings>());

			// Act
			var result = throttler.GetTimeToSleep(new List<IRunnableServiceTask>(), allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleepIfQueueIsEmptyAndTasksRunning_ShouldConsiderSecondaryProcessesSpinUpDelay()
		{
			// Arrange
			var schedule1 = TaskSchedulerTest.CreateSchedule("ONE", Factory);
			schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(1);
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));
			var allTasks = new List<IRunnableServiceTask>() { task1 };
			var runnerPool = new Mock<IProcessRunnerPool>();
			runnerPool.Setup(rp => rp.RunningCount(null)).Returns(1);
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottling(taskSelector.Object, runnerPool.Object, TimeSpan.FromSeconds(5), Mock.Of<IHostRegistrySettings>());

			// Act
			var result = throttler.GetTimeToSleep(new List<IRunnableServiceTask>(), allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(TimeSpan.FromSeconds(5)));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleepIfQueueIsEmpty_NextRuntimeIsEmpty()
		{
			// Arrange
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o => o.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog == TimeSpan.FromSeconds(1));

			var schedule1 = TaskSchedulerTest.CreateSchedule("ONE", Factory);
			schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(5); // later
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));
			var schedule2 = TaskSchedulerTest.CreateSchedule("TWO", Factory);
			schedule2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.Empty;
			var task2 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule2));
			var allTasks = new List<IRunnableServiceTask>() { task1, task2 };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottling(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, hostRegistry);

			// Act
			var result = throttler.GetTimeToSleep(new List<IRunnableServiceTask>(), allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(hostRegistry.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog));
		}

		[TestDate(2016, 04, 13, 12, 00, 00)]
		[ExpectNoExceptions]
		public void TestGetTimeToSleepIfQueueIsEmpty_SelectsTimeUntilTheNextTaskIsToBeRun_IgnoresContinuousTasks()
		{
			// Arrange
			var schedule1 = TaskSchedulerTest.CreateSchedule("ONE", Factory);
			schedule1.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(5); // later
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule1));
			var schedule2 = TaskSchedulerTest.CreateSchedule("TWO", Factory);
			schedule2.S5_NextScheduledPrintRunTimeUtc = ZDateTime.UtcNow.AddMinutes(1); // earlier
			var task2 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule2));
			var allTasks = new List<IRunnableServiceTask>() { task1, task2 };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottling(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, Mock.Of<IHostRegistrySettings>());

			// Act
			var result = throttler.GetTimeToSleep(new List<IRunnableServiceTask>(), allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(TimeSpan.FromMinutes(1)));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleepIfQueueContainsTasks_RequiringBacklogHelpAndMaxSecondaryReached_SelectsAppropriateMsSleep()
		{
			// Arrange
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o => o.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary == TimeSpan.FromSeconds(1));

			var task = new Mock<IRunnableServiceTask>();
			task.Setup(t => t.MaxSecondaryRunningCount).Returns(9);
			var allTasks = new List<IRunnableServiceTask>() { task.Object };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			taskSelector.Setup(ts => ts.ReachedMaxSecondary(It.IsAny<IRunnableServiceTask>())).Returns(true);
			var throttler = new DispatchingLoopThrottling(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, hostRegistry);

			// Act
			var result = throttler.GetTimeToSleep(allTasks, allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(hostRegistry.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleepRestOfCases_WhenQueueIsNotEmpty_SelectsAppropriateMsSleep()
		{
			// Arrange
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o =>
				o.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary == TimeSpan.FromMilliseconds(1000)
				&& o.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog == TimeSpan.FromMilliseconds(100)
				&& o.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog == TimeSpan.FromMilliseconds(30)
				&& o.ThrottlingTargetTimeToClearQueueBacklog == TimeSpan.FromMilliseconds(5000));

			var task = new Mock<IRunnableServiceTask>();
			var allTasks = new List<IRunnableServiceTask>() { task.Object };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottling(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, hostRegistry);

			// Act
			var result = throttler.GetTimeToSleep(allTasks, allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(hostRegistry.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleep_DetectedBacklogExceeded_ChosenCorrectValue()
		{
			// Arrange
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o => o.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog == TimeSpan.FromSeconds(1));

			var task = new Mock<IRunnableServiceTask>();
			var allTasks = new List<IRunnableServiceTask>() { task.Object };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottlingForTest(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, hostRegistry);
			throttler.SetLoopsAllowedForDeepSleep(100);

			// Act
			var result = throttler.GetTimeToSleep(allTasks, allTasks, true);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(hostRegistry.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleep_DetectedBacklogExceeded_FallBackToHigherValueAfterSetNumberOfIterations()
		{
			// Arrange
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o =>
				o.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary == TimeSpan.FromMilliseconds(1000)
				&& o.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog == TimeSpan.FromMilliseconds(100)
				&& o.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog == TimeSpan.FromMilliseconds(30)
				&& o.ThrottlingTargetTimeToClearQueueBacklog == TimeSpan.FromMilliseconds(5000));

			var allowedLoopsWithoutDeepSleep = 10;
			var task = new Mock<IRunnableServiceTask>();
			var allTasks = new List<IRunnableServiceTask>() { task.Object };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottlingForTest(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, hostRegistry);
			throttler.SetLoopsAllowedForDeepSleep(allowedLoopsWithoutDeepSleep);

			// Act
			var result = TimeSpan.Zero;
			for (var i = 0; i < allowedLoopsWithoutDeepSleep + 1; i++)
			{
				result = throttler.GetTimeToSleep(allTasks, allTasks, true);
			}

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(hostRegistry.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog));
		}

		[ExpectNoExceptions]
		public void TestGetTimeToSleep_IfSystemIsUnregistered()
		{
			// Arrange
			var hostRegistry = Mock.Of<IHostRegistrySettings>(o =>
				o.ThrottlingPeriodForDispatchingLoopWithBacklogWaitingForMaxSecondary == TimeSpan.FromMilliseconds(1000)
				&& o.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog == TimeSpan.FromMilliseconds(100)
				&& o.ThrottlingPeriodForDispatchingLoopWithExceedingBacklog == TimeSpan.FromMilliseconds(30)
				&& o.ThrottlingTargetTimeToClearQueueBacklog == TimeSpan.FromMilliseconds(5000));

			var allowedLoopsWithoutDeepSleep = 10;
			var task = new Mock<IRunnableServiceTask>();
			var allTasks = new List<IRunnableServiceTask>() { task.Object };
			var taskSelector = new Mock<ITaskSelector>();
			taskSelector.Setup(ts => ts.GetTasksPotentiallyEligibleForRunning(It.IsAny<ICollection<IRunnableServiceTask>>())).Returns(allTasks);
			var throttler = new DispatchingLoopThrottlingForTest(taskSelector.Object, new Mock<IProcessRunnerPool>().Object, TimeSpan.MaxValue, hostRegistry);
			throttler.SetLoopsAllowedForDeepSleep(allowedLoopsWithoutDeepSleep);

			// Act
			var result = TimeSpan.Zero;
			for (var i = 0; i < allowedLoopsWithoutDeepSleep + 1; i++)
			{
				result = throttler.GetTimeToSleep(allTasks, allTasks, true);
			}

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(hostRegistry.ThrottlingPeriodForDispatchingLoopWithAcceptableBacklog));
		}
	}

	class DispatchingLoopThrottlingForTest : DispatchingLoopThrottling
	{
		public void SetLoopsAllowedForDeepSleep(int value)
		{
			loopsAllowedWithoutDeepSleep = value;
		}

		public DispatchingLoopThrottlingForTest(ITaskSelector taskSelector, IProcessRunnerPool runnerPool, TimeSpan secondaryProcessesSpinUpDelay, IHostRegistrySettings hostRegistry) : base(taskSelector, runnerPool, secondaryProcessesSpinUpDelay, hostRegistry)
		{
		}
	}
}
