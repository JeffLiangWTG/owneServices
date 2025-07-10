#if DEBUG

using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class Payment
	{
		public void OnFactorySaved_ForTestOnly(bool saveSucceeded)
		{
			OnFactorySaved(saveSucceeded);
		}

		public void ResetRelatedPaymentApproval_ForTestOnly()
		{
			ResetRelatedPaymentApproval();
		}

		public void SetDDRCollection_ForTestOnly(DirectDebitBatchLineCollection dDRCollection)
		{
			fDDRCollection = dDRCollection;
		}
	}
}

#endif
