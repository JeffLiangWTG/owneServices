using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Customs;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Customs
{
	[TestedType(typeof(CaptureUSLowValueByBillUsageForHVLV))]
	sealed class CaptureUSLowValueByBillUsageForHVLVTest : CaptureUSLowValueByBillUsageTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			CombineAssertions(() =>
			{
				AssertEquals("Number of Transactions", 1, transactions.Count());
				AssertRow(transactions.First(), "3", "USC", "USB", new DateTime(2020, 8, 12), "FFF", 1, "ULH03", "9012", "RRR", "HVL");
			});
		}
	}
}
