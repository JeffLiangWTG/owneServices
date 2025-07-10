#if DEBUG

using CargoWise.Types;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	public partial class PaymentDocumentsPrinter
	{
		public bool ShouldReprintCheque_ForTestOnly()
		{
			return ShouldReprintCheque();
		}

		public string ChequeTemplate_ForTestOnly
		{
			get { return ChequeTemplate; }
			set { ChequeTemplate = value; }
		}

		public bool ValidateChequePrinting_ForTestOnly(Business.Base.Transaction.TransactionHeader payment)
		{
			return ValidateChequePrinting(payment);
		}

		public bool HasEmptyRecipient_ForTestOnly
		{
			get { return HasEmptyRecipient; }
			set { HasEmptyRecipient = value; }
		}

		public void PrintDocumentsForPaymentBatch_ForTestOnly(ZBool printPaymentVouchers, ZBool printRemittanceAdvices, ZBool printCheques, ZBool printPaymentBatchListing, ZGuid printerPK)
		{
			PrintDocumentsForPaymentBatch(printPaymentVouchers, printRemittanceAdvices, printCheques, printPaymentBatchListing, printerPK);
		}

		public int CountOfPaymentDocumentPacksCreated_ForTestOnly => CountOfPaymentDocumentPacksCreated;
	}
}

#endif
