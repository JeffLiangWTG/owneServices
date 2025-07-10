using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(AccPLBSExcludingAlternates))]
	class AccPLBSExcludingAlternatesTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccPLBSExcludingAlternates()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.20");
			Helper.InsertGLAccount(2, "2010.00.30", "TTL");
			Helper.InsertGLAccount(3, "2010.00.10", "TTL", totalLevel: 1);
			Helper.InsertGLAccount(4, "2010.00.40", "CLN", consolidationAccountKey: 3);
			Helper.InsertBASAccount(1);
			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAggregate(13, 202002, 1, "", 13);
			Helper.InsertGLAggregate(21, 202001, 4, "", 21);
			Helper.InsertGLAggregate(22, 202002, 4, "", 22);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccPLBSExcludingAlternates(202001, 202002, 202003, 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have two row", 2, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData row 0 GLAccountKey", 4L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 0 CurrentAmount", DBNull.Value, result.Rows[0]["CurrentAmount"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 1 GLAccountKey", 1L, result.Rows[1]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData row 1 CurrentAmount", 25m, result.Rows[1]["CurrentAmount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
