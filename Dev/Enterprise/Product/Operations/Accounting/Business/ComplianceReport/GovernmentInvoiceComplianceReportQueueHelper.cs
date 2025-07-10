using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class GovernmentInvoiceComplianceReportQueueHelper : IGovernmentInvoiceComplianceReportQueueHelper
	{
		public void QueueForComplianceReports(IAccTransactionHeader transaction)
		{
			if (transaction.AH_TransactionType == TransactionTypes.Invoice
				|| transaction.AH_TransactionType == TransactionTypes.CreditNote
				|| transaction.AH_TransactionType == TransactionTypes.AdjustmentNote)
			{
				var factory = new BusinessObjectFactory();
				var governmentInvoice = transaction as GovernmentInvoice;
				var wrappedInvoice = factory.Load<InvoicingBase>(governmentInvoice.PK);
				if (wrappedInvoice != null)
				{
					wrappedInvoice.AH_ComplianceSubType = governmentInvoice.AH_ComplianceSubType;
					wrappedInvoice.AH_TransactionReference = governmentInvoice.AH_TransactionReference;
					wrappedInvoice.QueueForComplianceReports(governmentInvoice.Factory);
				}
			}
		}
	}
}
