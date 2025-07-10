using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ElectronicHouseBillOfLadingSurrendered))]
	sealed class ElectronicHouseBillOfLadingSurrenderedTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(new DateTime(2024, 04, 10, 05, 00, 00), new DateTime(2024, 04, 10, 06, 00, 00));

		protected override bool IsMandatoryForMilestones => false;

		public void TestVersions()
		{
			AssertEquals("24.8.8.51", ScriptToTest.MinCW1Version);
			AssertEquals(string.Empty, ScriptToTest.MaxCW1Version);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(4, transactions.Count());

			AssertNull("The Shipment has a BLU before the query time range.", transactions.FirstOrDefault(x => x.Reference1 == "JS02"));
			AssertNull("The TYP is not 'Surrendered'", transactions.FirstOrDefault(x => x.Reference1 == "JS06"));
			AssertNull("The DEP is not 'Title Registry'", transactions.FirstOrDefault(x => x.Reference1 == "JS07"));
			AssertNull("The SL_SE_NKEvent is not 'BLU'", transactions.FirstOrDefault(x => x.Reference1 == "JS08"));
			AssertNull("The SL_Table is not 'JobShipment'", transactions.FirstOrDefault(x => x.Reference1 == "JS09"));
			AssertNull("The SL_PostedTimeUtc is not in the query time range. Before", transactions.FirstOrDefault(x => x.Reference1 == "JS10"));
			AssertNull("The SL_PostedTimeUtc is not in the query time range. After", transactions.FirstOrDefault(x => x.Reference1 == "JS11"));

			var row0 = transactions.Single(x => x.Reference1 == "JS01");
			var row1 = transactions.Single(x => x.Reference1 == "JS03");
			var row2 = transactions.Single(x => x.Reference1 == "JS04");
			var row3 = transactions.Single(x => x.Reference1 == "JS05");

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "SYD", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 30, 58, 563), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JS01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "HBL01", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "JC - Justin Case", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "TRA-Transferable", row0.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ShipmentType\":\"STD\",\"ContainerMode\":\"FCL\",\"FirstHolder\":\"JC - Justin Case\",\"BillNumber\":\"HBL01\\/1\",\"BillType\":\"STR\",\"BillTerms\":\"TRA\",\"eBillIdentifier\":\"01EBLR\",\"PlaceOfReceipt\":\"AUMEL\",\"PlaceOfDelivery\":\"GBSOU\"}", row0.AdditionalRefs);

				AssertEquals("[Row-1] CompanyCode", "MEL", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 46, 58, 563), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JS03", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "HBL03", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "HB - Honey Bee", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "TRA-Transferable", row1.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ShipmentType\":\"STD\",\"ContainerMode\":\"ROR\",\"FirstHolder\":\"HB - Honey Bee\",\"BillNumber\":\"HBL03\\/3\",\"BillType\":\"BLE\",\"BillTerms\":\"TRA\",\"eBillIdentifier\":\"03EBLR\",\"PlaceOfReceipt\":\"AUSYD\",\"PlaceOfDelivery\":\"GBSOU\"}", row1.AdditionalRefs);

				AssertEquals("[Row-2] CompanyCode", "SHA", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB3", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 10, 58, 563), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JS04", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "HBL04", row2.Reference2);
				AssertNull("[Row-2] TransactionReference03", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "NTR-Non-Transferable", row2.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ShipmentType\":\"STD\",\"ContainerMode\":\"BLK\",\"BillNumber\":\"HBL04\\/4\",\"BillType\":\"TOR\",\"BillTerms\":\"NTR\",\"eBillIdentifier\":\"04EBLR\",\"PlaceOfReceipt\":\"GBSOU\",\"PlaceOfDelivery\":\"AUSYD\"}", row2.AdditionalRefs);

				AssertEquals("[Row-3] CompanyCode", "SZG", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB4", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 55, 58, 563), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JS05", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "HBL05", row3.Reference2);
				AssertNull("[Row-3] TransactionReference03", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "TRA-Transferable", row3.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ShipmentType\":\"STD\",\"ContainerMode\":\"LQD\",\"BillNumber\":\"HBL05\\/5\",\"BillType\":\"\",\"BillTerms\":\"TRA\",\"eBillIdentifier\":\"05EBLR\",\"PlaceOfReceipt\":\"AUSYD\",\"PlaceOfDelivery\":\"AUMEL\"}", row3.AdditionalRefs);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
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

DECLARE @GcPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk04 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES 
	(@GcPk00, 'LAX', 'US company', 'USD', 'US'),
	(@GcPk01, 'SYD', 'AU company1', 'AUD', 'AU'),
	(@GcPk02, 'MEL', 'AU company2', 'AUD', 'AU'),
	(@GcPk03, 'SHA', 'CN company1', 'CNY', 'CN'),
	(@GcPk04, 'SZG', 'CN company2', 'CNY', 'CN');

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES 
	(NEWID(), 'GB0', @GcPk00, NULL, 'USLAX'),
	(NEWID(), 'GB1', @GcPk01, NULL, 'AUSYD'),
	(NEWID(), 'GB2', @GcPk02, NULL, 'AUMEL'),
	(NEWID(), 'GB3', @GcPk03, NULL, 'CNSHA'),
	(NEWID(), 'GB4', @GcPk04, NULL, 'CNSZG');

INSERT dbo.JobShipment (JS_PK, JS_UniqueConsignRef, JS_HouseBill, JS_ShipmentType, JS_PackingMode, JS_RL_NKOrigin, JS_RL_NKDestination, JS_ElectronicBillOfLadingVersion, JS_ElectronicBillOfLadingType, JS_ElectronicBillOfLadingTerms, JS_ElectronicBillOfLadingReference) VALUES
	(@JsPk01, 'JS01', 'HBL01', 'STD', 'FCL', 'AUMEL', 'GBSOU', 01, 'STR', 'TRA', '01EBLR'),
	(@JsPk02, 'JS02', 'HBL02', 'STD', 'LCL', 'AUMEL', 'AUSYD', 02, 'TOR', 'NTR', '02EBLR'),
	(@JsPk03, 'JS03', 'HBL03', 'STD', 'ROR', 'AUSYD', 'GBSOU', 03, 'BLE', 'TRA', '03EBLR'),
	(@JsPk04, 'JS04', 'HBL04', 'STD', 'BLK', 'GBSOU', 'AUSYD', 04, 'TOR', 'NTR', '04EBLR'),
	(@JsPk05, 'JS05', 'HBL05', 'STD', 'LQD', 'AUSYD', 'AUMEL', 05, '',    'TRA', '05EBLR'),
	(@JsPk06, 'JS06', 'HBL06', 'STD', 'FCL', 'GBSOU', 'AUSYD', 06, 'BLE', 'NTR', '06EBLR'),
	(@JsPk07, 'JS07', 'HBL07', 'STD', 'LCL', 'GBLGP', 'AUSYD', 07, 'STR', 'TRA', '07EBLR'),
	(@JsPk08, 'JS08', 'HBL08', 'STD', 'ROR', 'GBLGP', 'AUMEL', 08, 'TOR', 'NTR', '08EBLR'),
	(@JsPk09, 'JS09', 'HBL09', 'STD', 'BLK', 'GBLGP', 'GBSOU', 09, 'BLE', 'TRA', '09EBLR'),
	(@JsPk10, 'JS10', 'HBL10', 'STD', 'LQD', 'GBSOU', 'GBLGP', 10, 'BLE', 'NTR', '10EBLR'),
	(@JsPk11, 'JS11', 'HBL11', 'STD', 'LQD', 'CNSHG', 'GBLGP', 11, 'BLE', 'NTR', '11EBLR');

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'JC', 'Justin Case', 1, null),
	(@OhPk02, 1, 'HB', 'Honey Bee', 1, null);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk01, @OhPk01, 1, '10', 'Pi St', 'SYD', 2000, 'AU'),
	(@OaPk02, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU');

INSERT dbo.JobDocAddress (E2_PK, E2_ParentID, E2_OA_Address, E2_ParentTableCode, E2_AddressType, E2_AddressOverride) VALUES
	(NEWID(), @JsPk01, @OaPk01, 'JS', 'HLD', 0),
	(NEWID(), @JsPk03, @OaPk02, 'JS', 'HLD', 0); 

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 05:30:58.563', @JsPk01 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'Y'),-- This is the first BLU Log for this Shipment @JsPk01 [0]
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 05:46:58.563', @JsPk01 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'Y'),-- This is not the first BLU Log for this Shipment @JsPk01
	(NEWID(), 'JobShipment', getdate() , '2024-04-01 04:46:58.563', @JsPk01 , 'MSN', '|DEP=Title Registry|TYP=Original Bill Sent for Publication', 'GB1', 'N'),

	(NEWID(), 'JobShipment', getdate() , '2024-03-10 05:46:58.563', @JsPk02 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'Y'),-- This is the first BLU Log for this Shipment @JsPk02 [0]
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 05:46:58.563', @JsPk02 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- This is not the first BLU Log for this Shipment @JsPk02

	(NEWID(), 'JobShipment', getdate() , '2024-04-10 05:46:58.563', @JsPk03 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- This is the first BLU Log for this Shipment @JsPk03 [1]
	(NEWID(), 'JobShipment', getdate() , '2024-05-10 06:46:58.563', @JsPk03 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'),
	(NEWID(), 'JobShipment', getdate() , '2024-04-01 04:46:58.563', @JsPk03 , 'MSN', '|DEP=Title Registry|TYP=Original Bill Sent for Publication', 'GB2', 'N'),
	(NEWID(), 'JobShipment', getdate() , '2024-03-10 04:50:58.563', @JsPk03 , 'MSN', '|DEP=Title Registry|TYP=Original Bill Sent for Publication', 'GB1', 'N'),

	(NEWID(), 'JobShipment', getdate() , '2024-04-10 05:10:58.563', @JsPk04 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'),-- [2]
	(NEWID(), 'JobShipment', getdate() , '2024-04-01 04:46:58.563', @JsPk04 , 'MSN', '|DEP=Title Registry|TYP=Original Bill Sent for Publication', 'GB3', 'N'),

	(NEWID(), 'JobShipment', getdate() , '2024-04-10 05:55:58.563', @JsPk05 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- [3]
	(NEWID(), 'JobShipment', getdate() , '2024-04-01 04:46:58.563', @JsPk05 , 'MSN', '|DEP=Title Registry|TYP=Original Bill Sent for Publication', 'GB4', 'N'),

	(NEWID(), 'JobShipment', getdate() , '2024-04-10 03:46:58.563', @JsPk06 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- The TYP is not 'Surrendered' @JsPk06
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 03:46:58.563', @JsPk07 , 'BLU', '|DEP=Cargowise|TYP=Surrendered', 'GB0', 'N'), -- The DEP is not 'Title Registry' @JsPk07
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 03:46:58.563', @JsPk08 , 'ISN', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- The SL_SE_NKEvent is not 'BLU' @JsPk08
	(NEWID(), 'JobDocumentData', getdate() , '2024-04-10 03:46:58.563', @JsPk09 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- The SL_Table is not 'JobShipment' @JsPk09
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 04:46:58.000', @JsPk10 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'), -- The SL_PostedTimeUtc is not in the query time range. @JsPk10
	(NEWID(), 'JobShipment', getdate() , '2024-04-10 06:00:00.000', @JsPk11 , 'BLU', '|DEP=Title Registry|TYP=Surrendered', 'GB0', 'N'); -- The SL_PostedTimeUtc is not in the query time range. @JsPk11";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
