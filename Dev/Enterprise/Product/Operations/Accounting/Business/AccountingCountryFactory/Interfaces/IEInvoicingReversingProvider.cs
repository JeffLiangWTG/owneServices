using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	internal interface IEInvoicingReversingProvider
	{
		bool CanReverseInvoice(InvoicingBase transaction);

		string GetPreventInvoiceReversingPrompt();
	}
}
