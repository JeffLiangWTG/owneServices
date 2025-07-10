using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderExportConsolByContainerTEU))]
	sealed class ForwarderExportConsolByContainerTEUTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => true;

		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @OhPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk05 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk06 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk07 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk08 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk09 UNIQUEIDENTIFIER = newid();

DECLARE @RcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk02 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk03 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk04 UNIQUEIDENTIFIER = newid();

DECLARE @OaPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk6 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk7 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk8 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk9 UNIQUEIDENTIFIER = NEWID();

DECLARE @RslPk01 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk02 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk03 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk04 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk05 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk06 UNIQUEIDENTIFIER = newid();

DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk11 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk12 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk13 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk14 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk15 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk16 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk17 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk18 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk19 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk20 UNIQUEIDENTIFIER = newid();

DECLARE @JddPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk11 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk12 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk13 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk14 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk15 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk16 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk17 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk18 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk19 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk20 UNIQUEIDENTIFIER = newid();

DECLARE @JwPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk16 UNIQUEIDENTIFIER = newid();

DECLARE @JcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk11 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk12 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk13 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk14 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk15 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk16 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk17 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk18 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk19 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk20 UNIQUEIDENTIFIER = newid();
DECLARE @JcPk21 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.RefShippingLine (RSL_PK, RSL_CarrierName, RSL_CargoWiseOneCode, RSL_IsSystem, RSL_IsActive, RSL_IsNVO, RSL_IsShippingLine, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_ShippingOrderAvailable) VALUES
	(@RslPk01, 'Yusen01', 'Y01', 0, 1, 0, 1, 1, 0, 0),
	(@RslPk02, 'DHL01', 'D01', 0, 1, 0, 1, 1, 0, 0),
	(@RslPk03, 'DNN01', 'D02', 0, 1, 1, 0, 0, 0, 0),
	(@RslPk04, 'Yusen02', 'Y02', 0, 1, 0, 1, 0, 1, 0),
	(@RslPk05, 'Yusen03', 'Y03', 0, 1, 0, 1, 0, 0, 1),
	(@RslPk06, 'Yusen04', 'Y04', 0, 1, 1, 1, 1, 1, 1);

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'VMLCCC', 'VML CCC', 1, @RslPk01),
	(@OhPk02, 1, 'VMLC1C', 'VML C1C', 1, @RslPk02),
	(@OhPk03, 1, 'KMJCCC', 'KMJ CCC', 1, null),
	(@OhPk04, 1, 'KMJC1C', 'KMJ C1C', 1, null),
	(@OhPk05, 1, 'XUJC1C', 'KMJ C1C', 1, null),
	(@OhPk06, 1, 'DNNC1C', 'DNN C1C', 1, @RslPk03),
	(@OhPk07, 1, 'VMKCCC', 'VMK CCC', 1, @RslPk04),
	(@OhPk08, 1, 'VMMCCC', 'VMM CCC', 1, @RslPk05),
	(@OhPk09, 1, 'ABCCCC', 'ABC CCC', 1, @RslPk06);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk1, @OhPk01, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk2, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk3, @OhPk03, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk4, @OhPk04, 1, '30', 'Me St', 'Mel', 3000, 'AU'),
	(@OaPk5, @OhPk05, 1, '30', 'Me St', 'Adg', 3000, 'AU'),
	(@OaPk6, @OhPk06, 1, '30', 'Vi St', 'Mel', 3000, 'AU'),
	(@OaPk7, @OhPk07, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk8, @OhPk08, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk9, @OhPk09, 1, '10', 'Pit St', 'SYD', 2000, 'AU');

INSERT dbo.OrgCusCode (OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(NEWID(), 1, 'HLCU', 'CCC', 'US', @OhPk01),
	(NEWID(), 1, 'XXCU', 'C1C', 'AU', @OhPk02),
	(NEWID(), 1, 'GLOU', 'CCC', 'US', @OhPk03),
	(NEWID(), 1, 'DYUI', 'C1C', 'AU', @OhPk04),
	(NEWID(), 1, 'DDNN', 'C1C', 'AU', @OhPk06),
	(NEWID(), 1, 'HLCX', 'CCC', 'US', @OhPk07),
	(NEWID(), 1, 'HLCY', 'CCC', 'US', @OhPk08),
	(NEWID(), 1, 'HLCY', 'CCC', 'US', @OhPk09);

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RN_NKCountryCode) VALUES (NEWID(), 'SY1', @GcPk01, @OhPk05, 'AU');

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'DVN', 'VN company', 'VND', 'VN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RN_NKCountryCode) VALUES (NEWID(), 'VN1', @GcPk02, NULL, 'VN');

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk03, 'DCN', 'CN company', 'CND', 'CN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RN_NKCountryCode) VALUES (NEWID(), 'CN1', @GcPk03, @OhPk08, 'CN');

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_BookingReference, JK_MasterBillNum, JK_CoLoadBookingReference, JK_CoLoadMasterBill, JK_IsForwarding, JK_AgentType, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_OA_CreditorAddress, JK_OA_SendingForwarderAddress, JK_SystemCreateTimeUtc) VALUES
	(@JkPk01, 'CON01', 'B01', 'M01', 'CB01', 'CM01', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk02, 'CON02', 'B02', 'M02', 'CB02', 'CM02', 1, 'AGT', 'SEA', 'FCL', @OaPk2, @OaPk4, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk03, 'CON03', 'B03', 'M03', 'CB03', 'CM03', 1, 'AGT', 'SEA', 'FCL', @OaPk2, @OaPk4, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk04, 'CON04', 'B04', 'M04', 'CB04', 'CM04', 1, 'AGT', 'SEA', 'FCL', @OaPk6, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk05, 'CON05', 'B05', 'M05', 'CB05', 'CM05', 1, 'CLD', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk06, 'CON06', 'B06', 'M06', 'CB06', 'CM06', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk07, 'CON07', 'B07', 'M07', 'CB07', 'CM07', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk08, 'CON08', 'B08', 'M08', 'CB08', 'CM08', 0, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk09, 'CON09', 'B09', 'M09', 'CB09', 'CM09', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk10, 'CON10', 'B10', 'M10', 'CB10', 'CM10', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk11, 'CON11', 'B11', 'M11', 'CB11', 'CM09', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk12, 'CON12', 'B12', 'M12', 'CB12', 'CM12', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk13, 'CON13', 'B13', 'M13', 'CB13', 'CM13', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk14, 'CON14', 'B14', 'M14', 'CB14', 'CM14', 1, 'CLD', 'SEA', 'FCL', @OaPk6, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk15, 'CON15', 'B15', 'M15', 'CB15', 'CM15', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk16, 'CON16', 'B16', 'M16', 'CB16', 'CM16', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk17, 'CON17', 'B09', 'M09', 'CB09', 'CM09', 1, 'AGT', 'SEA', 'FCL', @OaPk7, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk18, 'CON18', 'B09', 'M09', 'CB09', 'CM09', 1, 'AGT', 'SEA', 'FCL', @OaPk8, @OaPk3, @OaPk8, '2021-01-08 01:23:00'),
	(@JkPk19, 'CON19', 'B19', 'M19', 'CB19', 'CM19', 1, 'AGT', 'SEA', 'LCL', @OaPk9, @OaPk3, @OaPk5, '2021-01-08 01:23:00'),
	(@JkPk20, 'CON20', 'B20', 'M20', 'CB20', 'CM20', 1, 'AGT', 'SEA', 'FCL', @OaPk9, @OaPk3, @OaPk5, '2021-01-08 01:23:00');

INSERT dbo.JobDocumentData(JDD_PK, JDD_ParentTableCode, JDD_ParentID, JDD_Name, JDD_SystemCreateTimeUtc, JDD_SystemCreateUser, JDD_SystemLastEditTimeUtc, JDD_SystemLastEditUser) VALUES
	(@JddPk01, 'JK', @JkPk01, 'SeaBookingRequest2', '2021-03-01', 'US1', '2021-03-01', 'US1'),
	(@JddPk02, 'JK', @JkPk02, 'SeaBookingRequest2', '2021-03-01', 'US2', '2021-03-01', 'US2'),
	(@JddPk03, 'JK', @JkPk03, 'SeaBookingRequest2', '2021-03-01', 'US3', '2021-03-01', 'US3'),
	(@JddPk04, 'JK', @JkPk04, 'SeaBookingRequest2', '2021-03-01', 'US4', '2021-03-01', 'US4'),
	(@JddPk05, 'JK', @JkPk05, 'SeaBookingRequest2', '2021-03-01', 'US5', '2021-03-01', 'US5'),
	(@JddPk06, 'JK', @JkPk06, 'SeaBookingRequest2', '2021-03-01', 'US6', '2021-03-01', 'US6'),
	(@JddPk07, 'JK', @JkPk07, 'SeaBookingRequest2', '2021-03-01', 'US7', '2021-03-01', 'US7'),
	(@JddPk08, 'JK', @JkPk08, 'SeaBookingRequest2', '2021-03-01', 'US8', '2021-03-01', 'US8'),
	(@JddPk09, 'JK', @JkPk09, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk10, 'JK', @JkPk10, 'SeaBookingRequest2', '2021-03-01', 'US0', '2021-03-01', 'US0'),
	(@JddPk11, 'JK', @JkPk11, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk12, 'JK', @JkPk12, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk13, 'JK', @JkPk13, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk14, 'JK', @JkPk14, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk15, 'JK', @JkPk15, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk16, 'JK', @JkPk17, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk17, 'JK', @JkPk18, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk19, 'JK', @JkPk19, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9'),
	(@JddPk20, 'JK', @JkPk20, 'SeaBookingRequest2', '2021-03-01', 'US9', '2021-03-01', 'US9');

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_TransportMode, JW_ParentType, JW_ETD, JW_ATD) VALUES
	(@JwPk01, @JkPk01, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(@JwPk02, @JkPk02, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk03, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk04, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk05, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk06, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk07, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk08, 'SEA', 'CON', '2021-03-04', '2021-03-10'),
	(newID(), @JkPk09, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(@JwPk10, @JkPk10, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(@JwPk16, @JkPk16, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(newID(), @JkPk17, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(newID(), @JkPk18, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(newID(), @JkPk19, 'SEA', 'CON', '2021-03-03', '2021-03-09'),
	(newID(), @JkPk20, 'SEA', 'CON', '2021-03-03', '2021-03-09');

INSERT dbo.RefContainer (RC_PK, RC_TEU, RC_Code, RC_ISOType) VALUES
	(@RcPk01,  1, '20GX', '22G0'),
	(@RcPk02,  2, '40GX', '42G0'),
	(@RcPk03,  2.5, '60GX', 'L5G0'),
	(@RcPk04,  0, '80GX', 'A2G0');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RC, JC_ContainerNum, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(@JcPk01, @JkPk01, @RcPk01, 'HLCJ001', '2021-03-01', 'US1'),
	(@JcPk02, @JkPk02, @RcPk02, 'HLCJ002', '2021-03-02', 'US2'),
	(@JcPk03, @JkPk02, @RcPk02, 'HLCJ003', '2021-03-03', 'US2'),
	(@JcPk04, @JkPk03, @RcPk03, 'HLCJ004', '2021-02-04', 'US3'),
	(@JcPk05, @JkPk04, @RcPk01, 'HLCJ005', '2021-03-01', 'US4'),
	(@JcPk06, @JkPk05, @RcPk01, 'HLCJ006', '2021-03-01', 'US5'),
	(@JcPk07, @JkPk06, @RcPk01, 'HLCJ007', '2021-03-01', 'US6'),
	(@JcPk08, @JkPk07, @RcPk01, 'HLCJ008', '2021-03-01', 'US7'),
	(@JcPk09, @JkPk08, @RcPk01, 'HLCJ009', '2021-03-01', 'US8'),
	(@JcPk10, @JkPk09, @RcPk03, 'HLCJ010', '2021-03-01', 'US9'),
	(@JcPk11, @JkPk10, @RcPk04, 'HLCJ011', '2021-03-01', 'US0'),
	(@JcPk12, @JkPk11, @RcPk04, 'HLCJ012', '2021-03-01', 'US9'),
	(@JcPk13, @JkPk12, @RcPk04, '', '2021-03-01', 'US9'),
	(@JcPk14, @JkPk13, @RcPk04, 'HLCJ014', '2021-03-01', 'US9'),
	(@JcPk15, @JkPk14, @RcPk04, 'HLCJ015', '2021-03-01', 'US9'),
	(@JcPk16, @JkPk15, @RcPk04, 'HLCJ016', '2021-03-01', 'US9'),
	(@JcPk17, @JkPk16, @RcPk04, 'HLCJ017', '2021-03-01', 'US9'),
	(@JcPk18, @JkPk17, @RcPk03, 'HLCJ018', '2021-03-01', 'US9'),
	(@JcPk19, @JkPk18, @RcPk03, 'HLCJ019', '2021-03-01', 'US9'),
	(@JcPk20, @JkPk19, @RcPk03, 'HLCJ020', '2021-03-01', 'US9'),
	(@JcPk21, @JkPk20, @RcPk03, 'HLCJ021', '2021-03-01', 'US9');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES

-- this is to reproduce the situation when users have DEP events before MSN event (should not count)
(newid(), 'JobConsolTransport', '2021-02-09 02:23:00', '2021-02-09 02:23:00', @JwPk02, 'DEP', 'R2', 'SY1', 'N'),
(newid(), 'JobDocumentData', '2021-03-05 01:23:00', '2021-03-05 01:23:00', @JddPk02, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),

-- container departed (should not count - we look at DEP events on JobConsolTransport not JobContainer)
(newid(), 'JobContainer', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JcPk08, 'DEP', 'R7', 'SY1', 'N'),

-- user sent original and then another original (should count once)
(newid(), 'JobDocumentData', getdate(), '2021-03-05 00:23:00', @JddPk09, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk09, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 00:23:00', @JddPk16, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk16, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 00:23:00', @JddPk17, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'CN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk17, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'CN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk19, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'CN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk20, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'CN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk20, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'CN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk20, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'CN1', 'N'),

-- consol departed (should count)
(newid(), 'JobConsolTransport', '2021-03-06 01:23:00', '2021-03-06 01:23:00', @JwPk10, 'DEP', '10', 'SY1', 'N'),

-- user sent original in prev billing period and then another original in current billing period (should not count)
(newid(), 'JobDocumentData', getdate(), '2021-02-05 00:23:00', @JddPk11, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk11, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),

-- user sent original for container which doesn't have valid container number (should not count)
(newid(), 'JobDocumentData', getdate(), '2021-03-05 00:23:00', @JddPk12, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),

-- user send original 2 months prior the current billing collection period (should not count)
(newid(), 'JobDocumentData', getdate(), '2021-01-05 00:23:00', @JddPk13, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 00:23:00', @JddPk13, 'MSN', '|MST=Shipping Instruction|DEP=Carrier|RES=Amendment', 'VN1', 'N'),

-- user sent original but for non applicable CLD consol (should not count)
(newid(), 'JobDocumentData', getdate(), '2021-03-05 01:23:00', @JddPk14, 'MSN', '|MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),

-- original event created in prev collection range was cancelled and new event created in current collection range (should not count)
(newid(), 'JobConsolTransport', '2021-02-15 02:23:00', '2021-02-15 02:23:00', @JwPk16, 'DEP', 'R7', 'SY1', 'Y'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk16, 'DEP', 'R7', 'SY1', 'N');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertContainsExactElementsInAnyOrder("Expected 4 transactions for consols",
				new[] { "CON09", "CON10", "CON17", "CON18", "CON20" },
				transactions.Select(t => t.Reference1));

			var msnBasedRow1 = transactions.FirstOrDefault(t => t.Reference1 == "CON09");
			var msnBasedRow2 = transactions.FirstOrDefault(t => t.Reference1 == "CON17");
			var msnBasedRow3 = transactions.FirstOrDefault(t => t.Reference1 == "CON18");
			var depBasedRow = transactions.FirstOrDefault(t => t.Reference1 == "CON10");
			var msnBasedRow4 = transactions.FirstOrDefault(t => t.Reference1 == "CON20");

			Assert("CON03's Container ATD is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON03"));
			Assert("CON04's ShippingLine is NVO or not OceanCarrierMessagingAvailable", !transactions.Any(t => t.Reference1 == "CON04"));
			Assert("CON05 is CLD consol", !transactions.Any(t => t.Reference1 == "CON05"));
			Assert("CON06's Container ATD is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON06"));
			Assert("CON07's Container hasn't received FLO event", !transactions.Any(t => t.Reference1 == "CON07"));
			Assert("CON08 IsForwarding = 0", !transactions.Any(t => t.Reference1 == "CON08"));
			Assert("CON11 First MSN is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON11"));
			Assert("CON12 Container number is empty", !transactions.Any(t => t.Reference1 == "CON12"));
			Assert("CON13 MSN event is an amendment", !transactions.Any(t => t.Reference1 == "CON13"));
			Assert("CON14 is CLD NVOCC consol", !transactions.Any(t => t.Reference1 == "CON14"));
			Assert("CON15 First FLO's event date is not within billing period and previous billing period", !transactions.Any(t => t.Reference1 == "CON15"));
			Assert("CON16 First DEP was billed in previous period and then got cancelled", !transactions.Any(t => t.Reference1 == "CON16"));
			Assert("CON19's rsl is NVO and ShippingLine, but the container mode is LCL", !transactions.Any(t => t.Reference1 == "CON19"));

			CombineAssertions(() =>
			{
				AssertNotNull(msnBasedRow1);
				AssertEquals("[Row-0] CompanyCode", "DVN", msnBasedRow1.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "VN1", msnBasedRow1.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2021, 3, 5, 0, 23, 0), msnBasedRow1.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 2, msnBasedRow1.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "CON09", msnBasedRow1.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "B09", msnBasedRow1.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "M09", msnBasedRow1.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "HLCJ010/L5G0/2.50TEU", msnBasedRow1.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"MSN\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CoLoadBookingReference\":\"CB09\",\"CoLoadMasterBillNumber\":\"CM09\",\"CoLoadWith\":{\"OrgCode\":\"KMJCCC\",\"CarrierCode\":\"GLOU\"},\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"CarrierCode\":\"HLCU\"},\"IsNVO\":0,\"Container\":{\"ContainerNumber\":\"HLCJ010\",\"ContainerType\":\"60GX\",\"ISOCode\":\"L5G0\",\"TEU\":2.50}}"
					, msnBasedRow1.AdditionalRefs);

				AssertNotNull(msnBasedRow2);
				AssertEquals("[Row-1] CompanyCode", "DVN", msnBasedRow2.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "VN1", msnBasedRow2.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2021, 3, 5, 0, 23, 0), msnBasedRow2.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 2, msnBasedRow2.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "CON17", msnBasedRow2.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "B09", msnBasedRow2.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "M09", msnBasedRow2.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "HLCJ018/L5G0/2.50TEU", msnBasedRow2.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"MSN\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CoLoadBookingReference\":\"CB09\",\"CoLoadMasterBillNumber\":\"CM09\",\"CoLoadWith\":{\"OrgCode\":\"KMJCCC\",\"CarrierCode\":\"GLOU\"},\"ShippingLine\":{\"OrgCode\":\"VMKCCC\",\"CarrierCode\":\"HLCX\"},\"IsNVO\":0,\"Container\":{\"ContainerNumber\":\"HLCJ018\",\"ContainerType\":\"60GX\",\"ISOCode\":\"L5G0\",\"TEU\":2.50}}"
					, msnBasedRow2.AdditionalRefs);

				AssertNotNull(msnBasedRow3);
				AssertEquals("[Row-2] CompanyCode", "DCN", msnBasedRow3.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "CN1", msnBasedRow3.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2021, 3, 5, 0, 23, 0), msnBasedRow3.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 2, msnBasedRow3.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "CON18", msnBasedRow3.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "B09", msnBasedRow3.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "M09", msnBasedRow3.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "HLCJ019/L5G0/2.50TEU", msnBasedRow3.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"MSN\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CoLoadBookingReference\":\"CB09\",\"CoLoadMasterBillNumber\":\"CM09\",\"CoLoadWith\":{\"OrgCode\":\"KMJCCC\",\"CarrierCode\":\"GLOU\"},\"ShippingLine\":{\"OrgCode\":\"VMMCCC\",\"CarrierCode\":\"HLCY\"},\"IsNVO\":0,\"Container\":{\"ContainerNumber\":\"HLCJ019\",\"ContainerType\":\"60GX\",\"ISOCode\":\"L5G0\",\"TEU\":2.50}}"
					, msnBasedRow3.AdditionalRefs);

				AssertNotNull(depBasedRow);
				AssertEquals("[Row-3] CompanyCode", "DAU", depBasedRow.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "SY1", depBasedRow.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2021, 3, 6, 1, 23, 0), depBasedRow.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, depBasedRow.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "CON10", depBasedRow.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "B10", depBasedRow.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "M10", depBasedRow.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "HLCJ011/A2G0/1.18TEU", depBasedRow.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"DEP\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CoLoadBookingReference\":\"CB10\",\"CoLoadMasterBillNumber\":\"CM10\",\"CoLoadWith\":{\"OrgCode\":\"KMJCCC\",\"CarrierCode\":\"GLOU\"},\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"CarrierCode\":\"HLCU\"},\"IsNVO\":0,\"Container\":{\"ContainerNumber\":\"HLCJ011\",\"ContainerType\":\"80GX\",\"ISOCode\":\"A2G0\",\"TEU\":1.18}}"
					, depBasedRow.AdditionalRefs);

				AssertNotNull(msnBasedRow4);
				AssertEquals("[Row-4] CompanyCode", "DCN", msnBasedRow4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "CN1", msnBasedRow4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2021, 3, 5, 1, 23, 0), msnBasedRow4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 2, msnBasedRow4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "CON20", msnBasedRow4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "B20", msnBasedRow4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "M20", msnBasedRow4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "HLCJ021/L5G0/2.50TEU", msnBasedRow4.Reference4);
				AssertEquals("[Row-4] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"MSN\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CoLoadBookingReference\":\"CB20\",\"CoLoadMasterBillNumber\":\"CM20\",\"CoLoadWith\":{\"OrgCode\":\"KMJCCC\",\"CarrierCode\":\"GLOU\"},\"ShippingLine\":{\"OrgCode\":\"ABCCCC\",\"CarrierCode\":\"HLCY\"},\"IsNVO\":0,\"Container\":{\"ContainerNumber\":\"HLCJ021\",\"ContainerType\":\"60GX\",\"ISOCode\":\"L5G0\",\"TEU\":2.50}}"
					, msnBasedRow4.AdditionalRefs);
			});
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2021, 3);
			}
		}
	}
}
