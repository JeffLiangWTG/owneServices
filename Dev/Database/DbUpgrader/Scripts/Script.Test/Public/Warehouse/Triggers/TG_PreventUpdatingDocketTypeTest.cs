using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventUpdatingDocketType))]
	class TG_PreventUpdatingDocketTypeTest : DBCreateTriggerScriptTest
	{
		// Tested in Enterprise.Warehouse.Transactions.Business.WhsDocket
	}
}

