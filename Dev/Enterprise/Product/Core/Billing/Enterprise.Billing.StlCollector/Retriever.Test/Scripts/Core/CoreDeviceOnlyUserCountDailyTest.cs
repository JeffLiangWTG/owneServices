using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreDeviceOnlyUserCountDaily))]
	sealed class CoreDeviceOnlyUserCountDailyTest : CoreActiveStaffDailyTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transactionsForA = FindRowsByRef1(transactions, "@#A");
			var expectedCountForA = DaysInMonth - 9;
			AssertEquals("Number of Transactions for A", expectedCountForA, transactionsForA.Count());
			for (var i = 9; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForA, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#A", "Staff010", "staff.010");
			}

			var transactionsForD = FindRowsByRef1(transactions, "@#D");
			var expectedCountForD = DaysInMonth - 12;
			AssertEquals("Number of Transactions for D", expectedCountForD, transactionsForD.Count());
			for (var i = 12; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForD, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#D", "Staff013", "staff.013");
			}

			var transactionsForE = FindRowsByRef1(transactions, "@#E");
			var expectedCountForE = DaysInMonth;
			AssertEquals("Number of Transactions for E", expectedCountForE, transactionsForE.Count());
			for (var i = 0; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForE, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#E", "Staff014", "staff.014");
			}

			AssertEquals("Number of Transactions", expectedCountForA + expectedCountForD + expectedCountForE, transactions.Count());
		}
	}
}
