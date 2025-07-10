using System.Data;
using CargoWise.Data;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ItalyARStampDutyDetailWithoutSDLTest : ScriptTest
	{
		[TestDate(2019, 03, 19)]
		public void TestItalyARStampDutyDetail_PostBrexit()
		{
			var brexitDate = TestObjectCreator.SetupPostBrexitData();
			var gbOrgHeader = TestObjectCreator.ABIGAS;
			gbOrgHeader.OH_RL_NKClosestPort = "GBLON";

			var itOrgHeader = TestObjectCreator.Debtor;
			itOrgHeader.OH_RL_NKClosestPort = "ITROM";

			var invoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001000", TestObjectCreator.GBP);
			invoice1.AH_OH = gbOrgHeader.PK;
			invoice1.AH_PostDate = brexitDate.AddDays(-1);
			Factory.Save();

			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001001", TestObjectCreator.GBP);
			invoice2.AH_OH = gbOrgHeader.PK;
			invoice2.AH_PostDate = brexitDate;
			Factory.Save();

			var invoice3 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "00001002", TestObjectCreator.EUR);
			invoice3.AH_OH = itOrgHeader.PK;
			invoice3.AH_PostDate = brexitDate;
			Factory.Save();

			var fromDate = invoice1.AH_PostDate.ToString("yyyy-MM-dd");
			var toDate = invoice2.AH_PostDate.AddDays(1).ToString("yyyy-MM-dd");
			var result = RunScript(fromDate, toDate, "ALL");
			var lines = new[] {
					new object[] { "00001000", "OTHER EU" },
					new object[] { "00001001", "NON EU" },
					new object[] { "00001002", "ITALY" }
				};
			AssertTransactionRegNoForBrexit(result, lines);

			result = RunScript(fromDate, toDate, "OtherEU");
			lines = new[] {
					new object[] { "00001000", "OTHER EU" }
				};
			AssertTransactionRegNoForBrexit(result, lines);

			result = RunScript(fromDate, toDate, "Non-EU");
			lines = new[] {
					new object[] { "00001001", "NON EU" }
				};
			AssertTransactionRegNoForBrexit(result, lines);
		}

		void AssertTransactionRegNoForBrexit(DataTable result, object[][] lines)
		{
			var headers = new[] { "TransactionNum", "Location" };

			AssertDataTableAllRowsByKeyColumns("ItalyARStampDutyDetail", result, headers, lines);
		}

		DataTable RunScript(string startDate, string endDate, string location)
		{
			string sql = $@"select * from Report_ItalyARStampDutyDetailWithoutSDL (
null,											--@Period              INT, 
'{startDate}',									--@StartDate           SMALLDATETIME, 
'{endDate}',									--@EndDate             SMALLDATETIME, 
'{GlbCompany.CurrentCompany.PK.ToGuid()}',		--@CompanyPK           UNIQUEIDENTIFIER, 
null,											--@BranchList          VARCHAR(4000), 
'ALL',											--@TransactionTypeList VARCHAR(20),-- INV / CRD / ALL 
'{location}'									--@AROrgLocation       VARCHAR(8) -- ALL,Italy,Other EU,Non-EU
)";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}
