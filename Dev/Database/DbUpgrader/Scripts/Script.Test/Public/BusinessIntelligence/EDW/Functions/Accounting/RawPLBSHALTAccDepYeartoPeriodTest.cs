using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(RawPLBSHALTAccDepYeartoPeriod))]
	class RawPLBSHALTAccDepYeartoPeriodTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestRawPLBSHALTAccDepYeartoPeriod()
		{
			PrepareData();

			Helper.InsertGLAggregate(100, 201001, 1);
			Helper.InsertGLAggregate(200, 201009, 2);
			Helper.InsertGLAggregate(300, 200901, 3);
			Helper.InsertGLAggregate(400, 200912, 4);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.RawPLBSHALTAccDepYeartoPeriod (201009, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'CBH', '', '')");
			AssertEquals("Row count == 2", 2, result.Rows.Count);
			AssertEquals("The GLAccountKey of row 1", 1L, result.Rows[0]["GLAccountKey"]);
			AssertEquals("The AmountYearToPeriodRevenue of row 1", 0m, result.Rows[0]["AmountYearToPeriodRevenue"]);
			AssertEquals("The AmountYearToPeriodCost of row 1", -100m, result.Rows[0]["AmountYearToPeriodCost"]);
			AssertEquals("The GLAccountKey of row 2", 2L, result.Rows[1]["GLAccountKey"]);
			AssertEquals("The AmountYearToPeriodRevenue of row 2", -200m, result.Rows[1]["AmountYearToPeriodRevenue"]);
			AssertEquals("The AmountYearToPeriodCost of row 2", 0m, result.Rows[1]["AmountYearToPeriodCost"]);
		}

		void PrepareData()
		{
			Helper.InsertGLAccount(1, "1000.00.00");
			Helper.InsertGLAccount(2, "2000.00.00", "P&L", "CR");
			Helper.InsertGLAccount(3, "3010.00.00", "HDL");
			Helper.InsertGLAccount(4, "4010.00.00", "HDL");
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertPeriodForInputYear(2009);
			Helper.InsertStmData(new DateTime(2009, 01, 01));
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

