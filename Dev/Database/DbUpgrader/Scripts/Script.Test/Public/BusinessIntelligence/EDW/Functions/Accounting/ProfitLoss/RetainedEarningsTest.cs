using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitLoss;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.ProfitLoss
{
	[TestedType(typeof(RetainedEarnings))]
	class RetainedEarningsTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestRetainedEarnings()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));

			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAccount(1, "2010.00.00");
			var sqlText = $"SELECT [{ScriptDbName}].[dbo].RetainedEarnings(202001, 202002, 1) as RetainedEarnings";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 12m, result.Rows[0]["RetainedEarnings"]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 02, 01));
			Helper.InsertGLAccount(1, "2010.00.00");

			Helper.InsertBASGLAggregate(10, 202001, 1);
			Helper.InsertGLAggregate(12, 202003, 1, "", gLAmountLocalBalance: 12);

			var sqlText = $"SELECT [{ScriptDbName}].[dbo].RetainedEarnings(202001, 202005, 1) as RetainedEarnings";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 22m, result.Rows[0]["RetainedEarnings"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
