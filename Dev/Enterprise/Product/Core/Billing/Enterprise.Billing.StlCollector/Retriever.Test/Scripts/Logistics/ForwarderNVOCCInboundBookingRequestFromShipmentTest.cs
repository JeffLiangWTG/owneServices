using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderNVOCCInboundBookingRequestFromShipment))]
	sealed class ForwarderNVOCCInboundBookingRequestFromShipmentTest : RefStlScriptWithDefaultsTest
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

DECLARE @OaPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk6 UNIQUEIDENTIFIER = NEWID();

DECLARE @RslPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @RslPk06 UNIQUEIDENTIFIER = NEWID();

DECLARE @JsPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk04 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk05 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk06 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk07 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk08 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk09 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk10 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @JsPk14 UNIQUEIDENTIFIER = NEWID();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.RefShippingLine (RSL_PK, RSL_CarrierName, RSL_CargoWiseOneCode, RSL_IsSystem, RSL_IsActive, RSL_IsNVO, RSL_IsShippingLine, RSL_BookingRequestAvailable) VALUES
	(@RslPk01, 'Yusen01', 'Y01', 0, 1, 1, 0, 1),
	(@RslPk02, 'DHL01', 'D01', 0, 1, 1, 0, 1),
	(@RslPk03, 'DNN01', 'DN1', 0, 1, 0, 0, 1),

	(@RslPk04, '10mesuY', '10Y', 0, 1, 1, 0, 1),
	(@RslPk05, '01LHD', '10D', 0, 1, 1, 0, 0),
	(@RslPk06, '0IQ', '0IQ', 0, 1, 1, 1, 1);

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_IsSeaWholesaler, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'VMLCCC', 'VML CCC', 0, 1, @RslPk01),
	(@OhPk02, 1, 'VMLC1C', 'VML C1C', 0, 1, @RslPk02),
	(@OhPk03, 1, 'KMJCCC', 'KMJ C1C', 0, 1, @RslPk03),
	(@OhPk04, 1, 'LMVCCC', 'LMV C1C', 0, 1, @RslPk04),
	(@OhPk05, 1, 'JMKCCC', 'JMK C1C', 0, 1, @RslPk05),
	(@OhPk06, 1, 'ABCC1C', 'ABC C1C', 0, 1, @RslPk06);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk1, @OhPk01, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk2, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk3, @OhPk03, 1, '20', 'Ge St', 'Mel', 4000, 'AU'),
	(@OaPk4, @OhPk04, 1, '20', 'tS eG', 'irB', 4000, 'AU'),
	(@OaPk5, @OhPk05, 1, '20', 'tS tiP', 'DYS', 4000, 'AU'),
	(@OaPk6, @OhPk06, 1, '20', 'tS tiP', 'DYS', 4000, 'AU');

INSERT dbo.OrgCusCode (OK_PK, OK_IsValid, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(NEWID(), 1, 'HLCU', 'CCC', 'US', @OhPk01),
	(NEWID(), 1, 'XXCU', 'C1C', 'AU', @OhPk02),

	(NEWID(), 1, 'QQCU', 'C1C', 'AU', @OhPk04),
	(NEWID(), 1, 'ZZCU', 'C1C', 'AU', @OhPk05),
	(NEWID(), 1, 'ZZCU', 'C1C', 'AU', @OhPk06);

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode, GC_OH_OrgProxy) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU', @OhPk03);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC) VALUES (NEWID(), 'SY1', @GcPk01);

INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_HouseBill, JS_ShipmentType, JS_TransportMode, JS_PackingMode, JS_RL_NKOrigin, JS_RL_NKDestination, JS_OA_BookedShippingLineAddress, JS_IsForwardRegistered) VALUES
	(@JsPk01, 'SHP01', 'HB01', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk1, 1),
	(@JsPk02, 'SHP02', 'HB02', 'CLD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk2, 1),
	(@JsPk03, 'SHP03', 'HB03', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk3, 1),
	(@JsPk04, 'SHP04', 'HB04', 'CLD', 'SEA', 'LCL', 'AUSYD', 'USLAX', null, 1),
	(@JsPk05, 'SHP05', 'HB05', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk1, 0),
	(@JsPk06, 'SHP06', 'HB06', 'CLD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk2, 1),
	(@JsPk07, 'SHP07', 'HB07', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk1, 1),
	(@JsPk08, 'SHP08', 'HB08', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk1, 1),
	(@JsPk09, 'SHP09', 'HB09', 'STD', 'AIR', 'LSE', 'AUSYD', 'USLAX', @OaPk2, 1),
	(@JsPk10, 'SHP10', 'HB10', 'CLD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk1, 1),
	(@JsPk11, 'SHP11', 'HB11', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk4, 1),
	(@JsPk12, 'SHP12', 'HB12', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk5, 1),
	(@JsPk13, 'SHP13', 'HB13', 'STD', 'SEA', 'FCL', 'AUSYD', 'USLAX', @OaPk6, 1),
	(@JsPk14, 'SHP14', 'HB14', 'STD', 'SEA', 'LCL', 'AUSYD', 'USLAX', @OaPk6, 1);

INSERT dbo.CusEntryNum (CE_PK, CE_ParentTable, CE_ParentID, CE_EntryType, CE_EntryNum, CE_IsValid, CE_SystemCreateTimeUtc, CE_SystemCreateUser, CE_SystemLastEditTimeUtc, CE_SystemLastEditUser) VALUES
	(NEWID(), 'JobShipment', @JsPk01, 'BKG', 'REF01', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk02, 'BKG', 'REF02', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk03, 'BKG', 'REF03', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk04, 'BKG', 'REF04', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk05, 'BKG', 'REF05', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk06, 'BKG', 'REF06', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk07, 'BKG', 'REF07', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk08, 'BKG', 'REF08', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk09, 'BKG', 'REF09', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk10, 'BKG', 'REF10', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk11, 'BKG', 'REF11', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk12, 'BKG', 'REF12', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk13, 'BKG', 'REF13', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN'),
	(NEWID(), 'JobShipment', @JsPk14, 'BKG', 'REF14', 1, '2021-01-09', 'DNN', '2021-01-09', 'DNN');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 00:23:00', @JsPk01, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 01:23:00', @JsPk01, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-02-09 02:23:00', @JsPk02, 'MSN', '|DEP=WiseTechGlobal|MST=BOOKING REQUEST', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 02:23:00', @JsPk02, 'MRJ', '|DEP=WiseTechGlobal|MST=BOOKING REQUEST', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 03:23:00', @JsPk02, 'MSN', '|DEP=WiseTechGlobal|MST=booking request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 03:23:00', @JsPk02, 'MWA', '|DEP=WiseTechGlobal|MST=booking request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 00:00:00', @JsPk03, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 01:00:00', @JsPk03, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 01:23:00', @JsPk04, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 02:23:00', @JsPk04, 'MRJ', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 02:23:00', @JsPk05, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 02:23:00', @JsPk05, 'MWA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-02-08 02:23:00', @JsPk06, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-02-09 02:23:00', @JsPk06, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-04-09 01:23:00', @JsPk07, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-04-09 02:23:00', @JsPk07, 'MRJ', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 02:23:00', @JsPk08, 'MSN', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 02:23:00', @JsPk08, 'MWA', '|DEP=WiseTechGlobal|MST=Shipping Instruction', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 01:23:00', @JsPk09, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 02:23:00', @JsPk09, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-01-09 02:23:00', @JsPk10, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-09 02:23:00', @JsPk10, 'MRJ', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 00:23:00', @JsPk11, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'Y'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 11:23:00', @JsPk11, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'Y'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 00:23:00', @JsPk12, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 12:23:00', @JsPk12, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 00:23:01', @JsPk13, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 11:23:01', @JsPk13, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 00:23:02', @JsPk14, 'MSN', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N'),
	(NEWID(), 'JobShipment', getdate(), '2021-03-08 11:23:02', @JsPk14, 'MAA', '|DEP=WiseTechGlobal|MST=Booking Request', 'SY1', 'N');
";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 5, transactions.Count());

			Assert("SHP03's Planned Carrier is not NVO", transactions.All(t => t.Reference1 != "SHP03"));
			Assert("SHP04 does not have Planned Carrier", transactions.All(t => t.Reference1 != "SHP04"));
			Assert("SHP05 IsForwardRegistered = 0", transactions.All(t => t.Reference1 != "SHP05"));
			Assert("SHP06's Booking Request message DateTime is not in DateTime range", transactions.All(t => t.Reference1 != "SHP06"));
			Assert("SHP07's Booking Request message DateTime is not in DateTime range", transactions.All(t => t.Reference1 != "SHP07"));
			Assert("SHP08 hasn't received Booking Request inbound response event", transactions.All(t => t.Reference1 != "SHP08"));
			Assert("SHP09 is not SEA shipment", transactions.All(t => t.Reference1 != "SHP09"));
			Assert("SHP10 doesn't have MSN event in one month before inbound response message", transactions.All(t => t.Reference1 != "SHP10"));
			Assert("SHP12's RSL_BookingRequestAvailable is false", transactions.All(t => t.Reference1 != "SHP12"));
			Assert("SHP13's rsl is NVO and ShippingLine, but the container mode is not LCL", transactions.All(t => t.Reference1 != "SHP13"));

			CombineAssertions(() =>
			{
				var transaction1 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 1, 23, 0));
				AssertEquals("[Row-0] CompanyCode", "DAU", transaction1.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "SY1", transaction1.GetBranchCode());
				AssertEquals("[Row-0] ItemCount", 1, transaction1.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "SHP01", transaction1.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "REF01", transaction1.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "HB01", transaction1.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "Booking Request/MAA", transaction1.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ShipmentNumber\":\"SHP01\",\"ShipmentType\":\"STD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"Origin\":\"AUSYD\",\"Destination\":\"USLAX\",\"ReferenceNumber\":\"REF01\",\"HouseBillNumber\":\"HB01\",\"Carrier\":{\"OrgCode\":\"VMLCCC\",\"C1CCode\":\"HLCU\"},\"IsNVO\":1}"
					, transaction1.AdditionalRefs);

				var transaction2 = FindRowByOccured(transactions, new DateTime(2021, 3, 9, 2, 23, 0));
				AssertEquals("[Row-1] CompanyCode", "DAU", transaction2.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "SY1", transaction2.GetBranchCode());
				AssertEquals("[Row-1] ItemCount", 1, transaction2.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "SHP02", transaction2.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "REF02", transaction2.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "HB02", transaction2.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "Booking Request/MRJ", transaction2.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ShipmentNumber\":\"SHP02\",\"ShipmentType\":\"CLD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"Origin\":\"AUSYD\",\"Destination\":\"USLAX\",\"ReferenceNumber\":\"REF02\",\"HouseBillNumber\":\"HB02\",\"Carrier\":{\"OrgCode\":\"VMLC1C\",\"C1CCode\":\"XXCU\"},\"IsNVO\":1}"
					, transaction2.AdditionalRefs);

				var transaction3 = FindRowByOccured(transactions, new DateTime(2021, 3, 9, 3, 23, 0));
				AssertEquals("[Row-2] CompanyCode", "DAU", transaction3.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "SY1", transaction3.GetBranchCode());
				AssertEquals("[Row-2] ItemCount", 1, transaction3.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "SHP02", transaction3.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "REF02", transaction3.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "HB02", transaction3.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "Booking Request/MWA", transaction3.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ShipmentNumber\":\"SHP02\",\"ShipmentType\":\"CLD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"Origin\":\"AUSYD\",\"Destination\":\"USLAX\",\"ReferenceNumber\":\"REF02\",\"HouseBillNumber\":\"HB02\",\"Carrier\":{\"OrgCode\":\"VMLC1C\",\"C1CCode\":\"XXCU\"},\"IsNVO\":1}"
					, transaction3.AdditionalRefs);

				var transaction4 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 11, 23, 0));
				AssertNotNull("MAA and MSN are cancelled", transaction4);
				AssertEquals("[Row-3] CompanyCode", "DAU", transaction4.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "SY1", transaction4.GetBranchCode());
				AssertEquals("[Row-3] ItemCount", 1, transaction4.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "SHP11", transaction4.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "REF11", transaction4.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "HB11", transaction4.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "Booking Request/MAA", transaction4.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ShipmentNumber\":\"SHP11\",\"ShipmentType\":\"STD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"Origin\":\"AUSYD\",\"Destination\":\"USLAX\",\"ReferenceNumber\":\"REF11\",\"HouseBillNumber\":\"HB11\",\"Carrier\":{\"OrgCode\":\"LMVCCC\",\"C1CCode\":\"QQCU\"},\"IsNVO\":1}"
					, transaction4.AdditionalRefs);

				var transaction5 = FindRowByOccured(transactions, new DateTime(2021, 3, 8, 11, 23, 02));
				AssertEquals("[Row-4] CompanyCode", "DAU", transaction5.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "SY1", transaction5.GetBranchCode());
				AssertEquals("[Row-4] ItemCount", 1, transaction5.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "SHP14", transaction5.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "REF14", transaction5.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "HB14", transaction5.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "Booking Request/MAA", transaction5.Reference4);
				AssertEquals("[Row-4] AdditionalRefs", "{\"ShipmentNumber\":\"SHP14\",\"ShipmentType\":\"STD\",\"TransportMode\":\"SEA\",\"ContainerMode\":\"LCL\",\"Origin\":\"AUSYD\",\"Destination\":\"USLAX\",\"ReferenceNumber\":\"REF14\",\"HouseBillNumber\":\"HB14\",\"Carrier\":{\"OrgCode\":\"ABCC1C\",\"C1CCode\":\"ZZCU\"},\"IsNVO\":1}"
					, transaction5.AdditionalRefs);
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
