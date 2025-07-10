using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ServiceManager.Business;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace CargoWise.Bi.Product.ServiceTask.Maintenance.Testing
{
	[TestedType(typeof(AuditDbConsistencyCheckServiceTask))]
	class AuditDbConsistencyCheckServiceTaskTransactionedTest : ServiceTaskTestCase<AuditDbConsistencyCheckServiceTask>
	{
		public void TestInitialiseSchedule()
		{
			var testTask = new AuditDbConsistencyCheckServiceTask();
			InitialiseTaskSchedule(testTask, out StmServiceTask taskSchedule);
			Assert("TaskPeriod", taskSchedule.Recurrence.WeeksRange);
			AssertEquals("TaskPeriodCount", 1, taskSchedule.Recurrence.Period);
			Assert("WeekDayOccurrence", taskSchedule.Recurrence.Sunday);
			AssertEquals("Is DailyStartTime empty?", false, taskSchedule.Recurrence.RecurringStartTimeUtc.Equals(ZDateTime.MinSmallDateTimeValue));
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => System.Array.Empty<TaskNudgeInformationForTest>();
	}
}
