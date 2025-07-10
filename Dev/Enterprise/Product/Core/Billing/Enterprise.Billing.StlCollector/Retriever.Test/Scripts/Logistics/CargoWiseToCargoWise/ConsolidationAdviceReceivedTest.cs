using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(ConsolidationAdviceReceived))]
	sealed class ConsolidationAdviceReceivedTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 04);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(3, transactions.Count());

			AssertNull("MRR event is before DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JK02"));
			AssertNull("MRR event is after DateTime range", transactions.FirstOrDefault(x => x.Reference1 == "JK03"));
			AssertNull("This is XXX event", transactions.FirstOrDefault(x => x.Reference1 == "JK04"));

			var row0 = transactions.Single(x => x.Reference1.StartsWith("JK01"));
			var row1 = transactions.Single(x => x.Reference1.StartsWith("JK05"));
			var row2 = transactions.Single(x => x.Reference1.StartsWith("JK06"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DK1", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "A11", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2023, 04, 01, 12, 01, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "CoLoadBookingReference1", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "OrgCusCode1", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "Consolidation Advice - JinTianWanDeKaiXinMa(!.!)", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "KD1", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "B22", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2023, 04, 12, 12, 02, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK05", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "CoLoadBookingReference5", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "OrgCusCode5", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "Consolidation Advice", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "CN2", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB1", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2023, 04, 13, 12, 02, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JK06", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "CoLoadBookingReference6", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "OrgCusCode6", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "Consolidation Advice", row2.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			var sqlText = @"DECLARE @JkPk01 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk02 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk03 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk04 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk05 UNIQUEIDENTIFIER = newid();
DECLARE @JkPk06 UNIQUEIDENTIFIER = newid();

DECLARE @OhPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk05 UNIQUEIDENTIFIER = newid();
DECLARE @OhPk06 UNIQUEIDENTIFIER = newid();

DECLARE @OaPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk05 UNIQUEIDENTIFIER = newid();
DECLARE @OaPk06 UNIQUEIDENTIFIER = newid();

DECLARE @OcPk01 UNIQUEIDENTIFIER = newid();
DECLARE @OcPk02 UNIQUEIDENTIFIER = newid();
DECLARE @OcPk03 UNIQUEIDENTIFIER = newid();
DECLARE @OcPk04 UNIQUEIDENTIFIER = newid();
DECLARE @OcPk05 UNIQUEIDENTIFIER = newid();
DECLARE @OcPk06 UNIQUEIDENTIFIER = newid();

DECLARE @GcPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk4 UNIQUEIDENTIFIER = NEWID();

DECLARE @GbPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk4 UNIQUEIDENTIFIER = NEWID();

DECLARE @PerPk01 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.GlbPerson (PER_PK, PER_FullName) VALUES (@PerPk01, 'O.o');

INSERT dbo.OrgHeader (OH_PK, OH_Code,OH_FullName) VALUES
	(@OhPk01,'OH1','OrgHeader1'),
	(@OhPk02,'OH2','OrgHeader2'),
	(@OhPk03,'OH3','OrgHeader3'),
	(@OhPk04,'OH4','OrgHeader4'),
	(@OhPk05,'OH5','OrgHeader5'),
	(@OhPk06,'OH6','OrgHeader6');

INSERT dbo.OrgContact (OC_PK, OC_OH, OC_PER) VALUES
	(@OcPk01, @OhPk01, @PerPk01),
	(@OcPk02, @OhPk02, @PerPk01),
	(@OcPk03, @OhPk03, @PerPk01),
	(@OcPk04, @OhPk04, @PerPk01),
	(@OcPk05, @OhPk05, @PerPk01),
	(@OcPk06, @OhPk06, @PerPk01);

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES
	(@OaPk01 ,@OhPk01, 'Address01'),
	(@OaPk02 ,@OhPk02, 'Address02'),
	(@OaPk03 ,@OhPk03, 'Address03'),
	(@OaPk04 ,@OhPk04, 'Address04'),
	(@OaPk05 ,@OhPk05, 'Address05'),
	(@OaPk06 ,@OhPk06, 'Address06');

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES
	(@GcPk1, 'DK1', 'DK company', 'EUR', 'DK'),
	(@GcPk2, 'CN1', 'CN company1', 'CNY', 'CN'),

	(@GcPk3, 'KD1', 'KD company', 'EUR', 'DK'),

	(@GcPk4, 'CN2', 'CN company2', 'CNY', 'CN');

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES
	(@GbPk1, 'A11', @GcPk1, @OhPk01, 'AUSYD'),
	(@GbPk2, 'A12', @GcPk2, @OhPk01, 'CNSZG'),

	(@GbPk3, 'B22', @GcPk3, @OhPk05, 'AUSYD'),

	(@GbPk4, 'GB1', @GcPk4, NULL, 'CNBJA');

INSERT dbo.GlbBranchExtraPorts(GY_PK, GY_RL_NKAdditionalBranchRelatedPort, GY_GB) VALUES
	(NEWID(), 'AUSYD', @GbPk2),

	(NEWID(), 'CNSHG', @GbPk3);

INSERT dbo.OrgCusCode (OK_PK, OK_CustomsRegNo, OK_CodeType, OK_RN_NKCodeCountry, OK_OH) VALUES
	(newid(), 'OrgCusCode1', 'C1C', 'AU', @OhPk01),
	(newid(), 'OrgCusCode2', 'C1C', 'AU', @OhPk02),
	(newid(), 'OrgCusCode3', 'C1C', 'AU', @OhPk03),
	(newid(), 'OrgCusCode4', 'C1C', 'AU', @OhPk04),
	(newid(), 'OrgCusCode5', 'C1C', 'AU', @OhPk05),
	(newid(), 'OrgCusCode6', 'C1C', 'AU', @OhPk06);

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(newid(), 'JobConsol', getdate(), '2023-04-01 12:01:00', @JkPk01, 'MRR', 'from Carrier|MST=Consolidation Advice|MSB=JinTianWanDeKaiXinMa(!.!)', 'GB1', 'Y'), -- 00
	(newid(), 'JobConsol', getdate(), '2023-04-12 12:02:00', @JkPk05, 'MRR', 'from Carrier|MST=Consolidation Advice', 'GB2', 'N'),  -- 01
	(newid(), 'JobConsol', getdate(), '2023-03-01 12:03:00', @JkPk02, 'MRR', 'from Carrier|MST=Consolidation Advice', 'GB0', 'N'), -- Before date range
	(newid(), 'JobConsol', getdate(), '2023-05-01 12:04:00', @JkPk03, 'MRR', 'from Carrier|MST=Consolidation Advice', 'GB0', 'N'), -- After date range
	(newid(), 'JobConsol', getdate(), '2023-04-01 12:05:00', @JkPk04, 'XXX', 'from Carrier|MST=Consolidation Advice', 'GB0', 'N'), -- SL_SE_NKEvent is not MRR
	(newid(), 'JobConsol', getdate(), '2023-04-13 12:02:00', @JkPk06, 'MRR', 'from Carrier|MST=Consolidation Advice', 'GB1', 'N');  -- 02

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_OC_SendingForwarderContact, JK_CoLoadBookingReference, JK_RL_NKLoadPort, JK_OA_CreditorAddress) VALUES
	(@JkPk01, 'JK01', @OcPk01, 'CoLoadBookingReference1', 'AUSYD', @OaPk01),
	(@JkPk02, 'JK02', @OcPk02, 'CoLoadBookingReference2', 'DKASN', @OaPk02),
	(@JkPk03, 'JK03', @OcPk03, 'CoLoadBookingReference3', 'AUSYD', @OaPk03),
	(@JkPk04, 'JK04', @OcPk04, 'CoLoadBookingReference4', 'AUSYD', @OaPk04),
	(@JkPk05, 'JK05', @OcPk05, 'CoLoadBookingReference5', 'CNSHG', @OaPk05),
	(@JkPk06, 'JK06', @OcPk06, 'CoLoadBookingReference6', 'AUSYD', @OaPk06);";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
