using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Testing
{
	[TestedType(typeof(WhsCheckLinesDoNotExceedLocationCapacity))]
	class WhsCheckLinesDoNotExceedLocationCapacityTest : DbCreateScriptTest
	{
		// Tested in:
		//		Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing.Trigger_TG_PreventOverfillLocationWithUnitsCapacityTest
	}
}

