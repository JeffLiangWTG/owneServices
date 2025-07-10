using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class SetConsolidatedInvoiceRefForAmendedTransactionStrategy
	{
		public SetConsolidatedInvoiceRefForAmendedTransactionStrategy(InvoicingBase source)
		{
			this.source = source;
		}

		readonly InvoicingBase source;

		public void SetConsolidatedInvoiceRef(InvoicingBase target)
		{
			if (source.IsConsolInvoice)
			{
				target.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextConsolARInvoiceNumber(source.Factory, source.ConsolNumberFromConsolidatedInvoiceRef, target.PK);
			}
			else if (source.Job != null && source.Job is IPostingJob)
			{
				target.AH_ConsolidatedInvoiceRef = InvoiceLiteralNumberGenerator.GetNextAndUpdateUniqueJobARInvoiceNumber(target, (IPostingJob)source.Job);
			}
		}
	}
}