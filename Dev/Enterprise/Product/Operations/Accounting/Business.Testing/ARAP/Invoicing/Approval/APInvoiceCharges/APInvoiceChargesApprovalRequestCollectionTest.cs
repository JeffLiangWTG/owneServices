using Enterprise.Accounting.Business.TransactionApproval.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(APInvoiceChargesApprovalRequestCollection))]
	public class APInvoiceChargesApprovalRequestCollectionTest : TransactionApprovalRequestCollectionTest<APInvoiceChargesApprovalRequestCollection, APInvoiceChargesApprovalRequest>
	{
	}
}
