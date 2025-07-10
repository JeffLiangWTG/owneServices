using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueTransactionsForUSTVAReport))]
	class QueueTransactionsForUSTVAReportTestTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCall()
		{
			var helper = new TestDbHelper(TestConnection);
			var reportPK = helper.InsertComplianceReport("UVA", helper.ToDate("2021-11-01"), helper.ToDate("2021-11-30"));

			var defaultCompanyPK = TestDbHelper.DefaultCompanyPK;
			var branchPK = helper.InsertBranch("ZZB", defaultCompanyPK);
			var departmentPK = helper.InsertDepartment("ZZD");

			var creditor1PK = helper.InsertOrgHeader("CRD1", "Creditor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = defaultCompanyPK, OB_OH = creditor1PK, OB_IsCreditor = 1 });

			var creditor2PK = helper.InsertOrgHeader("CRD2", "Creditor2");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = defaultCompanyPK, OB_OH = creditor2PK, OB_IsCreditor = 1 });

			var debitor1PK = helper.InsertOrgHeader("DEB1", "Debitor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = defaultCompanyPK, OB_OH = debitor1PK, OB_IsCreditor = 0 });

			var debitor2PK = helper.InsertOrgHeader("DEB2", "Debitor2");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = defaultCompanyPK, OB_OH = debitor2PK, OB_IsCreditor = 0 });

			var glPnLAccount = helper.InsertGLAccount("1234.00.10", "BL P&L Account 1");
			var glAccount = helper.InsertGLAccount("1234.56.78", "Bank Account 1", "BSH");
			var bankAccount = helper.InsertBankAccount("BANK1", glAccount);
			var ustvaAccount = helper.InsertGLAccount("1234.11.00", "USTVA Account");

			var taxMsgId1 = helper.InsertInvMsg("IM1");        // MST    19% for AR			-> 81_na_na
			var taxMsgId2 = helper.InsertInvMsg("IM2");        // LOWMST  7% for AR			-> 86_na_na
			var taxMsgId3 = helper.InsertInvMsg("IM3");        // MSTREV  0% for AR			-> 49_na_na
			var taxMsgId4 = helper.InsertInvMsg("IM4");        // MST	19% for AP			-> na_na_66
			var taxMsgId5 = helper.InsertInvMsg("IM5");        // NOTREPORT for AR and AP	-> na_na_na
			var taxMsgId6 = helper.InsertInvMsg("IM6");        // LOWMST  7% for AP			-> na_na_66
			var taxMsgId7 = helper.InsertInvMsg("IM7");        // MSTREV  0% for AP			-> 46_47_67

			// AP - INVs
			var apInv1 = helper.InsertTransactionHeader("AP", "INV", "APINV0011", -357m, helper.ToDate("2021-12-11"), branchPK, departmentPK, org: creditor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			helper.InsertTransactionLine(apInv1, taxMessageIdPK: taxMsgId4, postDate: helper.ToDate("2021-12-11"), lineAmount: 300m, taxAmount: 57m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var apInv2 = helper.InsertTransactionHeader("AP", "INV", "APINV0021", -321m, helper.ToDate("2021-11-04"), branchPK, departmentPK, org: creditor2PK, currency: "EUR", companyPK: defaultCompanyPK);
			var apInv2L1 = helper.InsertTransactionLine(apInv2, taxMessageIdPK: taxMsgId6, postDate: helper.ToDate("2021-11-04"), lineAmount: 100m, taxAmount: 7m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var apInv2L2 = helper.InsertTransactionLine(apInv2, taxMessageIdPK: taxMsgId6, postDate: helper.ToDate("2021-11-04"), lineAmount: 200m, taxAmount: 14m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var apInv3 = helper.InsertTransactionHeader("AP", "INV", "APINV0031", -119m, helper.ToDate("2021-11-04"), branchPK, departmentPK, org: creditor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var apInv3L1 = helper.InsertTransactionLine(apInv3, taxMessageIdPK: taxMsgId4, postDate: helper.ToDate("2021-11-04"), lineAmount: 100m, taxAmount: 19m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var apInv4 = helper.InsertTransactionHeader("AP", "INV", "APINV0041", -150m, helper.ToDate("2021-11-06"), branchPK, departmentPK, org: creditor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var apInv4L1 = helper.InsertTransactionLine(apInv4, taxMessageIdPK: taxMsgId7, postDate: helper.ToDate("2021-11-06"), lineAmount: 150m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var apInv5 = helper.InsertTransactionHeader("AP", "INV", "APINV0051", -150m, helper.ToDate("2021-11-07"), branchPK, departmentPK, org: creditor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			helper.InsertTransactionLine(apInv5, taxMessageIdPK: taxMsgId5, postDate: helper.ToDate("2021-11-07"), lineAmount: 150m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			// AP - CRDs
			var apCrd1 = helper.InsertTransactionHeader("AP", "CRD", "APCRD0011", 59.5m, helper.ToDate("2021-11-04"), branchPK, departmentPK, org: creditor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var apCrd1L1 = helper.InsertTransactionLine(apCrd1, taxMessageIdPK: taxMsgId4, postDate: helper.ToDate("2021-11-04"), lineAmount: -50m, taxAmount: -9.5m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);

			// AR - INVs
			var arInv1 = helper.InsertTransactionHeader("AR", "INV", "ARINV0011", 357m, helper.ToDate("2021-11-10"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var arInv1L1 = helper.InsertTransactionLine(arInv1, taxMessageIdPK: taxMsgId1, postDate: helper.ToDate("2021-11-10"), lineAmount: -300m, taxAmount: -57m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv2 = helper.InsertTransactionHeader("AR", "INV", "ARINV0012", 464m, helper.ToDate("2021-11-11"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var arInv2L1 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: taxMsgId1, postDate: helper.ToDate("2021-11-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv2L2 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: taxMsgId1, postDate: helper.ToDate("2021-11-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv2L3 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: taxMsgId2, postDate: helper.ToDate("2021-11-11"), lineAmount: -100m, taxAmount: -7m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv2L4 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: taxMsgId1, postDate: helper.ToDate("2021-11-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv3 = helper.InsertTransactionHeader("AR", "INV", "ARINV0013", 238m, helper.ToDate("2021-12-12"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			helper.InsertTransactionLine(arInv3, taxMessageIdPK: taxMsgId1, postDate: helper.ToDate("2021-12-12"), lineAmount: -200m, taxAmount: -38m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv4 = helper.InsertTransactionHeader("AR", "INV", "ARINV0014", 200m, helper.ToDate("2021-11-13"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var arInv4L1 = helper.InsertTransactionLine(arInv4, taxMessageIdPK: taxMsgId3, postDate: helper.ToDate("2021-11-13"), lineAmount: -100m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv4L2 = helper.InsertTransactionLine(arInv4, taxMessageIdPK: taxMsgId3, postDate: helper.ToDate("2021-11-13"), lineAmount: -100m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv5 = helper.InsertTransactionHeader("AR", "INV", "ARINV0015", 200m, helper.ToDate("2021-11-13"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var arInv5L1 = helper.InsertTransactionLine(arInv5, taxMessageIdPK: taxMsgId3, postDate: helper.ToDate("2021-11-13"), lineAmount: -200m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arInv6 = helper.InsertTransactionHeader("AR", "INV", "ARINV0016", 200m, helper.ToDate("2021-11-15"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			helper.InsertTransactionLine(arInv6, taxMessageIdPK: taxMsgId5, postDate: helper.ToDate("2021-11-15"), lineAmount: -200m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			// AR - CRD
			var arCrd1 = helper.InsertTransactionHeader("AR", "CRD", "ARCRD0011", -119m, helper.ToDate("2021-11-11"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var arCrd1L1 = helper.InsertTransactionLine(arCrd1, taxMessageIdPK: taxMsgId1, postDate: helper.ToDate("2021-11-11"), lineAmount: 100m, taxAmount: 19m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);
			var arCrd2 = helper.InsertTransactionHeader("AR", "CRD", "ARCRD0012", -107m, helper.ToDate("2021-11-17"), branchPK, departmentPK, org: debitor1PK, currency: "EUR", companyPK: defaultCompanyPK);
			var arCrd2L1 = helper.InsertTransactionLine(arCrd2, taxMessageIdPK: taxMsgId2, postDate: helper.ToDate("2021-11-17"), lineAmount: 100m, taxAmount: 7m, transactionCurrency: "EUR", companyPK: defaultCompanyPK, branchPK: branchPK);

			// CB - DRCs
			var cbDrc1 = helper.InsertTransactionHeader("CB", "DRC", "CBDRC0011", 119m, helper.ToDate("2021-11-11"), bankAccountPK: bankAccount, companyPK: defaultCompanyPK, currency: "EUR", branchPK: branchPK);
			var cbDrc1L1 = helper.InsertTransactionLine(cbDrc1, null, null, glPnLAccount, branchPK, null, null, -100m, "DRC", postDate: helper.ToDate("2021-11-11"), null, -19m, taxMessageIdPK: taxMsgId1, transactionCurrency: "EUR", companyPK: defaultCompanyPK);
			var cbDrc2 = helper.InsertTransactionHeader("CB", "DRC", "CBDRC0012", 107m, helper.ToDate("2021-11-17"), bankAccountPK: bankAccount, companyPK: defaultCompanyPK, currency: "EUR", branchPK: branchPK);
			var cbDrc2L1 = helper.InsertTransactionLine(cbDrc2, null, null, glPnLAccount, branchPK, null, null, -100m, "DRC", postDate: helper.ToDate("2021-11-17"), null, -7m, taxMessageIdPK: taxMsgId2, transactionCurrency: "EUR", companyPK: defaultCompanyPK);
			// CB - DPYs
			var cbDpy1 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0011", -119m, helper.ToDate("2021-11-04"), bankAccountPK: bankAccount, companyPK: defaultCompanyPK, currency: "EUR", branchPK: branchPK);
			var cbDpy1L1 = helper.InsertTransactionLine(cbDpy1, null, null, glPnLAccount, branchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-11-04"), null, 19m, taxMessageIdPK: taxMsgId4, transactionCurrency: "EUR", companyPK: defaultCompanyPK);
			// CB - DPY for USTVA posted in November 2021, it has no tax message ID specified
			var cbDpy2 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0012", -100m, helper.ToDate("2021-11-15"), bankAccountPK: bankAccount, companyPK: defaultCompanyPK, currency: "EUR", branchPK: branchPK);
			helper.InsertTransactionLine(cbDpy2, null, null, ustvaAccount, branchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-11-15"), null, 0m, transactionCurrency: "EUR", companyPK: defaultCompanyPK);

			// @TVP
			var tvpList = new List<Tuple<string, Guid>>() {
				new Tuple<string, Guid>("81_NA_NA", taxMsgId1),
				new Tuple<string, Guid>("86_NA_NA", taxMsgId2),
				new Tuple<string, Guid>("49_NA_NA", taxMsgId3),
				new Tuple<string, Guid>("NA_NA_66", taxMsgId4),
				new Tuple<string, Guid>("NA_NA_66", taxMsgId6),
				new Tuple<string, Guid>("46_47_67", taxMsgId7)
			};

			var tvp = new DataTable();
			tvp.Locale = CultureInfo.InvariantCulture;
			tvp.Columns.Add("Code", typeof(string)); // Part of SQL code
			tvp.Columns.Add("Guid", typeof(Guid)); // Part of SQL code
			DataRow row;
			foreach (Tuple<string, Guid> t in tvpList)
			{
				row = tvp.NewRow();
				row["Code"] = t.Item1;
				row["Guid"] = t.Item2;
				tvp.Rows.Add(row);
			}

			//var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("EXEC QueueTransactionsForUSTVAReport @ReportPK, @TVP"));
			DataTable result;
			using (var command = Db.Connection.Command("EXEC QueueTransactionsForUSTVAReport @ReportPK, @TVP"))
			{
				command.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, reportPK);
				command.AddTableValuedParameter("@TVP", "dbo.TVP_CodeToGuidMapping", tvp);
				result = DataUtils.GetDataTableFromCommand(command);
				AssertEquals("Result should not have rows", 0, result.Rows.Count);
			}

			result = DataUtils.GetDataTableFromQuery(TestConnection, "SELECT * FROM dbo.AccTransactionComplianceReportQueue");
			AssertEquals("Result should have eighteen rows", 18, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue  WHERE ACQ_Date = 'Nov 11 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertSelectRow(result.Select($"ACQ_ParentID = '{arInv2L1}'"), 1, "81_NA_NA");
			AssertSelectRow(result.Select($"ACQ_ParentID = '{arInv2L2}'"), 1, "81_NA_NA");
			AssertSelectRow(result.Select($"ACQ_ParentID = '{arInv2L3}'"), 1, "86_NA_NA");
			AssertSelectRow(result.Select($"ACQ_ParentID = '{arInv2L4}'"), 1, "81_NA_NA");
			AssertSelectRow(result.Select($"ACQ_ParentID = '{arCrd1L1}'"), 1, "81_NA_NA");
			AssertSelectRow(result.Select($"ACQ_ParentID = '{cbDrc1L1}'"), 1, "81_NA_NA");

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = 'Nov 04 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertSelectRow(result.Select(""), 5, "NA_NA_66");
			AssertEquals(1, result.Select($"ACQ_ParentID = '{apInv2L1}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{apInv2L2}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{apInv3L1}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{apCrd1L1}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{cbDpy1L1}'").Length);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = 'Nov 13 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertSelectRow(result.Select(""), 3, "49_NA_NA");
			AssertEquals(1, result.Select($"ACQ_ParentID = '{arInv4L1}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{arInv4L2}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{arInv5L1}'").Length);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = 'Nov 06 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertSelectRow(result.Select(""), 1, "46_47_67");
			AssertEquals(1, result.Select($"ACQ_ParentID = '{apInv4L1}'").Length);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = 'Nov 10 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertSelectRow(result.Select(""), 1, "81_NA_NA");
			AssertEquals(1, result.Select($"ACQ_ParentID = '{arInv1L1}'").Length);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = 'Nov 15 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertEquals(0, result.Rows.Count);

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_Date = 'Nov 17 2021' AND ACQ_ReportType = 'UVA' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by 2",
				TestDbHelper.DefaultCompanyPK, branchPK));
			AssertSelectRow(result.Select(""), 2, "86_NA_NA");
			AssertEquals(1, result.Select($"ACQ_ParentID = '{arCrd2L1}'").Length);
			AssertEquals(1, result.Select($"ACQ_ParentID = '{cbDrc2L1}'").Length);

			void AssertSelectRow(DataRow[] selectRows, int noOfRows, string reportSubCode)
			{
				AssertEquals($"Should find {noOfRows} row(s) that match(es) criteria", noOfRows, selectRows.Length);
				AssertEquals($"ReportSubCode - Reverse Charge", reportSubCode, selectRows[0]["ACQ_ReportSubCode"]);
			}
		}
	}
}

