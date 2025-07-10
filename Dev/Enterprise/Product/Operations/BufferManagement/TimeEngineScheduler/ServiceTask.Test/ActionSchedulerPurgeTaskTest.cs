using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.TimeEngineScheduler.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using StatusConstants = Enterprise.TimeEngineScheduler.Integration.Constants.TimeActionScheduleStatus;

namespace Enterprise.TimeEngineScheduler.ServiceTask.Test
{
	[TestedType(typeof(ActionSchedulerPurgeTask))]
	class ActionSchedulerPurgeTaskTest : ServiceTaskTestCase<ActionSchedulerPurgeTask>
	{
		#region Supporting classes

		public class SchedulerPurgeTask_ForTest : ActionSchedulerPurgeTask
		{
			public SchedulerPurgeTask_ForTest(ILogger logger)
			{
				ServiceLogger = logger;
			}

			public override void RunTask(CancellationToken token)
			{
				using (EnvProxy.Instance.TemporaryServiceTaskContext(Code, canRunInAnyBranch: true))
				{
					base.RunTask(token);
				}
			}
		}

		#endregion

		#region ServiceTaskTestCase Impl

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#endregion

		[TestDate]
		public void TestShouldRemoveOldSchedules_IfOlderThanRegistrySetting()
		{
			ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks = 1;

			var provider = new ActionScheduleProvider();
			var schedule = provider.ScheduleAction("TST", ZDateTime.UtcNow.AddMinutes(-1), ZGuid.NewZGuid(), "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N");

			var logger = new TestServiceLogger();
			new SchedulerTask_ForTest(logger).RunTask();
			new SchedulerPurgeTask_ForTest(logger).RunTask(CancellationToken.None);

			CombineAssertions("Nothing was removed", () =>
			{
				AssertNotContains("old schedule(s) were purged.", logger.ToString());

				var loadedSchedule = new BusinessObjectFactory().Load<TimeActionSchedule>(schedule.PK);
				AssertNotNull(loadedSchedule);
				AssertEquals(StatusConstants.Closed, loadedSchedule.TAS_ExecutionStatus);
			});

			// fast-forward 2 months, so that the schedule is now old enough
			TestDateAttribute.AddMonths(2);
			new SchedulerPurgeTask_ForTest(logger).RunTask(CancellationToken.None);

			CombineAssertions("One row should be removed", () =>
			{
				AssertContains("1 old schedule(s) were purged.", logger.ToString());

				var loadedSchedule = new BusinessObjectFactory().Load<TimeActionSchedule>(schedule.PK);
				AssertNull(loadedSchedule);
			});
		}

		[TestDate]
		public void TestShouldRemoveSchedulesImmediately_ByDefault()
		{
			AssertEquals(0, ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks);

			var provider = new ActionScheduleProvider();
			var schedule = provider.ScheduleAction("TST", ZDateTime.UtcNow.AddMinutes(-1), ZGuid.NewZGuid(), "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N");

			var logger = new TestServiceLogger();
			new SchedulerTask_ForTest(logger).RunTask();
			new SchedulerPurgeTask_ForTest(logger).RunTask(CancellationToken.None);

			CombineAssertions("The schedule should be removed immediately.", () =>
			{
				AssertContains("1 old schedule(s) were purged.", logger.ToString());

				var loadedSchedule = new BusinessObjectFactory().Load<TimeActionSchedule>(schedule.PK);
				AssertNull(loadedSchedule);
			});
		}

		[TestDate]
		public void TestRemovesAllTheLogs()
		{
			AssertEquals(0, ObjectFactory.Get<IBMSRegistry>().TimeBeforeDeletingOldScheduledTasks);

			var provider = new ActionScheduleProvider();

			for (int i = 0; i < 10_000; i++)
			{
				provider.ScheduleAction("TST", ZDateTime.UtcNow.AddMinutes(-1), ZGuid.NewZGuid(), "JS", "|EVT=Z00|OFF=000:00|ACT=Reference1|EST=N");
			}

			var logger = new TestServiceLogger();
			new SchedulerTask_ForTest(logger).RunTask();
			new SchedulerPurgeTask_ForTest(logger).RunTask(CancellationToken.None);

			AssertContains("10000 old schedule(s) were purged.", logger.ToString());
		}
	}
}
