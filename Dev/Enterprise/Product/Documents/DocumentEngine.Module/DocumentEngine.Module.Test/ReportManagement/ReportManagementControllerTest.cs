using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ReportManagementController))]
	sealed class ReportManagementControllerTest : Enterprise.ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			var controller = new TestReportManagementController();
			AssertEquals("CheckPointForView", Env.Security.ReportManagementView, controller.CheckPointForView);
			AssertEquals("CheckPointForEdit", Env.Security.ReportManagement, controller.CheckPointForEdit);
			AssertEquals("CheckPointForNew", Env.Security.ReportManagement, controller.CheckPointForNew);
			AssertEquals("CheckPointForDelete", Env.Security.ReportManagement, controller.CheckPointForDelete);
		}

		public void TestShowViewForm()
		{
			var scheduleReport = Factory.NewWithValidTestData<ReportScheduleTask>();
			Factory.Save();
			var controller = new ReportManagementController();
			controller.ShowViewForm(scheduleReport);
			using (var form = (ScheduleTaskForm)controller.LastShownForm)
			{
				Assert(true);
			}
		}

		#region Test Classes

		class TestReportManagementController : ReportManagementController
		{
			public new SecurityCheckpoint CheckPointForView => base.CheckPointForView;

			public new SecurityCheckpoint CheckPointForEdit => base.CheckPointForEdit;

			public new SecurityCheckpoint CheckPointForNew => base.CheckPointForNew;

			public new SecurityCheckpoint CheckPointForDelete => base.CheckPointForDelete;
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var stmScheduleTask = Factory.NewWithValidTestData(typeof(StmScheduleTask));
			Factory.Save();
			return stmScheduleTask;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ReportManagement;
		}

		#endregion
	}
}
