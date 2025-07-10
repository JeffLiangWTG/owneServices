using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderExportConsol))]
	sealed class ForwarderExportConsolTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = $@"
DECLARE @OhPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk05 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk06 UNIQUEIDENTIFIER = newid();

DECLARE @RcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk02 UNIQUEIDENTIFIER = newid();

DECLARE @OaPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk6 UNIQUEIDENTIFIER = NEWID();

DECLARE @RslPk01 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk02 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk03 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk04 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk05 UNIQUEIDENTIFIER = newid();

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
DECLARE @JddPk15 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk16 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk17 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk18 UNIQUEIDENTIFIER = newid();
DECLARE @JddPk19 UNIQUEIDENTIFIER = newid();

DECLARE @JwPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk10 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk11 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk12 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk13 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk14 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk15 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk16 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk17 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk18 UNIQUEIDENTIFIER = newid();
DECLARE @JwPk19 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.RefShippingLine (RSL_PK, RSL_CarrierName, RSL_CargoWiseOneCode, RSL_IsSystem, RSL_IsActive, RSL_IsNVO, RSL_IsShippingLine, RSL_BookingRequestAvailable, RSL_ShippingInstructionAvailable, RSL_ShippingOrderAvailable) VALUES
	(@RslPk01, 'Yusen01', 'Y01', 0, 1, 1, 0, 1, 0, 0),
	(@RslPk02, 'DHL01', 'D01', 0, 1, 1, 0, 1, 0, 0),
	(@RslPk03, 'Yusen02', 'Y02', 0, 1, 1, 0, 0, 1, 0),
	(@RslPk04, 'Yusen03', 'Y03', 0, 1, 1, 0, 0, 0, 1),
	(@RslPk05, 'Yusen05', 'Y05', 0, 1, 1, 1, 1, 1, 1);

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_IsSeaWholesaler, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'VMLCCC', 'VML CCC', 0, 1, @RslPk01),
	(@OhPk02, 1, 'VMLC1C', 'VML C1C', 0, 1, @RslPk02),
	(@OhPk03, 1, 'KMJCCC', 'KMJ C1C', 0, 1, null),
	(@OhPk04, 1, 'VMKCCC', 'VMK CCC', 0, 1, @RslPk03),
	(@OhPk05, 1, 'VMMCCC', 'VMM CCC', 0, 1, @RslPk04),
	(@OhPk06, 1, 'ABCCCC', 'ABC CCC', 1, 1, @RslPk05);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk1, @OhPk01, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk2, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk3, @OhPk03, 1, '20', 'Ge St', 'Mel', 4000, 'AU'),
	(@OaPk4, @OhPk04, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk5, @OhPk05, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk6, @OhPk06, 1, '10', 'Pit St', 'SYD', 2000, 'AU');

INSERT dbo.OrgCusCode (OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(NEWID(), 1, 'HLCU', 'CCC', 'US', @OhPk01),
	(NEWID(), 1, 'XXCU', 'C1C', 'AU', @OhPk02),
	(NEWID(), 1, 'HLCX', 'CCC', 'US', @OhPk04),
	(NEWID(), 1, 'HLCY', 'CCC', 'US', @OhPk05),
	(NEWID(), 1, 'HLCY', 'CCC', 'US', @OhPk06);

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (NEWID(), 'SY1', @GcPk01, @OhPk03);

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'DVN', 'VN company', 'VND', 'VN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (NEWID(), 'VN1', @GcPk02, NULL);

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_BookingReference, JK_MasterBillNum, JK_CoLoadBookingReference, JK_CoLoadMasterBill, JK_IsForwarding, JK_AgentType, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_OA_CreditorAddress, JK_OA_SendingForwarderAddress, JK_SystemCreateTimeUtc) VALUES
	(@JkPk01, 'CON01', 'B01', 'M01', 'CB01', 'CM01', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk02, 'CON02', 'B02', 'M02', 'CB02', 'CM02', 1, 'CLD', 'SEA', 'FCL', null, @OaPk2, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk03, 'CON03', 'B03', 'M03', 'CB03', 'CM03', 1, 'CLD', 'SEA', 'FCL', null, @OaPk2, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk04, 'CON04', 'B04', 'M04', 'CB04', 'CM04', 1, 'AGT', 'SEA', 'FCL', null, @OaPk2, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk05, 'CON05', 'B05', 'M05', 'CB05', 'CM05', 1, 'CLD', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk06, 'CON06', 'B06', 'M06', 'CB06', 'CM06', 0, 'CLD', 'SEA', 'FCL', null, @OaPk2, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk07, 'CON07', 'B07', 'M07', 'CB07', 'CM07', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk08, 'CON08', 'B08', 'M08', 'CB08', 'CM08', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk09, 'CON09', 'B09', 'M09', 'CB09', 'CM09', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk10, 'CON10', 'B10', 'M10', 'CB10', 'CM10', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk11, 'CON11', 'B11', 'M11', 'CB11', 'CM11', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk12, 'CON12', 'B12', 'M12', 'CB12', 'CM12', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk13, 'CON13', 'B13', 'M13', 'CB13', 'CM13', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk14, 'CON14', 'B14', 'M14', 'CB14', 'CM14', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk15, 'CON15', 'B01', 'M01', 'CB01', 'CM01', 1, 'AGT', 'SEA', 'FCL', @OaPk4, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk16, 'CON16', 'B01', 'M01', 'CB01', 'CM01', 1, 'AGT', 'SEA', 'FCL', @OaPk5, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk17, 'CON17', 'B17', 'M17', 'CB17', 'CM17', 1, 'AGT', 'SEA', 'LCL', @OaPk6, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk18, 'CON18', 'B18', 'M18', 'CB18', 'CM18', 1, 'AGT', 'SEA', 'FCL', @OaPk6, null, @OaPk3, '2021-01-08 01:23:00'),
	(@JkPk19, 'CON19', 'B19', 'M19', 'CB19', 'CM19', 1, 'CLD', 'SEA', 'FCL', null, @OaPk6, @OaPk3, '2021-01-08 01:23:00');

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_ParentGUID, JW_TransportMode, JW_ParentType, JW_ETD, JW_ATD, JW_SystemCreateTimeUtc) VALUES
	(@JwPk01, @JkPk01, 'SEA', 'CON', '2021-03-03', '2021-03-09', '2021-01-09'),
	(@JwPk02, @JkPk02, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk03, @JkPk03, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk04, @JkPk04, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk05, @JkPk05, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk06, @JkPk06, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk07, @JkPk07, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk08, @JkPk08, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk09, @JkPk09, 'AIR', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk10, @JkPk10, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk11, @JkPk11, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk12, @JkPk12, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk13, @JkPk13, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk14, @JkPk14, 'SEA', 'CON', '2021-03-04', '2021-03-10', '2021-01-09'),
	(@JwPk15, @JkPk15, 'SEA', 'CON', '2021-03-03', '2021-03-09', '2021-01-09'),
	(@JwPk16, @JkPk16, 'SEA', 'CON', '2021-03-03', '2021-03-09', '2021-01-09'),
	(@JwPk17, @JkPk17, 'SEA', 'CON', '2021-03-03', '2021-03-09', '2021-01-09'),
	(@JwPk18, @JkPk18, 'SEA', 'CON', '2021-03-03', '2021-03-09', '2021-01-09'),
	(@JwPk19, @JkPk19, 'SEA', 'CON', '2021-03-03', '2021-03-09', '2021-01-09');

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
	(@JddPk15, 'JK', @JkPk15, 'SeaBookingRequest2', '2021-03-01', 'US1', '2021-03-01', 'US1'),
	(@JddPk16, 'JK', @JkPk16, 'SeaBookingRequest2', '2021-03-01', 'US1', '2021-03-01', 'US1'),
	(@JddPk17, 'JK', @JkPk17, 'SeaBookingRequest2', '2021-03-01', 'US1', '2021-03-01', 'US1'),
	(@JddPk18, 'JK', @JkPk18, 'SeaBookingRequest2', '2021-03-01', 'US1', '2021-03-01', 'US1'),
	(@JddPk19, 'JK', @JkPk19, 'SeaBookingRequest2', '2021-03-01', 'US1', '2021-03-01', 'US1');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk01, 'DEP', 'R1', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk02, 'DEP', 'R2', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-02-09 01:00:00', '2021-02-09 01:00:00', @JwPk03, 'DEP', 'R3', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk03, 'DEP', 'R3', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk04, 'DEP', 'R4', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk05, 'DEP', 'R5', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk06, 'DEP', 'R6', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-04-09 02:23:00', '2021-04-09 02:23:00', @JwPk07, 'DEP', 'R7', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk08, 'ARR', 'R8', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-09 02:23:00', '2021-03-09 02:23:00', @JwPk09, 'DEP', 'R9', 'SY1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk10, 'MSN', 'MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobConsolTransport', '2021-02-09 01:23:00', '2021-02-09 01:23:00', @JwPk10, 'DEP', 'R9', 'SY1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-02-09 02:23:00', @JddPk11, 'MSN', 'MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-09 02:23:00', @JddPk11, 'MSN', 'MST=Shipping Instruction|DEP=Carrier', 'VN1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-08 02:23:00', '2021-03-08 02:23:00', @JwPk11, 'DEP', 'R9', 'SY1', 'N'),
(newid(), 'JobDocumentData', getdate(), '2021-03-05 00:23:00', @JddPk12, 'MSN', 'MST=Shipping Instruction|DEP=Carrier|RES=Amendment', 'VN1', 'N'),
(newid(), 'JobConsolTransport', '2021-01-07 02:23:00', '2021-03-08 02:23:00', @JwPk13, 'DEP', '13', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-02-15 01:23:00', '2021-02-15 01:23:00', @JwPk14, 'DEP', 'R1', 'SY1', 'Y'),
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk14, 'DEP', 'R1', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk15, 'DEP', 'R1', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk16, 'DEP', 'R1', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk17, 'DEP', 'R1', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk18, 'DEP', 'R1', 'SY1', 'N'),
(newid(), 'JobConsolTransport', '2021-03-08 01:23:00', '2021-03-08 01:23:00', @JwPk19, 'DEP', 'R1', 'SY1', 'N');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 6, transactions.Count());

			Assert("CON03's ATD is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON03"));
			Assert("AGT Consol without ShippingLine address", !transactions.Any(t => t.Reference1 == "CON04"));
			Assert("CLD Consol without Creditor address", !transactions.Any(t => t.Reference1 == "CON05"));
			Assert("CON06 IsForwarding = 0", !transactions.Any(t => t.Reference1 == "CON06"));
			Assert("CON07's ATD is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON07"));
			Assert("CON08 hasn't received DEP event", !transactions.Any(t => t.Reference1 == "CON08"));
			Assert("CON09 does not have SEA transport", !transactions.Any(t => t.Reference1 == "CON09"));
			Assert("CON11 First MSN is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON11"));
			Assert("CON12 MSN event is an amendment", !transactions.Any(t => t.Reference1 == "CON12"));
			Assert("CON13 First DEP's event date is not within billing period and previous billing period", !transactions.Any(t => t.Reference1 == "CON13"));
			Assert("CON14 First DEP is billed in previous period and then got cancelled", !transactions.Any(t => t.Reference1 == "CON14"));
			Assert("non-CLD CON18's rslShippingLine is NVO and ShippingLine, but the container mode is not LCL", !transactions.Any(t => t.Reference1 == "CON18"));

			CombineAssertions(() =>
			{
				var transaction1 = FindRowByRef1(transactions, "CON01");
				AssertEquals("[Row-0] CompanyCode", "DAU", transaction1.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "SY1", transaction1.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2021, 3, 8, 1, 23, 0), transaction1.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, transaction1.BillableCount);
				AssertEquals("[Row-0] TransactionReference02", "B01", transaction1.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "M01", transaction1.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "NVOCC", transaction1.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"DEP\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B01\",\"CoLoadBookingConfirmationReference\":\"CB01\",\"CoLoadMasterBillNumber\":\"CM01\",\"WayBillNumber\":\"M01\",\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"CarrierCode\":\"HLCU\"},\"IsNVO\":1}"
					, transaction1.AdditionalRefs);

				var transaction2 = FindRowByRef1(transactions, "CON15");
				AssertEquals("[Row-1] CompanyCode", "DAU", transaction2.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "SY1", transaction2.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2021, 3, 8, 1, 23, 0), transaction2.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, transaction2.BillableCount);
				AssertEquals("[Row-1] TransactionReference02", "B01", transaction2.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "M01", transaction2.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "NVOCC", transaction2.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"DEP\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B01\",\"CoLoadBookingConfirmationReference\":\"CB01\",\"CoLoadMasterBillNumber\":\"CM01\",\"WayBillNumber\":\"M01\",\"ShippingLine\":{\"OrgCode\":\"VMKCCC\",\"CarrierCode\":\"HLCX\"},\"IsNVO\":1}"
					, transaction2.AdditionalRefs);

				var transaction3 = FindRowByRef1(transactions, "CON02");
				AssertEquals("[Row-2] CompanyCode", "DAU", transaction3.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "SY1", transaction3.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), transaction3.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, transaction3.BillableCount);
				AssertEquals("[Row-2] TransactionReference02", "CB02", transaction3.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "CM02", transaction3.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "NVOCC", transaction3.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ConsolType\":\"CLD\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"DEP\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B02\",\"CoLoadBookingConfirmationReference\":\"CB02\",\"CoLoadMasterBillNumber\":\"CM02\",\"WayBillNumber\":\"M02\",\"CoLoadWith\":{\"OrgCode\":\"VMLC1C\",\"CarrierCode\":\"XXCU\"},\"IsNVO\":1}"
					, transaction3.AdditionalRefs);

				var transaction4 = FindRowByRef1(transactions, "CON10");
				AssertEquals("[Row-3] CompanyCode", "DVN", transaction4.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "VN1", transaction4.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2021, 3, 9, 2, 23, 0), Convert.ToDateTime(transaction4.ServiceOccuredUTC));
				AssertEquals("[Row-3] ItemCount", 1, Convert.ToInt32(transaction4.BillableCount));
				AssertEquals("[Row-3] TransactionReference02", "B10", transaction4.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "M10", transaction4.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "NVOCC", transaction4.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"MSN\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B10\",\"CoLoadBookingConfirmationReference\":\"CB10\",\"CoLoadMasterBillNumber\":\"CM10\",\"WayBillNumber\":\"M10\",\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"CarrierCode\":\"HLCU\"},\"IsNVO\":1}"
					, transaction4.AdditionalRefs);

				var transaction5 = FindRowByRef1(transactions, "CON17");
				AssertEquals("[Row-4] CompanyCode", "DAU", transaction5.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "SY1", transaction5.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2021, 3, 8, 1, 23, 0), Convert.ToDateTime(transaction5.ServiceOccuredUTC));
				AssertEquals("[Row-4] ItemCount", 1, Convert.ToInt32(transaction5.BillableCount));
				AssertEquals("[Row-4] TransactionReference02", "B17", transaction5.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "M17", transaction5.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "NVOCC", transaction5.Reference4);
				AssertEquals("[Row-4] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"DEP\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B17\",\"CoLoadBookingConfirmationReference\":\"CB17\",\"CoLoadMasterBillNumber\":\"CM17\",\"WayBillNumber\":\"M17\",\"ShippingLine\":{\"OrgCode\":\"ABCCCC\",\"CarrierCode\":\"HLCY\"},\"IsNVO\":1}"
					, transaction5.AdditionalRefs);

				var transaction6 = FindRowByRef1(transactions, "CON19");
				AssertEquals("[Row-5] CompanyCode", "DAU", transaction6.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "SY1", transaction6.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2021, 3, 8, 1, 23, 0), Convert.ToDateTime(transaction6.ServiceOccuredUTC));
				AssertEquals("[Row-5] ItemCount", 1, Convert.ToInt32(transaction6.BillableCount));
				AssertEquals("[Row-5] TransactionReference02", "CB19", transaction6.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "CM19", transaction6.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "NVOCC", transaction6.Reference4);
				AssertEquals("[Row-5] AdditionalRefs", "{\"ConsolType\":\"CLD\",\"ConsolCreateDate\":\"2021-01-08T01:23:00\",\"EventType\":\"DEP\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B19\",\"CoLoadBookingConfirmationReference\":\"CB19\",\"CoLoadMasterBillNumber\":\"CM19\",\"WayBillNumber\":\"M19\",\"CoLoadWith\":{\"OrgCode\":\"ABCCCC\",\"CarrierCode\":\"HLCY\"},\"IsNVO\":1}"
					, transaction6.AdditionalRefs);
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
