using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(ctfn_JobShipmentOrg))]
	class ctfn_JobShipmentOrgEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/InternationalLogistics/ctfn_JobShipmentOrg.cs";
		protected override string expectedMainDbFunctionHash => "EFF2C579038E529CE2C46B7E4DF81CEF28D270626B8AB28157AD6B9E92358421";
		protected override string expectedEdwDbFunctionHash => "EE5F4D4C4CAD219297C1073B8F7474297695B3FE282AEF19F9BF6289F7EC59AB";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new ctfn_JobShipmentOrg();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.InternationalLogistics.ctfn_JobShipmentOrg();
		}
	}
}

