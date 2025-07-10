using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GLConsolidationGroupModule : ZFilterGridModule
	{
		public GLConsolidationGroupModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GLConsolidationGroups; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.GLConsolidationGroups; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.GLConsolidations; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.GLConsolidationGroups);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GLConsolidationGroupFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccConsolidationGroupCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GLConsolidationGroupFilterBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}
	}
}