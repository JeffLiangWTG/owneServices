using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class MaintenanceBillingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.MaintenanceBilling; }
		}

		protected override ZPopupController GetNewController()
		{
			return new MaintenanceBillingController();
		}

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.None; }
		}

		#endregion
	}
}
