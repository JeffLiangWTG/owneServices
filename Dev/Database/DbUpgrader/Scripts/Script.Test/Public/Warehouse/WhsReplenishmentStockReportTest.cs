using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsReplenishmentStockReport))]
	class WhsReplenishmentStockReportTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;
	}
}

