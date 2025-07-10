using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	[TestedType(typeof(BiDeploymentTask))]
	class BiDeploymentTaskTransactionedTest : ServiceTaskTestCase<BiDeploymentTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new BiDeploymentTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.HoursRange);
			AssertEquals("TaskPeriodCount", 24, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		}

		[TestDate]
		public void TestIsValidTimeOfDayToRun_12am() => AssertTimeOfDay(new DateTime(2017, 4, 7, 0, 0, 0), TimeSpan.Zero);
		public void TestIsValidTimeOfDayToRun_1am() => AssertTimeOfDay(new DateTime(2017, 4, 7, 1, 0, 0), TimeSpan.FromHours(1));
		public void TestIsValidTimeOfDayToRun_12pm() => AssertTimeOfDay(new DateTime(2017, 4, 7, 12, 0, 0), TimeSpan.FromHours(12));

		void AssertTimeOfDay(DateTime testDate, TimeSpan expectedTimeOfDay)
		{
			var testTask = new BiDeploymentTask();

			TestDateAttribute.Date = testDate;
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);

			AssertEquals($"{expectedTimeOfDay} should make valid time to run.", expectedTimeOfDay, taskSchedule.NextRunTime.TimeOfDay);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
