using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportCompletedConsignment))]
	sealed class LandTransportCompletedConsignmentTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2022, 8);

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'
DECLARE @oh1PK UNIQUEIDENTIFIER = '1389d2b3-335e-4f28-a884-88a6de57b3c6'
DECLARE @oh2PK UNIQUEIDENTIFIER = '46708197-3541-41b5-9772-eacf8b6d19c1'
DECLARE @oh3PK UNIQUEIDENTIFIER = '336b5392-51e3-41ac-8847-ab9479e8282f'

DECLARE @cn1PK UNIQUEIDENTIFIER = '6b581b3f-1b20-4f77-aa2f-8dae1b466614'
DECLARE @cn2PK UNIQUEIDENTIFIER = 'ca9779ec-fe67-4508-8d54-fb99934668cb'
DECLARE @cn3PK UNIQUEIDENTIFIER = '7766faaa-6967-42b2-b76d-fb0dc3b5811b'

DECLARE @bookingPK UNIQUEIDENTIFIER = 'd4fe680f-c4b9-4049-b7f1-f529e7087c69'
DECLARE @bookingConsolidationPK UNIQUEIDENTIFIER = '50b8f4ea-4e2b-4a3b-a55a-c84c22000890'
DECLARE @jobHeaderPK UNIQUEIDENTIFIER = 'a0ebbfef-0cc1-4c2b-b52c-0a66a9ac60bc'

DECLARE @cnAddr1PK UNIQUEIDENTIFIER = '8d3ffe41-a97f-4d48-aa22-e38f416b4826'
DECLARE @cnAddr2PK UNIQUEIDENTIFIER = 'aa6fa00b-44dc-472d-b02a-e0204981fad6'
DECLARE @cnAddr3PK UNIQUEIDENTIFIER = 'fac7c39e-dbfc-4f82-9c74-dcae3955d28a'
DECLARE @cnAddr4PK UNIQUEIDENTIFIER = 'e578aa9a-5012-44a9-8ed5-da159e44af94'
DECLARE @cnAddr5PK UNIQUEIDENTIFIER = '6427f8a4-6407-45e5-b1a6-f62281896af4'
DECLARE @cnAddr6PK UNIQUEIDENTIFIER = 'bc84cea4-34f8-4046-bf37-7a551dea256a'
DECLARE @cnAddr7PK UNIQUEIDENTIFIER = 'bb59f092-986f-4207-b347-7da67146b2ca'

DECLARE @oaAddr1PK UNIQUEIDENTIFIER = '1e0c1026-7901-46d0-ae71-70438ddd1474'
DECLARE @oaAddr2PK UNIQUEIDENTIFIER = '45e085a2-84e8-47cc-8719-5f353cf63808'
DECLARE @oaAddr3PK UNIQUEIDENTIFIER = 'b6ecd480-7d6d-439a-8eb6-f1d1d724ca58'

DECLARE @jobDocAddr1PK UNIQUEIDENTIFIER = 'b5ca7936-af96-4d2c-998d-1e86af415ada'
DECLARE @jobDocAddr2PK UNIQUEIDENTIFIER = 'e295c210-dfe2-4497-a360-a83e52348b84'
DECLARE @jobDocAddr3PK UNIQUEIDENTIFIER = '0f296d0e-4554-4916-985d-3457fc25191b'
DECLARE @jobDocAddr4PK UNIQUEIDENTIFIER = '83c2b6d7-c2dd-4975-a10d-413761624fe1'
DECLARE @jobDocAddr5PK UNIQUEIDENTIFIER = '65f93b69-73f4-4acb-9fbd-bcba1b77ff75'
DECLARE @jobDocAddr6PK UNIQUEIDENTIFIER = 'e3bf147f-6587-4dbd-846e-5a1384c2e845'
DECLARE @jobDocAddr7PK UNIQUEIDENTIFIER = 'b3869148-bb67-480d-9a43-bbee44c02353'

DECLARE @jobServicePK UNIQUEIDENTIFIER = 'dd525457-90ab-447d-8c8b-c5e5ea2fbe57'
DECLARE @pkgJob1PK UNIQUEIDENTIFIER = 'c3eb3796-e58e-41a1-b64f-f6a0cca072c2'
DECLARE @pkgJob2PK UNIQUEIDENTIFIER = 'c0e5d801-5587-4f54-9e8f-ba67a0a867ab'
DECLARE @pkg1PK UNIQUEIDENTIFIER = '224570c1-ce1d-40dc-be89-3d5d5f8d9eb3'
DECLARE @pkg2PK UNIQUEIDENTIFIER = '4c5e9f62-da15-4e11-a370-4b000cdb5fc0'
DECLARE @pkg3PK UNIQUEIDENTIFIER = 'c7098ef8-8e98-4402-a0d7-85dcf8e23209'

DECLARE @undg1PK UNIQUEIDENTIFIER = '4b96a999-6627-45dc-918d-c7d6e664353d'
DECLARE @undg2PK UNIQUEIDENTIFIER = '67df5565-6ca4-4469-a6b5-ac924b6a5ada'

DECLARE @pkgHeader1PK UNIQUEIDENTIFIER = '75cdeb5c-9a7a-4d12-9032-bb547a146572'

DECLARE @rs1PK UNIQUEIDENTIFIER = '66a282e7-43a4-4f07-9898-d19c7235a8c5'
DECLARE @rs2PK UNIQUEIDENTIFIER = 'ed96517d-8b95-4dd2-9308-3b6717fcb17a'
DECLARE @rsInstruction1PK UNIQUEIDENTIFIER = 'e72feb3e-1baa-4bc8-bbde-5202c1849c89'
DECLARE @rsInstruction2PK UNIQUEIDENTIFIER = 'c5ff7b69-15ef-4e82-8889-edcb5277eabd'
DECLARE @cnAction1PK UNIQUEIDENTIFIER = 'dbacdeb1-9064-4fcc-ae45-950cea28b609'
DECLARE @cnAction2PK UNIQUEIDENTIFIER = '35c91253-fe3a-4a73-a77c-4dde0a8b7beb'
DECLARE @actionPkgDivot1PK UNIQUEIDENTIFIER = 'a06bda09-6fce-4169-a588-b782f3e8d5b8'
DECLARE @actionPkgDivot2PK UNIQUEIDENTIFIER = '8565683f-9d98-4e83-8bfd-0100cc32731d'

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsMiscFreightServices, OH_IsPackDepot)
	VALUES (@oh1PK, 'ARORG1', 1, 1),
	(@oh2PK, 'ARORG2', 0, 0),
	(@oh3PK, 'ARORG3', 0, 0);

INSERT INTO [dbo].[DtbBookingConsolidation] ([KB_PK], [KB_JobID], [KB_JobDirection], [KB_Status], [KB_IsOverridden], [KB_ParentID], [KB_ParentTableCode], [KB_GoodsDescription], [KB_JobType], [KB_AutoVersion], [KB_SystemCreateTimeUtc], [KB_SystemCreateUser], [KB_SystemLastEditTimeUtc], [KB_SystemLastEditUser])
	VALUES (@bookingConsolidationPK, 'test', 'AVL', 'LOC', 0, NULL, '', '', 'BKG', 0, '2022-08-11 02:23:00', 'TST', GETUTCDATE(), 'TST')

INSERT INTO dbo.DtbBooking (KM_PK, KM_JobID, KM_IsActive, KM_IsHazardous, KM_Status, KM_Description, KM_Direction, KM_KB_Booking, KM_Chargeable, KM_OverrideChargeable, KM_PL_NKCarrierServiceLevel, KM_RatingFreightMode, KM_TransportReference, KM_Distance, KM_DistanceUnit, KM_JobType, KM_AutoVersion, KM_SystemCreateTimeUtc, KM_SystemCreateUser, KM_SystemLastEditTimeUtc, KM_SystemLastEditUser)
	VALUES (@bookingPK, 'test', 1, 0, 'AVL', 'description', 'LOC', @bookingConsolidationPK, 0.0, 0, '', '', '', 0.0, 'KM', 'BKG', 0, '2022-08-11 02:23:00', 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_JobID, LTC_ConsignmentType, LTC_ConnoteNumber, LTC_JobType, LTC_Status, LTC_Direction, LTC_GB_Branch, LTC_KM_Booking, LTC_IsRouteOverridden, LTC_SystemCreateTimeUtc, LTC_SystemCreateUser, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser)
	VALUES (@cn1PK, 'CN0001', 'LTC', 'CNNote001', 'LTL', 'CMP', 'LOC', @SYDBranchPk, @bookingPK, 1, '2022-08-11 02:23:00', 'TST', GETUTCDATE(), 'TST'),
	(@cn2PK, 'CN0002', 'LTC', 'CNNote002', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 0, '2022-08-12 12:33:00', 'TS3', GETUTCDATE(), 'TS3'),
	(@cn3PK, 'CN0003', 'LTC', 'CNNote003', 'LTL', 'BKD', 'LOC', @SYDBranchPk, NULL, 0, '2022-08-11 02:33:00', 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.JobHeader ([JH_PK], [JH_IsValid], [JH_HeaderType], [JH_Name], [JH_Description], [JH_JobNum], [JH_JobLocalReference], [JH_ARInvoiceReference], [JH_TH_NKQuoteNumber], [JH_Status], [JH_ProfitLossReasonCode], [JH_A_JOP], [JH_RevenueRecognizedDate], [JH_A_JCL], [JH_JobPlannedStartDate], [JH_JobBufferPercentOverride], [JH_IsProfitSharePosted], [JH_LocalChargesCFX], [JH_OA_LocalChargesAddr], [JH_OC_LocalBillingContact], [JH_OA_AgentCollectAddr], [JH_AgentChargesCFX], [JH_LocalClientInvoicingStyle], [JH_SingleAgentsInvoicePerConsol], [JH_UniqueJobInvoiceNumber], [JH_PaymentCollectionStatus], [JH_RatingHasBeenRun], [JH_ExcludeFromPeriodicRating], [JH_ProfitShareInvoice], [JH_GB], [JH_GE], [JH_GC], [JH_GS_NKRepSales], [JH_GS_NKRepOps], [JH_ParentID], [JH_ParentTableCode], [JH_JH_ParentJob], [JH_SystemCreateTimeUtc], [JH_SystemCreateUser], [JH_SystemLastEditTimeUtc], [JH_SystemLastEditUser], [JH_HoldReason], [JH_AutoVersion], [JH_IsActive], [JH_GB_TaxBranch], [JH_ClientContractNumber])
	VALUES (@jobHeaderPK, 0, N'JOB', N'', N'', N'CN0001', N'00000009', N'', N'', N'WRK', N'', CAST(N'2022-09-07T15:42:00' AS SMALLDATETIME), NULL, NULL, NULL, 0, 0, CAST(0.000 AS DECIMAL(6, 3)), N'bccd75a6-bcd7-4be0-9b0a-0e7f650b471d', NULL, NULL, CAST(0.000 AS DECIMAL(6, 3)), N'', 1, 0, N'', 0, 0, NULL, @SYDBranchPk, N'8eeb6773-d15c-47b2-ad51-2f499919a420', @EDICompanyPk, N'', N'TST', @cn1PK, N'LTC', NULL, CAST(N'2022-09-07T05:42:00' AS SMALLDATETIME), N'TST', CAST(N'2022-09-07T05:42:00' AS SMALLDATETIME), N'WX', N'', 0, 1, NULL, NULL)

INSERT INTO dbo.JobService ([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate], [ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser])
	VALUES (@jobServicePK, 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, @EDICompanyPk, @cn1PK, 'LTC', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST');

INSERT INTO dbo.DtbConsignmentAddress (LTS_PK, LTS_LTC_Consignment, LTS_InstructionType, LTS_Sequence, LTS_Status, LTS_SystemCreateTimeUtc, LTS_SystemCreateUser, LTS_SystemLastEditTimeUtc, LTS_SystemLastEditUser)
	VALUES (@cnAddr1PK, @cn1PK, 'PIC', 1, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAddr2PK, @cn1PK, 'DLV', 2, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAddr3PK, @cn2PK, 'PIC', 1, 'INC', '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST'),
	(@cnAddr7PK, @cn2PK, 'PIC', 5, 'INC', '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST'),
	(@cnAddr4PK, @cn2PK, 'MLT', 2, 'INC', '2022-08-11 01:23:00', 'TST', '2022-08-11 01:23:00', 'TST'),
	(@cnAddr5PK, @cn2PK, 'DLV', 3, 'INC', '2022-08-11 01:23:00', 'TST', '2022-08-11 01:23:00', 'TST'),
	(@cnAddr6PK, @cn2PK, 'DLV', 4, 'INC', '2022-08-12 01:23:00', 'TST', '2022-08-12 01:23:00', 'TST');

INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_City, OA_State, OA_RN_NKCountryCode, OA_GeoLocation, OA_OH, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
	VALUES (@oaAddr1PK, 'test address 1', 'DINGLEY', 'VIC', 'AU', geography::Point(47.651, -122.362, 4326), @oh1PK, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@oaAddr2PK, 'test address 2', 'MELBOURNE', 'VIC', 'AU', geography::Point(47.652, -122.363, 4326), @oh2PK, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@oaAddr3PK, 'test address 3', 'MOUNT GAMBIER', 'SA', 'AU', geography::Point(47.653, -122.364, 4326), @oh3PK, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_City, E2_State, E2_Postcode, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address, E2_AddressOverride, E2_ValidationStatus, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
	VALUES (@jobDocAddr1PK, @cnAddr1PK, 'LTS', 'LCF', 'ALEXANDRIA', 'NSW', '2000', 'AU', geography::Point(47.616, -122.360, 4326), @oaAddr2PK, 1, 'NYV', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@jobDocAddr2PK, @cnAddr2PK, 'LTS', 'CRB', 'DUTTON PARK', 'QLD', '4000', 'AU', geography::Point(47.626, -122.360, 4326), @oaAddr1PK, 1, 'NYV', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@jobDocAddr3PK, @cnAddr3PK, 'LTC', 'LCF', '', '', '', '', geography::Point(47.636, -122.360, 4326), @oaAddr2PK, 0, 'NRQ', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@jobDocAddr4PK, @cnAddr4PK, 'LTS', 'LCF', 'ADELAIDE', 'SA', '5000', 'AU', geography::Point(47.646, -122.360, 4326), NULL, 0, 'NRQ', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@jobDocAddr5PK, @cnAddr5PK, 'LTS', 'CRB', 'UNLEY', 'SA', '5001', 'AU', geography::Point(47.656, -122.360, 4326), NULL, 0, 'NRQ', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@jobDocAddr6PK, @cnAddr6PK, 'LTS', 'LCF', 'STAWELL', 'VIC', '3000', 'AU', geography::Point(47.656, -122.360, 4326), @oaAddr3PK, 0, 'NRQ', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@jobDocAddr7PK, @cn1PK, 'LTC', 'LCF', 'DUTTON PARK', 'QLD', '4000', 'AU', geography::Point(47.626, -122.360, 4326), @oaAddr1PK, 1, 'NYV', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO [dbo].[PkgPackageJob] ([KJ_PK], [KJ_JobID], [KJ_ParentID], [KJ_ParentTableCode], [KJ_ReleasedTimeUtc], [KJ_GS_NKReleasedBy], [KJ_IsFinalized], [KJ_AutoVersion], [KJ_SystemLastEditTimeUtc], [KJ_SystemLastEditUser], [KJ_SystemCreateTimeUtc], [KJ_SystemCreateUser])
	VALUES (@pkgJob1PK, N'P0186', @cn1PK, N'LTC', NULL, N'', 0, 0, CAST(N'2022-09-07T05:43:00' AS SMALLDATETIME), N'TST', CAST(N'2022-09-07T05:43:00' AS SMALLDATETIME), N'TST'),
	(@pkgJob2PK, N'P0187', @cn2PK, N'LTC', NULL, N'', 0, 0, CAST(N'2022-09-07T05:44:00' AS SMALLDATETIME), N'TST', CAST(N'2022-09-07T05:44:00' AS SMALLDATETIME), N'TST');

INSERT INTO [dbo].[PkgPackageHeader] ([KPH_PK], [KPH_PackageID], [KPH_SystemCreateTimeUtc], [KPH_SystemCreateUser], [KPH_AutoVersion], [KPH_SystemLastEditTimeUtc], [KPH_SystemLastEditUser])
	VALUES (@pkgHeader1PK, N'MAEU01561124', GETUTCDATE(), 'TST', 0, GETUTCDATE(), 'TST');

INSERT INTO [dbo].[PkgPackage] ([KP_PK], [KP_F3_NKPackType], [KP_PackageQty], [KP_Length], [KP_Width], [KP_Height], [KP_DimensionUQ], [KP_Weight], [KP_WeightUQ], [KP_Volume], [KP_RH_NKCommodityCode], [KP_VolumeUQ], [KP_MarksAndNumbers], [KP_TransportRef], [KP_HSCode], [KP_RequiresTemperatureControl], [KP_RequiredTemperatureMinimum], [KP_RequiredTemperatureMaximum], [KP_RequiredTemperatureUnit], [KP_ClosedTimeUtc], [KP_GS_NKClosedBy], [KP_IsReleasedViaJob], [KP_ReleasedTimeUtc], [KP_GS_NKReleasedBy], [KP_KP_ParentPackage], [KP_KJ_ParentPackageJob], [KP_PreviousPackageID], [KP_Sequence], [KP_KPH_PackageHeader], [KP_IsDamaged], [KP_IsHeld], [KP_IsCheckedWeighedCubed], [KP_DamagedReason], [KP_AutoVersion], [KP_ExternalReference], [KP_IsPillaged], [KP_KP_TopHandlingUnitPackage], [KP_GoodsDescription], [KP_SystemCreateTimeUtc], [KP_SystemCreateUser], [KP_SystemLastEditTimeUtc], [KP_SystemLastEditUser], [KP_DunnageWeight], [KP_TareWeight], [KP_MarksAndNumbersVersion])
	VALUES (@pkg1PK, N'PLT', 1, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(1500.000 AS DECIMAL(9, 3)), N'KG', CAST(3.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 1, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @pkgJob1PK, N'', 1, @pkgHeader1PK, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0),	
	(@pkg2PK, N'CNT', 2, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(238.000 AS DECIMAL(9, 3)), N'KG', CAST(2.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 0, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @pkgJob2PK, N'', 1, NULL, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0),
	(@pkg3PK, N'PLT', 1, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(1500.000 AS DECIMAL(9, 3)), N'KG', CAST(3.000 AS DECIMAL(9, 3)), N'', N'M3', N'', N'', N'', 0, CAST(0.000 AS DECIMAL(8, 3)), CAST(0.000 AS DECIMAL(8, 3)), N'', NULL, N'', 0, NULL, N'', NULL, @pkgJob2PK, N'', 1, NULL, 0, 0, 0, N'', 1, N'', 0, NULL, N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST', NULL, NULL, 0);

INSERT INTO [dbo].[UNDGDataItem] ([DI_PK], [DI_DGFlashPoint], [DI_TechnicalName], [DI_MPMarinePollutant], [DI_OC_DGContact], [DI_DGVolume], [DI_UnitOfVolume], [DI_DGWeight], [DI_UnitOfWeight], [DI_ParentTableCode], [DI_ParentID], [DI_IMOClass], [DI_PackageCount], [DI_F3_NKPackType], [DI_IsLimitedQuantity], [DI_DG], [DI_HasOverpack], [DI_OverpackID], [DI_AutoVersion], [DI_PackingInstructionSection], [DI_HazardousWasteCode], [DI_IsNotOtherwiseSpecified], [DI_IsResidueLastContained], [DI_IsSalvagePackaging], [DI_SpecialPermitIssueDate], [DI_SpecialPermitNumber], [DI_CriticalitySafetyIndex], [DI_IsCombustible], [DI_IsExclusiveUse], [DI_IsFissileExcepted], [DI_IsHighwayRouteControlledQuantity], [DI_MaterialFormDescription], [DI_RadioactiveLabelCategory], [DI_RadioactiveMaximumActivity], [DI_RadioactiveMaximumActivityUnit], [DI_RadioactiveTransportIndex], [DI_RadionuclideElement], [DI_RadionuclideElementSuffix], [DI_SystemCreateTimeUtc], [DI_SystemCreateUser], [DI_SystemLastEditTimeUtc], [DI_SystemLastEditUser])
	VALUES (@undg1PK, CAST(35.0 AS DECIMAL(8, 1)), N'', N'', NULL, CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', N'KP', @pkg1PK, N'', 0, N'', 0, NULL, 0, N'', 0, N'', N'', 0, 0, 0, NULL, N'', CAST(0.0 AS DECIMAL(6, 1)), 0, 0, 0, 0, N'', N'', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@undg2PK, CAST(35.0 AS DECIMAL(8, 1)), N'', N'', NULL, CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', N'KP', @pkg1PK, N'', 0, N'', 0, NULL, 0, N'', 0, N'', N'', 0, 0, 0, NULL, N'', CAST(0.0 AS DECIMAL(6, 1)), 0, 0, 0, 0, N'', N'', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT [dbo].[DtbConsignmentRunSheet] ([KG_PK], [KG_RunSheetNumber], [KG_RQ_Truck], [KG_OH_TransportCo], [KG_GS_NKTruckDriver], [KG_AdHocDriversLicence], [KG_AdHocDriversName], [KG_AdHocTransportCoName], [KG_Duration], [KG_IsPlanning], [KG_PL_NKCarrierServiceLevel], [KG_AdHocTruckRegistration], [KG_TransitTime], [KG_ContainerMode], [KG_TransportMode], [KG_SystemCreateTimeUtc], [KG_SystemCreateUser], [KG_SystemLastEditTimeUtc], [KG_SystemLastEditUser], [KG_GB_Branch], [KG_EndTime], [KG_StartTime], [KG_AutoVersion])
	VALUES (@rs1PK, N'CR0001', NULL, NULL, N'TS1', N'', N'', N'', NULL, 1, N'', N'', NULL, N'LTL', N'ROA', GETUTCDATE(), N'TST', GETUTCDATE(), N'TST', @SYDBranchPk, CAST(N'2019-05-02T16:30:00.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2019-05-02T07:30:00.0000000+10:00' AS DATETIMEOFFSET), 0),
	(@rs2PK, N'CR0002', NULL, NULL, N'TS2', N'', N'', N'', NULL, 1, N'', N'', NULL, N'LTL', N'ROA', GETUTCDATE(), N'TST', GETUTCDATE(), N'TST', @SYDBranchPk, CAST(N'2019-05-02T16:30:00.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2019-05-02T07:30:00.0000000+10:00' AS DATETIMEOFFSET), 0);

INSERT INTO [dbo].[DtbConsignmentRunSheetInstruction] ([K1_PK], [K1_KG_RunSheet], [K1_Sequence], [K1_IsAcceptedByDriver], [K1_ReceivedBy], [K1_ReceivedBySignature], [K1_FailureReason], [K1_FailureNotes], [K1_InstructionType], [K1_RegisteredWeight], [K1_RegisteredWeightUQ], [K1_RegisteredVolume], [K1_RegisteredVolumeUQ], [K1_RegisteredPalletSpaces], [K1_EstimatedTimeIn], [K1_EstimatedTimeOut], [K1_TimeIn], [K1_TimeOut], [K1_SystemCreateTimeUtc], [K1_SystemCreateUser], [K1_SystemLastEditTimeUtc], [K1_SystemLastEditUser])
	VALUES (@rsInstruction1PK, @rs1PK, 1, 1, N'John', NULL, N'Fut', N'Futile', N'', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', 0, NULL, NULL, CAST(N'2020-08-21T13:47:15.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2020-08-21T13:47:38.0000000+10:00' AS DATETIMEOFFSET), GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@rsInstruction2PK, @rs2PK, 1, 1, N'TS2', NULL, N'Fut', N'Futile', N'', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', 0, NULL, NULL, CAST(N'2020-08-21T13:47:15.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2020-08-21T13:47:38.0000000+10:00' AS DATETIMEOFFSET), GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO [dbo].[DtbConsignmentAction] ([LTA_PK], [LTA_ActionID], [LTA_ActionType], [LTA_ReferenceNumber], [LTA_SignedBy], [LTA_SignedBySignature], [LTA_LTS_ConsignmentAddress], [LTA_K1_RunSheetInstruction], [LTA_LHM_Manifest], [LTA_ActualTime], [LTA_EstimatedTime], [LTA_RequiredFrom], [LTA_RequiredTo], [LTA_Slot], [LTA_AutoVersion], [LTA_FailureReason], [LTA_FailureNotes], [LTA_SystemCreateTimeUtc], [LTA_SystemCreateUser], [LTA_SystemLastEditTimeUtc], [LTA_SystemLastEditUser])
	VALUES (@cnAction1PK, 'LTA00000001', N'PIC', N'0040155', N'', NULL, @cnAddr1PK, @rsInstruction1PK, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAction2PK, 'LTA00000002', N'DLV', N'0040156', N'', NULL, @cnAddr2PK, @rsInstruction2PK, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO [dbo].[DtbConsignmentActionPackageDivot] ([LTP_PK], [LTP_AutoVersion], [LTP_LTA_ConsignmentAction], [LTP_KP_Package], [LTP_PackageQuantity], [LTP_SystemCreateTimeUtc], [LTP_SystemCreateUser], [LTP_SystemLastEditTimeUtc], [LTP_SystemLastEditUser])
	VALUES (@actionPkgDivot1PK, 1, @cnAction1PK, @pkg1PK, 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@actionPkgDivot2PK, 1, @cnAction2PK, @pkg2PK, 1, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');
			";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Consignment Usage Statistics Records", 2, transactions.Count());
			AssertRowResult(transactions, "EDI", "SYD", new DateTime(2022, 8, 11, 2, 23, 0), "6b581b3f-1b20-4f77-aa2f-8dae1b466614", "LTL", "CNNote001", "CN0001", "TST", "{\"JobType\":\"LTL\",\"HasTB\":true,\"StaffCode\":\"TST\",\"BillingLinesAttached\":false,\"CountOfPICInstructions\":1,\"CountOfPICInstructionsLinkedToMiscellaneousAddress\":1,\"CountOfDLVInstructions\":1,\"CountOfDLVInstructionsLinkedToMiscellaneousAddress\":1,\"CountOfMLTInstructions\":0,\"PCity\":\"ALEXANDRIA\",\"PState\":\"NSW\",\"PPostcode\":\"2000\",\"PCountry\":\"AU\",\"PGeoloc\":\"POINT (-122.36 47.616)\",\"AnyPicAddressIsDepot\":false,\"AnyDlvAddressIsDepot\":true,\"DCity\":\"DUTTON PARK\",\"DState\":\"QLD\",\"DPostcode\":\"4000\",\"DCountry\":\"AU\",\"DGeoloc\":\"POINT (-122.36 47.626)\",\"CountOfServices\":1,\"CountOfAdditionalAddresses\":1,\"HasHazardousPackage\":true,\"HasRefrigeratedPackage\":true,\"TotalPacksQty\":1,\"ContainerCount\":0,\"PackLineCount\":1,\"TotalPacksHavingPackageID\":1,\"TotalPacksWeightKG\":1500.0000000000000,\"TotalPacksVolumeM3\":3.0000000000000,\"TotalRejectedPIC\":1,\"TotalRejectedDLV\":1,\"IfAnyActionPackageDivot\":true,\"IfRoutingOverridden\":true}");
			AssertRowResult(transactions, "EDI", "SYD", new DateTime(2022, 8, 12, 12, 33, 0), "ca9779ec-fe67-4508-8d54-fb99934668cb", "LTL", "CNNote002", "CN0002", "TS3", "{\"JobType\":\"LTL\",\"HasTB\":false,\"StaffCode\":\"TS3\",\"BillingLinesAttached\":false,\"CountOfPICInstructions\":2,\"CountOfPICInstructionsLinkedToMiscellaneousAddress\":0,\"CountOfDLVInstructions\":2,\"CountOfDLVInstructionsLinkedToMiscellaneousAddress\":0,\"CountOfMLTInstructions\":1,\"AnyPicAddressIsDepot\":false,\"AnyDlvAddressIsDepot\":false,\"DCity\":\"MOUNT GAMBIER\",\"DState\":\"SA\",\"DPostcode\":\"\",\"DCountry\":\"AU\",\"DGeoloc\":\"POINT (-122.364 47.653)\",\"CountOfServices\":0,\"CountOfAdditionalAddresses\":0,\"HasHazardousPackage\":false,\"HasRefrigeratedPackage\":false,\"TotalPacksQty\":3,\"ContainerCount\":1,\"PackLineCount\":2,\"TotalPacksHavingPackageID\":0,\"TotalPacksWeightKG\":1738.0000000000000,\"TotalPacksVolumeM3\":5.0000000000000,\"TotalRejectedPIC\":0,\"TotalRejectedDLV\":0,\"IfAnyActionPackageDivot\":false,\"IfRoutingOverridden\":false}");
		}

		void AssertRowResult(IEnumerable<IStlTransaction> transactions, string companyCode, string branchCode, DateTime transactionDate, string referenceGuid, string reference1, string reference2, string reference3, string userCode, string additonalRefs)
		{
			var transaction = transactions.Single(t => t.Reference5.ToLower() == referenceGuid.ToLower());
			AssertEquals($"Company Code should be {companyCode}", companyCode, transaction.GetCompanyCode());
			AssertEquals($"Branch Code should be {branchCode}", branchCode, transaction.GetBranchCode());
			AssertEquals($"TransactionDateUtc should be {transactionDate:dd/MM/yyyy HH:mm:ss}", transactionDate, transaction.ServiceOccuredUTC);
			AssertEquals($"Billing Reference 1 should be {reference1}", reference1, transaction.Reference3);
			AssertEquals($"Billing Reference 2 should be {reference2}", reference2, transaction.Reference2);
			AssertEquals($"Billing Reference 3 should be {reference3}", reference3, transaction.Reference1);
			AssertEquals($"Creating User Code should be {userCode}", userCode, transaction.ClientStaffCode);

			AssertEquals("AdditionalRefs should be as expected", additonalRefs, transaction.AdditionalRefs);
		}
	}
}
