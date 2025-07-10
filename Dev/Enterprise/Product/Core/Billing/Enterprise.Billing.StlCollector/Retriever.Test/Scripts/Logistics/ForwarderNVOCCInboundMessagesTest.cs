using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderNVOCCInboundMessages))]
	sealed class ForwarderNVOCCInboundMessagesTest : RefStlScriptWithDefaultsTest
	{
		protected override bool IsMandatoryForMilestones => true;

		protected override void PrepareTestData()
		{
			var sqlText = @"
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
DECLARE @RslPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk06 UNIQUEIDENTIFIER = NEWID();
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
DECLARE @JkPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk14 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk15 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk16 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk17 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk18 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk19 UNIQUEIDENTIFIER = NEWID();
DECLARE @JkPk20 UNIQUEIDENTIFIER = NEWID();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.RefShippingLine (RSL_PK, RSL_CarrierName, RSL_CargoWiseOneCode, RSL_IsSystem, RSL_IsActive, RSL_IsNVO, RSL_IsShippingLine, RSL_ShippingInstructionAvailable, RSL_BookingRequestAvailable) VALUES
	(@RslPk01, 'Yusen01', 'Y01', 0, 1, 1, 0, 1, 1),
	(@RslPk02, 'DHL01', 'D01', 0, 1, 1, 0, 1, 1),
	(@RslPk04, 'SF', 'S01', 0, 1, 1, 0, 0, 1),
	(@RslPk05, 'JD', 'J01', 0, 1, 1, 0, 1, 0),
	(@RslPk06, 'YD', 'Z01', 0, 1, 1, 0, 0, 0),
	(@RslPk07, 'TX', 'T01', 0, 1, 1, 0, 0, 1),
	(@RslPk08, 'AL', 'A01', 0, 1, 1, 0, 1, 0),
	(@RslPk09, 'IQ', 'IQ1', 0, 1, 1, 1, 1, 1);

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_IsSeaWholesaler, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'VMLCCC', 'VML CCC', 0, 1, @RslPk01),
	(@OhPk02, 1, 'VMLC1C', 'VML C1C', 0, 1, @RslPk02),
	(@OhPk03, 1, 'KMJCCC', 'KMJ C1C', 0, 1, null),
	(@OhPk04, 1, 'SFCCC', 'SF C1C', 0, 1, @RslPk04),
	(@OhPk05, 1, 'JDCCC', 'JD C1C', 0, 1, @RslPk05),
	(@OhPk06, 1, 'YDCCC', 'YD C1C', 0, 1, @RslPk06),
	(@OhPk07, 1, 'TXCCC', 'TX C1C', 0, 1, @RslPk07),
	(@OhPk08, 1, 'ALCCC', 'AL C1C', 0, 1, @RslPk08),
	(@OhPk09, 1, 'ABCCC', 'AB C1C', 0, 1, @RslPk09);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk1, @OhPk01, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk2, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk3, @OhPk03, 1, '20', 'Ge St', 'Mel', 4000, 'AU'),
	(@OaPk4, @OhPk04, 1, '20', 'SF', 'elM', 4000, 'AU'),
	(@OaPk5, @OhPk05, 1, '20', 'JD', 'eMl', 4000, 'AU'),
	(@OaPk6, @OhPk06, 1, '20', 'YD', 'lMe', 4000, 'AU'),
	(@OaPk7, @OhPk07, 1, '22', 'TX', 'TX', 4000, 'AU'),
	(@OaPk8, @OhPk08, 1, '22', 'AL', 'AL', 4000, 'AU'),
	(@OaPk9, @OhPk09, 1, '22', 'AL', 'AL', 4000, 'AU');

INSERT dbo.OrgCusCode (OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(NEWID(), 1, 'HLCU', 'CCC', 'US', @OhPk01),
	(NEWID(), 1, 'XXCU', 'C1C', 'AU', @OhPk02),
	(NEWID(), 1, 'SFCU', 'C1C', 'AU', @OhPk04),
	(NEWID(), 1, 'JDCU', 'C1C', 'AU', @OhPk05),
	(NEWID(), 1, 'YDCU', 'C1C', 'AU', @OhPk06),
	(NEWID(), 1, 'TXCU', 'C1C', 'AU', @OhPk07),
	(NEWID(), 1, 'ALCU', 'C1C', 'AU', @OhPk08),
	(NEWID(), 1, 'ALCU', 'C1C', 'AU', @OhPk09);

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU', @OhPk03);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES (NEWID(), 'SY1', @GcPk01);

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_BookingReference, JK_MasterBillNum, JK_CoLoadBookingReference, JK_CoLoadMasterBill, JK_IsForwarding, JK_AgentType, JK_TransportMode, JK_ConsolMode, JK_OA_ShippingLineAddress, JK_OA_CreditorAddress, JK_OA_SendingForwarderAddress) VALUES
	(@JkPk01, 'CON01', 'B01', 'M01', 'CB01', 'CM01', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk02, 'CON02', 'B02', 'M02', 'CB02', 'CM02', 1, 'CLD', 'SEA', 'FCL', null, @OaPk2, @OaPk3),
	(@JkPk03, 'CON03', 'B03', 'M03', 'CB03', 'CM03', 1, 'CLD', 'SEA', 'FCL', null, @OaPk2, @OaPk3),
	(@JkPk04, 'CON04', 'B04', 'M04', 'CB04', 'CM04', 1, 'AGT', 'SEA', 'FCL', null, @OaPk2, @OaPk3),
	(@JkPk05, 'CON05', 'B05', 'M05', 'CB05', 'CM05', 1, 'CLD', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk06, 'CON06', 'B06', 'M06', 'CB06', 'CM06', 0, 'CLD', 'SEA', 'FCL', null, @OaPk2, @OaPk3),
	(@JkPk07, 'CON07', 'B07', 'M07', 'CB07', 'CM07', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk08, 'CON08', 'B08', 'M08', 'CB08', 'CM08', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk09, 'CON09', 'B09', 'M09', 'CB09', 'CM09', 1, 'AGT', 'AIR', 'LSE', @OaPk1, null, @OaPk3),
	(@JkPk10, 'CON10', 'B10', 'M10', 'CB10', 'CM10', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk11, 'CON11', 'B11', 'M11', 'CB11', 'CM11', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk12, 'CON12', 'B12', 'M12', 'CB12', 'CM12', 1, 'AGT', 'SEA', 'FCL', @OaPk1, null, @OaPk3),
	(@JkPk13, 'CON13', 'B13', 'M13', 'CB13', 'CM13', 1, 'AGT', 'SEA', 'FCL', @OaPk4, null, @OaPk3),
	(@JkPk14, 'CON14', 'B14', 'M14', 'CB14', 'CM14', 1, 'AGT', 'SEA', 'FCL', @OaPk5, null, @OaPk3),
	(@JkPk15, 'CON15', 'B15', 'M15', 'CB15', 'CM15', 1, 'AGT', 'SEA', 'FCL', @OaPk6, null, @OaPk3),
	(@JkPk16, 'CON16', 'B16', 'M16', 'CB16', 'CM16', 1, 'CLD', 'SEA', 'FCL', null, @OaPk7, @OaPk3),
	(@JkPk17, 'CON17', 'B17', 'M17', 'CB17', 'CM17', 1, 'AGT', 'SEA', 'FCL', @OaPk8, null, @OaPk3),
	(@JkPk18, 'CON18', 'B18', 'M18', 'CB18', 'CM18', 1, 'AGT', 'SEA', 'FCL', @OaPk9, null, @OaPk3),
	(@JkPk19, 'CON19', 'B19', 'M19', 'CB19', 'CM19', 1, 'AGT', 'SEA', 'LCL', @OaPk9, null, @OaPk3),
	(@JkPk20, 'CON20', 'B20', 'M20', 'CB20', 'CM20', 1, 'CLD', 'SEA', 'FCL', null, @OaPk9, @OaPk3);

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk01, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 01:23:00', @JkPk01, 'MAA', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-02-09 02:23:00', @JkPk02, 'MSN', '|DEP=WiseTechGlobal|MST=shipping instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk02, 'MRJ', '|DEP=WiseTechGlobal|MST=shipping instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk02, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 03:23:00', @JkPk02, 'MWA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-02-09 00:00:00', @JkPk03, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-02-09 01:00:00', @JkPk03, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 02:23:00', @JkPk04, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk04, 'MRJ', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 01:23:00', @JkPk05, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk05, 'MWA', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 02:23:00', @JkPk06, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk06, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-04-09 01:23:00', @JkPk07, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-04-09 02:23:00', @JkPk07, 'MRJ', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 02:23:00', @JkPk08, 'MSN', '|DEP=WiseTechGlobal|MST=Not Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk08, 'MWA', '|DEP=WiseTechGlobal|MST=Not Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 01:23:00', @JkPk09, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk09, 'MAA', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk10, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 01:23:00', @JkPk10, 'ISN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 01:23:00', @JkPk11, 'MAA', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-01-08 01:23:00', @JkPk12, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk12, 'MSN', '|DEP=WiseTechGlobal|MST=BOOKING REQUEST', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk13, 'MSN', '|DEP=WiseTechGlobal|MST=Booking RequestN', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 13:23:00', @JkPk13, 'MAA', '|DEP=WiseTechGlobal|MST=Booking RequestN', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk14, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'Y'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 14:23:00', @JkPk14, 'MRJ', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'Y'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk15, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 15:23:00', @JkPk15, 'MWA', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-02-09 02:23:00', @JkPk16, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-09 02:23:00', @JkPk16, 'MRJ', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:00', @JkPk17, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 13:23:00', @JkPk17, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:01', @JkPk18, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 14:23:01', @JkPk18, 'MRJ', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:02', @JkPk19, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 14:23:02', @JkPk19, 'MRJ', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-07 01:23:03', @JkPk20, 'MSN', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N'),
	(NEWID(), 'JobConsol', getdate(), '2021-03-08 14:23:03', @JkPk20, 'MRJ', '|DEP=WiseTechGlobal|MST=SHIPPING INSTRUCTION', 'SY1', 'N');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 7, transactions.Count());

			Assert("CON03's MAA response DateTime is not in DateTime range", transactions.All(t => t.Reference1 != "CON03"));
			Assert("AGT Consol without ShippingLine address", transactions.All(t => t.Reference1 != "CON04"));
			Assert("CLD Consol without Creditor address", transactions.All(t => t.Reference1 != "CON05"));
			Assert("CON06 IsForwarding = 0", transactions.All(t => t.Reference1 != "CON06"));
			Assert("CON07's MRJ message DateTime is not in DateTime range", transactions.All(t => t.Reference1 != "CON07"));
			Assert("CON08 hasn't received MWA response with Shipping Instruction or Booking Request", transactions.All(t => t.Reference1 != "CON08"));
			Assert("CON09 is not SEA consol", transactions.All(t => t.Reference1 != "CON09"));
			Assert("CON10 hasn't received outbound response event", transactions.All(t => t.Reference1 != "CON10"));
			Assert("CON11 didn't have MSN event", transactions.All(t => t.Reference1 != "CON11"));
			Assert("CON12 doesn't have MSN event with matching MST in one month before ISN", transactions.All(t => t.Reference1 != "CON12"));
			Assert("CON15's RSL_BookingRequestAvailable and RSL_ShippingInstructionAvailable are false", transactions.All(t => t.Reference1 != "CON15"));
			Assert("The MST value of StmALog is 'Shipping Instruction'. CON16's RSL_BookingRequestAvailable is true, but RSL_ShippingInstructionAvailable is false", transactions.All(t => t.Reference1 != "CON16"));
			Assert("The MST value of StmALog is 'Booking Request'. CON17's RSL_ShippingInstructionAvailable is true, but RSL_BookingRequestAvailable is false", transactions.All(t => t.Reference1 != "CON17"));
			Assert("non-CLD CON18's rslShippingLine is NVO and ShippingLine, but the container mode is not LCL", !transactions.Any(t => t.Reference1 == "CON18"));

			CombineAssertions(() =>
			{
				var transaction1 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 1, 23, 0));
				AssertEquals("[Row-0] CompanyCode", "DAU", transaction1.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "SY1", transaction1.GetBranchCode());
				AssertEquals("[Row-0] ItemCount", 1, transaction1.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "CON01", transaction1.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "B01", transaction1.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "M01", transaction1.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "Shipping Instruction/MAA", transaction1.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ConsolNumber\":\"CON01\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B01\",\"CoLoadBookingConfirmationReference\":\"CB01\",\"WayBillNumber\":\"M01\",\"CoLoadMasterBillNumber\":\"CM01\",\"ShippingLine\":{\"OrgCode\":\"VMLCCC\",\"C1CCode\":\"HLCU\"},\"IsNVO\":1}"
					, transaction1.AdditionalRefs);

				var transaction2 = FindRowByOccured(transactions, new DateTime(2021, 3, 9, 2, 23, 0));
				AssertEquals("[Row-1] CompanyCode", "DAU", transaction2.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "SY1", transaction2.GetBranchCode());
				AssertEquals("[Row-1] ItemCount", 1, transaction2.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "CON02", transaction2.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CB02", transaction2.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "CM02", transaction2.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "Shipping Instruction/MRJ", transaction2.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ConsolNumber\":\"CON02\",\"ConsolType\":\"CLD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B02\",\"CoLoadBookingConfirmationReference\":\"CB02\",\"WayBillNumber\":\"M02\",\"CoLoadMasterBillNumber\":\"CM02\",\"CoLoadWith\":{\"OrgCode\":\"VMLC1C\",\"C1CCode\":\"XXCU\"},\"IsNVO\":1}"
					, transaction2.AdditionalRefs);

				var transaction3 = FindRowByOccured(transactions, new DateTime(2021, 3, 9, 3, 23, 0));
				AssertEquals("[Row-2] CompanyCode", "DAU", transaction3.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "SY1", transaction3.GetBranchCode());
				AssertEquals("[Row-2] ItemCount", 1, transaction3.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "CON02", transaction3.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "CB02", transaction3.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "CM02", transaction3.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "Booking Request/MWA", transaction3.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ConsolNumber\":\"CON02\",\"ConsolType\":\"CLD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B02\",\"CoLoadBookingConfirmationReference\":\"CB02\",\"WayBillNumber\":\"M02\",\"CoLoadMasterBillNumber\":\"CM02\",\"CoLoadWith\":{\"OrgCode\":\"VMLC1C\",\"C1CCode\":\"XXCU\"},\"IsNVO\":1}"
					, transaction3.AdditionalRefs);

				var transaction4 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 13, 23, 0));
				AssertEquals("[Row-3] CompanyCode", "DAU", transaction4.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "SY1", transaction4.GetBranchCode());
				AssertEquals("[Row-3] ItemCount", 1, transaction4.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "CON13", transaction4.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "B13", transaction4.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "M13", transaction4.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "Booking Request/MAA", transaction4.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ConsolNumber\":\"CON13\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B13\",\"CoLoadBookingConfirmationReference\":\"CB13\",\"WayBillNumber\":\"M13\",\"CoLoadMasterBillNumber\":\"CM13\",\"ShippingLine\":{\"OrgCode\":\"SFCCC\",\"C1CCode\":\"SFCU\"},\"IsNVO\":1}"
					, transaction4.AdditionalRefs);

				var transaction5 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 14, 23, 0));
				AssertNotNull("MRJ and MSN are cancelled", transaction5);
				AssertEquals("[Row-4] CompanyCode", "DAU", transaction5.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "SY1", transaction5.GetBranchCode());
				AssertEquals("[Row-4] ItemCount", 1, transaction5.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "CON14", transaction5.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "B14", transaction5.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "M14", transaction5.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "Shipping Instruction/MRJ", transaction5.Reference4);
				AssertEquals("[Row-4] AdditionalRefs", "{\"ConsolNumber\":\"CON14\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B14\",\"CoLoadBookingConfirmationReference\":\"CB14\",\"WayBillNumber\":\"M14\",\"CoLoadMasterBillNumber\":\"CM14\",\"ShippingLine\":{\"OrgCode\":\"JDCCC\",\"C1CCode\":\"JDCU\"},\"IsNVO\":1}"
					, transaction5.AdditionalRefs);

				var transaction6 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 14, 23, 02));
				AssertEquals("[Row-5] CompanyCode", "DAU", transaction6.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "SY1", transaction6.GetBranchCode());
				AssertEquals("[Row-5] ItemCount", 1, transaction6.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "CON19", transaction6.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "B19", transaction6.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "M19", transaction6.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "Shipping Instruction/MRJ", transaction6.Reference4);
				AssertEquals("[Row-5] AdditionalRefs", "{\"ConsolNumber\":\"CON19\",\"ConsolType\":\"AGT\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B19\",\"CoLoadBookingConfirmationReference\":\"CB19\",\"WayBillNumber\":\"M19\",\"CoLoadMasterBillNumber\":\"CM19\",\"ShippingLine\":{\"OrgCode\":\"ABCCC\",\"C1CCode\":\"ALCU\"},\"IsNVO\":1}"
					, transaction6.AdditionalRefs);

				var transaction7 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 14, 23, 03));
				AssertEquals("[Row-5] CompanyCode", "DAU", transaction7.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "SY1", transaction7.GetBranchCode());
				AssertEquals("[Row-5] ItemCount", 1, transaction7.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "CON20", transaction7.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "CB20", transaction7.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "CM20", transaction7.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "Shipping Instruction/MRJ", transaction7.Reference4);
				AssertEquals("[Row-5] AdditionalRefs", "{\"ConsolNumber\":\"CON20\",\"ConsolType\":\"CLD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"FCL\",\"FirstLoadPort\":\"\",\"LastDiscPort\":\"\",\"CarrierBookingReference\":\"B20\",\"CoLoadBookingConfirmationReference\":\"CB20\",\"WayBillNumber\":\"M20\",\"CoLoadMasterBillNumber\":\"CM20\",\"CoLoadWith\":{\"OrgCode\":\"ABCCC\",\"C1CCode\":\"ALCU\"},\"IsNVO\":1}"
					, transaction7.AdditionalRefs);
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
