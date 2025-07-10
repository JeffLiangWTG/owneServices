using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class AccountingVoucherModule : ZPopupModule
	{
		public AccountingVoucherModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccountingVoucher; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AccountingVoucher; }
		}

		protected override ZPopupController GetNewController()
		{
			return new AccountingVoucherController();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}
	}
}
