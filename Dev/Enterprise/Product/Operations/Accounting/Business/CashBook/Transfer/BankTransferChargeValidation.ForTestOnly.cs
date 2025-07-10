#if DEBUG

using Enterprise.Accounting.Business.CashBook.DirectPayment;

namespace Enterprise.Accounting.Business.CashBook.Transfer
{
	public partial class BankTransferChargeValidation : DirectPaymentValidation
	{
		public static string ExchangeRatesPositiveMessage_ForTestOnly => ExchangeRatesPositiveMessage;
	}
}

#endif
