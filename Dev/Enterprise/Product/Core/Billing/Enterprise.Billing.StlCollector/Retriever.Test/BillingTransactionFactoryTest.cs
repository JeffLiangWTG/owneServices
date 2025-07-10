using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever.Scripts;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	public class BillingTransactionFactoryTest : TestCaseWithFactory
	{
		public void TestBillingTransactionsWithItemCountZero()
		{
			AssertBillingTransactionNotCreated(itemCountOverride: 0);
		}

		public void TestBillingTransactionsWithItemCountNegative()
		{
			AssertBillingTransactionNotCreated(itemCountOverride: -15);
		}

		public void AssertBillingTransactionNotCreated(int itemCountOverride)
		{
			var script = new TestTransactionalStlItem(itemCountOverride, submitAsBillable: true);
			var range = AusydMonthRange.New(2024, 12);

			var createdTransactions = script.Run(range);
			AssertEquals($"Transactions reported even though item count was {itemCountOverride}", 0, createdTransactions.Count());
		}
	}
}
