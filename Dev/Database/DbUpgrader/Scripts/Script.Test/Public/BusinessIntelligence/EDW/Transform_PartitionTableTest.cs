using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Testing
{
	[TestedType(typeof(Transform_PartitionTable))]
	internal class Transform_PartitionTableTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestContainsTPTTComments()
		{
			CheckContainsTPTTComments("Inc", 2);
		}
	}
}

