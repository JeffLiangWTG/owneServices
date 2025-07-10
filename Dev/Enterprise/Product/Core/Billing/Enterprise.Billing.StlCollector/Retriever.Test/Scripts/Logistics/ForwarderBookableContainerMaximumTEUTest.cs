using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ForwarderBookableContainerMaximumTEU))]
	sealed class ForwarderBookableContainerMaximumTEUTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return new RecurringRange(new DateTime(2022, 10, 01, 00, 00, 00), new DateTime(2022, 10, 31, 23, 59, 59));
			}
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(5, transactions.Count());

			Assert("JK_UniqueRef01 is before DateTime range.", !transactions.Any(t => t.Reference1 == "JK_UniqueRef01"));
			Assert("JK_UniqueRef02 is after DateTime range.", !transactions.Any(t => t.Reference1 == "JK_UniqueRef02"));
			Assert("Consol MWR Log", !transactions.Any(t => t.Reference4.StartsWith("AAAA")));
			Assert("Consol MSN Log", !transactions.Any(t => t.Reference4.StartsWith("BBBB")));
			Assert("Consol MRJ Log", !transactions.Any(t => t.Reference4.StartsWith("CCCC")));
			Assert("Shipment OCB Log", !transactions.Any(t => t.Reference4.StartsWith("DDDD")));
			Assert("JobDocumentData OCB Log", !transactions.Any(t => t.Reference4.StartsWith("EEEE")));

			Assert("The QTY value of JK_UniqueRef06 cannot be converted to DECIMAL, the default value is 0.", !transactions.Any(t => t.Reference1 == "JK_UniqueRef06"));
			Assert("The QTY value of JK_UniqueRef08 is 0.", !transactions.Any(t => t.Reference1 == "JK_UniqueRef08"));
			Assert("The QTY of JK_UniqueRef10 is missing. Default value is 0, Missing QTY is for testing SQL", !transactions.Any(t => t.Reference1 == "JK_UniqueRef10"));

			var row0 = transactions.First(t => t.Reference1 == "JK_UniqueRef03");
			var row1 = transactions.First(t => t.Reference1 == "JK_UniqueRef04");
			var row2 = transactions.First(t => t.Reference1 == "JK_UniqueRef05");
			var row3 = transactions.First(t => t.Reference1 == "JK_UniqueRef07");
			var row4 = transactions.First(t => t.Reference1 == "JK_UniqueRef09");

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DCN", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "GB3", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2022, 10, 15, 2, 12, 0), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 2, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK_UniqueRef03", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "JK_BookingReference03", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "JK_MasterBillNum03", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "ONEY / Shipping Instruction[ORG]", row0.Reference4);
				AssertEquals("[Row-0] STL_AdditionalRefs", @"{""EventReference"":""|CMP=ONEY|MAX=0|NEW=0|OLD=0|QTY=2|STA=ORG|TYP=Shipping Instruction"",""ConsolType"":""COU"",""TransportMode"":""SEA"",""Container Mode"":""FCL"",""FirstLoadPort"":""CNSHA"",""LastDischargePort"":""GBFXT""}", row0.AdditionalRefs);

				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB1", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2022, 10, 11, 2, 13, 0), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 3, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK_UniqueRef04", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "JK_BookingReference04", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "JK_MasterBillNum04", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "ONEY / Departure[AMD]", row1.Reference4);
				AssertEquals("[Row-1] STL_AdditionalRefs", @"{""EventReference"":""|CMP=ONEY|MAX=0|NEW=0|OLD=0|QTY=3.3|STA=AMD|TYP=Departure"",""ConsolType"":""DRT"",""TransportMode"":""SEA"",""Container Mode"":""GRP"",""FirstLoadPort"":""USLAX"",""LastDischargePort"":""AUSYD""}", row1.AdditionalRefs);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB2", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2022, 10, 12, 2, 15, 0), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 4, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JK_UniqueRef05", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "JK_BookingReference05", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "JK_MasterBillNum05", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "OOLU / Departure[AMD]", row2.Reference4);
				AssertEquals("[Row-2] STL_AdditionalRefs", @"{""EventReference"":""|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=4|STA=AMD|TYP=Departure"",""ConsolType"":""AGT"",""TransportMode"":""RAI"",""Container Mode"":""FCL"",""FirstLoadPort"":""CNSHA"",""LastDischargePort"":""IN5PA""}", row2.AdditionalRefs);

				AssertEquals("[Row-3] CompanyCode", "DAU", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "GB1", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2022, 10, 13, 2, 39, 0), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 6, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JK_UniqueRef07", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "JK_BookingReference07", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "JK_MasterBillNum07", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "OOLU / Booking Request[AMD]", row3.Reference4);
				AssertEquals("[Row-3] STL_AdditionalRefs", @"{""EventReference"":""|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=6|STA=AMD|TYP=Booking Request"",""ConsolType"":""COU"",""TransportMode"":""SEA"",""Container Mode"":""GRP"",""FirstLoadPort"":""GBFXT"",""LastDischargePort"":""AUSYD""}", row3.AdditionalRefs);

				AssertEquals("[Row-4] CompanyCode", "DAU", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "GB2", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2022, 10, 11, 2, 30, 0), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 2, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JK_UniqueRef09", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "JK_BookingReference09", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "JK_MasterBillNum09", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04, Missing TYP is for testing SQL", "OOLU / [ORG]", row4.Reference4);
				AssertEquals("[Row-4] STL_AdditionalRefs, Modify the order of EventReference is to test SQL", @"{""EventReference"":""|MAX=0|NEW=0|OLD=0|QTY=2|STA=ORG|CMP=OOLU"",""ConsolType"":""AGT"",""TransportMode"":""SEA"",""Container Mode"":""GRP"",""FirstLoadPort"":""GBLBA"",""LastDischargePort"":""USLAX""}", row4.AdditionalRefs);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"
DECLARE @JkPK01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk06 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk07 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk08 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk09 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk10 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @GePk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk01, 'DAU', 'AU company', 'AUD', 'AU');
INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES (@GcPk02, 'DCN', 'CN company', 'CNY', 'CN');
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk01, 'GB1', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk02, 'GB2', @GcPk01, NULL);
INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy) VALUES (@GbPk03, 'GB3', @GcPk02, NULL);
INSERT dbo.GlbDepartment (GE_PK, GE_Code) VALUES (@GePk01, 'DEP');

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobConsol', getdate(), '2022-09-30 23:59:59', @JkPK01, 'OCB', '|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=1.2|STA=AMD|TYP=Shipping Instruction', 'GB1', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-11-01 00:00:00', @JkPk02, 'OCB', '|CMP=ONEY|MAX=0|NEW=0|OLD=0|QTY=1|STA=AMD|TYP=Shipping Instruction', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-15 02:12:00', @JkPk03, 'OCB', '|CMP=ONEY|MAX=0|NEW=0|OLD=0|QTY=2|STA=ORG|TYP=Shipping Instruction', 'GB3', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:13:00', @JkPk04, 'OCB', '|CMP=ONEY|MAX=0|NEW=0|OLD=0|QTY=3.3|STA=AMD|TYP=Departure', 'GB1', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-12 02:15:00', @JkPk05, 'OCB', '|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=4|STA=AMD|TYP=Departure', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:29:00', @JkPk06, 'OCB', '|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=xxxxx|STA=AMD|TYP=Departure', 'GB3', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-13 02:39:00', @JkPk07, 'OCB', '|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=6|STA=AMD|TYP=Booking Request', 'GB1', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:30:00', @JkPk08, 'OCB', '|CMP=OOLU|MAX=0|NEW=0|OLD=0|QTY=0|STA=ORG|TYP=Booking Request', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:30:00', @JkPk09, 'OCB', '|MAX=0|NEW=0|OLD=0|QTY=2|STA=ORG|CMP=OOLU', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:30:00', @JkPk10, 'OCB', '|CMP=OOLU|MAX=0|NEW=0|OLD=0|STA=ORG|TYP=Booking Request', 'GB2', 'N'),

	(newid(), 'JobConsol', getdate(), '2022-10-11 02:30:00', newid(), 'MWR', '|CMP=AAAA|TYP=Booking Request', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:30:00', @JkPk08, 'MSN', '|CMP=BBBB', 'GB2', 'N'),
	(newid(), 'JobConsol', getdate(), '2022-10-11 02:30:00', @JkPk09, 'MRJ', '|CMP=CCCC', 'GB2', 'N'),
	(newid(), 'JobShipment', getdate(), '2022-10-11 02:30:00', newid(), 'OCB', '|CMP=DDDD|MAX=0|NEW=0|OLD=0|QTY=0|STA=ORG|TYP=Booking Request', 'GB2', 'N'),
	(newid(), 'JobDocumentData', getdate(), '2022-10-11 02:30:00', newid(), 'OCB', '|CMP=EEEE|MAX=0|NEW=0|OLD=0|QTY=0|STA=ORG|TYP=Booking Request', 'GB2', 'N');

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_BookingReference, JK_MasterBillNum, JK_AgentType, JK_TransportMode, JK_ConsolMode, JK_RL_NKLoadPort, JK_RL_NKDischargePort) VALUES
	(@JkPK01, 'JK_UniqueRef01', 'JK_BookingReference01', 'JK_MasterBillNum01', 'AGT', 'AIR', 'GRP', 'CNSHA', 'AUSYD'),
	(@JkPk02, 'JK_UniqueRef02', 'JK_BookingReference02', 'JK_MasterBillNum02', 'DRT', 'ROA', 'FCL', 'IN5PA', 'AUSYD'),
	(@JkPk03, 'JK_UniqueRef03', 'JK_BookingReference03', 'JK_MasterBillNum03', 'COU', 'SEA', 'FCL', 'CNSHA', 'GBFXT'),
	(@JkPk04, 'JK_UniqueRef04', 'JK_BookingReference04', 'JK_MasterBillNum04', 'DRT', 'SEA', 'GRP', 'USLAX', 'AUSYD'),
	(@JkPk05, 'JK_UniqueRef05', 'JK_BookingReference05', 'JK_MasterBillNum05', 'AGT', 'RAI', 'FCL', 'CNSHA', 'IN5PA'),
	(@JkPk06, 'JK_UniqueRef06', 'JK_BookingReference06', 'JK_MasterBillNum06', 'OTH', 'SEA', 'OTH', 'CNSHA', 'GBLBA'),
	(@JkPk07, 'JK_UniqueRef07', 'JK_BookingReference07', 'JK_MasterBillNum07', 'COU', 'SEA', 'GRP', 'GBFXT', 'AUSYD'),
	(@JkPk08, 'JK_UniqueRef08', 'JK_BookingReference08', 'JK_MasterBillNum08', 'AGT', 'SEA', 'GRP', 'GBLBA', 'USLAX'),
	(@JkPk09, 'JK_UniqueRef09', 'JK_BookingReference09', 'JK_MasterBillNum09', 'AGT', 'SEA', 'GRP', 'GBLBA', 'USLAX'),
	(@JkPk10, 'JK_UniqueRef10', 'JK_BookingReference10', 'JK_MasterBillNum10', 'AGT', 'SEA', 'GRP', 'GBLBA', 'USLAX');";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
