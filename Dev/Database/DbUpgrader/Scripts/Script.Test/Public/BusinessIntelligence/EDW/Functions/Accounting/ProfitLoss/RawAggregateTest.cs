using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitLoss;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.ProfitLoss
{
	[TestedType(typeof(RawAggregate))]
	class RawAggregateTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestRawAggregate()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));

			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAggregate(12, 202001, 1, "", gLAmountLocalBalance: 12);
			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].RawAggregate(202001, 202002, 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 12m, result.Rows[0]["CurrentAmount"]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 02, 01));

			Helper.InsertBASGLAggregate(10, 202001, 1);
			Helper.InsertGLAggregate(12, 202003, 1, "", gLAmountLocalBalance: 12);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].RawAggregate(202001, 202003, 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 22m, result.Rows[0]["CurrentAmount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

