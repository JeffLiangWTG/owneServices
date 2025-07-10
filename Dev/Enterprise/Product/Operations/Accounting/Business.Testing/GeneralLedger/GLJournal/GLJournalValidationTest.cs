using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	public class GLJournalValidationTest : TransactionHeaderWithLinesValidationTest
	{
		public void TestAH_OSExTaxAmountAuthorization()
		{
			var header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			var header2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.ReportSection;
			threshold.ReportSection = AccGLHeader.Constants.SectionTypes.Codes.TradingStatement;

			var settings1 = threshold.AuthorisationSettings.AddNew();
			settings1.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings1.Amount = 100.00m;
			settings1.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;

			var settings2 = threshold.AuthorisationSettings.AddNew();
			settings2.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings2.Amount = 200.00m;
			settings2.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;

			var settings3 = threshold.AuthorisationSettings.AddNew();
			settings3.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			settings3.Amount = 200.00m;
			settings3.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);

			Env.Security.GeneralLedgerJournal_SecondApproval.IsAllowed = false;
			Env.Security.GeneralLedgerJournal_ThirdApproval.IsAllowed = false;

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 60, DebitCredit.CR, header1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 70, DebitCredit.CR, header1.PK);
			var line3 = TestObjectCreator.CreateGLJournalLine(journal, 80, DebitCredit.CR, header2.PK);

			var expectedNoRightWarning = "You do not have rights to create a journal for this amount without authorization.";
			journal.RunPreSaveValidation();
			AssertHasWarning(journal.AH_OSExTaxAmountInfo, expectedNoRightWarning);

			line2.UnsignedLocalLineAmount = 30;
			AssertNoWarnings(journal.AH_OSExTaxAmountInfo);

			line2.AL_AG = header2.PK;
			AssertHasWarning(journal.AH_OSExTaxAmountInfo, expectedNoRightWarning);

			var registryInteranls = (IRegistryItemInternals)AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup;
			registryInteranls.DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			journal.RunPreSaveValidation();
			AssertNoWarnings(journal.AH_OSExTaxAmountInfo);
		}

		public void TestTransactionType()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA2";
			category.Description = (NoResString)"Category 2";
			category.Bool = true; // active
			category.Bool2 = true; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(1, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionCategory = category.Code;

			Assert("Precondition: Journal is elimination journal", journal.IsEliminationJournal);

			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			AssertNoError(journal.AH_TransactionTypeInfo, "Transaction Type must be General Journal when Presentation Category is of type Elimination Journal.");

			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			AssertHasError(journal.AH_TransactionTypeInfo, "Transaction Type must be General Journal when Presentation Category is of type Elimination Journal.");

			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			AssertHasError(journal.AH_TransactionTypeInfo, "Transaction Type must be General Journal when Presentation Category is of type Elimination Journal.");

			journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertHasError(journal.AH_TransactionTypeInfo, "Transaction Type must be General Journal when Presentation Category is of type Elimination Journal.");

			var glHeaderInvalidErrorMessage = "GL Account with 'BSH' and 'P&L' account types cannot be used for the creation of NTE journal. Please select another GL Account.";

			var glJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var glHeader = TestObjectCreator.CreateGLHeader();
			glHeader.AG_AccountType = AccountType.BalanceSheetAccount;

			var line = glJournal.GLJournalLines.AddNew();
			line.AL_AG = glHeader.PK;

			AssertNoError("Transaction Type have no errors", glJournal.AH_TransactionTypeInfo, glHeaderInvalidErrorMessage);

			glJournal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertHasError("Transaction Type have error when journal type is 'NJL' and account type is not 'NET'.", glJournal.AH_TransactionTypeInfo, glHeaderInvalidErrorMessage);

			glHeader.AG_AccountType = AccountType.Note;
			glJournal.Validation.ValidateAH_TransactionType();
			AssertNoError("Transaction Type have no errors", glJournal.AH_TransactionTypeInfo, glHeaderInvalidErrorMessage);
		}

		public void TestTransactionCategory()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "CA1";
			category1.Description = (NoResString)"Category 1";

			var category2 = list.AddNew();
			category2.Code = "CA2";
			category2.Description = (NoResString)"Category 2";
			category2.Bool = true; // active
			category2.Bool2 = true; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(2, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();

			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			journal.AH_TransactionCategory = category1.Code;
			AssertNoError(journal.AH_TransactionCategoryInfo, "Presentation Category can only be of Elimination Journal type when transaction type is General Journal.");

			journal.AH_TransactionCategory = category2.Code;
			AssertHasError(journal.AH_TransactionCategoryInfo, "Presentation Category can only be of Elimination Journal type when transaction type is General Journal.");

			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			journal.AH_TransactionCategory = category1.Code;
			AssertNoError(journal.AH_TransactionCategoryInfo, "Presentation Category can only be of Elimination Journal type when transaction type is General Journal.");

			journal.AH_TransactionCategory = category2.Code;
			AssertHasError(journal.AH_TransactionCategoryInfo, "Presentation Category can only be of Elimination Journal type when transaction type is General Journal.");

			journal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			journal.AH_TransactionCategory = category1.Code;
			AssertNoError(journal.AH_TransactionCategoryInfo, "Presentation Category can only be of Elimination Journal type when transaction type is General Journal.");

			journal.AH_TransactionCategory = category2.Code;
			AssertNoError(journal.AH_TransactionCategoryInfo, "Presentation Category can only be of Elimination Journal type when transaction type is General Journal.");
		}

		public void TestCheckDueDateDoesNotValidateRange()
		{
			GLJournal journal = GetGLJournalForTest();
			journal.AgePeriod = 100 * (ZDateTime.Today.Year + 7) + 6;

			AssertNoErrors("Due Date on Journal should have no errors", journal.AH_DueDateInfo);
		}

		public void TestCheckDueDateIsNotInValidPeriod()
		{
			var errorMessage = @"This date does not fall into a valid accounting period’s date range.
Please go to Manage > General Ledger > Period Management > Set Up Next Accounting Year, to ensure there is an accounting period for the date you wish to post to.";

			var yearDueDate = ZDateTime.Today.Year + 8;

			GLJournal journal = GetGLJournalForTest();
			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			AssertNoErrors("Due Date on Journal should have no errors", journal.AH_DueDateInfo);

			journal.AgePeriod = 100 * (yearDueDate) + 6;
			journal.AH_DueDate = new ZDateTime(yearDueDate, 6, 1);
			AssertHasErrors("Due Date on Journal should have errors", journal.AH_DueDateInfo);
			AssertHasError(journal.AH_DueDateInfo, errorMessage);
		}

		public void TestCheckPostDateDoesNotValidateRange()
		{
			GLJournal journal = GetGLJournalForTest();
			journal.PostPeriod = 100 * (ZDateTime.Today.Year + 7) + 6;

			AssertNoErrors("Post Date on Journal should have no errors", journal.AH_PostDateInfo);
		}

		public void TestOnlyValidateCategoryWhenNotInDatabase()
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category1 = list.AddNew();
			category1.Code = "CA1";
			category1.Description = (NoResString)"Category 1";
			var category2 = list.AddNew();
			category2.Code = "CA2";
			category2.Description = (NoResString)"Category 2";

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AssertEquals(2, AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Count);

			GLJournal journal = GetGLJournalForTest();
			journal.AH_TransactionCategory = "CAX";
			journal.Validation.ValidateAH_TransactionCategory();
			AssertHasErrors("Category should have list validation error", journal.AH_TransactionCategoryInfo);

			journal.AH_TransactionCategory = "CA1";
			journal.Validation.ValidateAH_TransactionCategory();
			AssertNoErrors("Category is valid - no error should be present", journal.AH_TransactionCategoryInfo);

			Factory.Save();

			category1.Bool = false;
			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			journal = factory.Load<GLJournal>(journal.PK);

			journal.Validation.ValidateAH_TransactionCategory();
			AssertNoErrors("Journal is already saved - no error should be present, even though the category is no longer defined in the registry.", journal.AH_TransactionCategoryInfo);
		}

		[TestDate(2009, 10, 06)]
		public void TestCheckPostPeriod()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			AccPeriodManagement period1 = helper.SetupSinglePeriod(200807, new ZDateTime(2008, 7, 1), new ZDateTime(2008, 7, 31));
			AccPeriodManagement period2 = helper.SetupSinglePeriod(200808, new ZDateTime(2008, 8, 1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			GLJournal journal = GetGLJournalForTest();
			journal.PostPeriod = period1.AM_Period;

			GLJournal journalCopy = (GLJournal)journal.TemplateCopy();
			AssertEquals("The PostPeriod on journal should be equal 200807", 200807, journal.PostPeriod);
			AssertEquals("The PostPeriod on journalCopy should be equal 200808", 200808, journalCopy.PostPeriod);

			GLJournalValidation validation = new GLJournalValidation(journalCopy);
			validation.ValidateAll();
			AssertNoErrors(journalCopy.PostPeriodInfo);

			period2.AM_IsGeneralLedgerClosed = true;
			validation.ValidateAll();
			AssertHasError(journalCopy.PostPeriodInfo, "The General Ledger is closed for 200808 period.  Only Presentation Journals can now be created. A Presentation Category must be assigned.");
			period2.AM_IsSubledgerClosedForAdjustments = true;
			validation.ValidateAll();
			AssertHasError(journalCopy.PostPeriodInfo, "200808 period is closed. You cannot post to closed periods.");

			journalCopy.AH_TransactionCategory = "FIN";
			validation.ValidateAll();
			AssertHasError(journalCopy.PostPeriodInfo, "200808 period is closed. You cannot post to closed periods.");

			period2.AM_IsGeneralLedgerClosed = false;
			validation.ValidateAll();
			AssertHasError(journalCopy.PostPeriodInfo, "Period 200808 is closed for Adjustments.  Only Regular Journals can now be created. A Presentation Category cannot be assigned.");

			period2.AM_IsSubledgerClosedForAdjustments = false;
			validation.ValidateAll();
			AssertNoErrors(journalCopy.PostPeriodInfo);
		}

		[TestDate(2009, 10, 06)]
		public void TestCheckAgePeriod()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			AccPeriodManagement period1 = helper.SetupSinglePeriod(200807, new ZDateTime(2008, 7, 1), new ZDateTime(2008, 7, 31));
			AccPeriodManagement period2 = helper.SetupSinglePeriod(200808, new ZDateTime(2008, 8, 1), new ZDateTime(2008, 8, 31));
			AccPeriodManagement period3 = helper.SetupSinglePeriod(200809, new ZDateTime(2008, 9, 1), ZDateTime.MaxSmallDateTime);
			Factory.Save();

			GLJournal journal = GetGLJournalForTest();
			journal.PostPeriod = period1.AM_Period;
			journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLReversingJournal;
			journal.AgePeriod = period2.AM_Period;
			GLJournalValidation validation = new GLJournalValidation(journal);
			validation.ValidateAll();
			AssertNoErrors(journal.AgePeriodInfo);

			period2.AM_IsGeneralLedgerClosed = true;
			validation.ValidateAgePeriod();
			AssertHasError(journal.AgePeriodInfo, "The General Ledger is closed for 200808 period.  Only Presentation Journals can now be created. A Presentation Category must be assigned.");

			period2.AM_IsSubledgerClosedForAdjustments = true;
			validation.ValidateAll();
			AssertHasError(journal.AgePeriodInfo, "200808 period is closed. You cannot post to closed periods.");

			journal.AH_TransactionCategory = "FIN";
			validation.ValidateAll();
			AssertHasError(journal.AgePeriodInfo, "200808 period is closed. You cannot post to closed periods.");

			period2.AM_IsGeneralLedgerClosed = false;
			validation.ValidateAll();
			AssertHasError(journal.AgePeriodInfo, "Period 200808 is closed for Adjustments.  Only Regular Journals can now be created. A Presentation Category cannot be assigned.");

			period2.AM_IsSubledgerClosedForAdjustments = false;
			validation.ValidateAll();
			AssertNoErrors(journal.PostPeriodInfo);

			journal.PostPeriod = period2.AM_Period;
			journal.AgePeriod = period1.AM_Period;
			validation.ValidateAgePeriod();
			AssertHasError(journal.AgePeriodInfo, "Reverse/Ending Period must be greater than post period");

			journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLAutoJournal;
			journal.PostPeriod = period1.AM_Period;
			journal.AgePeriod = period3.AM_Period;
			validation.ValidateAgePeriod();
			AssertNoErrors(journal.AgePeriodInfo);

			journal.AH_TransactionCategory = "";
			period2.AM_IsGeneralLedgerClosed = true;
			validation.ValidateAgePeriod();
			AssertHasError(journal.AgePeriodInfo, "The General Ledger is closed for 200808 period.  Only Presentation Journals can now be created. A Presentation Category must be assigned.");

			period2.AM_IsSubledgerClosedForAdjustments = true;
			validation.ValidateAll();
			AssertHasError(journal.AgePeriodInfo, "200808 period is closed. You cannot post to closed periods.");

			journal.AH_TransactionCategory = "FIN";
			validation.ValidateAll();
			AssertHasError(journal.AgePeriodInfo, "200808 period is closed. You cannot post to closed periods.");

			period2.AM_IsGeneralLedgerClosed = false;
			validation.ValidateAll();
			AssertHasError(journal.AgePeriodInfo, "Period 200808 is closed for Adjustments.  Only Regular Journals can now be created. A Presentation Category cannot be assigned.");

			period2.AM_IsSubledgerClosedForAdjustments = false;
			validation.ValidateAll();
			AssertNoErrors(journal.PostPeriodInfo);
		}

		public void TestGLJournalWithoutLines()
		{
			// Create Journal
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			// Assert it has no Lines
			AssertEquals("Journal should have no lines", 0, journal.Lines?.Count ?? 0);
			// Run
			journal.Validation.ValidateAll();
			// Assert has expected Row Error. It is a good idea to assert we have only one 😃
			var expectedError = "You cannot post a GL Journal without transaction lines. Please add at least two lines to have a zero balance amount.";
			//journal.RunPreSaveValidation();
			AssertEquals("GL Journal should have one row error.", 1, journal.RowNotifications.Count());
			AssertHasRowError("GL Journal should have this row error message.", journal, expectedError);

			// Add Lines
			var header1 = TestObjectCreator.CreateAccGLHeader("1111.11.11", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 1", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			var header2 = TestObjectCreator.CreateAccGLHeader("1111.11.22", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "ACCOUNT 2", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Credit);
			var line1 = TestObjectCreator.CreateGLJournalLine(journal, 60, DebitCredit.CR, header1.PK);
			var line2 = TestObjectCreator.CreateGLJournalLine(journal, 60, DebitCredit.DR, header2.PK);

			// Run journal.Validation.ValidateAll()
			journal.Validation.ValidateAll();

			//Assert has no Row Errors
			var notificationCount = journal.RowNotifications == null ? 0 : journal.RowNotifications.Count();
			AssertEquals("", 0, notificationCount);
		}

		public override void TestCheckAH_PostDate()
		{
			Assert("This functionality is overridden here and tested separately", true);
		}

		public override void TestCheckAH_PostDateForBackPosting()
		{
			Assert("This functionality is overridden here and tested separately", true);
		}

		public override void TestAH_IsCancelledBeingChangedByDataRefreshBusShowsError_WhenSkipDataRefreshBusUpdateRegsitryIsSetToAnyChange()
		{
			Assert("It is not applicable as reversing of journal does not cancel and so does not change original transaction, which is precondition for this test", true);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper();
			testHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 7);
		}

		GLJournal GetGLJournalForTest()
		{
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionType = ZArchitecture.Core.TransactionTypes.GLAutoJournal;
			journal.PostPeriod = (ZDateTime.Today.Year + 7) * 100 + 1;
			journal.AgePeriod = (ZDateTime.Today.Year + 7) * 100 + 12;

			AddLine(journal, 10M, DebitCredit.CR);
			AddLine(journal, 10M, DebitCredit.DR);

			return journal;
		}

		GLJournalLine AddLine(GLJournal journal, decimal absoluteAmount, DebitCredit debitOrCredit)
		{
			GLJournalLine newLine = journal.GLJournalLines.AddNew();
			newLine.UnsignedOSLineAmount = absoluteAmount;
			newLine.DebitCreditSign = debitOrCredit.ToString();
			return newLine;
		}

		protected override Type HeaderType => typeof(GLJournal);

		#endregion
	}
}
