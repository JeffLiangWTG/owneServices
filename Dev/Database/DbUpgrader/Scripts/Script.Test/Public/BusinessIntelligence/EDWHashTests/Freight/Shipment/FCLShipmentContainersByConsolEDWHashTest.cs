using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(FCLShipmentContainersByConsol))]
	class FCLShipmentContainersByConsolEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Shipment/FCLShipmentContainersByConsol.sql";
		protected override string expectedMainDbFunctionHash => "092BEA4B040B5C381DD911CA99DCB0E888091A1B56A695D4F7E47AA9EE681B5B";
		protected override string expectedEdwDbFunctionHash => "2C9540128CFE4476B68D645977D68C78C06D14FB7C4DEAE0A36338205F622015";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new FCLShipmentContainersByConsol();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.FCLShipmentContainersByConsol();
		}
	}
}

