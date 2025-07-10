using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreNormalUserCount))]
	sealed class CoreNormalUserCountTest : CoreActiveStaffTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "@#1");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "@#1", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "staff.001", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "Staff001", transaction1.Reference3);

			var transaction2 = FindRowByRef1(transactions, "@#4");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", RangeMonthAsDate, transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "@#4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "staff.004", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "Staff004", transaction2.Reference3);

			var transaction3 = FindRowByRef1(transactions, "@#9");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", RangeMonthAsDate, transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "@#9", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference02", "staff.009", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", "Staff009", transaction3.Reference3);

			var transaction4 = FindRowByRef1(transactions, "@#A");
			AssertEquals("[T4] CompanyCode", "DEM", transaction4.GetCompanyCode());
			AssertEquals("[T4] BranchCode", "DEM", transaction4.GetBranchCode());
			AssertEquals("[T4] TransactionDateUtc", RangeMonthAsDate, transaction4.ServiceOccuredUTC);
			AssertEquals("[T4] UserCode", "@#A", transaction4.ClientStaffCode);
			AssertEquals("[T4] ItemCount", 1, transaction4.BillableCount);
			AssertEquals("[T4] TransactionReference02", "staff.010", transaction4.Reference2);
			AssertEquals("[T4] TransactionReference03", "Staff010", transaction4.Reference3);

			var transaction5 = FindRowByRef1(transactions, "@#C");
			AssertEquals("[T5] CompanyCode", "DEM", transaction5.GetCompanyCode());
			AssertEquals("[T5] BranchCode", "DEM", transaction5.GetBranchCode());
			AssertEquals("[T5] TransactionDateUtc", RangeMonthAsDate, transaction5.ServiceOccuredUTC);
			AssertEquals("[T5] UserCode", "@#C", transaction5.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction5.BillableCount);
			AssertEquals("[T5] TransactionReference02", "staff.012", transaction5.Reference2);
			AssertEquals("[T5] TransactionReference03", "Staff012", transaction5.Reference3);

			var transaction6 = FindRowByRef1(transactions, "@#F");
			AssertEquals("[T5] CompanyCode", "DEM", transaction6.GetCompanyCode());
			AssertEquals("[T5] BranchCode", "DEM", transaction6.GetBranchCode());
			AssertEquals("[T5] TransactionDateUtc", RangeMonthAsDate, transaction6.ServiceOccuredUTC);
			AssertEquals("[T5] UserCode", "@#F", transaction6.ClientStaffCode);
			AssertEquals("[T5] ItemCount", 1, transaction6.BillableCount);
			AssertEquals("[T5] TransactionReference02", "staff.015", transaction6.Reference2);
			AssertEquals("[T5] TransactionReference03", "Staff015", transaction6.Reference3);
		}
	}
}
