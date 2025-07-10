using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace Enterprise.ServiceManager.Host.Testing.Core.RunnableTask
{
	[TestedType(typeof(ScheduledTaskRunRequest))]
	class ScheduledTaskRunRequestTest : TaskRunRequestTest<ScheduledTaskRunRequest>
	{
		[SetUp]
		public override void SetUp()
		{
			base.SetUp();
			runnableServiceTaskMock = new Mock<IRunnableServiceTask>();
		}

		[Test]
		public void TestTask()
		{
			// Arrange
			var scheduledTaskRunRequest = new ScheduledTaskRunRequest(runnableServiceTaskMock.Object);

			// Act
			var result = scheduledTaskRunRequest.Task;

			// Assert
			Assert.That(result, Is.EqualTo(runnableServiceTaskMock.Object));
		}

		[Test]
		public void TestExpectedNextRunTime()
		{
			Assert.Multiple(() =>
			{
				Test(new DateTimeOffset(2020, 10, 19, 0, 0, 0, TimeSpan.FromSeconds(0)));
				Test(new DateTimeOffset(2020, 10, 20, 0, 0, 0, TimeSpan.FromSeconds(0)));
			});

			void Test(DateTimeOffset dateTime)
			{
				// Arrange
				runnableServiceTaskMock
					.SetupGet(task => task.NextRunTime)
					.Returns(dateTime);
				var scheduledTaskRunRequest = new ScheduledTaskRunRequest(runnableServiceTaskMock.Object);

				// Act
				var result = scheduledTaskRunRequest.ExpectedNextRunTime;

				// Assert
				Assert.That(result, Is.EqualTo(dateTime));
			}
		}

		[Test]
		public void TestNextRunTime()
		{
			Assert.Multiple(() =>
			{
				Test(new DateTimeOffset(2020, 10, 19, 0, 0, 0, TimeSpan.FromSeconds(0)));
				Test(new DateTimeOffset(2020, 10, 20, 0, 0, 0, TimeSpan.FromSeconds(0)));
			});

			void Test(DateTimeOffset dateTime)
			{
				// Arrange
				runnableServiceTaskMock
					.SetupGet(task => task.NextRunTime)
					.Returns(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromSeconds(0)));
				var scheduledTaskRunRequest = new ScheduledTaskRunRequest(runnableServiceTaskMock.Object);
				runnableServiceTaskMock
					.SetupGet(task => task.NextRunTime)
					.Returns(dateTime);

				// Act
				scheduledTaskRunRequest.TakeNextRunTimeFromTask();

				// Assert
				var result = scheduledTaskRunRequest.NextRunTime;
				Assert.That(result, Is.EqualTo(dateTime));
			}
		}

		[Test]
		public void TestNextRunTimeIsNotTaken()
		{
			// Arrange
			runnableServiceTaskMock
				.SetupSequence(task => task.NextRunTime)
				.Returns(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromSeconds(0)));
			var scheduledTaskRunRequest = new ScheduledTaskRunRequest(runnableServiceTaskMock.Object);

			// Act
			// Assert
			Assert.Throws<InvalidOperationException>(() => _ = scheduledTaskRunRequest.NextRunTime);
		}

		[Test]
		public void TestNextRunTimeIsTakenTwice()
		{
			Assert.Multiple(() =>
			{
				Test(new DateTimeOffset(2020, 10, 19, 0, 0, 0, TimeSpan.FromSeconds(0)));
				Test(new DateTimeOffset(2020, 10, 20, 0, 0, 0, TimeSpan.FromSeconds(0)));
			});

			void Test(DateTimeOffset dateTime)
			{
				// Arrange
				runnableServiceTaskMock
					.SetupGet(task => task.NextRunTime)
					.Returns(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.FromSeconds(0)));
				var scheduledTaskRunRequest = new ScheduledTaskRunRequest(runnableServiceTaskMock.Object);
				scheduledTaskRunRequest.TakeNextRunTimeFromTask();

				// Act
				// Assert
				Assert.Throws<InvalidOperationException>(() => scheduledTaskRunRequest.TakeNextRunTimeFromTask());
			}
		}

		Mock<IRunnableServiceTask> runnableServiceTaskMock;

		protected override ScheduledTaskRunRequest[] SameRequestList
		{
			get
			{
				return new ScheduledTaskRunRequest[]
				{
					CreateRequest("ABC"),
					CreateRequest("ABC"),
				};
			}
		}

		protected override ScheduledTaskRunRequest[] DifferentRequestList
		{
			get
			{
				return new ScheduledTaskRunRequest[]
				{
					CreateRequest("ABC"),
					CreateRequest("XXX"),
				};
			}
		}

		protected override ScheduledTaskRunRequest CreateRequest(string code)
		{
			return CreateRequest(code, new StopwatchProxy(), new StopwatchProxy());
		}

		protected override ScheduledTaskRunRequest CreateRequest(string code, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch)
		{
			return CreateRequest(code, stepDurationStopwatch, totalDurationStopwatch, true);
		}

		protected override ScheduledTaskRunRequest CreateRequest(string code, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch, bool echoes)
		{
			return new ScheduledTaskRunRequest(GetMockTask(code), stepDurationStopwatch, totalDurationStopwatch);
		}

		protected override Dictionary<LogMessageStage, (string regMessage, object[] parameters)> ExpectedLogMessages
		{
			get
			{
				var runnerMock = new Mock<ProcessServiceRunnerCore>(Mock.Of<ITaskScheduler>(), Mock.Of<IBackgroundThreadActionQueue>(), Mock.Of<IProcessFactory>(), Mock.Of<IHostLogger>(), Mock.Of<IServiceTaskLocksCleaner>(), Mock.Of<IDateTimeProvider>(), Mock.Of<IErrorReporterProxy>(), Mock.Of<IHostRegistrySettings>(), string.Empty);
				runnerMock
					.Setup(x => x.TaskRunning)
					.Returns(true);
				return new Dictionary<LogMessageStage, (string regMessage, object[] parameters)>()
				{
					{
						LogMessageStage.RequestIsCreated,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request is created.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.AbsorbedByAnotherRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request is absorbed by existing request \\[{GuidRegExTemplate}\\].",
							new object[] { CreateRequest(FormatLogMessagesTestCode) }
						)
					},
					{
						LogMessageStage.ReplacesRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request replaces existing request \\[{GuidRegExTemplate}\\].",
							new object[] { CreateRequest(FormatLogMessagesTestCode) }
						)
					},
					{
						LogMessageStage.EnqueuedRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request is enqueued [0-9]+ time.",
							new object[] { 1 }
						)
					},
					{
						LogMessageStage.DelayedRequestIsCreated,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Delayed Scheduled run request is created.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.DequeuedRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request is dequeued. \\[Time in the queue {TimeDurationRegExTemplate}\\]",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.RequestIsOverDue,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Rescheduling Scheduled run request because it was scheduled to run over [0-9]+\\:[0-9]+\\:[0-9]+ ago.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ReprocessRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Rescheduling Scheduled run request after an attempt failed due to reason\\: Another host was already scheduling this task. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]",
							new object[] { UnableToRunReason.OtherHostIsSchedulingTheTask }
						)
					},
					{
						LogMessageStage.IgnoreRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Ignoring Scheduled run request after an attempt failed due to reason\\: Another host was already scheduling this task. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]",
							new object[] { UnableToRunReason.OtherHostIsSchedulingTheTask }
						)
					},
					{
						LogMessageStage.RequestIsSentToRunner,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request is sent to Runner with PID=1. \\[Total Processing Duration\\: {TimeDurationRegExTemplate}]",
							new object[] { Mock.Of<IServiceRunner>(r => r.ProcessId == 1) }
						)
					},
					{
						LogMessageStage.ObtainingRunner,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Obtaining Runner.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.RunnerIsFound,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Idle Runner found in Runner Pool with Info: PID=None, State=Running. \\[Step Duration\\: {TimeDurationRegExTemplate}]",
							new object[] { runnerMock.Object }
						)
					},
					{
						LogMessageStage.RunnerIsCreated,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] New Runner is created with Info: PID=None, State=Running. \\[Step Duration\\: {TimeDurationRegExTemplate}]",
							new object[] { runnerMock.Object }
						)
					},
					{
						LogMessageStage.TypeNameIsEmpty,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Unable to run task as the TypeName is empty.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.PoolKeyError,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Unable to run task, please specify the Branch in the task settings.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.StoppingAccociatedRunners,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Stopping all associated Runners due to inactive task.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ValidatingRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Validating Scheduled run request.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ValidatedRequest,
						(
							$"\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request is validated.",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.RequestIsQueuedByRunner, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Received queued response from Runner with PID=1. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]$",
							new object[] { Mock.Of<IServiceRunner>(r => r.ProcessId == 1) }
						)
					},
					{
						LogMessageStage.NoRunsAttemptsRemaining, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Scheduled run request has no attempts remaining, queued [0-9]+ times.$",
							Array.Empty<object>()
						)
					},
				};
			}
		}
	}
}
