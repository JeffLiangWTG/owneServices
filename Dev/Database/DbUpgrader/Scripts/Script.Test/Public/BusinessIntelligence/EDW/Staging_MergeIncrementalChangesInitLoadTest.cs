using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Testing
{
	[TestedType(typeof(Staging_MergeIncrementalChangesInitLoad))]
	internal class Staging_MergeIncrementalChangesInitLoadTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestContainsTPTTComments()
		{
			CheckContainsTPTTComments("Ini", 2);
		}
	}
}

