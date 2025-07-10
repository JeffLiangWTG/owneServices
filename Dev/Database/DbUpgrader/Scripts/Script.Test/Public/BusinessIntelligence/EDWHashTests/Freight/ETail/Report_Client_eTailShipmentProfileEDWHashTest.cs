using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.ETail
{
	[TestedType(typeof(Report_Client_eTailShipmentProfile))]
	class Report_Client_eTailShipmentProfileEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/ETail/Report_Client_eTailShipmentProfile.cs";
		protected override string expectedMainDbFunctionHash => "3FDCB0C3E77F3879159A9430BEF656BA610B1A8CB4AE06BAD5663C6F9AEF9372";
		protected override string expectedEdwDbFunctionHash => "3FDCB0C3E77F3879159A9430BEF656BA610B1A8CB4AE06BAD5663C6F9AEF9372";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_Client_eTailShipmentProfile();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.ETail.Report_Client_eTailShipmentProfile();
		}
	}
}
