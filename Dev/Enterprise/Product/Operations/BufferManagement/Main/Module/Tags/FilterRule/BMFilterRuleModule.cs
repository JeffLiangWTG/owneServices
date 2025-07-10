using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMFilterRuleModule : ProcessHeaderModule
	{
		public BMFilterRuleModule()
			: base(shouldAddOperationalActions: false)
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BMFilterRule; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BMFilterRule);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new FilterRuleWorkflowCollection(Factory, new ZQuery());
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BMFilterRuleFilterControl(GridCollection, (BMFilterRuleFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BMFilterRuleFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.BMFilterRule; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}
	}
}
