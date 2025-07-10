using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Core;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	[TestedType(typeof(CoreDeviceOnlyUserCount))]
	sealed class CoreDeviceOnlyUserCountTest : CoreActiveStaffTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "@#A");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", RangeMonthAsDate, transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "@#A", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "staff.010", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "Staff010", transaction1.Reference3);

			var transaction2 = FindRowByRef1(transactions, "@#D");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", "DEM", transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", RangeMonthAsDate, transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "@#D", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "staff.013", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "Staff013", transaction2.Reference3);

			var transaction3 = FindRowByRef1(transactions, "@#E");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", "DEM", transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", RangeMonthAsDate, transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "@#E", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference02", "staff.014", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", "Staff014", transaction3.Reference3);
		}
	}
}
