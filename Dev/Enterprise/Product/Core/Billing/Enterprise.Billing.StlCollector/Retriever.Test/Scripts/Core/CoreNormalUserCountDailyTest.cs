using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreNormalUserCountDaily))]
	sealed class CoreNormalUserCountDailyTest : CoreActiveStaffDailyTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			var transactionsFor1 = FindRowsByRef1(transactions, "@#1");
			var expectedCountFor1 = DaysInMonth;
			AssertEquals("Number of Transactions for 1", expectedCountFor1, transactionsFor1.Count());
			for (var i = 0; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsFor1, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#1", "Staff001", "staff.001");
			}

			var transactionsFor4 = FindRowsByRef1(transactions, "@#4");
			var expectedCountFor4 = DaysInMonth;
			AssertEquals("Number of Transactions for 4", expectedCountFor4, transactionsFor4.Count());
			for (var i = 0; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsFor4, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#4", "Staff004", "staff.004");
			}

			var transactionsFor9 = FindRowsByRef1(transactions, "@#9");
			var expectedCountFor9 = DaysInMonth;
			AssertEquals("Number of Transactions for 9", expectedCountFor9, transactionsFor9.Count());
			for (var i = 0; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsFor9, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#9", "Staff009", "staff.009");
			}

			var transactionsForA = FindRowsByRef1(transactions, "@#A");
			var expectedCountForA = 10;
			AssertEquals("Number of Transactions for A", expectedCountForA, transactionsForA.Count());
			for (var i = 0; i < 10; ++i)
			{
				var transaction = FindRowByOccured(transactionsForA, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#A", "Staff010", "staff.010");
			}

			var transactionsForC = FindRowsByRef1(transactions, "@#C");
			var expectedCountForC = DaysInMonth - 11;
			AssertEquals("Number of Transactions for C", expectedCountForC, transactionsForC.Count());
			for (var i = 11; i < DaysInMonth; ++i)
			{
				var transaction = FindRowByOccured(transactionsForC, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#C", "Staff012", "staff.012");
			}

			var transactionsForF = FindRowsByRef1(transactions, "@#F");
			var expectedCountForF = 10;
			AssertEquals("Number of Transactions for F", expectedCountForF, transactionsForF.Count());
			for (var i = 0; i < 10; ++i)
			{
				var transaction = FindRowByOccured(transactionsForF, RangeMonthAsDate.AddDays(i));
				AssertTransaction(transaction, "@#F", "Staff015", "staff.015");
			}

			AssertEquals("Number of Transactions", expectedCountFor1 + expectedCountFor4 + expectedCountFor9 + expectedCountForA + expectedCountForC + expectedCountForF, transactions.Count());
		}
	}
}
