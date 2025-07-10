using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.ETail;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.ETail
{
	[TestedType(typeof(Report_eTailShipmentProfile))]
	class Report_eTailShipmentProfileEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/ETail/Report_eTailShipmentProfile.cs";
		protected override string expectedMainDbFunctionHash => "4D2F0A41421937506E1A49A4DF1D4DB7740ABE7AEAEA2A0856202AD33B4BB564";
		protected override string expectedEdwDbFunctionHash => "B3CF302CAFEEAC29DAED7C29822B8A4773F342153C9D60A14A46020C9A5CAAEA";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_eTailShipmentProfile();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.ETail.Report_eTailShipmentProfile();
		}
	}
}
