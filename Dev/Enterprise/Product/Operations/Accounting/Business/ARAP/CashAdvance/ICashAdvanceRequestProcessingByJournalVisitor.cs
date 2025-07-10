using Enterprise.Accounting.Business.ARAP.Journal;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public interface ICashAdvanceRequestProcessingByJournalVisitor
	{
		void Visit(ARJournal arJournal);

		void Visit(APJournal arJournal);
	}
}
