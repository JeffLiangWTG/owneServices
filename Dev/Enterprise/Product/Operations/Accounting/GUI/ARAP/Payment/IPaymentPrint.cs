using CargoWise.Types;

namespace Enterprise.Accounting.GUI.ARAP.Payment
{
	public interface IPaymentPrint
	{
		void PrintPaymentVoucher();
		void PrintRemittanceAdvice();
		void PrintCheque();
		void AutoPrintCheque(ZGuid printerPK);

		string PaymentType
		{
			get;
		}

		ZString PaymentOrganisationCode { get; }
		ZString PaymentTypeCode { get; }
		ZString PaymentChequeOrReference { get; }

		void PrintPaymentBatchListing();
	}
}
