using CargoWise.EntityFramework;
using Enterprise.ArchiveManager.Business.Schedule;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ArchiveManager.Module.Schedule
{
	public class ArchiveScheduleModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
			=> ModuleIDs.ArchiveSchedule;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
			=> ZControllerFactory.Create(ControllerIDs.ArchiveSchedule);

		protected override IFilterControl GetNewFilterControl()
			=> new ArchiveScheduleFilterControl(GridCollection, (ArchiveScheduleFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection()
			=> new ArchiveScheduleTaskCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject()
			=> new ArchiveScheduleFilterBusinessObject();

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
			=> Env.Licence.Core;

		public override SecurityCheckpoint SecurityCheckpoint
			=> Env.Security.ArchiveSchedule;

		#endregion
	}
}
