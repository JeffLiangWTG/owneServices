using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.FetchStrategies
{
	public class GLJournalLineFetchStrategy : TransactionLinesFetchStrategy
	{
		public GLJournalLineFetchStrategy(GLJournalLine journalLine)
			: base(journalLine)
		{
		}

		GLJournalLine JournalLine
		{
			get { return BusinessObject as GLJournalLine; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(GenExportBatchSequenceSchema.XB_ParentID, JournalLine.PK);
		}
	}
}