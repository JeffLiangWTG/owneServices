using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(csfn_ShipmentMainConsol))]
	class csfn_ShipmentMainConsolEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Shipment/csfn_ShipmentMainConsol.cs";
		protected override string expectedMainDbFunctionHash => "3BB08BA5A4D4D4E64723C426DC8065A2EEC75D15BDEA1847D7FF08A5098FC80C";
		protected override string expectedEdwDbFunctionHash => "B24908DF6550431798AFCA66B0B932E20A26293D7E56BAEBF4CBA54094F5C411";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_ShipmentMainConsol();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.csfn_ShipmentMainConsol();
		}
	}
}

