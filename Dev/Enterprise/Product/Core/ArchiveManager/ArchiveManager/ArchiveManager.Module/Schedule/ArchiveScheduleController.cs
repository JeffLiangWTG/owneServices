using System;
using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.ArchiveManager.GUI.Schedule;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ArchiveManager.Module.Schedule
{
	public class ArchiveScheduleController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany
			=> true;

		public override ControllerID ID
			=> ControllerIDs.ArchiveSchedule;

		public override Type TypeOfTopLevelBusinessObject
			=> typeof(ArchiveScheduleTask);

		protected override IZForm GetForm(IBusiness businessEntity)
			=> new ArchiveScheduleTaskForm(businessEntity as ArchiveScheduleTask);

		public override ModuleIdentifier ModuleID
			=> ModuleIDs.ArchiveSchedule;

		#region Security

		protected override SecurityCheckpoint CheckPointForView
			=> Env.Security.ArchiveSchedule;

		protected override SecurityCheckpoint CheckPointForNew
			=> Env.Security.ArchiveScheduleModify;

		protected override SecurityCheckpoint CheckPointForEdit
			=> Env.Security.ArchiveScheduleModify;

		protected override SecurityCheckpoint CheckPointForDelete
			=> Env.Security.ArchiveScheduleModify;

		#endregion
	}
}
