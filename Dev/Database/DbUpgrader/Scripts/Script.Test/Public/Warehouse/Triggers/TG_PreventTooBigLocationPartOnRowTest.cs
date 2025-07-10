using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Triggers;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Triggers.Testing
{
	[TestedType(typeof(TG_PreventTooBigLocationPartOnRow))]
	class TG_PreventTooBigLocationPartOnRowTest : DBCreateTriggerScriptTest
	{
		/* 
			Tested in: Enterprise.Warehouse.Environment.Business.Testing.WhsRowTriggersTest
		*/
	}
}

