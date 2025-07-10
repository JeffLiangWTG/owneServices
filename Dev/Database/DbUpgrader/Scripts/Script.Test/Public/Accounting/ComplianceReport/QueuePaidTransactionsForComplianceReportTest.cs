using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueuePaidTransactionsForComplianceReport))]
	class QueuePaidTransactionsForComplianceReportTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("LIB", helper.ToDate("2020-05-01"), helper.ToDate("2020-05-31"));

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var glAccount = helper.InsertGLAccount("1234.55.00", "GL Account");

			var creditor1PK = helper.InsertOrgHeader("CRD1", "Creditor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor1PK, OB_APPrintContractorForm = 1 });

			var creditor2PK = helper.InsertOrgHeader("CRD2", "Creditor2");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor2PK });

			//TODO: Add test data set up
			var inv11 = helper.InsertTransactionHeader("AP", "INV", "APINV0011", 100m, helper.ToDate("2020-04-21"), branchPK, departmentPK, org: creditor1PK);
			helper.InsertTransactionMatchLink(inv11, "M0001", helper.ToDate("2020-05-01"));
			var inv12 = helper.InsertTransactionHeader("AP", "INV", "APINV0012", 100m, helper.ToDate("2020-05-21"), branchPK, departmentPK, org: creditor1PK);
			var crd11 = helper.InsertTransactionHeader("AP", "CRD", "APCRD0011", 100m, helper.ToDate("2020-05-21"), branchPK, departmentPK, org: creditor1PK);
			helper.InsertTransactionMatchLink(crd11, "M0002", helper.ToDate("2020-05-21"));

			var inv21 = helper.InsertTransactionHeader("AP", "INV", "APINV0021", 100m, helper.ToDate("2020-05-21"), branchPK, departmentPK, org: creditor2PK);
			helper.InsertTransactionMatchLink(inv21, "M0003", helper.ToDate("2020-05-20"));

			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueuePaidTransactionsForComplianceReport @ReportPK = '{0}', @Ledger = '{1}', @TransactionType = '{2}'", reportPK, "AP", "INV"));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have one row", 1, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'May 1 2020' AND ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("ParentID", inv11, (Guid)result.Rows[0][0]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueuePaidTransactionsForComplianceReport @ReportPK = '{0}', @Ledger = '{1}', @TransactionType = '{2}', @DeleteExistingPivotAndQueue = 0", reportPK, "AP", "CRD"));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have two rows", 2, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_ReportType = 'LIB' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have two rows", 2, result.Rows.Count);
			AssertNotNull("Invoice", result.Rows.Cast<DataRow>().FirstOrDefault(x => (Guid)x[0] == inv11));
			AssertNotNull("Credit Note", result.Rows.Cast<DataRow>().FirstOrDefault(x => (Guid)x[0] == crd11));
		}
	}
}

