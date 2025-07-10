using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsProductListingReport))]
	class WhsProductListingReportTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;
	}
}

