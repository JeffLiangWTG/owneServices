using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationCollection))]
	public class TransactionPendingAllocationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			TransactionsPendingAllocation pendingAllocation = new TransactionsPendingAllocation(Factory);
			return new TransactionPendingAllocationCollection(pendingAllocation, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<TransactionPendingAllocation>();
		}
	}
}
