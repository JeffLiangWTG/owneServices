using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised))]
	class TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronisedTest : DBCreateTriggerScriptTest
	{
		// Tested implicitly in WhsTransfer.TestTG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised
	}
}

