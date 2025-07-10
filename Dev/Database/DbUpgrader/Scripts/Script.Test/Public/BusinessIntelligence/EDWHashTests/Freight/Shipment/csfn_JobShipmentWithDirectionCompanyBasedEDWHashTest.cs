using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(csfn_JobShipmentWithDirectionCompanyBased))]
	class csfn_JobShipmentWithDirectionCompanyBasedEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Shipment/csfn_JobShipmentWithDirectionCompanyBased.cs";
		protected override string expectedMainDbFunctionHash => "EF09C13B5ADE7E89515867FC945C690E5FEF912F9D8A222D5C70437B77BA49E5";
		protected override string expectedEdwDbFunctionHash => "8D50A2DCA3B714C746E3076F67516155E35A3DD791BB7FC5F723FC9BD421F45A";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new csfn_JobShipmentWithDirectionCompanyBased();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.csfn_JobShipmentWithDirectionCompanyBased();
		}
	}
}
