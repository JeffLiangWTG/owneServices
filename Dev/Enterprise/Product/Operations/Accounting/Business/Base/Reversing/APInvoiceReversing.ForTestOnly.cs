#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class APInvoiceReversing
	{
		public void GenerateReverseTransactions_ForTestOnly()
		{
			GenerateReverseTransactions();
		}

		public void SetCancellationFlagOnTransactionsToReverse_ForTestOnly()
		{
			SetCancellationFlagOnTransactionsToReverse();
		}

		public void SetReversingDescriptionOnTransactions_ForTestOnly()
		{
			SetReversingDescriptionOnTransactions();
		}

		public void GetMatchLinksFromTransactions_ForTestOnly()
		{
			GetMatchLinksFromTransactions();
		}

		public void SetMatchGroupNumberAndMatchDateOnSaving_ForTestOnly(BusinessObjectFactory factory)
		{
			SetMatchGroupNumberAndMatchDateOnSaving(factory);
		}

		public void SetTransactionBelongsToGroupOnTransactionsToReverse_ForTestOnly()
		{
			SetTransactionBelongsToGroupOnTransactionsToReverse();
		}

		public void FullyPayBothTransactions_ForTestOnly()
		{
			FullyPayBothTransactions();
		}

		public void SetOtherNumberFountainFields_ForTestOnly(BusinessObjectFactory factory)
		{
			SetOtherNumberFountainFields(factory);
		}

		public ARAP.Invoicing.APInvoice InvoiceToReverse_ForTestOnly => InvoiceToReverse;

		public ARAP.Invoicing.APCreditNote ReversingAPCreditNote_ForTestOnly => ReversingAPCreditNote;

		public ARAP.ReceiptPayment.APPayment ReversingAPPayment_ForTestOnly => ReversingAPPayment;

		public ARAP.ReceiptPayment.APPayment APPaymentToReverse_ForTestOnly => APPaymentToReverse;

		public void SetTransactionCount_ForTestOnly()
		{
			SetTransactionCount();
		}

		public Interfaces.IPayablesAndReceivables FReversingPayment_ForTestOnly
		{
			get { return fReversingPayment; }
			set { fReversingPayment = value; }
		}

		public CashBook.DirectDebitBatch.DirectDebitBatchHeader FDirectDebitBatchHeaderToReverse_ForTestOnly
		{
			get { return fDirectDebitBatchHeaderToReverse; }
			set { fDirectDebitBatchHeaderToReverse = value; }
		}
	}
}

#endif
