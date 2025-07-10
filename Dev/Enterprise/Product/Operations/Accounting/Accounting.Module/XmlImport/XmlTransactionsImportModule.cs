using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class XmlTransactionsImportModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.XmlTransactionsImport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.InterfaceConnector; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportXmlAccountingTransactions; }
		}

		protected override ZPopupController GetNewController()
		{
			return new XmlTransactionsImportController();
		}
	}
}
