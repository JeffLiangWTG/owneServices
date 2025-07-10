using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class XmlJournalExportModule : ZPopupModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.OustandingJournalsExport; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AlwaysAllow; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.ExportOutstandingJournals; }
		}

		protected override ZPopupController GetNewController()
		{
			return new XMLJournalExportController();
		}
	}
}
