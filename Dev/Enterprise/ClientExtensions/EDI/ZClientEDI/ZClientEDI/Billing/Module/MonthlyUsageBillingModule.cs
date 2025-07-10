using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class MonthlyUsageBillingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.MonthlyUsageBilling; }
		}

		protected override ZPopupController GetNewController()
		{
			return new MonthlyUsageBillingController();
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
