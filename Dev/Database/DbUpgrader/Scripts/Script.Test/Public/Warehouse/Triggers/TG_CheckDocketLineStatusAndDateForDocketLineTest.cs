using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_CheckDocketLineStatusAndDateForDocketLine))]
	class TG_CheckDocketLineStatusAndDateForDocketLineTest : DBCreateTriggerScriptTest
	{
		// Tested in WhsDocketLineTriggerTestCase.cs
	}
}

