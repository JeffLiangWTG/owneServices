using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterData.ComplianceRisk;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.MasterData.ComplianceRisk
{
	[TestedType(typeof(Report_Shipment_IndividualJobRiskStatusReport))]
	class Report_Shipment_IndividualJobRiskStatusReportTest : DbCreateScriptTest
	{
		#region Actual Report

		public void Test_Actual_IndividualJobRiskStatusReport_ColumnNamesAndRowCount()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-01', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert columns names
			Assert("Has a column named ShipperName", result.Columns.Contains("ShipperName"));
			Assert("Has a column named ConsigneeName", result.Columns.Contains("ConsigneeName"));
			Assert("Has a column named Origin Country Name", result.Columns.Contains("OriginCountryName"));
			Assert("Has a column named Destination Country Name", result.Columns.Contains("DestinationCountryName"));
			Assert("Has a column named Mode", result.Columns.Contains("Mode"));
			Assert("Has a column named VesselOrFlight", result.Columns.Contains("VesselOrFlight"));
			Assert("Has a column named FlightOrVoyage", result.Columns.Contains("FlightOrVoyage"));
			Assert("Has a column named ETD", result.Columns.Contains("ETD"));
			Assert("Has a column named ETA", result.Columns.Contains("ETA"));
			Assert("Has a column named Override Status User", result.Columns.Contains("OverrideStatusUserStaffName"));
			Assert("Has a column named Job Compliance Status", result.Columns.Contains("OverallComplianceRiskStatus"));
			Assert("Has a column named Parties Status", result.Columns.Contains("PartiesStatus"));
			Assert("Has a column named Locations Status", result.Columns.Contains("LocationsStatus"));
			Assert("Has a column named Commodities Status", result.Columns.Contains("CommoditiesStatus"));
			Assert("Has a column named Nomenclature Alerts", result.Columns.Contains("NomenclatureAlerts"));
			Assert("Has a column named Restricted Documents Released", result.Columns.Contains("RestrictedDocumentsReleased"));
			Assert("Has a column named Job Number", result.Columns.Contains("JobNumber"));

			//Row and column count
			AssertEquals("Result should return 14 row", 14, result.Rows.Count);
			AssertEquals("Result should return 22 column", 22, result.Columns.Count);
		}

		public void Test_Actual_IndividualJobRiskStatusReport_WithETDAndETA()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-01', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert column values
			AssertEquals("Australia", result.Rows[0]["OriginCountryName"]);
			AssertEquals("India", result.Rows[0]["DestinationCountryName"]);
			AssertEquals("AIR", result.Rows[0]["Mode"]);
			AssertEquals(DBNull.Value, result.Rows[0]["VesselOrFlight"]);
			AssertEquals(new DateTime(2023, 03, 01, 00, 00, 0), result.Rows[0]["ETD"]);
			AssertEquals(new DateTime(2023, 04, 01, 00, 00, 0), result.Rows[0]["ETA"]);
			AssertEquals("S00000001", result.Rows[0]["JobNumber"]);
		}

		public void Test_Actual_IndividualJobRiskStatusReport_WithETD()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert column values
			AssertEquals("QANTAS AIRWAYS", result.Rows[0]["ShipperName"]);
			AssertEquals("AIR INDIA", result.Rows[0]["ConsigneeName"]);
			AssertEquals("Australia", result.Rows[0]["OriginCountryName"]);
			AssertEquals("India", result.Rows[0]["DestinationCountryName"]);
			AssertEquals("AIR", result.Rows[0]["Mode"]);
			AssertEquals(DBNull.Value, result.Rows[0]["VesselOrFlight"]);
			AssertEquals(new DateTime(2023, 03, 01, 00, 00, 0), result.Rows[0]["ETD"]);
			AssertEquals(new DateTime(2023, 04, 01, 00, 00, 0), result.Rows[0]["ETA"]);
			AssertEquals("Authorization For Release(CargoWise Support)", result.Rows[0]["RestrictedDocumentsReleased"]);
			AssertEquals("S00000001", result.Rows[0]["JobNumber"]);
		}

		public void Test_Actual_IndividualJobRiskStatusReport_WithETA()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = null, @ETDTo = null, @ETAFrom = '2023-04-08', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert column values
			AssertEquals("Result should return 7 row", 7, result.Rows.Count);

			AssertEquals(DBNull.Value, result.Rows[0]["ShipperName"]);
			AssertEquals(DBNull.Value, result.Rows[0]["ConsigneeName"]);
			AssertEquals("Australia", result.Rows[0]["OriginCountryName"]);
			AssertEquals("India", result.Rows[0]["DestinationCountryName"]);
			AssertEquals("RAI", result.Rows[0]["Mode"]);
			AssertEquals(DBNull.Value, result.Rows[0]["VesselOrFlight"]);
			AssertEquals(new DateTime(2023, 03, 08, 00, 00, 0), result.Rows[0]["ETD"]);
			AssertEquals(new DateTime(2023, 04, 08, 00, 00, 0), result.Rows[0]["ETA"]);
			AssertEquals(string.Empty, result.Rows[0]["OverrideStatusUserStaffName"]);
			AssertEquals("Clear", result.Rows[0]["OverallComplianceRiskStatus"]);
			AssertEquals("Clear", result.Rows[0]["PartiesStatus"]);
			AssertEquals("Clear", result.Rows[0]["LocationsStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["CommoditiesStatus"]);
			AssertEquals(DBNull.Value, result.Rows[0]["RestrictedDocumentsReleased"]);
			AssertEquals("S00000008", result.Rows[0]["JobNumber"]);
		}

		public void Test_Actual_IndividualJobRiskStatusReport_NoSTULog_WithComplianceRiskStatus()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-02', @ETDTo = '2023-03-02', @ETAFrom = '2023-04-02', @ETATo = '2023-04-02', @Today = '2023-03-15', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			Assert("Pre-condition: There is no STU Log for the shipment", HasNoSTULogWithJobNumber(result.Rows[0]["JobNumber"]));
			Assert("Pre-condition: There is compliance risk status for the shipment", HasComplianceRiskStatusWithJobNumber(result.Rows[0]["JobNumber"]));

			AssertEquals("Override Clear", result.Rows[0]["OverallComplianceRiskStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["PartiesStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["LocationsStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["CommoditiesStatus"]);

			UpdateComplianceRiskStatus("PSK", "CLR", "PSK", "CLR", result.Rows[0]["JobNumber"] as string);
			TestConnection.ExecuteNonQuery(GetQueryToCreateCRILog(result.Rows[0]["JobNumber"] as string, "CAI"));

			result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			AssertEquals("Potential Risk", result.Rows[0]["OverallComplianceRiskStatus"]);
			AssertEquals("Clear", result.Rows[0]["PartiesStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["LocationsStatus"]);
			AssertEquals("Clear", result.Rows[0]["CommoditiesStatus"]);
		}

		#endregion Actual Report

		#region Planned Report

		public void Test_Planned_IndividualJobRiskStatusReport_ColumnNamesAndRowCount()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);

			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-01', @ETATo = '2023-04-30', @Today = '2023-03-15', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert columns names
			Assert("Has a column named ShipperName", result.Columns.Contains("ShipperName"));
			Assert("Has a column named Consignee", result.Columns.Contains("ConsigneeName"));
			Assert("Has a column named Origin Country", result.Columns.Contains("OriginCountryName"));
			Assert("Has a column named Destination Country", result.Columns.Contains("DestinationCountryName"));
			Assert("Has a column named Mode", result.Columns.Contains("Mode"));
			Assert("Has a column named VesselOrFlight", result.Columns.Contains("VesselOrFlight"));
			Assert("Has a column named ETD", result.Columns.Contains("ETD"));
			Assert("Has a column named ETA", result.Columns.Contains("ETA"));
			Assert("Has a column named Override Status User staff name", result.Columns.Contains("OverrideStatusUserStaffName"));
			Assert("Has a column named Job Compliance Status", result.Columns.Contains("OverallComplianceRiskStatus"));
			Assert("Has a column named Parties Status", result.Columns.Contains("PartiesStatus"));
			Assert("Has a column named Locations Status", result.Columns.Contains("LocationsStatus"));
			Assert("Has a column named Commodities Status", result.Columns.Contains("CommoditiesStatus"));
			Assert("Has a column named Nomenclature Alerts", result.Columns.Contains("NomenclatureAlerts"));
			Assert("Has a column named Restricted Documents Released", result.Columns.Contains("RestrictedDocumentsReleased"));
			Assert("Has a column named Job Number", result.Columns.Contains("JobNumber"));

			//Row and column count
			AssertEquals("Result should return 16 row", 16, result.Rows.Count);
			AssertEquals("Result should return 22 column", 22, result.Columns.Count);
		}

		public void Test_Planned_IndividualJobRiskStatusReport_WithNomenclatureAlerts()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var compareSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-14', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var compareResult = DataUtils.GetDataTableFromQuery(TestConnection, compareSql);
			AssertEquals("There are 17 shipments during this time period.", 17, compareResult.Rows.Count);

			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-14', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = 'YES'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			AssertEquals("There are 4 shipments with a Nomenclature Alert of Yes", 4, result.Rows.Count);

			AssertContainsExactElementsInAnyOrder("The Nomenclature Alert for shipment S00000015 and S00000030 is yes, and S00000016 and S00000029 are the parent shipments of S00000030.", new[]
			{
				("S00000015", "Yes"),
				("S00000016", "Yes"),
				("S00000029", "Yes"),
				("S00000030", "Yes")
			}, result.AsEnumerable().Select(u => (u["JobNumber"].ToString(), u["NomenclatureAlerts"].ToString())));
		}

		public void Test_Planned_IndividualJobRiskStatusReport_WithETDAndETA()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = '2023-04-01', @ETATo = '2023-04-30', @Today = '2023-03-10', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert column values
			CombineAssertions(() =>
			{
				AssertEquals(DBNull.Value, result.Rows[0]["ShipperName"]);
				AssertEquals(DBNull.Value, result.Rows[0]["ConsigneeName"]);
				AssertEquals("Australia", result.Rows[0]["OriginCountryName"]);
				AssertEquals("India", result.Rows[0]["DestinationCountryName"]);
				AssertEquals("COU", result.Rows[0]["Mode"]);
				AssertEquals("Vessel", result.Rows[0]["VesselOrFlight"]);
				AssertEquals("Flight", result.Rows[0]["FlightOrVoyage"]);
				AssertEquals(new DateTime(2023, 03, 10, 00, 00, 0), result.Rows[0]["ETD"]);
				AssertEquals(new DateTime(2023, 04, 10, 00, 00, 0), result.Rows[0]["ETA"]);
				AssertEquals(string.Empty, result.Rows[0]["OverrideStatusUserStaffName"]);
				AssertEquals("S00000010", result.Rows[0]["JobNumber"]);
			});
		}

		public void Test_Planned_IndividualJobRiskStatusReport_WithETD()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-25', @ETAFrom = null, @ETATo = null, @Today = '2023-03-11', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert column values
			CombineAssertions(() =>
			{
				AssertEquals(DBNull.Value, result.Rows[0]["ShipperName"]);
				AssertEquals(DBNull.Value, result.Rows[0]["ConsigneeName"]);
				AssertEquals("Australia", result.Rows[0]["OriginCountryName"]);
				AssertEquals("India", result.Rows[0]["DestinationCountryName"]);
				AssertEquals("FSA", result.Rows[0]["Mode"]);
				AssertEquals(DBNull.Value, result.Rows[0]["VesselOrFlight"]);
				AssertEquals(new DateTime(2023, 03, 11, 00, 00, 0), result.Rows[0]["ETD"]);
				AssertEquals(new DateTime(2023, 04, 11, 00, 00, 0), result.Rows[0]["ETA"]);
				AssertEquals(string.Empty, result.Rows[0]["OverrideStatusUserStaffName"]);
				AssertEquals("S00000011", result.Rows[0]["JobNumber"]);
			});
		}

		public void Test_Planned_IndividualJobRiskStatusReport_WithETA()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = null, @ETDTo = null, @ETAFrom = '2023-04-01', @ETATo = '2023-04-30', @Today = '2023-03-12', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			//Assert column values
			CombineAssertions(() =>
			{
				AssertEquals(DBNull.Value, result.Rows[0]["ShipperName"]);
				AssertEquals(DBNull.Value, result.Rows[0]["ConsigneeName"]);
				AssertEquals("Australia", result.Rows[0]["OriginCountryName"]);
				AssertEquals("India", result.Rows[0]["DestinationCountryName"]);
				AssertEquals("FSA", result.Rows[0]["Mode"]);
				AssertEquals(DBNull.Value, result.Rows[0]["VesselOrFlight"]);
				AssertEquals(new DateTime(2023, 03, 12, 00, 00, 0), result.Rows[0]["ETD"]);
				AssertEquals(new DateTime(2023, 04, 12, 00, 00, 0), result.Rows[0]["ETA"]);
				AssertEquals(string.Empty, result.Rows[0]["OverrideStatusUserStaffName"]);
			});
		}

		public void Test_Planned_IndividualJobRiskStatusReport_NoSTULog_WithComplianceRiskStatus()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-09', @ETDTo = '2023-03-09', @ETAFrom = '2023-04-09', @ETATo = '2023-04-09', @Today = '2023-03-04', @ReportType = 'Planned', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			Assert("Pre-condition: There is no STU Log for the shipment", HasNoSTULogWithJobNumber(result.Rows[0]["JobNumber"]));
			Assert("Pre-condition: There is compliance risk status for the shipment", HasComplianceRiskStatusWithJobNumber(result.Rows[0]["JobNumber"]));

			AssertEquals("Potential Risk", result.Rows[0]["OverallComplianceRiskStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["PartiesStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["LocationsStatus"]);
			AssertEquals("Incomplete", result.Rows[0]["CommoditiesStatus"]);

			UpdateComplianceRiskStatus("OVR", "CLR", "PSK", "CLR", result.Rows[0]["JobNumber"] as string);
			TestConnection.ExecuteNonQuery(GetQueryToCreateCRILog(result.Rows[0]["JobNumber"] as string, "CAD"));

			result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);
			AssertEquals("Override Clear", result.Rows[0]["OverallComplianceRiskStatus"]);
			AssertEquals("Clear", result.Rows[0]["PartiesStatus"]);
			AssertEquals("Potential Risk", result.Rows[0]["LocationsStatus"]);
			AssertEquals("Clear", result.Rows[0]["CommoditiesStatus"]);
		}

		#endregion Planned Report

		bool HasNoSTULogWithJobNumber(object jobNumber)
		{
			var getStmALogQuery = $@"SELECT * FROM dbo.StmALog sal INNER JOIN dbo.JobShipment js ON js.JS_PK = sal.SL_Parent AND js.JS_UniqueConsignRef = '{jobNumber}' WHERE SL_SE_NKEvent = 'STU'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, getStmALogQuery);

			return result.Rows.Count == 0;
		}

		bool HasComplianceRiskStatusWithJobNumber(object jobNumber)
		{
			var getComplianceRiskStatus = $@"SELECT * FROM dbo.ComplianceRiskStatus crs INNER JOIN dbo.JobShipment js ON js.JS_PK = crs.COR_ParentID AND js.JS_UniqueConsignRef = '{jobNumber}'";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, getComplianceRiskStatus);

			return result.Rows.Count == 1;
		}

		void UpdateComplianceRiskStatus(string overAllRisk, string partyRisk, string locationRisk, string commodityRisk, string jobNumber)
		{
			TestConnection.ExecuteNonQuery($"Update dbo.ComplianceRiskStatus SET [COR_OverallRisk] = '{overAllRisk}',[COR_LocationRisk] = '{locationRisk}',[COR_CommodityRisk] = '{commodityRisk}',[COR_PartyRisk] = '{partyRisk}' FROM dbo.ComplianceRiskStatus JOIN dbo.JobShipment ON JS_PK = COR_ParentID WHERE JS_UniqueConsignRef = '{jobNumber}'");
		}

		public void Test_IndividualJobRiskStatusReport_WithSingleShipmentLinkedWithMultipleJobHeadersUsingCurrentCompany()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-02-01', @ETDTo = '2023-03-02', @ETAFrom = null, @ETATo = null, @Today = '2023-03-13', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'ALL', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			AssertEquals(2, result.Rows.Count);
			AssertEquals("S00000001", result.Rows[0]["JobNumber"]);
			AssertEquals("S00000002", result.Rows[1]["JobNumber"]);
		}

		public void Test_IndividualJobRiskStatusReport_ShowOnlyExportShipments()
		{
			TestConnection.ExecuteNonQuery(TestDataSql);
			var reportSql = @"EXEC Report_Shipment_IndividualJobRiskStatusReport @CurrentCountry = 'AU', @CompanyPk = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D', @ETDFrom = '2023-03-01', @ETDTo = '2023-03-30', @ETAFrom = null, @ETATo = null, @Today = '2023-03-30', @ReportType = 'Departed', @Consignor = '', @Consignee = '', @ContainerMode = '', @TransportMode = '', @Origin = NULL, @Destination = NULL, @CoLoadType =  'ALL', @Direction = 'EXP', @NomenclatureAlerts = ''";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, reportSql);

			for (int i = 0; i < result.Rows.Count; i++)
			{
				AssertNotEquals(result.Rows[i]["DestinationCountryCode"], result.Rows[i]["OriginCountryCode"]);
			}
		}

		string GetQueryToCreateCRILog(string jobNumber, string logCode)
		{
			return $"INSERT INTO [dbo].[StmComplianceEvent] ([SCE_PK],[SCE_ParentID],[SCE_ParentTableCode],[SCE_EventType],[SCE_EventSubType],[SCE_EventReference],[SCE_NewValue],[SCE_OldValue],[SCE_Snapshot],[SCE_EventTimeOffset],[SCE_SystemCreateTimeUtc],[SCE_SystemCreateUser],[SCE_SystemLastEditTimeUtc],[SCE_SystemLastEditUser]) VALUES (NEWID(), (SELECT TOP 1 JS_PK FROM [dbo].[JobShipment] WHERE JS_UniqueConsignRef = '{jobNumber}'), 'JS', 'CRI', '{logCode}', 'Compliance Assessment {(logCode == "CAI" ? "Initialized" : "Declined")}', '', '', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E')";
		}

		string TestDataSql
		{
			get
			{
				var orgHeader1PK = Guid.NewGuid();
				var orgHeader2PK = Guid.NewGuid();
				var orgHeaderLocalPK = Guid.NewGuid();
				var orgAddress1PK = Guid.NewGuid();
				var orgAddress2PK = Guid.NewGuid();
				var orgAddressLocalPK = Guid.NewGuid();
				var shipmentPKs = new Guid[30];

				for (int i = 0; i < 30; i++)
				{
					shipmentPKs[i] = Guid.NewGuid();
				}

				var commodityRiskStatusPK1 = Guid.NewGuid();
				var commodityRiskStatusPK2 = Guid.NewGuid();

				var consolePK = Guid.NewGuid();
				return $@"
DECLARE @CompanyPk AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68D'
DECLARE @CompanyPk1 AS UNIQUEIDENTIFIER = 'D381CB3B-281E-4BA4-B7B8-A0B045FDA68C'
DECLARE @BranchPk AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775433'
DECLARE @BranchPk1 AS UNIQUEIDENTIFIER = '3C282841-E41C-4CBD-9A98-15ED3C775432'
DECLARE @DepartmentPk AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FB'
DECLARE @DepartmentPk1 AS UNIQUEIDENTIFIER = 'A433C594-8C39-46D2-B601-30877A9F69FA'

INSERT INTO dbo.GlbCompany (GC_PK,GC_Code, GC_Name) VALUES (@CompanyPk, 'DAN', 'AU company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC) VALUES (@BranchPk, @CompanyPk)
INSERT INTO dbo.GlbDepartment (GE_PK) VALUES (@DepartmentPk)

INSERT INTO dbo.GlbCompany (GC_PK, GC_CODE, GC_Name) VALUES (@CompanyPk1, 'COM', 'AU company')
INSERT INTO dbo.GlbBranch (GB_PK, GB_GC, GB_CODE) VALUES (@BranchPk1, @CompanyPk1, 'ABC')
INSERT INTO dbo.GlbDepartment (GE_PK, GE_CODE) VALUES (@DepartmentPk1, 'DEF')

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignor, OH_OverrideAdditionalAddressInformation) VALUES ('{orgHeader1PK}', 'QANAIR_WW', 'QANTAS AIRWAYS', 1, 1)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignee, OH_OverrideAdditionalAddressInformation) VALUES ('{orgHeader2PK}', 'INDIA_WW', 'AIR INDIA', 1, 1)
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_IsConsignee, OH_OverrideAdditionalAddressInformation) VALUES ('{orgHeaderLocalPK}', 'LOCAL CLIENT', 'LOCAL CLIENT', 1, 1)

INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation) VALUES ('{orgAddress1PK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeader1PK}', 'AUSYD', 'AU', 'SYDNEY', 'GEORGE STREET BUILDING')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation) VALUES ('{orgAddress2PK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeader2PK}', 'INBLR', 'IN', 'Bangalore', 'Brigade')
INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_State, OA_PostCode, OA_OH, OA_RL_NKRelatedPortCode, OA_RN_NKCountryCode, OA_City, OA_AdditionalAddressInformation) VALUES ('{orgAddressLocalPK}', 'CNR QANTAS DRIVE & LINK ROAD', 'NSW', '2000', '{orgHeaderLocalPK}', 'INBLR', 'IN', 'Bangalore', 'Brigade')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000001','{shipmentPKs[0]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', '{orgAddressLocalPK}')
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000001','{shipmentPKs[0]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk1, 'WRK', '{orgAddressLocalPK}')

INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000002','{shipmentPKs[1]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000003','{shipmentPKs[2]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000004','{shipmentPKs[3]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000005','{shipmentPKs[4]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000006','{shipmentPKs[5]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000007','{shipmentPKs[6]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000008','{shipmentPKs[7]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '00000009','{shipmentPKs[8]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000010','{shipmentPKs[9]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000011','{shipmentPKs[10]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000012','{shipmentPKs[11]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000013','{shipmentPKs[12]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000014','{shipmentPKs[13]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000015','{shipmentPKs[14]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000016','{shipmentPKs[15]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000017','{shipmentPKs[16]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000018','{shipmentPKs[17]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000019','{shipmentPKs[18]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000020','{shipmentPKs[19]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000021','{shipmentPKs[20]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000022','{shipmentPKs[21]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000023','{shipmentPKs[22]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000024','{shipmentPKs[23]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000025','{shipmentPKs[24]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000026','{shipmentPKs[25]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000027','{shipmentPKs[26]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000028','{shipmentPKs[27]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000029','{shipmentPKs[28]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)
INSERT INTO dbo.JobHeader(JH_PK, JH_JobNum, JH_ParentID, JH_ParentTableCode,JH_HeaderType,JH_GB, JH_GE, JH_GC, JH_Status, JH_OA_LocalChargesAddr) VALUES(NEWID(), '000000030','{shipmentPKs[29]}','JS','JOB', @BranchPk, @DepartmentPk, @CompanyPk, 'WRK', NULL)

INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[0]}', 'S00000001', '2023-03-01', '2023-04-01', 'AIR', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[1]}', 'S00000002', '2023-03-02', '2023-04-02', 'AIR', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[2]}', 'S00000003', '2023-03-03', '2023-04-03', 'SEA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[3]}', 'S00000004', '2023-03-04', '2023-04-04', 'SEA', 'CN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[4]}', 'S00000005', '2023-03-05', '2023-04-05', 'ROA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[5]}', 'S00000006', '2023-03-06', '2023-04-06', 'ROA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[6]}', 'S00000007', '2023-03-07', '2023-04-07', 'RAI', 'CN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[7]}', 'S00000008', '2023-03-08', '2023-04-08', 'RAI', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[8]}', 'S00000009', '2023-03-09', '2023-04-09', 'COU', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[9]}', 'S00000010', '2023-03-010', '2023-04-10', 'COU', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[10]}', 'S00000011', '2023-03-11', '2023-04-11', 'FSA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[11]}', 'S00000012', '2023-03-12', '2023-04-12', 'FSA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[12]}', 'S00000013', '2023-03-13', '2023-04-13', 'FAS', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[13]}', 'S00000014', '2023-03-14', '2023-04-14', 'FAS', 'CN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[14]}', 'S00000015', '2023-03-15', '2023-04-15', 'FAS', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[15]}', 'S00000016', '2023-03-16', '2023-04-16', 'AIR', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[16]}', 'S00000017', '2023-03-17', '2023-04-17', 'AIR', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[17]}', 'S00000018', '2023-03-18', '2023-04-18', 'SEA', 'IN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[18]}', 'S00000019', '2023-03-19', '2023-04-19', 'SEA', 'IN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[19]}', 'S00000020', '2023-03-20', '2023-04-20', 'ROA', 'IN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[20]}', 'S00000021', '2023-03-21', '2023-04-21', 'ROA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[21]}', 'S00000022', '2023-03-22', '2023-04-22', 'RAI', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[22]}', 'S00000023', '2023-03-23', '2023-04-23', 'RAI', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[23]}', 'S00000024', '2023-03-24', '2023-04-24', 'COU', 'IN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[24]}', 'S00000025', '2023-03-25', '2023-04-25', 'COU', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[25]}', 'S00000026', '2023-03-26', '2023-04-26', 'FSA', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[26]}', 'S00000027', '2023-03-27', '2023-04-27', 'FSA', 'IN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[27]}', 'S00000028', '2023-03-28', '2023-04-28', 'FAS', 'AU', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_JS_ColoadMasterShipment, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[28]}', 'S00000029', '{shipmentPKs[15]}', '2023-03-29', '2023-04-29', 'FAS', 'CN', 'IN')
INSERT INTO dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_JS_ColoadMasterShipment, JS_E_DEP, JS_E_ARV, JS_TransportMode, JS_RL_NKOrigin, JS_RL_NKDestination) VALUES ('{shipmentPKs[29]}', 'S00000030', '{shipmentPKs[28]}', '2023-03-30', '2023-04-30', 'FAS', 'AU', 'IN')

INSERT INTO dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_RL_NKLoadPort, JK_RL_NKDischargePort, JK_IsCancelled) 
VALUES ('{consolePK}', 'C00001000', 'AUSYD', 'USORD', 0)

INSERT INTO dbo.JobConShipLink (JN_PK, JN_JK, JN_JS)
VALUES (newid(), '{consolePK}', '{shipmentPKs[9]}')

INSERT INTO [dbo].[JobConsolTransport]
([JW_PK], [JW_TransportMode], [JW_Vessel], [JW_VoyageFlight], [JW_ETD], [JW_ETA], [JW_RL_NKLoadPort], [JW_RL_NKDiscPort], [JW_ParentGUID])
VALUES (newid(), 'AIR', 'Vessel', 'Flight', '2023-02-01', '2023-04-01','AUBNE', 'CNTAI', '{consolePK}');

INSERT INTO [dbo].[ComplianceRiskStatus] ([COR_PK],[COR_ParentTableCode],[COR_ParentID],[COR_OverallRisk],[COR_LocationRisk],[COR_CommodityRisk],[COR_PartyRisk],[COR_SystemCreateTimeUtc],[COR_SystemCreateUser],[COR_SystemLastEditTimeUtc],[COR_SystemLastEditUser])
VALUES(newid(),'JS', '{shipmentPKs[0]}','PSK','PSK','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[1]}','OVR','PSK','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[2]}','CLR','CLR','CLR','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[3]}','PSK','PSK','INC','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[4]}','PSK','CLR','INC','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[5]}','PSK','PSK','CLR','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[6]}','OVR','PSK','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[7]}','CLR','CLR','PSK','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[8]}','PSK','PSK','INC','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[9]}','OVR','CLR','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[10]}','PSK','PSK','CLR','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[11]}','CLR','CLR','INC','CLR',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[12]}','OVR','PSK','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	(newid(),'JS', '{shipmentPKs[13]}','PSK','PSK','PSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	('{commodityRiskStatusPK1}','JS', '{shipmentPKs[14]}','HLD','PSK','HSK','PSK',GETDATE(),'E',GETDATE(),'E'),
	('{commodityRiskStatusPK2}','JS', '{shipmentPKs[29]}','HLD','PSK','HSK','PSK',GETDATE(),'E',GETDATE(),'E')

--commodity
INSERT INTO [dbo].[ComplianceCommodityDetail] ([CCD_PK], [CCD_COR_ComplianceRisk], [CCD_CountryOrGrouping], [CCD_SystemCreateTimeUtc], [CCD_SystemCreateUser], [CCD_SystemLastEditTimeUtc] ,[CCD_SystemLastEditUser], [CCD_HarmonizedCode], [CCD_RiskStatus], [CCD_NomenclatureCondition])
VALUES(newid(), '{commodityRiskStatusPK1}', 'WCO', GETDATE(),'E',GETDATE(),'E', '998757', 'HSK', 1),
(newid(), '{commodityRiskStatusPK2}', 'WCO', GETDATE(),'E',GETDATE(),'E', '998758', 'HSK', 1);

--shipment 1 Logs
INSERT INTO [dbo].[StmALog] ([SL_PK],[SL_Table],[SL_Parent],[SL_IsEstimate],[SL_IsCancelled],[SL_Reference],[SL_PostedTimeUtc],[SL_EventTime],[SL_GS_NKUser],[SL_SE_NKEvent],[SL_GB_NKBranch],[SL_GE_NKDepartment],[SL_FireWorkflow])
VALUES
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|TYP=Job compliance status','2023-02-25','2023-02-25','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Location risk status','2023-02-25','2023-02-25','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|TYP=Party risk status','2023-02-25','2023-02-25','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|TYP=Commodity risk status','2023-02-25','2023-02-25','E', 'STU','BNE','BRN',0),

	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Job compliance status','2023-03-01','2023-03-01','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|TYP=Location risk status','2023-03-01','2023-03-01','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Party risk status','2023-03-01','2023-03-01','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Commodity risk status','2023-03-01','2023-03-01','E', 'STU','BNE','BRN',0),

	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Override Clear|OLD=Potential Risk|TYP=Job compliance status','2023-02-28','2023-02-28','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|TYP=Location risk status','2023-02-28','2023-02-28','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|OLD=Clear|TYP=Party risk status','2023-02-28','2023-02-28','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|TYP=Commodity risk status','2023-02-28','2023-02-28','E', 'STU','BNE','BRN',0),

	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Job compliance status','2023-03-20','2023-03-20','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Potential Risk|TYP=Location risk status','2023-03-20','2023-03-20','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Party risk status','2023-03-20','2023-03-20','E', 'STU','BNE','BRN',0),
	(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|NEW=Clear|OLD=Potential Risk|TYP=Commodity risk status','2023-03-20','2023-03-20','E', 'STU','BNE','BRN',0)

INSERT INTO [dbo].[StmALog] ([SL_PK],[SL_Table],[SL_Parent],[SL_IsEstimate],[SL_IsCancelled],[SL_Reference],[SL_PostedTimeUtc],[SL_EventTime],[SL_GS_NKUser],[SL_SE_NKEvent],[SL_GB_NKBranch],[SL_GE_NKDepartment],[SL_FireWorkflow])
VALUES(NEWID(), 'JobShipment','{shipmentPKs[0]}', 'N','N','|MST=Compliance Risk|Document Hold Status Overridden|Authorization For Release', DATEADD(day, -2, GETDATE()),DATEADD(DAY, -2, GETDATE()),'E', 'HSO','BNE','BRN',0)

--shipment 1 Logs
INSERT INTO [dbo].[StmComplianceEvent] ([SCE_PK],[SCE_ParentID],[SCE_ParentTableCode],[SCE_EventType],[SCE_EventSubType],[SCE_EventReference],[SCE_NewValue],[SCE_OldValue],[SCE_Snapshot],[SCE_EventTimeOffset],[SCE_SystemCreateTimeUtc],[SCE_SystemCreateUser],[SCE_SystemLastEditTimeUtc],[SCE_SystemLastEditUser])
VALUES
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'PSK', 'OVR', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'CLR', 'PSK', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'PSK', 'CLR', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'PSK', 'INC', '', '2023-02-25 18:00:00 +11:00', '2023-02-25 07:00:00', 'E', '2023-02-25 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'CLR', 'PSK', '', '2023-03-01 18:00:00 +11:00', '2023-03-01 07:00:00', 'E', '2023-03-01 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'PSK', 'CLR', '', '2023-03-01 18:00:00 +11:00', '2023-03-01 07:00:00', 'E', '2023-03-01 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'CLR', 'PSK', '', '2023-03-01 18:00:00 +11:00', '2023-03-01 07:00:00', 'E', '2023-03-01 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'CLR', 'PSK', '', '2023-03-01 18:00:00 +11:00', '2023-03-01 07:00:00', 'E', '2023-03-01 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'CLR', 'PSK', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'CLR', 'PSK', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'PSK', 'CLR', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'PSK', 'CLR', '', '2023-02-28 18:00:00 +11:00', '2023-02-28 07:00:00', 'E', '2023-02-28 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'OVL', 'Job Compliance status', 'CLR', 'PSK', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'LOC', 'Location Compliance Risk status', 'PSK', 'CLR', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'PTY', 'Party Compliance Risk status', 'CLR', 'PSK', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),
	(NEWID(), '{shipmentPKs[0]}', 'JS', 'STU', 'CMD', 'Commodities Compliance Risk status', 'CLR', 'PSK', '', '2023-03-20 18:00:00 +11:00', '2023-03-20 07:00:00', 'E', '2023-03-20 07:00:00', 'E'),

	(NEWID(), '{shipmentPKs[0]}', 'JS', 'HSO', 'OVR', 'Document Hold Status Overridden|Authorization For Release', '', '', '', '2023-03-21 18:00:00 +11:00', '2023-03-21 07:00:00', 'E', '2023-03-21 07:00:00', 'E')

INSERT INTO dbo.JobDocAddress
(E2_PK, E2_OA_Address, E2_AddressOverride, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_AdditionalAddressInformation, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
VALUES(NEWID(), '{orgAddress1PK}', 0, '{shipmentPKs[0]}', 'JS', 'CRD', 'Inner Sanctum', '2020-12-17 15:31', 'E'),
	(NEWID(), '{orgAddress2PK}', 0, '{shipmentPKs[0]}', 'JS', 'CED', 'Inner Sanctum', '2020-12-17 15:31', 'E')
";
			}
		}
	}
}
