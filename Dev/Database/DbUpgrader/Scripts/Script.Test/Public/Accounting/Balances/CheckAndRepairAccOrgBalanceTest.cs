using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.Balances;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Balances
{
	[TestedType(typeof(CheckAndRepairAccOrgBalance))]
	class CheckAndRepairAccOrgBalanceTest : DbCreateScriptTest
	{
		// this stored procedure is tested in the unit tests for these two classes:
		// Dev\Enterprise\Product\Core\DbUpgrader\Transformation\DataModification\Public\Accounting\PopulateAccOrgBalanceFromDetailedRecords.cs
		// Dev\Enterprise\Product\Operations\Accounting\Business\StabilityCheck\AccountingStabilityChecker.cs

		/// <summary>
		/// Using snapshot protection because just to communicate that it executes procedure without a transaction.
		/// This is done only to ensure the procedure is compiled before putting Sch-M locks on for testing.
		/// In fact, after compilation, the execution fails due to the transaction check.
		/// </summary>
		[UseSnapshotProtection]
		public void TestTransactionCountIsTheSameBeforeAndAfterExecution()
		{
			TestHelper.AssertTransactionCountIsTheSameBeforeAndAfterExecution(TestConnection, ScriptToTest.Name, "AccOrgBalanceChanges", (cmd) => { });
		}

		/// <summary>
		/// Using snapshot protection because it attempts to execute procedure without a transaction.
		/// </summary>
		[UseSnapshotProtection]
		public void TestMustRunInTransaction()
		{
			TestHelper.AssertProcedureMustRunInTransaction(ScriptToTest.Name, (cmd) => { });
		}

		public void TestClaim()
		{
			var helper = new TestDbHelper(TestConnection);

			var org1 = helper.InsertOrgHeader("ZZC", "Org1");
			var orgContact1 = helper.InsertOrgContact("OC1", "", false, org1);
			var org2 = helper.InsertOrgHeader("ZZD", "Org2");
			var orgContact2 = helper.InsertOrgContact("OC2", "", false, org2);
			var company1 = TestDbHelper.DefaultCompanyPK;
			var branch1 = helper.InsertBranch("ZZB", company1);
			var department1 = helper.InsertDepartment("ZZD");

			var transactionPK = helper.InsertTransactionHeader("AR", "INV", "00001001", 100m, DateTime.Now, branch1, department1, companyPK: company1, org: org1, outstandingAmount: 100m);
			var transactionPK2 = helper.InsertTransactionHeader("AR", "INV", "00001002", 100m, DateTime.Now, branch1, department1, companyPK: company1, org: org2, currency: "CNY", exchangeRate: 2m, outstandingAmount: 100m);
			helper.InsertQueryClaim("Test1", "OPN", 10m, transactionPK, branch1, orgContact1, org1);

			var sql = $"SELECT * FROM dbo.AccOrgBalanceChanges WHERE Y2_GC = '{company1}'";
			var rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals("Precondition", 3, rows.Count());
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 10m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));

			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.AccQueryClaim DISABLE TRIGGER TG_AccQueryClaim_InsertToAccOrgBalanceChanges");

			helper.InsertQueryClaim("Test2", "OPN", 20m, transactionPK, branch1, orgContact1, org1);
			helper.InsertQueryClaim("Test3", "OPN", 10m, transactionPK2, branch1, orgContact2, org2);

			sql = $"SELECT * FROM dbo.AccOrgBalanceChanges WHERE Y2_GC = '{company1}'";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals("Precondition", 3, rows.Count());
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 10m));
			Assert("Precondition", rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));

			using (var cmd = TestConnection.Command("CheckAndRepairAccOrgBalance"))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.ExecuteNonQuery();
			}

			sql = $"SELECT * FROM dbo.AccOrgBalanceChanges WHERE Y2_GC = '{company1}'";
			rows = DataUtils.GetDataTableFromQuery(TestConnection, sql).AsEnumerable();
			AssertEquals(5, rows.Count());
			Assert(rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert(rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 10m));
			Assert(rows.Any(x => x.Field<Guid>("Y2_OH") == org1 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 20m));
			Assert(rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 0m));
			Assert(rows.Any(x => x.Field<Guid>("Y2_OH") == org2 && x.Field<string>("Y2_Ledger") == "AR" && x.Field<decimal>("Y2_ClaimDelta") == 5m));

			TestConnection.ExecuteNonQuery("ALTER TABLE dbo.AccQueryClaim ENABLE TRIGGER TG_AccQueryClaim_InsertToAccOrgBalanceChanges");
		}

		public void TestView_ExpectedJobChargeIndexesUsed()
		{
			var helper = new TestDbHelper(TestConnection);
			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.InsertBranch("ZZB", companyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var shipmentPK = helper.InsertShipment("S001", new DateTime(2012, 01, 01));
			var jobPK = helper.InsertJob("S001", companyPK, branchPK, departmentPK, "JS", shipmentPK, "WRK", new DateTime(2012, 07, 25));
			var chargeCodePK = helper.InsertChargeCode(companyPK, "CC1");
			var orgPK = helper.InsertOrgHeader("GRS", "OrgG");

			var jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 30m);
			jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 89m);
			jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 56.08m, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 65m);

			for (int i = 0; i < 100; i++)
			{
				var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "ARINV" + i, 10M, DateTime.Today, branchPK, departmentPK);
				var transactionLinePKRev = helper.InsertTransactionLine(transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, 100m, "REV", DateTime.Today, null);
				transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "APINV" + i, -10M, DateTime.Today, branchPK, departmentPK);
				var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, orgPK, -100m, "CST", DateTime.Today, null);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_ProFormaCost: true, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_LocalCostAmt: 96.08m + i, jR_ProFormaCost: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_LocalSellAmt: 14.89m + i, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_OH_CostAccount: orgPK, jR_ProFormaCost: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_OH_SellAccount: orgPK, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 56.08m + i, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 65m + i);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, transactionLinePKCst, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 46.09m + i, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 34.9m + i, jR_ProFormaCost: true, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, transactionLinePKCst, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 30m + i);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, transactionLinePKCst, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 40.90m + i, jR_ProFormaCost: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, Guid.Empty, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 89m + i);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, transactionLinePKRev, Guid.Empty, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 18.79m + i, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_ProFormaCost: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 40.90m + i, jR_ProFormaCost: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 18.79m + i, jR_ProFormaRevenue: true);
				jobChargePK = helper.InsertJobCharge(jobPK, branchPK, companyPK, departmentPK, chargeCodePK, Guid.Empty, Guid.Empty, jR_OH_CostAccount: orgPK, jR_LocalCostAmt: 46.09m + i, jR_OH_SellAccount: orgPK, jR_LocalSellAmt: 34.9m + i, jR_ProFormaCost: true, jR_ProFormaRevenue: true);
			}

			Db.Connection.ExecuteNonQuery($"UPDATE STATISTICS {JobChargeSchema.Constants.TableName} WITH FULLSCAN");

			// Please read the following content if changes are required: https://devops.wisetechglobal.com/wtg/CargoWise/_wiki/wikis/CargoWise.wiki?wikiVersion=GBwikiMaster&pagePath=%2FCargoWise%20Wiki%2FAccounting%2FReference%20and%20Checklists%2FAccounting%20DB%20Hits%20(and%20other%20performance%20related%20regressions)&pageId=1538 
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var totalRows = 0;
				TestConnection.ExecuteReader($"SELECT * FROM dbo.vw_AccOrgBalanceFromDetailedRecords",
					_ => totalRows++);
				AssertEquals(2, totalRows);
				var queryPlan = TestConnection.ExecutedCommandsAndQueryPlans.First(t => t.Item1.Contains("vw_AccOrgBalanceFromDetailedRecords"));
				var queryPlanAnalyzer = new QueryPlanalyzer(queryPlan.Item2.First());
				Assert("Non clustered index on NR_RX__JR_OH_CostAccount_JR_GC must be used.", queryPlanAnalyzer.IndexScans.Any(x => x.IndexName == "NR_RX__JR_OH_CostAccount_JR_GC"));
				Assert("Non clustered index on NR_RX__JR_OH_SellAccount_JR_GC must be used.", queryPlanAnalyzer.IndexScans.Any(x => x.IndexName == "NR_RX__JR_OH_SellAccount_JR_GC"));
			}
		}
	}
}

