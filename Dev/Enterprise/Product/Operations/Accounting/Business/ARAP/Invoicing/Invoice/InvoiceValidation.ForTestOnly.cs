#if DEBUG

using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceValidation
	{
		public void CheckAH_InvoiceTerm_ForTestOnly()
		{
			CheckAH_InvoiceTerm();
		}

		public void CheckAH_InvoiceTermDays_ForTestOnly()
		{
			CheckAH_InvoiceTermDays();
		}

		public AccAPAccountDetails AccountDetails_ForTestOnly => AccountDetails;

		public void ValidateReceiptPaymentCommon_ForTestOnly()
		{
			ValidateReceiptPaymentCommon();
		}

		public void ValidatePayment_ForTestOnly()
		{
			ValidatePayment();
		}
	}
}

#endif
