#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class DepositBatchCreator
	{
		public CashBook.DepositBatch.DepositBatch RelatedDepositBatch_ForTestOnly
		{
			get { return RelatedDepositBatch; }
			set { RelatedDepositBatch = value; }
		}
	}
}

#endif
