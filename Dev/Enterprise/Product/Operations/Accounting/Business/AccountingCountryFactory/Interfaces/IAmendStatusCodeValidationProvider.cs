using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IAmendStatusCodeValidationProvider
	{
		public void ValidateAmendStatusCode(InvoicingBase invoicingBase);

		public void ValidateAmendStatusCodeForInvoice(InvoicingBase invoicingBase);

		public void ValidateAmendStatusCodeForInvoiceReversal(InvoicingBase invoicingBase);
	}
}
