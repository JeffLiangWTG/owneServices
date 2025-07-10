using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsPickLine_EnsureAllocatedInventoryIsAllocateable))]
	class TG_WhsPickLine_EnsureAllocatedInventoryIsAllocateableTest : DBCreateTriggerScriptTest
	{
		// tested in WhsPickLineTest
	}
}
