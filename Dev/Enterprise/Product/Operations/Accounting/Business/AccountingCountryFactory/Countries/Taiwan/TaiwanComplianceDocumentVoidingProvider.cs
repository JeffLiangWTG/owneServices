using System.Linq;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class TaiwanComplianceDocumentVoidingProvider : IComplianceDocumentVoidingProvider
	{
		bool IComplianceDocumentVoidingProvider.IsAllowedSpecialVoid(AccComplianceDocumentHeader complianceDocumentHeader) => complianceDocumentHeader != null && complianceDocumentHeader.ADH_TransactionType == TransactionTypes.Invoice;

		bool IComplianceDocumentVoidingProvider.ShouldPreventVoidAmendingInvoiceWithCreditNote(InvoicingBase[] invoicingBases) => invoicingBases?.Any(x => x.AH_TransactionType == TransactionTypes.Invoice && x.GetRelatedAmendingTransactions().Count > 0) ?? false;
	}
}
