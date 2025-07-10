using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(Compact_AccGLAggregate))]
	class Compact_AccGLAggregateTest : DbCreateScriptTest
	{
		public void TestAccGLAggregateTableStructureHasNotChanged()
		{
			var tableName = "AccGlAggregate";

			var message = $"Compact_AccGLAggregate depends on the table {tableName}. If this test fails it means it's structure has changed and need to be reflected in Compact_AccGLAggregate. Most likely, columns in {tableName} should be either grouped by, or aggregated to preserve data when compacting.";

			var expectedColumns = new string[] {
				"AA_PK",					// Ignore
				"AA_Amount",				// SUM()
				"AA_TransactionCategory",	// GROUP BY
				"AA_Period",				// GROUP BY
				"AA_AG",					// GROUP BY
				"AA_GB",					// GROUP BY
				"AA_GE",					// GROUP BY
				"AA_GC",					// GROUP BY
				"AA_SystemCreateTimeUtc",	// Ignore
				"AA_SystemCreateUser",		// Ignore
				"AA_SystemLastEditTimeUtc",	// Ignore
				"AA_SystemLastEditUser"		// Ignore
			};

			var template = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='{tableName}'";

			var result = DataUtils.GetDataTableFromQuery(TestConnection, template);
			AssertEquals(message, expectedColumns.Length, result.Rows.Count);

			var actualColumns = result.Rows.OfType<DataRow>().Select(r => r["COLUMN_NAME"].ToString());

			AssertContainsExactElementsInAnyOrder(message, expectedColumns, actualColumns);
		}

		public void TestCompact_AccGLAggregate_DifferentPeriod()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			helper.InsertAccPeriod(2019, 7);
			var period2 = 201907;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");

			helper.InsertGLAggregate(50m, "", period1, account1);
			helper.InsertGLAggregate(50m, "", period2, account1);

			AssertEquals("Precondition", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
			AssertEquals("Precondition", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period2));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");
			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201907");

			AssertEquals("Should not compact", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
			AssertEquals("Should not compact", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period2));
		}

		public void TestCompact_AccGLAggregate_DifferentTransactionCategory()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");

			helper.InsertGLAggregate(50m, "T1", period1, account1);
			helper.InsertGLAggregate(50m, "T2", period1, account1);

			AssertEquals("Precondition", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");

			AssertEquals("Should not compact", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
		}

		public void TestCompact_AccGLAggregate_DifferentAccount()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");
			var account2 = helper.InsertGLAccount("TestAcc2", "test");

			helper.InsertGLAggregate(50m, "", period1, account1);
			helper.InsertGLAggregate(50m, "", period1, account2);

			AssertEquals("Precondition", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
			AssertEquals("Precondition", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account2, period1));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");

			AssertEquals("Should not compact", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
			AssertEquals("Should not compact", (50m, 1), GetTotalAmountAndCountForAccountAndPeriod(account2, period1));
		}

		public void TestCompact_AccGLAggregate_DifferentBranch()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");
			var otherBranchPK = helper.InsertBranch("OBR");

			helper.InsertGLAggregate(50m, "", period1, account1, branchPK: helper.DefaultBranchPK);
			helper.InsertGLAggregate(50m, "", period1, account1, branchPK: otherBranchPK);

			AssertEquals("Precondition", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");

			AssertEquals("Should not compact", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
		}

		public void TestCompact_AccGLAggregate_DifferentDepartment()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");
			var otherDepartmentPK = helper.InsertDepartment("ODP");

			helper.InsertGLAggregate(50m, "", period1, account1, departmentPK: TestDbHelper.DepartmentBrnPK);
			helper.InsertGLAggregate(50m, "", period1, account1, departmentPK: otherDepartmentPK);

			AssertEquals("Precondition", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");

			AssertEquals("Should not compact", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
		}

		public void TestCompact_AccGLAggregate_DifferentCompany()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");
			var otherCompanyPK = helper.InsertCompany("OCP", "Other Company", "AUD", "AU", false, false);

			helper.InsertGLAggregate(50m, "", period1, account1, companyPK: TestDbHelper.DefaultCompanyPK);
			helper.InsertGLAggregate(50m, "", period1, account1, companyPK: otherCompanyPK);

			AssertEquals("Precondition", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");
			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{otherCompanyPK}', 201906");

			AssertEquals("Should not compact", (100m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
		}

		public void TestCompact_AccGLAggregate_Aggregation()
		{
			var helper = new TestDbHelper(TestConnection);
			helper.InsertAccPeriod(2019, 6);
			var period1 = 201906;
			var account1 = helper.InsertGLAccount("TestAcc1", "test");
			var account2 = helper.InsertGLAccount("TestAcc2", "test");
			var account3 = helper.InsertGLAccount("TestAcc3", "test");
			var account4 = helper.InsertGLAccount("TestAcc4", "test");

			helper.InsertGLAggregate(50m, "", period1, account1);
			helper.InsertGLAggregate(5m, "", period1, account1);
			helper.InsertGLAggregate(50m, "", period1, account2);
			helper.InsertGLAggregate(-5m, "", period1, account2);
			helper.InsertGLAggregate(50m, "", period1, account3);
			helper.InsertGLAggregate(-50m, "", period1, account3);
			helper.InsertGLAggregate(0m, "", period1, account4);
			helper.InsertGLAggregate(0m, "", period1, account4);

			AssertEquals("Precondition", (55m, 2), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
			AssertEquals("Precondition", (45m, 2), GetTotalAmountAndCountForAccountAndPeriod(account2, period1));
			AssertEquals("Precondition", (0m, 2), GetTotalAmountAndCountForAccountAndPeriod(account3, period1));
			AssertEquals("Precondition", (0m, 2), GetTotalAmountAndCountForAccountAndPeriod(account4, period1));

			TestConnection.ExecuteNonQuery($"EXEC Compact_AccGLAggregate '{TestDbHelper.DefaultCompanyPK}', 201906");

			AssertEquals("Should compact account and sum total amounts", (55m, 1), GetTotalAmountAndCountForAccountAndPeriod(account1, period1));
			AssertEquals("Should compact account and sum total amounts", (45m, 1), GetTotalAmountAndCountForAccountAndPeriod(account2, period1));
			AssertEquals("Should compact account and sum total amounts", (0m, 1), GetTotalAmountAndCountForAccountAndPeriod(account3, period1));
			AssertEquals("Should compact account and sum total amounts", (0m, 1), GetTotalAmountAndCountForAccountAndPeriod(account4, period1));
		}

		(decimal, int) GetTotalAmountAndCountForAccountAndPeriod(Guid glAccount, int period)
		{
			var sql = "SELECT ISNULL(SUM(AA_Amount), 0), COUNT(1) FROM AccGLAggregate WHERE AA_AG = @AG AND AA_Period = @Period";
			var cmd = TestConnection.Command(sql);
			cmd.AddParameter("@AG", SqlDbType.UniqueIdentifier, glAccount);
			cmd.AddParameter("@Period", SqlDbType.Int, period);
			using (var reader = cmd.ExecuteReader())
			{
				reader.Read();
				return ((decimal)reader[0], (int)reader[1]);
			}
		}
	}
}
