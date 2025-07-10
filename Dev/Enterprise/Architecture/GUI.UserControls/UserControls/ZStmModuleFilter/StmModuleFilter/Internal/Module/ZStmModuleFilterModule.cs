using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmModuleFilterModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.StmModuleFilter;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;
		public override bool AllowView => false;
		public override bool AllowDelete => false;
		public override bool AllowDefaultActivateDeactivate => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.StmModuleFilter);
		}

		protected override bool ShowRecentItemsCore()
		{
			return false;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ZStmModuleFilterFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ZStmModuleFilterFilterControl(GridCollection, (ZStmModuleFilterFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new StmModuleFilterCollection(Factory);
		}
	}
}
