using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocation))]
	public class TransactionPendingAllocationMatchingTest : InvoicingBaseMatchingTest
	{
		protected override InvoicingBase GetNewInvoice()
		{
			return Factory.New<TransactionPendingAllocation>();
		}
	}
}
