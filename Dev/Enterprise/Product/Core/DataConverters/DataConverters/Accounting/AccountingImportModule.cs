using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataConverters.Accounting
{
	public class AccountingImportModule : ZPopupModule
	{
		public AccountingImportModule()
		{
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportAccountingData; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.ImportAccountingData; }
		}

		protected override ZPopupController GetNewController()
		{
			return new AccountingImportController();
		}

		public override void Show()
		{
			if (!MainForm.IsShown)
			{
				base.Show();
			}
		}
	}
}
