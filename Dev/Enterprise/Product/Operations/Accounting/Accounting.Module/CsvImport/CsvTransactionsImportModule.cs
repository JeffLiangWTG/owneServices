using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class CsvTransactionsImportModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.CsvTransactionsImport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Accountant; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportCsvAccountingTransactions; }
		}

		protected override ZPopupController GetNewController()
		{
			return new CsvTransactionsImportController();
		}

#if DEBUG
		public CsvTransactionsImportController GetNewController_ForTest()
		{
			return (CsvTransactionsImportController)GetNewController();
		}
#endif
	}
}
