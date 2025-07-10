using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Module
{
	public class BMTagRuleModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public BMTagRuleModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BMTagRule; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.BMTagRule);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BMTagRuleFilterControl(GridCollection, (BMTagRuleFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new TagRuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BMTagRuleFilterBusinessObject();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.BufferManagement; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.TagRule; }
		}

		public override bool AllowUniversalCopy
		{
			get { return false; }
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new BMTagRuleOperationalActionSupporter(); }
		}
	}
}
