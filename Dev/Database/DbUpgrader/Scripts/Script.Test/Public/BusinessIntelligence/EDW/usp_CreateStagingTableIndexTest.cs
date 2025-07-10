using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW
{
	[TestedType(typeof(usp_CreateStagingTableIndex))]
	class usp_CreateStagingTableIndexTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestContainsTPTTComments()
		{
			CheckContainsTPTTComments("Ini", 1);
		}
	}
}
