using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(HVLVItemStandardUsage))]
	sealed class HVLVItemStandardUsageTest : HVLVItemUsageTest
	{
		protected override void AssertResultSetCore(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			CombineAssertions(() =>
			{
				AssertRowContent(transactions, 0, "100", 2, 1, null);
				AssertRowContent(transactions, 1, "004", 2, 28, "GB1");
				AssertRowContent(transactions, 2, "008", 2, 28, "GB2");
				AssertRowContent(transactions, 3, "001", 2, 28, "GB1");
				AssertRowContent(transactions, 4, "006", 2, 28, "GB2");
				AssertRowContent(transactions, 5, "101", 2, 28, null);
			});
		}

		protected override void AddExtraTestItemsWithUsageTimes()
		{
			AddTestItemWithUsageTimesToInclude("NULL", "NULL", "'2019-02-01'", "NULL");
			AddTestItemWithUsageTimesToInclude("NULL", "NULL", "'2019-02-28'", "NULL");

			AddTestItemWithUsageTimesToExclude("NULL", "NULL", "'2019-02-15'", "NULL", true);
			AddTestItemWithUsageTimesToExclude("NULL", "NULL", "'2019-01-31'", "NULL");
			AddTestItemWithUsageTimesToExclude("NULL", "NULL", "'2019-03-01'", "NULL", true);
			AddTestItemWithUsageTimesToExclude("NULL", "NULL", "'2019-03-08'", "NULL");
		}
	}
}
