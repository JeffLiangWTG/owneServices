#if DEBUG

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class InvoicingPostManager
	{
		public void ProcessEligibleCharges_ForTestOnly(Posting.IReceivablesPostingChargeCollection filteredCharges)
		{
			ProcessEligibleCharges(filteredCharges);
		}

		public APInvoiceCreator FCostTransactionCreator_ForTestOnly
		{
			get { return fCostTransactionCreator; }
			set { fCostTransactionCreator = value; }
		}

		public APCreditNoteCreator FAPCreditNoteCreator_ForTestOnly
		{
			get { return fAPCreditNoteCreator; }
			set { fAPCreditNoteCreator = value; }
		}
	}
}

#endif
