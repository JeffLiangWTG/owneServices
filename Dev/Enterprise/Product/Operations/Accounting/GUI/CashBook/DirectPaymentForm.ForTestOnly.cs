#if DEBUG

namespace Enterprise.Accounting.GUI.CashBook
{
	public partial class DirectPaymentForm
	{
		public ARAP.ReceiptPayment.PaymentPrintManager PrintManager_ForTestOnly => PrintManager;
	}
}

#endif
