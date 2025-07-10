using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Netting
{
	public interface INettingClearingJournalExporter
	{
		bool ExportClearingJournal(OrgHeader nettingSystemOrgHeader, ZString participantEHubID, ZString csvContent);
	}
}
