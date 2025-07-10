#if DEBUG

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class InvoicingBaseReversing
	{
		public bool CanTransactionBeReversed_ForTestOnly()
		{
			return CanTransactionBeReversed();
		}

		public ARAP.Invoicing.InvoicingBase OriginalInvocingBase_ForTestOnly => OriginalInvoicingBase;

		public bool HasNonCancelledAmendingTransactions_ForTestOnly => HasNonCancelledAmendingTransactions;

		public string CantReverseTransactionAttachedToClaimErrorMessage_ForTestOnly => CantReverseTransactionAttachedToClaimErrorMessage;

		public string TransactionIsInInvoiceBatchErrorMessage_ForTestOnly => TransactionIsInInvoiceBatchErrorMessage;

		public string TransactionFromOtherCompanyErrorMessage_ForTestOnly => TransactionFromOtherCompanyErrorMessage;

		public bool HasGeneratedComplianceDocument_ForTestOnly => HasGeneratedComplianceDocument;

		public string HasGeneratedComplianceDocumentErrorMessage_ForTestOnly => HasGeneratedComplianceDocumentErrorMessage;
	}
}

#endif
