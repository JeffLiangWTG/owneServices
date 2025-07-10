using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.Freight.ETail;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.Freight.ETail.Testing
{
	[TestedType(typeof(Report_Client_eTailShipmentProfile))]
	class Report_Client_eTailShipmentProfileEDWTest : BiCreateScriptTest
	{
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

				var result = GetReportFunctionResult(Guid.Empty);

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

				var result = GetReportFunctionResult(Guid.Empty);

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

				var result = GetReportFunctionResult(Guid.Empty, "Y");
				AssertEquals("Result should have one row", 1, result.Rows.Count);
			}
		}

		public void TestReport_eTailShipmentProfile_FilterClient()
		{
			using (CultureUtils.WithCulture(System.Globalization.CultureInfo.CreateSpecificCulture("en-AU")))
			{
				var shipmentId1 = PrepTestReport_eTailShipmentItem("shipment001", "TSTORG1");
				var shipmentId2 = PrepTestReport_eTailShipmentItem("shipment002", "TSTORG2");
				var shipmentId3 = PrepTestReport_eTailShipmentItem("shipment003", "TSTORG3");
				var shipmentId4 = PrepTestReport_eTailShipmentItem("shipment004", "TSTORG4");

				var (organisationId1, organisationKey1) = EDWTestDataCreator.CreateOrganisation("TSTORG11", "Test Organisation 11");
				var (_, addressKey1) = EDWTestDataCreator.CreateOrganisationAddress(organisationKey1, "Address1", "Test Address 1");
				EDWTestDataCreator.CreateDocAddress(addressKey1, shipmentId1, "CRD");

				var (organisationId2, organisationKey2) = EDWTestDataCreator.CreateOrganisation("TSTORG22", "Test Organisation 22");
				var (_, addressKey2) = EDWTestDataCreator.CreateOrganisationAddress(organisationKey2, "Address2", "Test Address 2");
				EDWTestDataCreator.CreateDocAddress(addressKey2, shipmentId2, "CED");

				var (organisationId3, organisationKey3) = EDWTestDataCreator.CreateOrganisation("TSTORG33", "Test Organisation 33");
				var (_, addressKey3) = EDWTestDataCreator.CreateOrganisationAddress(organisationKey3, "Address3", "Test Address 3");

				EDWTestDataCreator.CreateJobHeader("TestJobNum", shipmentId3, addressKey3, TestDbHelper.DefaultCompanyPK);

				EDWTestDataCreator.CreateDocAddress(addressKey1, shipmentId4, "CRD");
				EDWTestDataCreator.CreateDocAddress(addressKey2, shipmentId4, "CED");

				CombineAssertions(() =>
				{
					var result1 = GetReportFunctionResult(Guid.Empty);
					AssertEquals("Result should have four row when Client is null", 4, result1.Rows.Count);

					var result2 = GetReportFunctionResult(organisationId1);
					AssertEquals("Result should have two row when Client is eTailer", 2, result2.Rows.Count);

					var result3 = GetReportFunctionResult(organisationId2);
					AssertEquals("Result should have two row when Client is Consignee", 2, result3.Rows.Count);

					var result4 = GetReportFunctionResult(organisationId3);
					AssertEquals("Result should have one row when Client is Local Client", 1, result4.Rows.Count);
				});
			}
		}

		Guid PrepTestReport_eTailShipmentItem(string shipmentNumber, string orgCode)
		{
			var (_, organisationKey) = EDWTestDataCreator.CreateOrganisation(orgCode, orgName);
			EDWTestDataCreator.CreateOrganisationAddress(organisationKey, addCode, address1);

			var (shipmentId, shipmentKey) = EDWTestDataCreator.CreateShipment(shipmentNumber, transportMode, packingMode, origin, destination, DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1);
			var (_, consignmentKey) = EDWTestDataCreator.CreateHVLVConsignment();
			EDWTestDataCreator.CreateHVLVItem(consignmentKey, shipmentKey, shipmentId, SurplusAtDestinationDepot);

			return shipmentId;
		}

		DataTable GetReportFunctionResult(Guid client, string onlyExceptions = "")
		{
			var clientParamValue = client == Guid.Empty ? "null" : $"'{client}'";
			return DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM [{ScriptDbName}].[dbo].[Report_Client_eTailShipmentProfile]({clientParamValue}, '{TestDbHelper.DefaultCompanyCountryCode}', '{TestDbHelper.DefaultCompanyPK}', '', '', '', '', '', '{onlyExceptions}')");
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
