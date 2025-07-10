#if DEBUG

namespace Enterprise.Accounting.Business.Base.Reversing
{
	public partial class DepositBatchReversing
	{
		public string DirectCreditReceiptMessage_ForTestOnly => DirectCreditReceiptMessage;

		public string ContainCancelledReceiptMessage_ForTestOnly => ContainCancelledReceiptMessage;

		public string PeriodSubLedgerClosedMessage_ForTestOnly => PeriodSubLedgerClosedMessage;

		public string ClearedInCashBookMessage_ForTestOnly => ClearedInCashBookMessage;
	}
}

#endif
