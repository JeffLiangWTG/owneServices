using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Balances;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Balances
{
	[TestedType(typeof(AggregateAccOrgBalanceChangesIntoAccOrgBalance))]
	class AggregateAccOrgBalanceChangesIntoAccOrgBalanceTest : DbCreateScriptTest
	{
		// this stored procedure is tested in vw_AccOrgBalanceTest.

		/// <summary>
		/// Using snapshot protection because just to communicate that it executes procedure without a transaction.
		/// This is done only to ensure the procedure is compiled before putting Sch-M locks on for testing.
		/// In fact, after compilation, the execution fails due to the transaction check.
		/// </summary>
		[UseSnapshotProtection]
		public void TestTransactionCountIsTheSameBeforeAndAfterExecution()
		{
			TestHelper.AssertTransactionCountIsTheSameBeforeAndAfterExecution(TestConnection, ScriptToTest.Name, "AccOrgBalanceChanges", TestHelper.AddParameterSystemLastEditUser);
		}

		/// <summary>
		/// Using snapshot protection because it attempts to execute procedure without a transaction.
		/// </summary>
		[UseSnapshotProtection]
		public void TestMustRunInTransaction()
		{
			TestHelper.AssertProcedureMustRunInTransaction(ScriptToTest.Name, TestHelper.AddParameterSystemLastEditUser);
		}

		public void TestClaim()
		{
			var helper = new TestDbHelper(TestConnection);

			var org1 = helper.InsertOrgHeader("ZZC", "Org1");
			var org2 = helper.InsertOrgHeader("ZZD", "Org2");
			var orgContact1 = helper.InsertOrgContact("OC1", "", false, org1);
			var orgContact2 = helper.InsertOrgContact("OC2", "", false, org2);
			var company1 = TestDbHelper.DefaultCompanyPK;
			var branch1 = helper.InsertBranch("ZZB", company1);
			var department1 = helper.InsertDepartment("ZZD");

			var transactionPK = helper.InsertTransactionHeader("AR", "INV", "00001001", 100m, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: 100m);
			helper.InsertQueryClaim("Test1", "OPN", 10m, transactionPK, branch1, orgContact1, org1);

			var transactionPK2 = helper.InsertTransactionHeader("AP", "INV", "00001002", -90m, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: -90m);
			helper.InsertQueryClaim("Test2", "OPN", 20m, transactionPK2, branch1, orgContact1, org1);

			var transactionPK3 = helper.InsertTransactionHeader("AP", "INV", "00001003", -80m, DateTime.Now, branch1, department1, companyPK: company1, org: org2, outstandingAmount: -80m);
			helper.InsertQueryClaim("Test3", "OPN", 30m, transactionPK3, branch1, orgContact2, org2);

			var transactionPK4 = helper.InsertTransactionHeader("AP", "INV", "00001004", -70m, DateTime.Now, branch1, department1, companyPK: company1, org: org2, outstandingAmount: -70m);
			helper.InsertQueryClaim("Test4", "OPN", 40m, transactionPK4, branch1, orgContact2, org2);

			var sql = $"SELECT * FROM dbo.AccOrgBalanceChanges WHERE Y2_GC = '{company1}'";
			var rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals("Precondition", 8, rows.Count());
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 10m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AP" && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AP" && x.Field<decimal>("Y2_ClaimDelta") == 20m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AP" && x.Field<decimal>("Y2_BalanceDelta") == 80m && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AP" && x.Field<decimal>("Y2_ClaimDelta") == 30m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AP" && x.Field<decimal>("Y2_BalanceDelta") == 70m && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AP" && x.Field<decimal>("Y2_ClaimDelta") == 40m));

			sql = $"SELECT * FROM dbo.AccOrgBalance WHERE Y3_GC = '{company1}'";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals("Precondition", 0, rows.Count());

			using (var cmd = TestConnection.Command("AggregateAccOrgBalanceChangesIntoAccOrgBalance"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, "TST");
				cmd.ExecuteNonQuery();
			}

			sql = $"SELECT * FROM dbo.AccOrgBalanceChanges WHERE Y2_GC = '{company1}'";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals(0, rows.Count());

			sql = $"SELECT * FROM dbo.AccOrgBalance WHERE Y3_GC = '{company1}'";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals("Precondition", 3, rows.Count());
			var row = rows.FirstOrDefault(x => x.Field<Guid>("Y3_OH") == org1 && x.Field<string>("Y3_Ledger") == "AR");
			AssertEquals(10m, row["Y3_Claim"]);
			row = rows.FirstOrDefault(x => x.Field<Guid>("Y3_OH") == org1 && x.Field<string>("Y3_Ledger") == "AP");
			AssertEquals(20m, row["Y3_Claim"]);
			row = rows.FirstOrDefault(x => x.Field<Guid>("Y3_OH") == org2 && x.Field<string>("Y3_Ledger") == "AP");
			AssertEquals(70m, row["Y3_Claim"]);
		}

		public void TestAggregateAccOrgBalanceChangesIntoAccOrgBalanceWhenAccOrgBalanceChangesAddsNewColumn()
		{
			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.AccOrgBalanceChanges ADD TestColumn char(1) NULL");
			using (var cmd = TestConnection.Command("AggregateAccOrgBalanceChangesIntoAccOrgBalance"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@SystemLastEditUser", SqlDbType.VarChar, "TST");
				AssertNoExceptionThrown(() => cmd.ExecuteNonQuery());
			}
		}
	}
}

