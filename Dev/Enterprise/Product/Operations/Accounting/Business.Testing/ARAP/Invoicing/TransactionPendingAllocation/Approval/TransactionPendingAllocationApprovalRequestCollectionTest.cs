using Enterprise.Accounting.Business.TransactionApproval.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalRequestCollection))]
	public class TransactionPendingAllocationApprovalRequestCollectionTest : TransactionApprovalRequestCollectionTest<TransactionPendingAllocationApprovalRequestCollection, TransactionPendingAllocationApprovalRequest>
	{
	}
}
