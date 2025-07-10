using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Testing
{
	[TestedType(typeof(Report_CashFlowStatementChina))]
	class Report_CashFlowStatementChinaTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestLastProcessedDate()
		{
			var helper = new PrepareDataHelper(TestConnection, ScriptDbName);
			helper.InsertCompanyBranchAndDepartment();
			helper.InsertGLAccount(1, "3987");
			helper.InsertBASAccount(2);
			helper.InsertPeriodForInputYear(2023);
			helper.InsertGLAggregate(2, 202304, 1, "", 2);

			var branchSql = $"SELECT TOP 1 BranchID FROM [{ScriptDbName}].[Organization].[BAS__Branch]";
			var branchID = (Guid)TestConnection.ExecuteScalar(branchSql);

			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304, branchID);
			AssertEquals(20, result.Select("TotalAmount = 0").Length);

			helper.InsertStmData(new DateTime(2023, 02, 06));
			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202301, branchID);
			AssertEquals(20, result.Rows.Count);

			TestConnection.ExecuteNonQuery($"delete from [{ScriptDbName}].[Finance].BAS__StmDataDate");
			helper.InsertStmData(new DateTime(2022, 02, 06));
			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304, branchID);
			AssertEquals(20, result.Rows.Count);
		}

		protected DataTable Excute(string companyPK, int period, Guid branchID)
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].[dbo].Report_CashFlowStatementChina({period}, '{companyPK}', '{branchID}')"));
		}
	}
}

