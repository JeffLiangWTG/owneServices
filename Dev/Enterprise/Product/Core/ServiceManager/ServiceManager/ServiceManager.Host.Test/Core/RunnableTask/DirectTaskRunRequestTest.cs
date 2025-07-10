using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Host.Testing.Core.RunnableTask
{
	[TestedType(typeof(DirectTaskRunRequest))]
	class DirectTaskRunRequestTest : TaskRunRequestTest<DirectTaskRunRequest>
	{
		protected override DirectTaskRunRequest[] SameRequestList
		{
			get
			{
				return new DirectTaskRunRequest[]
				{
					NewRequest("ABC", false),
					NewRequest("ABC", false),
				};
			}
		}

		protected override DirectTaskRunRequest[] DifferentRequestList
		{
			get
			{
				return new DirectTaskRunRequest[]
				{
					NewRequest("ABC", true),
					NewRequest("XXX"),
					NewRequest("ABC", false),
				};
			}
		}

		DirectTaskRunRequest NewRequest(string code, bool echoes = true)
			=> new DirectTaskRunRequest(GetMockTask(code), echoes);

		protected override DirectTaskRunRequest CreateRequest(string code)
		{
			return CreateRequest(code, new StopwatchProxy(), new StopwatchProxy());
		}

		protected override DirectTaskRunRequest CreateRequest(string code, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch)
		{
			return CreateRequest(code, stepDurationStopwatch, totalDurationStopwatch, true);
		}

		protected override DirectTaskRunRequest CreateRequest(string code, IStopwatch stepDurationStopwatch, IStopwatch totalDurationStopwatch, bool echoes)
		{
			return new DirectTaskRunRequest(GetMockTask(code), stepDurationStopwatch, totalDurationStopwatch, echoes);
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
						LogMessageStage.RequestIsCreated, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request is created.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.AbsorbedByAnotherRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request is absorbed by existing request \\[{GuidRegExTemplate}\\].$",
							new object[] { CreateRequest(FormatLogMessagesTestCode) }
						)
					},
					{
						LogMessageStage.ReplacesRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request replaces existing request \\[{GuidRegExTemplate}\\].$",
							new object[] { CreateRequest(FormatLogMessagesTestCode) }
						)
					},
					{
						LogMessageStage.EnqueuedRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request is enqueued [0-9]+ time, echo [0-9]+/[0-9]+.$",
							new object[] { 1, 1u }
						)
					},
					{
						LogMessageStage.DelayedRequestIsCreated, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Delayed Nudge run request is created.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.DequeuedRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request is dequeued. \\[Time in the queue {TimeDurationRegExTemplate}\\]$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.RequestIsOverDue, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Rescheduling Nudge run request because it was scheduled to run over [0-9]+\\:[0-9]+\\:[0-9]+ ago.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ReprocessRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Requeueing Nudge run request after an attempt failed due to reason\\: Another host was already scheduling this task. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]$",
							new object[] { UnableToRunReason.OtherHostIsSchedulingTheTask }
						)
					},
					{
						LogMessageStage.IgnoreRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Ignoring Nudge run request after an attempt failed due to reason\\: Another host was already scheduling this task. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]$",
							new object[] { UnableToRunReason.OtherHostIsSchedulingTheTask }
						)
					},
					{
						LogMessageStage.RequestIsSentToRunner, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request is sent to Runner with PID=1. \\[Total Processing Duration\\: {TimeDurationRegExTemplate}\\]$",
							new object[] { Mock.Of<IServiceRunner>(r => r.ProcessId == 1) }
						)
					},
					{
						LogMessageStage.TypeNameIsEmpty, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Unable to run task as the TypeName is empty.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.PoolKeyError, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Unable to run task, please specify the Branch in the task settings.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ObtainingRunner, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Obtaining Runner.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.RunnerIsFound, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Idle Runner found in Runner Pool with Info: PID=None, State=Running. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]$",
							new object[] { runnerMock.Object }
						)
					},
					{
						LogMessageStage.RunnerIsCreated, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] New Runner is created with Info: PID=None, State=Running. \\[Step Duration\\: {TimeDurationRegExTemplate}\\]$",
							new object[] { runnerMock.Object }
						)
					},
					{
						LogMessageStage.StoppingAccociatedRunners, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Stopping all associated Runners due to inactive task.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ValidatingRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Validating Nudge run request.$",
							Array.Empty<object>()
						)
					},
					{
						LogMessageStage.ValidatedRequest, (
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request is validated.$",
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
							$"^\\[[0-9A-Z]{{3}}/{GuidRegExTemplate}\\] Nudge run request has no attempts remaining, queued [0-9]+ times, echo [0-9]+/[0-9]+.$",
							Array.Empty<object>()
						)
					},
				};
			}
		}

		[Test]
		public void TestHasRunsRemaining()
		{
			// Arrange
			var runnableServiceTaskMock = Mock.Of<IRunnableServiceTask>();

			var requestWithNoEchoesAndNoRuns = new DirectTaskRunRequest(runnableServiceTaskMock, false);

			var requestWithNoEchoesAndOneRun = new DirectTaskRunRequest(runnableServiceTaskMock, false);
			requestWithNoEchoesAndOneRun.OnSuccessfulRun();

			var requestWithEchoAndNoRuns = new DirectTaskRunRequest(runnableServiceTaskMock, true);

			var requestWithEchoAndOneRun = new DirectTaskRunRequest(runnableServiceTaskMock, true);
			requestWithEchoAndOneRun.OnSuccessfulRun();

			var requestWithEchoAndTwoRuns = new DirectTaskRunRequest(runnableServiceTaskMock, true);
			requestWithEchoAndTwoRuns.OnSuccessfulRun();
			requestWithEchoAndTwoRuns.OnSuccessfulRun();

			// Act
			// Assert
			Assert.That(requestWithNoEchoesAndNoRuns.HasRunsRemaining, Is.EqualTo(true), "NoEchoesAndNoRuns");
			Assert.That(requestWithNoEchoesAndOneRun.HasRunsRemaining, Is.EqualTo(false), "NoEchoesAndOneRun");
			Assert.That(requestWithEchoAndNoRuns.HasRunsRemaining, Is.EqualTo(true), "EchoAndNoRuns");
			Assert.That(requestWithEchoAndOneRun.HasRunsRemaining, Is.EqualTo(true), "EchoAndOneRun");
			Assert.That(requestWithEchoAndTwoRuns.HasRunsRemaining, Is.EqualTo(false), "EchoAndTwoRuns");
		}

		[Test]
		public void TestNextRunDelay()
		{
			// Arrange
			var request = new DirectTaskRunRequest(Mock.Of<IRunnableServiceTask>(), true);

			// Act
			var nextRunDelay1 = request.NextRunDelay;
			request.OnSuccessfulRun();
			var nextRunDelay2 = request.NextRunDelay;

			// Assert
			Assert.That(TimeSpan.FromSeconds(30), Is.EqualTo(nextRunDelay1));
			Assert.That(TimeSpan.FromSeconds(30), Is.EqualTo(nextRunDelay2));
		}

		[Test]
		public void TestHasMoreRetriesRemained()
		{
			var runnableServiceTaskMock = Mock.Of<IRunnableServiceTask>();

			var requestWithOneRetryLeft = new DirectTaskRunRequest(runnableServiceTaskMock, true);

			var requestWithNoRetryLeft = new DirectTaskRunRequest(runnableServiceTaskMock, true);
			requestWithNoRetryLeft.OnSuccessfulRun();

			Assert.That(requestWithOneRetryLeft.HasMoreRetriesRemained(requestWithNoRetryLeft), Is.EqualTo(true));
			Assert.That(requestWithNoRetryLeft.HasMoreRetriesRemained(requestWithOneRetryLeft), Is.EqualTo(false));
			Assert.That(requestWithNoRetryLeft.HasMoreRetriesRemained(null), Is.EqualTo(true));
		}
	}
}
