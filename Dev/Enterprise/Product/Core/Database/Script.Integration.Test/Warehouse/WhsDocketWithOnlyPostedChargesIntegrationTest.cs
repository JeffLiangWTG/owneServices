using System;
using System.Data;
using System.Text;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using OrgHeader = CargoWise.Database.TestFramework.ObjectModel.OrgHeader;

namespace Enterprise.Build.Database.Script.Public.Warehouse
{
	class WhsDocketPostedChargesIntegrationTest : TransactionedTestCase
	{
		#region TestWhsDocketPostedCharges

		public void TestWhsDocketPostedCharges()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			var sql = new StringBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("W1", branchPK).WithDockDoor(TestConnection);
			var receive1 = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1", "ER00001") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var jobStorage = new JobStorage("I1", warehouse, client, today.AddDays(-1), today.AddDays(+1)).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var storageJobPK = helper.InsertJob("J01", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "ET", jobStorage.PK, "WRK", today);

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branchPK, departmentPK);
			var transactionLinePKRev1 = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "002", 0M, today, branchPK, departmentPK);
			var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, -1m, "CST", today, today);
			var jobChargePK = helper.InsertJobCharge(storageJobPK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev1, transactionLinePKCst, 1m);

			var properties = typeof(JobChargeAttribTypeList.Codes).GetFields();
			foreach (var property in properties)
			{
				var chargeCode = property.GetValue(null) as string;
				helper.InsertJobChargeAttrib(jobChargePK, chargeCode, "ER00001", 10);
				AssertEquals($"Should only consider charge is DocketReference but the current code is '{chargeCode}'.", chargeCode == JobChargeAttribTypeList.Codes.DocketReference ? 1 : 0, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges").Count);
				if (chargeCode == JobChargeAttribTypeList.Codes.DocketReference)
				{
					AssertEquals("Docket pk should match.", receive1.PK, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges")[0]["WD_PK"]);
				}
				Db.Connection.ExecuteNonQuery("Delete from dbo.JobChargeAttrib"); // clean for next charge code test 
			}
		}

		#endregion

		#region TestWhsDocketPostedCharges_DocketCharge

		public void TestWhsDocketPostedCharges_DocketCharge()
		{
			TestWhsDocketPostedCharges_DocketCharge_Core("WD", expectedChargesCount: 1);
		}

		public void TestWhsDocketPostedCharges_StorageCharge()
		{
			TestWhsDocketPostedCharges_DocketCharge_Core("ET", expectedChargesCount: 0); // cherge for docket should have WD parent table code
		}

		public void TestWhsDocketPostedCharges_OtherCharges()
		{
			TestWhsDocketPostedCharges_DocketCharge_Core("JS", expectedChargesCount: 0);
		}

		void TestWhsDocketPostedCharges_DocketCharge_Core(string parentTableCode, int expectedChargesCount)
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			var sql = new StringBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("W1", branchPK).WithDockDoor(TestConnection);
			var receive = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var jobPK = helper.InsertJob("R01", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, parentTableCode, receive.PK, "WRK", today);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branchPK, departmentPK);
			var transactionLinePKRev1 = helper.InsertTransactionLine(transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "002", 0M, today, branchPK, departmentPK);
			var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, jobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, -1m, "CST", today, today);
			helper.InsertJobCharge(jobPK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev1, transactionLinePKCst, 1m);

			AssertEquals("Is not same as expected charges count.", expectedChargesCount, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges").Count);
		}

		#endregion

		#region TestWhsDocketPostedChargesJobStrageDate

		public void TestWhsDocketPostedChargesJobStrageDate_InRange()
		{
			TestWhsDocketPostedChargesJobStrageDate_Core(-1, +1, expectedResultCount: 1);
		}

		public void TestWhsDocketPostedChargesJobStrageDate_NotInRange()
		{
			TestWhsDocketPostedChargesJobStrageDate_Core(+1, +3, expectedResultCount: 0);
		}

		public void TestWhsDocketPostedChargesJobStrageDate_Core(int fromDateOffset, int toDateOffset, int expectedResultCount)
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			var sql = new StringBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("W1", branchPK).WithDockDoor(TestConnection);
			var receive1 = new WhsDocket(client.PK, warehouse.PK, "INW", "REC", "FIN", "R1", "ER00001") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var jobStorage = new JobStorage("I1", warehouse, client, today.AddDays(fromDateOffset), today.AddDays(toDateOffset)).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var storageJobPK = helper.InsertJob("J01", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "ET", jobStorage.PK, "WRK", today);

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branchPK, departmentPK);
			var transactionLinePKRev1 = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "002", 0M, today, branchPK, departmentPK);
			var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, -1m, "CST", today, today);
			var jobChargePK = helper.InsertJobCharge(storageJobPK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev1, transactionLinePKCst, 1m);

			helper.InsertJobChargeAttrib(jobChargePK, JobChargeAttribTypeList.Codes.DocketReference, "ER00001", 10);
			AssertEquals("Only Should have result when docket Finalise data is not during in storage date.", expectedResultCount, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges").Count);
			if (expectedResultCount == 1)
			{
				AssertEquals("Only Should have result when docket Finalise data is not during in storage date.", receive1.PK, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges")[0]["WD_PK"]);
			}
		}

		#endregion

		#region TestWhsDocketPostedChargesJobStrageDate_InSameWarehouse

		public void TestWhsDocketPostedChargesJobStrageDate_InSameWarehouse()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branch1PK = helper.InsertBranch("ZZA", TestDbHelper.DefaultCompanyPK);
			var branch2PK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			var sql = new StringBuilder();
			var client = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var warehouse1 = new WhsWarehouse("W1", branch1PK).WithDockDoor(TestConnection);
			var warehouse2 = new WhsWarehouse("W2", branch2PK).WithDockDoor(TestConnection);
			new WhsDocket(client.PK, warehouse2.PK, "INW", "REC", "FIN", "R1", "ER00001") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var jobStorage = new JobStorage("I1", warehouse1, client, today.AddDays(-1), today.AddDays(+1)).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var storageJobPK = helper.InsertJob("J01", TestDbHelper.DefaultCompanyPK, branch1PK, departmentPK, "ET", jobStorage.PK, "WRK", today);

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branch1PK, departmentPK);
			var transactionLinePKRev1 = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branch1PK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "002", 0M, today, branch1PK, departmentPK);
			var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branch1PK, departmentPK, debtorOrgPK, -1m, "CST", today, today);
			var jobChargePK = helper.InsertJobCharge(storageJobPK, branch1PK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev1, transactionLinePKCst, 1m);

			helper.InsertJobChargeAttrib(jobChargePK, JobChargeAttribTypeList.Codes.DocketReference, "ER00001", 10);
			AssertEquals("Docket is not in same warehouse.", 0, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges").Count);
		}

		#endregion

		#region TestWhsDocketPostedChargesJobStrageDate_OnlySameClient

		public void TestWhsDocketPostedChargesJobStrageDate_OnlySameClient()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccountPK = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");

			var sql = new StringBuilder();
			var client1 = new OrgHeader("C1").AppendInsertAndReturnObject(sql);
			var client2 = new OrgHeader("C2").AppendInsertAndReturnObject(sql);
			var warehouse = new WhsWarehouse("W1", branchPK).WithDockDoor(TestConnection);
			new WhsDocket(client1.PK, warehouse.PK, "INW", "REC", "FIN", "R1", "ER00001") { WD_FinalisedDate = today, WD_ArrivalDate = today }.AppendInsertAndReturnObject(sql);
			var jobStorage = new JobStorage("I1", warehouse, client2, today.AddDays(-1), today.AddDays(+1)).AppendInsertAndReturnObject(sql);
			TestConnection.ExecuteNonQuery(sql.ToString());

			var storageJobPK = helper.InsertJob("J01", TestDbHelper.DefaultCompanyPK, branchPK, departmentPK, "ET", jobStorage.PK, "WRK", today);

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branchPK, departmentPK);
			var transactionLinePKRev1 = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, 1m, "REV", today, today);
			transactionHeaderPK = helper.InsertTransactionHeader("AP", "INV", "002", 0M, today, branchPK, departmentPK);
			var transactionLinePKCst = helper.InsertTransactionLine(transactionHeaderPK, storageJobPK, chargeCodePK, glAccountPK, branchPK, departmentPK, debtorOrgPK, -1m, "CST", today, today);
			var jobChargePK = helper.InsertJobCharge(storageJobPK, branchPK, TestDbHelper.DefaultCompanyPK, departmentPK, chargeCodePK, transactionLinePKRev1, transactionLinePKCst, 1m);

			helper.InsertJobChargeAttrib(jobChargePK, JobChargeAttribTypeList.Codes.DocketReference, "ER00001", 10);
			AssertEquals("Docket is for not same client.", 0, GetRowsByQuery("SELECT * FROM dbo.WhsDocketWithOnlyPostedCharges").Count);
		}

		#endregion

		#region Helper

		static DataRowCollection GetRowsByQuery(string sqlQuery) => DataUtils.GetDataTableFromCommand(Db.Connection.Command(sqlQuery)).Rows;
		#endregion
	}
}

