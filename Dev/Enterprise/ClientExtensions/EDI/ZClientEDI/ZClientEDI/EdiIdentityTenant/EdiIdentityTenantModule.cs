using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IdentityTenant
{
	internal class EdiIdentityTenantModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ClientModuleRegistration.EdiIdentityTenant;
		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.EdiIdentityTenant;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ClientControllerRegistration.EdiIdentityTenant);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiIdentityTenantCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiIdentityTenantFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EdiIdentityTenantFilterBusinessObject();
		}

		public override bool AllowNew => true;
		public override bool AllowEdit => true;
		public override bool AllowDelete => false;
		public override bool AllowView => true;
	}
}
