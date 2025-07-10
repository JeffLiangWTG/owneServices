using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.Testing
{
	[TestedType(typeof(StatementAccountingReconReportSP))]
	class StatementAccountingReconReportSPTest : DbCreateScriptTest
	{
		public void TestQueryAllWithNullDatetime()
		{
			var stmtHeaderPK = TestDataCreator.CreateCusStatementHeader(companyPK, "TST00001", "B", 0, "10207", "");
			var stmtLinePK = TestDataCreator.CreateCusStatementLine(stmtHeaderPK, "EntryNum1", "AI", 100);

			var sql = $"EXEC StatementAccountingReconReportSP null, null, null, null, null, null, '', 'ALL', null, '{companyPK}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("Should have a row", stmtLinePK, result.Rows[0]["B3_PK"]);
		}

		Guid companyPK;

		protected override void SetUp()
		{
			base.SetUp();
			companyPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
		}
	}
}

