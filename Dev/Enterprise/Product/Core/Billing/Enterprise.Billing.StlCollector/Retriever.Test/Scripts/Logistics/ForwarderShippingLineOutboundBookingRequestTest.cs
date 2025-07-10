using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderShippingLineOutboundBookingRequest))]
	sealed class ForwarderShippingLineOutboundBookingRequestTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => false;

		protected override void PrepareTestData()
		{
			var sqlText = @"
DECLARE @RcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk02 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk03 UNIQUEIDENTIFIER = newid();
DECLARE @RcPk04 UNIQUEIDENTIFIER = newid();

DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk09 UNIQUEIDENTIFIER = NEWID();

DECLARE @OaPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk6 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk7 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk8 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk9 UNIQUEIDENTIFIER = NEWID();

DECLARE @RslPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk09 UNIQUEIDENTIFIER = NEWID();

DECLARE @JkPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk09 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk10 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk14 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk15 UNIQUEIDENTIFIER = NEWID();

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

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.RefShippingLine (RSL_PK, RSL_CarrierName, RSL_CargoWiseOneCode, RSL_IsSystem, RSL_IsActive, RSL_IsNVO, RSL_IsShippingLine, RSL_BookingRequestAvailable) VALUES
	(@RslPk01, 'Yusen01', 'Y01', 0, 1, 0, 1, 1),
	(@RslPk02, 'DHL01', 'D01', 0, 1, 0, 1, 1),
	(@RslPk03, 'DNN01', 'D02', 0, 1, 1, 0, 0),
	(@RslPk07, 'SF', 'S02', 0, 1, 0, 1, 0),
	(@RslPk08, 'JD', 'J02', 0, 1, 0, 1, 1),
	(@RslPk09, 'IQ', 'I01', 0, 1, 1, 1, 1);

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'VMLCCC', 'VML CCC', 1, @RslPk01),
	(@OhPk02, 1, 'VMLC1C', 'VML C1C', 1, @RslPk02),
	(@OhPk03, 1, 'KMJCCC', 'KMJ CCC', 1, null),
	(@OhPk04, 1, 'KMJC1C', 'KMJ C1C', 1, null),
	(@OhPk05, 1, 'XUJC1C', 'KMJ C1C', 1, null),
	(@OhPk06, 1, 'DNNC1C', 'DNN C1C', 1, @RslPk03),
	(@OhPk07, 1, 'SFC1C', 'SF C1C', 1, @RslPk07),
	(@OhPk08, 1, 'JDC1C', 'JD C1C', 1, @RslPk08),
	(@OhPk09, 1, 'IQC1C', 'IQ C1C', 1, @RslPk09);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk1, @OhPk01, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk2, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk3, @OhPk03, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk4, @OhPk04, 1, '30', 'Me St', 'Mel', 3000, 'AU'),
	(@OaPk5, @OhPk05, 1, '30', 'Me St', 'Adg', 3000, 'AU'),
	(@OaPk6, @OhPk06, 1, '30', 'Vi St', 'Mel', 3000, 'AU'),
	(@OaPk7, @OhPk07, 1, '30', 'SF', 'XXX', 3000, 'AU'),
	(@OaPk8, @OhPk08, 1, '30', 'JD', 'ZZZ', 3000, 'AU'),
	(@OaPk9, @OhPk09, 1, '30', 'IQ', 'III', 3000, 'AU');

INSERT dbo.OrgCusCode (OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(NEWID(), 1, 'HLCU', 'CCC', 'US', @OhPk01),
	(NEWID(), 1, 'XXCU', 'C1C', 'AU', @OhPk02),
	(NEWID(), 1, 'GLOU', 'CCC', 'US', @OhPk03),
	(NEWID(), 1, 'DYUI', 'C1C', 'AU', @OhPk04),
	(NEWID(), 1, 'DDNN', 'C1C', 'AU', @OhPk06),
	(NEWID(), 1, 'XXXX', 'C1C', 'AU', @OhPk07),
	(NEWID(), 1, 'ZZZZ', 'C1C', 'AU', @OhPk08),
	(NEWID(), 1, 'IIII', 'C1C', 'AU', @OhPk09);

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_BookingReference, JK_MasterBillNum, JK_CoLoadBookingReference, JK_CoLoadMasterBill, JK_IsForwarding, JK_AgentType, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_OA_CreditorAddress, JK_OA_SendingForwarderAddress) VALUES
	(@JkPk01, 'CON01', 'B01', 'M01', 'CB01', 'CM01', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5),
	(@JkPk02, 'CON02', 'B02', 'M02', 'CB02', 'CM02', 1, 'AGT', 'SEA', 'FCL', @OaPk2, @OaPk4, @OaPk5),
	(@JkPk03, 'CON03', 'B03', 'M03', 'CB03', 'CM03', 1, 'AGT', 'SEA', 'FCL', @OaPk2, @OaPk4, @OaPk5),
	(@JkPk04, 'CON04', 'B04', 'M04', 'CB04', 'CM04', 1, 'AGT', 'SEA', 'FCL', @OaPk6, @OaPk3, @OaPk5),
	(@JkPk05, 'CON05', 'B05', 'M05', 'CB05', 'CM05', 1, 'CLD', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5),
	(@JkPk06, 'CON06', 'B06', 'M06', 'CB06', 'CM06', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5),
	(@JkPk07, 'CON07', 'B07', 'M07', 'CB07', 'CM07', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5),
	(@JkPk08, 'CON08', 'B08', 'M08', 'CB08', 'CM08', 0, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5),
	(@JkPk09, 'CON09', 'B09', 'M09', 'CB09', 'CM09', 1, 'AGT', 'AIR', 'LSE', @OaPk1, @OaPk3, @OaPk3),
	(@JkPk10, 'CON10', 'B10', 'M10', 'CB10', 'CM10', 1, 'AGT', 'SEA', 'FCL', @OaPk1, @OaPk3, @OaPk5),
	(@JkPk12, 'CON12', 'B12', 'M12', 'CB12', 'CM12', 1, 'AGT', 'SEA', 'FCL', @OaPk7, @OaPk3, @OaPk5),
	(@JkPk13, 'CON13', 'B13', 'M13', 'CB13', 'CM13', 1, 'AGT', 'SEA', 'FCL', @OaPk8, @OaPk3, @OaPk5),
	(@JkPk14, 'CON14', 'B14', 'M14', 'CB14', 'CM14', 1, 'AGT', 'SEA', 'LCL', @OaPk9, @OaPk3, @OaPk5),
	(@JkPk15, 'CON15', 'B15', 'M15', 'CB15', 'CM15', 1, 'AGT', 'SEA', 'FCL', @OaPk9, @OaPk3, @OaPk5);

INSERT dbo.RefContainer (RC_PK, RC_TEU, RC_Code, RC_ISOType) VALUES
	(@RcPk01,  1, '20GX', '22G0'),
	(@RcPk02,  2, '40GX', '42G0'),
	(@RcPk03,  3, '60GX', 'A2G0'),
	(@RcPk04,  0, '80GX', 'A2G0');

INSERT dbo.JobContainer (JC_PK, JC_JK, JC_RC, JC_ContainerNum, JC_SystemCreateTimeUtc, JC_SystemCreateUser) VALUES
	(@JcPk01, @JkPk01, @RcPk01, 'HLCJ001', '2021-03-01', 'US1'),
	(@JcPk11, @JkPk01, @RcPk01, 'HLCJ011', '2021-03-01', 'US1'),
	(@JcPk02, @JkPk02, @RcPk02, 'HLCJ002', '2021-03-02', 'US2'),
	(@JcPk03, @JkPk03, @RcPk02, 'HLCJ003', '2021-03-03', 'US3'),
	(@JcPk04, @JkPk04, @RcPk03, 'HLCJ004', '2021-02-04', 'US4'),
	(@JcPk05, @JkPk05, @RcPk01, 'HLCJ005', '2021-03-01', 'US5'),
	(@JcPk06, @JkPk06, @RcPk01, 'HLCJ006', '2021-03-01', 'US6'),
	(@JcPk07, @JkPk07, @RcPk01, 'HLCJ007', '2021-03-01', 'US7'),
	(@JcPk08, @JkPk08, @RcPk01, 'HLCJ008', '2021-03-01', 'US8'),
	(@JcPk09, @JkPk09, @RcPk01, 'HLCJ009', '2021-03-01', 'US9'),
	(@JcPk10, @JkPk10, @RcPk03, 'HLCJ010', '2021-03-01', 'US0'),
	(@JcPk12, @JkPk12, @RcPk01, 'HLCJ012', '2021-03-01', 'US1'),
	(@JcPk13, @JkPk13, @RcPk01, 'HLCJ013', '2021-03-01', 'US1'),
	(@JcPk14, @JkPk14, @RcPk01, 'HLCJ014', '2021-03-01', 'US1'),
	(@JcPk15, @JkPk15, @RcPk01, 'HLCJ015', '2021-03-01', 'US1');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobConsol', getdate(), '2021-03-08 00:23:00', @JkPk01, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 01:23:00', @JkPk01, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 02:23:00', @JkPk01, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 03:23:00', @JkPk01, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-02-09 02:23:00', @JkPk02, 'MSN', 'MST=BOOKING REQUEST', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk02, 'ISN', 'MST=BOOKING REQUEST', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-02-08 02:23:00', @JkPk03, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-02-09 02:23:00', @JkPk03, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 01:23:00', @JkPk04, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk04, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 02:23:00', @JkPk05, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk05, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-04-09 01:23:00', @JkPk06, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-04-09 02:23:00', @JkPk06, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 02:23:00', @JkPk07, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk07, 'MRJ', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 01:23:00', @JkPk08, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk08, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk09, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 01:23:00', @JkPk09, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-01-08 01:23:00', @JkPk10, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 01:23:00', @JkPk10, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 00:23:00', @JkPk12, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 12:23:00', @JkPk12, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 00:23:00', @JkPk13, 'MSN', 'MST=Booking Request', 'SY1', 'Y'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 13:23:00', @JkPk13, 'ISN', 'MST=Booking Request', 'SY1', 'Y'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 00:23:01', @JkPk14, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 13:23:01', @JkPk14, 'ISN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 00:23:02', @JkPk15, 'MSN', 'MST=Booking Request', 'SY1', 'N'),
	(newid(), 'JobConsol', getdate(), '2021-03-08 13:23:02', @JkPk15, 'ISN', 'MST=Booking Request', 'SY1', 'N');

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU', @OhPk05);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES (NEWID(), 'SY1', @GcPk01);
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());

			Assert("CON03 container's SBK message PostedTime is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON03"));
			Assert("CON04's ShippingLine is NVO or not OceanCarrierMessagingAvailable", !transactions.Any(t => t.Reference1 == "CON04"));
			Assert("CON05 is CLD consol", !transactions.Any(t => t.Reference1 == "CON05"));
			Assert("CON06 container's SBK message PostedTime is not in DateTime range", !transactions.Any(t => t.Reference1 == "CON06"));
			Assert("CON07 container hasn't received ISN with MST=Booking Request", !transactions.Any(t => t.Reference1 == "CON07"));
			Assert("CON08 IsForwarding = 0", !transactions.Any(t => t.Reference1 == "CON08"));
			Assert("CON09 is not SEA consol", !transactions.Any(t => t.Reference1 == "CON09"));
			Assert("CON10 container doesn't have MSN event in one month before ISN", !transactions.Any(t => t.Reference1 == "CON10"));
			Assert("CON12's RSL_BookingRequestAvailable is false", !transactions.Any(t => t.Reference1 == "CON12"));
			Assert("CON14's rsl is NVO and ShippingLine, but the container mode is LCL", !transactions.Any(t => t.Reference1 == "CON14"));

			CombineAssertions(() =>
			{
				var transaction1 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 1, 23, 0));
				AssertEquals("[Row-0] CompanyCode", "DAU", transaction1.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "SY1", transaction1.GetBranchCode());
				AssertEquals("[Row-0] ItemCount", 1, transaction1.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "CON01", transaction1.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "B01", transaction1.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "M01", transaction1.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "Booking Request", transaction1.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ConsolNumber\":\"CON01\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B01\",\"CoLoadBookingConfirmationReference\":\"CB01\",\"WayBillNumber\":\"M01\",\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"C1CCode\":\"HLCU\"},\"IsNVO\":0}",
					transaction1.AdditionalRefs);

				var transaction2 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 3, 23, 0));
				AssertEquals("[Row-1] CompanyCode", "DAU", transaction2.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "SY1", transaction2.GetBranchCode());
				AssertEquals("[Row-1] ItemCount", 1, transaction2.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "CON01", transaction2.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "B01", transaction2.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "M01", transaction2.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "Booking Request", transaction2.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ConsolNumber\":\"CON01\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B01\",\"CoLoadBookingConfirmationReference\":\"CB01\",\"WayBillNumber\":\"M01\",\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"C1CCode\":\"HLCU\"},\"IsNVO\":0}",
					transaction2.AdditionalRefs);

				var transaction3 = FindRowByOccured(transactions, new DateTime(2021, 3, 9, 2, 23, 0));
				AssertEquals("[Row-2] CompanyCode", "DAU", transaction3.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "SY1", transaction3.GetBranchCode());
				AssertEquals("[Row-2] ItemCount", 1, transaction3.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "CON02", transaction3.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "B02", transaction3.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "M02", transaction3.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "Booking Request", transaction3.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ConsolNumber\":\"CON02\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B02\",\"CoLoadBookingConfirmationReference\":\"CB02\",\"WayBillNumber\":\"M02\",\"ShippingLine\":{\"OrgCode\":\"VMLC1C\",\"C1CCode\":\"XXCU\"},\"IsNVO\":0}",
					transaction3.AdditionalRefs);

				var transaction4 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 13, 23, 0));
				AssertNotNull("ISN and MSN are cancelled", transaction4);
				AssertEquals("[Row-3] CompanyCode", "DAU", transaction4.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "SY1", transaction4.GetBranchCode());
				AssertEquals("[Row-3] ItemCount", 1, transaction4.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "CON13", transaction4.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "B13", transaction4.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "M13", transaction4.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "Booking Request", transaction4.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ConsolNumber\":\"CON13\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B13\",\"CoLoadBookingConfirmationReference\":\"CB13\",\"WayBillNumber\":\"M13\",\"ShippingLine\":{\"OrgCode\":\"JDC1C\",\"C1CCode\":\"ZZZZ\"},\"IsNVO\":0}",
					transaction4.AdditionalRefs);

				var transaction5 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 13, 23, 2));
				AssertEquals("[Row-4] CompanyCode", "DAU", transaction5.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "SY1", transaction5.GetBranchCode());
				AssertEquals("[Row-4] ItemCount", 1, transaction5.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "CON15", transaction5.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "B15", transaction5.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "M15", transaction5.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "Booking Request", transaction5.Reference4);
				AssertEquals("[Row-4] AdditionalRefs", "{\"ConsolNumber\":\"CON15\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B15\",\"CoLoadBookingConfirmationReference\":\"CB15\",\"WayBillNumber\":\"M15\",\"ShippingLine\":{\"OrgCode\":\"IQC1C\",\"C1CCode\":\"IIII\"},\"IsNVO\":0}",
					transaction5.AdditionalRefs);
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
