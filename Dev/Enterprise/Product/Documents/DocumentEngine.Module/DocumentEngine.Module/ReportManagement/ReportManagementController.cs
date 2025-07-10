using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.GUI.Scheduler;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.Scheduler.Module
{
	public class ReportManagementController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ModuleIdentifier ModuleID => ModuleIDs.ReportManagement;

		public override ControllerID ID => ControllerIDs.ReportManagement;

		public override Type TypeOfTopLevelBusinessObject => typeof(ReportScheduleTask);

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ScheduleTaskForm((ReportScheduleTask)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView => Env.Security.ReportManagementView;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.ReportManagement;

		protected override SecurityCheckpoint CheckPointForNew => Env.Security.ReportManagement;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.ReportManagement;
	}
}
