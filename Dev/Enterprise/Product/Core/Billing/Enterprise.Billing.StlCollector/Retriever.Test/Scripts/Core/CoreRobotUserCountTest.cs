using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreRobotUserCount))]
	sealed class CoreRobotUserCountTest : CoreActiveStaffTest
	{
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "@#F");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "@#F", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "staff.015", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "Staff015", transaction1.Reference3);

			var transaction2 = FindRowByRef1(transactions, "@#G");
			AssertEquals("[T1] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, transaction2.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "@#G", transaction2.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T1] TransactionReference02", "staff.016", transaction2.Reference2);
			AssertEquals("[T1] TransactionReference03", "Staff016", transaction2.Reference3);
		}
	}
}
