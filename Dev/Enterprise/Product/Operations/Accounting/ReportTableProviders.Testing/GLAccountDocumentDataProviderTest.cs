using System;
using System.Collections;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.ReportTableProviders.GLAccountDocumentDataProvider;
using AccGLHeader = Enterprise.MasterFiles.Business.AccGLHeader;

namespace Enterprise.Accounting.ReportTableProviders.Testing
{
	public class GLAccountDocumentDataProviderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 1, 1), new ZDateTime(2003, 1, 31, 23, 59, 00));
			testHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1), new ZDateTime(2003, 2, 28, 23, 59, 00));
			testHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 30, 23, 59, 00));

			TestProvider = GLAccountDocumentDataProvider.New();
		}

		public void TestEndDate()
		{
			Guid gLAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK.ToGuid();

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { 0, 0, gLAccount, gLAccount, GlbCompany.CurrentCompany.PK.ToGuid()
					, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()
					, GLAccountDocumentDataProvider.HeaderDescOption, new ZDateTime(2003, 1, 1), new ZDateTime(2003, 2, 15) });

			AssertEquals(new ZDateTime(2003, 2, 15, 23, 59, 00), TestProvider.EndDate_ForTestOnly);
		}

		public void TestOpeningBalanceDatesFilter()
		{
			TestProvider.FRetainedEarningsPreviousYear_ForTestOnly = new Account((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			var testGLHeader = Factory.Load<AccGLHeader>(TestProvider.FRetainedEarningsPreviousYear_ForTestOnly.PK);

			var gLAccount = testGLHeader.PK;
			var startdate = new ZDateTime(2003, 1, 10);
			var enddate = new ZDateTime(2003, 1, 31);

			var journal = Factory.New<ARJournal>();
			journal.AH_PostDate = new ZDateTime(2003, 1, 2);
			journal.AH_AG = gLAccount;
			journal.AH_PostToGL = "Y";
			journal.AH_InvoiceAmount = 100m;
			journal.AH_OSTotal = 100m;
			journal.AH_OutstandingAmount = 100m;

			var journal2 = Factory.New<ARJournal>();
			journal2.AH_PostDate = new ZDateTime(2003, 1, 10);
			journal2.AH_AG = gLAccount;
			journal2.AH_PostToGL = "Y";
			journal2.AH_InvoiceAmount = 50m;
			journal2.AH_OSTotal = 50m;
			journal2.AH_OutstandingAmount = 50m;

			Factory.Save();

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { 200301, 200301, gLAccount.ToGuid(), gLAccount.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid()
					, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()
					, GLAccountDocumentDataProvider.HeaderDescOption, ZDateTime.Empty, ZDateTime.Empty });

			DataTable table = TestProvider.GetDataTable_ForTestOnly();
			AssertEquals("Should be 2 row in table", 2, table.Rows.Count);
			AssertEquals(0m, Convert.ToDecimal(table.Rows[0]["OpeningBalance"]));

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { 0, 0, gLAccount.ToGuid(), gLAccount.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid()
					, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()
					, GLAccountDocumentDataProvider.HeaderDescOption, startdate, enddate });

			table = TestProvider.GetDataTable_ForTestOnly();
			AssertEquals("Should be 1 row in table", 1, table.Rows.Count);
			AssertEquals(-100m, Convert.ToDecimal(table.Rows[0]["OpeningBalance"]));
		}

		public void TestOpeningBalanceRetainedEarnings()
		{
			TestProvider.FRetainedEarningsPreviousYear_ForTestOnly = new Account((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AccGLHeader testGLHeader = Factory.Load<AccGLHeader>(TestProvider.FRetainedEarningsPreviousYear_ForTestOnly.PK);

			AccPeriodManagementCollection pC = new AccPeriodManagementCollection(Factory);
			pC.Load();

			ARJournal journal = Factory.New<ARJournal>();
			journal.AH_PostDate = new ZDateTime(2003, 1, 31);
			journal.AH_AG = testGLHeader.PK;
			journal.AH_PostToGL = "Y";
			journal.AH_OSTotal = 100m;
			journal.AH_InvoiceAmount = 100m;
			journal.AH_OutstandingAmount = 100m;

			ARJournal journal2 = Factory.New<ARJournal>();
			journal2.AH_PostDate = new ZDateTime(2003, 2, 28);
			journal2.AH_AG = testGLHeader.PK;
			journal2.AH_PostToGL = "Y";
			journal2.AH_OSTotal = 50m;
			journal2.AH_InvoiceAmount = 50m;
			journal2.AH_OutstandingAmount = 50m;

			BatchTestHelper testHelper = new BatchTestHelper(Factory);
			testHelper.InsertTestAggregateRow(Factory, testGLHeader.PK, 100m, 200301);
			testHelper.InsertTestAggregateRow(Factory, testGLHeader.PK, 200m, 200302);

			Factory.Save();

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { 200302, 200302, testGLHeader.PK.ToGuid(), testGLHeader.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid()
					, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()
					, GLAccountDocumentDataProvider.HeaderDescOption, ZDateTime.Empty, ZDateTime.Empty });

			DataTable table = TestProvider.GetDataTable_ForTestOnly();
			AssertEquals(100m, Convert.ToDecimal(table.Rows[0]["OpeningBalance"]));
		}

		public void TestWIPJournals()
		{
			TestProvider.FRetainedEarningsPreviousYear_ForTestOnly = new Account((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AccGLHeader testGLHeader = Factory.Load<AccGLHeader>(TestProvider.FRetainedEarningsPreviousYear_ForTestOnly.PK);

			ZGuid gLAccount = testGLHeader.PK;
			ZInt startPeriod = 200301;
			ZInt endPeriod = 200301;
			ZDateTime startdate = new ZDateTime(2003, 1, 1);
			ZDateTime enddate = new ZDateTime(2003, 1, 31);
			Guid companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			AccChargeCode testChargeCode = TestObjectCreator.CC1;
			testChargeCode.AC_AG_WIPAccount = gLAccount;
			string descOption = GLAccountDocumentDataProvider.HeaderDescOption;
			ZDateTime postDate = new ZDateTime(2003, 1, 2);

			Factory.Save();

			WIP testWIP = Factory.NewWithValidTestData<WIP>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_ARLine = testWIP.PK;
			testWIP.AL_JH = charge.JR_JH;
			testWIP.AL_PostToGL = "Y";
			testWIP.AL_PostDate = postDate;
			testWIP.AL_AC = testChargeCode.PK;
			testWIP.AL_GB = branchPK;
			testWIP.AL_GE = departmentPK;
			testWIP.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testWIP.AL_OSExTaxAmount = -10m;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(testWIP);

			Factory.Save();

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { startPeriod, endPeriod, gLAccount.ToGuid(), gLAccount.ToGuid(), companyPK, branchPK, departmentPK, descOption, startdate, enddate });
			DataTable table = TestProvider.GetDataTable_ForTestOnly();
			AssertEquals("Should have one row in the table", 1, table.Rows.Count);
			AssertEquals(10m, table.Rows[0]["Amount"]);
			AssertEquals("WIP", table.Rows[0]["Type"]);
		}

		public void TestACRJournals()
		{
			TestProvider.FRetainedEarningsPreviousYear_ForTestOnly = new Account((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AccGLHeader testGLHeader = Factory.Load<AccGLHeader>(TestProvider.FRetainedEarningsPreviousYear_ForTestOnly.PK);

			ZGuid gLAccount = testGLHeader.PK;
			ZInt startPeriod = 200301;
			ZInt endPeriod = 200301;
			ZDateTime startdate = new ZDateTime(2003, 1, 1);
			ZDateTime enddate = new ZDateTime(2003, 1, 31);
			Guid companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			AccChargeCode testChargeCode = TestObjectCreator.CC1;
			testChargeCode.AC_AG_AccrualAccount = gLAccount;
			string descOption = GLAccountDocumentDataProvider.HeaderDescOption;
			ZDateTime postDate = new ZDateTime(2003, 1, 2);

			Accrual testACR = Factory.NewWithValidTestData<Accrual>();
			JobCharge charge = Factory.NewWithValidTestData<JobCharge>();
			charge.JR_AL_APLine = testACR.PK;
			testACR.AL_JH = charge.JR_JH;
			testACR.AL_PostToGL = "Y";
			testACR.AL_PostDate = postDate;
			testACR.AL_AC = testChargeCode.PK;
			testACR.AL_GB = branchPK;
			testACR.AL_GE = departmentPK;
			testACR.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testACR.AL_OSExTaxAmount = 10m;
			TransactionLineTestHelper.SinchronizeChargeAndWipAccrualAmounts(testACR);

			Factory.Save();

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { startPeriod, endPeriod, gLAccount.ToGuid(), gLAccount.ToGuid(), companyPK, branchPK, departmentPK, descOption, startdate, enddate });
			DataTable table = TestProvider.GetDataTable_ForTestOnly();
			AssertEquals("Should have one row in the table", 1, table.Rows.Count);
			AssertEquals(10m, table.Rows[0]["Amount"]);
			AssertEquals("ACR", table.Rows[0]["Type"]);
		}

		public void TestShowHeaderDescription()
		{
			TestProvider.FRetainedEarningsPreviousYear_ForTestOnly = new Account((Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));

			AccGLHeader testGLHeader = Factory.Load<AccGLHeader>(TestProvider.FRetainedEarningsPreviousYear_ForTestOnly.PK);

			ZGuid gLAccount = testGLHeader.PK;
			ZInt startPeriod = 200301;
			ZInt endPeriod = 200301;
			ZDateTime startdate = new ZDateTime(2003, 1, 1);
			ZDateTime enddate = new ZDateTime(2003, 1, 31);

			Guid companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			string descOption = GLAccountDocumentDataProvider.HeaderDescOption;

			ZDateTime postDate = new ZDateTime(2003, 1, 2);

			// Invoice                
			ARInvoice invoice = Factory.New<ARInvoice>();
			invoice.AH_Desc = "Header Description";
			invoice.AH_PostDate = postDate;
			invoice.AH_PostToGL = "Y";

			ARInvoiceLine line1 = (ARInvoiceLine)invoice.Lines.AddNew();
			line1.AL_Desc = "Line Description";
			line1.AL_AG = gLAccount;

			Factory.Save();

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { startPeriod, endPeriod, gLAccount.ToGuid(), gLAccount.ToGuid(), companyPK, branchPK, departmentPK, descOption, startdate, enddate });
			DataTable table = TestProvider.GetDataTable_ForTestOnly();
			DataRow invoiceRow = table.Rows[0];
			AssertEquals("Should have one row in the table", 1, table.Rows.Count);
			AssertEquals("Should be showing header description", "Header Description", invoiceRow["TransactionDesc"]);

			descOption = GLAccountDocumentDataProvider.LineDescOption;
			TestProvider = GLAccountDocumentDataProvider.New();
			TestProvider.FShowHeaderDescription_ForTestOnly = false;
			TestProvider.SetupParameterValues_ForTestOnly(new object[] { startPeriod, endPeriod, gLAccount.ToGuid(), gLAccount.ToGuid(), companyPK, branchPK, departmentPK, descOption, startdate, enddate });
			table = TestProvider.GetDataTable_ForTestOnly();
			invoiceRow = table.Rows[0];
			AssertEquals("Should have one row in the table", 1, table.Rows.Count);
			AssertEquals("Should show line description", "Line Description", invoiceRow["TransactionDesc"]);
		}

		public void TestExcludeZeroBalanceRows()
		{
			Guid gLAccount = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK.ToGuid();
			ZInt startPeriod = 200301;
			ZInt endPeriod = 200301;
			ZDateTime startdate = new ZDateTime(2003, 1, 1);
			ZDateTime enddate = new ZDateTime(2003, 1, 31);
			Guid companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			Guid branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
			Guid departmentPK = GlbDepartment.CurrentDepartment.PK.ToGuid();
			string descOption = GLAccountDocumentDataProvider.LineDescOption;

			TestProvider.SetupParameterValues_ForTestOnly(new object[] { startPeriod, endPeriod, gLAccount, gLAccount, companyPK, branchPK, departmentPK, descOption, startdate, enddate });

			DataTable table = TestProvider.GetDataTable_ForTestOnly();
			AssertEquals("Should be no rows in the table", 0, table.Rows.Count);
		}

		public void TestGetStartPeriod()
		{
			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider(Guid.NewGuid(), Guid.NewGuid());
			testDataProvider.Factory_Exposed = Factory;

			int startPeriod = testDataProvider.GetStartPeriodOfFinancialYear_Exposed(200302);
			AssertEquals(200301, startPeriod);

			startPeriod = testDataProvider.GetStartPeriodOfFinancialYear_Exposed(200401);
			AssertEquals(0, startPeriod);
		}

		public void TestGetGLAccountType()
		{
			string bSHAccountNum = "BSHAccount";
			string pLAccountNum = "P&LAccount";

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider(Guid.NewGuid(), Guid.NewGuid());
			testDataProvider.Factory_Exposed = Factory;

			AccGLHeader bSHAccount = testDataProvider.Factory_Exposed.New(typeof(AccGLHeader)) as AccGLHeader;
			AccGLHeader pLAccount = testDataProvider.Factory_Exposed.New(typeof(AccGLHeader)) as AccGLHeader;

			bSHAccount.AG_AccountNum = bSHAccountNum;
			bSHAccount.AG_AccountType = "BSH";

			pLAccount.AG_AccountNum = pLAccountNum;
			pLAccount.AG_AccountType = "P&L";

			AssertEquals("BSH", testDataProvider.GetGLAccountType_Exposed(bSHAccountNum));
			AssertEquals("P&L", testDataProvider.GetGLAccountType_Exposed(pLAccountNum));
		}

		public void TestGetPeriodRange()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.PostPeriodsForEntireYear(2004);

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider(Guid.NewGuid(), Guid.NewGuid());
			testDataProvider.SetPeriodRange_Exposed(200403, 200407);

			AssertEquals(5, testDataProvider.PeriodRangeExposed.Length);
			AssertEquals(200403, testDataProvider.PeriodRangeExposed[0]);
			AssertEquals(200407, testDataProvider.PeriodRangeExposed[4]);
		}

		public void TestAutoGLJournal()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();

			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 29, 23, 59, 59));
			testHelper.SetupSinglePeriod(200403, new ZDateTime(2004, 3, 1), new ZDateTime(2004, 3, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200404, new ZDateTime(2004, 4, 1), new ZDateTime(2004, 4, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200405, new ZDateTime(2004, 5, 1), new ZDateTime(2004, 5, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200406, new ZDateTime(2004, 6, 1), new ZDateTime(2004, 6, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200407, new ZDateTime(2004, 7, 1), new ZDateTime(2004, 7, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200408, new ZDateTime(2004, 8, 1), new ZDateTime(2004, 8, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200409, new ZDateTime(2004, 9, 1), new ZDateTime(2004, 9, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(2004010, new ZDateTime(2004, 10, 1), new ZDateTime(2004, 10, 31, 23, 59, 59));

			SetGLAccounts();

			ARInvoice transaction = Factory.NewWithValidTestData<ARInvoice>();

			SetHeaderOnlyLines(transaction, TransactionTypes.GLAutoJournal, 120, new ZDateTime(2004, 2, 1));
			transaction.AH_DueDate = new ZDateTime(2004, 9, 1).ToDateTime();
			transaction.AH_Ledger = LedgerTypes.General;

			var linesRow1 = (ARInvoiceLine)transaction.Lines.AddNew();
			var linesRow2 = (ARInvoiceLine)transaction.Lines.AddNew();

			SetHeaderWithLines(linesRow1, transaction, TransactionTypes.GLAutoJournal, Utilities.GetGuidFromObject(GLAccounts[0]), 60m);
			SetHeaderWithLines(linesRow2, transaction, TransactionTypes.GLAutoJournal, Utilities.GetGuidFromObject(GLAccounts[1]), -60m);

			linesRow1.AL_PostDate = new ZDateTime(2004, 2, 1).ToDateTime();
			linesRow1.AL_ReverseDate = new ZDateTime(2004, 9, 1).ToDateTime();
			linesRow2.AL_PostDate = new ZDateTime(2004, 2, 1).ToDateTime();
			linesRow2.AL_ReverseDate = new ZDateTime(2004, 9, 1).ToDateTime();

			Factory.Save();

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider((Guid)GLAccounts[0], (Guid)GLAccounts[39]);
			GLAccountListDocumentDataSet resultData = new GLAccountListDocumentDataSet();
			testDataProvider.EndPeriodExposed = 200409;
			testDataProvider.StartPeriodExposed = 200401;
			testDataProvider.SetDatesFromPeriods_ForTestOnly();
			testDataProvider.SetPeriodRange_Exposed(200401, 200409);

			testDataProvider.GetControlAccountExposed();
			testDataProvider.AddControlAccountToList();

			testDataProvider.FillDataSetForTestOnly(resultData);

			AssertEquals(16, resultData.GLAccountListDataSet.Count);
		}

		public void TestAutoGLJournalPeriodRange()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);

			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 29, 23, 59, 59));
			testHelper.SetupSinglePeriod(200403, new ZDateTime(2004, 3, 1), new ZDateTime(2004, 3, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200404, new ZDateTime(2004, 4, 1), new ZDateTime(2004, 4, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200405, new ZDateTime(2004, 5, 1), new ZDateTime(2004, 5, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200406, new ZDateTime(2004, 6, 1), new ZDateTime(2004, 6, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200407, new ZDateTime(2004, 7, 1), new ZDateTime(2004, 7, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200408, new ZDateTime(2004, 8, 1), new ZDateTime(2004, 8, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200409, new ZDateTime(2004, 9, 1), new ZDateTime(2004, 9, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200410, new ZDateTime(2004, 10, 1), new ZDateTime(2004, 10, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200411, new ZDateTime(2004, 11, 1), new ZDateTime(2004, 11, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200412, new ZDateTime(2004, 12, 1), new ZDateTime(2004, 12, 31, 23, 59, 59));

			SetGLAccounts();

			ARInvoice transaction = Factory.NewWithValidTestData<ARInvoice>();

			SetHeaderOnlyLines(transaction, TransactionTypes.GLAutoJournal, 0, new ZDateTime(2004, 2, 1));
			transaction.AH_Ledger = LedgerTypes.General;
			transaction.AH_DueDate = new ZDateTime(2004, 9, 1).ToDateTime();

			var linesRow1 = (ARInvoiceLine)transaction.Lines.AddNew();

			SetHeaderWithLines(linesRow1, transaction, TransactionTypes.GLAutoJournal, Utilities.GetGuidFromObject(GLAccounts[0]));
			linesRow1.AL_PostDate = new ZDateTime(2004, 2, 1).ToDateTime();
			linesRow1.AL_ReverseDate = new ZDateTime(2004, 9, 1).ToDateTime();

			Factory.Save();

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider((Guid)GLAccounts[0], (Guid)GLAccounts[39]);
			testDataProvider.Factory_Exposed = Factory;
			GLAccountListDocumentDataSet resultData = new GLAccountListDocumentDataSet();
			testDataProvider.EndPeriodExposed = 200409;
			testDataProvider.StartPeriodExposed = 200401;
			testDataProvider.SetDatesFromPeriods_ForTestOnly();
			testDataProvider.SetPeriodRange_Exposed(200401, 200409);

			testDataProvider.GetControlAccountExposed();
			testDataProvider.AddControlAccountToList();

			testDataProvider.FillDataSetForTestOnly(resultData);

			AssertEquals(8, resultData.GLAccountListDataSet.Count);
			AssertEquals(200402, resultData.GLAccountListDataSet[0].Period);
			AssertEquals(200403, resultData.GLAccountListDataSet[1].Period);
			AssertEquals(200404, resultData.GLAccountListDataSet[2].Period);
			AssertEquals(200405, resultData.GLAccountListDataSet[3].Period);
			AssertEquals(200406, resultData.GLAccountListDataSet[4].Period);
			AssertEquals(200407, resultData.GLAccountListDataSet[5].Period);
			AssertEquals(200408, resultData.GLAccountListDataSet[6].Period);
			AssertEquals(200409, resultData.GLAccountListDataSet[7].Period);

			testDataProvider.EndPeriodExposed = 200407;
			testDataProvider.StartPeriodExposed = 200403;
			testDataProvider.SetPeriodRange_Exposed(200403, 200407);
			resultData = new GLAccountListDocumentDataSet();
			testDataProvider.FillDataSetForTestOnly(resultData);
			AssertEquals(5, resultData.GLAccountListDataSet.Count);
			AssertEquals(200403, resultData.GLAccountListDataSet[0].Period);
			AssertEquals(200404, resultData.GLAccountListDataSet[1].Period);
			AssertEquals(200405, resultData.GLAccountListDataSet[2].Period);
			AssertEquals(200406, resultData.GLAccountListDataSet[3].Period);
			AssertEquals(200407, resultData.GLAccountListDataSet[4].Period);

			testDataProvider.EndPeriodExposed = 200412;
			testDataProvider.StartPeriodExposed = 200404;
			testDataProvider.SetPeriodRange_Exposed(200404, 200412);
			resultData = new GLAccountListDocumentDataSet();
			testDataProvider.FillDataSetForTestOnly(resultData);
			AssertEquals(6, resultData.GLAccountListDataSet.Count);
			AssertEquals(200404, resultData.GLAccountListDataSet[0].Period);
			AssertEquals(200405, resultData.GLAccountListDataSet[1].Period);
			AssertEquals(200406, resultData.GLAccountListDataSet[2].Period);
			AssertEquals(200407, resultData.GLAccountListDataSet[3].Period);
			AssertEquals(200408, resultData.GLAccountListDataSet[4].Period);
			AssertEquals(200409, resultData.GLAccountListDataSet[5].Period);

			testDataProvider.EndPeriodExposed = 200412;
			testDataProvider.StartPeriodExposed = 200401;
			testDataProvider.SetPeriodRange_Exposed(200401, 200412);
			resultData = new GLAccountListDocumentDataSet();
			testDataProvider.FillDataSetForTestOnly(resultData);
			AssertEquals(8, resultData.GLAccountListDataSet.Count);
			AssertEquals(200402, resultData.GLAccountListDataSet[0].Period);
			AssertEquals(200403, resultData.GLAccountListDataSet[1].Period);
			AssertEquals(200404, resultData.GLAccountListDataSet[2].Period);
			AssertEquals(200405, resultData.GLAccountListDataSet[3].Period);
			AssertEquals(200406, resultData.GLAccountListDataSet[4].Period);
			AssertEquals(200407, resultData.GLAccountListDataSet[5].Period);
			AssertEquals(200408, resultData.GLAccountListDataSet[6].Period);
			AssertEquals(200409, resultData.GLAccountListDataSet[7].Period);

			testDataProvider.EndPeriodExposed = 200409;
			testDataProvider.StartPeriodExposed = 200402;
			testDataProvider.SetPeriodRange_Exposed(200402, 200409);
			resultData = new GLAccountListDocumentDataSet();
			testDataProvider.FillDataSetForTestOnly(resultData);
			AssertEquals(8, resultData.GLAccountListDataSet.Count);
			AssertEquals(200402, resultData.GLAccountListDataSet[0].Period);
			AssertEquals(200403, resultData.GLAccountListDataSet[1].Period);
			AssertEquals(200404, resultData.GLAccountListDataSet[2].Period);
			AssertEquals(200405, resultData.GLAccountListDataSet[3].Period);
			AssertEquals(200406, resultData.GLAccountListDataSet[4].Period);
			AssertEquals(200407, resultData.GLAccountListDataSet[5].Period);
			AssertEquals(200408, resultData.GLAccountListDataSet[6].Period);
			AssertEquals(200409, resultData.GLAccountListDataSet[7].Period);
		}

		public void TestARAPJournal()
		{
			SetGLAccounts();

			ARJournal testJournal1 = Factory.NewWithValidTestData<ARJournal>();
			ARJournal testJournal2 = Factory.NewWithValidTestData<ARJournal>();
			ARJournal testJournal3 = Factory.NewWithValidTestData<ARJournal>();
			ARJournal testJournal4 = Factory.NewWithValidTestData<ARJournal>();

			SetAPARJournal(testJournal1, Utilities.GetGuidFromObject(GLAccounts[0]), -100.0M, Date200302);
			SetAPARJournal(testJournal2, Utilities.GetGuidFromObject(GLAccounts[1]), -100.0M, Date200302);
			SetAPARJournal(testJournal3, Utilities.GetGuidFromObject(GLAccounts[0]), -50.0M, Date200302);
			SetAPARJournal(testJournal4, Utilities.GetGuidFromObject(GLAccounts[0]), -50.0M, Date200303);

			testJournal1.AH_OutstandingAmount = -100M;
			testJournal2.AH_OutstandingAmount = -100M;
			testJournal3.AH_OutstandingAmount = -50M;
			testJournal4.AH_OutstandingAmount = -50M;

			Factory.Save();

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider((Guid)GLAccounts[0], (Guid)GLAccounts[39], 200302, 200303);

			GLAccountListDocumentDataSet resultData = new GLAccountListDocumentDataSet();
			testDataProvider.GetControlAccountExposed();
			testDataProvider.AddControlAccountToList();

			testDataProvider.FillDataSetForTestOnly(resultData);

			AssertEquals(4, resultData.GLAccountListDataSet.Count);

			AssertEquals(testDataProvider.GetARControlExposed(200302).Amount, -250.0);
			AssertEquals(testDataProvider.GetARControlExposed(200303).Amount, -50.0);
		}

		public void TestContra()
		{
			SetGLAccounts();

			ARContraRow arContraRow1 = Factory.NewWithValidTestData<ARContraRow>();
			ARContraRow arContraRow2 = Factory.NewWithValidTestData<ARContraRow>();
			ARContraRow arContraRow3 = Factory.NewWithValidTestData<ARContraRow>();

			APContraRow apContraRow1 = Factory.NewWithValidTestData<APContraRow>();
			APContraRow apContraRow2 = Factory.NewWithValidTestData<APContraRow>();
			APContraRow apContraRow3 = Factory.NewWithValidTestData<APContraRow>();

			arContraRow1.AH_InvoiceAmount = arContraRow1.AH_OutstandingAmount = arContraRow1.AH_OSTotal = 100.0M;
			arContraRow1.AH_PostDate = Date200302;
			arContraRow2.AH_InvoiceAmount = arContraRow2.AH_OutstandingAmount = arContraRow2.AH_OSTotal = -70.0M;
			arContraRow2.AH_PostDate = Date200302;
			arContraRow3.AH_InvoiceAmount = arContraRow3.AH_OutstandingAmount = arContraRow3.AH_OSTotal = 50.0M;
			arContraRow3.AH_PostDate = Date200303;

			apContraRow1.AH_InvoiceAmount = apContraRow1.AH_OutstandingAmount = apContraRow1.AH_OSTotal = -100.0M;
			apContraRow1.AH_PostDate = Date200302;
			apContraRow2.AH_InvoiceAmount = apContraRow2.AH_OutstandingAmount = apContraRow2.AH_OSTotal = 70.0M;
			apContraRow2.AH_PostDate = Date200302;
			apContraRow3.AH_InvoiceAmount = apContraRow3.AH_OutstandingAmount = apContraRow3.AH_OSTotal = -50.0M;
			apContraRow3.AH_PostDate = Date200303;

			arContraRow1.AH_PostToGL = arContraRow2.AH_PostToGL = arContraRow3.AH_PostToGL = "Y";
			apContraRow1.AH_PostToGL = apContraRow2.AH_PostToGL = apContraRow3.AH_PostToGL = "Y";

			Factory.Save();

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider((Guid)GLAccounts[0], (Guid)GLAccounts[39], 200302, 200303);

			GLAccountListDocumentDataSet resultData = new GLAccountListDocumentDataSet();
			testDataProvider.fGLTransactionContainsControlAccount_Exposed = true;
			testDataProvider.GetControlAccountExposed();
			testDataProvider.AddControlAccountToList();
			testDataProvider.FillDataSetForTestOnly(resultData);

			AssertEquals(0, resultData.GLAccountListDataSet.Count);
			AssertEquals(30.0, testDataProvider.GetARControlExposed(200302).Amount);
			AssertEquals(50.0, testDataProvider.GetARControlExposed(200303).Amount);
			AssertEquals(-30.0, testDataProvider.GetAPControlExposed(200302).Amount);
			AssertEquals(-50.0, testDataProvider.GetAPControlExposed(200303).Amount);
		}

		public void TestLineDescriptions()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);

			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 29, 23, 59, 59));
			testHelper.SetupSinglePeriod(200403, new ZDateTime(2004, 3, 1), new ZDateTime(2004, 3, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200404, new ZDateTime(2004, 4, 1), new ZDateTime(2004, 4, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200405, new ZDateTime(2004, 5, 1), new ZDateTime(2004, 5, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200406, new ZDateTime(2004, 6, 1), new ZDateTime(2004, 6, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200407, new ZDateTime(2004, 7, 1), new ZDateTime(2004, 7, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200408, new ZDateTime(2004, 8, 1), new ZDateTime(2004, 8, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200409, new ZDateTime(2004, 9, 1), new ZDateTime(2004, 9, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200410, new ZDateTime(2004, 10, 1), new ZDateTime(2004, 10, 31, 23, 59, 59));
			testHelper.SetupSinglePeriod(200411, new ZDateTime(2004, 11, 1), new ZDateTime(2004, 11, 30, 23, 59, 59));
			testHelper.SetupSinglePeriod(200412, new ZDateTime(2004, 12, 1), new ZDateTime(2004, 12, 31, 23, 59, 59));

			SetGLAccounts();

			string headerDescription = "Header Description";
			string lineDescription = "123456789_123456789_123456789_123456789_123456789_123456789_this part should get truncated";

			var headerAJL = Factory.NewWithValidTestData<GLJournal>();
			headerAJL.AH_Ledger = LedgerTypes.General;
			SetHeaderOnlyLines(headerAJL, TransactionTypes.GLAutoJournal, 0, new ZDateTime(2004, 2, 1));
			headerAJL.AH_DueDate = new ZDateTime(2004, 9, 1).ToDateTime();
			headerAJL.AH_Desc = headerDescription;

			var headerRJL = Factory.NewWithValidTestData<GLJournal>();
			headerRJL.AH_Ledger = LedgerTypes.General;
			SetHeaderOnlyLines(headerRJL, TransactionTypes.GLReversingJournal, 0, new ZDateTime(2004, 3, 1));
			headerRJL.AH_DueDate = new ZDateTime(2004, 10, 1).ToDateTime();
			headerRJL.AH_Desc = headerDescription;

			var headerGJL = Factory.NewWithValidTestData<GLJournal>();
			headerGJL.AH_Ledger = LedgerTypes.General;
			SetHeaderOnlyLines(headerGJL, TransactionTypes.GLStandardJournal, 0, new ZDateTime(2004, 4, 1));
			headerGJL.AH_DueDate = new ZDateTime(2004, 11, 1).ToDateTime();
			headerGJL.AH_Desc = headerDescription;

			var lineAJL = (GLJournalLine)headerAJL.Lines.AddNew();
			lineAJL.AL_PostDate = new ZDateTime(2004, 2, 1).ToDateTime();
			lineAJL.AL_ReverseDate = new ZDateTime(2004, 9, 1).ToDateTime();
			lineAJL.AL_Desc = lineDescription;

			var lineRJL = (GLJournalLine)headerAJL.Lines.AddNew();
			lineRJL.AL_PostDate = new ZDateTime(2004, 2, 1).ToDateTime();
			lineRJL.AL_ReverseDate = new ZDateTime(2004, 9, 1).ToDateTime();
			lineRJL.AL_Desc = lineDescription;

			var lineGJL = (GLJournalLine)headerAJL.Lines.AddNew();
			lineGJL.AL_PostDate = new ZDateTime(2004, 2, 1).ToDateTime();
			lineGJL.AL_ReverseDate = new ZDateTime(2004, 9, 1).ToDateTime();
			lineGJL.AL_Desc = lineDescription;

			SetHeaderWithLines(lineAJL, headerAJL, TransactionTypes.GLAutoJournal, Utilities.GetGuidFromObject(GLAccounts[8]));
			SetHeaderWithLines(lineRJL, headerRJL, TransactionTypes.GLReversingJournal, Utilities.GetGuidFromObject(GLAccounts[9]));
			SetHeaderWithLines(lineGJL, headerGJL, TransactionTypes.GLStandardJournal, Utilities.GetGuidFromObject(GLAccounts[10]));

			Factory.Save();

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider((Guid)GLAccounts[0], (Guid)GLAccounts[39]);
			testDataProvider.Factory_Exposed = Factory;
			GLAccountListDocumentDataSet resultData = new GLAccountListDocumentDataSet();
			testDataProvider.EndPeriodExposed = 200409;
			testDataProvider.StartPeriodExposed = 200401;
			testDataProvider.SetDatesFromPeriods_ForTestOnly();
			testDataProvider.SetPeriodRange_Exposed(200401, 200409);
			testDataProvider.GetControlAccountExposed();
			testDataProvider.AddControlAccountToList();
			testDataProvider.FillDataSetForTestOnly(resultData);

			foreach (GLAccountListDocumentDataSet.GLAccountListDataSetRow row in resultData.GLAccountListDataSet)
			{
				AssertEquals("Transaction lines of type " + row.Type + " should display line descriptions truncated to 60 characaters.", lineDescription.Substring(0, 60), row.TransactionDesc);
			}
		}

		public void TestARAPExchangeDifference()
		{
			SetGLAccounts();

			ExchangeDifference transaction1 = Factory.NewWithValidTestData<ARExchangeDifference>();
			ExchangeDifference transaction2 = Factory.NewWithValidTestData<ARExchangeDifference>();
			ExchangeDifference transaction3 = Factory.NewWithValidTestData<APExchangeDifference>();

			transaction1.AH_InvoiceAmount = transaction1.AH_OSTotal = -50.0M;
			transaction2.AH_InvoiceAmount = transaction2.AH_OSTotal = 100.0M;
			transaction3.AH_InvoiceAmount = transaction3.AH_OSTotal = 100.0M;

			transaction1.AH_PostDate = transaction2.AH_PostDate = transaction3.AH_PostDate = Date200302;
			transaction1.AH_PostToGL = transaction2.AH_PostToGL = transaction3.AH_PostToGL = "Y";

			transaction1.AH_OutstandingAmount = -50M;
			transaction2.AH_OutstandingAmount = 100M;
			transaction3.AH_OutstandingAmount = 100M;

			PrepareMiscellaneousTransactionForSaving(transaction1);
			PrepareMiscellaneousTransactionForSaving(transaction2);
			PrepareMiscellaneousTransactionForSaving(transaction3);

			Factory.Save();

			MockGLAccountDocumentDataProvider testDataProvider = new MockGLAccountDocumentDataProvider((Guid)GLAccounts[0], (Guid)GLAccounts[39], 200302, 200302);

			GLAccountListDocumentDataSet resultData = new GLAccountListDocumentDataSet();
			testDataProvider.GetControlAccountExposed();
			testDataProvider.fGLTransactionContainsControlAccount_Exposed = true;
			testDataProvider.FillDataSetForTestOnly(resultData);
			resultData = testDataProvider.Sort("Amount", resultData);

			AssertEquals(3, resultData.GLAccountListDataSet.Count);
			AssertEquals(testDataProvider.ExchangeDifference.AccountNo, resultData.GLAccountListDataSet[0].GLAccount);
			AssertEquals(testDataProvider.ExchangeDifference.AccountNo, resultData.GLAccountListDataSet[1].GLAccount);
			AssertEquals(testDataProvider.ExchangeDifference.AccountNo, resultData.GLAccountListDataSet[2].GLAccount);

			AssertEquals(50.0M, resultData.GLAccountListDataSet[2].Amount);
			AssertEquals(-100.0M, resultData.GLAccountListDataSet[0].Amount);

			AssertEquals(50.0, testDataProvider.GetARControlExposed(200302).Amount);
			AssertEquals(100.0, testDataProvider.GetAPControlExposed(200302).Amount);
		}

		#region Implementation

		void PrepareMiscellaneousTransactionForSaving(TransactionHeader miscHeader)
		{
			AccTransactionHeader journalToMatch = miscHeader.Factory.New<AccTransactionHeader>();

			journalToMatch.AH_InvoiceDate = DateTime.Now;
			journalToMatch.AH_Ledger = LedgerTypes.AccountsReceivable;
			journalToMatch.AH_TransactionType = TransactionTypes.Journal;
			journalToMatch.AH_InvoiceAmount = -miscHeader.AH_InvoiceAmount;
			journalToMatch.AH_OutstandingAmount = -miscHeader.AH_OutstandingAmount;
			journalToMatch.AH_PostDate = miscHeader.AH_PostDate;
			journalToMatch.AH_GB = miscHeader.AH_GB;
			journalToMatch.AH_GE = miscHeader.AH_GE;
			journalToMatch.AH_TransactionNum = miscHeader.AH_TransactionNum + "J";
			journalToMatch.AH_PostToGL = "N";

			AccTransactionMatchLink link1 = miscHeader.Factory.New<AccTransactionMatchLink>();
			AccTransactionMatchLink link2 = miscHeader.Factory.New<AccTransactionMatchLink>();

			link1.AP_AH = miscHeader.PK;
			link1.AP_Amount = miscHeader.AH_OutstandingAmount;
			link2.AP_AH = journalToMatch.PK;
			link2.AP_Amount = journalToMatch.AH_OutstandingAmount;
			link1.AP_MatchDate = link2.AP_MatchDate = DateTime.Now;
			link1.AP_MatchGroupNum = link1.AP_MatchGroupNum = "100100";

			TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
			matchlinks.Add(link1);
			matchlinks.Add(link2);
			miscHeader.AH_OutstandingAmount = 0m;
			journalToMatch.AH_OutstandingAmount = 0m;
		}

		ArrayList GLAccounts;
		ArrayList GLAccountDescription;
		ArrayList GLAccountNumber;
		ArrayList GLAccountType;

		readonly ZDateTime Date200302 = new ZDateTime(2003, 2, 15);
		readonly ZDateTime Date200303 = new ZDateTime(2003, 3, 15);

		protected GLAccountDocumentDataProvider TestProvider;

		void SetAPARJournal(Journal testJournal, Guid accountPK, decimal amount, ZDateTime postDateTime)
		{
			testJournal.AH_AG = accountPK;
			testJournal.AH_InvoiceAmount = testJournal.AH_OSTotal = amount;
			testJournal.AH_PostDate = postDateTime.ToDateTime();
			testJournal.AH_GB = GlbBranch.CurrentBranch.PK.ToGuid();
			testJournal.AH_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();

			testJournal.AH_PostToGL = "Y";
		}

		void SetHeaderOnlyLines(TransactionHeaderWithLines testTransaction, string transactionType, decimal amount, ZDateTime postDateTime)
		{
			testTransaction.AH_TransactionType = transactionType;
			testTransaction.AH_InvoiceAmount = testTransaction.AH_OSTotal = amount;
			testTransaction.AH_PostDate = postDateTime.ToDateTime();
			testTransaction.AH_GB = GlbBranch.CurrentBranch.PK.ToGuid();
			testTransaction.AH_GE = GlbDepartment.CurrentDepartment.PK.ToGuid();
			testTransaction.AH_PostToGL = "Y";
		}

		void SetHeaderWithLines(TransactionLine testLinesRow, TransactionHeaderWithLines testTransaction, string lineType, Guid accountPK, decimal amount = 0m)
		{
			testLinesRow.AL_AH = testTransaction.PK;
			testLinesRow.AL_GB = testTransaction.AH_GB;
			testLinesRow.AL_GE = testTransaction.AH_GE;
			testLinesRow.AL_AG = accountPK;
			testLinesRow.AL_LineType = lineType;
			if (amount != 0)
			{
				testLinesRow.AL_LocalExTaxAmount = amount;
			}
		}

		void SetGLAccounts()
		{
			GLAccounts = new ArrayList();
			GLAccountDescription = new ArrayList();
			GLAccountNumber = new ArrayList();
			GLAccountType = new ArrayList();

			using (var reader = ((IDbConnected)Factory).Connection.Command("SELECT TOP 40 AG_PK, AG_Description, AG_AccountNum, AG_AccountType FROM dbo.AccGLHeader Order By AG_AccountNum").ExecuteReader())
			{
				while (reader.Read())
				{
					GLAccounts.Add(reader.GetGuid(0));
					GLAccountDescription.Add(reader.GetString(1));
					GLAccountNumber.Add(reader.GetString(2));
					GLAccountType.Add(reader.GetString(3));
				}
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
