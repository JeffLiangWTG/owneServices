using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class EdiUserAgreementAcceptanceLogModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.UserAgreementAcceptances;
		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.UserAgreementAcceptanceLogs;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(Modules.ClientControllerRegistration.UserAgreementAcceptances);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EdiUserAgreementAcceptanceLogCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EdiUserAgreementAcceptanceLogFilterControl(GridCollection, (EdiUserAgreementAcceptanceLogFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EdiUserAgreementAcceptanceLogFilterBusinessObject();
		}

		public override bool AllowNew => false;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowView => false;
	}
}
