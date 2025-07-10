using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public class SystemUserAccountsModule : ZPopupModule
	{
		public override ModuleIdentifier ID => Modules.ClientModuleRegistration.SystemUserAccounts;

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

		public override SecurityCheckpoint SecurityCheckpoint => EDISecurityCheckpoints.SystemUserAccounts;

		protected override ZPopupController GetNewController() => new SystemUserAccountsController();
	}
}
