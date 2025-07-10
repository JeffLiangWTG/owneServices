using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.Accounting.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(GenerateComplianceReport))]
	class GenerateComplianceReportTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("LIB", helper.ToDate("2015-10-31"), helper.ToDate("2015-11-30"));

			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			QueueForComplianceReport_ARAPCTRTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_ARAPJNLTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_CBTRFEXXTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_DRCDPYTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_EXXOVPDSCTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_GJLRJLTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_INVCRDADJTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_JCJNLTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_JCJRJTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_PAYRECTest.InsertTransactions(helper, branchPK, departmentPK);
			QueueForComplianceReport_WIPACRTest.InsertTransactions(helper, branchPK, departmentPK);

			var openingCategory = "OPN";
			var closingCategory = "CLS";
			DataTable result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueAllForComplianceReport @ReportPK = '{0}', @OpeningCategory = '{1}', @ClosingCategory = '{2}'", reportPK, openingCategory, closingCategory));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");

			AssertEquals("Result should have rows", 138, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC GenerateComplianceReport @ReportPK = '{0}', @ReportGroupBy = 'DAB', @ReportOrderBy = 'LDS', @OpeningCategory = '{1}', @ClosingCategory = '{2}'", reportPK, openingCategory, closingCategory));
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
			AssertEquals("Result should have rows", 138, result.Rows.Count);

			//check generated sequence started with opening journal and ends with closing journal
			AssertEquals("*GL*GJL**", result.Rows[0]["ACL_ReportSubCode"]);
			AssertEquals("*GL*GJL**", result.Rows[137]["ACL_ReportSubCode"]);
			var checkCategoryForOpening = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT AH_TransactionCategory from dbo.AccTransactionLines JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK WHERE AL_PK = '{0}'", result.Rows[0]["ACL_ParentID"]));
			var openingCode = checkCategoryForOpening.Rows[0]["AH_TransactionCategory"];
			var checkCategoryForClosing = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT AH_TransactionCategory from dbo.AccTransactionLines JOIN dbo.AccTransactionHeader ON AL_AH = AH_PK WHERE AL_PK = '{0}'", result.Rows[137]["ACL_ParentID"]));
			var closingCode = checkCategoryForClosing.Rows[0]["AH_TransactionCategory"];
			AssertEquals("Expect opening journal", openingCategory, openingCode);
			AssertEquals("Expect closing journal", closingCategory, closingCode);
		}

		public void TestExcludeOrganisationWhenAH_OHISNULL()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("LIB", helper.ToDate("2015-10-31"), helper.ToDate("2015-11-30"));
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			QueueForComplianceReport_DRCDPYTest.InsertTransactions(helper, branchPK, departmentPK);
			DataUtils.GetDataTableFromQuery(TestConnection, $@"EXEC QueueAllForComplianceReport @ReportPK = '{reportPK}', @OpeningCategory = 'OPN', @ClosingCategory = 'CLS'");

			var transactionHeaders = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader");
			Assert("Percondition", transactionHeaders.Rows.Cast<DataRow>().All(x => x["AH_OH"] == DBNull.Value));

			var qqq = DataUtils.GetDataTableFromQuery(TestConnection, $@"EXEC GenerateComplianceReport @ReportPK = '{reportPK}', @ReportGroupBy = 'DAB', @ReportOrderBy = 'LDS', @OpeningCategory = 'OPN', @ClosingCategory = 'CLS', @ThresholdLevel = 'ORG', @GoodsServiceType = 'SRV', @ExTaxThresholdAmount = 9999999999");
			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
			Assert("Result should have rows", result.Rows.Count > 0);
		}

		public void TestExcludeTransactionOrOrganisation()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("LIB", helper.ToDate("2015-10-31"), helper.ToDate("2015-11-30"));
			var branchPK = helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");
			var org1 = helper.InsertOrgHeader("CRDFIRST", "First OrgHeader");
			var org2 = helper.InsertOrgHeader("CRDSECOND", "Second OrgHeader");
			QueueForComplianceReport_DRCDPYTest.InsertTransactions(helper, branchPK, departmentPK);
			DataUtils.GetDataTableFromQuery(TestConnection, $@"UPDATE dbo.AccTransactionHeader SET AH_OH = '{org1}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_TransactionNum = '001'");
			DataUtils.GetDataTableFromQuery(TestConnection, $@"UPDATE dbo.AccTransactionHeader SET AH_OH = '{org2}', AH_SystemLastEditTimeUtc = GETUTCDATE(), AH_SystemLastEditUser = 'TST' WHERE AH_TransactionNum = '002'");
			DataUtils.GetDataTableFromQuery(TestConnection, $@"EXEC QueueAllForComplianceReport @ReportPK = '{reportPK}', @OpeningCategory = 'OPN', @ClosingCategory = 'CLS'");

			var sql = "EXEC GenerateComplianceReport @ReportPK = '{0}', @ThresholdLevel = '{1}', @ExTaxThresholdAmount = {2}, @ReportGroupBy = 'DAB', @ReportOrderBy = 'LDS', @OpeningCategory = 'OPN', @ClosingCategory = 'CLS', @GoodsServiceType = 'SRV'";

			var thresholdLevel = "HDR";

			var exTaxThresholdAmount = 0;
			AssertResult("Result should contain 2 transaction", 2);

			exTaxThresholdAmount = 101;
			AssertResult("Result should contain 1 transaction", 1);
			var transactionHeaders = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader WHERE AH_PK IN (SELECT ACL_ParentID FROM dbo.AccComplianceReportTransactionPivot)");
			Assert("Should not Contain transaction 001", !transactionHeaders.Rows.Cast<DataRow>().Any(x => (string)x["AH_TransactionNum"] == "001"));
			Assert("Should Contain transaction 002", transactionHeaders.Rows.Cast<DataRow>().Any(x => (string)x["AH_TransactionNum"] == "002"));

			thresholdLevel = "ORG";

			exTaxThresholdAmount = 0;
			AssertResult("Result should contain 2 transaction", 2);

			exTaxThresholdAmount = 501;
			AssertResult("Result should contain 1 transaction", 1);
			transactionHeaders = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionHeader WHERE AH_PK IN (SELECT ACL_ParentID FROM dbo.AccComplianceReportTransactionPivot)");
			Assert("Should not Contain transaction 001", !transactionHeaders.Rows.Cast<DataRow>().Any(x => (string)x["AH_TransactionNum"] == "001"));
			Assert("Should Contain transaction 002", transactionHeaders.Rows.Cast<DataRow>().Any(x => (string)x["AH_TransactionNum"] == "002"));

			void AssertResult(string messgae, int transactionCount)
			{
				DataUtils.GetDataTableFromQuery(TestConnection, "Delete FROM dbo.AccComplianceReportTransactionPivot");
				var qqq = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sql, reportPK, thresholdLevel , exTaxThresholdAmount));
				var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
				AssertEquals(messgae, transactionCount, result.Rows.Cast<DataRow>().Where(x => (string)x["ACL_ParentTableCode"] == "AH").Select(x => x["ACL_ParentID"]).Distinct().Count());
			}
		}

		[ExpectNoExceptions]
		public void TestSampleCall_TransactionPayments()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("LIB", helper.ToDate("2020-05-01"), helper.ToDate("2020-05-31"));

			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.DefaultBranchPK;
			var departmentPK = helper.DefaultDepartmentPK;

			var creditor1 = helper.InsertOrgHeader("CRDFIRST", "First Creditor");
			var creditor2 = helper.InsertOrgHeader("CRDSECOND", "Second Creditor");

			var inv11 = helper.InsertTransactionHeader("AP", "INV", "INV1001", 110, helper.ToDate("2020-05-20"), branchPK, departmentPK, org: creditor1);
			helper.InsertComplianceReportQueue("LIB", inv11, "AH", helper.ToDate("2020-05-21")); // The date on queue record represents date of the first MatchLink in the Report period
			var inv12 = helper.InsertTransactionHeader("AP", "INV", "INV1002", 120, helper.ToDate("2020-05-19"), branchPK, departmentPK, org: creditor1);
			helper.InsertComplianceReportQueue("LIB", inv12, "AH", helper.ToDate("2020-05-20"));
			var inv13 = helper.InsertTransactionHeader("AP", "INV", "INV1003", 120, helper.ToDate("2020-05-25"), branchPK, departmentPK, org: creditor1);
			helper.InsertComplianceReportQueue("LIB", inv13, "AH", helper.ToDate("2020-05-25"));

			var inv21 = helper.InsertTransactionHeader("AP", "INV", "INV2001", 110, helper.ToDate("2020-05-18"), branchPK, departmentPK, org: creditor2);
			helper.InsertComplianceReportQueue("LIB", inv21, "AH", helper.ToDate("2020-05-20"));
			var inv22 = helper.InsertTransactionHeader("AP", "INV", "INV2002", 120, helper.ToDate("2020-05-19"), branchPK, departmentPK, org: creditor2);
			helper.InsertComplianceReportQueue("LIB", inv22, "AH", helper.ToDate("2020-05-21"));

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Should have added queue records", 5, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC GenerateComplianceReport @ReportPK = '{reportPK}', @ReportGroupBy = 'TPA', @ReportOrderBy = 'ORG'");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
			AssertEquals("Result should have rows", 5, result.Rows.Count);

			Assert("ACL_GC_Company", result.Rows.Cast<DataRow>().All(x => companyPK == (Guid)x["ACL_GC_Company"]));
			Assert("ACL_ACR_Report", result.Rows.Cast<DataRow>().All(x => reportPK == (Guid)x["ACL_ACR_Report"]));
			Assert("ACL_ParentTableCode", result.Rows.Cast<DataRow>().All(x => "AH" == (string)x["ACL_ParentTableCode"]));
			// Pivots shoud be ordered by Org Code and Queue record Date
			AssertEquals("Pivot 0", inv12, (Guid)result.Rows[0]["ACL_ParentID"]);
			AssertEquals("Pivot 1", inv11, (Guid)result.Rows[1]["ACL_ParentID"]);
			AssertEquals("Pivot 2", inv13, (Guid)result.Rows[2]["ACL_ParentID"]);
			AssertEquals("Pivot 3", inv21, (Guid)result.Rows[3]["ACL_ParentID"]);
			AssertEquals("Pivot 4", inv22, (Guid)result.Rows[4]["ACL_ParentID"]);
		}

		[ExpectNoExceptions]
		public void TestSampleCall_TypeUVA()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("UVA", helper.ToDate("2021-05-01"), helper.ToDate("2021-05-31"));

			var companyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.DefaultBranchPK;
			var departmentPK = helper.DefaultDepartmentPK;

			var creditor1 = helper.InsertOrgHeader("CRD1", "First Creditor");
			var debtor1 = helper.InsertOrgHeader("DEB1", "First Debtor");

			// AP
			var inv11 = helper.InsertTransactionHeader("AP", "INV", "INV1001", -300m, helper.ToDate("2021-05-20"), branchPK, departmentPK, org: creditor1);
			var inv11l1 = helper.InsertTransactionLine(inv11, postDate: helper.ToDate("2021-05-20"), lineAmount: -300m, taxAmount: -57m, companyPK: companyPK, branchPK: branchPK);
			helper.InsertComplianceReportQueue("UVA", inv11l1, "AL", helper.ToDate("2021-05-20"), null, null, "NA_NA_66");

			var inv12 = helper.InsertTransactionHeader("AP", "INV", "INV1002", -119m, helper.ToDate("2021-05-19"), branchPK, departmentPK, org: creditor1);
			var inv12l1 = helper.InsertTransactionLine(inv12, postDate: helper.ToDate("2021-05-19"), lineAmount: -100m, taxAmount: -19m, companyPK: companyPK, branchPK: branchPK);
			helper.InsertComplianceReportQueue("UVA", inv12l1, "AL", helper.ToDate("2021-05-19"), null, null, "NA_NA_66");

			// AR
			var inv21 = helper.InsertTransactionHeader("AR", "INV", "INV2001", 357, helper.ToDate("2021-05-17"), branchPK, departmentPK, org: debtor1);
			var inv21l1 = helper.InsertTransactionLine(inv21, postDate: helper.ToDate("2021-05-17"), lineAmount: 300m, taxAmount: 57m, companyPK: companyPK, branchPK: branchPK);
			helper.InsertComplianceReportQueue("UVA", inv21l1, "AL", helper.ToDate("2021-05-17"), null, null, "81_NA_NA");
			var inv21l2 = helper.InsertTransactionLine(inv21, postDate: helper.ToDate("2021-05-17"), lineAmount: 100m, taxAmount: 19m, companyPK: companyPK, branchPK: branchPK);
			helper.InsertComplianceReportQueue("UVA", inv21l2, "AL", helper.ToDate("2021-05-17"), null, null, "86_NA_NA");

			var inv22 = helper.InsertTransactionHeader("AR", "INV", "INV2003", 100, helper.ToDate("2021-05-15"), branchPK, departmentPK, org: debtor1);
			var inv22l1 = helper.InsertTransactionLine(inv22, postDate: helper.ToDate("2021-05-15"), lineAmount: 100m, taxAmount: 0, companyPK: companyPK, branchPK: branchPK);
			helper.InsertComplianceReportQueue("UVA", inv22l1, "AL", helper.ToDate("2021-05-15"), null, null, "46_47_67");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Should have added queue records", 5, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC GenerateComplianceReport @ReportPK = '{reportPK}', @ReportGroupBy = 'HRS'");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
			AssertEquals("Result should have rows", 5, result.Rows.Count);

			Assert("ACL_GC_Company", result.Rows.Cast<DataRow>().All(x => companyPK == (Guid)x["ACL_GC_Company"]));
			Assert("ACL_ACR_Report", result.Rows.Cast<DataRow>().All(x => reportPK == (Guid)x["ACL_ACR_Report"]));
			Assert("ACL_ParentTableCode", result.Rows.Cast<DataRow>().All(x => "AL" == (string)x["ACL_ParentTableCode"]));

			// Pivots should be ordered by Ledger, Transaction Header, Posting Date and Report Sub Code
			AssertEquals("Pivot 0", inv12l1, (Guid)result.Rows[0]["ACL_ParentID"]);
			AssertEquals("Pivot 0 - ReportSubCode", "NA_NA_66", (string)result.Rows[0]["ACL_reportSubCode"]);
			AssertEquals("Pivot 0 - ReportSequence", 1, (int)result.Rows[0]["ACL_ReportSequence"]);

			AssertEquals("Pivot 1", inv11l1, (Guid)result.Rows[1]["ACL_ParentID"]);
			AssertEquals("Pivot 1 - ReportSubCode", "NA_NA_66", (string)result.Rows[1]["ACL_reportSubCode"]);
			AssertEquals("Pivot 1 - ReportSequence", 2, (int)result.Rows[1]["ACL_ReportSequence"]);

			AssertEquals("Pivot 2", inv22l1, (Guid)result.Rows[2]["ACL_ParentID"]);
			AssertEquals("Pivot 2 - ReportSubCode", "46_47_67", (string)result.Rows[2]["ACL_reportSubCode"]);
			AssertEquals("Pivot 2 - ReportSequence", 3, (int)result.Rows[2]["ACL_ReportSequence"]);

			AssertEquals("Pivot 3", inv21l1, (Guid)result.Rows[3]["ACL_ParentID"]);
			AssertEquals("Pivot 3 - ReportSubCode", "81_NA_NA", (string)result.Rows[3]["ACL_reportSubCode"]);
			AssertEquals("Pivot 3 - ReportSequence", 4, (int)result.Rows[3]["ACL_ReportSequence"]);

			AssertEquals("Pivot 4", inv21l2, (Guid)result.Rows[4]["ACL_ParentID"]);
			AssertEquals("Pivot 4 - ReportSubCode", "86_NA_NA", (string)result.Rows[4]["ACL_reportSubCode"]);
			AssertEquals("Pivot 4 - ReportSequence", 5, (int)result.Rows[4]["ACL_ReportSequence"]);
		}

		[ExpectNoExceptions]
		public void TestSampleCall_TypeUST111WithoutPEoT()
		{
			var helper = new TestDbHelper(TestConnection);
			var helperData = new TestDbHelperData(helper).SetupData();
			var helperDataPKs = helperData.dbHelperDataPKs;

			// USTVA's posted with 1 month delay
			var cbDpy1L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202101", "2021-02-28", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy2L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202102", "2021-03-31", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy3L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202103", "2021-04-30", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy4L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202104", "2021-05-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy5L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202105", "2021-06-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy6L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202106", "2021-07-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy7L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202107", "2021-08-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy8L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202108", "2021-09-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy9L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202109", "2021-10-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy10L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202110", "2021-11-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDpy11L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202111", "2021-12-15", "EUR", "U11", -1, "USTVA", 100);
			var cbDrc12L1 = helperData.CreateACashbookQueueEntry("DRC", "USTVA-202112", "2022-01-15", "EUR", "U11", -1, "USTVA", -100);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Should have added 12 queue records", 12, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC GenerateComplianceReport @ReportPK = '{helperDataPKs.ReportPK}', @ReportGroupBy = 'RSH'");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
			AssertEquals("Result should have 12 rows", 12, result.Rows.Count);

			Assert("ACL_GC_Company", result.Rows.Cast<DataRow>().All(x => helperDataPKs.DefaultCompanyPK == (Guid)x["ACL_GC_Company"]));
			Assert("ACL_ACR_Report", result.Rows.Cast<DataRow>().All(x => helperDataPKs.ReportPK == (Guid)x["ACL_ACR_Report"]));
			Assert("ACL_ParentTableCode", result.Rows.Cast<DataRow>().All(x => "AL" == (string)x["ACL_ParentTableCode"]));

			var rows = result.Select("");
			var expected = new[] { ("USTVA - 202101", 1, cbDpy1L1), ("USTVA - 202102", 2, cbDpy2L1), ("USTVA - 202103", 3, cbDpy3L1), ("USTVA - 202104", 4, cbDpy4L1), ("USTVA - 202105", 5, cbDpy5L1), ("USTVA - 202106", 6, cbDpy6L1),
									("USTVA - 202107", 7, cbDpy7L1), ("USTVA - 202108", 8, cbDpy8L1), ("USTVA - 202109", 9, cbDpy9L1), ("USTVA - 202110", 10, cbDpy10L1), ("USTVA - 202111", 11, cbDpy11L1), ("USTVA - 202112", 12, cbDrc12L1) };
			var actual = rows.Select(r => ((string)r["ACL_ReportSubCode"], (int)r["ACL_ReportSequence"], (Guid)r["ACL_ParentID"]));
			AssertContainsExactElementsInExactOrder("Pivots should be ordered by Report Sub Code, Ledger, Sequence Date and Transaction Header", expected, actual);
		}

		[ExpectNoExceptions]
		public void TestSampleCall_TypeUST111WithPEoT()
		{
			var helper = new TestDbHelper(TestConnection);
			var helperData = new TestDbHelperData(helper).SetupData();
			var helperDataPKs = helperData.dbHelperDataPKs;

			var cbDpyU11 = helper.InsertTransactionHeader("CB", "DPY", "UST111-2021", -100m, helper.ToDate("2021-02-28"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpyU11L1 = helper.InsertTransactionLine(cbDpyU11, null, null, helperDataPKs.Ust111Account, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-02-28"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			helper.InsertComplianceReportQueue("U11", cbDpyU11L1, "AL", helper.ToDate("2021-02-28"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "UST 1/11 - 202102");
			// USTVA's posted with 2 months delay
			var cbDpy2L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202101", "2021-03-31", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy3L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202102", "2021-04-30", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy4L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202103", "2021-05-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy5L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202104", "2021-06-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy6L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202105", "2021-07-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy7L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202106", "2021-08-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy8L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202107", "2021-09-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy9L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202108", "2021-10-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy10L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202109", "2021-11-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDpy11L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202110", "2021-12-15", "EUR", "U11", -2, "USTVA", 100);
			var cbDrc12L1 = helperData.CreateACashbookQueueEntry("DPY", "USTVA-202111", "2022-01-15", "EUR", "U11", -2, "USTVA", 100);

			// USTVA for December 2021 is not yet booked as CB - DPY, the data has to be determined from transaction lines in the same way as for the USTVA December 2021
			var tvp = helperData.CreateTVP(helper);
			var tmiRows = tvp.Select("");

			var apInv1 = helper.InsertTransactionHeader("AP", "INV", "APINV0011", -357m, helper.ToDate("2021-12-12"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Creditor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var apInv1L1 = helper.InsertTransactionLine(apInv1, taxMessageIdPK: tmiRows[3].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-12"), lineAmount: 300m, taxAmount: 57m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			helper.InsertComplianceReportQueue("U11", apInv1L1, "AL", helper.ToDate("2021-12-12"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			var apInv2 = helper.InsertTransactionHeader("AP", "CRD", "APINV0021", 107m, helper.ToDate("2021-12-04"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Creditor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var apInv2L1 = helper.InsertTransactionLine(apInv2, taxMessageIdPK: tmiRows[5].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-04"), lineAmount: -100m, taxAmount: -7m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			helper.InsertComplianceReportQueue("U11", apInv2L1, "AL", helper.ToDate("2021-12-04"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			var arInv1 = helper.InsertTransactionHeader("AR", "INV", "ARINV0012", 238m, helper.ToDate("2021-12-11"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Debitor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var arInv1L1 = helper.InsertTransactionLine(arInv1, taxMessageIdPK: tmiRows[0].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			var arInv1L2 = helper.InsertTransactionLine(arInv1, taxMessageIdPK: tmiRows[0].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			helper.InsertComplianceReportQueue("U11", arInv1L1, "AL", helper.ToDate("2021-12-11"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			helper.InsertComplianceReportQueue("U11", arInv1L2, "AL", helper.ToDate("2021-12-11"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			var arInv2 = helper.InsertTransactionHeader("AR", "CRD", "ARCRD0014", -100m, helper.ToDate("2021-12-13"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Debitor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var arInv2L1 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: tmiRows[2].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-13"), lineAmount: 100m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			helper.InsertComplianceReportQueue("U11", arInv2L1, "AL", helper.ToDate("2021-12-13"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			var cbDrc14 = helper.InsertTransactionHeader("CB", "DRC", "CBDRC00114", 119m, helper.ToDate("2021-12-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDrc14L1 = helper.InsertTransactionLine(cbDrc14, null, null, helperDataPKs.GlPnLAccount, helperDataPKs.BranchPK, null, null, -100m, "DRC", postDate: helper.ToDate("2021-12-15"), null, -19m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, taxMessageIdPK: tmiRows[0].Field<Guid>("Guid"));
			helper.InsertComplianceReportQueue("U11", cbDrc14L1, "AL", helper.ToDate("2021-12-15"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			var cbDpy15 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00115", -238m, helper.ToDate("2021-12-16"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy15L1 = helper.InsertTransactionLine(cbDpy15, null, null, helperDataPKs.GlPnLAccount, helperDataPKs.BranchPK, null, null, 200m, "DPY", postDate: helper.ToDate("2021-12-16"), null, 38m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, taxMessageIdPK: tmiRows[3].Field<Guid>("Guid"));
			helper.InsertComplianceReportQueue("U11", cbDpy15L1, "AL", helper.ToDate("2021-12-16"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");
			var cbDpy16 = helper.InsertTransactionHeader("CB", "DRC", "CBDPY00116", 300m, helper.ToDate("2021-12-16"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy16L1 = helper.InsertTransactionLine(cbDpy16, null, null, helperDataPKs.GlPnLAccount, helperDataPKs.BranchPK, null, null, -300m, "DRC", postDate: helper.ToDate("2021-12-16"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, taxMessageIdPK: null);
			helper.InsertComplianceReportQueue("U11", cbDpy16L1, "AL", helper.ToDate("2021-12-16"), helperDataPKs.DefaultCompanyPK, helperDataPKs.BranchPK, "USTVA - 202112");

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Should have added 20 queue records", 20, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, $"EXEC GenerateComplianceReport @ReportPK = '{helperDataPKs.ReportPK}', @ReportGroupBy = 'RSH'");
			AssertEquals("Result should not have rows", 0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccComplianceReportTransactionPivot ORDER BY ACL_ReportSequence");
			AssertEquals("Result should have 20 rows", 20, result.Rows.Count);

			Assert("ACL_GC_Company", result.Rows.Cast<DataRow>().All(x => helperDataPKs.DefaultCompanyPK == (Guid)x["ACL_GC_Company"]));
			Assert("ACL_ACR_Report", result.Rows.Cast<DataRow>().All(x => helperDataPKs.ReportPK == (Guid)x["ACL_ACR_Report"]));
			Assert("ACL_ParentTableCode", result.Rows.Cast<DataRow>().All(x => "AL" == (string)x["ACL_ParentTableCode"]));

			var rows = result.Select("");
			var expected = new[] { ("UST 1/11 - 202102", 1, cbDpyU11L1),
									("USTVA - 202101", 2, cbDpy2L1), ("USTVA - 202102", 3, cbDpy3L1), ("USTVA - 202103", 4, cbDpy4L1), ("USTVA - 202104", 5, cbDpy5L1), ("USTVA - 202105", 6, cbDpy6L1),
									("USTVA - 202106", 7, cbDpy7L1), ("USTVA - 202107", 8, cbDpy8L1), ("USTVA - 202108", 9, cbDpy9L1), ("USTVA - 202109", 10, cbDpy10L1), ("USTVA - 202110", 11, cbDpy11L1), ("USTVA - 202111", 12, cbDrc12L1),
									("USTVA - 202112", 13, apInv2L1), ("USTVA - 202112", 14, apInv1L1), ("USTVA - 202112", 15, arInv1L1), ("USTVA - 202112", 15, arInv1L2), ("USTVA - 202112", 16, arInv2L1),
									("USTVA - 202112", 17, cbDrc14L1), ("USTVA - 202112", 18, cbDpy15L1), ("USTVA - 202112", 19, cbDpy16L1) };
			var actual = rows.Select(r => ((string)r["ACL_ReportSubCode"], (int)r["ACL_ReportSequence"], (Guid)r["ACL_ParentID"]));
			AssertContainsExactElementsInExactOrder("Pivots should be ordered by Report Sub Code, Ledger, Sequence Date and Transaction Header", expected, actual);
		}
	}
}
