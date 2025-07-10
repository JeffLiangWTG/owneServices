using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.CashFlow
{
	[TestedType(typeof(CashAtBeginningOfPeriod))]
	class CashAtBeginningOfPeriodTest : BiCreateScriptTest
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

			var result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304);
			AssertEquals(DBNull.Value, result.Rows[0]["Value"]);

			helper.InsertStmData(new DateTime(2023, 05, 06));
			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304);
			AssertEquals(DBNull.Value, result.Rows[0]["Value"]);

			TestConnection.ExecuteNonQuery($"delete from [{ScriptDbName}].[Finance].BAS__StmDataDate");

			helper.InsertStmData(new DateTime(2022, 05, 06));
			result = Excute("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC", 202304);
			AssertEquals(0m, result.Rows[0]["Value"]);
		}

		protected DataTable Excute(string companyPK, int period, string branchID = "", char isYearToDate = 'N')
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].[dbo].CashAtBeginningOfPeriod({period}, '{companyPK}', '{branchID}', '{isYearToDate}')"));
		}
	}
}

