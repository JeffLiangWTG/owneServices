using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public interface IComplianceDocumentVoidingProvider
	{
		bool IsAllowedSpecialVoid(AccComplianceDocumentHeader complianceDocumentHeader);

		bool ShouldPreventVoidAmendingInvoiceWithCreditNote(InvoicingBase[] invoicingBase);
	}
}
