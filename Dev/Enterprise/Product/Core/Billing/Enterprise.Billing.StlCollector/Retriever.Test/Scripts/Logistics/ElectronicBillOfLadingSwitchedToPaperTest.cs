using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ElectronicBillOfLadingSwitchedToPaper))]
	sealed class ElectronicBillOfLadingSwitchedToPaperTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => new RecurringRange(new DateTime(2024, 04, 10, 05, 00, 00), new DateTime(2024, 04, 10, 06, 00, 00));

		protected override bool IsMandatoryForMilestones => false;

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(4, transactions.Count());

			AssertNull("The Consol has a BLU before the query time range.", transactions.FirstOrDefault(x => x.Reference1 == "JK02"));
			AssertNull("The TYP is not 'Switched To Paper'", transactions.FirstOrDefault(x => x.Reference1 == "JK06"));
			AssertNull("The DEP is not 'Carrier'", transactions.FirstOrDefault(x => x.Reference1 == "JK07"));
			AssertNull("The SL_SE_NKEvent is not 'BLU'", transactions.FirstOrDefault(x => x.Reference1 == "JK08"));
			AssertNull("The SL_Table is not 'JobConsol'", transactions.FirstOrDefault(x => x.Reference1 == "JK09"));
			AssertNull("The SL_PostedTimeUtc is not in the query time range. Before", transactions.FirstOrDefault(x => x.Reference1 == "JK10"));
			AssertNull("The SL_PostedTimeUtc is not in the query time range. After", transactions.FirstOrDefault(x => x.Reference1 == "JK11"));

			var row0 = transactions.Single(x => x.Reference1 == "JK01");
			var row1 = transactions.Single(x => x.Reference1 == "JK03");
			var row2 = transactions.Single(x => x.Reference1 == "JK04");
			var row3 = transactions.Single(x => x.Reference1 == "JK05");

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "SYD", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB1", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 30, 58, 563), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "01MBN", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "Y01 [YSCA]", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "TRA-Transferable", row0.Reference4);
				AssertEquals("[Row-0] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ContainerMode\":\"FCL\",\"CarrierName\":\"VML CCC\",\"CarrierBookingReference\":\"01BR\",\"BillType\":\"STR\",\"BillTerms\":\"TRA\",\"eBillIdentifier\":\"01EBLR\",\"PlaceOfReceipt\":\"AUMEL\",\"PlaceOfDelivery\":\"GBSOU\"}", row0.AdditionalRefs);

				AssertEquals("[Row-1] CompanyCode", "MEL", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB2", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 46, 58, 563), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK03", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "03MBN", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "D02 []", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "TRA-Transferable", row1.Reference4);
				AssertEquals("[Row-1] AdditionalRefs", "{\"ConsolType\":\"AGT\",\"ContainerMode\":\"GPR\",\"CarrierName\":\"KMJ CCC\",\"CarrierBookingReference\":\"03BR\",\"BillType\":\"BLE\",\"BillTerms\":\"TRA\",\"eBillIdentifier\":\"03EBLR\",\"PlaceOfReceipt\":\"AUSYD\",\"PlaceOfDelivery\":\"GBSOU\"}", row1.AdditionalRefs);

				AssertEquals("[Row-2] CompanyCode", "SHA", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB3", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 10, 58, 563), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JK04", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "04CMB", row2.Reference2);
				AssertNull("[Row-2] TransactionReference03", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "NTR-Non-Transferable", row2.Reference4);
				AssertEquals("[Row-2] AdditionalRefs", "{\"ConsolType\":\"CLD\",\"ContainerMode\":\"BLK\",\"CarrierName\":\"KMJ C1C\",\"CarrierBookingReference\":\"04CBR\",\"BillType\":\"TOR\",\"BillTerms\":\"NTR\",\"eBillIdentifier\":\"04EBLR\",\"PlaceOfReceipt\":\"GBSOU\",\"PlaceOfDelivery\":\"AUSYD\"}", row2.AdditionalRefs);

				AssertEquals("[Row-3] CompanyCode", "SZG", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB4", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2024, 04, 10, 05, 55, 58, 563), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JK05", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "05CMB", row3.Reference2);
				AssertNull("[Row-3] TransactionReference03", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "TRA-Transferable", row3.Reference4);
				AssertEquals("[Row-3] AdditionalRefs", "{\"ConsolType\":\"CLD\",\"ContainerMode\":\"LQD\",\"CarrierBookingReference\":\"05CBR\",\"BillType\":\"\",\"BillTerms\":\"TRA\",\"eBillIdentifier\":\"05EBLR\",\"PlaceOfReceipt\":\"AUSYD\",\"PlaceOfDelivery\":\"AUMEL\"}", row3.AdditionalRefs);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = $@"
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

DECLARE @GcPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @RslPk01 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk02 UNIQUEIDENTIFIER = newid();
DECLARE @RslPk03 UNIQUEIDENTIFIER = newid();

DECLARE @OhPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk04 UNIQUEIDENTIFIER = newid();

DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk04 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk00, 'LAX', 'US company', 'USD', 'US');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'SYD', 'AU company1', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'MEL', 'AU company2', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk03, 'SHA', 'CN company1', 'CNY', 'CN');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk04, 'SZG', 'CN company2', 'CNY', 'CN');

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy,GB_RL_NKHomePort) VALUES (@GbPk00, 'GB0', @GcPk00, NULL,'USLAX');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy,GB_RL_NKHomePort) VALUES (@GbPk01, 'GB1', @GcPk01, NULL,'AUSYD');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy,GB_RL_NKHomePort) VALUES (@GbPk02, 'GB2', @GcPk02, NULL,'AUMEL');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy,GB_RL_NKHomePort) VALUES (@GbPk03, 'GB3', @GcPk03, NULL,'CNSHA');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy,GB_RL_NKHomePort) VALUES (@GbPk04, 'GB4', @GcPk04, NULL,'CNSZG');

INSERT dbo.RefShippingLine (RSL_PK, RSL_CarrierName, RSL_CargoWiseOneCode, RSL_StandardCarrierAlphaCode) VALUES
	(@RslPk01, 'Yusen01', 'Y01', 'YSCA'),
	(@RslPk02, 'DHL01', 'D01', 'DSAC'),
	(@RslPk03, 'DNN01', 'D02', '');

INSERT dbo.OrgHeader (OH_PK, OH_IsActive, OH_Code, OH_FullName, OH_IsShippingLine, OH_RSL_ShippingLine) VALUES
	(@OhPk01, 1, 'VMLCCC', 'VML CCC', 1, @RslPk01),
	(@OhPk02, 1, 'VMLC1C', 'VML C1C', 1, @RslPk02),
	(@OhPk03, 1, 'KMJCCC', 'KMJ CCC', 1, @RslPk03),
	(@OhPk04, 1, 'KMJC1C', 'KMJ C1C', 1, null);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_IsActive, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_RN_NKCountryCode) VALUES
	(@OaPk01, @OhPk01, 1, '10', 'Pit St', 'SYD', 2000, 'AU'),
	(@OaPk02, @OhPk02, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk03, @OhPk03, 1, '20', 'Ge St', 'Bri', 4000, 'AU'),
	(@OaPk04, @OhPk04, 1, '30', 'Me St', 'Mel', 3000, 'AU');


INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(),'JobConsol', getdate() ,'2024-04-10 05:30:58.563', @JkPk01 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','Y'), -- This is the first BLU Log for this Consol @JkPk01 [0]
	(newid(),'JobConsol', getdate() ,'2024-04-10 05:46:58.563', @JkPk01 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','Y'), -- This is not the first BLU Log for this Consol @JkPk01
	(newid(),'JobConsol', getdate() ,'2024-03-10 04:46:58.563', @JkPk01 ,'ADD','','GB1','N'),

	(newid(),'JobConsol', getdate() ,'2024-03-10 05:46:58.563', @JkPk02 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','Y'),
	(newid(),'JobConsol', getdate() ,'2024-04-10 05:46:58.563', @JkPk02 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- This is not the first BLU Log for this Consol @JkPk02

	(newid(),'JobConsol', getdate() ,'2024-04-10 05:46:58.563', @JkPk03 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- This is the first BLU Log for this Consol @JkPk03 [1]
	(newid(),'JobConsol', getdate() ,'2024-05-10 06:46:58.563', @JkPk03 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'),
	(newid(),'JobConsol', getdate() ,'2024-03-10 04:46:58.563', @JkPk03 ,'ADD','','GB2','N'),
	(newid(),'JobConsol', getdate() ,'2024-03-10 04:50:58.563', @JkPk03 ,'ADD','','GB1','N'),

	(newid(),'JobConsol', getdate() ,'2024-04-10 05:10:58.563', @JkPk04 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- [2]
	(newid(),'JobConsol', getdate() ,'2024-03-10 04:46:58.563', @JkPk04 ,'ADD','','GB3','N'),

	(newid(),'JobConsol', getdate() ,'2024-04-10 05:55:58.563', @JkPk05 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- [3]
	(newid(),'JobConsol', getdate() ,'2024-03-10 04:46:58.563', @JkPk05 ,'ADD','','GB4','N'),

	(newid(),'JobConsol', getdate() ,'2024-04-10 03:46:58.563', @JkPk06 ,'BLU','|DEP=Carrier|TYP=Surrendered','GB0','N'), -- The TYP is not 'Switched To Paper' @JkPk06
	(newid(),'JobConsol', getdate() ,'2024-04-10 03:46:58.563', @JkPk07 ,'BLU','|DEP=Cargowise|TYP=Switched To Paper','GB0','N'), -- The DEP is not 'Carrier' @JkPk07
	(newid(),'JobConsol', getdate() ,'2024-04-10 03:46:58.563', @JkPk08 ,'ISN','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- The SL_SE_NKEvent is not 'BLU' @JkPk08
	(newid(),'JobDocumentData', getdate() ,'2024-04-10 03:46:58.563', @JkPk09 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- The SL_Table is not 'JobConsol' @JkPk09
	(newid(),'JobConsol', getdate() ,'2024-04-10 04:46:58.000', @JkPk10 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'), -- The SL_PostedTimeUtc is not in the query time range. @JkPk10
	(newid(),'JobConsol', getdate() ,'2024-04-10 06:00:00.000', @JkPk11 ,'BLU','|DEP=Carrier|TYP=Switched To Paper','GB0','N'); -- The SL_PostedTimeUtc is not in the query time range. @JkPk11


INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_AgentType, JK_ConsolMode, JK_CoLoadMasterBill, JK_MasterBillNum, JK_CoLoadBookingReference, JK_BookingReference, JK_RL_NKLoadPort, JK_RL_NKDischargePort, JK_ElectronicBillOfLadingType, JK_ElectronicBillOfLadingTerms, JK_ElectronicBillOfLadingReference, JK_OA_ShippingLineAddress, JK_OA_CreditorAddress) VALUES
	(@JkPk01, 'JK01', 'AGT', 'FCL', '01CMB', '01MBN', '01CBR', '01BR', 'AUMEL', 'GBSOU', 'STR', 'TRA', '01EBLR', @OaPk01, @OaPk01),
	(@JkPk02, 'JK02', 'AGT', 'LCL', '02CMB', '02MBN', '02CBR', '02BR', 'AUMEL', 'AUSYD', 'TOR', 'NTR', '02EBLR', @OaPk02, @OaPk02),
	(@JkPk03, 'JK03', 'AGT', 'GPR', '03CMB', '03MBN', '03CBR', '03BR', 'AUSYD', 'GBSOU', 'BLE', 'TRA', '03EBLR', @OaPk03, @OaPk03),
	(@JkPk04, 'JK04', 'CLD', 'BLK', '04CMB', '04MBN', '04CBR', '04BR', 'GBSOU', 'AUSYD', 'TOR', 'NTR', '04EBLR', @OaPk04, @OaPk04),
	(@JkPk05, 'JK05', 'CLD', 'LQD', '05CMB', '05MBN', '05CBR', '05BR', 'AUSYD', 'AUMEL', '', 'TRA', '05EBLR', null, null),
	(@JkPk06, 'JK06', 'CLD', 'FCL', '06CMB', '06MBN', '06CBR', '06BR', 'GBSOU', 'AUSYD', 'BLE', 'NTR', '06EBLR', null, null),
	(@JkPk07, 'JK07', 'CLD', 'LCL', '07CMB', '07MBN', '07CBR', '07BR', 'GBLGP', 'AUSYD', 'STR', 'TRA', '07EBLR', null, null),
	(@JkPk08, 'JK08', 'CLD', 'GPR', '08CMB', '08MBN', '08CBR', '08BR', 'GBLGP', 'AUMEL', 'TOR', 'NTR', '08EBLR', null, null),
	(@JkPk09, 'JK09', 'CLD', 'BLK', '09CMB', '09MBN', '09CBR', '09BR', 'GBLGP', 'GBSOU', 'BLE', 'TRA', '09EBLR', null, null),
	(@JkPk10, 'JK10', 'AGT', 'LQD', '10CMB', '10MBN', '10CBR', '10BR', 'GBSOU', 'GBLGP', 'BLE', 'NTR', '10EBLR', null, null),
	(@JkPk11, 'JK11', 'AGT', 'LQD', '11CMB', '11MBN', '11CBR', '11BR', 'CNSHG', 'GBLGP', 'BLE', 'NTR', '11EBLR', null, null);";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
