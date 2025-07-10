using CargoWise.DbUpgrader.Scripts.Definitions.TransitWarehouse;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.TransitWarehouse.Testing
{
	[TestedType(typeof(Report_WhsTransitPackageOnHandWithUNDGCode))]
	class Report_WhsTransitPackageOnHandWithUNDGCodeTest : DbCreateScriptTest
	{
		// Tested in TransitWarehouse.Business.Testing/Reports/TransitWarehousePackageOnHandWithUNDGCodeReportTest.cs
		protected override bool RequiresSchemaBinding => false;
	}
}

