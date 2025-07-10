using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(ChinaStatementOfShareholdersEquity))]
	internal class ChinaStatementOfShareholdersEquityEdwTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumber()
		{
			SetUpDataForTestTransactionDescPaymentReferenceNumber();
			var result = CallStoredProcedure();
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("Should be 119 columns in result", 119, result.Columns.Count);
			AssertEquals("B03_Credit should be 3000.0000", 3000m, result.Rows[0]["B03_Credit"]);
		}

		void SetUpDataForTestTransactionDescPaymentReferenceNumber()
		{
			var helper = new PrepareDataHelper(TestConnection, ScriptDbName);
			helper.InsertCompanyBranchAndDepartment();
			helper.InsertPeriodForInputYear(2015);
			helper.InsertGLAccount(1,"3510.00.00");
			helper.InsertAccountDescriptor(language: "ZH-CN", countryOfCompliance: "CN", reportType: "SSE", reportCategory: "B03");
			helper.InsertAccountDescriptorPivot();
			helper.InsertCUSGeneralLedgerTransactionData("2015-04-24", "2015-04-24", 3000, 2015);
			helper.InsertStmData(new DateTime(2014,12,1));
		}

		DataTable CallStoredProcedure()
		{
			var sqlText = $@"EXEC [{ScriptDbName}].[dbo].ChinaStatementOfShareholdersEquity
							@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
							@EndPeriod = 201505,
							@Branch = NULL";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			return result;
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}
