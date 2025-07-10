using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(ProfitAndLoss_Multilingual))]
	class ProfitAndLoss_MultilingualTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestQuery()
		{
			Helper.PrepareTestData();
			Helper.InsertBASGLAggregate(-1m, 202211, 1);
			Helper.CreateGRPGeneralLedgerAggregateData(1, 202301, 1m, 0);
			Helper.InsertBASGLAggregate(-2m, 202211, 2);
			Helper.CreateGRPGeneralLedgerAggregateData(2, 202301, 2m, 0);
			Helper.CreateBASAccount("GL_PL_APPROPRIATION_ACCOUNT", 100);

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 1, glAccountKey: 1, localAccountNumber: "6000.10.10", language: "ZH-CN", reportCategory: "P&L");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, glAccountKey: 2, localAccountNumber: "6000.10.20", language: "ZH-CN", reportCategory: "BSH");
			Helper.InsertAccountDescriptorPivot(glAccountDescriptorKey: 1, glAccountKey: 1);
			Helper.InsertAccountDescriptorPivot(glAccountDescriptorKey: 2, glAccountKey: 2);

			var result = Exec(202311, "0000.00.00", "9999.99.99", Helper.CurrentCompany, "", "", "ZH-CN", "", "N");

			AssertEquals("Result should have 2 row(s)", 2, result.Rows.Count);

			var row = result.Select("AccountNumber = '6000.10.10'")[0];
			AssertEquals(1m, row["CurrentPeriod"]);
			AssertEquals(2m, row["YearToPeriod"]);
			AssertEquals(1m, row["LastYearTotal"]);

			row = result.Select("AccountNumber = '6000.10.20'")[0];
			AssertEquals(2m, row["CurrentPeriod"]);
			AssertEquals(6m, row["YearToPeriod"]);
			AssertEquals(2m, row["LastYearTotal"]);
		}

		public void TestHasValue_WhenPeriodBeforeThanLastProcessedDate()
		{
			Helper.PrepareTestData();
			Helper.InsertBASGLAggregate(-1m, 202111, 1);
			Helper.InsertBASGLAggregate(-1m, 202201, 1);
			Helper.InsertBASGLAggregate(-2m, 202111, 2);
			Helper.InsertBASGLAggregate(-2m, 202201, 2);
			Helper.CreateBASAccount("GL_PL_APPROPRIATION_ACCOUNT", 100);

			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 1, glAccountKey: 1, localAccountNumber: "6000.10.10", language: "ZH-CN", reportCategory: "P&L");
			Helper.InsertAccountDescriptor(glAccountDescriptorKey: 2, glAccountKey: 2, localAccountNumber: "6000.10.20", language: "ZH-CN", reportCategory: "BSH");
			Helper.InsertAccountDescriptorPivot(glAccountDescriptorKey: 1, glAccountKey: 1);
			Helper.InsertAccountDescriptorPivot(glAccountDescriptorKey: 2, glAccountKey: 2);

			var result = Exec(202212, "0000.00.00", "9999.99.99", Helper.CurrentCompany, "", "", "ZH-CN", "", "N");

			AssertEquals("Result should have 2 row(s)", 2, result.Rows.Count);

			var row = result.Select("AccountNumber = '6000.10.10'")[0];
			AssertEquals(1m, row["YearToPeriod"]);
			AssertEquals(1m, row["LastYearTotal"]);

			row = result.Select("AccountNumber = '6000.10.20'")[0];
			AssertEquals(4m, row["YearToPeriod"]);
			AssertEquals(2m, row["LastYearTotal"]);
		}

		DataTable Exec(int period, string startAccount, string endAccount, Guid company, string branchCodes, string departmentCodes, string language, string countryCode, string inculdeZero)
		{
			var sql = $@"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]({period}, '{startAccount}', '{endAccount}', '{company}', '{departmentCodes}', '{branchCodes}', '{language}', '{countryCode}', '{inculdeZero}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		CreateEDWDataHelper Helper => helper ?? (helper = new CreateEDWDataHelper(TestConnection, ScriptDbName));
		CreateEDWDataHelper helper;
	}
}
