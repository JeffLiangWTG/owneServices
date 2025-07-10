using CargoWise.Types;

namespace Enterprise.Accounting.GUI.ARAP.Payment
{
	public interface IPaymentBatchPrint : IPaymentPrint
	{
		void PrintDocumentsForPaymentBatch(ZBool printPaymentVouchers, ZBool printRemittanceAdvices, ZBool printCheques, ZBool printPaymentBatchListing);
		void AutoPrintCheques(ZGuid printerPK);
	}
}
