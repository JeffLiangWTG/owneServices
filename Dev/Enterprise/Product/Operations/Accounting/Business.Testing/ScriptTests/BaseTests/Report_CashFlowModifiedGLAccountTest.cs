using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class Report_CashFlowModifiedGLAccountTest : ScriptTest
	{
		public void TestReport_CashFlowModifiedGLAccountTest()
		{
			using (Connection = Db.NewAdminConnection())
			{
				PrepareReport_CashFlowModifiedGLAccountTest();

				DataTable result = null;
				AssertNoExceptionThrown("Staff full name won't be truncated", () => result = DataUtils.GetDataTableFromQuery(Connection, $"SELECT * FROM [{ScriptDbName}].[dbo].Report_CashFlowModifiedGLAccount(200710, '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC')"));
				AssertEquals(1, result.Rows.Count);
				AssertEquals("staffFullName", result.Rows[0]["StaffName"]);
				AssertEquals("Undefined", result.Rows[0]["oldCashFlowDescription"]);
				AssertEquals("XXX", result.Rows[0]["oldCashFlowCode"]);
				AssertEquals("CSH", result.Rows[0]["newCashFlowCode"]);
				AssertEquals("Cash or Cash Equivalent", result.Rows[0]["newCashFlowDescription"]);
				AssertEquals(new DateTime(2007, 10, 01), result.Rows[0]["EventTime"]);
			}
		}

		protected virtual void PrepareReport_CashFlowModifiedGLAccountTest()
		{
			PrepareData();
		}

		protected virtual string ScriptDbName => Db.DatabaseName;

		void PrepareData()
		{
			DbHelper.InsertAccPeriod(2007, 10);

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_DebitCredit, AG_ControlAccount, AG_AG_HeaderDependsOnTotal, AG_TotalLevel, AG_Column)
			VALUES ('E2502D59-69F0-4E8C-A2EC-40E40E006B3F', '1330.20.30', 'PNL Account 1', 'P&L', 'CR', 1, NULL, 0, 'TS');");

			Connection.ExecuteNonQuery(@"INSERT INTO dbo.StmALog (SL_PK, SL_EventTime, SL_PostedTimeUtc, SL_SE_NKEvent, SL_Parent, SL_Table, SL_GS_NKUser, SL_Reference)
			VALUES(NEWID(), '2007-10-01 00:00:00', '2007-10-01 00:00:00', 'EDT', 'E2502D59-69F0-4E8C-A2EC-40E40E006B3F', 'AccGLHeader', 'C', 'Cash Flow Type updated. Previous value: ''[XXX]'', New value: ''(CSH)''')");

			Connection.ExecuteNonQuery(@"UPDATE dbo.GlbStaff SET GS_FullName = 'staffFullName', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetUtcDate() WHERE GS_Code = 'C';");
		}

		protected AdminConnection Connection;

		protected TestDbHelper DbHelper => dbHelper ?? (dbHelper = new TestDbHelper(Connection));
		TestDbHelper dbHelper;
	}
}
