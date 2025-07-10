using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(TransportBookingIntelligence))]
	sealed class TransportBookingIntelligenceTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;
		protected override void PrepareTestData()
		{
			var sqlText = @"
			DECLARE @utcNow smalldatetime = GetUtcDate(); 
			DECLARE @UserCode1 NVARCHAR(50) = 'US1';

			DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
			DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
			DECLARE @GePk UNIQUEIDENTIFIER = (SELECT TOP(1) GE_PK FROM dbo.GlbDepartment);

			INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
				(NEWID(), 'SY1', @GcPk),
				(NEWID(), 'TK1', @GcPk);

			--JobShipment
			DECLARE @JSPk01 UNIQUEIDENTIFIER = newid();

			--DtbBookingConsolidation
			DECLARE @KBPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @KBPk02 UNIQUEIDENTIFIER = newid();

			--DtbBooking
			DECLARE @KMPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @KMPk02 UNIQUEIDENTIFIER = newid();

			--DtbBookingInstruction
			DECLARE @KNPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @KNPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @KNPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @KNPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @KNPk05 UNIQUEIDENTIFIER = newid();
			DECLARE @KNPk06 UNIQUEIDENTIFIER = newid();

			--DtbBookingConfirmation
			DECLARE @KKPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @KKPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @KKPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @KKPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @KKPk05 UNIQUEIDENTIFIER = newid();
			DECLARE @KKPk06 UNIQUEIDENTIFIER = newid();
			DECLARE @KKPk07 UNIQUEIDENTIFIER = newid();

			--OrgHeader
			DECLARE @OHPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @OHPk05 UNIQUEIDENTIFIER = newid();

			--OrgAddress
			DECLARE @OAPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk04 UNIQUEIDENTIFIER = newid();
			DECLARE @OAPk05 UNIQUEIDENTIFIER = newid();

			--JobDocAddress
			DECLARE @E2Pk01 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk02 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk03 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk04 UNIQUEIDENTIFIER = newid();
			DECLARE @E2Pk05 UNIQUEIDENTIFIER = newid();

			--DtbBookingInstructionPkgDivot
			DECLARE @BPPK01 UNIQUEIDENTIFIER = newid();
			DECLARE @BPPK02 UNIQUEIDENTIFIER = newid();
			DECLARE @BPPK03 UNIQUEIDENTIFIER = newid();
			DECLARE @BPPK04 UNIQUEIDENTIFIER = newid();
			DECLARE @BPPK05 UNIQUEIDENTIFIER = newid();

			--PkgPackage
			DECLARE @PPPk01 UNIQUEIDENTIFIER = newid();
			DECLARE @PPPk02 UNIQUEIDENTIFIER = newid();
			DECLARE @PPPk03 UNIQUEIDENTIFIER = newid();
			DECLARE @PPPk04 UNIQUEIDENTIFIER = newid();

			--PkgPackageJob
			DECLARE @PJPK01 UNIQUEIDENTIFIER = newid();

			--PkgPackageContainer
			DECLARE @PCPK01 UNIQUEIDENTIFIER = newid();
			DECLARE @PCPK02 UNIQUEIDENTIFIER = newid();
			DECLARE @PCPK03 UNIQUEIDENTIFIER = newid();

			--RefContainer
			DECLARE @RCPK01 UNIQUEIDENTIFIER = (SELECT RC_PK from dbo.RefContainer where RC_Code = '20GP');
			DECLARE @RCPK02 UNIQUEIDENTIFIER = (SELECT RC_PK from dbo.RefContainer where RC_Code = '40GP');

			INSERT INTO [dbo].[JobShipment] (JS_PK,JS_UniqueConsignRef,JS_ShipmentType,JS_TransportMode,JS_PackingMode,JS_RL_NKOrigin,JS_RL_NKDestination) VALUES
			(@JSPk01,N'JsUniqueConsignRef01',N'STD',N'SEA',N'FCL',N'CNSAA',N'AUS2E');

			INSERT INTO [dbo].[DtbBookingConsolidation] ([KB_PK], [KB_JobType], [KB_JobID],[KB_ParentID], [KB_ParentTableCode] ,[KB_SystemCreateTimeUtc], [KB_SystemCreateUser], [KB_SystemLastEditTimeUtc], [KB_SystemLastEditUser])
			VALUES
			(@KBPk01, 'BKG', 'CM00000023',@JSPK01, 'JS', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KBPk02, 'BKG', 'CM00000024',null, '', @utcNow, @UserCode1, @utcNow, @UserCode1);

			-- Booking with a booking template attached
			INSERT INTO [dbo].[DtbBooking] ([KM_PK], [KM_JobID], [KM_KB_Booking], [KM_RatingFreightMode], [KM_GB_Branch], [KM_SystemCreateTimeUtc], [KM_SystemCreateUser], [KM_SystemLastEditTimeUtc], [KM_SystemLastEditUser], [KM_TransportMode], [KM_KT_NKBookingTemplate])
			VALUES
			(@KMPk01, 'TB00000022', @KBPk01, 'BTH', @GbPk, '2024-10-01 10:01:00', @UserCode1, '2024-10-01 11:20:00', @UserCode1, 'ROA', 'DLCW'),
			(@KMPk02, 'TB00000023', @KBPk02, 'BTH', @GbPk, '2024-10-05 07:50:00', @UserCode1, '2024-10-06 07:50:00', @UserCode1, 'ROA', '');

			INSERT INTO [dbo].[DtbBookingInstruction]
			([KN_PK], [KN_Sequence], [KN_InstructionType], [KN_Status], [KN_KM_BookingMovement], [KN_SystemCreateTimeUtc], [KN_SystemCreateUser], [KN_SystemLastEditTimeUtc], [KN_SystemLastEditUser])
			VALUES
			(@KNPk01, 1, 'PIC', 'PIC', @KMPk01, '2024-10-09 11:55:00', @UserCode1, '2024-10-09 11:55:00', @UserCode1),
			(@KNPk02, 2, 'PIC', 'PIC', @KMPk01, '2024-10-09 11:55:00', @UserCode1, '2024-10-09 11:55:00', @UserCode1),
			(@KNPk03, 3, 'MLT', 'AVL', @KMPk01, '2024-10-09 11:55:00', @UserCode1, '2024-10-09 11:55:00', @UserCode1),
			(@KNPk04, 4, 'DLV', 'AVL', @KMPk01, '2024-10-09 11:55:00', @UserCode1, '2024-10-09 11:55:00', @UserCode1),
			(@KNPk05, 1, 'PIC', 'PIC', @KMPk02, '2024-10-10 11:55:00', @UserCode1, '2024-10-10 11:55:00', @UserCode1),
			(@KNPk06, 2, 'PIC', 'PIC', @KMPk02, '2024-11-10 11:55:00', @UserCode1, '2024-11-10 11:55:00', @UserCode1); -- out of date

			INSERT INTO [dbo].[DtbBookingConfirmation] 
			([KK_PK], [KK_ConfirmationType], [KK_Estimated], [KK_EstimatedUtc], [KK_Actual], [KK_RequiredFrom], [KK_RequiredFromUtc], [KK_RequiredTo], [KK_RequiredToUtc], [KK_KN_BookingInstruction], [KK_SystemCreateTimeUtc], [KK_SystemCreateUser], [KK_SystemLastEditTimeUtc], [KK_SystemLastEditUser])
			VALUES
			(@KKPk01, 'PIC', '2024-10-11 10:55:00', '2024-10-10 23:55:00', '2024-10-16 17:11:00', '2024-10-10 17:11:00', '2024-10-10 06:11:00', '2024-10-17 17:11:00', '2024-10-17 06:11:00', @KNPk01, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KKPk02, 'PIC', '2024-10-10 10:55:00', '2024-10-10 09:55:00', '2024-10-18 11:10:00', '2024-10-11 11:10:00', '2024-10-11 10:10:00', '2024-10-12 11:09:00', '2024-10-12 10:09:00', @KNPk02, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KKPk03, 'DLV', '2024-10-12 10:56:00', '2024-10-12 09:56:00', null, null, null, null, null, @KNPk03, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KKPk04, 'PIC', '2024-10-12 10:56:00', '2024-10-12 09:56:00', null, null, null, null, null, @KNPk03, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KKPk05, 'DLV', '2024-10-13 10:56:00', '2024-10-13 09:56:00', null, null, null, null, null, @KNPk04, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KKPk06, 'PIC', '2024-10-13 10:56:00', '2024-10-13 09:56:00', null, null, null, null, null, @KNPk05, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KKPk07, 'DLV', '2024-10-14 10:56:00', '2024-10-14 09:56:00', null, null, null, null, null, @KNPk06, @utcNow, @UserCode1, @utcNow, @UserCode1);

			INSERT INTO [dbo].[OrgHeader] ([OH_PK], [OH_Code])
			VALUES
			(@OHPk01, 'OH1'),
			(@OHPk02, 'OH2'),
			(@OHPk03, 'OH3'),
			(@OHPk04, 'OH4'),
			(@OHPk05, 'OH5');

			INSERT INTO [dbo].[OrgAddress] ([OA_PK], [OA_OH], [OA_Code], [OA_Address1], [OA_City], [OA_State], [OA_PostCode], [OA_RN_NKCountryCode], OA_SystemCreateTimeUtc, [OA_GeoLocation])
			VALUES
			(@OAPk01, @OHPk01, 'PST: 10 HUTCHESON Street', '10 BEDFORD SQUARE', 'Sydney', 'LND', '1900', 'AU', @utcNow, geography::Point(47.626, -122.360, 4326)),
			(@OAPk02, @OHPk02, 'PST: 12 YORKSHIRE Street', '12 WEST YORKSHIRE', 'Como', 'QLD', '2000', 'AU', @utcNow, geography::Point(47.616, -122.360, 4326)),
			(@OAPk03, @OHPk03, 'PST: 13 HUTCHESON Street', '13 BEDFORD SQUARE', '', 'LND', '2001', 'AU', @utcNow, geography::Point(47.652, -122.363, 4326)),
			(@OAPk04, @OHPk04, 'PST: 14 HUTCHESON Street', '14 BEDFORD SQUARE', '', 'LND', '2002', 'AU', @utcNow, geography::Point(47.646, -122.360, 4326)),
			(@OAPk05, @OHPk05, 'PST: 15 HUTCHESON Street', '15 BEDFORD SQUARE', '', 'LND', '2003', 'AU', @utcNow, geography::Point(47.656, -122.360, 4326));

			INSERT INTO [dbo].[JobDocAddress] ([E2_PK], [E2_ParentID], [E2_ParentTableCode], [E2_AddressType], [E2_City], [E2_State], [E2_RN_NKCountryCode], [E2_OA_Address], [E2_AddressOverride], [E2_ValidationStatus], [E2_CompanyName], [E2_SystemCreateTimeUtc], [E2_SystemCreateUser], [E2_SystemLastEditTimeUtc], [E2_SystemLastEditUser])
			VALUES
			(@E2Pk01, @KMPK01, 'KM', 'TRA', 'Edgecliff', 'NSW', 'AU', @OAPK01, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@E2Pk02, @KNPK01, 'KN', 'LCF', '', '', 'AU', @OAPK02, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@E2Pk03, @KNPK02, 'KN', 'LCF', 'Caringbah', 'NSW', 'AU', @OAPK03, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@E2Pk04, @KNPK03, 'KN', 'LCY', 'Wentworth Point', 'NSW', 'AU', @OAPK04, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@E2Pk05, @KNPK04, 'KN', 'LCY', 'Waterloo', 'NSW', 'AU', @OAPK05, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1);

			INSERT INTO [dbo].[PkgPackageJob] ([KJ_PK], [KJ_JobID], [KJ_ParentID], [KJ_ParentTableCode], [KJ_ReleasedTimeUtc], [KJ_GS_NKReleasedBy], [KJ_IsFinalized], [KJ_AutoVersion], [KJ_SystemLastEditTimeUtc], [KJ_SystemLastEditUser], [KJ_SystemCreateTimeUtc], [KJ_SystemCreateUser])
			VALUES (@PJPK01, N'P0186', newid(), N'LTC', NULL, N'', 0, 0, CAST(N'2022-09-07T05:43:00' AS SMALLDATETIME), N'TST', CAST(N'2022-09-07T05:43:00' AS SMALLDATETIME), N'TST');

			INSERT INTO [dbo].[PkgPackage] ([KP_PK], [KP_F3_NKPackType], [KP_PackageQty], [KP_Length], [KP_Width], [KP_Height], [KP_DimensionUQ], [KP_Weight], [KP_WeightUQ], [KP_Volume], [KP_RH_NKCommodityCode], [KP_VolumeUQ], [KP_MarksAndNumbers],  [KP_TransportRef], [KP_HSCode], [KP_RequiresTemperatureControl],  [KP_RequiredTemperatureMinimum], [KP_RequiredTemperatureMaximum], [KP_RequiredTemperatureUnit], [KP_ClosedTimeUtc],[KP_GS_NKClosedBy], [KP_IsReleasedViaJob], [KP_ReleasedTimeUtc], [KP_GS_NKReleasedBy], [KP_KP_ParentPackage], [KP_KJ_ParentPackageJob], [KP_PreviousPackageID], [KP_Sequence], [KP_KPH_PackageHeader], [KP_IsDamaged], [KP_IsHeld], [KP_IsCheckedWeighedCubed], [KP_DamagedReason], [KP_AutoVersion],  [KP_ExternalReference], [KP_IsPillaged], [KP_KP_TopHandlingUnitPackage], [KP_GoodsDescription], [KP_SystemCreateTimeUtc], [KP_SystemCreateUser], [KP_SystemLastEditTimeUtc], [KP_SystemLastEditUser], [KP_DunnageWeight], [KP_TareWeight], [KP_MarksAndNumbersVersion])
			VALUES 
			(@PPPk01, N'CNT', 4, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(1500.000 AS DECIMAL(9, 3)), N'KG', CAST(3.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 1, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @PJPK01, N'', 1, NULL, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0),	
			(@PPPk02, N'CNT', 2, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(238.000 AS DECIMAL(9, 3)), N'KG', CAST(2.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 0, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @PJPK01, N'', 1, NULL, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0),
			(@PPPk03, N'PLT', 1, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(1500.000 AS DECIMAL(9, 3)), N'KG', CAST(3.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 0, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @PJPK01, N'', 1, NULL, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0),
			(@PPPk04, N'CNT', 1, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(1500.000 AS DECIMAL(9, 3)), N'KG', CAST(3.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 0, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @PJPK01, N'', 1, NULL, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0);

			INSERT INTO [dbo].[DtbBookingInstructionPkgDivot]([KD_PK], [KD_Quantity], [KD_KP_Package], [KD_KN_BookingInstruction], [KD_AutoVersion],[KD_SystemCreateTimeUtc], [KD_SystemCreateUser],[KD_SystemLastEditTimeUtc], [KD_SystemLastEditUser])
			VALUES
			(@BPPK01, 2, @PPPk01, @KNPk01, 0, GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1 ),
			(@BPPK02, 1, @PPPk02, @KNPk01, 0, GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1 ),
			(@BPPK03, 1, @PPPk03, @KNPk01, 0, GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1 ),
			(@BPPK04, 1, @PPPk04, @KNPk02, 0, GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1 ),
			(@BPPK05, 1, @PPPk02, @KNPk02, 0, GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1 );

			INSERT INTO [dbo].[PkgPackageContainer]([K0_PK], [K0_KP_Package], [K0_RC_ContainerType], [K0_ContainerMode],[K0_SystemCreateTimeUtc], [K0_SystemCreateUser],[K0_SystemLastEditTimeUtc], [K0_SystemLastEditUser])
			VALUES
			(@PCPK01, @PPPk01, @RCPK01, 'FCL',GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1),
			(@PCPK02, @PPPk02, @RCPK02, 'FCL', GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1),
			(@PCPK03, @PPPk04, @RCPK02, 'LCL', GETUTCDATE(), @UserCode1, GETUTCDATE(), @UserCode1);
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			var tb1Transactions = transactions.Where(x => x.Reference1 == "TB00000022");
			AssertEquals("Number of Transactions for TB00000022", 5, tb1Transactions.Count());
			AssertEquals("Number of Transactions for TB00000022 with Reference3 equal to JS", 5, tb1Transactions.Count(x => x.Reference3 == "JS"));

			// instruction with id - KNPK03
			VerifyTransaction(tb1Transactions, new Dictionary<string, object>
			{
				{ "Estimated", "2024-10-12T10:56:00" },
				{ "TransportCompany", "OH1" },
				{ "PostalCode", "2002" },
				{ "City", "Wentworth Point" },
				{ "State", "NSW" },
				{ "Country", "AU" },
				{ "CreatedDate", "2024-10-01T10:01:00" },
				{ "Direction", "DST" },
				{ "Geoloc", "POINT (-122.36 47.646)" },
				{ "IsFirstInstruction", "N" },
				{ "IsLastInstruction", "N" },
				{ "OrganisationType", "LCY" },
				{ "ConfirmationType", "DLV" }
			});

			// instruction with id - KNPK03
			VerifyTransaction(tb1Transactions, new Dictionary<string, object>
			{
				{ "Estimated", "2024-10-12T10:56:00" },
				{ "TransportCompany", "OH1" },
				{ "PostalCode", "2002" },
				{ "City", "Wentworth Point" },
				{ "State", "NSW" },
				{ "Country", "AU" },
				{ "CreatedDate", "2024-10-01T10:01:00" },
				{ "Direction", "DST" },
				{ "Geoloc", "POINT (-122.36 47.646)" },
				{ "IsFirstInstruction", "N" },
				{ "IsLastInstruction", "N" },
				{ "OrganisationType", "LCY" },
				{ "ConfirmationType", "PIC" }
			});

			// instruction with id - KNPK04
			VerifyTransaction(tb1Transactions, new Dictionary<string, object>
			{
				{ "Estimated", "2024-10-13T10:56:00" },
				{ "TransportCompany", "OH1" },
				{ "PostalCode", "2003" },
				{ "City", "Waterloo" },
				{ "State", "NSW" },
				{ "Country", "AU" },
				{ "CreatedDate", "2024-10-01T10:01:00" },
				{ "Direction", "DST" },
				{ "Geoloc", "POINT (-122.36 47.656)" },
				{ "IsFirstInstruction", "N" },
				{ "IsLastInstruction", "Y" },
				{ "OrganisationType", "LCY" },
				{ "ConfirmationType", "DLV" }
			});

			// instruction with id - KNPK01
			VerifyTransaction(tb1Transactions, new Dictionary<string, object>
			{
				{ "Estimated", "2024-10-11T10:55:00" },
				{ "Actual", "2024-10-16T17:11:00" },
				{ "RequiredFrom", "2024-10-10T17:11:00" },
				{ "RequiredTo", "2024-10-17T17:11:00" },
				{ "TransportCompany", "OH1" },
				{ "PostalCode", "2000" },
				{ "City", "Como" },
				{ "State", "QLD" },
				{ "Country", "AU" },
				{ "CreatedDate", "2024-10-01T10:01:00" },
				{ "Direction", "DST" },
				{ "Geoloc", "POINT (-122.36 47.616)" },
				{ "IsFirstInstruction", "Y" },
				{ "IsLastInstruction", "N" },
				{ "OrganisationType", "LCF" },
				{ "PacklineCount", 3 },
				{ "ContainerCount", 3 },
				{ "OtherPackageTypeCount", 1 },
				{ "ContainerMode", "FCL,FCL" },
				{ "ContainerType", "20GP,40GP" },
				{ "ConfirmationType", "PIC" }
			});

			VerifyTransaction(tb1Transactions, new Dictionary<string, object>
			{
				{ "Estimated", "2024-10-10T10:55:00" },
				{ "Actual", "2024-10-18T11:10:00" },
				{ "RequiredFrom", "2024-10-11T11:10:00" },
				{ "RequiredTo", "2024-10-12T11:09:00" },
				{ "TransportCompany", "OH1" },
				{ "PostalCode", "2001" },
				{ "City", "Caringbah" },
				{ "State", "NSW" },
				{ "Country", "AU" },
				{ "CreatedDate", "2024-10-01T10:01:00" },
				{ "Direction", "DST" },
				{ "Geoloc", "POINT (-122.363 47.652)" },
				{ "IsFirstInstruction", "N" },
				{ "IsLastInstruction", "N" },
				{ "OrganisationType", "LCF" },
				{ "PacklineCount", 2 },
				{ "ContainerCount", 2 },
				{ "OtherPackageTypeCount", 0 },
				{ "ContainerMode", "FCL,LCL" },
				{ "ContainerType", "40GP,40GP" },
				{ "ConfirmationType", "PIC" }
			});

			var tb2Transactions = transactions.Where(x => x.Reference1 == "TB00000023");
			AssertEquals("Number of Transactions for TB00000023", 1, tb2Transactions.Count());
			AssertNull(tb2Transactions.First().Reference3); // No parent table code

			VerifyTransaction(tb2Transactions, new Dictionary<string, object>
			{
				{ "Estimated", "2024-10-13T10:56:00" },
				{ "IsFirstInstruction", "Y" },
				{ "IsLastInstruction", "N" },
				{ "ConfirmationType", "PIC" }
			});
		}
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 10);
		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}

		void VerifyTransaction(IEnumerable<IStlTransaction> transactions, Dictionary<string, object> keyValuePairs)
		{
			var expectedValues = BuildExpectedValues(keyValuePairs);
			var expectedValuesString = ConvertDictionaryToString(expectedValues);
			var transaction = transactions.Single(x => x.AdditionalRefs == $"{{{expectedValuesString}}}");
			AssertNotNull(transaction);
		}

		Dictionary<string, object> BuildExpectedValues(Dictionary<string, object> keyValuePairs)
		{
			return new Dictionary<string, object>(keyValuePairs);
		}

		string ConvertDictionaryToString(Dictionary<string, object> dictionary)
		{
			return dictionary.Select(kv => $"\"{kv.Key}\":{(kv.Value is string ? $"\"{kv.Value}\"" : kv.Value)}")
							 .Aggregate((current, next) => current + "," + next);
		}
	}
}
