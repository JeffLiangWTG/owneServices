using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(BalanceSheetAndProfitAndLossBase))]
	class BalanceSheetAndProfitAndLossBaseTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestBalanceSheetAndProfitAndLossBase()
		{
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertGLAccount(1, "2010.00.00");
			Helper.InsertGLAggregate(12, 202201, 1, gLAmountLocalBalance: 20m);
			Helper.InsertPeriodForInputYear(2021);
			Helper.InsertStmData(new DateTime(2021, 12, 01));

			var sqlText = $"SELECT * FROM [{ScriptDbName}].[dbo].BalanceSheetAndProfitAndLossBase(202201, 1, 'AU1', 'CBH', 'Y', '', '', 'PNL', '', 'DAU', '', '', '0', '', '', '{"{0}"}', '')";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, "A00"));
			var rows = result.Select($"AggregateGLAccountKey = 1");
			AssertEquals("Result should have one row", 1, rows.Length);
			AssertEquals("2010.00.00", rows[0]["AccountNumber"]);
			AssertEquals("ACC1", rows[0]["AccountName"]);
			AssertEquals("P&L", rows[0]["AccountType"]);
			AssertEquals(-20m, rows[0]["CurrentPeriod"]);
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

