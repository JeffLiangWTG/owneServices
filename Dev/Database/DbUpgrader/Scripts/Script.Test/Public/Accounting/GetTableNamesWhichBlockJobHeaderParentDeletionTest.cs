using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.Testing
{
	[TestedType(typeof(GetTableNamesWhichBlockJobHeaderParentDeletion))]
	class GetTableNamesWhichBlockJobHeaderParentDeletionTest : DbCreateScriptTest
	{
		public void TestGetTableNamesWhichBlockJobHeaderParentDeletion()
		{
			var helper = new TestDbHelper(TestConnection);
			var today = DateTime.Today;
			var shipmentPK = helper.InsertShipment("S99999999", today);
			var sql = $"SELECT * FROM GetTableNamesWhichBlockJobHeaderParentDeletion('{shipmentPK}', '{string.Empty}', 0)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(0, result.Rows.Count);

			PrepareData(helper, shipmentPK);

			sql = $"SELECT JH_JobNum, GC_Code, TableName, BlockingReason FROM GetTableNamesWhichBlockJobHeaderParentDeletion('{shipmentPK}', '{string.Empty}', 0) ORDER BY GC_Code, TableName";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var expectedRows = new List<Tuple<string, string, string, string>>
			{
				Tuple.Create("EDI", "job1", "AccHotCheque", "HasSavedHotCheques"),
				Tuple.Create("EDI", "job1", "AccTransactionHeader", "HasPostedTransactions"),
				Tuple.Create("EDI", "job1", "AccTransactionLines", "HasPostedTransactions"),
				Tuple.Create("EDI", "job1", "AccTransactionLines", "HasPostedTransactions"),
				Tuple.Create("EDI", "job1", "JobCharge", "HasSavedNonZeroJobCharges"),
				Tuple.Create("SSC", "job2", "AccHotCheque", "HasSavedHotCheques"),
				Tuple.Create("SSC", "job2", "AccTransactionHeader", "HasPostedTransactions"),
				Tuple.Create("SSC", "job2", "AccTransactionLines", "HasPostedTransactions"),
				Tuple.Create("SSC", "job2", "JobCharge", "HasSavedNonZeroJobCharges"),
			};

			AssertEquals("Result should have rows", 8, result.Rows.Count);
			AssertDataTableContains(result, expectedRows);
		}

		public void TestGetTableNamesWhichBlockJobHeaderParentDeletionForNPL()
		{
			var helper = new TestDbHelper(TestConnection);
			var today = DateTime.Today;
			var shipmentPK = helper.InsertShipment("S99999999", today);

			var sql = $"SELECT * FROM GetTableNamesWhichBlockJobHeaderParentDeletion('{shipmentPK}', '{string.Empty}', 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(0, result.Rows.Count);

			var chargeCodePK = PrepareData(helper, shipmentPK);

			sql = $"SELECT JH_JobNum, GC_Code, TableName, BlockingReason FROM GetTableNamesWhichBlockJobHeaderParentDeletion('{shipmentPK}', '{string.Empty}', 1) ORDER BY GC_Code, TableName";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			var expectedRows = new List<Tuple<string, string, string, string>>
			{
				Tuple.Create("EDI", "job1", "AccHotCheque", "HasSavedHotCheques"),
				Tuple.Create("EDI", "job1", "AccTransactionHeader", "HasPostedTransactions"),
				Tuple.Create("EDI", "job1", "AccTransactionLines", "HasNotReversedWIPACR"),
				Tuple.Create("EDI", "job1", "JobCharge", "HasSavedNonZeroJobCharges"),
				Tuple.Create("SSC", "job2", "AccHotCheque", "HasSavedHotCheques"),
				Tuple.Create("SSC", "job2", "AccTransactionLines", "HasPostedTransactions"),
				Tuple.Create("SSC", "job2", "JobCharge", "HasSavedNonZeroJobCharges"),
			};

			AssertEquals("Result should have rows", 7, result.Rows.Count);
			AssertDataTableContains(result, expectedRows);

			helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "GLOBAL");
			sql = $"SELECT JH_JobNum, GC_Code, TableName, BlockingReason FROM GetTableNamesWhichBlockJobHeaderParentDeletion('{shipmentPK}', 'GLOBAL', 1) ORDER BY GC_Code, TableName";
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			AssertEquals("Result should have rows", 7, result.Rows.Count);
			AssertDataTableContains(result, expectedRows);
		}

		Guid PrepareData(TestDbHelper helper, Guid parentPK)
		{
			var today = DateTime.Today;
			var company1 = TestDbHelper.DefaultCompanyPK;
			var company2 = helper.InsertCompany("SSC", "Company2", "AUD", "AU", true, true);
			var company3 = helper.InsertCompany("DDC", "Company3", "AUD", "AU", true, true);
			var branch1 = helper.InsertBranch("ZZB", company1);
			var branch2 = helper.InsertBranch("SSB", company2);
			var branch3 = helper.InsertBranch("DDB", company3);
			var department1 = helper.InsertDepartment("ZZD");
			var department2 = helper.InsertDepartment("SSD");
			var department3 = helper.InsertDepartment("DDD");

			var creditorOrgPK = helper.InsertOrgHeader("ZZC", "Creditor");
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var glAccount = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(company1, "CC1");

			var job1 = helper.InsertJob("job1", company1, branch1, department1, "JS", parentPK, "WRK", today);
			var job2 = helper.InsertJob("job2", company2, branch2, department2, "JS", parentPK, "WRK", today);

			var line1 = helper.InsertTransactionLine(null, job1, chargeCodePK, null, branch1, department1, debtorOrgPK, 10m, "REV", today, today, companyPK: company1);
			var line2 = helper.InsertTransactionLine(null, job1, chargeCodePK, null, branch1, department1, creditorOrgPK, -10m, "CST", today, today, companyPK: company1);
			helper.InsertJobCharge(job1, branch1, company1, department1, chargeCodePK, line1, line2, 10);

			var transactionHeaderPK = helper.InsertTransactionHeader("AR", "INV", "001", 0M, today, branch1, department1, job: job1);
			var line3 = helper.InsertTransactionLine(transactionHeaderPK, job1, chargeCodePK, null, branch1, department1, debtorOrgPK, 20m, "REV", today, today, companyPK: company1);
			var line4 = helper.InsertTransactionLine(transactionHeaderPK, job1, chargeCodePK, null, branch1, department1, creditorOrgPK, -20m, "CST", today, today, companyPK: company1);
			helper.InsertJobCharge(job1, branch1, TestDbHelper.DefaultCompanyPK, department1, chargeCodePK, line3, line4, 20m);

			var bankAccount = helper.InsertBankAccount("BANK1", glAccount);
			var chequeBook1 = helper.InsertChequeBook("Book1", 100, 134, 500, bankAccount, branch1);
			helper.InsertHotCheque(chequeBook1, job1);

			var line1Company2 = helper.InsertTransactionLine(null, job2, chargeCodePK, null, branch2, department2, debtorOrgPK, 10m, "REV", today, today, companyPK: company2);
			var line2Company2 = helper.InsertTransactionLine(null, job2, chargeCodePK, null, branch2, department2, creditorOrgPK, -10m, "CST", today, today, companyPK: company2);
			helper.InsertJobCharge(job2, branch2, company2, department2, chargeCodePK, line1Company2, line2Company2, 10);

			var transactionHeaderPKCompany2 = helper.InsertTransactionHeader("JC", "JRJ", "001", 0M, today, branch2, department2, job: job2, companyPK: company2);
			var line3Company2 = helper.InsertTransactionLine(transactionHeaderPKCompany2, job2, chargeCodePK, null, branch2, department2, debtorOrgPK, 20m, "REV", today, today, companyPK: company2);
			var line4Company2 = helper.InsertTransactionLine(transactionHeaderPKCompany2, job2, chargeCodePK, null, branch2, department2, creditorOrgPK, -20m, "CST", today, today, companyPK: company2);
			helper.InsertJobCharge(job2, branch2, company2, department2, chargeCodePK, line3Company2, line4Company2, 20m);

			var chequeBook1Company2 = helper.InsertChequeBook("Book2", 100, 134, 500, bankAccount, branch2);
			helper.InsertHotCheque(chequeBook1Company2, job2);

			var transactionHeaderWithoutJob = helper.InsertTransactionHeader("AR", "INV", "003", 0M, today, branch3, department3, companyPK: company3);
			helper.InsertTransactionLine(transactionHeaderWithoutJob, null, chargeCodePK, null, branch3, department3, debtorOrgPK, 20m, "REV", today, today, companyPK: company3);
			var chequeBook2 = helper.InsertChequeBook("Book3", 100, 134, 500, bankAccount, branch3);
			helper.InsertHotCheque(chequeBook2);

			helper.InsertTransactionLine(null, job1, null, glAccount, branch2, department2, debtorOrgPK, 500M, "WIP", today, reverseDate: null, companyPK: company2);

			return chargeCodePK;
		}

		public void TestGetTableNamesWhichBlockJobHeaderParentDeletionForNPLWithSpecialJRJ()
		{
			var today = DateTime.Today;
			var helper = new TestDbHelper(TestConnection);

			var company1 = TestDbHelper.DefaultCompanyPK;
			var branch1 = helper.InsertBranch("ZZB", company1);
			var department1 = helper.InsertDepartment("ZZD");

			var creditorOrgPK = helper.InsertOrgHeader("ZZC", "Creditor");
			var debtorOrgPK = helper.InsertOrgHeader("ZZD", "Debtor");
			var glAccount = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
			var chargeCodePK = helper.InsertChargeCode(company1, "CC1");

			var shipment = helper.InsertShipment("S0000001", today);

			var job = helper.InsertJob("job", company1, branch1, department1, "JS", shipment, "WRK", today);

			var transactionHeader = helper.InsertTransactionHeader("JC", "JRJ", "002", 0M, today, branch1, department1, job: job);
			var line1 = helper.InsertTransactionLine(transactionHeader, job, chargeCodePK, null, branch1, department1, debtorOrgPK, 20m, "REV", today, today, companyPK: company1);
			var line2 = helper.InsertTransactionLine(transactionHeader, job, chargeCodePK, null, branch1, department1, creditorOrgPK, -20m, "CST", today, today, companyPK: company1);

			helper.InsertJobCharge(job, branch1, company1, department1, chargeCodePK, line1, line2, 0);
			helper.InsertTransactionLine(null, job, chargeCodePK, glAccount, branch1, department1, debtorOrgPK, 10m, "WIP", today, today, companyPK: company1);

			var sql = $"SELECT * FROM GetTableNamesWhichBlockJobHeaderParentDeletion('{shipment}', 'CC1', 1)";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(0, result.Rows.Count);
		}

		void AssertDataTableContains(DataTable dataTable, List<Tuple<string, string, string, string>> expectedValues)
		{
			foreach (var expected in expectedValues)
			{
				var isFound = dataTable.AsEnumerable().Any(row =>
					row["GC_Code"].ToString() == expected.Item1 &&
					row["JH_JobNum"].ToString() == expected.Item2 &&
					row["TableName"].ToString() == expected.Item3 &&
					row["BlockingReason"].ToString() == expected.Item4);

				AssertEquals($"The expected row was not found: GC_Code={expected.Item1}, JH_JobNum={expected.Item2}, TableName={expected.Item3}, BlockingReason={expected.Item4}.", true, isFound);
			}
		}
	}
}
