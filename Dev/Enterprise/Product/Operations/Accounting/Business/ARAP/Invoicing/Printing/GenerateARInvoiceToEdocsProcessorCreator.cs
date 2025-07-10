using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing
{
	class GenerateARInvoiceToEdocsProcessorCreator : IGenerateARInvoiceToEdocsProcessorCreator
	{
		public IProcessor GenerateARInvoiceToEdocsProcessor(IWorkflowProvider provider)
		{
			IProcessor result = null;
			InvoicingBase invoice = provider as InvoicingBase;
			if (invoice != null && invoice.AH_Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable &&
				(invoice.AH_TransactionType == TransactionTypes.CreditNote || invoice.AH_TransactionType == TransactionTypes.Invoice || invoice.AH_TransactionType == TransactionTypes.AdjustmentNote))
			{
				result = new GenerateARInvoiceToEdocsProcessor(invoice);
			}
			return result;
		}
	}
}
