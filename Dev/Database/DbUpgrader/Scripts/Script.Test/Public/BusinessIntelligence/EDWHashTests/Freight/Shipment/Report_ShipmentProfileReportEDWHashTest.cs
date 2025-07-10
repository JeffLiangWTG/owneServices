using CargoWise.DbUpgrader.Scripts.Abstractions;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDWHashTests.Freight.Shipment
{
	[TestedType(typeof(Report_ShipmentProfileReport))]
	internal class Report_ShipmentProfileReportEDWHashTest : EdwHashTest
	{
		protected override string edwScriptPath => "ReportFunctions/Freight/Shipment/Report_ShipmentProfileReport.sql";
		protected override string expectedMainDbFunctionHash => "381558A0ECFD50B1CA6173D66EDE1EB06FAA575411242803C41272A7C8727819";
		protected override string expectedEdwDbFunctionHash => "A03B60C6494AF8724A8898CD3E4BB1210B5E4E67C19DDFDD1C6E5962DB4C03ED";

		protected override DbCreateScript GetMainDbFunction()
		{
			return new Report_ShipmentProfileReport();
		}

		protected override BiCreateScript GetEdwDbFunction()
		{
			return new CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.Shipment.Report_ShipmentProfileReport();
		}
	}
}
