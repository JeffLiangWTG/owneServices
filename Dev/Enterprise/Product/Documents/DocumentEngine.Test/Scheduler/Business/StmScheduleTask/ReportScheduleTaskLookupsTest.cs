using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	sealed class ReportScheduleTaskLookupsTest : BusinessObjectLookupsTestCase
	{
		#region MenuItems

		public void TestMenuItems()
		{
			StmMenuItem report = Factory.New<StmMenuItem>();
			report.SU_BusinessContext = "RepAReport";

			StmMenuItem document = Factory.New<StmMenuItem>();
			document.SU_BusinessContext = "ADocument";

			ScheduleTask.Lookups.MenuItems.Load();
			AssertCollectionNotContains(document, ScheduleTask.Lookups.MenuItems);
			AssertCollectionContains(report, ScheduleTask.Lookups.MenuItems);

			AssertType<SchedulableReportCollection>(ScheduleTask.Lookups.MenuItems);
		}

		#endregion

		#region Implementation

		ReportScheduleTask ScheduleTask
		{
			get
			{
				if (scheduleTask == null)
				{
					scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				}
				return scheduleTask;
			}
		}
		ReportScheduleTask scheduleTask;

		#endregion
	}
}
