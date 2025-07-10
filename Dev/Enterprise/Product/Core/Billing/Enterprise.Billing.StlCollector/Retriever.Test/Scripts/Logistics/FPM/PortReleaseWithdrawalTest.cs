using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Logistics;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Logistics
{
	[TestedType(typeof(PortReleaseWithdrawal))]
	sealed class PortReleaseWithdrawalTest : RefStlScriptWithDefaultsTest
	{
		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(2023, 01);

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals(6, transactions.Count());

			var row0 = transactions.Single(x => x.Reference2.StartsWith("DKAAB"));
			var row1 = transactions.Single(x => x.Reference2.StartsWith("MARS1"));
			var row2 = transactions.Single(x => x.Reference2.StartsWith("Notification"));
			var row3 = transactions.Single(x => x.Reference2.StartsWith("DKASN"));
			var row4 = transactions.Single(x => x.Reference2.StartsWith("DKCNL"));
			var row5 = transactions.Single(x => x.Reference2.StartsWith("DKFBG"));

			CombineAssertions(() =>
			{
				AssertEquals("[Row-0] CompanyCode", "DK1", row0.GetCompanyCode());
				AssertEquals("[Row-0] BranchCode", "B11", row0.GetBranchCode());
				AssertEquals("[Row-0] TransactionDateUtc", new DateTime(2023, 01, 10, 12, 00, 00), row0.ServiceOccuredUTC);
				AssertEquals("[Row-0] ItemCount", 1, row0.BillableCount);
				AssertEquals("[Row-0] TransactionReference01", "JK_C01", row0.Reference1);
				AssertEquals("[Row-0] TransactionReference02", "DKAAB - Port", row0.Reference2);
				AssertEquals("[Row-0] TransactionReference03", "ATW - aaaaa", row0.Reference3);
				AssertEquals("[Row-0] TransactionReference04", "EQN01 - A", row0.Reference4);

				AssertEquals("[Row-1] CompanyCode", "DAU", row1.GetCompanyCode());
				AssertEquals("[Row-1] BranchCode", "GB0", row1.GetBranchCode());
				AssertEquals("[Row-1] TransactionDateUtc", new DateTime(2023, 01, 10, 12, 00, 00), row1.ServiceOccuredUTC);
				AssertEquals("[Row-1] ItemCount", 1, row1.BillableCount);
				AssertEquals("[Row-1] TransactionReference01", "JK_C01", row1.Reference1);
				AssertEquals("[Row-1] TransactionReference02", "MARS1 - Release", row1.Reference2);
				AssertEquals("[Row-1] TransactionReference03", "ATW - bbbbb", row1.Reference3);
				AssertEquals("[Row-1] TransactionReference04", "EQN02", row1.Reference4);

				AssertEquals("[Row-2] CompanyCode", "DAU", row2.GetCompanyCode());
				AssertEquals("[Row-2] BranchCode", "GB0", row2.GetBranchCode());
				AssertEquals("[Row-2] TransactionDateUtc", new DateTime(2023, 01, 10, 12, 00, 00), row2.ServiceOccuredUTC);
				AssertEquals("[Row-2] ItemCount", 1, row2.BillableCount);
				AssertEquals("[Row-2] TransactionReference01", "JK_C01", row2.Reference1);
				AssertEquals("[Row-2] TransactionReference02", "Notification", row2.Reference2);
				AssertEquals("[Row-2] TransactionReference03", "ATW - zzzzz", row2.Reference3);
				AssertEquals("[Row-2] TransactionReference04", "EQN0Z - Z", row2.Reference4);

				AssertEquals("[Row-3] CompanyCode", "CN2", row3.GetCompanyCode());
				AssertEquals("[Row-3] BranchCode", "B22", row3.GetBranchCode());
				AssertEquals("[Row-3] TransactionDateUtc", new DateTime(2023, 01, 11, 12, 00, 00), row3.ServiceOccuredUTC);
				AssertEquals("[Row-3] ItemCount", 1, row3.BillableCount);
				AssertEquals("[Row-3] TransactionReference01", "JK_C02", row3.Reference1);
				AssertEquals("[Row-3] TransactionReference02", "DKASN - Withdrawal", row3.Reference2);
				AssertEquals("[Row-3] TransactionReference03", "ATW", row3.Reference3);
				AssertEquals("[Row-3] TransactionReference04", "EQN05 - Assens", row3.Reference4);

				AssertEquals("[Row-4] CompanyCode", "DK3", row4.GetCompanyCode());
				AssertEquals("[Row-4] BranchCode", "B33", row4.GetBranchCode());
				AssertEquals("[Row-4] TransactionDateUtc", new DateTime(2023, 01, 12, 12, 00, 00), row4.ServiceOccuredUTC);
				AssertEquals("[Row-4] ItemCount", 1, row4.BillableCount);
				AssertEquals("[Row-4] TransactionReference01", "JK_C03", row4.Reference1);
				AssertEquals("[Row-4] TransactionReference02", "DKCNL - Proxy", row4.Reference2);
				AssertEquals("[Row-4] TransactionReference03", "ATW - fffff", row4.Reference3);
				AssertEquals("[Row-4] TransactionReference04", "EQN06 - Achim", row4.Reference4);

				AssertEquals("[Row-5] CompanyCode", "DE4", row5.GetCompanyCode());
				AssertEquals("[Row-5] BranchCode", "B44", row5.GetBranchCode());
				AssertEquals("[Row-5] TransactionDateUtc", new DateTime(2023, 01, 13, 12, 00, 00), row5.ServiceOccuredUTC);
				AssertEquals("[Row-5] ItemCount", 1, row5.BillableCount);
				AssertEquals("[Row-5] TransactionReference01", "JK_C04", row5.Reference1);
				AssertEquals("[Row-5] TransactionReference02", "DKFBG", row5.Reference2);
				AssertEquals("[Row-5] TransactionReference03", "ATW - ggggg", row5.Reference3);
				AssertEquals("[Row-5] TransactionReference04", "Altdorf", row5.Reference4);
			});
		}

		protected override void PrepareTestData()
		{
			string sqlText = $@"
DECLARE @JcPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @JcPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @JcPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @JcPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @Jk_cPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @Jk_cPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @Jk_cPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @Jk_cPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @GcPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk14 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk22 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk23 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk24 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk33 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk34 UNIQUEIDENTIFIER = NEWID();
DECLARE @GcPk44 UNIQUEIDENTIFIER = NEWID();

DECLARE @GbPk00 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk11 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk12 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk13 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk14 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk22 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk23 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk24 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk33 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk34 UNIQUEIDENTIFIER = NEWID();
DECLARE @GbPk44 UNIQUEIDENTIFIER = NEWID();

DECLARE @OhPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OhPk04 UNIQUEIDENTIFIER = NEWID();

DECLARE @OaPk01 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk02 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk03 UNIQUEIDENTIFIER = NEWID();
DECLARE @OaPk04 UNIQUEIDENTIFIER = NEWID();

INSERT dbo.OrgHeader (OH_PK, OH_Code) VALUES
	(@OhPk01, 'OH1'),
	(@OhPk02, 'OH2'),
	(@OhPk03, 'OH3'),
	(@OhPk04, 'OH4');

INSERT dbo.OrgAddress (OA_PK, OA_OH, OA_Address1) VALUES
	(@OaPk01 ,@OhPk01, 'Address01'),
	(@OaPk02 ,@OhPk02, 'Address02'),
	(@OaPk03 ,@OhPk03, 'Address03'),
	(@OaPk04 ,@OhPk04, 'Address04');

INSERT dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RX_NKLocalCurrency, GC_RN_NKCountryCode) VALUES
	(@GcPk00, 'DAU', 'AU company', 'AUD', 'AU'),

	(@GcPk11, 'DK1', 'DK company', 'EUR', 'DK'),
	(@GcPk12, 'CN1', 'CN company', 'CNY', 'CN'),
	(@GcPk13, 'KD1', 'KD company', 'EUR', 'DK'),
	(@GcPk14, 'DE1', 'DE company', 'EUR', 'DE'),

	(@GcPk22, 'CN2', 'CN company2', 'CNY', 'CN'),
	(@GcPk23, 'DK2', 'DK company2', 'EUR', 'DK'),
	(@GcPk24, 'DE2', 'DE company2', 'EUR', 'DE'),

	(@GcPk33, 'DK3', 'DK company3', 'EUR', 'DK'),
	(@GcPk34, 'DE3', 'DE company3', 'EUR', 'DE'),

	(@GcPk44, 'DE4', 'DE company4', 'EUR', 'DE');

INSERT dbo.GlbBranch (GB_PK, GB_Code, GB_GC, GB_OH_OrgProxy, GB_RL_NKHomePort) VALUES
	(@GbPk00, 'GB0', @GcPk00, NULL, 'AUSYD'),

	(@GbPk11, 'B11', @GcPk11, @OhPk01, 'DKAAB'),
	(@GbPk12, 'B12', @GcPk12, @OhPk01, 'CNSZG'),
	(@GbPk13, 'B13', @GcPk13, NULL, 'DKAAB'),
	(@GbPk14, 'B14', @GcPk14, NULL, 'DEACM'),

	(@GbPk22, 'B22', @GcPk22, @OhPk02, 'CNSZG'),
	(@GbPk23, 'B23', @GcPk23, NULL, 'DKASN'),
	(@GbPk24, 'B24', @GcPk24, NULL, 'DEACM'),

	(@GbPk33, 'B33', @GcPk33, NULL, 'DKCNL'),
	(@GbPk34, 'B34', @GcPk34, @OhPk04, 'DEACM'),

	(@GbPk44, 'B44', @GcPk44, @OhPk04, 'DEACM');

INSERT dbo.GlbBranchExtraPorts(GY_PK, GY_RL_NKAdditionalBranchRelatedPort, GY_GB) VALUES
	(NEWID(), 'DKAAB', @GbPk12),
	(NEWID(), 'DKAAB', @GbPk14),

	(NEWID(), 'DKASN', @GbPk22),
	(NEWID(), 'DKASN', @GbPk24),

	(NEWID(), 'DKCNL', @GbPk34),

	(NEWID(), 'DKFBG', @GbPk44);

INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference, SL_GB_NKBranch, SL_IsCancelled) VALUES
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=A|LOC=DKAAB|EQN=EQN01|STA=aaaaa|MST=Port', 'GB0', 'N'), -- Org Proxy, Home Port
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Container Release|LOC=MARS1|EQN=EQN02|STA=bbbbb|MST=Release', 'GB0', 'N'),
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=Z|EQN=EQN0Z|STA=zzzzz|MST=Notification', 'GB0', 'N'),
	(NEWID(), 'JobContainer', GETDATE(), '2022-12-10 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=C|LOC=CNSHG|EQN=EQN03|STA=ccccc', 'GB0', 'N'), -- Before
	(NEWID(), 'JobContainer', GETDATE(), '2023-02-02 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=D|LOC=CNSHG|EQN=EQN01|STA=aaaaa', 'GB0', 'N'), -- After
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATH', 'DEP=Terminal|TYP=Container Release|RFN=D|LOC=CNSHG|EQN=EQN01|STA=aaaaa', 'GB0', 'N'), -- SL_SE_NKEvent != 'ATW'
	(NEWID(), 'JobShipment', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=D|LOC=CNSHG|EQN=EQN01|STA=aaaaa', 'GB0', 'N'), -- SL_Table != 'JobContainer'
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATW', 'DEP=WTG|TYP=Container Release|RFN=D|LOC=CNSHG|EQN=EQN01|STA=aaaaa', 'GB0', 'N'), -- DEP != 'Terminal'
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-10 12:00:00', @JcPk01, 'ATW', 'DEP=Terminal|TYP=Alien|RFN=D|LOC=CNSHG|EQN=EQN01|STA=aaaaa', 'GB0', 'N'), -- TYP != 'Container Release'

	(NEWID(), 'JobContainer', GETDATE(), '2023-01-11 12:00:00', @JcPk02, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=Assens|MST=Withdrawal|LOC=DKASN|EQN=EQN05', 'GB0', 'N'), -- Org Proxy, Additional Related Ports
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-12 12:00:00', @JcPk03, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=Achim|MST=Proxy|LOC=DKCNL|EQN=EQN06|STA=fffff', 'GB0', 'N'), -- Home Port
	(NEWID(), 'JobContainer', GETDATE(), '2023-01-13 12:00:00', @JcPk04, 'ATW', 'DEP=Terminal|TYP=Container Release|RFN=Altdorf|LOC=DKFBG|STA=ggggg', 'GB0', 'N'); -- Additional Related Ports

INSERT dbo.JobConsol (JK_PK, JK_UniqueConsignRef, JK_OA_ReceivingForwarderAddress) VALUES
	(@Jk_cPk01, 'JK_C01', @OaPk01),
	(@Jk_cPk02, 'JK_C02', @OaPk02),
	(@Jk_cPk03, 'JK_C03', @OaPk03),
	(@Jk_cPk04, 'JK_C04', NULL);

INSERT dbo.JobContainer (JC_PK, JC_JK) VALUES
	(@JcPk01, @Jk_cPk01),
	(@JcPk02, @Jk_cPk02),
	(@JcPk03, @Jk_cPk03),
	(@JcPk04, @Jk_cPk04);";

			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
