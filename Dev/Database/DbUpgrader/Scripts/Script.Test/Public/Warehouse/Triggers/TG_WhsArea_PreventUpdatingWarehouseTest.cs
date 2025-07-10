using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsArea_PreventUpdatingWarehouse))]
	class TG_WhsArea_PreventUpdatingWarehouseTest : DBCreateTriggerScriptTest
	{
		// Tested in Enterprise.Warehouse.Environment.Business.WhsArea.cs
	}
}

