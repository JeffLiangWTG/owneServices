#if DEBUG

namespace Enterprise.Accounting.Business.CashBook.DepositBatch
{
	public partial class DepositBatchTransactionLineValidation
	{
		public void CheckIsSelected_ForTestOnly()
		{
			CheckIsSelected();
		}

		public static string TransactionAlreadyBatchedMessage_ForTestOnly => TransactionAlreadyBatchedMessage;

		public static string TransactionCancelled_ForTestOnly => TransactionCancelled;
	}
}

#endif
