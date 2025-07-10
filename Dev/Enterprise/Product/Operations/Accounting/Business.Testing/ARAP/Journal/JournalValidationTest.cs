using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Journal.Testing
{
	public abstract class JournalValidationTest : TransactionHeaderValidationTest
	{
		protected abstract Type GetExpectedType();

		public void TestCheckAH_TransactionCategory()
		{
			Journal journal = (Journal)Factory.NewWithValidTestData(GetExpectedType());

			journal.AH_TransactionCategory = "";
			AssertEquals(true, journal.AH_TransactionCategoryInfo.HasErrors());

			journal.AH_TransactionCategory = "XXX";
			AssertEquals(true, journal.AH_TransactionCategoryInfo.HasErrors());

			journal.AH_TransactionCategory = TransactionCategory.Codes.Standard;
			AssertEquals(false, journal.AH_TransactionCategoryInfo.HasErrors());

			var categoryValues = (journal.Lookups as JournalLookups).TransactionCategories.GetAllCodes();
			AssertCollectionNotContains($"{TransactionCategory.Codes.PaymentBasisWithholding} should not be available in Transaction Category list", TransactionCategory.Codes.PaymentBasisWithholding, categoryValues);
			AssertCollectionNotContains($"{TransactionCategory.Codes.ClearingJournal} should not be available in Transaction Category list", TransactionCategory.Codes.ClearingJournal, categoryValues);
			AssertCollectionNotContains($"{TransactionCategory.Codes.InstalmentJournal} should not be available in Transaction Category list", TransactionCategory.Codes.InstalmentJournal, categoryValues);

			journal.AH_TransactionCategory = TransactionCategory.Codes.PaymentBasisWithholding;
			AssertWhenTransactionCategoryIsPBW(journal.AH_TransactionCategoryInfo);

			journal.AH_TransactionCategory = TransactionCategory.Codes.ClearingJournal;
			AssertEquals(false, journal.AH_TransactionCategoryInfo.HasErrors());

			journal.AH_TransactionCategory = TransactionCategory.Codes.InstalmentJournal;
			AssertEquals(false, journal.AH_TransactionCategoryInfo.HasErrors());
		}

		protected abstract void AssertWhenTransactionCategoryIsPBW(ZPropertyInfo aH_TransactionCategoryInfo);

		public void TestDebitCreditSignWithAPJournal()
		{
			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(APJournal));

			const string InvalidInput = "AB";

			journal.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals(false, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = InvalidInput;
			AssertEquals(true, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals(false, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = InvalidInput;
			journal.RunPreSaveValidation();
			AssertEquals(true, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = "";
			AssertEquals(true, journal.DebitCreditSignInfo.HasErrors());
		}

		public void TestDebitCreditSignWithARJournal()
		{
			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(ARJournal));

			const string InvalidInput = "AB";

			journal.DebitCreditSign = DebitCreditDataEntry.DR;
			AssertEquals(false, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = InvalidInput;
			AssertEquals(true, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = DebitCreditDataEntry.CR;
			AssertEquals(false, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = InvalidInput;
			journal.RunPreSaveValidation();
			AssertEquals(true, journal.DebitCreditSignInfo.HasErrors());

			journal.DebitCreditSign = "";
			AssertEquals(true, journal.DebitCreditSignInfo.HasErrors());
		}

		public void TestCheckAH_OH_IsValid()
		{
			var journal = (Journal)Factory.New(HeaderType);
			var validation = new JournalValidation(journal);
			var validAccount = journal.AH_Ledger == LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			var invalidAccount = journal.AH_Ledger != LedgerTypes.AccountsReceivable ? TestObjectCreator.Debtor.PK : TestObjectCreator.Creditor1.PK;
			journal.AH_OH = invalidAccount;
			validation.ValidateAH_OH();
			AssertHasError(journal.AH_OHInfo, "Enter a valid Account.");
			journal.AH_OH = validAccount;
			validation.ValidateAH_OH();
			AssertNoErrors(journal.AH_OHInfo);
		}

		public void TestCheckAH_OHForAPJournal()
		{
			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(APJournal));

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = false;
			testOrg.OH_IsCreditor = false;

			string expectedError = "The Organization must have an Organization Type of Payables selected.";

			journal.AH_OH = testOrg.PK;
			AssertHasError(journal.AH_OHInfo, expectedError);

			testOrg.OH_IsDebtor = false;
			testOrg.OH_IsCreditor = true;
			journal.Validation.ValidateAH_OH();
			AssertNoError(journal.AH_OHInfo, expectedError);

			testOrg.OH_IsDebtor = true;
			testOrg.OH_IsCreditor = false;
			journal.Validation.ValidateAH_OH();
			AssertHasError(journal.AH_OHInfo, expectedError);

			testOrg.OH_IsDebtor = true;
			testOrg.OH_IsCreditor = true;
			journal.Validation.ValidateAH_OH();
			AssertNoError(journal.AH_OHInfo, expectedError);
		}

		public void TestCheckAH_OHForARJournal()
		{
			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(ARJournal));

			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsDebtor = false;
			testOrg.OH_IsCreditor = false;

			var expectedError = "The Organization must have an Organization Type of Receivables selected.";

			journal.AH_OH = testOrg.PK;
			AssertHasError(journal.AH_OHInfo, expectedError);

			testOrg.OH_IsDebtor = true;
			testOrg.OH_IsCreditor = false;
			journal.Validation.ValidateAH_OH();
			AssertNoError(journal.AH_OHInfo, expectedError);

			testOrg.OH_IsDebtor = false;
			testOrg.OH_IsCreditor = true;
			journal.Validation.ValidateAH_OH();
			AssertHasError(journal.AH_OHInfo, expectedError);

			testOrg.OH_IsDebtor = true;
			testOrg.OH_IsCreditor = true;
			journal.Validation.ValidateAH_OH();
			AssertNoError(journal.AH_OHInfo, expectedError);
		}

		public void TestCheckAH_AG_ForDisallowDirectPosting()
		{
			var creator = new TestObjectCreator(Factory);
			var glHeader1 = creator.CreateGLHeader();
			glHeader1.AG_DisallowDirectPosting = true;
			var glHeader2 = creator.CreateGLHeader();
			glHeader2.AG_DisallowDirectPosting = false;

			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(ARJournal));

			journal.AH_AG = glHeader1.PK;
			AssertHasError(journal.AH_AGInfo, "This GL Account is flagged to prevent direct posting. Please select a different GL Account.");

			journal.AH_AG = glHeader2.PK;
			AssertNoError(journal.AH_AGInfo, "This GL Account is flagged to prevent direct posting. Please select a different GL Account.");
		}

		public void TestCheckAH_AG_ForCashAdvanceJournal()
		{
			var creator = new TestObjectCreator(Factory);
			var glHeader1 = creator.CreateGLHeader();
			glHeader1.AG_DisallowDirectPosting = true;

			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(ARJournal));
			journal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.CashAdvanceReceived;
			journal.AH_AG = ZGuid.Empty;
			AssertHasError(journal.AH_AGInfo, "This Advance Payment can't be matched as there is no Advance Payment Clearing Account recorded in the Accounting -> Advance Payments -> Receivables -> Advance Payment Clearing Account registry. Please ensure this registry has a Advance Payment Clearing account recorded and then try the match again.");

			journal.AH_AG = glHeader1.PK;
			AssertNoError(journal.AH_AGInfo, "This Advance Payment can't be matched as there is no Advance Payment Clearing Account recorded in the Accounting -> Advance Payments -> Receivables -> Advance Payment Clearing Account registry. Please ensure this registry has a Advance Payment Clearing account recorded and then try the match again.");
		}

		public void TestCheckAH_AGBasedOnCompanyFilter()
		{
			var creator = new TestObjectCreator(Factory);
			var glHeader1 = creator.CreateGLHeader();
			glHeader1.AG_IsGlobal = true;
			var glHeader2 = creator.CreateGLHeader();
			glHeader2.AG_IsGlobal = false;
			var filterForGLHeader2 = glHeader2.CompanyFilters.AddNew();
			filterForGLHeader2.ACF_GC_Company = GlbCompany.CurrentCompany.PK;
			var glHeader3 = creator.CreateGLHeader();
			glHeader3.AG_IsGlobal = false;
			var filterForGLHeader3 = glHeader3.CompanyFilters.AddNew();
			filterForGLHeader3.ACF_GC_Company = creator.NonCurrentCompany.PK;

			Journal journal = (Journal)Factory.NewWithValidTestData(typeof(ARJournal));

			journal.AH_AG = glHeader1.PK;
			AssertNoErrors(journal.AH_AGInfo);

			journal.AH_AG = glHeader2.PK;
			AssertNoErrors(journal.AH_AGInfo);

			journal.AH_AG = glHeader3.PK;
			AssertHasError(journal.AH_AGInfo, "This GL Account cannot be used.");
		}

		public void TestBranchDepartmentCombinationValidation_ARJournal()
		{
			var arJournal = Factory.NewWithValidTestData<ARJournal>();
			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { arJournal.AH_GB = branch; arJournal.AH_GE = department; }, arJournal.AH_GEInfo);
		}

		public void TestBranchDepartmentCombinationValidation_APJournal()
		{
			var apJournal = Factory.NewWithValidTestData<APJournal>();
			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { apJournal.AH_GB = branch; apJournal.AH_GE = department; }, apJournal.AH_GEInfo);
		}

		public void TestBranchDepartmentCombinationValidation_GLJournal()
		{
			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObj(Factory,
				(branch, department) => { glJournal.AH_GB = branch; glJournal.AH_GE = department; }, glJournal.AH_GEInfo);
		}

		public void TestCheckAH_AG_ForSubAccounts_WithMultipleSubAccounts()
		{
			var journal = (Journal)Factory.NewWithValidTestData(HeaderType);

			journal.AH_AG = Guid.Empty;
			journal.Validation.ValidateAH_AG();
			Assert(journal.AH_AGInfo.HasError("Please enter a GL Account."));
			Assert(!journal.AH_AGInfo.Notifications.ContainsNotificationContaining("You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, false);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, false);
			journal.AH_AG = TestObjectCreator.GLHeader1.PK;
			journal.Validation.ValidateAH_AG();
			AssertEquals(2, journal.SubAccounts.Count);
			Assert(!journal.AH_AGInfo.HasError("Please enter a GL Account."));
			Assert(!journal.AH_AGInfo.Notifications.ContainsNotificationContaining("You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbStaffSchema.Constants.Prefix, false);
			journal.AH_AG = TestObjectCreator.GLHeader1.PK;
			journal.Validation.ValidateAH_AG();
			AssertEquals(3, journal.SubAccounts.Count);
			Assert(!journal.AH_AGInfo.HasWarning("This GL account has 3 sub account types. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

			journal.EnableCheckSubAccountsForGLHeader = true;
			journal.Validation.ValidateAH_AG();
			Assert(journal.AH_AGInfo.HasWarning("This GL account has 3 sub account types. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, GlbGroupSchema.Constants.Prefix, true);
			journal.AH_AG = TestObjectCreator.GLHeader1.PK;
			journal.Validation.ValidateAH_AG();
			AssertEquals(4, journal.SubAccounts.Count);
			Assert(journal.AH_AGInfo.HasError("This GL account has 4 sub account types. Some or all are mandatory. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));

			journal.SubAccounts.OfType<AccTransactionHeaderSubAccount>().First(x => x.AHS_SubClassParentTableCode == GlbGroupSchema.Constants.Prefix).AHS_SubClassParentId = TestObjectCreator.GG1.PK;
			journal.Validation.ValidateAH_AG();
			Assert(!journal.AH_AGInfo.HasError("This GL account has 4 sub account types. Some or all are mandatory. You can specify the additional sub accounts by selecting the journal, then click on the Edit menu."));
		}

		public void TestCheckSubAccountWithMultipleSubAccounts()
		{
			var journal = (Journal)Factory.NewWithValidTestData(HeaderType);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, OrgHeaderSchema.Constants.Prefix, true);
			TestObjectCreator.CreateGLHeaderSubAccount(TestObjectCreator.GLHeader1, AccGroupsSchema.Constants.Prefix, true);
			journal.EnableCheckSubAccountsForGLHeader = true;
			journal.AH_AG = TestObjectCreator.GLHeader1.PK;
			journal.Validation.ValidateAll();
			AssertEquals(2, journal.SubAccounts.Count);
			Assert(journal.AH_Calc_FirstSubClassParentIdInfo.HasError("Please enter a Sub Account 1."));
			Assert(journal.AH_Calc_SecondSubClassParentIdInfo.HasError("Please enter a Sub Account 2."));

			journal.AH_Calc_FirstSubClassParentId = TestObjectCreator.Creditor1.PK;
			journal.AH_Calc_SecondSubClassParentId = TestObjectCreator.AR1.PK;
			journal.Validation.ValidateAll();
			Assert(!journal.AH_Calc_FirstSubClassParentIdInfo.HasError("Please enter a Sub Account 1."));
			Assert(!journal.AH_Calc_SecondSubClassParentIdInfo.HasError("Please enter a Sub Account 2."));
		}
	}
}
