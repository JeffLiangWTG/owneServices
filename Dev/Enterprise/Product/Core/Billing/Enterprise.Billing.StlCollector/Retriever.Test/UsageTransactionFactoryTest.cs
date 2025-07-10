using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Billing.StlCollector.Retriever.Scripts;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	public class UsageTransactionFactoryTest : TestCaseWithFactory
	{
		public void TestUsageTransactionsWithItemCountZero()
		{
			AssertUsageTransactionNotCreated(itemCountOverride: 0);
		}

		public void TestUsageTransactionsWithItemCountNegative()
		{
			AssertUsageTransactionNotCreated(itemCountOverride: -15);
		}

		public void AssertUsageTransactionNotCreated(int itemCountOverride)
		{
			var script = new TestTransactionalStlItem(itemCountOverride, submitAsBillable: false);
			var range = AusydMonthRange.New(2024, 12);

			var createdTransactions = script.Run(range);
			AssertEquals($"Transactions reported even though item count was {itemCountOverride}", 0, createdTransactions.Count());
		}
	}
}
