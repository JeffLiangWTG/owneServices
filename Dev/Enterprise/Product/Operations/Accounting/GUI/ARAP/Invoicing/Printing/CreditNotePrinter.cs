using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;

namespace Enterprise.Accounting.GUI
{
	public class CreditNotePrinter : InvoicePrinter
	{
		protected override bool IsGovtTaxInvoicePrintingUsed
		{
			get { return false; }
		}

		protected override InvoicePrintTask NewTask(InvoicingBase arTransaction, InvoicePrintContext context)
		{
			return new InvoicePrintTask(new InvoicePrintTask.Configuration(arTransaction.PK));
		}
	}
}
