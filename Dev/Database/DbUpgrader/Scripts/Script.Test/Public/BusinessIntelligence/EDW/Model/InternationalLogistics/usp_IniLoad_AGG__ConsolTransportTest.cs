using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(usp_IniLoad_AGG__ConsolTransport))]
	internal class usp_IniLoad_AGG__ConsolTransportTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, null, new DateTime(2020, 11, 21), new Guid("3E6E31E2-A116-45AE-81F5-1F71523031E0"), new Guid("F83151B4-A9A2-4B79-B729-8B7FFBDC8534"), 1, new DateTime(2023, 05, 23), new DateTime(2023, 05, 10), new DateTime(2023, 05, 06), new DateTime(2023, 05, 29), 1, "USATL", new DateTime(2019, 08, 08), new DateTime(2020, 12, 03), new DateTime(2020, 12, 01), new DateTime(2020, 12, 02), new DateTime(2019, 08, 12), new DateTime(2019, 08, 09), new DateTime(2020, 12, 04), 1, 1, 1, 1, new Guid("9424227B-065C-41A9-AE52-2634260FE694"), 1, "USNYC", 1, new Guid("3E6E31E2-A116-45AE-81F5-1F71523031E0"), new Guid("13FEB832-D008-4BB0-B074-78C46353E051"), new DateTime(2020, 12, 03), new DateTime(2020, 11, 21), "AIR", "MAI", "RAIL JOURNEY", "RAIL55");
				AssertRowValues(resultTable, null, new DateTime(2022, 09, 22), new Guid("3E6E31E2-A116-45AE-81F5-1F71523031E0"), new Guid("832C742D-3A1C-4DEE-BEA4-71159F4C4023"), 2, new DateTime(2018, 05, 05), new DateTime(2018, 04, 30), new DateTime(2018, 04, 30), new DateTime(2018, 05, 07), 2, "USATL", new DateTime(2010, 11, 22), new DateTime(2022, 09, 24), new DateTime(2022, 09, 22), new DateTime(2010, 07, 14), new DateTime(2010, 11, 22), new DateTime(2010, 11, 19), new DateTime(2010, 07, 17), 1, 1, 2, 2, new Guid("589E99A8-6E63-4F9A-B669-E0849CAD338D"), 1, "USORD", 1, new Guid("3E6E31E2-A116-45AE-81F5-1F71523031E0"), new Guid("2AE6C887-CD91-4EA3-83BC-B5F895C916C1"), new DateTime(2022, 09, 24), new DateTime(2022, 09, 22), "AIR", "MAI", "APL KING", "333");
			});
		}

		void AssertRowValues(DataTable resultTable, DateTime? aTA, DateTime? aTD, Guid? carrier, Guid? consolAndShipmentTransportID, int? consolAndShipmentTransportKey, DateTime? deportAvalibilityDate, DateTime? depotCutOff, DateTime? depotReceivalCommences, DateTime? depotStorageDate, int? destinationKey, string discPort, DateTime? documentaryCutoff, DateTime? eTA, DateTime? eTD, DateTime? fCLAvailabilityDate, DateTime? fCLCutOff, DateTime? fCLReceivalCommences, DateTime? fCLStorageDate, int? isActive, int? isLinked, int? jobVoyageKey, int? jobVoyOriginKey, Guid? jX, int? legOrder, string loadPort, int? organizationAddressKey, Guid? organizationID, Guid? parentGUID, DateTime? timeofArrival, DateTime? timeofDischarge, string transportMode, string transportType, string vessel, string voyageFlight)
		{
			var selectqry = string.Format("ATA {0} AND ATD {1} AND Carrier {2} AND ConsolAndShipmentTransportID {3} AND ConsolAndShipmentTransportKey {4} AND DepotAvailabilityDate {5} AND DepotCutOff {6} AND DepotReceivalCommences {7} AND DepotStorageDate {8} AND DestinationKey {9} AND DiscPort {10} AND DocumentaryCutoff {11} AND ETA {12} AND ETD {13} AND FCLAvailabilityDate {14} AND FCLCutOff {15} AND FCLReceivalCommences {16} AND FCLStorageDate {17} AND IsActive {18} AND IsLinked {19} AND JobVoyageKey {20} AND JobVoyOriginKey {21} AND JX {22} AND LegOrder {23} AND LoadPort {24} AND OrganizationAddressKey {25} AND OrganizationID {26} AND ParentGUID {27} AND TimeofArrival {28} AND TimeofDischarge{29} AND TransportMode {30} AND TransportType {31} AND Vessel {32} AND VoyageFlight {33}",
				aTA == null ? "IS NULL" : "= '" + aTA + "'",
				aTD == null ? "IS NULL" : "= '" + aTD + "'",
				carrier == null ? "IS NULL" : "= '" + carrier + "'",
				consolAndShipmentTransportID == null ? "IS NULL" : "= '" + consolAndShipmentTransportID + "'",
				consolAndShipmentTransportKey == null ? "IS NULL" : "= '" + consolAndShipmentTransportKey + "'",
				deportAvalibilityDate == null ? "IS NULL" : "= '" + deportAvalibilityDate + "'",
				depotCutOff == null ? "IS NULL" : "= '" + depotCutOff + "'",
				depotReceivalCommences == null ? "IS NULL" : "= '" + depotReceivalCommences + "'",
				depotStorageDate == null ? "IS NULL" : "= '" + depotStorageDate + "'",
				destinationKey == null ? "IS NULL" : "= " + destinationKey,
				discPort == null ? "IS NULL" : "= '" + discPort + "'",
				documentaryCutoff == null ? "IS NULL" : "= '" + documentaryCutoff + "'",
				eTA == null ? "IS NULL" : "= '" + eTA + "'",
				eTD == null ? "IS NULL" : "= '" + eTD + "'",
				fCLAvailabilityDate == null ? "IS NULL" : "= '" + fCLAvailabilityDate + "'",
				fCLCutOff == null ? "IS NULL" : "= '" + fCLCutOff + "'",
				fCLReceivalCommences == null ? "IS NULL" : "= '" + fCLReceivalCommences + "'",
				fCLStorageDate == null ? "IS NULL" : "= '" + fCLStorageDate + "'",
				isActive == null ? "IS NULL" : "= " + isActive,
				isLinked == null ? "IS NULL" : "= " + isLinked,
				jobVoyageKey == null ? "IS NULL" : "= " + jobVoyageKey,
				jobVoyOriginKey == null ? "IS NULL" : "= " + jobVoyOriginKey,
				jX == null ? "IS NULL" : "= '" + jX + "'",
				legOrder == null ? "IS NULL" : "= " + legOrder,
				loadPort == null ? "IS NULL" : "= '" + loadPort + "'",
				organizationAddressKey == null ? "IS NULL" : "= " + organizationAddressKey,
				organizationID == null ? "IS NULL" : "= '" + organizationID + "'",
				parentGUID == null ? "IS NULL" : "= '" + parentGUID + "'",
				timeofArrival == null ? "IS NULL" : "= '" + timeofArrival + "'",
				timeofDischarge == null ? "IS NULL" : "= '" + timeofDischarge + "'",
				transportMode == null ? "IS NULL" : "= '" + transportMode + "'",
				transportType == null ? "IS NULL" : "= '" + transportType + "'",
				vessel == null ? "IS NULL" : "= '" + vessel + "'",
				voyageFlight == null ? "IS NULL" : "= '" + voyageFlight + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[AGG__ConsolTransport]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__ConsolAndShipmentTransport]
					([ConsolAndShipmentTransportKey], [ConsolAndShipmentTransportID], [JobSailingKey], [CarrierAddressKey], [ParentType], [ATA], [ATD], [ETA], [ETD], [DiscPort], [IsLinked], [JX],
						[LegOrder], [LoadPort], [ParentGUID], [TransportMode], [TransportType], [Vessel], [VoyageFlight])
					VALUES
						(1, 'F83151B4-A9A2-4B79-B729-8B7FFBDC8534', 1, 1, 'CON', NULL, '2020-11-21', '2020-12-03', '2020-12-01', 'USATL', 1, '9424227B-065C-41A9-AE52-2634260FE694', 1, 'USNYC', '13FEB832-D008-4BB0-B074-78C46353E051', 'RAI', 'MAI', 'RAIL JOURNEY', 'RAIL55'),
						(2, '832C742D-3A1C-4DEE-BEA4-71159F4C4023', 2, 1, 'CON', NULL, '2022-09-22', '2022-09-24', '2022-09-22', 'USATL', 1, '589E99A8-6E63-4F9A-B669-E0849CAD338D', 1, 'USORD', '2AE6C887-CD91-4EA3-83BC-B5F895C916C1', 'SEA', 'MAI', 'APL KING', '333');

				INSERT [{0}].[Sailing].[BAS__JobSailing]
					([JobSailingKey], [JobSailingID], [JobVoyOriginKey], [JobVoyDestinationKey], [DepotAvailabilityDate], [DepotCutOff], [DepotReceivalCommences], [DepotStorageDate] )
					VALUES
						(1, newid(), 1, 1, '2023-05-23', '2023-05-10', '2023-05-06', '2023-05-29'),
						(2, newid(), 2, 2, '2018-05-05' ,'2018-04-30', '2018-04-30', '2018-05-07');

				INSERT [{0}].[Sailing].[BAS__JobVoyDestination]
					([JobVoyDestinationKey], [JobVoyDestinationID],[AvailabilityDate], [StorageDate])
					VALUES
						(1, newid(), '2020-12-02', '2020-12-04'),
						(2, newid(), '2010-07-14', '2010-07-17');

				INSERT [{0}].[Sailing].[BAS__JobVoyOrigin]
					([JobVoyOriginKey],[JobVoyOriginID],[JobVoyageKey],[ReceivalCommences],[CutOff],[DocumentaryCutoff])
					VALUES
						(1,newid(), 1, '2019-08-09',	'2019-08-12', '2019-08-08'),
						(2,newid(), 2, '2010-11-19', '2010-11-22', '2010-11-22');

				INSERT INTO [{0}].[Sailing].[BAS__JobVoyage]
					([JobVoyageKey], [JobVoyageID], [Organization], [IsActive], [AirSeaRoad])
					VALUES
						(1, newid(), '3E6E31E2-A116-45AE-81F5-1F71523031E0', 1, 'AIR'),
						(2, newid(), '3E6E31E2-A116-45AE-81F5-1F71523031E0', 1, 'AIR');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [Organization])
					VALUES (1, newid(), '3E6E31E2-A116-45AE-81F5-1F71523031E0');",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
