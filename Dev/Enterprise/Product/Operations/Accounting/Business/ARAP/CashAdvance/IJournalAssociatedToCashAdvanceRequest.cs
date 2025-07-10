namespace Enterprise.Accounting.Business.ARAP.CashAdvance
{
	public interface IJournalAssociatedToCashAdvanceRequest
	{
		void Accept(ICashAdvanceRequestProcessingByJournalVisitor visitor);
	}
}
