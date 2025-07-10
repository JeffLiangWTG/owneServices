using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(AccRawPLBSExcludingAlternates))]
	class AccRawPLBSExcludingAlternatesTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccRawPLBSExcludingAlternates()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.00");
			Helper.InsertGLAccount(2, "2010.00.10", "BSH");
			Helper.InsertBASAccount(1);
			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAggregate(13, 202002, 1, "", 13);
			Helper.InsertGLAggregate(15, 202004, 2, "", 15);
			Helper.InsertGLAggregate(17, 202006, 2, "", 17);
			
			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccRawPLBSExcludingAlternates(202001, 202002, 202003, 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 25m, result.Rows[0]["Amount"]);

			sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccRawPLBSExcludingAlternates(202003, 202005, 202005, 1)";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have two row", 2, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 2L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 15m, result.Rows[0]["Amount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[1]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 0m, result.Rows[1]["Amount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
