using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventUpdatingDocketLineType))]
	class TG_PreventUpdatingDocketLineTypeTest : DBCreateTriggerScriptTest
	{
		// Tested in Enterprise.Warehouse.Transactions.Business.WhsDocketLine
	}
}

