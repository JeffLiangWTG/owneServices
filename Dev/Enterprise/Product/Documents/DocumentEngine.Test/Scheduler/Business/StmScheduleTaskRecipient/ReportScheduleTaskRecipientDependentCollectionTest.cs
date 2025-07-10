using CargoWise.EntityFramework;
using Enterprise.Scheduler.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(ReportScheduleTaskRecipientDependentCollection))]
	sealed class ReportScheduleTaskRecipientDependentCollectionTest : StmScheduleTaskRecipientDependentCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ReportScheduleTask scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			return new ReportScheduleTaskRecipientDependentCollection(scheduleTask);
		}

		public void TestRptScheduleTask()
		{
			var reportScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var collection = new ReportScheduleTaskRecipientDependentCollection(reportScheduleTask);

			AssertEquals(collection.RptScheduleTask, reportScheduleTask);
		}
	}
}
