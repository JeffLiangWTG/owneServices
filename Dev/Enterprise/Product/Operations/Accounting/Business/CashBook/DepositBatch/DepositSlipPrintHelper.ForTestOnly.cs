#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositSlipPrintHelper
	{
		public DepositBatch GetBatchTransactionHeaderFromNumber_ForTestOnly(string batchNum)
		{
			return GetBatchTransactionHeaderFromNumber(batchNum);
		}
	}
}

#endif
