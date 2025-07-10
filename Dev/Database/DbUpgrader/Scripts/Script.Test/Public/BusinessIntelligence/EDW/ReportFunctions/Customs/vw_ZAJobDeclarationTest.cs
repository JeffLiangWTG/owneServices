using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.ReportFunctions.Customs
{
	[TestedType(typeof(vw_ZAJobDeclaration))]
	sealed class vw_ZAJobDeclarationTest : BiCreateScriptTest
	{
		public void TestView()
		{
			EDWTestDataCreator.CreateDeclaration("US", "AAA", "BBB", "HB1", "JOB1", "MB1", 10, 26, 8, "AGTCode=ABC");
			var (_, declarationKey) = EDWTestDataCreator.CreateDeclaration("ZA", "APC", "SYD", "HB1", "JOB1", "MB1", 10, 26, 8, "AGTCode=ABC");
			var sql = $"SELECT * FROM [{ScriptDbName}].[dbo].[vw_ZAJobDeclaration]";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("ABC", result.Rows[0]["AGT Code"]);
			AssertEquals("APC", result.Rows[0]["Application Code"]);
			AssertEquals(10L, result.Rows[0]["Branch Key"]);
			AssertEquals("SYD", result.Rows[0]["Customs Office"]);
			AssertEquals(8L, result.Rows[0]["Declarant Address Key"]);
			AssertEquals(declarationKey, result.Rows[0]["Declaration Key"]);
			AssertEquals("HB1", result.Rows[0]["House Bill Number"]);
			AssertEquals(26L, result.Rows[0]["Importer Key"]);
			AssertEquals("JOB1", result.Rows[0]["Job Number"]);
			AssertEquals("MB1", result.Rows[0]["Master Bill Number"]);
		}

		protected override string ScriptDbName { get { return Db.EdwDatabaseName; } }
	}
}
