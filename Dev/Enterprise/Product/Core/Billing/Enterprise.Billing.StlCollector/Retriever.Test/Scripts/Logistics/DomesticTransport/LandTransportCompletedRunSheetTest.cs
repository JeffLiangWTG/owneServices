using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(LandTransportCompletedRunSheet))]
	sealed class LandTransportCompletedRunSheetTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.NewByStartEnd(new DateTime(2022, 11, 18, 0, 0, 0), new DateTime(2022, 11, 18, 23, 59, 59));

		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlQuery = @"
DECLARE @EDICompanyPk UNIQUEIDENTIFIER = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC'
DECLARE @SYDBranchPk UNIQUEIDENTIFIER = 'FDD429D2-648C-4895-8F9F-06E90DED2BE5'

DECLARE @rs1PK UNIQUEIDENTIFIER = '66a282e7-43a4-4f07-9898-d19c7235a8c5'
DECLARE @rs2PK UNIQUEIDENTIFIER = 'ed96517d-8b95-4dd2-9308-3b6717fcb17a'
DECLARE @rs3PK UNIQUEIDENTIFIER = 'd2c851a2-5b95-47b0-a101-fccce3348381'
DECLARE @rs4PK UNIQUEIDENTIFIER = '17b98b8b-1852-44df-aa57-92e9e93a5950'

DECLARE @refContainer1PK UNIQUEIDENTIFIER = 'bb92b201-358b-4cca-9d84-b4a527518737'
DECLARE @equipmentConfig1PK UNIQUEIDENTIFIER = '794d38d9-3c63-4e6a-912b-15a63b24c7e1'
DECLARE @equipmentConfigItem1PK UNIQUEIDENTIFIER = '1a75fe46-b126-423d-af37-c31a7b548cbb'
DECLARE @equipmentItem1PK UNIQUEIDENTIFIER = '7aa8f2f1-c131-4262-9559-1972dc20557e'
DECLARE @equipmentItem2PK UNIQUEIDENTIFIER = '41bae630-be34-4349-a32b-5d88837ac4c2'
DECLARE @equipment1PK UNIQUEIDENTIFIER = 'd604c5d1-5bc2-42db-8bdd-ac8a31996282'
DECLARE @equipment2PK UNIQUEIDENTIFIER = 'cf6192c2-0e45-48dd-a816-8931e21f1b76'
DECLARE @oh1PK UNIQUEIDENTIFIER = '2322f80e-126c-4f77-94d0-812e9fb27634'

DECLARE @cn1PK UNIQUEIDENTIFIER = '6b581b3f-1b20-4f77-aa2f-8dae1b466614'
DECLARE @cnAddr1PK UNIQUEIDENTIFIER = '8d3ffe41-a97f-4d48-aa22-e38f416b4826'
DECLARE @cnAddr2PK UNIQUEIDENTIFIER = 'aa6fa00b-44dc-472d-b02a-e0204981fad6'

DECLARE @oaAddr1PK UNIQUEIDENTIFIER = '1e0c1026-7901-46d0-ae71-70438ddd1474'

DECLARE @jobDocAddr1PK UNIQUEIDENTIFIER = 'b5ca7936-af96-4d2c-998d-1e86af415ada'

DECLARE @jobServicePK UNIQUEIDENTIFIER = 'dd525457-90ab-447d-8c8b-c5e5ea2fbe57'

DECLARE @rsInstruction1PK UNIQUEIDENTIFIER = 'e72feb3e-1baa-4bc8-bbde-5202c1849c89'
DECLARE @rsInstruction2PK UNIQUEIDENTIFIER = 'c5ff7b69-15ef-4e82-8889-edcb5277eabd'
DECLARE @rsInstruction3PK UNIQUEIDENTIFIER = 'e39d21f9-433a-4a52-b896-b4b6d87f5353'
DECLARE @rsInstruction4PK UNIQUEIDENTIFIER = '9bb90fcd-d474-40a7-a0bb-75ae99f6ec4f'
DECLARE @cnAction1PK UNIQUEIDENTIFIER = 'dbacdeb1-9064-4fcc-ae45-950cea28b609'
DECLARE @cnAction2PK UNIQUEIDENTIFIER = '35c91253-fe3a-4a73-a77c-4dde0a8b7beb'
DECLARE @cnAction3PK UNIQUEIDENTIFIER = 'd255eba6-4a60-4e99-8b9c-6502ce6e7ed9'
DECLARE @cnAction4PK UNIQUEIDENTIFIER = '716f277f-593a-4e92-9e53-9a09b37b3e2e'

DECLARE @reference1PK UNIQUEIDENTIFIER = 'd25454e0-9177-47f0-97d7-ca99705597fd'
DECLARE @jobConsollCost1PK UNIQUEIDENTIFIER = 'fa2dd99d-36fa-4273-9b8e-1e16346baa6e'
DECLARE @accChargeCode1PK UNIQUEIDENTIFIER = 'a031155b-b767-4c3b-b6fc-005e4336cef3'

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_IsMiscFreightServices, OH_IsPackDepot)
	VALUES (@oh1PK, 'ARORG1', 1, 1)

INSERT [dbo].[RefContainer] ([RC_PK], [RC_Code], [RC_ShippingMode], [RC_Description], [RC_Length], [RC_Height], [RC_Width], [RC_ContainerType], [RC_ISOType], [RC_TareWeight], [RC_GrossWeight], [RC_CubicCapacity], [RC_StorageClass], [RC_HandlingRateClass], [RC_FreightRateClass], [RC_IATARateClass], [RC_USContainerCode], [RC_TEU], [RC_IsActive], [RC_IsHighCube], [RC_HasTynes], [RC_HasVents], [RC_IsIso], [RC_ISOEquipmentSizeTypeCode], [RC_SystemCreateTimeUtc], [RC_SystemCreateUser], [RC_SystemLastEditTimeUtc], [RC_SystemLastEditUser], [RC_IsSystem], [RC_Contour], [RC_InsideHeight], [RC_InsideLength], [RC_InsideWidth], [RC_NetWeight])
	VALUES (@refContainer1PK, N'', N'SEA', N'', CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', N'    ', CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', N'', N'', N'', N'', CAST(0.00 AS DECIMAL(5, 2)), 1, 0, 0, 0, 0, N'', NULL, N'', NULL, N'', 1, N'', CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)))

INSERT [dbo].[RefEquipment] ([RQ_PK], [RQ_Description], [RQ_EquipmentType], [RQ_EquipmentGroup], [RQ_2WayInfo], [RQ_GateTransponder1], [RQ_GateTransponder2], [RQ_GateTransponder3], [RQ_TollPass], [RQ_CapacityInTEU], [RQ_WeightCapacity], [RQ_TareWeight], [RQ_WeightUnit], [RQ_CubicCapacity], [RQ_CubicUnit], [RQ_PackCapacity], [RQ_PackType], [RQ_PurchaseDate], [RQ_DisposalDate], [RQ_RegExpiry], [RQ_RegState], [RQ_VIN], [RQ_AddReference1], [RQ_AddReference2], [RQ_GeoProviderType], [RQ_GeoProviderID], [RQ_RC_RoadContainerType], [RQ_GS_NKPreferredDriver], [RQ_F3_NKPackType], [RQ_IsActive], [RQ_OH_Owner], [RQ_RN_NKRegistrationCountry], [RQ_AddFlag1], [RQ_AddCheckBox1], [RQ_IsVehicle], [RQ_ShortCode], [RQ_OwnerType], [RQ_Registration], [RQ_AutoVersion], [RQ_SystemCreateTimeUtc], [RQ_SystemCreateUser], [RQ_SystemLastEditTimeUtc], [RQ_SystemLastEditUser])
	VALUES (@equipment1PK, N'RIGID', N'   ', N'RIG', N'', N'', N'', N'', N'', 0, CAST(18500.000 AS DECIMAL(9, 3)), CAST(16000.000 AS DECIMAL(9, 3)), N'T', CAST(0.000 AS DECIMAL(9, 3)), N'', 24, N'', CAST(N'2012-07-01T00:00:00' AS SMALLDATETIME), NULL, NULL, N'VIC', N'1M8GDM9AKP042788', N'', N'', N'WTG', N'554546', NULL, N'', N'SPA', 1, NULL, N'AU', 0, 0, 0, N'1111111', N'', N'CMB894', 4, '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST'),
	(@equipment2PK, N'PRIME MOVER  RED', N'   ', N'', N'', N'', N'', N'', N'', 0, CAST(0.000 AS DECIMAL(9, 3)), CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', 0, N'', NULL, NULL, CAST(N'2019-06-30T00:00:00' AS SMALLDATETIME), N'', N'', N'', N'', N'', N'', NULL, N'', N'', 0, NULL, N'', 0, 0, 1, N'a11112555', N'', N'a11112555', 1, '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST')

INSERT [dbo].[DtbEquipmentItem] ([LTE_PK], [LTE_ParentID], [LTE_ParentTableCode], [LTE_RQ_Equipment], [LTE_EquipmentTypeQuantity], [LTE_RC_EquipmentType], [LTE_AutoVersion], [LTE_SystemCreateTimeUtc], [LTE_SystemCreateUser], [LTE_SystemLastEditTimeUtc], [LTE_SystemLastEditUser])
	VALUES (@equipmentItem1PK, @rs1PK, N'KG', @equipment1PK, 0, NULL, 0, '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST'),
	(@equipmentItem2PK, @rs1PK, N'KG', NULL, 1, @refContainer1PK, 0, '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST');

INSERT [dbo].[DtbConsignmentRunSheet] ([KG_PK], [KG_RunSheetNumber], [KG_TransportMode], [KG_ContainerMode], [KG_PL_NKCarrierServiceLevel], [KG_GS_NKTruckDriver], [KG_RQ_Truck], [KG_OH_TransportCo], [KG_AdHocDriversLicence], [KG_AdHocDriversName], [KG_AdHocTransportCoName], [KG_AdHocTruckRegistration], [KG_Duration], [KG_IsPlanning], [KG_TransitTime], [KG_SystemCreateTimeUtc], [KG_SystemCreateUser], [KG_SystemLastEditTimeUtc], [KG_SystemLastEditUser], [KG_GB_Branch], [KG_EndTime], [KG_StartTime], [KG_AutoVersion])
	VALUES (@rs1PK, N'CR0001', N'AIR', N'ULD', N'STD', N'DR1', @equipment1PK, @oh1PK, N'BY2233', N'Terry', N'Transport A', N'SB9481', '1901-01-02 16:00:00', 1, CAST(N'2022-11-18 12:43:10' AS SMALLDATETIME), CAST(N'2022-11-07 13:00:00' AS SMALLDATETIME), N'TST', GETUTCDATE(), N'TST', @SYDBranchPk, CAST(N'2022-11-18T16:30:00.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2022-11-18T07:30:00.0000000+10:00' AS DATETIMEOFFSET), 0),
	(@rs2PK, N'CR0002', N'SEA', N'LCl', N'EXP', N'DR2', NULL, NULL, N'', N'', N'', N'', NULL, 0, CAST(N'2022-11-18 11:22:10' AS SMALLDATETIME), CAST(N'2022-11-08 12:59:29' AS SMALLDATETIME), N'TS3', GETUTCDATE(), N'TS3', @SYDBranchPk, CAST(N'2022-11-18T11:30:00.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2022-11-18T07:20:00.0000000+10:00' AS DATETIMEOFFSET), 0),
	(@rs3PK, N'CR0003', N'ROA', N'LTL', N'STD', N'DR3', NULL, NULL, N'ABC123', N'AdHoc Drivers1', N'AdHoc Transport Co1', N'SB1234', NULL, 1, CAST(N'2022-11-08 12:43:10' AS SMALLDATETIME), CAST(N'2022-11-07 12:59:29' AS SMALLDATETIME), N'TST', GETUTCDATE(), N'TST', @SYDBranchPk, CAST(N'2022-11-18T14:40:00.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2022-11-18T05:30:00.0000000+10:00' AS DATETIMEOFFSET), 0),
	(@rs4PK, N'CR0004', N'ROA', N'FTL', N'STD', N'DR4', NULL, NULL, N'CD3323', N'AdHoc Drivers3', N'AdHoc Transport Co2', N'SB3245', NULL, 1, CAST(N'2022-11-08 12:43:10' AS SMALLDATETIME), CAST(N'2022-11-08 13:00:00' AS SMALLDATETIME), N'TST', GETUTCDATE(), N'TST', @SYDBranchPk, CAST(N'2022-11-18T16:30:00.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2022-11-18T06:30:00.0000000+10:00' AS DATETIMEOFFSET), 0)

INSERT [dbo].[DtbConsignmentRunSheetInstruction] ([K1_PK], [K1_KG_RunSheet], [K1_Sequence], [K1_IsAcceptedByDriver], [K1_ReceivedBy], [K1_ReceivedBySignature], [K1_FailureReason], [K1_FailureNotes], [K1_InstructionType], [K1_RegisteredWeight], [K1_RegisteredWeightUQ], [K1_RegisteredVolume], [K1_RegisteredVolumeUQ], [K1_RegisteredPalletSpaces], [K1_EstimatedTimeIn], [K1_EstimatedTimeOut], [K1_TimeIn], [K1_TimeOut], [K1_SystemCreateTimeUtc], [K1_SystemCreateUser], [K1_SystemLastEditTimeUtc], [K1_SystemLastEditUser])
	VALUES (@rsInstruction1PK, @rs1PK, 1, 1, N'John', NULL, N'Fut', N'Futile', N'ORG', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', 0, NULL, NULL, CAST(N'2020-08-21T13:47:15.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2020-08-21T13:47:38.0000000+10:00' AS DATETIMEOFFSET), GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@rsInstruction3PK, @rs1PK, 2, 1, N'John', NULL, N'', N'', N'DST', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', 0, NULL, NULL, CAST(N'2020-08-21T13:47:15.0000000+10:00' AS DATETIMEOFFSET), NULL, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@rsInstruction2PK, @rs2PK, 3, 1, N'TS2', NULL, N'Fut', N'Futile', N'', CAST(0.000 AS DECIMAL(9, 3)), N'', CAST(0.000 AS DECIMAL(9, 3)), N'', 0, NULL, NULL, CAST(N'2020-08-21T13:47:15.0000000+10:00' AS DATETIMEOFFSET), CAST(N'2020-08-21T13:47:38.0000000+10:00' AS DATETIMEOFFSET), GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.DtbConsignment (LTC_PK, LTC_JobID, LTC_ConsignmentType, LTC_ConnoteNumber, LTC_JobType, LTC_Status, LTC_Direction, LTC_GB_Branch, LTC_KM_Booking, LTC_IsRouteOverridden, LTC_SystemCreateTimeUtc, LTC_SystemCreateUser, LTC_SystemLastEditTimeUtc, LTC_SystemLastEditUser)
	VALUES (@cn1PK, 'CN0001', 'LTC', 'CNNote001', 'LTL', 'CMP', 'LOC', @SYDBranchPk, NULL, 1, '2022-08-11 02:23:00', 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.DtbConsignmentAddress (LTS_PK, LTS_LTC_Consignment, LTS_InstructionType, LTS_Sequence, LTS_Status, LTS_SystemCreateTimeUtc, LTS_SystemCreateUser, LTS_SystemLastEditTimeUtc, LTS_SystemLastEditUser)
	VALUES (@cnAddr1PK, @cn1PK, 'PIC', 1, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAddr2PK, @cn1PK, 'DLV', 2, 'INC', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT [dbo].[DtbConsignmentAction] ([LTA_PK], [LTA_ActionID], [LTA_ActionType], [LTA_ReferenceNumber], [LTA_SignedBy], [LTA_SignedBySignature], [LTA_LTS_ConsignmentAddress], [LTA_K1_RunSheetInstruction], [LTA_LHM_Manifest], [LTA_ActualTime], [LTA_EstimatedTime], [LTA_RequiredFrom], [LTA_RequiredTo], [LTA_Slot], [LTA_AutoVersion], [LTA_FailureReason], [LTA_FailureNotes], [LTA_SystemCreateTimeUtc], [LTA_SystemCreateUser], [LTA_SystemLastEditTimeUtc], [LTA_SystemLastEditUser])
	VALUES
	(@cnAction1PK, 'LTA00000001', N'PIC', N'0040155', N'', NULL, @cnAddr1PK, @rsInstruction1PK, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAction2PK, 'LTA00000002', N'DLV', N'0040156', N'', NULL, @cnAddr2PK, @rsInstruction2PK, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAction3PK, 'LTA00000003', N'PIC', N'0040157', N'', NULL, @cnAddr2PK, @rsInstruction3PK, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST'),
	(@cnAction4PK, 'LTA00000004', N'DLV', N'0040158', N'', NULL, @cnAddr2PK, @rsInstruction3PK, NULL, NULL, NULL, NULL, NULL, NULL, 1, N'', N'', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT INTO dbo.JobService ([ES_PK], [ES_IsValid], [ES_ServiceCode], [ES_BookedDateTimeOffset], [ES_Duration], [ES_ServiceCount], [ES_CompletedDateTimeOffset], [ES_References], [ES_OH_Contractor], [ES_OA_Location], [ES_RQ], [ES_GC], [ES_ParentID], [ES_ParentTableCode], [ES_CurrentContextID], [ES_CurrentContextTableCode], [ES_MeasurementBasis], [ES_RX_NKServiceRateCurrency], [ES_ServiceRate], [ES_SubLocation], [ES_ServiceNote], [ES_AutoVersion], [ES_SystemCreateTimeUtc], [ES_SystemCreateUser], [ES_SystemLastEditTimeUtc], [ES_SystemLastEditUser])
	VALUES (@jobServicePK, 1, 'TTT', NULL, NULL, 0.0, NULL, '', NULL, NULL, NULL, @EDICompanyPk, @rs1PK, 'KG', NULL, '', 'AAA', 'AAA', 1.0, '', '', 0, '2022-08-10 01:23:00', 'TST', '2022-08-10 01:23:00', 'TST');

INSERT INTO dbo.OrgAddress (OA_PK, OA_Address1, OA_City, OA_State, OA_RN_NKCountryCode, OA_GeoLocation, OA_OH, OA_SystemCreateTimeUtc, OA_SystemCreateUser, OA_SystemLastEditTimeUtc, OA_SystemLastEditUser)
	VALUES (@oaAddr1PK, 'test address 1', 'DINGLEY', 'VIC', 'AU', geography::Point(47.651, -122.362, 4326), @oh1PK, GETUTCDATE(), 'TST', GETUTCDATE(), 'TST')

INSERT INTO dbo.JobDocAddress (E2_PK, E2_ParentID, E2_ParentTableCode, E2_AddressType, E2_City, E2_State, E2_Postcode, E2_RN_NKCountryCode, E2_GeoLocation, E2_OA_Address, E2_AddressOverride, E2_ValidationStatus, E2_SystemCreateTimeUtc, E2_SystemCreateUser, E2_SystemLastEditTimeUtc, E2_SystemLastEditUser)
	VALUES (@jobDocAddr1PK, @cnAddr1PK, 'LTS', 'LCF', 'ALEXANDRIA', 'NSW', '2000', 'AU', geography::Point(47.616, -122.360, 4326), @oaAddr1PK, 1, 'NYV', GETUTCDATE(), 'TST', GETUTCDATE(), 'TST');

INSERT [dbo].[CusEntryNum] ([CE_PK], [CE_ParentID], [CE_ParentTable], [CE_EntryNum], [CE_EntryType], [CE_EntryLineReference], [CE_EntryStatus], [CE_IsValid], [CE_Category], [CE_IssueDate], [CE_ExpiryDate], [CE_RN_NKCountryCode], [CE_EntryIsSystemGenerated], [CE_SystemCreateTimeUtc], [CE_SystemCreateUser], [CE_AutoVersion], [CE_SystemLastEditTimeUtc], [CE_SystemLastEditUser])
	VALUES (@reference1PK, @rs1PK, N'DtbConsignmentRunSheet', N'', N'', N'', N'', 0, N'CUS', NULL, NULL, N'', 1, GETUTCDATE(), 'TST', 1, GETUTCDATE(), 'TST');

INSERT [dbo].[AccChargeCode] ([AC_PK], [AC_Code], [AC_Desc], [AC_LocalLanguageDescription], [AC_ChargeType], [AC_MarginPercentage], [AC_AW_WithholdingTaxRate], [AC_AT_GSTRate], [AC_AG_RevenueAccount], [AC_AG_WIPAccount], [AC_AG_CostAccount], [AC_AG_AccrualAccount], [AC_PrintSequence], [AC_AR_SalesGroup], [AC_AR_ExpenseGroup], [AC_ChargeGroup], [AC_ChargeSubGroup], [AC_RateCalculator], [AC_DepartmentFilterList], [AC_IATA_ChargeCodeMap], [AC_GC], [AC_ENettChargeCodeMap], [AC_GoodsServiceType], [AC_ChargeOtherGroups], [AC_AX_TaxOverrideGroup], [AC_IsActive], [AC_IsGroupageCharge], [AC_ShowOnQuotation], [AC_SuppressOnQuoteIfZero], [AC_AllowDescriptionOvertype], [AC_IsCommissionable], [AC_InputGSTVATRecoverable], [AC_EnergySourceGroup], [AC_DefaultCommissionProduct], [AC_DefaultCommissionService], [AC_DefaultCommissionSubModule], [AC_AC_RevenueChargeCode], [AC_IsAdhocServiceCharge], [AC_GovtChargeCode], [AC_AG_CostClearingAccount], [AC_AG_RevenueClearingAccount], [AC_AutoVersion], [AC_AG_DisbursementShortfallAccount], [AC_AG_DisbursementSurplusAccount], [AC_SystemCreateTimeUtc], [AC_SystemCreateUser], [AC_SystemLastEditTimeUtc], [AC_SystemLastEditUser])
	VALUES (@accChargeCode1PK, N'OCAR', N'Miscellaneous Transport Change', N'', N'MRG', CAST(25.00 AS DECIMAL(5, 2)), NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, N'TBC', N'TST', N'FLT', N'ALL', N'', NULL, N'', N'SRV', N'PRC', NULL, 1, 1, 1, 0, 1, 1, CAST(1.0000 AS DECIMAL(5, 4)), N'', N'', N'', N'', NULL, 1, N'', NULL, NULL, 1, NULL, NULL, CAST(N'2015-04-27T06:46:00' AS SMALLDATETIME), N'MW ', CAST(N'2019-07-08T01:16:00' AS SMALLDATETIME), N'MW ')

INSERT [dbo].[JobConsolCost] ([E6_PK], [E6_IsValid], [E6_Sequence], [E6_AC_ChargeCode], [E6_InvoiceNum], [E6_InvoiceDate], [E6_RX_NKCurrency], [E6_OSCostAmount], [E6_OSGSTAmount], [E6_ExchangeRate], [E6_LocalCostAmount], [E6_OH_Creditor], [E6_PPDCLT], [E6_ApportionmentMethod], [E6_PaymentDate], [E6_PaymentType], [E6_AB_BankAccount], [E6_AK_ChequeBook], [E6_AT_TaxRate], [E6_AH_ARInvoice], [E6_AH_APInvoice], [E6_GC], [E6_ParentID], [E6_ParentTableCode], [E6_A9_VATClass], [E6_IsForCollectInvoice], [E6_ChequeOrReference], [E6_CostReference], [E6_Description], [E6_TaxDate], [E6_IsTaxAmountOverridden], [E6_PlaceOfSupply], [E6_PlaceOfSupplyType], [E6_GatewaySellChargeID], [E6_GS_NKConsolCostOwner], [E6_DocumentReceivedDate], [E6_AutoVersion], [E6_RatingBehaviour], [E6_SystemCreateTimeUtc], [E6_SystemCreateUser], [E6_SystemLastEditTimeUtc], [E6_SystemLastEditUser], [E6_SupplyType], [E6_GB_CostTaxBranch])
	VALUES (@jobConsollCost1PK, 0, 0, @accChargeCode1PK, N'', NULL, N'USD', 100.0000, 0.0000, CAST(1.000000000 AS DECIMAL(18, 9)), 100.0000, NULL, N'ALL', N'CHG', NULL, N'', NULL, NULL, NULL, NULL, NULL, @EDICompanyPk, @rs1PK, N'KG', NULL, 0, N'', N'', N'', NULL, 0, N'', N'', NULL, N'KRB', NULL, 0, N'REA', CAST(N'2022-06-23T00:24:00' AS SMALLDATETIME), N'KRB', CAST(N'2022-06-23T00:24:00' AS SMALLDATETIME), N'KRB', N'', NULL);
			";

			TestConnection.ExecuteNonQuery(sqlQuery);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Run Sheet Usage Statistics Records", 2, transactions.Count());
			AssertRowResult(transactions, "EDI", "SYD", "66a282e7-43a4-4f07-9898-d19c7235a8c5", "CR0001", "AIR", "ULD", "STD", "TST", $"{{\"SystemCreateTimeUtc\":\"2022-11-07T13:00:00\",\"DriverAllocated\":\"DR1\",\"TransportCompanyAllocated\":\"ARORG1\",\"PrimeMoverAllocated\":\"1111111\",\"StartTime\":\"2022-11-18T07:30:00+10:00\",\"EndTime\":\"2022-11-18T16:30:00+10:00\",\"TransitTime\":\"2022-11-18T12:43:00\",\"Duration\":2400,\"AdhocDriversLicence\":\"BY2233\",\"AdhocDriversName\":\"Terry\",\"AdhocTransportCoName\":\"Transport A\",\"AdhocTruckRegistration\":\"SB9481\",\"CountOfDepotInstructions\":1,\"CountOfDLVInstructions\":0,\"CountOfPICInstructions\":1,\"CountOfMLTInstructions\":1,\"CountOfAllocatedPICActions\":2,\"CountOfNotRejectedOrCompletedInstructions\":1,\"CountOfRejectedInstructions\":1,\"CountOfInstructions\":2,\"HasORGInstruction\":true,\"HasDSTInstruction\":true,\"HasBillingLinesAttached\":true,\"HasAdditionalService\":1,\"HasAdditionalReference\":1,\"CountOfAdditionalAssets\":1,\"CountOfAncillaryAssets\":1}}");
			AssertRowResult(transactions, "EDI", "SYD", "ed96517d-8b95-4dd2-9308-3b6717fcb17a", "CR0002", "SEA", "LCl", "EXP", "TS3", $"{{\"SystemCreateTimeUtc\":\"2022-11-08T12:59:00\",\"DriverAllocated\":\"DR2\",\"StartTime\":\"2022-11-18T07:20:00+10:00\",\"EndTime\":\"2022-11-18T11:30:00+10:00\",\"TransitTime\":\"2022-11-18T11:22:00\",\"Duration\":0,\"AdhocDriversLicence\":\"\",\"AdhocDriversName\":\"\",\"AdhocTransportCoName\":\"\",\"AdhocTruckRegistration\":\"\",\"CountOfDepotInstructions\":0,\"CountOfDLVInstructions\":1,\"CountOfPICInstructions\":0,\"CountOfMLTInstructions\":0,\"CountOfAllocatedPICActions\":0,\"CountOfNotRejectedOrCompletedInstructions\":0,\"CountOfRejectedInstructions\":1,\"CountOfInstructions\":1,\"HasORGInstruction\":false,\"HasDSTInstruction\":false,\"HasBillingLinesAttached\":false,\"HasAdditionalService\":0,\"HasAdditionalReference\":0,\"CountOfAdditionalAssets\":0,\"CountOfAncillaryAssets\":0}}");
		}

		void AssertRowResult(IEnumerable<IStlTransaction> transactions, string companyCode, string branchCode, string referenceGuid, string reference1, string reference2, string reference3, string reference4, string userCode, string additonalRefs)
		{
			var transaction = transactions.Single(t => t.Reference5.ToLower() == referenceGuid.ToLower());
			AssertEquals($"Company Code should be {companyCode}", companyCode, transaction.GetCompanyCode());
			AssertEquals($"Branch Code should be {branchCode}", branchCode, transaction.GetBranchCode());
			AssertEquals($"Billing Reference 1 should be {reference1}", reference1, transaction.Reference1);
			AssertEquals($"Billing Reference 2 should be {reference2}", reference2, transaction.Reference2);
			AssertEquals($"Billing Reference 3 should be {reference3}", reference3, transaction.Reference3);
			AssertEquals($"Billing Reference 4 should be {reference4}", reference4, transaction.Reference4);
			AssertEquals($"Creating User Code should be {userCode}", userCode, transaction.ClientStaffCode);

			AssertEquals("AdditionalRefs should be as expected", additonalRefs, transaction.AdditionalRefs);
		}
	}
}
