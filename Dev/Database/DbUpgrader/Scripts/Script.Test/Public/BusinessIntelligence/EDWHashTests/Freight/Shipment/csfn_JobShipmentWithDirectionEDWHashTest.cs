using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(csfn_JobShipmentWithDirection))]
	class csfn_JobShipmentWithDirectionEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Shipment/csfn_JobShipmentWithDirection.cs";
		protected override string expectedMainDbFunctionHash => "CEA66E3C2F041A05DA6E6236175ECE8722A3620B42CED8ED0AF315DBFA60E34B";
		protected override string expectedEdwDbFunctionHash => "91C06EC229D999DF196E2719DD9A70C9C06A0943068B10E89A8BF85F2A63F990";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_JobShipmentWithDirection();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.csfn_JobShipmentWithDirection();
		}
	}
}
