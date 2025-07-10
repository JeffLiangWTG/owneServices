using Enterprise.Accounting.Business.GeneralLedger.GLJournals;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public partial class MultiCompaniesGLJournalFlatFileDataImporter
	{
		public GLJournalCollection ImportedJournals_ForTestOnly => ImportedJournals;
	}
}
