using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	internal sealed class ShippingOutstandingInvoiceAndCreditNoteDetailsIntegrationTest : TransactionedTestCase
	{
		[TestDate(2020, 01, 01)]
		public void TestShippingOutstandingInvoiceAndCreditNoteDetails_TotalLocal()
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

			var sqlQuery = $"select * from ShippingOutstandingInvoiceAndCreditNoteDetails('{companyPK}', 'AR', '2020-02-01', null, null, null, null, null, null, 'ALL', null, null, null, null, null, null, null, GETDATE()) order by TransactionNum";
			var command = Db.Connection.Command(sqlQuery);

			var result = DataUtils.GetDataTableFromCommand(command);

			AssertEquals("2 rows returned", 2, result.Rows.Count);
			AssertEquals(1110M, result.Rows[0]["TotalLocal"]);
			AssertEquals(2220M, result.Rows[1]["TotalLocal"]);
		}
	}
}

