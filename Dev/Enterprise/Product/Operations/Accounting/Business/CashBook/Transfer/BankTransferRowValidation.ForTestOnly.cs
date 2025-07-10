#if DEBUG

using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	partial class BankTransferRowValidation : TransactionHeaderValidation
	{
		public static string EnterValidBankAccountMessage_ForTestOnly => EnterValidBankAccountMessage;

		public static string BankAccountsMustBeDifferenthMessage_ForTestOnly => BankAccountsMustBeDifferenthMessage;

		public static string GreaterThanZeroMessage_ForTestOnly => GreaterThanZeroMessage;

		public static string AmountPositiveMessage_ForTestOnly => AmountPositiveMessage;

		public static string ExchangeRatesPositiveMessage_ForTestOnly => ExchangeRatesPositiveMessage;
	}
}

#endif
