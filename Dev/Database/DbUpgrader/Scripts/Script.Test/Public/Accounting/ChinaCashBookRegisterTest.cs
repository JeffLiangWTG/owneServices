using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(ChinaCashBookRegister))]
	class ChinaCashBookRegisterTest : DbCreateScriptTest
	{
		public void TestReceiptsWithNoDepositBatch_AH_PostDateRange()
		{
			var helper = new TestDbHelper(TestConnection);
			var currentCompany = TestDbHelper.DefaultCompanyPK;
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");
			var bankAccountPK = helper.InsertBankAccount("BANKCDE", GetFirstGLAccount());

			var fromDate = new DateTime(2021, 01, 01, 00, 00, 00);
			var toDate = new DateTime(2021, 01, 31, 00, 00, 00);

			TestReceiptsWithNoDepositBatch_AH_PostDateRange_ForTransactionHeadersWhereClause();
			TestReceiptsWithNoDepositBatch_AH_PostDateRange_ForReceiptBatchesWhereClause();

			void TestReceiptsWithNoDepositBatch_AH_PostDateRange_ForTransactionHeadersWhereClause()
			{
				var receiptBatchPostDate = fromDate.AddDays(-1);

				TestReceiptsWithNoDepositBatch_AH_PostDateRange(false, fromDate.AddMinutes(-1), receiptBatchPostDate);
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(true, fromDate, receiptBatchPostDate);
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(true, fromDate.AddMinutes(1), receiptBatchPostDate);

				TestReceiptsWithNoDepositBatch_AH_PostDateRange(true, toDate.AddMinutes(-1), receiptBatchPostDate);
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(true, toDate, receiptBatchPostDate); // Should failed When use old sql
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(false, toDate.AddMinutes(1), receiptBatchPostDate);
			}

			void TestReceiptsWithNoDepositBatch_AH_PostDateRange_ForReceiptBatchesWhereClause()
			{
				var receiptPostDate = fromDate.AddDays(1);

				TestReceiptsWithNoDepositBatch_AH_PostDateRange(true, receiptPostDate, fromDate.AddMinutes(-1));
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(false, receiptPostDate, fromDate);
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(false, receiptPostDate, fromDate.AddMinutes(1));

				TestReceiptsWithNoDepositBatch_AH_PostDateRange(false, receiptPostDate, toDate.AddMinutes(-1));
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(false, receiptPostDate, toDate); // Should failed When use old sql
				TestReceiptsWithNoDepositBatch_AH_PostDateRange(true, receiptPostDate, toDate.AddMinutes(1));
			}

			void TestReceiptsWithNoDepositBatch_AH_PostDateRange(bool shouldContainTransaction, DateTime receiptPostDate, DateTime receiptBatchPostDate)
			{
				helper.InsertTransactionHeader("CB", "REC", "000000001", -50m, receiptPostDate, currentBranch, currentDep, bankAccountPK, receiptBatchNo: "000000002");
				helper.InsertTransactionHeader("CB", "RCB", "000000002", -70m, receiptBatchPostDate, currentBranch, currentDep, bankAccountPK);

				var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM ChinaCashBookRegister('{bankAccountPK}', '{fromDate.ToString("yyyy-MM-dd HH:mm:ss")}', '{toDate.ToString("yyyy-MM-dd HH:mm:ss")}', '{currentCompany}', '{currentBranch}')");
				AssertEquals(shouldContainTransaction ? 2 : 1, result.Rows.Count);
				AssertEquals("OSCredit", 70M, result.Rows[0]["OSCredit"]);
				if (shouldContainTransaction)
				{
					AssertEquals("OSDebit", 50M, result.Rows[1]["OSDebit"]);
				}
				DataUtils.GetDataTableFromQuery(TestConnection, "DELETE FROM dbo.AccTransactionHeader");
			}
		}

		public void TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange()
		{
			var helper = new TestDbHelper(TestConnection);
			var currentCompany = TestDbHelper.DefaultCompanyPK;
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");
			var bankAccountPK = helper.InsertBankAccount("BANKCDE", GetFirstGLAccount());

			var fromDate = new DateTime(2021, 01, 01, 00, 00, 00);
			var toDate = new DateTime(2021, 01, 31, 00, 00, 00);

			TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange_ForTransactionHeadersWhereClause();
			TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange_ForReceiptBatchesWhereClause();

			void TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange_ForTransactionHeadersWhereClause()
			{
				var receiptBatchPostDate = fromDate.AddDays(1);
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(true, fromDate.AddMinutes(-1), receiptBatchPostDate);
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(false, fromDate, receiptBatchPostDate);
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(false, fromDate.AddMinutes(1), receiptBatchPostDate);
			}

			void TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange_ForReceiptBatchesWhereClause()
			{
				var receiptPostDate = fromDate.AddDays(-1);
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(false, receiptPostDate, fromDate.AddMinutes(-1));
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(true, receiptPostDate, fromDate);
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(true, receiptPostDate, fromDate.AddMinutes(1));

				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(true, receiptPostDate, toDate.AddMinutes(-1));
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(true, receiptPostDate, toDate);  // Should failed When use old sql
				TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(false, receiptPostDate, toDate.AddMinutes(1));
			}

			void TestReceiptsBatchedThisDateRangeButReceiptedEarlier_AH_PostDateRange(bool shouldContainTransaction, DateTime receiptPostDate, DateTime receiptBatchPostDate)
			{
				helper.InsertTransactionHeader("CB", "REC", "000000001", -50m, receiptPostDate, currentBranch, currentDep, bankAccountPK, receiptBatchNo: "000000002");
				helper.InsertTransactionHeader("CB", "RCB", "000000002", -70m, receiptBatchPostDate, currentBranch, currentDep, bankAccountPK);

				var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM ChinaCashBookRegister('{bankAccountPK}', '{fromDate.ToString("yyyy-MM-dd HH:mm:ss")}', '{toDate.ToString("yyyy-MM-dd HH:mm:ss")}', '{currentCompany}', '{currentBranch}')");
				AssertEquals(shouldContainTransaction ? 2 : 1, result.Rows.Count);
				AssertEquals("OSCredit", 70M, result.Rows[0]["OSCredit"]);
				if (shouldContainTransaction)
				{
					AssertEquals("OSCredit", 50M, result.Rows[1]["OSCredit"]);
				}
				DataUtils.GetDataTableFromQuery(TestConnection, "DELETE FROM dbo.AccTransactionHeader");
			}
		}

		public void TestSampleCall()
		{
			var fromDate = new DateTime(2005, 05, 20).ToString("yyyyMMdd");
			var toDate = new DateTime(2005, 05, 31).ToString("yyyyMMdd");
			var postDate = new DateTime(2005, 05, 25);

			var helper = new TestDbHelper(TestConnection);
			var currentCompany = TestDbHelper.DefaultCompanyPK;
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");

			var bankAccountPK = helper.InsertBankAccount("BANKCDE", GetFirstGLAccount());
			var nonCurrentBranch = helper.InsertBranch("OTH", currentCompany);

			helper.InsertTransactionHeader("AP", "PAY", "00005013", -100m, postDate, currentBranch, currentDep, bankAccountPK);
			helper.InsertTransactionHeader("AP", "PAY", "00005014", -1000m, postDate, nonCurrentBranch, currentDep, bankAccountPK);

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from ChinaCashBookRegister('{bankAccountPK}', '{fromDate}', '{toDate}', '{currentCompany}', '{currentBranch}')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("BankCode", "BANKCDE", result.Rows[0]["BankCode"]);
			AssertEquals("PostDate", postDate, result.Rows[0]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", 100m, result.Rows[0]["OSDebit"]);
			AssertEquals("OSCredit", DBNull.Value, result.Rows[0]["OSCredit"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"select * from ChinaCashBookRegister('{bankAccountPK}', '{fromDate}', '{toDate}', '{currentCompany}', '{nonCurrentBranch}')");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			AssertEquals("BankCode", "BANKCDE", result.Rows[0]["BankCode"]);
			AssertEquals("PostDate", postDate, result.Rows[0]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", 1000m, result.Rows[0]["OSDebit"]);
			AssertEquals("OSCredit", DBNull.Value, result.Rows[0]["OSCredit"]);

			result = DataUtils.GetDataTableFromQuery(TestConnection,
				$"select * from ChinaCashBookRegister('{bankAccountPK}', '{fromDate}', '{toDate}', '{currentCompany}', null) ORDER BY OSDebit");
			AssertEquals("Result should have two row", 2, result.Rows.Count);

			AssertEquals("BankCode", "BANKCDE", result.Rows[0]["BankCode"]);
			AssertEquals("PostDate", postDate, result.Rows[0]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", 100m, result.Rows[0]["OSDebit"]);
			AssertEquals("OSCredit", DBNull.Value, result.Rows[0]["OSCredit"]);

			AssertEquals("BankCode", "BANKCDE", result.Rows[1]["BankCode"]);
			AssertEquals("PostDate", postDate, result.Rows[1]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", 1000m, result.Rows[1]["OSDebit"]);
			AssertEquals("OSCredit", DBNull.Value, result.Rows[1]["OSCredit"]);
		}

		public void TestThreeUnionSelects()
		{
			var fromDate = new DateTime(2005, 05, 20);
			var toDate = new DateTime(2005, 05, 31);

			var helper = new TestDbHelper(TestConnection);
			var currentCompany = TestDbHelper.DefaultCompanyPK;
			var currentBranch = helper.InsertBranch("CBH", TestDbHelper.DefaultCompanyPK);
			var currentDep = helper.InsertDepartment("CDP");

			var bankAccountPK = helper.InsertBankAccount("BANKCDE", GetFirstGLAccount());

			helper.InsertTransactionHeader("CB", "RCB", "00005013", -70m, new DateTime(2005, 05, 24), currentBranch, currentDep, bankAccountPK);
			helper.InsertTransactionHeader("CB", "REC", "00005014", -60m, new DateTime(2005, 05, 23), currentBranch, currentDep, bankAccountPK);
			helper.InsertTransactionHeader("CB", "REC", "00005015", -50m, new DateTime(2005, 05, 10), currentBranch, currentDep, bankAccountPK, receiptBatchNo: "00005013");

			var result = DataUtils.GetDataTableFromQuery(TestConnection,
				$@"select * from ChinaCashBookRegister('{bankAccountPK}', '{fromDate.ToString("yyyyMMdd")}', '{toDate.ToString("yyyyMMdd")}', '{currentCompany}', '{currentBranch}') 
ORDER BY ISNULL(OSDebit,0) + ISNULL(OSCredit,0) DESC");

			AssertEquals("Result should have three row", 3, result.Rows.Count);

			AssertEquals("BankCode", "BANKCDE", result.Rows[0]["BankCode"]);
			AssertEquals("PostDate", new DateTime(2005, 05, 24), result.Rows[0]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", DBNull.Value, result.Rows[0]["OSDebit"]);
			AssertEquals("OSCredit", 70m, result.Rows[0]["OSCredit"]);

			AssertEquals("BankCode", "BANKCDE", result.Rows[1]["BankCode"]);
			AssertEquals("PostDate", fromDate, result.Rows[1]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", 60m, result.Rows[1]["OSDebit"]);
			AssertEquals("OSCredit", DBNull.Value, result.Rows[1]["OSCredit"]);

			AssertEquals("BankCode", "BANKCDE", result.Rows[2]["BankCode"]);
			AssertEquals("PostDate", fromDate, result.Rows[2]["PostDateFilterColumn"]);
			AssertEquals("OSDebit", DBNull.Value, result.Rows[2]["OSDebit"]);
			AssertEquals("OSCredit", 50m, result.Rows[2]["OSCredit"]);
		}

		Guid GetFirstGLAccount()
		{
			string sql = "SELECT TOP 1 AG_PK FROM dbo.AccGLHeader";
			return (Guid)TestConnection.ExecuteScalar(sql);
		}
	}
}

