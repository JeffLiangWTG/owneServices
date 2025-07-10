using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Module.Testing
{
	[TestedType(typeof(ScheduledReportsController))]
	sealed class StmScheduleTaskControllerTest : ZControllerBasherTest
	{
		public void TestSecurityCheckpoints()
		{
			TestStmScheduleTaskController controller = new TestStmScheduleTaskController();
			AssertEquals("CheckPointForView", Env.Security.ScheduledTaskView, controller.CheckPointForView);
			AssertEquals("CheckPointForEdit", Env.Security.ScheduledTaskEdit, controller.CheckPointForEdit);
			AssertEquals("CheckPointForNew", Env.Security.ScheduledTaskNew, controller.CheckPointForNew);
			AssertEquals("CheckPointForDelete", Env.Security.ScheduledTaskDelete, controller.CheckPointForDelete);
		}

		public void TestShowNewForm()
		{
			StmMenuItem menuItem = Factory.New<StmMenuItem>();
			ScheduledReportsController controller = new ScheduledReportsController();
			controller.ShowNewForm(menuItem);
			using (ScheduleTaskForm form = (ScheduleTaskForm)controller.LastShownForm)
			{
				AssertEquals("LastShownForm.BusinessEntity.S5_ParentID", menuItem.PK, form.BusinessEntity.S5_ParentID);
				AssertEquals("LastShownForm's Reports FindBox should be read only", true, form.BusinessEntity.S5_ParentID_ReadOnly);
			}
		}
		public void TestDeleteOtherScheduleUser()
		{
			var scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			scheduleTask.S5_DailyStartTime = new ZDateTime(2017, 3, 1, 23, 0, 0);
			scheduleTask.S5_StartDate = new ZDateTime(2011, 11, 1);
			scheduleTask.S5_SystemCreateUser = "TST";
			scheduleTask.S5_SystemLastEditTimeUtc = new ZDateTime(2017, 1, 21);
			scheduleTask.S5_IsActive = true;

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "USDFW";
			branch.GB_GC = company.PK;
			scheduleTask.S5_GB = branch.PK;
			Factory.Save();

			GlbGroup postMastersGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff staff = postMastersGroup.Staff.AddNew();
			staff.GS_EmailAddress = "sango.peeters@cargowise.com";
			staff.GS_Code = "SPE";
			Factory.Save();

			TestStmScheduleTaskController controller = new TestStmScheduleTaskController();
			AssertEquals("CheckPointForDelete", Env.Security.ScheduledTaskDeleteOtherReport, controller.GetCheckPointForDelete(scheduleTask));
		}

		#region Test Classes

		class TestStmScheduleTaskController : ScheduledReportsController
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

			public new SecurityCheckpoint CheckPointForDeleteOtherReport
			{
				get { return CheckPointForDelete; }
			}
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObject stmScheduleTask = Factory.NewWithValidTestData(typeof(StmScheduleTask));
			Factory.Save();
			return stmScheduleTask;
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ScheduledReports;
		}

		#endregion
	}
}
