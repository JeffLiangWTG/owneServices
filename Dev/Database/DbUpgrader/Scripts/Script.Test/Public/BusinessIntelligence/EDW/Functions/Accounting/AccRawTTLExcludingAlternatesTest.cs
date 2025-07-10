using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(AccRawTTLExcludingAlternates))]
	class AccRawTTLExcludingAlternatesTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestAccRawTTLExcludingAlternates()
		{
			Helper.InsertPeriodForInputYear(2020);
			Helper.InsertStmData(new DateTime(2020, 01, 01));
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.20");
			Helper.InsertGLAccount(2, "2010.00.30", "TTL");
			Helper.InsertGLAccount(3, "2010.00.10", totalLevel: 1);
			Helper.InsertBASAccount(1);
			Helper.InsertGLAggregate(12, 202001, 1, "", 12);
			Helper.InsertGLAggregate(13, 202002, 1, "", 13);
			Helper.InsertGLAggregate(15, 202004, 2, "", 15);
			Helper.InsertGLAggregate(17, 202006, 2, "", 17);

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].AccRawTTLExcludingAlternates(202001, 202002, 202003, 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			AssertEquals("Result should have one row", 1, result.Rows.Count);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("GRP__GeneralLedgerAggregateData data", 25m, result.Rows[0]["Amount"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}
