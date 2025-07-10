#if DEBUG

using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoicePrinter
	{
		public void PrintSelfBillingInvoice_ForTestOnly(TransactionHeader selfBillingInvoice)
		{
			PrintSelfBillingInvoice(selfBillingInvoice);
		}

		public void PrintCostConfirmationDocument_ForTestOnly(TransactionHeader transaction)
		{
			PrintCostConfirmationDocument(transaction);
		}

		public TransactionPrintingResults PrintARTransaction_ForTestOnly(Form parentForm, InvoicePrintContext context, InvoicingBase arTransaction)
		{
			return PrintARTransaction(parentForm, context, arTransaction);
		}

		public void PrintNormalInvoice_ForTestOnly(InvoicingBase aRTransaction, string message, string caption, InvoicePrintContext context)
		{
			PrintNormalInvoice(aRTransaction, message, caption, context);
		}
	}
}

#endif
