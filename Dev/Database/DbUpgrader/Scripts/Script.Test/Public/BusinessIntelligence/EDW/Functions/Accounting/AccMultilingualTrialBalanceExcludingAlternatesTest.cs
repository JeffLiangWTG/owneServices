using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(AccMultilingualTrialBalanceExcludingAlternates))]
	class AccMultilingualTrialBalanceExcludingAlternatesTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccMultilingualTrialBalanceExcludingAlternates()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.20");
			Helper.InsertGLAccount(2, "2010.00.30", "BSH");
			Helper.InsertGLAccount(3, "2010.00.10", "NTE");
			Helper.InsertGLAccount(4, "2010.00.40", "CLN", consolidationAccountKey: 3);
			Helper.InsertBASAccount(1);
			Helper.InsertBASAccount(2, "GL_BS_ACCOUNT_START");
			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAggregate(13, 202002, 1, "", 13);
			Helper.InsertGLAggregate(21, 202001, 2, "", 21);
			Helper.InsertGLAggregate(22, 202002, 2, "", 22);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccMultilingualTrialBalanceExcludingAlternates(202001, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have three row", 3, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData row 1 GLAccountKey", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 1 AccountNum", DBNull.Value, result.Rows[0]["AccountNum"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 1 CurrentAmount", 12m, result.Rows[0]["CurrentAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 1 ClosingAmount", 12m, result.Rows[0]["ClosingAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 2 GLAccountKey", 2L, result.Rows[1]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 2 AccountNum", DBNull.Value, result.Rows[1]["AccountNum"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 2 CurrentAmount", 21m, result.Rows[1]["CurrentAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 2 ClosingAmount", 21m, result.Rows[1]["ClosingAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 3 GLAccountKey", 3L, result.Rows[2]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 3 AccountNum", "2010.00.10", result.Rows[2]["AccountNum"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 3 CurrentAmount", DBNull.Value, result.Rows[2]["CurrentAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 3 ClosingAmount", DBNull.Value, result.Rows[2]["ClosingAmount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
