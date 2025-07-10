using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting.ComplianceReport;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.Build.Database.Script.Testing.Public.Accounting.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting.ComplianceReport
{
	[TestedType(typeof(QueueTransactionsForUST111Report))]
	class QueueTransactionsForUST111ReportTestTest : DbCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestSampleCallWithoutPEoT()
		{
			var helper = new TestDbHelper(TestConnection);
			var helperData = new TestDbHelperData(helper).SetupData();
			var helperDataPKs = helperData.dbHelperDataPKs;

			// CB - DPY for UST 1/11, in 2020, test edge case of posting date
			var cbDpy = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0000", -100m, helper.ToDate("2020-12-31"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			helper.InsertTransactionLine(cbDpy, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2020-12-31"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			// CB - DPY for USTVA December 2020
			var cbDpy0 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0010", -100m, helper.ToDate("2021-01-31"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			helper.InsertTransactionLine(cbDpy0, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-01-31"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			// CB - DPYs for USTVA for January to December posted with 1 month delay
			var cbDpy1 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0011", -100m, helper.ToDate("2021-02-28"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy1L1 = helper.InsertTransactionLine(cbDpy1, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-02-28"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy2 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0012", -100m, helper.ToDate("2021-03-31"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy2L1 = helper.InsertTransactionLine(cbDpy2, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-03-31"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy3 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0013", -100m, helper.ToDate("2021-04-30"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy3L1 = helper.InsertTransactionLine(cbDpy3, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-04-30"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy4 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0014", -100m, helper.ToDate("2021-05-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy4L1 = helper.InsertTransactionLine(cbDpy4, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-05-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy5 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0015", -100m, helper.ToDate("2021-06-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy5L1 = helper.InsertTransactionLine(cbDpy5, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-06-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy6 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0016", -100m, helper.ToDate("2021-07-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy6L1 = helper.InsertTransactionLine(cbDpy6, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-07-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy7 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0017", -100m, helper.ToDate("2021-08-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy7L1 = helper.InsertTransactionLine(cbDpy7, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-08-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy8 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0018", -100m, helper.ToDate("2021-09-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy8L1 = helper.InsertTransactionLine(cbDpy8, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-09-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy9 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0019", -100m, helper.ToDate("2021-10-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy9L1 = helper.InsertTransactionLine(cbDpy9, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-10-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy10 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00110", -100m, helper.ToDate("2021-11-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy10L1 = helper.InsertTransactionLine(cbDpy10, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-11-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy11 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00111", -100m, helper.ToDate("2021-12-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy11L1 = helper.InsertTransactionLine(cbDpy11, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-12-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDrc12 = helper.InsertTransactionHeader("CB", "DRC", "CBDRC00112", 100m, helper.ToDate("2022-01-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDrc12L1 = helper.InsertTransactionLine(cbDrc12, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, -100m, "DRC", postDate: helper.ToDate("2022-01-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy13 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00113", -100m, helper.ToDate("2022-02-01"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			helper.InsertTransactionLine(cbDpy13, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2022-02-01"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);

			var tvp = helperData.CreateTVP(helper);

			DataTable result;
			using (var command = Db.Connection.Command("EXEC QueueTransactionsForUST111Report @ReportPK, @PKSpecialVATPrepaymentAccount, @PKAdvancedTurnoverTaxReturnAccount, @TVP"))
			{
				command.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, helperDataPKs.ReportPK);
				command.AddParameter("@PKSpecialVATPrepaymentAccount", SqlDbType.UniqueIdentifier, helperDataPKs.Ust111Account);
				command.AddParameter("@PKAdvancedTurnoverTaxReturnAccount", SqlDbType.UniqueIdentifier, helperDataPKs.UstvaAccount);
				command.AddTableValuedParameter("@TVP", "dbo.TVP_CodeToGuidMapping", tvp);
				result = DataUtils.GetDataTableFromCommand(command);
				AssertEquals("Result should not have rows", 0, result.Rows.Count);
			}

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode, ACQ_Date FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_ReportType = 'U11' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by ACQ_ReportSubCode, ACQ_Date",
				TestDbHelper.DefaultCompanyPK, helperDataPKs.BranchPK));
			AssertEquals("Result should have 12 rows", 12, result.Rows.Count);

			var rows = result.Select("");
			AssertRow(rows[0], "USTVA - 202101", helper.ToDate("2021-01-28"), cbDpy1L1);
			AssertRow(rows[1], "USTVA - 202102", helper.ToDate("2021-02-28"), cbDpy2L1);
			AssertRow(rows[2], "USTVA - 202103", helper.ToDate("2021-03-30"), cbDpy3L1);
			AssertRow(rows[3], "USTVA - 202104", helper.ToDate("2021-04-15"), cbDpy4L1);
			AssertRow(rows[4], "USTVA - 202105", helper.ToDate("2021-05-15"), cbDpy5L1);
			AssertRow(rows[5], "USTVA - 202106", helper.ToDate("2021-06-15"), cbDpy6L1);
			AssertRow(rows[6], "USTVA - 202107", helper.ToDate("2021-07-15"), cbDpy7L1);
			AssertRow(rows[7], "USTVA - 202108", helper.ToDate("2021-08-15"), cbDpy8L1);
			AssertRow(rows[8], "USTVA - 202109", helper.ToDate("2021-09-15"), cbDpy9L1);
			AssertRow(rows[9], "USTVA - 202110", helper.ToDate("2021-10-15"), cbDpy10L1);
			AssertRow(rows[10], "USTVA - 202111", helper.ToDate("2021-11-15"), cbDpy11L1);
			AssertRow(rows[11], "USTVA - 202112", helper.ToDate("2021-12-15"), cbDrc12L1);
		}

		[ExpectNoExceptions]
		public void TestSampleCallWithPEoT()
		{
			var helper = new TestDbHelper(TestConnection);
			var helperData = new TestDbHelperData(helper).SetupData();
			var helperDataPKs = helperData.dbHelperDataPKs;

			// CB - DPY for USTVA November and December 2020
			var cbDpy0 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0010", -100m, helper.ToDate("2021-01-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			helper.InsertTransactionLine(cbDpy0, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-01-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy1 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0011", -100m, helper.ToDate("2021-02-28"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			helper.InsertTransactionLine(cbDpy1, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-02-28"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			// CB - DPY for UST 1/11, last year 2021
			var cbDpyU11 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0000", -100m, helper.ToDate("2021-02-28"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpyU11L1 = helper.InsertTransactionLine(cbDpyU11, null, null, helperDataPKs.Ust111Account, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-02-28"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			// CB - DPYs for USTVA for January to November 2021 posted with 2 months delay
			var cbDpy2 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0012", -100m, helper.ToDate("2021-03-31"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy2L1 = helper.InsertTransactionLine(cbDpy2, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-03-31"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy3 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0013", -100m, helper.ToDate("2021-04-30"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy3L1 = helper.InsertTransactionLine(cbDpy3, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-04-30"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy4 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0014", -100m, helper.ToDate("2021-05-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy4L1 = helper.InsertTransactionLine(cbDpy4, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-05-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy5 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0015", -100m, helper.ToDate("2021-06-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy5L1 = helper.InsertTransactionLine(cbDpy5, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-06-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy6 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0016", -100m, helper.ToDate("2021-07-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy6L1 = helper.InsertTransactionLine(cbDpy6, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-07-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy7 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0017", -100m, helper.ToDate("2021-08-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy7L1 = helper.InsertTransactionLine(cbDpy7, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-08-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy8 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0018", -100m, helper.ToDate("2021-09-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy8L1 = helper.InsertTransactionLine(cbDpy8, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-09-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy9 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY0019", -100m, helper.ToDate("2021-10-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy9L1 = helper.InsertTransactionLine(cbDpy9, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-10-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy10 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00110", -100m, helper.ToDate("2021-11-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy10L1 = helper.InsertTransactionLine(cbDpy10, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-11-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy11 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00111", -100m, helper.ToDate("2021-12-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy11L1 = helper.InsertTransactionLine(cbDpy11, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2021-12-15"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDrc12 = helper.InsertTransactionHeader("CB", "DRC", "CBDRC00112", 100m, helper.ToDate("2022-01-31"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDrc12L1 = helper.InsertTransactionLine(cbDrc12, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, -100m, "DRC", postDate: helper.ToDate("2022-01-31"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var cbDpy13 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00113", -100m, helper.ToDate("2022-03-01"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			helper.InsertTransactionLine(cbDpy13, null, null, helperDataPKs.UstvaAccount, helperDataPKs.BranchPK, null, null, 100m, "DPY", postDate: helper.ToDate("2022-03-01"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);

			var tvp = helperData.CreateTVP(helper);
			var tmiRows = tvp.Select("");

			// USTVA for December 2021 is not yet booked as CB - DPY, the data has to be determined from transaction lines in the same way as for the USTVA December 2021
			// AP - INVs
			var apInv1 = helper.InsertTransactionHeader("AP", "INV", "APINV0011", -357m, helper.ToDate("2021-12-12"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Creditor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var apInv1L1 = helper.InsertTransactionLine(apInv1, taxMessageIdPK: tmiRows[3].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-12"), lineAmount: 300m, taxAmount: 57m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			var apInv1L2 = helper.InsertTransactionLine(apInv1, null, postDate: helper.ToDate("2021-12-12"), lineAmount: 300m, taxAmount: 57m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			var apInv2 = helper.InsertTransactionHeader("AP", "CRD", "APINV0021", 107m, helper.ToDate("2021-12-04"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Creditor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var apInv2L1 = helper.InsertTransactionLine(apInv2, taxMessageIdPK: tmiRows[5].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-04"), lineAmount: -100m, taxAmount: -7m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			// AR - INVs
			var arInv1 = helper.InsertTransactionHeader("AR", "INV", "ARINV0012", 238m, helper.ToDate("2021-12-11"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Debitor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var arInv1L1 = helper.InsertTransactionLine(arInv1, taxMessageIdPK: tmiRows[0].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			var arInv1L2 = helper.InsertTransactionLine(arInv1, taxMessageIdPK: tmiRows[0].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-11"), lineAmount: -100m, taxAmount: -19m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			var arInv2 = helper.InsertTransactionHeader("AR", "CRD", "ARINV0014", -100m, helper.ToDate("2021-12-13"), helperDataPKs.BranchPK, helperDataPKs.DepartmentPK, org: helperDataPKs.Debitor1PK, currency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK);
			var arInv2L1 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: tmiRows[2].Field<Guid>("Guid"), postDate: helper.ToDate("2021-12-13"), lineAmount: 100m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			var arInv2L2 = helper.InsertTransactionLine(arInv2, taxMessageIdPK: null, postDate: helper.ToDate("2021-12-13"), lineAmount: 100m, taxAmount: 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, branchPK: helperDataPKs.BranchPK, glAccountPK: helperDataPKs.GlPnLAccount);
			// CB - DRC/DPY
			var cbDrc14 = helper.InsertTransactionHeader("CB", "DRC", "CBDRC00114", 119m, helper.ToDate("2021-12-15"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDrc14L1 = helper.InsertTransactionLine(cbDrc14, null, null, helperDataPKs.GlPnLAccount, helperDataPKs.BranchPK, null, null, -100m, "DRC", postDate: helper.ToDate("2021-12-15"), null, -19m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, taxMessageIdPK: tmiRows[0].Field<Guid>("Guid"));
			var cbDpy15 = helper.InsertTransactionHeader("CB", "DPY", "CBDPY00115", -238m, helper.ToDate("2021-12-16"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy15L1 = helper.InsertTransactionLine(cbDpy15, null, null, helperDataPKs.GlPnLAccount, helperDataPKs.BranchPK, null, null, 200m, "DPY", postDate: helper.ToDate("2021-12-16"), null, 38m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, taxMessageIdPK: tmiRows[3].Field<Guid>("Guid"));
			var cbDpy16 = helper.InsertTransactionHeader("CB", "DRC", "CBDPY00116", 300m, helper.ToDate("2021-12-16"), bankAccountPK: helperDataPKs.BankAccount, companyPK: helperDataPKs.DefaultCompanyPK, currency: "EUR", branchPK: helperDataPKs.BranchPK);
			var cbDpy16L1 = helper.InsertTransactionLine(cbDpy16, null, null, helperDataPKs.GlPnLAccount, helperDataPKs.BranchPK, null, null, -300m, "DRC", postDate: helper.ToDate("2021-12-16"), null, 0m, transactionCurrency: "EUR", companyPK: helperDataPKs.DefaultCompanyPK, taxMessageIdPK: null);

			DataTable result;
			using (var command = Db.Connection.Command("EXEC QueueTransactionsForUST111Report @ReportPK, @PKSpecialVATPrepaymentAccount, @PKAdvancedTurnoverTaxReturnAccount, @TVP"))
			{
				command.AddParameter("@ReportPK", SqlDbType.UniqueIdentifier, helperDataPKs.ReportPK);
				command.AddParameter("@PKSpecialVATPrepaymentAccount", SqlDbType.UniqueIdentifier, helperDataPKs.Ust111Account);
				command.AddParameter("@PKAdvancedTurnoverTaxReturnAccount", SqlDbType.UniqueIdentifier, helperDataPKs.UstvaAccount);
				command.AddTableValuedParameter("@TVP", "dbo.TVP_CodeToGuidMapping", tvp);
				result = DataUtils.GetDataTableFromCommand(command);
				AssertEquals("Result should not have rows", 0, result.Rows.Count);
			}

			result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT ACQ_ParentID, ACQ_ReportSubCode, ACQ_Date FROM dbo.AccTransactionComplianceReportQueue WHERE ACQ_ReportType = 'U11' AND ACQ_ParentTableCode = 'AL' AND  ACQ_GC_Company = '{0}' AND ACQ_GB_Branch = '{1}' order by ACQ_ReportSubCode, ACQ_Date",
				TestDbHelper.DefaultCompanyPK, helperDataPKs.BranchPK));
			AssertEquals("Result should have 19 rows", 19, result.Rows.Count);

			var rows = result.Select("");
			AssertRow(rows[0], "UST 1/11 - 202102", helper.ToDate("2021-02-28"), cbDpyU11L1);
			AssertRow(rows[1], "USTVA - 202101", helper.ToDate("2021-01-31"), cbDpy2L1);
			AssertRow(rows[2], "USTVA - 202102", helper.ToDate("2021-02-28"), cbDpy3L1);
			AssertRow(rows[3], "USTVA - 202103", helper.ToDate("2021-03-15"), cbDpy4L1);
			AssertRow(rows[4], "USTVA - 202104", helper.ToDate("2021-04-15"), cbDpy5L1);
			AssertRow(rows[5], "USTVA - 202105", helper.ToDate("2021-05-15"), cbDpy6L1);
			AssertRow(rows[6], "USTVA - 202106", helper.ToDate("2021-06-15"), cbDpy7L1);
			AssertRow(rows[7], "USTVA - 202107", helper.ToDate("2021-07-15"), cbDpy8L1);
			AssertRow(rows[8], "USTVA - 202108", helper.ToDate("2021-08-15"), cbDpy9L1);
			AssertRow(rows[9], "USTVA - 202109", helper.ToDate("2021-09-15"), cbDpy10L1);
			AssertRow(rows[10], "USTVA - 202110", helper.ToDate("2021-10-15"), cbDpy11L1);
			AssertRow(rows[11], "USTVA - 202111", helper.ToDate("2021-11-30"), cbDrc12L1);
			AssertRow(rows[12], "USTVA - 202112", helper.ToDate("2021-12-04"), apInv2L1);
			AssertRow(rows[13], "USTVA - 202112", helper.ToDate("2021-12-11"), arInv1L1);
			AssertRow(rows[14], "USTVA - 202112", helper.ToDate("2021-12-11"), arInv1L2);
			AssertRow(rows[15], "USTVA - 202112", helper.ToDate("2021-12-12"), apInv1L1);
			AssertRow(rows[16], "USTVA - 202112", helper.ToDate("2021-12-13"), arInv2L1);
			AssertRow(rows[17], "USTVA - 202112", helper.ToDate("2021-12-15"), cbDrc14L1);
			AssertRow(rows[18], "USTVA - 202112", helper.ToDate("2021-12-16"), cbDpy15L1);
		}

		void AssertRow(DataRow row, string acqReportSubCode, DateTime acqDate, Guid acqParentID)
		{
			AssertEquals("ACQ_ReportSubCode", acqReportSubCode, row["ACQ_ReportSubCode"]);
			AssertEquals("ACQ_Date", acqDate, row["ACQ_Date"]);
			AssertEquals("ACQ_ParentID", acqParentID, row["ACQ_ParentID"]);
		}
	}
}


