using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration
{
	internal class eNettOutboundFinancialInvoiceDataAdapter : ExportFinancialInvoiceDataAdapter
	{
		protected override bool ShouldPopulateAttachment(InvoicingBase invoice)
		{
			return invoice.AH_Ledger == LedgerTypes.AccountsReceivable;
		}
	}
}
