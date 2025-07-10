using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US.RefDb;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US.RefDb
{
	[TestedType(typeof(vw_RefDbEntUs_USCForeignAndRegionPort))]
	class vw_RefDbEntUs_USCForeignAndRegionPortTest : DbCreateScriptTest
	{
		public void TestSampleCall()
		{
			string testName = TestConnection.ExecuteScalar("SELECT TOP 1 US_PortName FROM " + ScriptToTest.Name).ToString();
			AssertEquals("UH_Name empty?", false, string.IsNullOrEmpty(testName));
		}
	}
}

