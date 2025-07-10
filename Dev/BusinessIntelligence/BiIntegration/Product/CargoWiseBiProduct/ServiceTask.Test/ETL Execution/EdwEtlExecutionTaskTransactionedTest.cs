using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	[TestedType(typeof(EdwEtlExecutionTask))]
	class EdwEtlExecutionTaskTransactionedTest : ServiceTaskTestCase<EdwEtlExecutionTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new EdwEtlExecutionTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			AssertEquals("IsActive", ZBool.True, taskSchedule.SST_Active);
			Assert("TaskPeriod", taskSchedule.Recurrence.MinutesRange);
			AssertEquals("TaskPeriodCount", 15, taskSchedule.Recurrence.Period);
			AssertEquals("WeekDaysOnly", ZBool.False, taskSchedule.Recurrence.WeekDaysOnly);
			AssertEquals("Is DailyStartTime empty?", true, taskSchedule.Recurrence.CalcDailyStartTimeUtc.IsEmpty);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
