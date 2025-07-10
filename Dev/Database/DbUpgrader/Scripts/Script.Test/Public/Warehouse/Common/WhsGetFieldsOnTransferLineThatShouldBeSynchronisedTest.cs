using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse.Common;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse.Common
{
	[TestedType(typeof(WhsGetFieldsOnTransferLineThatShouldBeSynchronised))]
	class WhsGetFieldsOnTransferLineThatShouldBeSynchronisedTest : DbCreateScriptTest
	{
		// Tested implicitly in the tests for the trigger it is consumed in
	}
}

