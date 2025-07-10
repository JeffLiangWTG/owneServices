using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.DataInterface;
using Enterprise.Accounting.Business.DataInterface.ChinaDataInterface;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.AccountingVoucherPrint
{
	[TestedType(typeof(ChinaJournalListingPrintWrapper))]
	public class ChinaJournalListingPrintWrapperTest : NonPersistentBusinessObjectTestCase
	{
		// New Test
		public void TestValidateFromPeriodReturnNoError()
		{
			AccPeriodManagement testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 200301;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.True;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			TestWrapper.FromPeriod = testPeriod.AM_Period;
			TestWrapper.ValidateFromPeriod();
			AssertNoErrors(TestWrapper.FromPeriodInfo);
		}

		//New Test
		public void TestPeriodEntered()
		{
			TestWrapper.ValidateFromPeriod();
			AssertHasErrorContaining(TestWrapper.FromPeriodInfo, "Please enter a value.");
		}

		public void TestValidateFromPeriodWhenPeriodIsClosed()
		{
			AccPeriodManagement testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 200301;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.True;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			TestWrapper.FromPeriod = testPeriod.AM_Period;
			TestWrapper.ValidateFromPeriod();
			Assert(!TestWrapper.FromPeriodInfo.HasError("This period is not closed."));
		}

		public void TestValidateFromPeriodWhenPeriodIsOpen()
		{
			AccPeriodManagement testPeriod = Factory.New(typeof(AccPeriodManagement)) as AccPeriodManagement;
			testPeriod.AM_Period = 200301;
			testPeriod.AM_IsGeneralLedgerClosed = ZBool.False;
			testPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			TestWrapper.FromPeriod = testPeriod.AM_Period;
			TestWrapper.ValidateFromPeriod();
			AssertHasError(TestWrapper.FromPeriodInfo, "This period is not closed.");
		}

		//New Test
		public void TestFromDateCheckValidZDateTimeWithoutRange()
		{
			TestWrapper.FromDate = ZDateTime.Invalid;
			TestWrapper.ValidateFromDate();
			AssertHasErrorContaining(TestWrapper.FromDateInfo, "Enter a valid selection.");
		}

		// New Test
		public void TestFromDateCheckValidZDateTimeRange()
		{
			ZDateTime tenYearsEarlier = new ZDateTime(ZDateTime.Now.AddYears(-10));
			TestWrapper.FromDate = tenYearsEarlier;
			TestWrapper.ValidateFromDate();
			AssertHasWarnings(TestWrapper.FromDateInfo);
		}

		// New Test
		public void TestFromDateCheckEntered()
		{
			TestWrapper.ValidateFromDate();
			AssertHasErrorContaining(TestWrapper.FromDateInfo, "Please enter a value.");
		}

		public void TestValidateFromDateIfFromPeriodSetForVoucher()
		{
			SetupPeriods();
			TestWrapper.FromPeriod = 200402;
			TestWrapper.FromDate = new ZDateTime(2004, 1, 15);
			TestWrapper.ValidateFromDate();
			AssertHasError(TestWrapper.FromDateInfo, "From Date must be within the Period specified.");
			TestWrapper.FromDate = new ZDateTime(2004, 2, 15);
			TestWrapper.ValidateFromDate();
			AssertNoError(TestWrapper.FromDateInfo, "From Date must be within the Period specified.");
			TestWrapper.FromDate = new ZDateTime(2004, 3, 15);
			TestWrapper.ValidateFromDate();
			AssertHasError(TestWrapper.FromDateInfo, "From Date must be within the Period specified.");
		}

		//New Test
		public void TestEndDateCheckValidZDateTimeWithoutRange()
		{
			TestWrapper.EndDate = ZDateTime.Invalid;
			TestWrapper.ValidateEndDate();
			AssertHasErrorContaining(TestWrapper.EndDateInfo, "Enter a valid selection.");
		}

		// New Test
		public void TestEndDateCheckValidZDateTimeRange()
		{
			ZDateTime tenYearsEarlier = new ZDateTime(ZDateTime.Now.AddYears(-10));
			TestWrapper.EndDate = tenYearsEarlier;
			TestWrapper.ValidateEndDate();
			AssertHasWarnings(TestWrapper.EndDateInfo);
		}

		// New Test
		public void TestEndDateCheckEntered()
		{
			TestWrapper.ValidateEndDate();
			AssertHasErrorContaining(TestWrapper.EndDateInfo, "Please enter a value.");
		}

		public void TestValidateToDateIfFromPeriodSetForVoucher()
		{
			SetupPeriods();
			TestWrapper.FromPeriod = 200402;
			TestWrapper.EndDate = new ZDateTime(2004, 1, 15);
			TestWrapper.ValidateEndDate();
			AssertHasError(TestWrapper.EndDateInfo, "End Date must be within the Period specified.");
			TestWrapper.EndDate = new ZDateTime(2004, 2, 15);
			TestWrapper.ValidateEndDate();
			AssertNoError(TestWrapper.EndDateInfo, "End Date must be within the Period specified.");
			TestWrapper.EndDate = new ZDateTime(2004, 3, 15);
			TestWrapper.ValidateEndDate();
			AssertHasError(TestWrapper.EndDateInfo, "End Date must be within the Period specified.");
		}

		public void TestDocWrapperGeneratedWithFilterSetToARAndRec()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper();
			AssertEquals(1, wrapper.Length);
		}

		public void TestGetWrapperCount()
		{
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			AssertEquals(1, TestWrapper.GetWrapperCount());
		}

		[ExpectNoExceptions]
		public void TestPrintDocuments()
		{
			SetupTransaction();
			var mockTestWrapper = new Mock<ChinaJournalListingPrintWrapper>(Factory) { CallBase = true };
			ChinaJournalListingPrintWrapper testPrintWrapper = mockTestWrapper.Object;
			testPrintWrapper.FromPeriod = TestPeriod;
			mockTestWrapper.Protected().Setup("PrintCore");
			testPrintWrapper.GenerateDocWrapper();
			testPrintWrapper.PrintChinaJournalListingDocument();
			mockTestWrapper.VerifyAll();
		}

		public void TestSelectBranchShouldShowWarning()
		{
			var branch = Factory.Load<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK)).FirstOrDefault();
			AssertNotNull(branch);
			SetupTransaction();
			TestWrapper.FromPeriod = TestPeriod;
			TestWrapper.Branch = branch.PK;
			AssertHasWarning(TestWrapper.BranchInfo, "You cannot select Branch Filter if you are printing Job Costing Voucher.");
			TestWrapper.Branch = ZGuid.Empty;
			AssertNoWarnings(TestWrapper.BranchInfo);
		}

		public void TestBranchFilter()
		{
			TestPeriod = 200401;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			APInvoice testInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2004, 1, 15);
			testInvoice.AH_TransactionNum = "TEST012";
			testInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
			APInvoice testAPInvoice = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvoice.AH_PostDate = new ZDateTime(2004, 1, 15);
			testAPInvoice.AH_GB = NonCurrentBranch.PK;
			testAPInvoice.AH_TransactionNum = "TEST022";
			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_PostDate = new ZDateTime(2004, 1, 5);
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment.AH_TransactionNum = "TEST032";
			Factory.Save();
			TestWrapper.Branch = GlbBranch.CurrentBranch.PK;
			TestWrapper.FromDate = new ZDateTime(2004, 1, 1);
			TestWrapper.EndDate = new ZDateTime(2004, 1, 31);
			DocumentWrapper[] wrapper = TestWrapper.GenerateDocWrapper();
			AssertEquals(1, wrapper.Length);
		}

		public void TestWIPAccrualVoucher()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestPeriod = 200401;
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));

				var testObjectCreator = new TestObjectCreator(Factory);
				var accrual = testObjectCreator.CreateAccrual();
				accrual.AL_PostDate = new ZDateTime(2004, 01, 31, 23, 59, 59);
				testObjectCreator.CreateAccountDescriptor(accrual.AL_AG, "1000000.1", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "Desc", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				testObjectCreator.CreateAccountDescriptor(AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value, "88.88.888.8", AccGLAccountDescriptor.ReportTypeCOA, "", DataInterfaceUtils.GetLocalLanguage(), "CostControlDescription", Constants.CountryCodes.China, Constants.DebitCredit.Debit);
				Factory.Save();

				TestWrapper.FromDate = new ZDateTime(2004, 1, 1);
				TestWrapper.EndDate = new ZDateTime(2004, 1, 31);
				DocumentWrapper[] wrappers = TestWrapper.GenerateDocWrapper();
				AssertEquals(1, wrappers.Length);

				var chinaJournalListing = wrappers[0].WrappedObject as ChinaJournalListing;
				AssertEquals(2, chinaJournalListing.AccountingVouchers.Count);
			}
		}

		public void TestGLJournalVoucher()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestPeriod = 200401;
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));

				var testObjectCreator = new TestObjectCreator(Factory);
				var testGLJournal = testObjectCreator.CreateGLJournal(TransactionTypes.GLStandardJournal, new ZDateTime(2004, 1, 5), new ZDateTime(2004, 1, 5));
				testObjectCreator.CreateGLJournalLine(testGLJournal, 250m, DebitCredit.DR, testObjectCreator.GLHeader1.PK);
				testObjectCreator.CreateGLJournalLine(testGLJournal, 250m, DebitCredit.CR, testObjectCreator.GLHeader2.PK);
				Factory.Save();

				TestWrapper.FromDate = new ZDateTime(2004, 1, 1);
				TestWrapper.EndDate = new ZDateTime(2004, 1, 31);
				DocumentWrapper[] wrappers = TestWrapper.GenerateDocWrapper();
				AssertEquals(1, wrappers.Length);

				var chinaJournalListing = wrappers[0].WrappedObject as ChinaJournalListing;
				AssertEquals(2, chinaJournalListing.AccountingVouchers.Count);
			}
		}

		public void TestJobRevenueJournalVoucher()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var jobRevenueJournalControlAccount = testObjectCreator.CreateAccGLHeader("6555.55.55", "AS", "Job Revenue Journal Control Account", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid()))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestPeriod = 200401;
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));

				Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
				var testJobRevenueJournal = testObjectCreator.CreateJobRevenueJournal(typeof(JobRevenueJournal), testObjectCreator.CC1, job.PK, 100);
				testJobRevenueJournal.AH_PostDate = new ZDateTime(2004, 1, 1);
				Factory.Save();

				TestWrapper.FromDate = new ZDateTime(2004, 1, 1);
				TestWrapper.EndDate = new ZDateTime(2004, 1, 31);
				DocumentWrapper[] wrappers = TestWrapper.GenerateDocWrapper();
				AssertEquals(1, wrappers.Length);

				var chinaJournalListing = wrappers[0].WrappedObject as ChinaJournalListing;
				AssertEquals(4, chinaJournalListing.AccountingVouchers.Count);
			}
		}

		public void TestBankTransferVoucher()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				TestPeriod = 200401;
				AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
				testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));

				var newFactory = Factory.CreateNewFactory();
				var testObjectCreator = new TestObjectCreator(newFactory);
				var bankTransfer1 = testObjectCreator.CreateBankTransfer(new ZDateTime(2004, 1, 15), testObjectCreator.CHNBankAccount.PK, testObjectCreator.CHNBankAccount2.PK, 2500m, 1.0m);
				var bankTransfer2 = testObjectCreator.CreateBankTransfer(new ZDateTime(2004, 1, 15), testObjectCreator.CHNBankAccount.PK, testObjectCreator.CHNBankAccount2.PK, 2500m, 1.0m);
				bankTransfer2.TransferRowFrom.AH_TransactionCount = 5;
				bankTransfer2.TransferRowTo.AH_TransactionCount = 4;
				newFactory.Save();

				TestWrapper.FromDate = new ZDateTime(2004, 1, 1);
				TestWrapper.EndDate = new ZDateTime(2004, 1, 31);
				DocumentWrapper[] wrappers = TestWrapper.GenerateDocWrapper();
				AssertEquals(1, wrappers.Length);

				var chinaJournalListing = wrappers[0].WrappedObject as ChinaJournalListing;
				AssertEquals(4, chinaJournalListing.AccountingVouchers.Count);
			}
		}

		void SetupPeriods()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			testHelper.SetupSinglePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 28));
			AccPeriodManagement period200402 = Factory.LoadTop1(typeof(AccPeriodManagement), new ZQuery(AccPeriodManagementSchema.AM_Period, 200401)) as AccPeriodManagement;
			period200402.AM_IsGeneralLedgerClosed = ZBool.True;
			period200402.AM_IsSubLedgerClosed = ZBool.True;
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ChinaJournalListingPrintWrapper(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			fTestWrapper = null;
		}

		int TestPeriod;
		ChinaJournalListingPrintWrapper fTestWrapper;
		ChinaJournalListingPrintWrapper TestWrapper
		{
			get
			{
				if (fTestWrapper == null)
				{
					fTestWrapper = new ChinaJournalListingPrintWrapper(Factory);
				}

				return fTestWrapper;
			}
		}

		void SetupTransaction()
		{
			TestPeriod = 200401;
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(TestPeriod, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			ARInvoice testInvoice = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testInvoice.AH_PostDate = new ZDateTime(2004, 1, 15);
			testInvoice.AH_TransactionNum = "TEST012";
			APInvoice testAPInvocie = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie.AH_PostDate = new ZDateTime(2004, 1, 15);
			testAPInvocie.AH_TransactionNum = "TEST022";
			APPayment testPayment = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment.AH_PostDate = new ZDateTime(2004, 1, 5);
			testPayment.AH_TransactionNum = "TES45i7";
			ARReceipt testARReceipt = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			testARReceipt.AH_PostDate = new ZDateTime(2004, 1, 5);
			testARReceipt.AH_TransactionNum = "TES3452i7";
			DirectReceipt testDirectReceipt = Factory.NewWithValidTestData(typeof(DirectReceipt)) as DirectReceipt;
			testDirectReceipt.AH_PostDate = new ZDateTime(2004, 1, 5);
			testDirectReceipt.AH_TransactionNum = "TES345ew2i7";
			AccTransactionHeader testRCB = Factory.NewWithValidTestData(typeof(AccTransactionHeader)) as AccTransactionHeader;
			testRCB.AH_GB = GlbBranch.CurrentBranch.PK;
			testRCB.AH_GE = GlbDepartment.CurrentDepartment.PK;
			testRCB.AH_TransactionType = TransactionTypes.ReceiptBatch;
			testRCB.AH_Ledger = LedgerTypes.CashBook;
			testRCB.AH_PostDate = new ZDateTime(2004, 1, 5);
			ARInvoice testInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			testInvoice2.AH_PostDate = new ZDateTime(2003, 1, 15);
			testInvoice2.AH_TransactionNum = "TEST012a";
			APInvoice testAPInvocie2 = Factory.NewWithValidTestData(typeof(APInvoice)) as APInvoice;
			testAPInvocie2.AH_PostDate = new ZDateTime(2005, 1, 15);
			testAPInvocie2.AH_TransactionNum = "TEST022a";
			APPayment testPayment2 = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			testPayment2.AH_PostDate = new ZDateTime(2003, 1, 5);
			testPayment2.AH_TransactionNum = "TES45i7a";
			ARReceipt testARReceipt2 = Factory.NewWithValidTestData(typeof(ARReceipt)) as ARReceipt;
			testARReceipt2.AH_PostDate = new ZDateTime(2005, 1, 5);
			testARReceipt2.AH_TransactionNum = "TES3452i7a";
			Factory.Save();
		}

		OrgHeader fFromAccount;
		protected OrgHeader FromAccount
		{
			get
			{
				if (fFromAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery fromAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					fromAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(fromAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					query.AddToFilter(OrgHeaderSchema.PK, ToAccount.PK);
					fFromAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fFromAccount;
			}
		}

		OrgHeader fToAccount;
		protected OrgHeader ToAccount
		{
			get
			{
				if (fToAccount == null)
				{
					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
					ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
					ZQuery toAccountFilter = new ZQuery(OrgCompanyDataSchema.OB_IsCreditor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_IsDebtor, ZBool.True);
					toAccountFilter.AddToFilter(OrgCompanyDataSchema.OB_GC, GlbCompany.CurrentCompany.PK);
					subQuery.AddToFilter(toAccountFilter);
					query.AddSubQuery(subQuery, JoinCondition.And);
					fToAccount = Factory.LoadTop1<OrgHeader>(query);
				}

				return fToAccount;
			}
		}

		GlbBranch fNonCurrentBranch;
		protected GlbBranch NonCurrentBranch
		{
			get
			{
				if (fNonCurrentBranch == null)
				{
					fNonCurrentBranch = Factory.LoadTop1(typeof(GlbBranch), new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK)) as GlbBranch;
				}

				return fNonCurrentBranch;
			}
		}
	}
}
