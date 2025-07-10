using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.ETail;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.ETail.Testing
{
	[TestedType(typeof(Report_eTailShipmentProfile))]
	class Report_eTailShipmentProfileEDWTest : BiCreateScriptTest
	{
		public void TestSQLHasNotChanged()
		{
			var expectedHashedCode = "B3CF302CAFEEAC29DAED7C29822B8A4773F342153C9D60A14A46020C9A5CAAEA";
			var sqlFunction = new Report_eTailShipmentProfile();
			var actualHashedCode = GetHashString(sqlFunction.Text);
			AssertEquals("A version of this function exists in the Odyssey database(/CargoWise.DbUpgrader/src/Scripts/Scripts.Definitions/Freight/Etail/Report_eTailShipmentProfile.sql), please update it and recalculate the hash.", expectedHashedCode, actualHashedCode);
		}

		public void TestColumnsLCL_ULD_LSE_AvailableDateAreAdded()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var lclAvailableDate = DateTime.Today.AddDays(2);
				var uldAvailableDate = DateTime.Today.AddDays(3);
				var lseAvailableDate = DateTime.Today.AddDays(4);

				var (_, organisationKey) = EDWTestDataCreator.CreateOrganisation(orgCode, orgName);
				EDWTestDataCreator.CreateOrganisationAddress(organisationKey, addCode, address1);

				var (shipmentId, shipmentKey) = EDWTestDataCreator.CreateShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);
				EDWTestDataCreator.CreateDocumentAndCartage(shipmentKey, lclAvailableDate);

				var (_, voyageKey) = EDWTestDataCreator.CreateJobVoyage();

				var (_, originKey1) = EDWTestDataCreator.CreateJobVoyOrigin(voyageKey);
				var (_, destinationKey1) = EDWTestDataCreator.CreateJobVoyDestination(uldAvailableDate.AddDays(-1));
				var (_, sailingKey1) = EDWTestDataCreator.CreateJobSailing(originKey1, destinationKey1, lseAvailableDate.AddDays(-5));

				var (_, originKey2) = EDWTestDataCreator.CreateJobVoyOrigin(voyageKey);
				var (_, destinationKey2) = EDWTestDataCreator.CreateJobVoyDestination(uldAvailableDate);
				var (_, sailingKey2) = EDWTestDataCreator.CreateJobSailing(originKey2, destinationKey2, lseAvailableDate);

				var (consolId, consolKey) = EDWTestDataCreator.CreateConsolidation("C00012345", "USCHI", "AUSYD");
				EDWTestDataCreator.CreateShipmentMainConsol(consolId, shipmentId);
				EDWTestDataCreator.CreateConsolAndShipmentTransport(consolKey, consolId, sailingKey1, "CON", DateTime.Today, DateTime.Today.AddDays(1), 1);
				EDWTestDataCreator.CreateConsolAndShipmentTransport(consolKey, consolId, sailingKey2, "CON", DateTime.Today.AddDays(1), DateTime.Today.AddDays(2), 2);

				var (_, consignmentKey) = EDWTestDataCreator.CreateHVLVConsignment();
				EDWTestDataCreator.CreateHVLVItem(consignmentKey, shipmentKey, shipmentId, ManifestedByETailer);

				var result = GetReportFunctionResult();

				CombineAssertions(() =>
				{
					AssertEquals("Result should have one row", 1, result.Rows.Count);
					AssertEquals($"LCL Available Date should be [{lclAvailableDate}]", lclAvailableDate, result.Rows[0]["LCLAvailableDate"]);
					AssertEquals($"ULD Available Date should be [{uldAvailableDate}]", uldAvailableDate, result.Rows[0]["ULDAvailableDate"]);
					AssertEquals($"LSE Available Date should be [{lseAvailableDate}]", lseAvailableDate, result.Rows[0]["LSEAvailableDate"]);
				});
			}
		}

		public void TestReport_eTailShipmentProfile()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var (_, organisationKey) = EDWTestDataCreator.CreateOrganisation(orgCode, orgName);
				EDWTestDataCreator.CreateOrganisationAddress(organisationKey, addCode, address1);

				var (shipmentId, shipmentKey) = EDWTestDataCreator.CreateShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);
				var (_, consignmentKey) = EDWTestDataCreator.CreateHVLVConsignment();
				EDWTestDataCreator.CreateHVLVItem(consignmentKey, shipmentKey, shipmentId, ManifestedByETailer);

				var result = GetReportFunctionResult();

				CombineAssertions(() =>
				{
					AssertEquals("Result should have one row", 1, result.Rows.Count);
					AssertEquals($"ShipmentID should be [{shipmentNumber}]", shipmentNumber, result.Rows[0]["ShipmentID"]);
					AssertEquals($"Origin should be [{origin}]", origin, result.Rows[0]["Origin"]);
					AssertEquals($"Origin should be [{destination}]", destination, result.Rows[0]["Destination"]);
				});
			}
		}

		public void TestReport_eTailShipmentProfile_ExceptionItems()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var (_, organisationKey) = EDWTestDataCreator.CreateOrganisation(orgCode, orgName);
				EDWTestDataCreator.CreateOrganisationAddress(organisationKey, addCode, address1);

				var (shipmentId, shipmentKey) = EDWTestDataCreator.CreateShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);
				var (_, consignmentKey) = EDWTestDataCreator.CreateHVLVConsignment();
				EDWTestDataCreator.CreateHVLVItem(consignmentKey, shipmentKey, shipmentId, SurplusAtDestinationDepot);

				var result = GetReportFunctionResult("Y");
				AssertEquals("Result should have one row", 1, result.Rows.Count);
			}
		}

		DataTable GetReportFunctionResult(string onlyExceptions = "")
		{
			return DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM [{ScriptDbName}].[dbo].[Report_eTailShipmentProfile]('{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '{onlyExceptions}')");
		}

		protected override string ScriptDbName => Db.EdwDatabaseName;

		static readonly string orgCode = "TSTORG";
		static readonly string orgName = "Test Organisation";
		static readonly string addCode = "TSTORGADD";
		static readonly string address1 = "Test Organisation Address1";
		static readonly string shipmentNumber = "TSTShipment";
		static readonly string transportMode = "SEA";
		static readonly string packingMode = "LSE";
		static readonly string origin = "NZAKL";
		static readonly string destination = "AUSYD";
		const string ManifestedByETailer = "MAN";
		const string SurplusAtDestinationDepot = "SUD";
	}
}
