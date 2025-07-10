using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(vw_ShipmentPrePlanningTransports))]
	class vw_ShipmentPrePlanningTransportsTest : DbCreateScriptTest
	{
	}
}

