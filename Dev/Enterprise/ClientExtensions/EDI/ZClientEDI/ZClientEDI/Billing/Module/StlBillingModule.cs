using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class StlBillingModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.StlBilling; }
		}

		protected override ZPopupController GetNewController()
		{
			return new StlBillingController();
		}

		#region Checkpoints

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.STLBilling; }
		}

		#endregion
	}
}
