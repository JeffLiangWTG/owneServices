using CargoWise.EntityFramework;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.FeatureControl.Module
{
	public class FeatureSetModule : ZFilterGridModule
	{
		#region Implementation

		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.FeatureSet;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.FeatureSet;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new FeatureSetController();
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new FeatureSetFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new FeatureSetFilterControl(GridCollection, (FeatureSetFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new FeatureControlSetCollection(Factory);
		}

		public override bool AllowDelete => false;

		#endregion
	}
}
