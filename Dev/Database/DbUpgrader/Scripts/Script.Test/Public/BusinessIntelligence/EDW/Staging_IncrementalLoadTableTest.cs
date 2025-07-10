using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Testing
{
	[TestedType(typeof(Staging_IncrementalLoadTable))]
	internal class Staging_IncrementalLoadTableTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestContainsTPTTComments()
		{
				//CheckContainsTPTTComments("Ini", 1);
				AssertContains("EXEC biadmin.usp_CreateStagingTableIndex", ScriptToTest.Text);
				CheckContainsTPTTComments("Inc", 1);
		}
	}
}
