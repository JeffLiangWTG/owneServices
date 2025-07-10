using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ContainerLoadPlanLineUsage))]
	sealed class ContainerLoadPlanLineUsageTest : BaseOrderManagerContainerLoadUsageTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 1, transactions.Count());
				AssertRow(transactions.Single(t => t.Reference1 == "CLH02" && t.Reference2 == "JSL02" && t.Reference3 == "456"), "0", "TG1", "TD1", new DateTime(2014, 10, 1), "US1", 1, "CLH02", "JSL02", "456", null);
			});
		}
	}
}
