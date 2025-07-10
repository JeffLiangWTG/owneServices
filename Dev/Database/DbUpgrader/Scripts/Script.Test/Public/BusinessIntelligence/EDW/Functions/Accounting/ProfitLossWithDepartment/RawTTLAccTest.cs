using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ProfitLossWithDepartment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.ProfitLossWithDepartment
{
	[TestedType(typeof(RawTTLAcc))]
	class RawTTLAccTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		public void TestQuery()
		{
			Helper.PrepareTestData();
			Helper.CreateBASAccount("GL_PL_APPROPRIATION_ACCOUNT", 100);

			var result = Exec(202301, 202301, 202312, 1, "AU1", "SYN");

			AssertEquals("Result should have 2 row", 2, result.Rows.Count);
			AssertEquals(-1m, result.Select("GLAccountKey = 1")[0]["Amount"]);
			AssertEquals(-2m, result.Select("GLAccountKey = 2")[0]["Amount"]);
		}

		DataTable Exec(int bshStartPeriod, int plStartPeriod, int endPeriod, int companyKey, string departmentCodes, string branchCodes)
		{
			var sql = $@"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{ScriptToTest.Name}]({bshStartPeriod}, {plStartPeriod}, {endPeriod}, {companyKey}, '{departmentCodes}', '{branchCodes}')";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		CreateEDWDataHelper Helper => helper ?? (helper = new CreateEDWDataHelper(TestConnection, ScriptDbName));
		CreateEDWDataHelper helper;
	}
}
