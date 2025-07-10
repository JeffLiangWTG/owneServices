using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CaptureUSLowValueByBillUsageForCustoms))]
	sealed class CaptureUSLowValueByBilUsageForCustomsTest : CaptureUSLowValueByBillUsageTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 2, transactions.Count());
				AssertRowMatchingRef1(transactions, "1", "USC", "USB", new DateTime(2020, 8, 10), "DDD", 1, "ULH01", "1234", "PPP", "AAA");
				AssertRowMatchingRef1(transactions, "2", "USC", "USB", new DateTime(2020, 8, 11), "EEE", 1, "ULH02", "5678", "QQQ", "BBB");
			});
		}
	}
}
