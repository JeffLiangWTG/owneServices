using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckInterWhsTransfersAreInSync))]
	class WhsCheckInterWhsTransfersAreInSyncTest : DbCreateScriptTest
	{
		// Tested implicitly in WhsTransfer.TestTG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised
	}
}

