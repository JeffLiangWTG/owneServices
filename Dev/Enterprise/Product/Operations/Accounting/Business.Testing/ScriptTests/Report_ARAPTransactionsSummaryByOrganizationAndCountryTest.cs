using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ARAPTransactionsSummaryByOrganizationAndCountryTest : ScriptTest
	{
		public void TestSimpleRun()
		{
			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1m);
			invoice1.AH_OH = TestObjectCreator.AALSHI.PK;
			invoice1.AH_PostDate = ZDateTime.Today;

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(APInvoice), "002", TestObjectCreator.AUD, 1m);
			invoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice2.AH_PostDate = ZDateTime.Today;

			Factory.Save();

			var headers = new[] { "OrganizationCode", "OrganizationName", "MainUNLOCO", "Address", "City", "State", "CountryName", "CountryCode", "Ledger", "TotalAmount", "AmountExcludingTax", "TaxAmount" };

			var result = RunScript(GlbCompany.CurrentCompany.PK, ZDateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"), ZDateTime.Today.AddDays(1).ToString("yyyy-MM-dd"), "AR&AP", "INV", "", "", "", "", "");

			var lines = new[]
			{
				new object[] { "AALSHI      ", "A.A.L. SHIPPING AGENCIES P/L", "AUBNE", "PO BOX 10446, ADELAIDE ST, BRISBANE  QLD",
					"", "", "Australia", "AU", "AR", 0m, 0m, 0m },
				new object[] { "ABIGAS      ", "ABI GAS & TOOLS", "AUBNE", "171 ABBOTSFORD ROAD, MAYNE, QLD",
					"", "", "Australia", "AU", "AP", 0m, 0m, 0m }
			};

			AssertDataTableAllRowsByKeyColumns("AccountsPayable", result, headers, lines);
		}

		DataTable RunScript(ZGuid companyPK, string startDate, string endDate, string ledger, string transactionTypeList, string accHeaderBranchList,
			string countryList, string exCountryList, string orgList, string consolidationCategory)
		{
			var sql = string.Format(@"
SELECT * FROM Report_ARAPTransactionsSummaryByOrganizationAndCountry(
	'{0}',				-- CompanyPK
	NULL,				-- Period
	'{1}',				-- StartDate
	'{2}',				-- EndDate
	'{3}',				-- Ledger
	'{4}',				-- TransactionTypeList
	'{5}',				-- AccHeaderBranchList
	'{6}',				-- IncludeCountryList
	'{7}',				-- ExcludeCountryList
	'{8}',				-- OrgList
	'{9}'				-- ConsolidatedCategory
)",
							companyPK,												//@OB_GC
							startDate,												//@AH_PostDate
							endDate,												//@AH_PostDate
							ledger,													//@AH_Ledger
							transactionTypeList,									//@AH_TransactionType
							accHeaderBranchList,									//@GB_Code
							countryList,											//@RN_CODE
							exCountryList,											//@RN_CODE
							orgList,												//@OH_Code
							consolidationCategory									//@OB_ARConsolidatedAccountingCategory
	);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
