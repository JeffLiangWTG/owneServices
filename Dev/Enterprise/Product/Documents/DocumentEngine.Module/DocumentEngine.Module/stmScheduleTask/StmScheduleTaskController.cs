using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ScheduledReportsController : ZController, IScheduledReportsController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.ScheduledReports; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ScheduledReports; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ReportScheduleTask); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ScheduleTaskForm((ReportScheduleTask)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.ScheduledTaskView; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.ScheduledTaskEdit; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ScheduledTaskNew; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.ScheduledTaskDelete; }
		}

		protected  SecurityCheckpoint CheckPointForDeleteOtherReport
		{
			get { return Env.Security.ScheduledTaskDeleteOtherReport; }
		}

		public override SecurityCheckpoint GetCheckPointForEdit(BusinessObject bizObject)
		{
			return ((StmScheduleTask)bizObject).S5_SystemCreateUser == Env.CurrentUser.Initials ? Env.Security.ScheduledTaskEdit : Env.Security.ScheduledTaskEditOtherReport;
		}

		public override SecurityCheckpoint GetCheckPointForDelete(BusinessObject bizObject)
		{
			return ((StmScheduleTask)bizObject).S5_SystemCreateUser == Env.CurrentUser.Initials ? Env.Security.ScheduledTaskDelete : Env.Security.ScheduledTaskDeleteOtherReport;
		}

		#region IStmScheduleTaskController Members

		public void ShowNewForm(IStmMenuItem menuItem)
		{
			ReportScheduleTask scheduleTask = (ReportScheduleTask)GetNewBusinessEntityInLocalFactory();
			scheduleTask.PopulateDefaultsFromMenuItem(menuItem);
			scheduleTask.S5_ParentID_ReadOnly = true;
			ShowFormForNewEntity(scheduleTask);
		}

		#endregion
	}
}
