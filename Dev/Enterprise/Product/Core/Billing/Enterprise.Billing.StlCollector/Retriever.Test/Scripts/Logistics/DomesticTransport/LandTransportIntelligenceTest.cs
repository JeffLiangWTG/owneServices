using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportIntelligence))]
	sealed class LandTransportIntelligenceTest : RefStlScriptWithDefaultsTest
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

			-- Insert into GlbBranch
			INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES
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

			-- OrgHeader
			DECLARE @OHPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OHPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OHPk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OHPk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OHPk05 UNIQUEIDENTIFIER = NEWID();

			-- OrgAddress
			DECLARE @OAPk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OAPk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OAPk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OAPk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @OAPk05 UNIQUEIDENTIFIER = NEWID();

			-- JobDocAddress
			DECLARE @E2Pk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @E2Pk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @E2Pk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @E2Pk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @E2Pk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @E2Pk06 UNIQUEIDENTIFIER = NEWID();

			-- RunSheet
			DECLARE @KGpk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @KGpk02 UNIQUEIDENTIFIER = NEWID();

			-- RunSheet instruction
			DECLARE @K1pk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @K1pk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @K1pk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @K1pk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @K1pk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @K1pk06 UNIQUEIDENTIFIER = NEWID();

			-- Consignment
			DECLARE @LTCpk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTCpk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTCpk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTCpk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTCpk05 UNIQUEIDENTIFIER = NEWID();

			-- Consignment Address
			DECLARE @LTSpk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk06 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk07 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk08 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk09 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTSpk10 UNIQUEIDENTIFIER = NEWID();

			-- Consignment Action
			DECLARE @LTApk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk05 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk06 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk07 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk08 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk09 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTApk10 UNIQUEIDENTIFIER = NEWID();

			-- Consignment Leg
			DECLARE @LTGpk01 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTGpk02 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTGpk03 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTGpk04 UNIQUEIDENTIFIER = NEWID();
			DECLARE @LTGpk05 UNIQUEIDENTIFIER = NEWID();

			-- Insert into OrgHeader
			INSERT INTO dbo.OrgHeader (OH_PK, OH_Code) VALUES
			(@OHPk01, 'OH1'),
			(@OHPk02, 'OH2'),
			(@OHPk03, 'OH3'),
			(@OHPk04, 'OH4'),
			(@OHPk05, 'OH5');

			INSERT INTO [dbo].[JobShipment] (JS_PK,JS_UniqueConsignRef,JS_ShipmentType,JS_TransportMode,JS_PackingMode,JS_RL_NKOrigin,JS_RL_NKDestination) VALUES
			(@JSPk01,N'JsUniqueConsignRef01',N'STD',N'SEA',N'FCL',N'CNSAA',N'AUS2E');

			INSERT INTO [dbo].[DtbBookingConsolidation] ([KB_PK], [KB_JobType], [KB_JobID],[KB_ParentID], [KB_ParentTableCode] ,[KB_SystemCreateTimeUtc], [KB_SystemCreateUser], [KB_SystemLastEditTimeUtc], [KB_SystemLastEditUser])
			VALUES
			(@KBPk01, 'BKG', 'CM00000023',@JSPK01, 'JS', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KBPk02, 'BKG', 'CM00000024',null, '', @utcNow, @UserCode1, @utcNow, @UserCode1);

			INSERT INTO [dbo].[DtbBooking] ([KM_PK], [KM_JobID], [KM_KB_Booking], [KM_RatingFreightMode], [KM_GB_Branch], [KM_SystemCreateTimeUtc], [KM_SystemCreateUser], [KM_SystemLastEditTimeUtc], [KM_SystemLastEditUser], [KM_TransportMode])
			VALUES
			(@KMPk01, 'TB00000022', @KBPk01, 'BTH', @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1, 'ROA');

			-- Insert into DtbConsignmentRunSheet
			INSERT INTO dbo.DtbConsignmentRunSheet (KG_PK, KG_RunSheetNumber, KG_OH_TransportCo, KG_TransportMode, KG_ContainerMode, KG_GB_Branch, KG_SystemCreateTimeUtc, KG_SystemCreateUser, KG_SystemLastEditTimeUtc, KG_SystemLastEditUser)
			VALUES 
			(@KGpk01, 'CR00000001', @OHPk01, 'ROA', 'FCL', @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@KGpk02, 'CR00000002', @OHPk01, 'ROA', 'FTL', @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1);

			-- Insert into DtbConsignmentRunSheetInstruction
			INSERT INTO dbo.DtbConsignmentRunSheetInstruction (K1_PK, K1_KG_RunSheet, K1_Sequence, K1_IsAcceptedByDriver, K1_EstimatedTimeIn, K1_EstimatedTimeOut, K1_TimeIn, K1_TimeOut, K1_SystemCreateTimeUtc, K1_SystemCreateUser, K1_SystemLastEditTimeUtc, K1_SystemLastEditUser)
			VALUES 
			(@K1pk01, @KGpk01, 1, 1, CAST(N'2024-12-01 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-02 14:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-01 15:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-02 16:09:00.0000000 +11:00' AS DATETIMEOFFSET), '2024-10-24 13:29:00', @UserCode1,'2024-10-24 13:29:00', @UserCode1),
			(@K1pk02, @KGpk01, 2, 1, CAST(N'2024-12-02 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-03 14:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-02 15:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-03 16:09:00.0000000 +11:00' AS DATETIMEOFFSET), '2024-10-24 13:29:00', @UserCode1,'2024-10-24 13:29:00', @UserCode1),
			(@K1pk03, @KGpk01, 3, 1, CAST(N'2024-12-03 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-04 14:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-03 15:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-04 16:09:00.0000000 +11:00' AS DATETIMEOFFSET), '2024-10-24 13:29:00', @UserCode1,'2024-10-24 13:29:00', @UserCode1),
			(@K1pk04, @KGpk01, 4, 1, CAST(N'2024-12-04 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-05 14:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-04 15:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-05 16:09:00.0000000 +11:00' AS DATETIMEOFFSET), '2024-10-24 13:29:00', @UserCode1,'2024-10-24 13:29:00', @UserCode1),
			(@K1pk05, @KGpk02, 1, 1, CAST(N'2024-12-05 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-06 14:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-05 15:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-06 16:09:00.0000000 +11:00' AS DATETIMEOFFSET), '2024-10-24 13:29:00', @UserCode1,'2024-10-24 13:29:00', @UserCode1),
			(@K1pk06, @KGpk02, 2, 1, CAST(N'2024-12-06 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-07 14:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-06 15:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-12-07 16:09:00.0000000 +11:00' AS DATETIMEOFFSET), '2024-10-24 13:29:00', @UserCode1,'2024-10-24 13:29:00', @UserCode1);

			-- Insert into DtbConsignment
			INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_JobID, LTC_JobType, LTC_Status, LTC_Direction, LTC_ConsignmentType, LTC_KM_Booking, LTC_GB_Branch, LTC_SystemCreateTimeUtc, LTC_SystemCreateUser, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser)
			VALUES 
			(@LTCpk01, 'CN00000001', 'LTL', 'BKD', 'LOC', 'CBC', NULL, @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTCpk02, 'CN00000002', 'LTL', 'SVM', 'LOC', 'CBC', @KMPk01, @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTCpk03, 'CN00000003', 'LTL', 'SVM', 'LOC', 'CBC', NULL, @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTCpk04, 'CN00000004', 'LTL', 'SVM', 'LOC', 'CBC', NULL, @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTCpk05, 'CN00000005', 'LTL', 'BKD', 'LOC', 'CBC', NULL, @GbPk, @utcNow, @UserCode1, @utcNow, @UserCode1);

			-- Insert into DtbConsignmentAddress
			INSERT INTO dbo.DtbConsignmentAddress (LTS_PK, LTS_LTC_Consignment, LTS_InstructionType, LTS_Sequence, LTS_Status, LTS_SystemCreateTimeUtc, LTS_SystemCreateUser, LTS_SystemLastEditTimeUtc, LTS_SystemLastEditUser)
			VALUES 
			(@LTSpk01, @LTCpk01, 'PIC', 1, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk02, @LTCpk01, 'DLV', 2, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk03, @LTCpk02, 'PIC', 1, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk04, @LTCpk02, 'DLV', 2, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk05, @LTCpk03, 'PIC', 1, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk06, @LTCpk03, 'DLV', 2, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk07, @LTCpk04, 'PIC', 1, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk08, @LTCpk04, 'DLV', 2, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk09, @LTCpk05, 'PIC', 1, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTSpk10, @LTCpk05, 'DLV', 2, 'INC', @utcNow, @UserCode1, @utcNow, @UserCode1);

			-- Insert into DtbConsignmentAction
			INSERT INTO dbo.DtbConsignmentAction (LTA_PK, LTA_ActionID, LTA_ActionType, LTA_LTS_ConsignmentAddress, LTA_K1_RunSheetInstruction, LTA_ActualTime, LTA_EstimatedTime, LTA_RequiredFrom, LTA_RequiredTo, LTA_SystemCreateTimeUtc, LTA_SystemCreateUser, LTA_SystemLastEditTimeUtc, LTA_SystemLastEditUser)
			VALUES 
			(@LTApk01, 'LTA00000001', N'PIC', @LTSpk01, NULL, NULL, NULL, CAST(N'2024-11-06 14:10:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-07 13:05:00.0000000 +11:00' AS DATETIMEOFFSET), @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk02, 'LTA00000002', N'DLV', @LTSpk02, NULL, NULL, NULL, CAST(N'2024-11-08 16:20:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-09 15:10:00.0000000 +11:00' AS DATETIMEOFFSET), @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk03, 'LTA00000003', N'DLV', @LTSpk06, @K1pk06, NULL, NULL, CAST(N'2024-11-06 13:02:00.0000000 +11:00' AS DATETIMEOFFSET), NULL, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk04, 'LTA00000004', N'PIC', @LTSpk05, @K1pk05, NULL, NULL, CAST(N'2024-11-05 13:05:00.0000000 +11:00' AS DATETIMEOFFSET), NULL, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk05, 'LTA00000005', N'PIC', @LTSpk09, @K1pk01, CAST(N'2024-11-05 12:05:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-01 13:09:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-05 00:00:00.0000000 +11:00' AS DATETIMEOFFSET), NULL, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk06, 'LTA00000006', N'PIC', @LTSpk03, @K1pk02, NULL, NULL, CAST(N'2024-11-04 16:00:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-04 16:00:00.0000000 +11:00' AS DATETIMEOFFSET), @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk07, 'LTA00000007', N'DLV', @LTSpk04, @K1pk04, NULL, NULL, CAST(N'2024-11-06 00:00:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-06 00:00:00.0000000 +11:00' AS DATETIMEOFFSET), @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk08, 'LTA00000008', N'DLV', @LTSpk10, @K1pk03, NULL, CAST(N'2024-11-13 23:05:00.0000000 +11:00' AS DATETIMEOFFSET), CAST(N'2024-11-07 14:11:00.0000000 +11:00' AS DATETIMEOFFSET), NULL, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk09, 'LTA00000009', N'DLV', @LTSpk08, NULL, NULL, NULL, CAST(N'2024-11-07 00:00:00.0000000 +11:00' AS DATETIMEOFFSET), NULL, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTApk10, 'LTA00000010', N'PIC', @LTSpk07, NULL, NULL, NULL, CAST(N'2024-11-06 00:00:00.0000000 +11:00' AS DATETIMEOFFSET), NULL, @utcNow, @UserCode1, @utcNow, @UserCode1);

			-- Insert into DtbConsignmentLeg
			INSERT INTO dbo.DtbConsignmentLeg (LTG_PK, LTG_LTC_Consignment, LTG_LTA_Pickup, LTG_LTA_Delivery, LTG_Sequence, LTG_SystemCreateTimeUtc, LTG_SystemCreateUser, LTG_SystemLastEditTimeUtc, LTG_SystemLastEditUser)
			VALUES 
			(@LTGpk01, @LTCpk03, @LTApk06, @LTApk07, 1, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTGpk02, @LTCpk05, @LTApk10, @LTApk09, 1, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTGpk03, @LTCpk01, @LTApk01, @LTApk02, 1, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTGpk04, @LTCpk04, @LTApk05, @LTApk08, 1, @utcNow, @UserCode1, @utcNow, @UserCode1),
			(@LTGpk05, @LTCpk02, @LTApk04, @LTApk03, 1, @utcNow, @UserCode1, @utcNow, @UserCode1);

			-- Insert into OrgAddress
			INSERT INTO dbo.OrgAddress (OA_PK, OA_OH, OA_Code, OA_Address1, OA_City, OA_State, OA_PostCode, OA_RN_NKCountryCode, OA_SystemCreateTimeUtc)
			VALUES 
			(@OAPk01, @OHPk01, 'PST: 10 HUTCHESON Street', '10 BEDFORD SQUARE', 'Sydney', 'LND', '1900', 'AU', @utcNow),
			(@OAPk02, @OHPk02, 'PST: 12 YORKSHIRE Street', '12 WEST YORKSHIRE', 'Como', 'QLD', '2000', 'AU', @utcNow),
			(@OAPk03, @OHPk03, 'PST: 13 HUTCHESON Street', '13 BEDFORD SQUARE', '', 'LND', '2001', 'AU', @utcNow),
			(@OAPk04, @OHPk04, 'PST: 14 HUTCHESON Street', '14 BEDFORD SQUARE', 'Melbourne', 'VIC', '2002', 'AU', @utcNow),
			(@OAPk05, @OHPk05, 'PST: 15 HUTCHESON Street', '15 BEDFORD SQUARE', '', 'LND', '2003', 'AU', @utcNow);

			-- Insert into JobDocAddress
			INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_City, E2_State, E2_RN_NKCountryCode, E2_OA_Address, E2_AddressOverride, E2_ValidationStatus, E2_CompanyName, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser) VALUES
				(@E2Pk01, @LTSpk06, 'LTS', 'LCT', 'Edgecliff', 'NSW', 'AU', @OAPK01, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk02, @LTSpk05, 'LTS', 'LCI', '', '', 'AU', @OAPK02, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk03, @LTSpk09, 'LTS', 'LCY', '', '', 'AU', @OAPK03, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk04, @LTSpk03, 'LTS', 'LCI', '', '', 'AU', @OAPK04, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk05, @LTSpk04, 'LTS', 'LCI', 'Meadowbank', '', 'AU', @OAPK05, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1),
				(@E2Pk06, @LTSpk10, 'LTS', 'LCI', 'Brisbane', 'QLD', 'AU', @OAPK01, 0, 'NRQ', '', @utcNow, @UserCode1, @utcNow, @UserCode1);
";

			TestConnection.ExecuteNonQuery(sqlText);
		}
		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			var tb1Transactions = transactions.Where(x => x.Reference1 == "CN00000002");
			AssertEquals("Number of Transactions for CN00000002", 2, tb1Transactions.Count());

			VerifyTransaction(tb1Transactions, new Dictionary<string, string>
			{
				{ "EstimatedTimeIn", "2024-12-04T13:09:00+11:00" },
				{ "TimeIn", "2024-12-04T15:09:00+11:00" },
				{ "EstimatedTimeOut", "2024-12-05T14:09:00+11:00" },
				{ "TimeOut", "2024-12-05T16:09:00+11:00" },
				{ "JobType", "DLV" },
				{ "PickupRequiredFrom", "2024-11-05T13:05:00+11:00" },
				{ "DeliveryRequiredFrom", "2024-11-06T13:02:00+11:00" },
				{ "RunSheetNumber", "CR00000001" },
				{ "InstructionPostCode", "2003" },
				{ "InstructionCity", "Meadowbank" },
				{ "InstructionState", "LND" },
				{ "InstructionCountryCode", "AU" },
				{ "ParentEntityTB", "Y" }
			});

			VerifyTransaction(tb1Transactions, new Dictionary<string, string>
			{
				{ "EstimatedTimeIn", "2024-12-02T13:09:00+11:00" },
				{ "TimeIn", "2024-12-02T15:09:00+11:00" },
				{ "EstimatedTimeOut", "2024-12-03T14:09:00+11:00" },
				{ "TimeOut", "2024-12-03T16:09:00+11:00" },
				{ "JobType", "PIC" },
				{ "PickupRequiredFrom", "2024-11-05T13:05:00+11:00" },
				{ "DeliveryRequiredFrom", "2024-11-06T13:02:00+11:00" },
				{ "RunSheetNumber", "CR00000001" },
				{ "InstructionPostCode", "2002" },
				{ "InstructionCity", "Melbourne" },
				{ "InstructionState", "VIC" },
				{ "InstructionCountryCode", "AU" },
				{ "ParentEntityTB", "Y" }
			});

			var tb2Transactions = transactions.Where(x => x.Reference1 == "CN00000003");
			AssertEquals("Number of Transactions for CN00000003", 2, tb2Transactions.Count());

			VerifyTransaction(tb2Transactions, new Dictionary<string, string>
			{
				{ "EstimatedTimeIn", "2024-12-05T13:09:00+11:00" },
				{ "TimeIn", "2024-12-05T15:09:00+11:00" },
				{ "EstimatedTimeOut", "2024-12-06T14:09:00+11:00" },
				{ "TimeOut", "2024-12-06T16:09:00+11:00" },
				{ "JobType", "PIC" },
				{ "PickupRequiredTo", "2024-11-04T16:00:00+11:00" },
				{ "PickupRequiredFrom", "2024-11-04T16:00:00+11:00" },
				{ "DeliveryRequiredTo", "2024-11-06T00:00:00+11:00" },
				{ "DeliveryRequiredFrom", "2024-11-06T00:00:00+11:00" },
				{ "RunSheetNumber", "CR00000002" },
				{ "InstructionPostCode", "2000" },
				{ "InstructionCity", "Como" },
				{ "InstructionState", "QLD" },
				{ "InstructionCountryCode", "AU" },
				{ "ParentEntityTB", "N" }
			});

			VerifyTransaction(tb2Transactions, new Dictionary<string, string>
			{
			  { "EstimatedTimeIn", "2024-12-06T13:09:00+11:00" },
				{ "TimeIn", "2024-12-06T15:09:00+11:00" },
				{ "EstimatedTimeOut", "2024-12-07T14:09:00+11:00" },
				{ "TimeOut", "2024-12-07T16:09:00+11:00" },
				{ "JobType", "DLV" },
				{ "PickupRequiredTo", "2024-11-04T16:00:00+11:00" },
				{ "PickupRequiredFrom", "2024-11-04T16:00:00+11:00" },
				{ "DeliveryRequiredTo", "2024-11-06T00:00:00+11:00" },
				{ "DeliveryRequiredFrom", "2024-11-06T00:00:00+11:00" },
				{ "RunSheetNumber", "CR00000002" },
				{ "InstructionPostCode", "1900" },
				{ "InstructionCity", "Edgecliff" },
				{ "InstructionState", "NSW" },
				{ "InstructionCountryCode", "AU" },
				{ "ParentEntityTB", "N" }
			});

			var tb3Transactions = transactions.Where(x => x.Reference1 == "CN00000005");
			AssertEquals("Number of Transactions for CN00000005", 2, tb3Transactions.Count());

			VerifyTransaction(tb3Transactions, new Dictionary<string, string>
			{
				{ "EstimatedTimeIn", "2024-12-03T13:09:00+11:00" },
				{ "TimeIn", "2024-12-03T15:09:00+11:00" },
				{ "EstimatedTimeOut", "2024-12-04T14:09:00+11:00" },
				{ "TimeOut", "2024-12-04T16:09:00+11:00" },
				{ "JobType", "DLV" },
				{ "PickupRequiredFrom", "2024-11-06T00:00:00+11:00" },
				{ "DeliveryRequiredFrom", "2024-11-07T00:00:00+11:00" },
				{ "RunSheetNumber", "CR00000001" },
				{ "InstructionPostCode", "1900" },
				{ "InstructionCity", "Brisbane" },
				{ "InstructionState", "QLD" },
				{ "InstructionCountryCode", "AU" },
				{ "ParentEntityTB", "N" }
			});

			VerifyTransaction(tb3Transactions, new Dictionary<string, string>
			{
				{ "EstimatedTimeIn", "2024-12-01T13:09:00+11:00" },
				{ "TimeIn", "2024-12-01T15:09:00+11:00" },
				{ "EstimatedTimeOut", "2024-12-02T14:09:00+11:00" },
				{ "TimeOut", "2024-12-02T16:09:00+11:00" },
				{ "JobType", "PIC" },
				{ "PickupRequiredFrom", "2024-11-06T00:00:00+11:00" },
				{ "DeliveryRequiredFrom", "2024-11-07T00:00:00+11:00" },
				{ "RunSheetNumber", "CR00000001" },
				{ "InstructionPostCode", "2001" },
				{ "InstructionCity", "" },
				{ "InstructionState", "LND" },
				{ "InstructionCountryCode", "AU" },
				{ "ParentEntityTB", "N" }
			});
		}
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2024, 10);
		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}

		void VerifyTransaction(IEnumerable<IStlTransaction> transactions, Dictionary<string, string> keyValuePairs)
		{
			var expectedValues = BuildExpectedValues(keyValuePairs);
			var expectedValuesString = ConvertDictionaryToString(expectedValues);
			var transaction = transactions.Single(x => x.AdditionalRefs == $"{{{expectedValuesString}}}");
			AssertNotNull(transaction);
		}

		Dictionary<string, string> BuildExpectedValues(Dictionary<string, string> keyValuePairs)
		{
			return new Dictionary<string, string>(keyValuePairs);
		}

		string ConvertDictionaryToString(Dictionary<string, string> dictionary)
		{
			return dictionary.Select(kv => $"\"{kv.Key}\":\"{kv.Value}\"")
							 .Aggregate((current, next) => current + "," + next);
		}
	}
}
