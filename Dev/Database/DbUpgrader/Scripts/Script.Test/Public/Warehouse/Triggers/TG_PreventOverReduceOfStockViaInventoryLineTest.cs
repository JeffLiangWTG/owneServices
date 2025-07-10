using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventOverReduceOfStockViaInventoryLine))]
	class TG_PreventOverReduceOfStockViaInventoryLineTest : DBCreateTriggerScriptTest
	{
		// tested in PreventOverReducingStockViaWhsInventoryTest
	}
}

