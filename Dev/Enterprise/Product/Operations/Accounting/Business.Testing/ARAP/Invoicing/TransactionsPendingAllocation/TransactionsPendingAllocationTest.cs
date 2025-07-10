using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionsPendingAllocation))]
	public class TransactionsPendingAllocationTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TransactionsPendingAllocation(Factory);
		}

		public void TestTotals()
		{
			TransactionsPendingAllocation bizo = new TransactionsPendingAllocation(Factory);
			TransactionPendingAllocation transaction1 = bizo.Transactions.AddNew();
			transaction1.AH_OSExTaxAmount = 100m;
			AssertEquals(100m, bizo.BatchLocalTotal);
			TransactionPendingAllocation transaction2 = bizo.Transactions.AddNew();
			transaction2.AH_OSExTaxAmount = 200m;
			AssertEquals(300m, bizo.BatchLocalTotal);
		}
	}
}
