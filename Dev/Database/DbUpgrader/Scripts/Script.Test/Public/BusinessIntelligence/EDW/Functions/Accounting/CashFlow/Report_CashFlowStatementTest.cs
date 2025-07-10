using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.CashFlow;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting.CashFlow.Testing
{
	[TestedType(typeof(Report_CashFlowStatement))]
	class Report_CashFlowStatementTest : BiCreateScriptTest
	{
		public void TestReport_CashFlowStatement()
		{
			PrepareData();

			var result = Execute(companyPK, 201512, branchPK);
			AssertEquals(23, result.Rows.Count);
			AssertEquals(1, result.Select("CashFlowCode = '111' and TotalAmount = 30").Length);
			AssertEquals(1, result.Select("CashFlowCode = 'tst' and TotalAmount = -100").Length);
			AssertEquals(1, result.Select("CashFlowCode = 'OEQ' and TotalAmount = 30").Length);
			AssertEquals(1, result.Select("CashFlowCode = 'O04' and TotalAmount = 30").Length);
		}

		void PrepareData()
		{
			var orgPK = Guid.NewGuid();
			var orgCreditorGroupPK = Guid.NewGuid();
			var departmentPK = Guid.NewGuid();
			var glAccountPK = Guid.NewGuid();

			var companyKey = TestHelper.InsertCompanyAndGetCompanyKey(companyPK, "CNY", "ABC");
			var branchKey = TestHelper.InsertBranchAndGetBranchKey(branchPK, companyKey, "ABC");
			var organizationKey = TestHelper.InsertOrganizationAndGetOrganizationKey("1stCarDiv", orgPK);
			var departmentKey = TestHelper.CreateBASDepartment(departmentPK);
			var glAccountKey = TestHelper.InsertGLAccountAndGetGLAccountKey("4444.22.22", glAccountPK, "P&L", cashFlowType: "111");
			var orgCreditorGroupKey = TestHelper.InsertOrgCreditorGroupAndGetOrgCreditorGroupKey(orgCreditorGroupPK);

			TestHelper.InsertPeriodManagement(2015, 12, companyKey);
			TestHelper.InsertPeriodManagement(2016, 1, companyKey);
			TestHelper.InsertPeriodManagement(2016, 2, companyKey);
			TestHelper.InsertOrgCompanyData(companyKey, organizationKey, "AAA", orgCreditorGroupKey);

			var date1 = new DateTime(2015, 12, 5);
			var date2 = new DateTime(2015, 1, 5);
			var transactionHeader1 =
				new AccTransactionHeader("AR", "REC", branchPK, departmentPK, companyPK)
				{
					AH_TransactionNum = "00001000",
					AH_InvoiceDate = date1,
					AH_InvoiceAmount = 100m,
					AH_OSTotal = 100m
				};

			var transactionHeader2 =
				new AccTransactionHeader("AP", "PAY", branchPK, departmentPK, companyPK)
				{
					AH_TransactionNum = "00001001",
					AH_InvoiceDate = date2,
					AH_InvoiceAmount = -30m,
					AH_OSTotal = -30m
				};

			var transactionHeader3 =
				new AccTransactionHeader("CB", "PAY", branchPK, departmentPK, companyPK)
				{
					AH_TransactionNum = "00001002",
					AH_InvoiceDate = date2,
					AH_InvoiceAmount = -30m,
					AH_OSTotal = -30m
				};

			TestHelper.InsertGLTransactionHeader(transactionHeader1, date1, date1, "Y", organizationKey, companyKey, branchKey, departmentKey, "tst");
			TestHelper.InsertGLTransactionHeader(transactionHeader2, date2, date1, "Y", organizationKey, companyKey, branchKey, departmentKey, string.Empty);

			var transactionHeaderKey = TestHelper.InsertGLTransactionHeader(transactionHeader3, date2, date1, "Y", organizationKey, companyKey, branchKey, departmentKey, string.Empty);
			var date3 = new DateTime(2015, 12, 2);
			TestHelper.InsertAccGLTransactionLine(companyKey, branchKey, departmentKey, glAccountKey, transactionHeaderKey, "DRC", date3, 30m);

			TestHelper.InsertCashFlowCategoryBasedOnCreditorGroup("O04", orgCreditorGroupPK);
			TestHelper.InsertCashFlowCategoryBasedOnCreditorGroup("OEQ", orgCreditorGroupPK);
			TestHelper.InsertStmDataDate("JournalEntriesLastProcessedDate", new DateTime(2016, 1, 1), companyKey);
		}

		protected DataTable Execute(Guid companyPK, int period, Guid branchPk, char isYearToDate = 'N')
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, string.Format($"SELECT * FROM [{ScriptDbName}].[dbo].Report_CashFlowStatement({period}, '{companyPK}', '{branchPk}', '{isYearToDate}')"));
		}

		protected override string ScriptDbName => Db.EdwDatabaseName;

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;

		readonly Guid companyPK = Guid.Parse("C76A85CC-64BD-48AD-84B6-2D0FC185C258");
		readonly Guid branchPK = Guid.Parse("A08C05DA-19F2-4BEC-A61B-19A4C38CA1D3");
	}
}
