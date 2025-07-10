using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(FCLShipmentContainers))]
	class FCLShipmentContainersEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Shipment/FCLShipmentContainers.sql";
		protected override string expectedMainDbFunctionHash => "148B8296FC145BA367C077B13A7957506A4B2023A940D67A846D8FBCD234B8DC";
		protected override string expectedEdwDbFunctionHash => "2610607BF871A35F924FD023D8B48A08EBF0380F3BDA9A57BDAF1D087B115CD8";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new FCLShipmentContainers();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.FCLShipmentContainers();
		}
	}
}

