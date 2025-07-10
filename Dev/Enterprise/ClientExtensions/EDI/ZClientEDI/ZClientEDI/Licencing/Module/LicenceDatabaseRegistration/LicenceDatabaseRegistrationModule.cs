using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module
{
	public class LicenceDatabaseRegistrationModule : ZPopupModule
	{
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.LicenceDatabaseRegistration;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.LicenceDatabaseRegistration;

		protected override ZPopupController GetNewController() => new LicenceDatabaseRegistrationController();
	}
}
