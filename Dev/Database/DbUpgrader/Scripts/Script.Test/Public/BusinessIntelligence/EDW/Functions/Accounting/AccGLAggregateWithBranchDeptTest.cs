using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(AccGLAggregateWithBranchDept))]
	class AccGLAggregateWithBranchDeptTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccGLAggregateWithBranchDept()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.00");
			Helper.InsertGLAggregate(12, 202201, 1, gLAmountLocalBalance: 20m);
			Helper.InsertPeriodForInputYear(2021);
			Helper.InsertStmData(new DateTime(2021, 12, 01));

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccGLAggregateWithBranchDept(202201, 1,'AU1','CBH','{"{0}"}')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals(1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals(1L, result.Rows[0]["BranchKey"]);
			AssertEquals(1L, result.Rows[0]["DepartmentKey"]);
			AssertEquals(20m, result.Rows[0]["GLAmountLocalBalance"]);
			AssertEquals(202201, result.Rows[0]["PostPeriod"]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 02, 01));

			Helper.InsertBASGLAggregate(10, 202001, 1);
			Helper.InsertGLAggregate(12, 202003, 1, "", gLAmountLocalBalance: 12);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccGLAggregateWithBranchDept(202003, 1, 'AU1', 'CBH', '')";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 2, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 202003, result.Rows[0]["PostPeriod"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 12m, result.Rows[0]["GLAmountLocalBalance"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 202001, result.Rows[1]["PostPeriod"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 10m, result.Rows[1]["GLAmountLocalBalance"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

