using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers.CheckProcedures;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Testing
{
	[TestedType(typeof(WhsCheckDocketStatusAndDateWithLines))]
	class WhsCheckDocketStatusAndDateWithLinesTest : DbCreateScriptTest
	{
		// Tested in WhsDocket.cs - WhsDocketTriggerTest
	}
}

