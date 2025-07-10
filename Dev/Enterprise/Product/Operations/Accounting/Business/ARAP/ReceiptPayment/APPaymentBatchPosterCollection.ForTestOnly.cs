#if DEBUG

using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class APPaymentBatchPosterCollection
	{
		public TransactionHeaderCollection ReloadedTransactionCollection_ForTestOnly => ReloadedTransactionCollection;
	}
}

#endif
