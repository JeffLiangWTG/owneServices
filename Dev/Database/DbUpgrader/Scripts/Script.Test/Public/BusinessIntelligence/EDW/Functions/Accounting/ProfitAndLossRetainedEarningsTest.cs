using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(ProfitAndLossRetainedEarnings))]
	class ProfitAndLossRetainedEarningsTest : BiCreateScriptTest
	{
		public void TestAmountWhenGLAccountIsCR()
		{
			PrepareTestData("CR");
			AssertAmount(-200m, -1200m);
		}

		public void TestAmountWhenGLAccountIsDR()
		{
			PrepareTestData("DR");
			AssertAmount(200m, 1200m);
		}

		void AssertAmount(decimal currentPeriod, decimal yearToPeriod)
		{
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $@"SELECT * FROM {ScriptDbName}.dbo.ProfitAndLossRetainedEarnings (201009, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '', '')");
			AssertEquals("Retained Earnings This Period", currentPeriod, result.Rows[0]["CurrentPeriod"]);
			AssertEquals("Retained Earnings Year To Date", yearToPeriod, result.Rows[0]["YearToPeriod"]);
		}

		void PrepareTestData(string debitCreditCode)
		{
			Helper.InsertPeriodForInputYear(2009);
			Helper.InsertStmData(new DateTime(2009, 01, 01));
			Helper.InsertGLAccount(1, "4900.00.00", "BSH", debitCreditCode);
			Helper.InsertGLAccount(2, "2010.00.00");
			Helper.InsertCompanyBranchAndDepartment();
			Helper.InsertBASAccount(1L);
			Helper.InsertGLAggregate(100, 201008, 1);
			Helper.InsertGLAggregate(200, 201009, 1);
			Helper.InsertGLAggregate(300, 200910, 1);
			Helper.InsertGLAggregate(400, 201008, 2);
			Helper.InsertGLAggregate(500, 201009, 2);
			Helper.InsertGLAggregate(600, 200910, 2);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		PrepareDataHelper Helper => helper ?? (helper = new PrepareDataHelper(TestConnection, ScriptDbName));
		PrepareDataHelper helper;
	}
}

