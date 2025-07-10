using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_LocationIsInCorrectWarehouse))]
	class TG_WhsDocketLine_LocationIsInCorrectWarehouseTest : DBCreateTriggerScriptTest
	{
		// Tested in WhsDocketLineTriggerTestCase and TG_WhsDocketLine_LocationIsInCorrectWarehouse_ConcurrencyTest
	}
}

