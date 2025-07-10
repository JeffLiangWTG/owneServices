using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(Report_ProfitAndLossByDepartments))]
	class Report_ProfitAndLossByDepartmentsTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestReport_ProfitAndLossByDepartments()
		{
			PrepareData();

			Helper.InsertPeriodForInputYear(2009);
			Helper.InsertStmData(new DateTime(2009, 03, 01));

			Helper.InsertGLAggregate(100, 201001, 1, gLAmountLocalBalance: 100);
			Helper.InsertGLAggregate(200, 201002, 2, gLAmountLocalBalance: 200);
			Helper.InsertGLAggregate(300, 200901, 3, gLAmountLocalBalance: 300);
			Helper.InsertGLAggregate(400, 200912, 4, gLAmountLocalBalance: 400);
			Helper.InsertGLAggregate(500, 201009, 5, gLAmountLocalBalance: 500);
			Helper.InsertGLAggregate(600, 201009, 6, gLAmountLocalBalance: 600);
			Helper.InsertGLAggregate(700, 201009, 7, gLAmountLocalBalance: 700);
			Helper.InsertGLAggregate(800, 201009, 8, gLAmountLocalBalance: 800);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.Report_ProfitAndLossByDepartments (201009, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'CBH', '')");
			AssertEquals("Row count == 1", 1, result.Rows.Count);
			AssertEquals("The YearToPeriodRevenue of row 1", -800m, result.Rows[0]["YearToPeriodRevenue"]);
			AssertEquals("The YearToPeriodCost of row 1", -600m, result.Rows[0]["YearToPeriodCost"]);
			AssertEquals("The CurrentPeriodRevenue of row 1", -600m, result.Rows[0]["CurrentPeriodRevenue"]);
			AssertEquals("The CurrentPeriodCost of row 1", -500m, result.Rows[0]["CurrentPeriodCost"]);
		}

		public void TestCanGetAllAggregateData()
		{
			PrepareData();

			Helper.InsertBASGLAggregate(100, 201001, 1);
			Helper.InsertBASGLAggregate(300, 201002, 2);

			Helper.InsertPeriodForInputYear(2010);
			Helper.InsertStmData(new DateTime(2010, 03, 01));

			Helper.InsertGLAggregate(500, 201009, 5, gLAmountLocalBalance: 500);
			Helper.InsertGLAggregate(600, 201009, 6, gLAmountLocalBalance: 600);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.Report_ProfitAndLossByDepartments (201009, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', 'CBH', '')");
			AssertEquals("Row count == 1", 1, result.Rows.Count);
			AssertEquals("The YearToPeriodRevenue of row 1", -900m, result.Rows[0]["YearToPeriodRevenue"]);
			AssertEquals("The YearToPeriodCost of row 1", -600m, result.Rows[0]["YearToPeriodCost"]);
			AssertEquals("The CurrentPeriodRevenue of row 1", -600m, result.Rows[0]["CurrentPeriodRevenue"]);
			AssertEquals("The CurrentPeriodCost of row 1", -500m, result.Rows[0]["CurrentPeriodCost"]);
		}

		void PrepareData()
		{
			Helper.InsertGLAccount(1, "1000.00.00");
			Helper.InsertGLAccount(2, "2000.00.00", "P&L", "CR");
			Helper.InsertGLAccount(3, "3010.00.00", "HDL");
			Helper.InsertGLAccount(4, "4010.00.00", "HDL");
			Helper.InsertGLAccount(5, "5000.00.00");
			Helper.InsertGLAccount(6, "6000.00.00", "P&L", "CR");
			Helper.InsertGLAccount(7, "3010.00.00", "HDL");
			Helper.InsertGLAccount(8, "4010.00.00", "HDL");
			Helper.InsertCompanyBranchAndDepartment();
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

