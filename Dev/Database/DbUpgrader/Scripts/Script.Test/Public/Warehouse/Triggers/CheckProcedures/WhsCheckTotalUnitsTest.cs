using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckTotalUnits))]
	class WhsCheckTotalUnitsTest : DbCreateScriptTest
	{
		// Tested in PreventOverReducingStockViaWhsInventoryTest.cs
	}
}

