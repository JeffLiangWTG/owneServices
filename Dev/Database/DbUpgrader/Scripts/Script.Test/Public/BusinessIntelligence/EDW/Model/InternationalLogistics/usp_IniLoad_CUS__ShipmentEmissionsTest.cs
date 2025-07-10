using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__ShipmentEmissions))]
	internal class usp_IniLoad_CUS__ShipmentEmissionsTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 3, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, 1, "SGSIN", "AUSYD", "SEA",
					999.9m, 5000m, 1000.02m, "KG",
					"2024-05-02 12:00:00", "2024-05-01 12:00:00", "2024-05-06 12:00:00", "2024-05-01 12:00:00",
					new Guid("F3B53329-75C0-4872-8FE7-48D36FD95423"),
					new Guid("5334CA8D-0FCA-4D06-BC62-D737ADCADB4C"),
					new Guid("7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9"),
					new Guid("DBFBA57E-4DCE-4373-9854-1941367598FD"),
					new Guid("126DE328-EE0A-4C21-97B6-BBA8BD253B8A"),
					"CUR");

				AssertRowValues(resultTable, 2, 2, "AUSYD", "AUMBE", "AIR",
					1.0m, 10000m, 9.390m, "T",
					"2024-05-03 12:00:00", "2024-05-04 12:00:00", "2024-05-04 12:00:00", "2024-05-03 12:00:00",
					new Guid("840CD929-2AFB-4584-88FC-FFDF674E5C8A"),
					new Guid("5334CA8D-0FCA-4D06-BC62-D737ADCADB4C"),
					new Guid("7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9"),
					new Guid("DBFBA57E-4DCE-4373-9854-1941367598FD"),
					new Guid("126DE328-EE0A-4C21-97B6-BBA8BD253B8A"),
					"NCU");

				AssertRowValues(resultTable, 1, 1, "SGSIN", "AUSYD", "SEA",
					999.9m, 5000m, 1000.02m, "KG",
					"2024-05-02 12:00:00", "2024-05-01 12:00:00", "2024-05-06 12:00:00", "2024-05-01 12:00:00",
					new Guid("7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9"),
					new Guid("5334CA8D-0FCA-4D06-BC62-D737ADCADB4C"),
					new Guid("7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9"),
					new Guid("DBFBA57E-4DCE-4373-9854-1941367598FD"),
					new Guid("126DE328-EE0A-4C21-97B6-BBA8BD253B8A"),
					"CUR");
			});
		}

		void AssertRowValues(DataTable resultTable, int? shipmentKey, int? jobHeaderKey, string portOfDestination, string portOfOrigin, string transportMode,
			decimal? totalCO2e, decimal? distance, decimal? shipmentWeight, string shipmentUnitOfWeight,
			string arrivalDate, string departureDate, string actualArrivalDate, string actualDepartureDate,
			Guid? carrierOrgId, Guid? consigneeOrgID, Guid? consignorOrgID, Guid? controllingCustomerOrgID, Guid? localClientOrgID, string co2eStatus)
		{
			var conditions = new Dictionary<string, object>
			{
				{ "ShipmentKey", shipmentKey },
				{ "JobHeaderKey", jobHeaderKey },
				{ "PortOfDestination", portOfDestination },
				{ "PortOfOrigin", portOfOrigin },
				{ "TransportMode", transportMode },
				{ "TotalCO2e", totalCO2e },
				{ "DistanceInKM", distance },
				{ "ShipmentWeight", shipmentWeight },
				{ "ShipmentUnitOfWeight", shipmentUnitOfWeight },
				{ "ArrivalDate", arrivalDate },
				{ "DepartureDate", departureDate },
				{ "ActualArrivalDate", actualArrivalDate },
				{ "ActualDepartureDate", actualDepartureDate },
				{ "CarrierOrgID", carrierOrgId },
				{ "ConsigneeOrgID", consigneeOrgID },
				{ "ConsignorOrgID", consignorOrgID },
				{ "ControllingCustomerOrgID", controllingCustomerOrgID },
				{ "LocalClientOrgID", localClientOrgID },
				{ "CO2eStatus", co2eStatus }
			};

			var selectQuery = string.Join(" AND ", conditions.Select(kvp =>
				$"{kvp.Key} {(kvp.Value == null ? "IS NULL" : $"= '{kvp.Value}'")}"));

			var rows = resultTable.Select(selectQuery);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[InternationalLogistics].[CUS__ShipmentEmissions]",
				ScriptDbName);
			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName => Db.EdwDatabaseName;

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__Shipment]
				([ShipmentKey], [ShipmentID], [ArrivalDate], [DepartureDate], [PortOfDestination], [PortOfOrigin], [SystemCreateTimeUtc], [TransportMode], [ActualWeight], [UnitOfWeight])
				VALUES
					(1, newid(), '2024-05-02 12:00:00', '2024-05-01 12:00:00', 'SGSIN', 'AUSYD', '2024-05-01 12:00:00', 'SEA', 1000.02, 'KG'),
					(2, newid(), '2024-05-03 12:00:00', '2024-05-04 12:00:00', 'AUSYD', 'AUMBE', '2024-05-01 12:00:00', 'AIR', 9.390, 'T');

				INSERT [{0}].[InternationalLogistics].[BAS__ShipmentCO2e]
				([ShipmentCO2eKey], [ShipmentCO2eID], [ShipmentKey], [Distance], [TotalCO2e], [CO2eStatus])
				VALUES
					(1, newid(), 1, 5000, 999.9, 'CUR'),
					(2, newid(), 2, 10000, 1.0, 'NCU');

				INSERT [{0}].[Organization].[BAS__Organization]
				([OrganizationKey], [OrganizationID])
				VALUES
					(1, '126DE328-EE0A-4C21-97B6-BBA8BD253B8A'),
					(2, 'DBFBA57E-4DCE-4373-9854-1941367598FD'),
					(3, '5334CA8D-0FCA-4D06-BC62-D737ADCADB4C'),
					(4, '7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9'),
					(5, 'F3B53329-75C0-4872-8FE7-48D36FD95423'),
					(6, '840CD929-2AFB-4584-88FC-FFDF674E5C8A');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
				([OrganizationAddressKey], [OrganizationAddressID], [OrganizationKey], [OrganizationID])
				VALUES
					(1, newid(), 1, '126DE328-EE0A-4C21-97B6-BBA8BD253B8A'),
					(2, newid(), 2, 'DBFBA57E-4DCE-4373-9854-1941367598FD'),
					(3, newid(), 3, '5334CA8D-0FCA-4D06-BC62-D737ADCADB4C'),
					(4, newid(), 4, '7C1D32A6-6EBF-4225-A554-E2C6B3C3FAC9'),
					(5, newid(), 5, 'F3B53329-75C0-4872-8FE7-48D36FD95423'),
					(6, newid(), 6, '840CD929-2AFB-4584-88FC-FFDF674E5C8A');

				INSERT [{0}].[Finance].[BAS__JobHeader]
				([JobHeaderKey], [JobHeaderID], [ShipmentKey], [LocalAgentAddressKey])
				VALUES
					(1, newid(), 1, 1),
					(2, newid(), 2, 1);

				INSERT [{0}].[InternationalLogistics].[BAS__ControllingCustomerAddress]
				([ControllingCustomerAddressKey], [ControllingCustomerAddressID], [ShipmentKey], [AddressKey])
				VALUES
					(1, newid(), 1, 2),
					(2, newid(), 2, 2);

				INSERT [{0}].[InternationalLogistics].[BAS__ConsigneeAddress]
				([ConsigneeAddressKey], [ConsigneeAddressID], [ShipmentKey], [AddressKey])
				VALUES
					(1, newid(), 1, 3),
					(2, newid(), 2, 3);

				INSERT [{0}].[InternationalLogistics].[BAS__ConsignorAddress]
				([ConsignorAddressKey], [ConsignorAddressID], [ShipmentKey], [AddressKey])
				VALUES
					(1, newid(), 1, 4),
					(2, newid(), 2, 4);

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolidationShipmentPivot]
				([ConsolidationShipmentPivotKey], [ConsolidationShipmentPivotID], [ShipmentKey], [ConsolidationKey])
				VALUES
					(1, newid(), 1, 1),
					(2, newid(), 2, 2);

				INSERT [{0}].[InternationalLogistics].[BAS__Consolidation]
				([ConsolidationKey], [ConsolidationID])
				VALUES
					(1, newid()),
					(2, newid());

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport]
				([ConsolAndShipmentTransportKey], [ConsolAndShipmentTransportID], [ConsolidationKey], [JobSailingKey], [CarrierAddressKey], [IsLinked], [TransportMode], [ParentType], [ATA], [ATD])
				VALUES
					(1, newid(), 1, 1, 6, 1, 'SEA', 'CON', '2024-05-02 12:00:00', '2024-05-01 12:00:00');

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport]
				([ConsolAndShipmentTransportKey], [ConsolAndShipmentTransportID], [ConsolidationKey], [CarrierAddressKey], [IsLinked], [TransportMode], [ParentType], [ATA], [ATD])
				VALUES
					(2, newid(), 2, 6, 0, 'AIR', 'CON', '2024-05-04 12:00:00', '2024-05-03 12:00:00');

				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport]
				([ConsolAndShipmentTransportKey], [ConsolAndShipmentTransportID], [ShipmentKey], [CarrierAddressKey], [IsLinked], [TransportMode], [ParentType], [ATA], [ATD])
				VALUES
					(3, newid(), 1, 4, 0, 'SEA', 'SHP', '2024-05-06 12:00:00', '2024-05-05 12:00:00');

				INSERT [{0}].[Sailing].[BAS__JobSailing]
				([JobSailingKey], [JobSailingID], [JobVoyOriginKey])
				VALUES
					(1, newid(), 1);

				INSERT [{0}].[Sailing].[BAS__JobVoyOrigin]
				([JobVoyOriginKey], [JobVoyOriginID], [JobVoyageKey])
				VALUES
					(1, newid(), 1);

				INSERT [{0}].[Sailing].[BAS__JobVoyage]
				([JobVoyageKey], [JobVoyageID], [OrganizationKey], [Organization])
				VALUES
					(1, newid(), 5, 'F3B53329-75C0-4872-8FE7-48D36FD95423');
				", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		string GetIniLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__ShipmentEmissions'",
				ScriptDbName);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			return record.ItemArray[0].ToString();
		}

		void Execute()
		{
			var iniLoadSQLText = GetIniLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;
			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
