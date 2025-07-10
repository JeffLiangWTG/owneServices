using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreHumanResourceUserCountDaily))]
	sealed class CoreHumanResourceUserCountDailyTest : CoreActiveStaffDailyTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transactionsForB = FindRowsByRef1(transactions, "@#B");
			var expectedCountForB = DaysInMonth;
			AssertEquals("Number of Transactions for B", expectedCountForB, transactionsForB.Count());
			for (var i = 0; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForB, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#B", "Staff011", "staff.011");
			}

			var transactionsForC = FindRowsByRef1(transactions, "@#C");
			var expectedCountForC = 12;
			AssertEquals("Number of Transactions for C", expectedCountForC, transactionsForC.Count());
			for (var i = 0; i < 12; ++i)
			{
				var transaction = FindRowByOccured(transactionsForC, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#C", "Staff012", "staff.012");
			}

			var transactionsForD = FindRowsByRef1(transactions, "@#D");
			var expectedCountForD = 13;
			AssertEquals("Number of Transactions for D", expectedCountForD, transactionsForD.Count());
			for (var i = 0; i < 13; ++i)
			{
				var transaction = FindRowByOccured(transactionsForD, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#D", "Staff013", "staff.013");
			}

			AssertEquals("Number of Transactions", expectedCountForB + expectedCountForC + expectedCountForD, transactions.Count());
		}
	}
}
