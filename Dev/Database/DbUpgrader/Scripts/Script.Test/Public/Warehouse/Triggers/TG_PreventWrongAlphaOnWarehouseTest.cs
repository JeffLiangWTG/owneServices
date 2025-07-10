using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventWrongAlphaOnWarehouse))]
	class TG_PreventWrongAlphaOnWarehouseTest : DBCreateTriggerScriptTest
	{
		/* 
			Tested in: Enterprise.Warehouse.Environment.Business.Testing.WhsWarehouseTriggersTest
		*/
	}
}

