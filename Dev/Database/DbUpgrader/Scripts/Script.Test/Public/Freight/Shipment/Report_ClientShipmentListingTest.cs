using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Shipment;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Shipment
{
	[TestedType(typeof(Report_ClientShipmentListing))]
	sealed class Report_ClientShipmentListingTest : DbCreateScriptTest
	{
		const string DateFormat = "d/MM/yyyy";

		public void TestExportDeclarationWillBeFilteredWhenDischargeETAIsNotYetPassed()
		{
			var importerPK = TestDbHelper.DefaultCompanyOrgProxyPK;
			var companyPK = TestDbHelper.DefaultCompanyPK;
			TestConnection.ExecuteNonQuery(GetInsertDeclarationCommand(importerPK, 1, "EXP", "2010-03-15 00:00:00"));

			var count = (int)TestConnection.ExecuteScalar(
				$"SELECT COUNT(*) FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', 'ETA not yet passed', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', null, null, '2010-04-15 00:00:00')");

			AssertEquals(0, count);
		}

		public void TestSampleCall()
		{
			var importerPK = TestDbHelper.DefaultCompanyOrgProxyPK;
			var companyPK = TestDbHelper.DefaultCompanyPK;

			InsertTestData(importerPK, 1);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			AssertEquals("Result should have one row", 1, result.Rows.Count);

			var dateDeliverAvailable = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT DateDeliverAvailable FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', null, null, '')");

			AssertEquals("DateDeliverAvailable should not be null", "16/10/2010", dateDeliverAvailable.ToString(DateFormat));

			var dateDeliverStorage = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT DateDeliverStorage FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			AssertEquals("DateDeliverStorage should not be null", "17/10/2010", dateDeliverStorage.ToString(DateFormat));

			result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', 'BLK', '')");

			AssertEquals("Calculated Declaration Container Mode = 'FCL', but 'BLK' selected by user in Report conditions - no rows expected", 0, result.Rows.Count);

			var atd = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT JW_ATDFirst FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			AssertEquals("ATD date should not be null", "15/10/2018", atd.ToString(DateFormat));

			var ata = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT JW_ATALast FROM Report_ClientShipmentListing('{importerPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			AssertEquals("ATA date should not be null", "16/10/2018", ata.ToString(DateFormat));
		}

		public void TestTotalCO2e()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var clientPK = TestDbHelper.DefaultCompanyOrgProxyPK;

			// Arrange
			InsertShipmentTestData(clientPK);
			TestConnection.ExecuteNonQuery(GetInsertJobCO2eCommand(adlShipmentPK, 0.02m));

			DataTable js_results = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			AssertEquals("Result should have one row", 1, js_results.Rows.Count);

			// Act
			var totalCO2e = (decimal)TestConnection.ExecuteScalar(
				$"SELECT TotalCO2e FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			// Assert
			AssertEquals("TotalCO2e should not be null", 0.02m, totalCO2e);
		}

		public void TestStatusCO2e()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var clientPK = TestDbHelper.DefaultCompanyOrgProxyPK;

			// Arrange
			InsertShipmentTestData(clientPK);
			TestConnection.ExecuteNonQuery(GetInsertJobCO2eCommand(adlShipmentPK, 0.02m));

			DataTable js_results = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			AssertEquals("Result should have one row", 1, js_results.Rows.Count);

			// Act
			var statusCO2e = TestConnection.ExecuteScalar(
				$"SELECT StatusCO2e FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', '', 'All', '', '', '', '', '', '', '{companyPK}', '', 'SEA', 'SEA', '', null, '')");

			// Assert
			AssertEquals("StatusCO2e should match the JobCO2e.JCO_Status", "CUR", statusCO2e);
		}

		public void TestShipmentAvailabilityWithOverrideAvailableStorageSea()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var clientPK = TestDbHelper.DefaultCompanyOrgProxyPK;
			var consolPk = Guid.NewGuid();
			var containerPk = Guid.NewGuid();
			var packLinePk = Guid.NewGuid();
			var refContainerPk = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '40F'");

			InsertShipmentTestData(clientPK);
			TestConnection.ExecuteNonQuery(GetInsertVoyageCommand());
			TestConnection.ExecuteNonQuery(GetInsertVoyageOriginCommand(voyOrigin1PK, jobVoyagePK1, "2018-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageDestinationCommand(voyDestination1PK, jobVoyagePK1, "2018-10-15 00:00:00", "2010-10-16 07:51:00"));
			TestConnection.ExecuteNonQuery(GetInsertSailingCommand(jobSailingPK1, voyOrigin1PK, voyDestination1PK));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPk, "SEA", "FCL", orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(containerPk, 1, true, consolPk, refContainerPk));
			TestConnection.ExecuteNonQuery(GetInsertConsolTransportCommand(consolPk, "CON", 1, jobSailingPK1, "2018-10-14 00:00:00", "2018-10-14 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertJobDocsAndCartageCommand(adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(consolPk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLinePk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(containerPk, packLinePk));

			var js_results = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("Result should have one row", 1, js_results.Rows.Count);
			var dateDeliverAvailable = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT DateDeliverAvailable FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("DateDeliverAvailable should not be null", "15/06/2024", dateDeliverAvailable.ToString(DateFormat));
		}

		public void TestShipmentAvailabilityWithOverrideAvailableStorageAir()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var clientPK = TestDbHelper.DefaultCompanyOrgProxyPK;
			var consolPk = Guid.NewGuid();
			var containerPk = Guid.NewGuid();
			var packLinePk = Guid.NewGuid();
			var refContainerPk = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '40F'");

			InsertShipmentTestData(clientPK);
			TestConnection.ExecuteNonQuery(GetInsertVoyageCommand());
			TestConnection.ExecuteNonQuery(GetInsertVoyageOriginCommand(voyOrigin1PK, jobVoyagePK1, "2018-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageDestinationCommand(voyDestination1PK, jobVoyagePK1, "2018-10-15 00:00:00", "2010-10-16 07:51:00"));
			TestConnection.ExecuteNonQuery(GetInsertSailingCommand(jobSailingPK1, voyOrigin1PK, voyDestination1PK));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPk, "AIR", "LSE", orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(containerPk, 1, true, consolPk, refContainerPk));
			TestConnection.ExecuteNonQuery(GetInsertConsolTransportCommand(consolPk, "CON", 1, jobSailingPK1, "2018-10-14 00:00:00", "2018-10-14 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertJobDocsAndCartageCommand(adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(consolPk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLinePk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(containerPk, packLinePk));

			var js_results = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("Result should have one row", 1, js_results.Rows.Count);
			var dateDeliverAvailable = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT DateDeliverAvailable FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("DateDeliverAvailable should not be null", "15/06/2024", dateDeliverAvailable.ToString(DateFormat));
		}

		public void TestShipmentAvailabilityWithNoOverrideAvailableStorageSea()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var clientPK = TestDbHelper.DefaultCompanyOrgProxyPK;
			var consolPk = Guid.NewGuid();
			var containerPk = Guid.NewGuid();
			var packLinePk = Guid.NewGuid();
			var refContainerPk = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '40F'");

			InsertShipmentTestData(clientPK);
			TestConnection.ExecuteNonQuery(GetInsertVoyageCommand());
			TestConnection.ExecuteNonQuery(GetInsertVoyageOriginCommand(voyOrigin1PK, jobVoyagePK1, "2018-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageDestinationCommand(voyDestination1PK, jobVoyagePK1, "2018-10-15 00:00:00", "2010-10-16 07:51:00"));
			TestConnection.ExecuteNonQuery(GetInsertSailingCommand(jobSailingPK1, voyOrigin1PK, voyDestination1PK));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPk, "SEA", "FCL", orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(containerPk, 1, false, consolPk, refContainerPk));
			TestConnection.ExecuteNonQuery(GetInsertConsolTransportCommand(consolPk, "CON", 1, jobSailingPK1, "2018-10-14 00:00:00", "2018-10-14 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertJobDocsAndCartageCommand(adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(consolPk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLinePk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(containerPk, packLinePk));

			var js_results = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("Result should have one row", 1, js_results.Rows.Count);
			var dateDeliverAvailable = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT DateDeliverAvailable FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("DateDeliverAvailable should not be null", "16/10/2010", dateDeliverAvailable.ToString(DateFormat));
		}

		public void TestShipmentAvailabilityWithNoOverrideAvailableStorageAir()
		{
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var clientPK = TestDbHelper.DefaultCompanyOrgProxyPK;
			var consolPk = Guid.NewGuid();
			var containerPk = Guid.NewGuid();
			var packLinePk = Guid.NewGuid();
			var refContainerPk = (Guid)TestConnection.ExecuteScalar($"SELECT TOP 1 RC_PK FROM dbo.RefContainer WHERE RC_StorageClass = '40F'");

			InsertShipmentTestData(clientPK);
			TestConnection.ExecuteNonQuery(GetInsertVoyageCommand());
			TestConnection.ExecuteNonQuery(GetInsertVoyageOriginCommand(voyOrigin1PK, jobVoyagePK1, "2018-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageDestinationCommand(voyDestination1PK, jobVoyagePK1, "2018-10-15 00:00:00", "2010-10-16 07:51:00"));
			TestConnection.ExecuteNonQuery(GetInsertSailingCommand(jobSailingPK1, voyOrigin1PK, voyDestination1PK));
			TestConnection.ExecuteNonQuery(GetInsertJobConsolCommand(consolPk, "AIR", "LSE", orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerCommand(containerPk, 1, false, consolPk, refContainerPk));
			TestConnection.ExecuteNonQuery(GetInsertConsolTransportCommand(consolPk, "CON", 1, jobSailingPK1, "2018-10-14 00:00:00", "2018-10-14 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertJobDocsAndCartageCommand(adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobConShipLinkCommand(consolPk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobPackLinesCommand(packLinePk, adlShipmentPK));
			TestConnection.ExecuteNonQuery(GetInsertJobContainerPackPivotCommand(containerPk, packLinePk));

			var js_results = DataUtils.GetDataTableFromQuery(TestConnection,
				$"SELECT * FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("Result should have one row", 1, js_results.Rows.Count);
			var dateDeliverAvailable = (DateTime)TestConnection.ExecuteScalar(
				$"SELECT DateDeliverAvailable FROM Report_ClientShipmentListing('{clientPK}', 'Y', 'Y', 'Y', 'All', '', '', '', '', '', '', '{companyPK}', '20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN', 'SEA', 'SEA', '', null, '')");
			AssertEquals("DateDeliverAvailable should not be null", "16/10/2010", dateDeliverAvailable.ToString(DateFormat));
		}

		readonly Guid jobVoyagePK1 = Guid.NewGuid();
		readonly Guid jobVoyagePK2 = Guid.NewGuid();
		readonly Guid voyOrigin1PK = Guid.NewGuid();
		readonly Guid voyOrigin2PK = Guid.NewGuid();
		readonly Guid voyDestination1PK = Guid.NewGuid();
		readonly Guid voyDestination2PK = Guid.NewGuid();
		readonly Guid jobSailingPK1 = Guid.NewGuid();
		readonly Guid jobSailingPK2 = Guid.NewGuid();
		readonly Guid declarationPK = Guid.NewGuid();
		readonly Guid adlShipmentPK = Guid.NewGuid();
		readonly Guid orgAddressPK = Guid.NewGuid();

		string GetInsertDeclarationCommand(Guid importerPK, int clusterKey, string messageType, string dateAtFinalDestination)
		{
			return $@"INSERT INTO dbo.JobDeclaration
           ([JE_PK]
           ,[JE_DataModel]
           ,[JE_OH_Importer]
           ,[JE_IsCancelled]
           ,[JE_OverrideFreightDefaults]
           ,[JE_MessageType]
           ,[JE_MessageSubType]
           ,[JE_RS_NKServiceLevel]
           ,[JE_IsPersonalEffects]
           ,[JE_DeclarationReference]
           ,[JE_RL_NKOrigin]
           ,[JE_RL_NKPortOfLoading]
           ,[JE_ExportDate]
           ,[JE_RL_NKPortOfFirstArrival]
           ,[JE_DateOfFirstArrival]
           ,[JE_RL_NKPortOfArrival]
           ,[JE_DateOfArrival]
           ,[JE_RL_NKFinalDestination]
           ,[JE_DateAtFinalDestination]
           ,[JE_VesselName]
           ,[JE_LloydsIMO]
           ,[JE_VoyageFlightNo]
           ,[JE_TransportMode]
           ,[JE_AddInfo]
           ,[JE_ContainerMode]
           ,[JE_GB]
           ,[JE_GC]
           ,[JE_ClusterKey])
     VALUES
           ('{declarationPK}'
           ,'!!'
           ,'{importerPK}'
           ,0
           ,0
           ,'{messageType}'
           ,'FRM'
           ,'STD'
           ,0
           ,'B0001111'
           ,'CNSHA'
           ,'CNSHA'
           ,'2010-10-15 00:00:00'
           ,'AUSYD'
           ,'2010-10-15 00:00:00'
           ,'AUSYD'
           ,'2010-10-15 00:00:00'
           ,'AUCNS'
           ,'{dateAtFinalDestination}'
           ,'APL IVORY'
           ,''
           ,'5841'
           ,'SEA'
           ,''
           ,''
           ,'{TestDbHelper.BranchBrnPK}'
           ,'{TestDbHelper.DefaultCompanyPK}'
           ,'{clusterKey}')";
		}

		string GetInsertVoyageCommand()
		{
			return
				$@"INSERT INTO dbo.JobVoyage ([JV_PK],[JV_AirSeaRoad],[JV_VoyageType],[JV_VoyageFlight],[JV_RV_NKVessel]) VALUES ('{jobVoyagePK1}','SEA','','5841','APL IVORY');
				   INSERT INTO dbo.JobVoyage ([JV_PK],[JV_AirSeaRoad],[JV_VoyageType],[JV_VoyageFlight],[JV_RV_NKVessel]) VALUES ('{jobVoyagePK2}','SEA','','1234','APL IVORY')";
		}

		string GetInsertVoyageOriginCommand(Guid origin, Guid voyage, string depDate)
		{
			return $@"INSERT INTO dbo.JobVoyOrigin
           ([JA_PK]
           ,[JA_IsValid]
           ,[JA_RL_NKPortOfLoading]
           ,[JA_Berth]
           ,[JA_DepartReference]
           ,[JA_AutoCreated]
           ,[JA_JV]
           ,[JA_SendersMessageReference]
           ,[JA_A_DEP])
     VALUES
           ('{origin}'
           ,0
           ,''
           ,''
           ,''
           ,0
           ,'{voyage}'
           ,''
           ,'{depDate}')";
		}

		string GetInsertVoyageDestinationCommand(Guid destination, Guid voyage, string arvDate, string availDate)
		{
			return $@"INSERT INTO dbo.JobVoyDestination
			([JB_PK]
			,[JB_RL_NKPortOfDischarge]
			,[JB_E_ARV]
			,[JB_JV]
			,[JB_AvailabilityDate]
			,[JB_StorageDate]
			,[JB_A_ARV])
		VALUES
			('{destination}'
			,'AUSYD'
			,'2010-10-15 00:00:00'
			,'{voyage}'
			,'{availDate}'
			,'2010-10-17 07:51:00'
			,'{arvDate}')";
		}

		string GetInsertSailingCommand(Guid sailing, Guid origin, Guid destination)
		{
			return $@"INSERT INTO dbo.JobSailing
			([JX_PK],
			[JX_JA],
			[JX_JB],
			[JX_ReservedMasterBill],
			[JX_ContainerReleaseNumber],
			[JX_IsPublished],
			[JX_IsValid],
			[JX_Status])
	VALUES
			('{sailing}',
			'{origin}',
			'{destination}',
			'',
			'',
			0,
			0,
			'')";
		}

		string GetInsertContainerCommand(string containerMode, int clusterKey)
		{
			return $@"INSERT INTO dbo.CusContainer
           ([CO_PK]
           ,[CO_ContainerNumber]
           ,[CO_Seal]
           ,[CO_FCL_LCL_AIR]
           ,[CO_ContainerSize]
           ,[CO_ContainerUQ]
           ,[CO_Weight]
           ,[CO_WeightUQ]
           ,[CO_AddInfo]
           ,[CO_CustomFlag1]
           ,[CO_CustomAttrib1]
           ,[CO_CustomDecimal1]
           ,[CO_JE]
           ,[CO_SecondSeal]
           ,[CO_MessageStatus]
           ,[CO_ClusterKey]
           ,[CO_DataModel])
     VALUES
           (NEWID()
           ,'CNT12456'
           ,''
           ,'{containerMode}'
           ,''
           ,''
           ,0
           ,''
           ,''
           ,''
           ,''
           ,0
           ,'{declarationPK}'
           ,''
           ,''
           ,'{clusterKey}'
           ,'ZZ')";
		}

		string GetInsertNoteGoodsHandlingInstuctions()
		{
			return $@"INSERT INTO dbo.StmNote
           ([ST_PK]
           ,[ST_ParentID]
           ,[ST_Table]
           ,[ST_Description]
           ,[ST_NoteData]
           ,[ST_NoteText]
           ,[ST_NoteType]
           ,[ST_NoteContext]
           ,[ST_IsCustomDescription]
           ,[ST_ForceRead])
     VALUES
           (NEWID()
           ,'{declarationPK}'
           ,'JobDeclaration'
           ,'Goods Handling Instructions'
           ,null
           ,'Goods handling instructions for this declaration'
           ,'PUB'
           ,'AAA'
           ,'0'
           ,'1')";
		}

		string GetInsertJobConShipLinkCommand(Guid consolPk, Guid shipmentPk)
		{
			return $@"INSERT INTO dbo.JobConShipLink
			([JN_PK]
			,[JN_JK]
			,[JN_JS])
		VALUES
			(NEWID()
			,'{consolPk}'
			,'{shipmentPk}')";
		}

		string GetInsertJobPackLinesCommand(Guid packLinePk, Guid shipmentPk)
		{
			return $@"INSERT INTO dbo.JobPackLines
			([JL_PK]
			,[JL_JS])
		VALUES
			('{packLinePk}',
			'{shipmentPk}')";
		}

		string GetInsertJobContainerPackPivotCommand(Guid containerPk, Guid packLinePk)
		{
			return $@"INSERT INTO dbo.JobContainerPackPivot
			([J6_PK]
			,[J6_JC]
			,[J6_JL])
		VALUES
			(NEWID(),
			'{containerPk}',
			'{packLinePk}')";
		}

		string GetInsertJobConsolCommand(Guid consol, string transportMode, string consolMode, Guid shippingLineAddressPk)
		{
			return $@"INSERT INTO dbo.JobConsol
			([JK_PK]
			,[JK_IsValid]
			,[JK_TransportMode]
			,[JK_AgentType]
			,[JK_IsForwarding]
			,[JK_ConsolMode]
			,[JK_IsCFS]
			,[JK_OA_ShippingLineAddress])
		VALUES
			('{consol}',
			1,
			'{transportMode}',
			'AGT',
			1,
			'{consolMode}',
			1,
			'{shippingLineAddressPk}')";
		}

		string GetInsertJobContainerCommand(Guid containerPk, int containerNum, bool overrideFCLAvailableStorage, Guid consolPk, Guid refContainerPk)
		{
			return $@"INSERT INTO dbo.JobContainer
			([JC_PK]
			,[JC_ContainerNum]
			,[JC_OverrideFCLAvailableStorage]
			,[JC_JK]
			,[JC_RC]
			,[JC_FCLAvailable]
			,[JC_LCLAvailable])
		VALUES
			('{containerPk}',
			'{containerNum}',
			{(overrideFCLAvailableStorage ? 1 : 0)},
			'{consolPk}',
			'{refContainerPk}',
			'2024/06/15',
			'2024/06/15')";
		}

		string GetInsertConsolTransportCommand(Guid parentGuid, string parentType, int order, Guid sailing, string atd, string ata)
		{
			return $@"INSERT INTO dbo.JobConsolTransport
			([JW_PK],
			 [JW_ParentGUID],
			[JW_ParentType],
			[JW_LegOrder],
			[JW_JX],
			[JW_ATD],
			[JW_ATA])
		VALUES
			(NEWID(),
			'{parentGuid}',
			'{parentType}',
			{order},
			'{sailing}',
			'{atd}',
			'{ata}')";
		}

		string GetInsertJobDocAddressCommand(Guid orgAddress, Guid jobShipment)
		{
			return $@"INSERT INTO dbo.JobDocAddress
			([E2_PK],
			[E2_OA_Address],
			[E2_ParentID],
			[E2_ParentTableCode],
			[E2_AddressType])
		VALUES
			(NEWID(),
			'{orgAddress}',
			'{jobShipment}',
			'JS',
			'CRD')";
		}

		string GetInsertOrgAddressCommand(Guid client, Guid orgAddress)
		{
			return $@"INSERT INTO dbo.OrgAddress
			([OA_PK],
			[OA_OH],
			[OA_Address1])
		VALUES
			('{orgAddress}',
			'{client}',
			'Address 1')";
		}

		string GetInsertJobShipmentCommand(Guid adlShipment, string transportMode, string packingMode)
		{
			return $@"INSERT INTO dbo.JobShipment
			([JS_PK],
			[JS_UniqueConsignRef],
			[JS_IsCancelled],
			[JS_IsShipping],
			[JS_TransportMode],
			[JS_PackingMode])
		VALUES
			('{adlShipment}',
			'S00001005',
			0,
			0,
			'{transportMode}',
			'{packingMode}')";
		}

		string GetInsertJobDocsAndCartageCommand(Guid shipmentPK)
		{
			return $@"INSERT INTO dbo.JobDocsAndCartage
			([JP_PK],
			[JP_IsValid],
			[JP_FCLDeliveryEquipmentNeeded],
			[JP_ParentID],
			[JP_ParentTableCode],
			[JP_FCLAvailable],
			[JP_LCLAvailable])
		VALUES
			(NEWID(),
			'1',
			'PSL',
			'{shipmentPK}',
			'JS',
			'2024/06/15',
			'2024/06/15')";
		}

		string GetInsertJobCO2eCommand(Guid adlShipment, decimal co2)
		{
			return $@"INSERT INTO dbo.JobCO2e
			([JCO_PK],
			[JCO_ParentID],
			[JCO_ParentTableCode],
			[JCO_Status],
			[JCO_CO2ePerTonneInKg],
			[JCO_TotalCO2e],
			[JCO_SystemCreateTimeUtc],
			[JCO_SystemCreateUser],
			[JCO_SystemLastEditTimeUtc],
			[JCO_SystemLastEditUser])
		VALUES
			(NEWID(),
			'{adlShipment}',
			'JS',
			'CUR',
			20,
			{co2},
			GETUTCDATE(),
			'~BP',
			GETUTCDATE(),
			'~BP')";
		}

		void InsertTestData(Guid importerPK, int clusterKey)
		{
			TestConnection.ExecuteNonQuery(GetInsertVoyageCommand());
			TestConnection.ExecuteNonQuery(GetInsertVoyageOriginCommand(voyOrigin1PK, jobVoyagePK1, "2018-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageOriginCommand(voyOrigin2PK, jobVoyagePK2, "2018-10-16 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageDestinationCommand(voyDestination1PK, jobVoyagePK1, "2018-10-15 00:00:00", "2010-10-16 07:51:00"));
			TestConnection.ExecuteNonQuery(GetInsertVoyageDestinationCommand(voyDestination2PK, jobVoyagePK2, "2018-10-16 00:00:00", "2010-10-16 07:51:00"));
			TestConnection.ExecuteNonQuery(GetInsertSailingCommand(jobSailingPK1, voyOrigin1PK, voyDestination1PK));
			TestConnection.ExecuteNonQuery(GetInsertSailingCommand(jobSailingPK2, voyOrigin2PK, voyDestination2PK));
			TestConnection.ExecuteNonQuery(GetInsertDeclarationCommand(importerPK, clusterKey, "IMP", "2010-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertContainerCommand("FCL", clusterKey));
			TestConnection.ExecuteNonQuery(GetInsertNoteGoodsHandlingInstuctions());
			TestConnection.ExecuteNonQuery(GetInsertConsolTransportCommand(declarationPK, "DEC", 1, jobSailingPK1, "2018-10-15 00:00:00", "2018-10-15 00:00:00"));
			TestConnection.ExecuteNonQuery(GetInsertConsolTransportCommand(declarationPK, "DEC", 2, jobSailingPK2, "2018-10-16 00:00:00", "2018-10-16 00:00:00"));
		}

		void InsertShipmentTestData(Guid clientPK)
		{
			TestConnection.ExecuteNonQuery(GetInsertOrgAddressCommand(clientPK, orgAddressPK));
			TestConnection.ExecuteNonQuery(GetInsertJobShipmentCommand(adlShipmentPK, "SEA", "FCL"));
			TestConnection.ExecuteNonQuery(GetInsertJobDocAddressCommand(orgAddressPK, adlShipmentPK));
		}
	}
}

