using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsCreatePutawayLocationCache))]
	class WhsCreatePutawayLocationCacheTest : DbCreateScriptTest
	{
		// Tested in Enterprise.Warehouse.Transactions.Business.Testing.WhsPutawayLocationCacheManagerTest
	}
}
