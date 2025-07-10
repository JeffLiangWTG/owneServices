using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventMismatchOnDocketStatusAndDateWithLines))]
	class TG_PreventMismatchOnDocketStatusAndDateWithLinesLines : DBCreateTriggerScriptTest
	{
		// Tested in WhsDocket.cs - WhsDocketTriggerTest
	}
}

