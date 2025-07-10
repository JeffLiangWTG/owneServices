using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventMismatchOnWarehouseAndAreaOrLocationViaW3Table))]
	class TG_PreventMismatchOnWarehouseAndAreaOrLocationViaW3TableTest : DBCreateTriggerScriptTest
	{
		// Tested in Enterprise.Warehouse.Transactions.Business.WhsProductParamsByWhsAndClient
	}
}

