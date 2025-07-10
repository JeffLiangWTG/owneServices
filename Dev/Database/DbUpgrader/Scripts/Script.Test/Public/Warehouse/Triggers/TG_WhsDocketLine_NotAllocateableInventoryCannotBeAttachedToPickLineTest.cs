using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_NotAllocateableInventoryCannotBeAttachedToPickLine))]
	class TG_WhsDocketLine_NotAllocateableInventoryCannotBeAttachedToPickLineTest : DBCreateTriggerScriptTest
	{
		// tested in WhsDocketLineTest
	}
}
