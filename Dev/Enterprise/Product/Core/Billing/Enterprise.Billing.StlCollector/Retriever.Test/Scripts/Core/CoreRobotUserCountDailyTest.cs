using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreRobotUserCountDaily))]
	sealed class CoreRobotUserCountDailyTest : CoreActiveStaffDailyTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transactionsForF = FindRowsByRef1(transactions, "@#F");
			var expectedCountForF = DaysInMonth - 9;
			AssertEquals("Number of Transactions for F", expectedCountForF, transactionsForF.Count());
			for (var i = 9; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForF, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#F", "Staff015", "staff.015");
			}

			var transactionsForG = FindRowsByRef1(transactions, "@#G");
			var expectedCountForG = DaysInMonth;
			AssertEquals("Number of Transactions for G", expectedCountForG, transactionsForG.Count());
			for (var i = 0; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForG, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#G", "Staff016", "staff.016");
			}

			AssertEquals("Number of Transactions", expectedCountForF + expectedCountForG, transactions.Count());
		}
	}
}
