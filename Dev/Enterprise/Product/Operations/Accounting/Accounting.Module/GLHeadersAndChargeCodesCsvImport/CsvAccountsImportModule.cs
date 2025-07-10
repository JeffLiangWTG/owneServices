using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class CsvAccountsImportModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CsvAccountsImport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Accountant; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportCsvChartOfAccounts; }
		}

		protected override ZPopupController GetNewController()
		{
			return new CsvAccountsChartImportController();
		}
	}
}
