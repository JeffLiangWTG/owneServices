
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class BankReconcilliationModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.BankReconcilliation; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.BankReconciliation; }
		}

		protected override ZPopupController GetNewController()
		{
			return new BankReconcilliationController();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
