using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ItalyARStampDutySummaryTest : ScriptTest
	{
		[TestDate(2019, 03, 19)]
		public void TestItalyARStampDutySummary_PostBrexit()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var gbOrgHeader = TestObjectCreator.ABIGAS;
			gbOrgHeader.OH_RL_NKClosestPort = "GBLON";

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", TestObjectCreator.GBP);
			invoice1.AH_OH = gbOrgHeader.PK;
			invoice1.AH_PostDate = brexitDate.AddDays(-1);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), "00001000", TestObjectCreator.GBP);
			invoice2.AH_OH = gbOrgHeader.PK;
			invoice2.AH_PostDate = brexitDate;
			Factory.Save();

			var fromDate = invoice1.AH_PostDate.ToString("yyyy-MM-dd");
			var toDate = invoice2.AH_PostDate.AddDays(1).ToString("yyyy-MM-dd");
			var result = RunScript(fromDate, toDate, "ALL");
			var lines = new[] {
					new object[] { "INV", "OTHER EU" },
					new object[] { "CRD", "NON EU" }
				};
			AssertTransactionRegNoForBrexit(result, lines);
		}

		void AssertTransactionRegNoForBrexit(DataTable result, object[][] lines)
		{
			var headers = new[] { "TransactionType", "Location" };

			AssertDataTableAllRowsByKeyColumns("Report_ItalyARStampDutySummary", result, headers, lines);
		}

		DataTable RunScript(string startDate, string endDate, string location)
		{
			string sql = $@"select * from Report_ItalyARStampDutySummary (
null,											--@Period              INT, 
'{startDate}',									--@StartDate           SMALLDATETIME, 
'{endDate}',									--@EndDate             SMALLDATETIME, 
'{GlbCompany.CurrentCompany.PK.ToGuid()}',		--@CompanyPK           UNIQUEIDENTIFIER, 
null											--@BranchList          VARCHAR(4000)
)";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
