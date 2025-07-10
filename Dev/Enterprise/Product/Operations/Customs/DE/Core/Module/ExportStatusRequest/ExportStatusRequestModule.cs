using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Module
{
	public class ExportStatusRequestModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.EU.DE.ExportStatusRequest;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.DE.ExportStatusRequest);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new ExportStatusRequestFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new ExportStatusRequestFilterStripControl(GridCollection, (ExportStatusRequestFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new StatusRequestCollection(Factory, GlbBranch.CurrentBranch);

		public override Security.SecurityCheckpoint SecurityCheckpoint => Env.Security.ExportMessaging;

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;
	}
}
