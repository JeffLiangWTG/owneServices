using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ReportStatisticsController))]
	sealed class ReportStatisticsControllerTest : Enterprise.ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new TestReportStatisticsController();
			AssertEquals("CheckPointForView", Env.Security.ReportStatisticsView, controller.CheckPointForView);
			AssertEquals("CheckPointForEdit", Env.Security.ReportStatistics, controller.CheckPointForEdit);
			AssertEquals("CheckPointForNew", Env.Security.ReportStatistics, controller.CheckPointForNew);
			AssertEquals("CheckPointForDelete", Env.Security.ReportStatistics, controller.CheckPointForDelete);
		}

		public void TestShowNewForm()
		{
			StmReportRun menuItem = Factory.NewWithValidTestData<StmReportRun>();
			Factory.Save();
			var controller = new ReportStatisticsController();
			controller.ShowViewForm(menuItem);
			using (ReportStatisticsForm form = (ReportStatisticsForm)controller.LastShownForm)
			{
				Assert(true);
			}
		}

		#region Test Classes

		class TestReportStatisticsController : ReportStatisticsController
		{
			public new SecurityCheckpoint CheckPointForView
			{
				get { return base.CheckPointForView; }
			}

			public new SecurityCheckpoint CheckPointForEdit
			{
				get { return base.CheckPointForEdit; }
			}

			public new SecurityCheckpoint CheckPointForNew
			{
				get { return base.CheckPointForNew; }
			}

			public new SecurityCheckpoint CheckPointForDelete
			{
				get { return base.CheckPointForDelete; }
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject stmReportRun = Factory.NewWithValidTestData(typeof(StmReportRun));
			Factory.Save();
			return stmReportRun;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ReportStatistics;
		}

		#endregion
	}
}
