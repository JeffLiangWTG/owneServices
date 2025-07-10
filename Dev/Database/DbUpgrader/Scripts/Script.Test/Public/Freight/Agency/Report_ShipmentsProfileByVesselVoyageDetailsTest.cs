using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Agency;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Agency.Testing
{
	[TestedType(typeof(Report_ShipmentsProfileByVesselVoyageDetails))]
	internal class Report_ShipmentsProfileByVesselVoyageDetailsTest : DbCreateScriptTest
	{
		protected override bool RequiresSchemaBinding => false;
	}
}

