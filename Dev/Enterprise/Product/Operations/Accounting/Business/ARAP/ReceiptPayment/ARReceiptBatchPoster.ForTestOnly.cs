#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment
{
	public partial class ARReceiptBatchPoster
	{
		public CashBook.DepositBatch.DepositBatch RelatedDepositBatch_ForTestOnly
		{
			get { return RelatedDepositBatch; }
			set { RelatedDepositBatch = value; }
		}

		public ZBool MatchingObjectsPrepared_ForTestOnly
		{
			get { return MatchingObjectsPrepared; }
			set { MatchingObjectsPrepared = value; }
		}

		public ZBool MatchingObjectsPreparedForTest_ForTestOnly
		{
			get { return MatchingObjectsPreparedForTest; }
			set { MatchingObjectsPreparedForTest = value; }
		}

		public ZBool NeedToCreateDepositBatch_ForTestOnly => NeedToCreateDepositBatch;

		public ZBool IsDepositBatchCreated_ForTestOnly
		{
			get { return IsDepositBatchCreated; }
			set { IsDepositBatchCreated = value; }
		}

		public ZBool NeedToPrintDepositBatch_ForTestOnly => NeedToPrintDepositBatch;

		public ZBool IsDepositBatchPrinted_ForTestOnly
		{
			get { return IsDepositBatchPrinted; }
			set { IsDepositBatchPrinted = value; }
		}

		public void ValidateBankAccountPK_ForTestOnly()
		{
			ValidateBankAccountPK();
		}

		public bool AllowFuturePosting_ForTestOnly => AllowFuturePosting;
	}
}

#endif
