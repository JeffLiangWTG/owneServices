using Enterprise.DbUpgrader.Schema.Testing;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class MetadataScriptRunnerTest : SchemaSyncTestCase
	{
		public void TestGetScriptReplacingDbNames()
		{
			string testDbBeingUpgraded = "TestGetScriptReplacingDbNamesDbBeingUpgraded";
			string testTemplateDb = "TestGetScriptReplacingDbNamesTemplateDb";
			string rawScript = "This is just a dummy test script: DB1 = [{0}] and DB2 = [{1}]";
			string expectedScript = "This is just a dummy test script: DB1 = [" + testDbBeingUpgraded + "] and DB2 = [" + testTemplateDb + "]";

			MetadataScriptRunnerForTesting testScriptRunner = new MetadataScriptRunnerForTesting(TestConnection, testDbBeingUpgraded, testTemplateDb);
			string actualScript = testScriptRunner.GetScriptReplacingDbNames_Exposed(rawScript);
			AssertEquals("Result script", expectedScript, actualScript);
		}
	}
}
