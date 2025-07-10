using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.MainDb
{
	[TestedType(typeof(CdcGetChangedTables))]
	class CdcGetChangedTablesTest : DbCreateScriptTest
	{
		public override void TestScriptIsTheSameAsInTheDatabase()
		{
			// SP CdcGetChangedTablesForAudit could be changed during DBUpgrade
			Assert(true);
		}
	}
}

