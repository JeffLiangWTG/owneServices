using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsWarehouse_PreventWrongFixedWidthOnWarehouse))]
	class TG_WhsWarehouse_PreventWrongFixedWidthOnWarehouseTest : DBCreateTriggerScriptTest
	{
		// Tested in: Enterprise.Warehouse.Environment.Business.Testing.WhsWarehouseTriggersTest
	}
}
