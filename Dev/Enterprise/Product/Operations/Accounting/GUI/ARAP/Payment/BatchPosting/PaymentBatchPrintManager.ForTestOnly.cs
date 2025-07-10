#if DEBUG

using System.Collections.Generic;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Payment;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	public partial class PaymentBatchPrintManager
	{
		public bool CheckIfAllChequesAreAutoPrintedAlready_ForTestOnly(IEnumerable<TransactionHeader> paymentCollection) => CheckIfAllChequesAreAutoPrintedAlready(paymentCollection);

		public IEnumerable<TransactionHeader> PaymentCollection_ForTestOnly => PaymentCollection;

		public IPaymentBatchPrint PaymentPrinter_ForTestOnly => PaymentPrinter;

		public IPaymentBatchPrint GetIPaymentBatchPrint_ForTestOnly() => GetIPaymentBatchPrint();

		public PaymentDocumentsPrintPopup PaymentPrintForm_ForTestOnly => PaymentPrintForm;

		public IPaymentBatchPrint PaymentPrinterField_ForTestOnly
		{
			set => fPaymentPrinter = value;
			get => fPaymentPrinter;
		}
	}
}

#endif
