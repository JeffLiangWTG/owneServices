using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.Warehouse;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	[TestedType(typeof(WhsChargeOnJobWithOnlyPostedCharges))]
	class WhsChargeOnJobWithOnlyPostedChargesTest : DbCreateScriptTest
	{
		#region TestWhsChargeOnJobWithOnlyPostedCharges

		public void TestWhsChargeOnJobWithOnlyPostedCharges()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			var sql = new SqlQueryBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("W1", branchPK).WithDockDoor(TestConnection);
			var receive1 = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var job1PK = helper.InsertJob("R01", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "WD", receive1.PK, "WRK", today);
			var receive2 = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R2") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var job2PK = helper.InsertJob("R02", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "WD", receive2.PK, "WRK", today);
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branchPK, departmentPK);
			var transactionLinePKRev1 = helper.InsertTransactionLine(transactionHeaderPK, job1PK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "002", 0M, today, branchPK, departmentPK);
			var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, job1PK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, -1m, "CST", today, today);
			var charge1 = helper.InsertJobCharge(job1PK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev1, transactionLinePKCst, 1m);

			var transactionLinePKRev2 = helper.InsertTransactionLine(transactionHeaderPK, job2PK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			var charge2 = helper.InsertJobCharge(job2PK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev2, transactionLinePKCst, 1m);

			var rows1 = GetCountByQuery("SELECT * FROM dbo.WhsChargeOnJobWithOnlyPostedCharges");
			AssertEquals("Should contains REV charges.", 2, rows1.Count());
			AssertEquals("Should contains REV charges.", 1, rows1.Count(r => r["JR_PK"].ToString() == charge1.ToString() && r["JR_JH"].ToString() == job1PK.ToString()));
			AssertEquals("Should contains REV charges.", 1, rows1.Count(r => r["JR_PK"].ToString() == charge2.ToString() && r["JR_JH"].ToString() == job2PK.ToString()));

			var transactionLinePKACR = helper.InsertTransactionLine(transactionHeaderPK, job1PK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "ACR", today, today);
			helper.InsertJobCharge(job1PK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKACR, transactionLinePKCst, 1m);

			var rows2 = GetCountByQuery("SELECT * FROM dbo.WhsChargeOnJobWithOnlyPostedCharges");
			AssertEquals("ACR should not effect on result.", 2, rows2.Count());
			AssertEquals("ACR should not effect on result.", 1, rows2.Count(r => r["JR_PK"].ToString() == charge1.ToString() && r["JR_JH"].ToString() == job1PK.ToString()));
			AssertEquals("ACR should not effect on result.", 1, rows2.Count(r => r["JR_PK"].ToString() == charge2.ToString() && r["JR_JH"].ToString() == job2PK.ToString()));

			var transactionLinePKWIP = helper.InsertTransactionLine(transactionHeaderPK, job1PK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "WIP", today, today);
			helper.InsertJobCharge(job1PK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKWIP, transactionLinePKCst, 1m);

			var rows3 = GetCountByQuery("SELECT * FROM dbo.WhsChargeOnJobWithOnlyPostedCharges");
			AssertEquals("Should not include job when it has WIP transaction line.", 1, rows3.Count());
			AssertEquals("Should not include job when it has WIP transaction line.", 0, rows3.Count(r => r["JR_PK"].ToString() == charge1.ToString() && r["JR_JH"].ToString() == job1PK.ToString()));
			AssertEquals("Should include job when it does not have WIP transaction line.", 1, rows3.Count(r => r["JR_PK"].ToString() == charge2.ToString() && r["JR_JH"].ToString() == job2PK.ToString()));
		}

		#endregion

		#region Helper

		static IEnumerable<DataRow> GetCountByQuery(string sqlQuery) => DataUtils.GetDataTableFromCommand(Db.Connection.Command(sqlQuery)).Rows.Cast<DataRow>();
		#endregion
	}
}

