#if DEBUG

using Enterprise.Accounting.GUI.ARAP.Payment;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	public partial class PaymentPrintManager
	{
		public PaymentPrintOptions PaymentPrintOption_ForTestOnly
		{
			set
			{
				PaymentPrintOption = value;
			}
		}

		public IPaymentPrint PaymentPrinter_ForTestOnly => PaymentPrinter;

		public IPaymentPrint PaymentPrinterField_ForTestOnly
		{
			set
			{
				fPaymentPrinter = value;
			}
		}
	}
}

#endif
