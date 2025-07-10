using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLConsolidations;
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
	[TestedType(typeof(GLJournalLine))]
	public class GLJournalLineValidationTest : DependentTransactionLineValidationTest
	{
		public void TestUnsignedLocalLineAmountAuthorization()
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

			var expectedNoRightWarning = "The total amount on all lines for this GL account requires approval on posting because it exceeds the registry defined approval threshold, or the registry has been set as 'ANY' that requires all journals to be approved.";
			var expectedHasRightWarning = "This line will be automatically approved when you post because you already have the necessary authorization security right.";

			AssertUnsignedLocalLineAmountAuthorization(TransactionTypes.GLStandardJournal);
			AssertUnsignedLocalLineAmountAuthorization(TransactionTypes.GLNoteJournal);

			var registryInteranls = (IRegistryItemInternals)AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup;
			registryInteranls.DeleteValue(Guid.Empty, Guid.Empty, Guid.Empty);
			journal.RunPreSaveValidation();
			AssertNoWarnings(line1.UnsignedLocalLineAmountInfo);
			AssertNoWarnings(line2.UnsignedLocalLineAmountInfo);
			AssertNoWarnings(line3.UnsignedLocalLineAmountInfo);

			void AssertUnsignedLocalLineAmountAuthorization(ZString transactionType)
			{
				journal.AH_TransactionType = transactionType;
				journal.RunPreSaveValidation();
				if (journal.IsNoteJournal)
				{
					AssertNoWarning(line1.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertNoWarning(line2.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertNoWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}
				else
				{
					AssertHasWarning(line1.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertHasWarning(line2.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertHasWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}

				line2.UnsignedLocalLineAmount = 30;
				if (journal.IsNoteJournal)
				{
					AssertNoWarning(line1.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertNoWarning(line2.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertNoWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}
				else
				{
					AssertHasWarning(line1.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertHasWarning(line2.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertHasWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}

				var testValidator = new GLJournalLineValidation(line1);
				testValidator.ValidateUnsignedLocalLineAmount();
				if (journal.IsNoteJournal)
				{
					AssertNoWarning(line1.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertNoWarning(line2.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertNoWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}
				else
				{
					AssertHasWarning(line1.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertHasWarning(line2.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertHasWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}

				line2.AL_AG = header2.PK;
				if (journal.IsNoteJournal)
				{
					AssertNoWarning(line1.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertNoWarning(line2.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertNoWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}
				else
				{
					AssertHasWarning(line1.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertHasWarning(line2.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertHasWarning(line3.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
				}

				testValidator = new GLJournalLineValidation(line3);
				testValidator.ValidateUnsignedLocalLineAmount();

				if (journal.IsNoteJournal)
				{
					AssertNoWarning(line1.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertNoWarning(line2.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertNoWarning(line3.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
				}
				else
				{
					AssertHasWarning(line1.UnsignedLocalLineAmountInfo, expectedHasRightWarning);
					AssertHasWarning(line2.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
					AssertHasWarning(line3.UnsignedLocalLineAmountInfo, expectedNoRightWarning);
				}
			}
		}

		public void TestCheckAL_OH_WithInvalidGuid()
		{
			// In order to get to the line I want, the validator must be tested with a line attached to an elimination journal
			GLJournalLine testLine = Factory.NewWithValidTestData<GLJournalLine>();
			testLine.AL_OH = ZGuid.Invalid;
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();

			journal.Lines.Add(testLine);
			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = "CA2";
			category.Description = (NoResString)"Category 2";
			category.Bool = true; // active
			category.Bool2 = true; // elimination

			AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			journal.AH_TransactionCategory = category.Code;

			GLJournalLineValidation testValidator = new GLJournalLineValidation(testLine);
			testValidator.ValidateAll();

			Assert(testLine.AL_OHInfo.HasErrors());
		}

		public override void TestCheckAL_SupplyType()
		{
			foreach (var regValue in new[] { true, false })
			{
				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var journalLine = Factory.NewWithValidTestData<GLJournalLine>();
					journalLine.AL_SupplyType = string.Empty;
					journalLine.RunPreSaveValidation();
					AssertNoErrors(journalLine.AL_SupplyTypeInfo);

					journalLine.AL_SupplyType = "XXX";
					journalLine.RunPreSaveValidation();
					AssertNoErrors("invalid supply type value should not throw a validation error", journalLine.AL_SupplyTypeInfo);
				}
			}
		}

		[TestDate(2023, 3, 6)]
		public void TestAL_PostDateAllowFutureDatesMoreThanFiveYears()
		{
			var testDate = ZDateTime.Now.AddYears(6);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(testDate.Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var line = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as GLJournalLine;
			line.AL_PostDateInfo.Value = testDate;
			AssertNoErrors("Reverse Date on Line should have no errors for Future Dates", line.AL_PostDateInfo);
		}

		[TestDate(2023, 3, 6)]
		public void TestAL_ReverseDateAllowFutureDatesMoreThanFiveYears()
		{
			var testDate = ZDateTime.Now.AddYears(6);
			new AccountingPeriodTestHelper(Factory).PostPeriodsForEntireYear(testDate.Year, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);

			var line = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as GLJournalLine;
			line.AL_ReverseDateInfo.Value = testDate;
			AssertNoErrors("Reverse Date on Line should have no errors for Future Dates", line.AL_ReverseDateInfo);
		}

		public void TestGLAccountValidation()
		{
			GLJournalLine testLine = Factory.NewWithValidTestData<GLJournalLine>();
			testLine.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AccGLHeader testGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLHeader.AG_IsActive = true;

			testLine.AL_AG = testGLHeader.PK;
			AssertEquals(false, testLine.AL_AGInfo.HasErrors());

			testGLHeader.AG_IsActive = false;
			testLine.AL_AG = testGLHeader.PK;
			AssertEquals(true, testLine.AL_AGInfo.HasError(((TransactionLineValidation)testLine.Validation).InactiveGLAccountError));

			testGLHeader.AG_AccountType = AccountTypeComboBoxConstants.Total;
			testLine.AL_AG = testGLHeader.PK;
			AssertHasError(testLine.AL_AGInfo, "For GJL, RJL and AJL journal types, you can only post to Balance Sheet or Profit & Loss Accounts.");
		}

		public void TestGLAccountValidationForForeignCurrency()
		{
			GLJournalLineHelper.ControlOrLinkAccountsForTest = null;
			var plAppropriationAccount = Factory.NewWithValidTestData<AccGLHeader>();
			plAppropriationAccount.AG_AccountNum = "9999.99.99";
			plAppropriationAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plAppropriationAccount.PK.ToGuid());

			var controlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			controlAccount.AG_AccountNum = "9999.99.90";
			controlAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			controlAccount.AG_ControlAccount = true;

			var balanceSheetAccount = Factory.NewWithValidTestData<AccGLHeader>();
			balanceSheetAccount.AG_AccountNum = "9999.99.00";
			balanceSheetAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			balanceSheetAccount.AG_ControlAccount = false;

			var hdrAccount = Factory.NewWithValidTestData<AccGLHeader>();
			hdrAccount.AG_AccountNum = "9999.00.00";
			hdrAccount.AG_AccountType = Core.Constants.AccountType.Header;

			var linkAccount = Factory.NewWithValidTestData<AccGLHeader>();
			linkAccount.AG_AccountNum = "9900.00.00";
			linkAccount.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, linkAccount.PK.ToGuid());

			var pnlAccount = Factory.NewWithValidTestData<AccGLHeader>();
			pnlAccount.AG_AccountNum = "9000.00.00";
			pnlAccount.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;
			pnlAccount.AG_ControlAccount = false;

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var testLine = Factory.NewWithValidTestData<GLJournalLine>();
			testLine.AL_RX_NKTransactionCurrency = "EUR";

			testLine.AL_AG = plAppropriationAccount.PK;
			AssertHasError(testLine.AL_AGInfo, "For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry");

			//Reset error
			testLine.AL_AG = balanceSheetAccount.PK;
			AssertNoError(testLine.AL_AGInfo, "For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry");

			testLine.AL_AG = linkAccount.PK;
			AssertHasError(testLine.AL_AGInfo, "For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry");

			//Reset error
			testLine.AL_AG = balanceSheetAccount.PK;
			AssertNoError(testLine.AL_AGInfo, "For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry");

			testLine.AL_AG = controlAccount.PK;
			AssertHasError(testLine.AL_AGInfo, "For a foreign currency line you can not post to a control account or any account that is configured as PL Appropriation, Control or Link Account in the Registry");

			testLine.AL_AG = hdrAccount.PK;

			AssertHasError(testLine.AL_AGInfo, "For a foreign currency line you can only post to a Balance Sheet or Profit & Loss account");

			testLine.AL_AG = balanceSheetAccount.PK;

			AssertNoError(testLine.AL_AGInfo, "For a foreign currency line you can only post to a Balance Sheet or Profit & Loss account");

			testLine.AL_AG = pnlAccount.PK;
			AssertNoError(testLine.AL_AGInfo, "For a foreign currency line you can only post to a Balance Sheet or Profit & Loss account");

			var fCBAdjustmentLine = Factory.NewWithValidTestData<FCBAdjustmentJournalLine>();
			fCBAdjustmentLine.AL_RX_NKTransactionCurrency = "EUR";
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, plAppropriationAccount.PK.ToGuid());

			fCBAdjustmentLine.AL_AG = balanceSheetAccount.PK;
			AssertNoErrors(fCBAdjustmentLine.AL_AGInfo);

			fCBAdjustmentLine.AL_AG = pnlAccount.PK;
			AssertHasError(fCBAdjustmentLine.AL_AGInfo, "For a foreign currency line you can only post to a Balance Sheet account");
		}

		public void TestInvalidGLAccount()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertInvalidGLAccountForNJL();
				AssertInvalidGLAccountForGJLRJLAJL();
				AssertInvalidGLAccountForFCB();

				void AssertInvalidGLAccountForNJL()
				{
					var header = TestObjectCreator.CreateGLHeader();
					var glNoteJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
					var glNoteJournalline = glNoteJournal.GLJournalLines.AddNew();
					AssertEquals("Pre-condition", "AUD", glNoteJournalline.AL_RX_NKTransactionCurrency);

					header.AG_AccountType = AccountType.BalanceSheetAccount;
					glNoteJournalline.AL_AG = header.PK;
					AssertHasError(glNoteJournalline.AL_AGInfo, "For NJL journal type, you can only post to Note Accounts.");

					header.AG_AccountType = AccountType.Note;
					glNoteJournalline.Validation.ValidateAL_AG();
					AssertNoError(glNoteJournalline.AL_AGInfo, "For NJL journal type, you can only post to Note Accounts.");
				}

				void AssertInvalidGLAccountForGJLRJLAJL()
				{
					var header = TestObjectCreator.CreateGLHeader();
					var glStandardJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
					var glJournalLine = glStandardJournal.GLJournalLines.AddNew();
					glJournalLine.AL_AG = header.PK;

					glJournalLine.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
					AssertInvalidGLAccount(glJournalLine);

					glStandardJournal.AH_TransactionType = TransactionTypes.GLAutoJournal;
					AssertInvalidGLAccount(glJournalLine);

					glStandardJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
					AssertInvalidGLAccount(glJournalLine);

					glJournalLine.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.China;
					glStandardJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
					AssertInvalidGLAccount(glJournalLine);

					glStandardJournal.AH_TransactionType = TransactionTypes.GLAutoJournal;
					AssertInvalidGLAccount(glJournalLine);

					glStandardJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
					AssertInvalidGLAccount(glJournalLine);

					void AssertInvalidGLAccount(GLJournalLine line)
					{
						var expectedAccountTypeIsNotNTEOrBSHOrPLErrorMessage = line.AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.LocalCurrency.Code ?
							"For GJL, RJL and AJL journal types, you can only post to Balance Sheet or Profit & Loss Accounts." :
							"For a foreign currency line you can only post to a Balance Sheet or Profit & Loss account";

						var expectedAccountTypeIsNoteErrorMessage = "GL Account with 'NTE' account type cannot be used for the creation of GJL, RJL and AJL journal. Please select another GL Account.";

						header.AG_AccountType = AccountType.Note;
						line.Validation.ValidateAL_AG();
						AssertHasError(line.AL_AGInfo, expectedAccountTypeIsNoteErrorMessage);
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNotNTEOrBSHOrPLErrorMessage);

						header.AG_AccountType = AccountType.Alternate;
						line.Validation.ValidateAL_AG();
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNoteErrorMessage);
						AssertHasError(line.AL_AGInfo, expectedAccountTypeIsNotNTEOrBSHOrPLErrorMessage);

						header.AG_AccountType = AccountType.BalanceSheetAccount;
						line.Validation.ValidateAL_AG();
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNoteErrorMessage);
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNotNTEOrBSHOrPLErrorMessage);
					}
				}

				void AssertInvalidGLAccountForFCB()
				{
					var header = TestObjectCreator.CreateGLHeader();
					var fcbJournal = TestObjectCreator.CreateFCBJournal(ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
					var fcbJournalLine = fcbJournal.GLJournalLines.AddNew();
					fcbJournalLine.AL_AG = header.PK;

					fcbJournalLine.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Australia;
					AssertInvalidGLAccount(fcbJournalLine);

					fcbJournalLine.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.China;
					AssertInvalidGLAccount(fcbJournalLine);

					void AssertInvalidGLAccount(GLJournalLine line)
					{
						var expectedAccountTypeIsNotNTEOrBSHErrorMessage = line.AL_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.LocalCurrency.Code ?
							"You can only post to Balance Sheet or Profit & Loss Accounts" :
							"For a foreign currency line you can only post to a Balance Sheet account";

						var expectedAccountTypeIsNoteErrorMessage = "GL Account with 'NTE' account type cannot be used for the creation of GJL, RJL and AJL journal. Please select another GL Account.";

						header.AG_AccountType = AccountType.Note;
						line.Validation.ValidateAL_AG();
						AssertHasError(line.AL_AGInfo, expectedAccountTypeIsNoteErrorMessage);
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNotNTEOrBSHErrorMessage);

						header.AG_AccountType = AccountType.Alternate;
						line.Validation.ValidateAL_AG();
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNoteErrorMessage);
						AssertHasError(line.AL_AGInfo, expectedAccountTypeIsNotNTEOrBSHErrorMessage);

						header.AG_AccountType = AccountType.BalanceSheetAccount;
						line.Validation.ValidateAL_AG();
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNoteErrorMessage);
						AssertNoError(line.AL_AGInfo, expectedAccountTypeIsNotNTEOrBSHErrorMessage);
					}
				}
			}
		}

		public void TestAL_ExchangeRateValidation()
		{
			var testLine = Factory.NewWithValidTestData<GLJournalLine>();
			testLine.AL_ExchangeRate = -1M;

			testLine.Validation.ValidateAL_ExchangeRate();

			AssertHasError(testLine.AL_ExchangeRateInfo, "Exchange Rate cannot be negative.");

			testLine.AL_ExchangeRate = 1M;

			testLine.Validation.ValidateAL_ExchangeRate();

			AssertNoError(testLine.AL_ExchangeRateInfo, "Exchange Rate cannot be negative.");

			testLine.AL_ExchangeRate = 0M;

			testLine.Validation.ValidateAL_ExchangeRate();

			AssertHasError(testLine.AL_ExchangeRateInfo, "Exchange Rate cannot be zero.");
			AssertHasWarning(testLine.AL_ExchangeRateInfo, "PER - Period End Rate exchange rate not found.");

			testLine.AL_ExchangeRate = 1.90M;

			testLine.Validation.ValidateAL_ExchangeRate();

			AssertNoError(testLine.AL_ExchangeRateInfo, "Exchange Rate cannot be zero.");
			AssertNoWarning(testLine.AL_ExchangeRateInfo, "PER - Period End Rate exchange rate not found.");
		}

		public void TestControlAccountValidation()
		{
			GLJournalLine testLine = Factory.NewWithValidTestData<GLJournalLine>();
			AccGLHeader testGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader.AG_AccountType = AccountTypeComboBoxConstants.BalanceSheetAccount;
			testGLHeader.AG_IsActive = true;
			Env.Security.GeneralLedgerJournalPostToControlAccounts.IsAllowed = false;

			testLine.AL_AG = testGLHeader.PK;
			AssertNoError("Shouldn't be any errors",
				testLine.AL_AGInfo,
				string.Format("This account is flagged as a control account. You do not have security rights to post to control accounts.\r\n{0}", Env.Security.GeneralLedgerJournalPostToControlAccounts.ErrorMessageForNotAllowed));

			testGLHeader.AG_ControlAccount = true;
			testLine.Validation.ValidateAL_AG();
			AssertHasError("Shouldn't be any errors",
				testLine.AL_AGInfo,
				string.Format("This account is flagged as a control account. You do not have security rights to post to control accounts.\r\n{0}", Env.Security.GeneralLedgerJournalPostToControlAccounts.ErrorMessageForNotAllowed));

			Env.Security.GeneralLedgerJournalPostToControlAccounts.IsAllowed = true;
			testLine.Validation.ValidateAL_AG();
			AssertNoError("Shouldn't be any errors",
				testLine.AL_AGInfo,
				string.Format("This account is flagged as a control account. You do not have security rights to post to control accounts.\r\n{0}", Env.Security.GeneralLedgerJournalPostToControlAccounts.ErrorMessageForNotAllowed));
		}

		public void TestMandatoryAmountFields()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			AccGLHeader testGLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			testGLHeader.AG_AccountType = AccountTypeComboBoxConstants.ProfitAndLossAccount;
			testGLHeader.AG_IsActive = true;

			AssertMandatoryAmountFields(TransactionTypes.GLStandardJournal);
			AssertMandatoryAmountFields(TransactionTypes.GLNoteJournal);

			void AssertMandatoryAmountFields(ZString transactionType)
			{
				journal.AH_TransactionType = transactionType;
				var testLine = journal.GLJournalLines.AddNew();
				testLine.AL_AG = testGLHeader.PK;

				testLine.UnsignedOSLineAmount = 0M;
				Assert("Precondition", testLine.UnsignedOSLineAmount.IsEmpty);
				Assert("Precondition", testLine.UnsignedLocalLineAmount.IsEmpty);

				testLine.Validation.ValidateAll();

				if (journal.IsNoteJournal)
				{
					AssertNoErrors(testLine.UnsignedOSLineAmountInfo);
					AssertNoErrors(testLine.UnsignedLocalLineAmountInfo);
				}
				else
				{
					AssertHasErrors(testLine.UnsignedOSLineAmountInfo);
					AssertHasErrors(testLine.UnsignedLocalLineAmountInfo);
				}

				testLine.UnsignedOSLineAmount = 50M;

				Assert(!testLine.UnsignedOSLineAmount.IsEmpty);
				Assert(!testLine.UnsignedLocalLineAmount.IsEmpty);

				AssertNoErrors(testLine.UnsignedOSLineAmountInfo);
				AssertNoErrors(testLine.UnsignedLocalLineAmountInfo);
			}
		}

		public void TestOrganizationValidation()
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

			TestObjectCreator creator = new TestObjectCreator(Factory);
			GLJournal journal = Factory.NewWithValidTestData<GLJournal>();
			GLJournalLine line = Factory.NewWithValidTestData<GLJournalLine>();
			journal.Lines.Add(line);

			OrgHeader org = creator.CreateOrgHeader("TestOrg1", false, false);
			Factory.Save();

			journal.AH_TransactionCategory = category1.Code;
			Assert("Precondition: Journal is not elimination journal", !journal.IsEliminationJournal);
			line.AL_OH = org.PK;
			AssertHasError(line.AL_OHInfo, "Organization can only be set for Elimination Journal.");

			journal.AH_TransactionCategory = category2.Code;
			Assert("Precondition: Journal is elimination journal", journal.IsEliminationJournal);
			line.AL_OH = org.PK;
			AssertHasError(line.AL_OHInfo, "Organization must be a branch or company proxy or should belong to a consolidation group.");

			GlbCompany company = creator.CreateNewCompany("TCM");
			company.GC_OH_OrgProxy = org.PK;
			Factory.Save();

			line.AL_OH = org.PK;
			AssertNoError(line.AL_OHInfo, "Organization must be a branch or company proxy or should belong to a consolidation group.");

			company.GC_OH_OrgProxy = Guid.Empty;
			Factory.Save();

			line.AL_OH = org.PK;
			AssertHasError(line.AL_OHInfo, "Organization must be a branch or company proxy or should belong to a consolidation group.");

			GlbBranch branch = creator.CreateNewBranch(company, "TBR");
			branch.GB_OH_OrgProxy = org.PK;
			Factory.Save();

			line.AL_OH = org.PK;
			AssertNoError(line.AL_OHInfo, "Organization must be a branch or company proxy or should belong to a consolidation group.");

			branch.GB_OH_OrgProxy = Guid.Empty;
			Factory.Save();

			line.AL_OH = org.PK;
			AssertHasError(line.AL_OHInfo, "Organization must be a branch or company proxy or should belong to a consolidation group.");

			var group = Factory.New<AccConsolidationGroup>();
			var member = group.GroupMembers.AddNew();

			member.YM_OH_Organisation = org.PK;
			Factory.Save();

			line.AL_OSExTaxAmount = -100;
			line.AL_OH = org.PK;
			AssertNoError(line.AL_OHInfo, "Organization must be a branch or company proxy or should belong to a consolidation group.");
			AssertHasError(line.AL_OHInfo, "When posting elimination journals and setting the notional organization, all postings must balance to zero by organization.");

			GLJournalLine line2 = Factory.NewWithValidTestData<GLJournalLine>();
			journal.Lines.Add(line2);
			line2.AL_OSExTaxAmount = 100;
			line2.AL_OH = org.PK;
			AssertNoError(line2.AL_OHInfo, "When posting elimination journals and setting the notional organization, all postings must balance to zero by organization.");
		}

		public virtual void TestCheckUnsignedLocalLineAmount()
		{
			var companyWthoutDecimalPoints = Factory.NewWithValidTestData<GlbCompany>();
			companyWthoutDecimalPoints.GC_RN_NKCountryCode = Core.Constants.CountryCodes.VietNam;
			companyWthoutDecimalPoints.GC_Name = "Vietnam Company";
			companyWthoutDecimalPoints.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.VietNam;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = companyWthoutDecimalPoints.PK;
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var glJournal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
				AssertCheckUnsignedLocalLineAmount(glJournal);

				void AssertCheckUnsignedLocalLineAmount(GLJournal journal)
				{
					var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.VietNam;
					var unsignedLocalLineAmountMessege = "The Local Amount should be equal to OS Amount when Local Currency is used";
					SetAndAssertLine();
					if (journal.IsNoteJournal)
					{
						AssertNoErrors(line.UnsignedLocalLineAmountInfo);
					}
					else
					{
						AssertHasError(line.UnsignedLocalLineAmountInfo, unsignedLocalLineAmountMessege);
					}

					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
					SetAndAssertLine();
					AssertNoError(line.UnsignedLocalLineAmountInfo, unsignedLocalLineAmountMessege);
					line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.VietNam;
					AssertNoError(line.UnsignedLocalLineAmountInfo, unsignedLocalLineAmountMessege);

					void SetAndAssertLine()
					{
						line.AL_ExchangeRate = 2m;
						line.UnsignedOSLineAmount = 200m;
						line.UnsignedLocalLineAmount = 100m;
						using (line.GetLocalAmountCalculationSuspender())
						{
							line.AL_ExchangeRate = 1m;
						}
						AssertEquals(200m, line.UnsignedOSLineAmount);
						AssertEquals(100m, line.UnsignedLocalLineAmount);
						AssertEquals(1m, line.AL_ExchangeRate);
						Assert("Local Amount and OS Amount is different", line.UnsignedOSLineAmount != line.UnsignedLocalLineAmount);
						((GLJournalLineValidation)line.Validation).ValidateUnsignedLocalLineAmount();
					}
				}
			}
		}

		public void TestUnsignedLocalLineAmountValidationDoesNotThrowExceptionWhenTransactionCurrencyIsEmpty()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line = TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);

			line.AL_RX_NKTransactionCurrency = ZString.Empty;
			line.UnsignedOSLineAmount = 100m;
			line.UnsignedLocalLineAmount = 200m;
			using (line.GetLocalAmountCalculationSuspender())
			{
				line.AL_ExchangeRate = 1m;
			}

			Assert("Transaction Currency is empty", line.AL_RX_NKTransactionCurrency.IsEmpty);
			AssertEquals("Exchange rate is 1", 1m, line.AL_ExchangeRate);
			AssertNotEquals("Local Amount and OS Amount is different", line.UnsignedOSLineAmount, line.UnsignedLocalLineAmount);
			AssertNoExceptionThrown("Null Reference Exception must no be thrown", () => ((GLJournalLineValidation)line.Validation).ValidateUnsignedLocalLineAmount());
		}

		public new void TestValidateAL_LocalExtraTaxAmountForIndia_STA_TaxID()
		{
			Assert("Not applicable as tax is not allowed for gl journal", true);
		}

		public void TestCheckUnitQuantity()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLNoteJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line = (GLJournalLine)journal.Lines.AddNew();
			var expectedErrorMesssage = "Please enter a Unit Quantity.";

			line.UnitQuantity = 0;
			AssertHasError(line.UnitQuantityInfo, expectedErrorMesssage);

			line.UnitQuantity = 1;
			AssertNoError(line.UnitQuantityInfo, expectedErrorMesssage);
		}

		public void TestCheckAL_OSExTaxAmount()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today);
			var line = journal.GLJournalLines.AddNew();
			AssertCheckAL_OSExTaxAmount();

			journal.AH_TransactionType = TransactionTypes.GLNoteJournal;
			AssertCheckAL_OSExTaxAmount();

			void AssertCheckAL_OSExTaxAmount()
			{
				var expectedErrorMessage = "Please enter an Amount.";
				var validation = line.Validation as GLJournalLineValidation;

				line.AL_AC = TestObjectCreator.CC1.PK;
				validation.ValidateAL_OSExTaxAmount();
				if (journal.IsNoteJournal)
				{
					AssertNoError(line.AL_OSExTaxAmountInfo, expectedErrorMessage);
				}
				else
				{
					AssertHasError(line.AL_OSExTaxAmountInfo, expectedErrorMessage);
				}

				line.AL_AC = TestObjectCreator.CommentChargeCode.PK;
				validation.ValidateAL_OSExTaxAmount();
				AssertNoError(line.AL_OSExTaxAmountInfo, expectedErrorMessage);
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator testObjectCreator;

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(GLJournal);
		}
	}
}
