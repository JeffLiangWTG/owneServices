#region Test

using System;
using System.Globalization;
using Enterprise.Integration.Billing;

#if DEBUG

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Core
{
	abstract class CoreActiveStaffDailyTest : RefStlScriptWithDefaultsTest
	{
		protected override sealed void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				DELETE dbo.GlbStaff WHERE GS_IsSystemAccount = 0;
				DECLARE @GsPk01 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk02 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk03 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk04 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk05 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk06 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk07 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk08 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk09 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk10 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk11 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk12 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk13 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk14 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk15 UNIQUEIDENTIFIER = newid();
				DECLARE @GsPk16 UNIQUEIDENTIFIER = newid();
				DECLARE @GbPk UNIQUEIDENTIFIER = (SELECT GB_PK FROM dbo.GlbBranch WHERE GB_Code = 'DEM');
				DECLARE @PerPk UNIQUEIDENTIFIER = newid();

				INSERT dbo.GlbPerson (PER_PK, PER_FullName) values (@PerPk, 'name')
				-- UserSeat = IsActive + !IsDevice + CanLogin
				INSERT dbo.GlbStaff (GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_GB_HomeBranch, GS_GB_LastLogonBranch, GS_IsActive, GS_IsDevice, GS_CanLogin, GS_IsResource, GS_IsSystemAccount, GS_PER, GS_IsRobot, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES
					(@GsPk01, '@#1', 'Staff001', 'staff.001', @GbPk,  null, 1, 0, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- (*USR,  DOU,  HRU) UserSeat(Active + non-Device + CanLogin) + No UST events after collection range
					(@GsPk02, '@#2', 'Staff002', 'staff.002',  null, @GbPk, 0, 1, 0, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU,  HRU) !Active + No UST events
					(@GsPk03, '@#3', 'Staff003', 'staff.003', @GbPk,  null, 0, 0, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU,  HRU) !Active + No UST events during or after range (irrelevant UST event before range)
					(@GsPk04, '@#4', 'Staff004', 'staff.004',  null, @GbPk, 0, 1, 0, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- (*USR,  DOU,  HRU) Made !Active after range (1st UST event after range = USR>>INA)
					(@GsPk05, '@#5', 'Staff005', 'staff.005', @GbPk,  null, 0, 1, 0, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU,  HRU) Was !Active during range + Made UserSeat, then HR User, then !Active again after range (1st UST event after range = INA>>USR)
					(@GsPk06, '@#6', 'Staff006', 'staff.006',  null, @GbPk, 1, 0, 1, 0, 1, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU,  HRU) System account
					(@GsPk07, '@#7', 'Staff007', 'staff.007', @GbPk,  null, 1, 0, 1, 1, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU,  HRU) Resource
					(@GsPk08, '@#8', 'Staff008', 'staff.008',  null, @GbPk, 1, 0, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU,  HRU) New UserSeat, created after range (1st UST event after range = NEW>>USR)
					(@GsPk09, '@#9', 'Staff009', 'staff.009', @GbPk,  null, 1, 0, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- (*USR,  DOU,  HRU) Was UserSeat + Made HR user, then DeviceOnly, then UserSeat again after range (1st UST event after range = USR>>HRU)
					(@GsPk10, '@#A', 'Staff010', 'staff.010',  null, @GbPk, 1, 1, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- (*USR, *DOU,  HRU) Was UserSeat + Made DeviceOnly during range (UST event = USR>>DOU)
					(@GsPk11, '@#B', 'Staff011', 'staff.011', @GbPk,  null, 1, 1, 0, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR,  DOU, *HRU) HR User (!CanLogin, IsDevice flag ignored) + No UST events
					(@GsPk12, '@#C', 'Staff012', 'staff.012',  null, @GbPk, 1, 0, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- (*USR,  DOU, *HRU) Made UserSeat during range (UST event = HRU>>USR)
					(@GsPk13, '@#D', 'Staff013', 'staff.013', @GbPk,  null, 1, 1, 1, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR, *DOU, *HRU) Was HR User + Made DeviceOnly during range (UST event = HRU>>DOU)
					(@GsPk14, '@#E', 'Staff014', 'staff.014',  null, @GbPk, 1, 0, 0, 0, 0, @PerPk, 0, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- ( USR, *DOU,  HRU) Was DeviceOnly + Made HR User after range (1st UST event after range = DOU>>HRU)
					(@GsPk15, '@#F', 'Staff015', 'staff.015',  null, @GbPk, 1, 0, 1, 0, 0, @PerPk, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'), -- (*USR, *RBU,  HRU) Was UserSeat + Made Robot during range (UST event = USR>>RBU)
					(@GsPk16, '@#G', 'Staff016', 'staff.016', @GbPk,  null, 1, 0, 1, 0, 0, @PerPk, 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP'); -- ( USR,  DOU, *RBU) Robot User + No UST events

				INSERT dbo.StmALog (SL_PK, SL_Table, SL_EventTime, SL_PostedTimeUtc, SL_Parent, SL_SE_NKEvent, SL_Reference) VALUES
					(newid(), 'ParentTable', getdate(), '{2}-{3}-01 01:23:00', @GsPk01, 'UST', 'INA>>USR'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-02 02:23:00', @GsPk02, 'ADD', ''        ),
					(newid(), 'ParentTable', getdate(), '{0}-{1}-03 03:01:00', @GsPk03, 'UST', 'USR>>INA'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-03 03:23:00', @GsPk03, 'LGO', ''        ),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-04 04:45:00', @GsPk04, 'UST', 'USR>>INA'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-05 05:45:00', @GsPk05, 'UST', 'INA>>USR'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-05 05:45:01', @GsPk05, 'UST', 'USR>>HRU'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-05 05:45:02', @GsPk05, 'UST', 'HRU>>INA'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-06 06:23:00', @GsPk06, 'EDT', ''        ),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-07 07:23:00', @GsPk07, 'LGI', ''        ),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-08 08:45:00', @GsPk08, 'UST', 'NEW>>USR'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-09 09:45:00', @GsPk09, 'UST', 'USR>>HRU'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-09 09:45:01', @GsPk09, 'UST', 'HRU>>DOU'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-09 09:45:02', @GsPk09, 'UST', 'DOU>>USR'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-10 10:23:00', @GsPk10, 'UST', 'USR>>DOU'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-12 12:23:00', @GsPk12, 'UST', 'HRU>>USR'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-13 12:23:00', @GsPk13, 'UST', 'HRU>>DOU'),
					(newid(), 'ParentTable', getdate(), '{4}-{5}-14 14:45:00', @GsPk14, 'UST', 'DOU>>HRU'),
					(newid(), 'ParentTable', getdate(), '{2}-{3}-10 10:23:00', @GsPk15, 'UST', 'USR>>RBU');
				",
				RangeMonthAsDate.AddMonths(-1).Year,
				RangeMonthAsDate.AddMonths(-1).Month,
				RangeMonthAsDate.Year,
				RangeMonthAsDate.Month,
				RangeMonthAsDate.AddMonths(1).Year,
				RangeMonthAsDate.AddMonths(1).Month);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected void AssertTransaction(IStlTransaction transaction, string expectedStaffCode, string expectedFullName, string expectedLoginName)
		{
			AssertEquals("[T1] CompanyCode", "DEM", transaction.GetCompanyCode());
			AssertEquals("[T1] BranchCode", "DEM", transaction.GetBranchCode());
			AssertEquals("[T1] UserCode", expectedStaffCode, transaction.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction.BillableCount);
			AssertEquals("[T1] TransactionReference02", TransactionPeriod, transaction.Reference2);
			AssertEquals("[T1] TransactionReference03", expectedFullName, transaction.Reference3);
			AssertEquals("[T1] TransactionReference04", expectedLoginName, transaction.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange => AusydMonthRange.New(Year, Month);

		protected DateTime RangeMonthAsDate => new DateTime(Year, Month, 1);

		protected int DaysInMonth
		{
			get
			{
				if (daysInMonth == null)
				{
					daysInMonth = DateTime.DaysInMonth(Year, Month);
				}
				return daysInMonth.Value;
			}
		}
		int? daysInMonth;

		protected string TransactionPeriod
		{
			get
			{
				if (transactionPeriod == null)
				{
					transactionPeriod = ((RangeMonthAsDate.Year * 100) + RangeMonthAsDate.Month).ToString();
				}
				return transactionPeriod;
			}
		}
		string transactionPeriod;

		protected const int Year = 2022;
		protected const int Month = 3;
	}
}

#endif
#endregion
