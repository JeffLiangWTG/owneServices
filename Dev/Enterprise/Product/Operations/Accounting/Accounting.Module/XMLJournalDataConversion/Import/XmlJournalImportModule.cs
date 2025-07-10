using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class XmlJournalImportModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OustandingJournalsImport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ImportOutstandingJournals; }
		}

		protected override ZPopupController GetNewController()
		{
			return new XmlJournalImportController();
		}
	}
}
