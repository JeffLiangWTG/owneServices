#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatchTransactionLine
	{
		public decimal CorrectSigns_ForTestOnly(decimal oSTotal)
		{
			return CorrectSigns(oSTotal);
		}
	}
}

#endif
