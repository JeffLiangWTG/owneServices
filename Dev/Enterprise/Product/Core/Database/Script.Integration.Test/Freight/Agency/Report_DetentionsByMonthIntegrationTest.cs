using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	internal class Report_DetentionsByMonthIntegrationTest : TransactionedTestCase
	{
		[TestDate(2020, 01, 01)]
		public void TestReport_DetentionsByMonth_PostedInvoiceWithTaxes()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = TestDbHelper.BranchBrnPK;
			var departmentPK = TestDbHelper.DepartmentBrnPK;
			var currencyCode = CurrencyCodes.Australia;

			var helper = new TestDbHelper(TestConnection);
			var orgPK = helper.InsertOrgHeader("ORG1", "Test Org1");
			var containerPK = helper.InsertJobContainerDetention("CD01", companyPK, orgPK, orgPK);
			var jobPK = helper.InsertJob("S001001", companyPK, branchPK, departmentPK, "NC", containerPK, "WRK", ZDate.Today.ToDateTime());
			helper.InsertTransactionHeader(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "1001", 1000m, ZDateTime.Today.ToDateTime(), branchPK, departmentPK, companyPK: companyPK, org: orgPK, outstandingAmount: 1100m, gstAmount: 100m, currency: currencyCode, localOtherTaxAmount: 10m, job: jobPK);
			helper.InsertTransactionHeader(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "1002", 2000m, ZDateTime.Today.ToDateTime(), branchPK, departmentPK, companyPK: companyPK, org: orgPK, outstandingAmount: 2200m, gstAmount: 200m, currency: currencyCode, localOtherTaxAmount: 20m, job: jobPK);

			var sqlQuery = $"select * from Report_DetentionsByMonth('{companyPK}', null, null, 'ALL', 'ALL', null, null, null, null)";
			var command = Db.Connection.Command(sqlQuery);

			var result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("1 row returned", 1, result.Rows.Count);
			var row = result.Rows[0];

			AssertEquals("CD01", row["JobNumber"]);
			AssertEquals(3000M, row["InvoicedAmount"]);
			AssertEquals(300M, row["InvoicedTax"]);
			AssertEquals(3330M, row["TotalInvoiced"]);
			AssertEquals(3000M, row["TotalAmount"]);
			AssertEquals(3300M, row["TotalOutstanding"]);
		}
	}
}
