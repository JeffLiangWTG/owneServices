using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Host.Testing;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Host.Abstractions;
using WTG.NUnit;

namespace Enterprise.ServiceManager.Host.Helpers.Testing
{
	class TaskQueueTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEnqueueTaskEnqueuesTask()
		{
			// Arrange
			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);

			var numTasksToAdd = 10;

			// Act
			for (var i = 0; i < numTasksToAdd; i++)
			{
				var task = new Mock<IRunnableServiceTask>();
				task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task.Object));
			}

			// Assert
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(numTasksToAdd), "Tasks were added");
		}

		[ExpectNoExceptions]
		public void TestEnqueueTask_DoesNotEnqueueTask_IfItsInTheQueue()
		{
			// Arrange
			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);
			var task = new Mock<IRunnableServiceTask>();
			task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());

			// Act
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task.Object));

			// Assert
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(1), "Only one task was added");
		}

		[ExpectNoExceptions]
		public void TestEnqueueTask_EnqueuesTask_IfRunRequestCameFromDifferentSource()
		{
			// Arrange
			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);
			var task = new Mock<IRunnableServiceTask>();
			task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());

			// Act
			runnableQueue.EnqueueTask(new DirectTaskRunRequest(task.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task.Object));

			// Assert
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(2), "Two tasks were added");
		}

		[ExpectNoExceptions]
		public void TestEnqueueTask_EnqueuesDirectAndScheduledRequestsTogether()
		{
			// Arrange
			var loggerMock = new Mock<IHostLogger>();
			var runnerPoolMock = Mock.Of<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPoolMock, loggerMock.Object);
			var taskMock = Mock.Of<IRunnableServiceTask>(
				serviceTask => serviceTask.Code == "TSK"
								&& serviceTask.TimeSinceLastEnqueued == Stopwatch.StartNew()
								&& serviceTask.TimeSinceLastDequeued == Stopwatch.StartNew());
			var directTaskRunRequest = new DirectTaskRunRequest(taskMock, false);
			var scheduledTaskRunRequest = new ScheduledTaskRunRequest(taskMock);

			// Act
			runnableQueue.EnqueueTask(scheduledTaskRunRequest);
			runnableQueue.EnqueueTask(directTaskRunRequest);

			// Assert
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().Count(), Is.EqualTo(2));
		}

		public void TestEnqueueTask_DoesNotEnqueueDirectRequestIfEchoLimitIsReached()
		{
			// Arrange
			var loggerMock = new Mock<IHostLogger>();
			var runnerPoolMock = Mock.Of<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPoolMock, loggerMock.Object);
			var taskMock = Mock.Of<IRunnableServiceTask>(
				serviceTask => serviceTask.Code == "TSK"
								&& serviceTask.TimeSinceLastEnqueued == Stopwatch.StartNew()
								&& serviceTask.TimeSinceLastDequeued == Stopwatch.StartNew());
			var directTaskRunRequest = new DirectTaskRunRequest(taskMock, true);

			QueueAndDequeToReflectTheProductionCase();

			// Act
			runnableQueue.EnqueueTask(directTaskRunRequest);

			// Assert
			AssertNoExceptionThrown(() =>
			{
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request is enqueued 1 time, echo 0/1.$", RegexOptions.IgnoreCase)), Times.Once);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request is enqueued 2 time, echo 0/1.$", RegexOptions.IgnoreCase)), Times.Once);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request is enqueued 3 time, echo 0/1.$", RegexOptions.IgnoreCase)), Times.Once);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request is enqueued 4 time, echo 1/1.$", RegexOptions.IgnoreCase)), Times.Once);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request has no attempts remaining, queued 4 times, echo 2/1.$", RegexOptions.IgnoreCase)), Times.Once);
				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request has no attempts remaining, queued 4 times, echo 3/1.$", RegexOptions.IgnoreCase)), Times.Once);

				loggerMock.Verify(logger => logger.Log(It.IsAny<LogLevel>(), It.IsRegex("Nudge run request is dequeued.", RegexOptions.IgnoreCase)), Times.Exactly(4));
				loggerMock.VerifyNoOtherCalls();
			});

			void QueueAndDequeToReflectTheProductionCase()
			{
				runnableQueue.EnqueueTask(directTaskRunRequest);
				runnableQueue.TryDequeueTask(out _);
				directTaskRunRequest.OnSuccessfulRunAttempt();
				directTaskRunRequest.OnUnableToRun(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock, true, true);

				runnableQueue.EnqueueTask(directTaskRunRequest);
				runnableQueue.TryDequeueTask(out _);
				directTaskRunRequest.OnSuccessfulRunAttempt();
				directTaskRunRequest.OnUnableToRun(UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock, true, true);

				runnableQueue.EnqueueTask(directTaskRunRequest);
				runnableQueue.TryDequeueTask(out _);
				directTaskRunRequest.OnSuccessfulRunAttempt();
				directTaskRunRequest.OnSuccessfulRun();

				runnableQueue.EnqueueTask(directTaskRunRequest);
				runnableQueue.TryDequeueTask(out _);
				directTaskRunRequest.OnSuccessfulRunAttempt();
				directTaskRunRequest.OnSuccessfulRun();

				runnableQueue.EnqueueTask(directTaskRunRequest);
				runnableQueue.TryDequeueTask(out _);
				directTaskRunRequest.OnSuccessfulRunAttempt();
				directTaskRunRequest.OnSuccessfulRun();
			}
		}

		[ExpectNoExceptions]
		public void TestTryDequeueNextTask_ReEnqueuesTasksExceedingRunningCount()
		{
			// Arrange
			var repo = new MockRepository(MockBehavior.Default);
			var logger = repo.Create<IHostLogger>();
			var runnerPool = repo.Create<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);

			var taskExceedingRunningCount = repo.Create<IRunnableServiceTask>();
			taskExceedingRunningCount.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			taskExceedingRunningCount.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
			runnerPool.Setup(rp => rp.RunningCount(taskExceedingRunningCount.Object)).Returns(999);
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(taskExceedingRunningCount.Object));

			var numTasksToAdd = 3;
			for (var i = 0; i < numTasksToAdd; i++)
			{
				var task = repo.Create<IRunnableServiceTask>();
				task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				task.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());
				runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task.Object));
			}

			ITaskRunRequest dequeuedTask;
			for (var i = 0; i < numTasksToAdd; i++)
			{
				runnableQueue.TryDequeueTask(out dequeuedTask);
				NUnit.Framework.Assert.That(dequeuedTask.Task, Is.Not.EqualTo(taskExceedingRunningCount));
			}

			runnableQueue.TryDequeueTask(out dequeuedTask);
			NUnit.Framework.Assert.That(dequeuedTask, Is.EqualTo(default(ITaskRunRequest)));
			var snapShot = runnableQueue.GetQueueSnapshot();
			NUnit.Framework.Assert.That(snapShot.Count(), Is.EqualTo(1));
			NUnit.Framework.Assert.That(snapShot.Last(), Is.EqualTo(taskExceedingRunningCount.Object));
		}

		[ExpectNoExceptions]
		public void TestTryDequeueNextTask_RetainsOrderAndReturnsFalseIfAllTasksInQueueExceededRunningCount()
		{
			// Arrange
			var repo = new MockRepository(MockBehavior.Default);
			var logger = repo.Create<IHostLogger>();

			var runnerPool = repo.Create<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);

			var t1 = repo.Create<IRunnableServiceTask>();
			t1.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			runnerPool.Setup(rp => rp.RunningCount(t1.Object)).Returns(999);
			var t2 = repo.Create<IRunnableServiceTask>();
			t2.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			runnerPool.Setup(rp => rp.RunningCount(t2.Object)).Returns(999);
			var t3 = repo.Create<IRunnableServiceTask>();
			t3.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			runnerPool.Setup(rp => rp.RunningCount(t3.Object)).Returns(999);
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t1.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t2.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t3.Object));

			// Act
			var result = runnableQueue.TryDequeueTask(out var dequeuedTask);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(false));
			NUnit.Framework.Assert.That(dequeuedTask, Is.EqualTo(default(ITaskRunRequest)));
			var snapshot = runnableQueue.GetQueueSnapshot();
			NUnit.Framework.Assert.That(snapshot.ElementAt(0), Is.EqualTo(t1.Object));
			NUnit.Framework.Assert.That(snapshot.ElementAt(1), Is.EqualTo(t2.Object));
			NUnit.Framework.Assert.That(snapshot.ElementAt(2), Is.EqualTo(t3.Object));
		}

		[ExpectNoExceptions]
		public void TestTryDequeueNextTask_ReturnsLastQueueElementIfAllPreviousAreExceedingRunningCount()
		{
			// Arrange
			var repo = new MockRepository(MockBehavior.Default);
			var logger = repo.Create<IHostLogger>();

			var runnerPool = repo.Create<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);

			var t1 = repo.Create<IRunnableServiceTask>();
			runnerPool.Setup(rp => rp.RunningCount(t1.Object)).Returns(999);
			t1.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			var t2 = repo.Create<IRunnableServiceTask>();
			runnerPool.Setup(rp => rp.RunningCount(t2.Object)).Returns(999);
			t2.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			var t3 = repo.Create<IRunnableServiceTask>();
			runnerPool.Setup(rp => rp.RunningCount(t3.Object)).Returns(999);
			t3.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			var t4 = repo.Create<IRunnableServiceTask>();
			t4.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
			t4.Setup(t => t.TimeSinceLastDequeued).Returns(new Stopwatch());

			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t1.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t2.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t3.Object));
			runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(t4.Object));

			// Act
			var result = runnableQueue.TryDequeueTask(out var dequeuedTask);

			// Assert
			NUnit.Framework.Assert.That(result, Is.EqualTo(true));
			NUnit.Framework.Assert.That(dequeuedTask.Task, Is.EqualTo(t4.Object));
			var snapshot = runnableQueue.GetQueueSnapshot();
			NUnit.Framework.Assert.That(snapshot.ElementAt(0), Is.EqualTo(t1.Object));
			NUnit.Framework.Assert.That(snapshot.ElementAt(1), Is.EqualTo(t2.Object));
			NUnit.Framework.Assert.That(snapshot.ElementAt(2), Is.EqualTo(t3.Object));
		}

		[ExpectNoExceptions]
		public void TestEnqueueTaskStartsEnqueuedTimer()
		{
			// Arrange
			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);

			var numTasksToAdd = 10;

			// Act
			for (var i = 0; i < numTasksToAdd; i++)
			{
				var task = TaskSchedulerTest.CreateTask(numTasksToAdd.ToString(), Factory);
				runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task));
			}

			// Assert
			NUnit.Framework.Assert.That(runnableQueue.GetQueueSnapshot().All(t => t.TimeSinceLastEnqueued.IsRunning), Is.True, "All timers started");
		}

		[ExpectNoExceptions]
		public void TestEmptyQueueEmptiesQueue()
		{
			// Arrange
			var repo = new MockRepository(MockBehavior.Default);
			var logger = repo.Create<IHostLogger>();
			var runnerPool = repo.Create<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);

			var numTasksToAdd = 10;
			for (var i = 0; i < numTasksToAdd; i++)
			{
				var task = repo.Create<IRunnableServiceTask>();
				task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task.Object));
			}

			var priorCount = runnableQueue.GetQueueSnapshot().Count();

			// Act
			runnableQueue.EmptyQueue();

			// Assert
			NUnit.Framework.Assert.That(priorCount, Is.EqualTo(numTasksToAdd), "Correct number of tasks added");
			NUnit.Framework.Assert.That(!runnableQueue.GetQueueSnapshot().Any(), Is.True, "EmptyQueue emptied the queue");
		}

		[ExpectNoExceptions]
		public void TestDequeueTaskDequeuesFromTopOfTheQueue()
		{
			//Arrange
			var schedule = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule));
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_TaskPeriod = "D";
			schedule.S5_DailyStartTime = new ZDateTime(1900, 01, 01, 14, 00, 00);
			var reference = DateTime.UtcNow;
			schedule.S5_NextScheduledPrintRunTimeUtc = reference;
			var task2 = TaskSchedulerTest.CreateTask("BBB", Factory);
			Factory.Save();

			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var queueProcessor = new TaskQueue(runnerPool.Object, logger.Object);

			ITaskRunRequest taskToRun;

			//Act
			queueProcessor.EnqueueTask(new ScheduledTaskRunRequest(task1));
			queueProcessor.EnqueueTask(new ScheduledTaskRunRequest(task2));

			NUnit.Framework.Assert.That(reference, Is.EqualTo(schedule.S5_NextScheduledPrintRunTimeUtc).Using(CustomComparers.TypeComparison), "Value did not change");

			var dequeueResult = queueProcessor.TryDequeueTask(out taskToRun);

			//Assert
			NUnit.Framework.Assert.That(dequeueResult, Is.True, "Dequeue result successful");
			NUnit.Framework.Assert.That(taskToRun.Task, Is.EqualTo(task1), "Correct task was dequeued.");
			NUnit.Framework.Assert.That(queueProcessor.GetQueueSnapshot().ElementAt(0), Is.EqualTo(task2), "Second task still in queue");
		}

		[TestUtcOffset(0, 0, 0)]
		[ExpectNoExceptions]
		public void TestDequeueTaskSetsDequeuedUTC()
		{
			//Arrange
			var schedule = TaskSchedulerTest.CreateSchedule("AAA", Factory);
			var task1 = TaskSchedulerTest.CreateTask(new SchedulerServiceTask(schedule));
			schedule.S5_TaskPeriodCount = 1;
			schedule.S5_TaskPeriod = "D";
			schedule.S5_DailyStartTime = new ZDateTime(1900, 01, 01, 14, 00, 00);
			schedule.S5_NextScheduledPrintRunTimeUtc = DateTime.UtcNow;
			Factory.Save();

			var logger = new Mock<IHostLogger>();
			var runnerPool = new Mock<IProcessRunnerPool>();
			var queueProcessor = new TaskQueue(runnerPool.Object, logger.Object);

			ITaskRunRequest taskToRun;

			//Act
			queueProcessor.EnqueueTask(new ScheduledTaskRunRequest(task1));

			NUnit.Framework.Assert.That(task1.TimeSinceLastDequeued.IsRunning, Is.EqualTo(false), "Timer is not yet running");

			var dequeueResult = queueProcessor.TryDequeueTask(out taskToRun);

			//Assert
			NUnit.Framework.Assert.That(dequeueResult, Is.True, "Dequeue result successful");
			NUnit.Framework.Assert.That(taskToRun.Task, Is.EqualTo(task1), "Correct task was dequeued.");
			NUnit.Framework.Assert.That(task1.TimeSinceLastDequeued.IsRunning, Is.EqualTo(true), "Timer is running.");
		}

		[SnailTest]
		public void TestTaskQueueGetSnapshotIsThreadSafe()
		{
			// Arrange
			var repo = new MockRepository(MockBehavior.Default);
			var logger = repo.Create<IHostLogger>();
			var runnerPool = repo.Create<IProcessRunnerPool>();
			var runnableQueue = new TaskQueue(runnerPool.Object, logger.Object);
			var mockTasksBagHelper = new ConcurrentBag<IRunnableServiceTask>();

			using var mres = new ManualResetEventSlim(false);

			for (var i = 0; i < 10000; i++)
			{
				var task = repo.Create<IRunnableServiceTask>();
				task.Setup(t => t.TimeSinceLastEnqueued).Returns(new Stopwatch());
				mockTasksBagHelper.Add(task.Object);
			}

			var enqueuingTask = Task.Factory.StartNew(() =>
			{
				mres.Wait();
				for (var j = 0; j < 10000; j++)
				{
					if (mockTasksBagHelper.TryTake(out var task))
					{
						runnableQueue.EnqueueTask(new ScheduledTaskRunRequest(task));
					}
				}
			}, TaskCreationOptions.LongRunning);

			var snapshottingTask = Task.Factory.StartNew(() =>
			{
				mres.Wait();
				for (var j = 0; j < 20000; j++)
				{
					var result = runnableQueue.GetQueueSnapshot();
				}
			}, TaskCreationOptions.LongRunning);

			// Act
			mres.Set();

			// Assert
			AssertNoExceptionThrown(() =>
			{
				enqueuingTask.Wait();
				snapshottingTask.Wait();
			});
		}

		public void TestWrongParamsCall()
		{
			NUnit.Framework.Assert.Multiple(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TaskQueue(null, Mock.Of<IHostLogger>()));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("runnerPool"));

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TaskQueue(Mock.Of<IProcessRunnerPool>(), null));
				NUnit.Framework.Assert.That(result.ParamName, Is.EqualTo("hostLogger"));
			});
		}
	}
}
