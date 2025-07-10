using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitLossWithDepartment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.ProfitLossWithDepartment
{
	[TestedType(typeof(RetainedEarningsAcc))]
	class RetainedEarningsAccTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestQuery()
		{
			Helper.PrepareTestData();

			var result = Exec(202301, "SYN", "AU1", 202312, 1);

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals("Result should be -1", -1m, result.Rows[0][0]);
		}

		public void TestCanGetAllAggregateData()
		{
			Helper.PrepareTestData();

			Helper.InsertBASGLAggregate(-2m, 202201, 1);

			var result = Exec(202201, "SYN", "AU2", 202312, 1);

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals("data should be null", true, result.Rows[0][0] is System.DBNull);

			result = Exec(202201, "SYN", "AU1", 202312, 1);

			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals(-3m, result.Rows[0][0]);
		}

		DataTable Exec(int bshStartPeriod, string branchCodes, string departmentCodes, int plStartPeriod, int companyKey)
		{
			var sql = $@"Select [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]({bshStartPeriod}, '{branchCodes}', '{departmentCodes}', {plStartPeriod}, {companyKey})";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		CreateEDWDataHelper Helper => helper ?? (helper = new CreateEDWDataHelper(TestConnection, ScriptDbName));
		CreateEDWDataHelper helper;
	}
}
