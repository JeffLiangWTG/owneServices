using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueFullyPaidAPInvoicesForPTRS))]
	class QueueFullyPaidAPInvoicesForPTRSTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("PTR", helper.ToDate("2021-01-01"), helper.ToDate("2021-06-30"));

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			var creditor1PK = helper.InsertOrgHeader("CRD1", "Creditor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor1PK, OB_IsCreditor = 1 });

			var creditor2PK = helper.InsertOrgHeader("CRD2", "Creditor2");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor2PK, OB_IsCreditor = 1, OB_APExcludeFromPaymentReports = 1 });
			helper.Insert("JobRequiredDocument", new { EQ_PK = Guid.NewGuid(), EQ_DocCategory = "CTR", EQ_DocType = "RSB", EQ_ParentTableCode = "OH", EQ_ParentID = creditor2PK, EQ_DateReceived = helper.ToDate("2021-04-30"), EQ_ValidToDate = helper.ToDate("2021-06-30") });

			var creditor3PK = helper.InsertOrgHeader("CRD3", "Creditor3");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor3PK, OB_IsCreditor = 1 });
			helper.Insert("JobRequiredDocument", new { EQ_PK = Guid.NewGuid(), EQ_DocCategory = "CTR", EQ_DocType = "RSB", EQ_ParentTableCode = "OH", EQ_ParentID = creditor3PK, EQ_DateReceived = helper.ToDate("2021-04-30"), EQ_ValidToDate = helper.ToDate("2021-06-30") });

			// Invoices for non-applicable creditors
			var inv11 = helper.InsertTransactionHeader("AP", "INV", "APINV0011", -110m, helper.ToDate("2021-04-11"), branchPK, departmentPK, org: creditor1PK, fullyPaidDate: helper.ToDate("2021-05-01"));
			helper.InsertTransactionMatchLink(inv11, "M00011", helper.ToDate("2021-05-01"), -110m);
			var inv21 = helper.InsertTransactionHeader("AP", "INV", "APINV0021", -210m, helper.ToDate("2021-04-21"), branchPK, departmentPK, org: creditor2PK, fullyPaidDate: helper.ToDate("2021-05-02"));
			helper.InsertTransactionMatchLink(inv21, "M00021", helper.ToDate("2021-05-02"), -210m);

			// Non-reportable Invoices for Creditor3
			var inv31 = helper.InsertTransactionHeader("AP", "INV", "APINV0031", -310m, helper.ToDate("2020-12-01"), branchPK, departmentPK, org: creditor3PK, fullyPaidDate: helper.ToDate("2020-12-31"));
			helper.InsertTransactionMatchLink(inv31, "M00035", helper.ToDate("2021-06-05"), -310m); // Matched together with reportable inv35
			var inv32 = helper.InsertTransactionHeader("AP", "INV", "APINV0032", -320m, helper.ToDate("2021-03-02"), branchPK, departmentPK, org: creditor3PK, fullyPaidDate: helper.ToDate("2021-07-01"));
			helper.InsertTransactionMatchLink(inv32, "M00032", helper.ToDate("2021-07-01"), -320m);
			var inv33 = helper.InsertTransactionHeader("AP", "INV", "APINV0033", -330m, helper.ToDate("2021-05-21"), branchPK, departmentPK, org: creditor3PK, isCancelled: true, fullyPaidDate: helper.ToDate("2021-06-01"));
			helper.InsertTransactionMatchLink(inv33, "M00033", helper.ToDate("2021-06-01"), -330m);

			// Reportable Invoices for Creditor3
			var inv34 = helper.InsertTransactionHeader("AP", "INV", "APINV0034", -340m, helper.ToDate("2021-05-21"), branchPK, departmentPK, org: creditor3PK, fullyPaidDate: helper.ToDate("2021-06-01"));
			helper.InsertTransactionMatchLink(inv34, "M00034", helper.ToDate("2021-06-01"), -340m);

			var inv35 = helper.InsertTransactionHeader("AP", "INV", "APINV0035", -350m, helper.ToDate("2021-05-05"), branchPK, departmentPK, org: creditor3PK, fullyPaidDate: helper.ToDate("2021-06-05"));
			var crd31 = helper.InsertTransactionHeader("AP", "CRD", "APCRD0031", 100m, helper.ToDate("2020-12-05"), branchPK, departmentPK, org: creditor3PK, fullyPaidDate: helper.ToDate("2021-06-05"));
			var crd32 = helper.InsertTransactionHeader("AP", "CRD", "APCRD0032", 200m, helper.ToDate("2021-01-05"), branchPK, departmentPK, org: creditor3PK, fullyPaidDate: helper.ToDate("2021-06-05"));
			helper.InsertTransactionMatchLink(inv35, "M00035", helper.ToDate("2021-06-05"), -350m);
			helper.InsertTransactionMatchLink(crd31, "M00035", helper.ToDate("2020-06-05"), 100m);
			helper.InsertTransactionMatchLink(crd32, "M00035", helper.ToDate("2020-06-05"), 200m);

			// Creditors having PTR document valid outside of Report period
			var creditor4PK = helper.InsertOrgHeader("CRD4", "Creditor4");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor4PK, OB_IsCreditor = 1 });
			helper.Insert("JobRequiredDocument", new { EQ_PK = Guid.NewGuid(), EQ_DocCategory = "CTR", EQ_DocType = "RSB", EQ_ParentTableCode = "OH", EQ_ParentID = creditor4PK, EQ_DateReceived = helper.ToDate("2020-07-04"), EQ_ValidToDate = helper.ToDate("2020-12-31") });

			var creditor5PK = helper.InsertOrgHeader("CRD5", "Creditor5");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor5PK, OB_IsCreditor = 1 });
			helper.Insert("JobRequiredDocument", new { EQ_PK = Guid.NewGuid(), EQ_DocCategory = "CTR", EQ_DocType = "RSB", EQ_ParentTableCode = "OH", EQ_ParentID = creditor5PK, EQ_DateReceived = helper.ToDate("2021-06-30"), EQ_ValidToDate = helper.ToDate("2021-07-01") });

			// Invoices for creditors having PTR document valid outside of Report period
			var inv41 = helper.InsertTransactionHeader("AP", "INV", "APINV0041", -110m, helper.ToDate("2021-04-11"), branchPK, departmentPK, org: creditor4PK, fullyPaidDate: helper.ToDate("2021-05-01"));
			helper.InsertTransactionMatchLink(inv41, "M00041", helper.ToDate("2021-05-01"), -110m);
			var inv51 = helper.InsertTransactionHeader("AP", "INV", "APINV0021", -210m, helper.ToDate("2021-04-21"), branchPK, departmentPK, org: creditor5PK, fullyPaidDate: helper.ToDate("2021-05-02"));
			helper.InsertTransactionMatchLink(inv51, "M00051", helper.ToDate("2021-05-02"), -210m);

			// Executing the stored proc
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueFullyPaidAPInvoicesForPTRS @ReportPK = '{0}'", reportPK));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have two rows", 2, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Jun 1 2021' AND ACQ_ReportType = 'PTR' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("ParentID", inv34, (Guid)result.Rows[0][0]);
			AssertEquals("ReportSubCode - Invoice was fully paid in 11 days", "340.00|11", (string)result.Rows[0][1]);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Jun 5 2021' AND ACQ_ReportType = 'PTR' AND ACQ_ParentTableCode = 'AH' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}'",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals("Result should have rows", 1, result.Rows.Count);
			AssertEquals("ParentID", inv35, (Guid)result.Rows[0][0]);
			AssertEquals("ReportSubCode - Invoice was fully paid in 31 day with inv31 (not reportable) and crd31, crd32 Credit Notes in the same Match Group 190.91 = 350 - 350 * ((100 + 200) / (310 + 350))",
				"190.91|31", (string)result.Rows[0][1]);
		}

		public void TestWhenInvoiceAPAmountIsZero()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("PTR", helper.ToDate("2021-01-01"), helper.ToDate("2021-06-30"));

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			var creditor1PK = helper.InsertOrgHeader("CRD1", "Creditor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = TestDbHelper.DefaultCompanyPK, OB_OH = creditor1PK, OB_IsCreditor = 1 });
			helper.Insert("JobRequiredDocument", new { EQ_PK = Guid.NewGuid(), EQ_DocCategory = "CTR", EQ_DocType = "RSB", EQ_ParentTableCode = "OH", EQ_ParentID = creditor1PK, EQ_DateReceived = helper.ToDate("2021-04-30"), EQ_ValidToDate = helper.ToDate("2021-06-30") });

			// Reportable Invoices for Creditor1
			var inv1 = helper.InsertTransactionHeader("AP", "INV", "APINV001", -350m, helper.ToDate("2021-05-05"), branchPK, departmentPK, org: creditor1PK, fullyPaidDate: helper.ToDate("2021-06-05"));
			var crd1 = helper.InsertTransactionHeader("AP", "CRD", "APCRD001", 200m, helper.ToDate("2021-01-05"), branchPK, departmentPK, org: creditor1PK, fullyPaidDate: helper.ToDate("2021-06-05"));
			helper.InsertTransactionMatchLink(inv1, "M0001", helper.ToDate("2021-06-05"), 0m);
			helper.InsertTransactionMatchLink(crd1, "M0001", helper.ToDate("2020-06-05"), 200m);

			DataTable result = null;
			AssertNoExceptionThrown(() => result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueFullyPaidAPInvoicesForPTRS @ReportPK = '{0}'", reportPK)));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);
		}
	}
}
