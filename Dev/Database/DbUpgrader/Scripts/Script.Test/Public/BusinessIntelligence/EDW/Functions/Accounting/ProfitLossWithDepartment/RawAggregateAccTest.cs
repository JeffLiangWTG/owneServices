using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitLossWithDepartment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.ProfitLossWithDepartment
{
	[TestedType(typeof(RawAggregateAcc))]
	class RawAggregateAccTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestQuery()
		{
			Helper.PrepareTestData();

			var result = Exec(202301, "SYN", "AU1", 202312, 1);

			AssertEquals("Result should have 2 row(s)", 2, result.Rows.Count);
			AssertEquals(-1m, result.Select("AG = 1")[0]["CurrentAmount"]);
			AssertEquals(-2m, result.Select("AG = 2")[0]["CurrentAmount"]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.PrepareTestData();

			Helper.InsertBASGLAggregate(-2m, 202201, 1);

			var result = Exec(202201, "SYN", "AU1", 202312, 1);

			AssertEquals("Result should have 2 row(s)", 2, result.Rows.Count);
			AssertEquals(-3m, result.Select("AG = 1")[0]["CurrentAmount"]);
			AssertEquals(-2m, result.Select("AG = 2")[0]["CurrentAmount"]);
		}

		DataTable Exec(int startPeriod, string branchCodes, string departmentCodes, int endPeriod, int companyKey)
		{
			var sql = $@"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]({startPeriod}, '{branchCodes}', {endPeriod}, {companyKey}, '{departmentCodes}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		CreateEDWDataHelper Helper => helper ?? (helper = new CreateEDWDataHelper(TestConnection, ScriptDbName));
		CreateEDWDataHelper helper;
	}
}
