using System;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.Utility.Testing.TaxFrameworkTestObjectCreator;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	internal class TestPeriodValidation : BusinessObjectValidationTestCase
	{
		[TestDate(1999, 7, 2)]
		public void TestEndDateLessThanStartDate()
		{
			SecondPeriod.AM_EndDate = SecondPeriod.AM_StartDate.AddDays(-1);
			AssertHasError(SecondPeriod.AM_EndDateInfo, PeriodValidation.EndDateLessThanStartDateErrorMessage);
			SecondPeriod.AM_EndDate = SecondPeriod.AM_StartDate;
			AssertEquals("Has No Error", false, SecondPeriod.AM_EndDateInfo.HasErrors());
			SecondPeriod.AM_EndDate = SecondPeriod.AM_StartDate.AddDays(1);
			AssertEquals("Has No Error", false, SecondPeriod.AM_EndDateInfo.HasErrors());
			FirstPeriod.AM_EndDate = SecondPeriod.AM_EndDate.AddDays(-1);
			AssertEquals("Has No Error", false, FirstPeriod.AM_EndDateInfo.HasErrors());
		}

		public void TestEndDateEqualOrMoreThanNextPeriodEndDate()
		{
			Factory.Save();
			FirstPeriod.AM_EndDate = SecondPeriod.AM_EndDate;
			AssertHasError(FirstPeriod.AM_EndDateInfo, PeriodValidation.EndDateEqualOrMoreThanNextPeriodEndDateErrorMessage);
			FirstPeriod.AM_EndDate = SecondPeriod.AM_EndDate.AddDays(1);
			AssertHasError(FirstPeriod.AM_EndDateInfo, PeriodValidation.EndDateEqualOrMoreThanNextPeriodEndDateErrorMessage);
			LastPeriod.AM_EndDate = FirstPeriodOfNextYear.AM_EndDate;
			AssertHasError(LastPeriod.AM_EndDateInfo, PeriodValidation.EndDateEqualOrMoreThanNextPeriodEndDateErrorMessage);
		}

		[TestDate(1999, 7, 20)]
		public void TestEndDateChangedWhenTransactionExist_WithDefaultDates()
			=> AssertEndDateChangedWhenTransactionExist(false);

		[TestDate(1999, 7, 20)]
		public void TestEndDateChangedWhenTransactionExist_WithCustomDates()
			=> AssertEndDateChangedWhenTransactionExist(true);

		void AssertEndDateChangedWhenTransactionExist(bool enablePostDateGLJournal)
		{
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, enablePostDateGLJournal);
			AssertEquals("PreCondition, testing date", new ZDateTime(1999, 7, 20), ZDateTime.Today);
			AssertEquals("PreCondition, FirstPeriod start date", new ZDateTime(1999, 7, 1), FirstPeriod.AM_StartDate.Date);
			AssertEquals("PreCondition, FirstPeriod end date", new ZDateTime(1999, 7, 31), FirstPeriod.AM_EndDate.Date);
			AssertEquals("PreCondition, SecondPeriod start date", new ZDateTime(1999, 8, 1), SecondPeriod.AM_StartDate.Date);
			AssertEquals("PreCondition, SecondPeriod end date", new ZDateTime(1999, 8, 31), SecondPeriod.AM_EndDate.Date);
			AssertEquals("PreCondition, ThirdPeriod start date", new ZDateTime(1999, 9, 1), ThirdPeriod.AM_StartDate.Date);
			AssertEquals("PreCondition, ThirdPeriod end date", new ZDateTime(1999, 9, 30), ThirdPeriod.AM_EndDate.Date);

			PrepareTransactions();

			FirstPeriod.AM_EndDate = new ZDateTime(1999, 7, 19);
			AssertHasError("Error for INV(PostDate/ReverseDate 20-Jul-99) and Overriden GJL(Post Date 21-Jul-99)", FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "19-Jul-99 and 31-Jul-99");

			FirstPeriod.AM_EndDate = new ZDateTime(1999, 7, 20);
			AssertHasError("Error for Overriden GJL (Post Date 21-Jul-99)", FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "20-Jul-99 and 31-Jul-99");

			FirstPeriod.AM_EndDate = new ZDateTime(1999, 7, 21);
			AssertNoErrors("No Errors for GJL/INV", FirstPeriod.AM_EndDateInfo);

			FirstPeriod.AM_EndDate = new ZDateTime(1999, 8, 20);
			AssertHasError("Errors for Overriden RJL(Post Date 20-Aug-99)", FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "31-Jul-99 and 20-Aug-99");

			FirstPeriod.AM_EndDate = new ZDateTime(1999, 8, 19);
			AssertNoErrors("No Errors for RJL", FirstPeriod.AM_EndDateInfo);

			RecoverTestPeriodYear(TestCurrentYearPeriodManager);

			SecondPeriod.AM_EndDate = new ZDateTime(1999, 8, 19);
			AssertHasError("Errors for Overriden RJL(Post Date 20-Aug-99)", SecondPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "19-Aug-99 and 31-Aug-99");

			SecondPeriod.AM_EndDate = new ZDateTime(1999, 8, 20);
			AssertErrorWhenEnableRegistry("Default RJL (Post Date 31-Aug-99)", SecondPeriod.AM_EndDateInfo, "20-Aug-99 and 31-Aug-99");

			SecondPeriod.AM_EndDate = new ZDateTime(1999, 9, 1);
			AssertErrorWhenEnableRegistry("Default RJL (Reverse Date 01-Sep-99)", SecondPeriod.AM_EndDateInfo, "31-Aug-99 and 01-Sep-99");

			SecondPeriod.AM_EndDate = new ZDateTime(1999, 9, 20);
			AssertHasError("Errors for Overriden RJL(Reverse Date 20-Sep-99)", SecondPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "31-Aug-99 and 20-Sep-99");

			RecoverTestPeriodYear(TestCurrentYearPeriodManager);

			ThirdPeriod.AM_EndDate = new ZDateTime(1999, 9, 19);
			AssertHasError("Errors for Overriden RJL(Reverse Date 20-Sep-99)", ThirdPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "19-Sep-99 and 30-Sep-99");

			ThirdPeriod.AM_EndDate = new ZDateTime(1999, 9, 20);
			AssertNoErrors("No Errors for RJL", ThirdPeriod.AM_EndDateInfo);

			void AssertErrorWhenEnableRegistry(string msg, ZPropertyInfo propInfo, string msgInterval)
			{
				if (enablePostDateGLJournal)
				{
					AssertHasError("Should have error for " + msg, propInfo, ThereAreTransactionPostedBetween + msgInterval);
				}
				else
				{
					AssertNoErrors("Should not have errors for " + msg, propInfo);
				}
			}

			void PrepareTransactions()
			{
				using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					var defualtAPInvoice = Factory.NewWithValidTestData<APInvoice>();
					var defualtAPInvoiceLine = (APInvoiceLine)defualtAPInvoice.Lines.AddNew();
					defualtAPInvoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
					defualtAPInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;

					var defaultRevJournal = Factory.NewWithValidTestData<GLJournal>();
					defaultRevJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
					var defaultRevJournalLine = (GLJournalLine)defaultRevJournal.Lines.AddNew();
					defaultRevJournalLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					defaultRevJournal.AH_PostDate = new ZDateTime(1999, 8, 20);
					defaultRevJournal.AH_DueDate = new ZDateTime(1999, 9, 20);

					Factory.Save();

					CombineAssertions("PreCondition Default INV", () => {
						AssertEquals("Reverse Date", new ZDateTime(1999, 7, 20), defualtAPInvoiceLine.AL_ReverseDate.Date);
						AssertEquals("Post Date", new ZDateTime(1999, 7, 20), defualtAPInvoiceLine.AL_PostDate.Date);
					});

					CombineAssertions("PreCondition Default RJL", () => {
						AssertEquals("Reverse Date", new ZDateTime(1999, 9, 01), defaultRevJournalLine.AL_ReverseDate.Date);
						AssertEquals("Post Date", new ZDateTime(1999, 8, 31), defaultRevJournalLine.AL_PostDate.Date);
					});
				}

				using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					var overridenGlJournal = Factory.NewWithValidTestData<GLJournal>();
					var overridenGlJournalLine = (GLJournalLine)overridenGlJournal.Lines.AddNew();
					overridenGlJournalLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					overridenGlJournal.AH_PostDate = new ZDateTime(1999, 7, 21);

					var overridenRevJournal = Factory.NewWithValidTestData<GLJournal>();
					overridenRevJournal.AH_TransactionType = TransactionTypes.GLReversingJournal;
					var overridenRevJournalLine = (GLJournalLine)overridenRevJournal.Lines.AddNew();
					overridenRevJournalLine.AL_AG = TestObjectCreator.GLHeader1.PK;
					overridenRevJournal.AH_PostDate = new ZDateTime(1999, 8, 20);
					overridenRevJournal.AH_DueDate = new ZDateTime(1999, 9, 20);

					Factory.Save();

					CombineAssertions("PreCondition Overriden GJL", () => {
						AssertEquals("Reverse Date", ZDateTime.Empty, overridenGlJournalLine.AL_ReverseDate.Date);
						AssertEquals("Post Date", new ZDateTime(1999, 7, 21), overridenGlJournalLine.AL_PostDate.Date);
					});
					CombineAssertions("PreCondition Overriden RJL", () => {
						AssertEquals("Reverse Date", new ZDateTime(1999, 9, 20), overridenRevJournalLine.AL_ReverseDate.Date);
						AssertEquals("Post Date", new ZDateTime(1999, 8, 20), overridenRevJournalLine.AL_PostDate.Date);
					});
				}
			}
		}

		[TestDate(1999, 7, 20)]
		public void TestThereArePostedTransactionInTheChangeablePeriod()
		{
			var today = ZDateTime.Today;
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			var testAPInvoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
			testAPInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			testAPInvoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;

			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionType = TransactionTypes.GLAutoJournal;
			var line = (GLJournalLine)journal.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			FirstPeriod.AM_EndDate = today;
			invoice.AH_PostDate = today.AddDays(-2);
			testAPInvoiceLine.AL_PostDate = today.AddDays(-2);
			journal.AH_PostDate = today.AddDays(-1);
			journal.AH_DueDate = today.AddMonths(1);

			Factory.Save();
			AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);

			FirstPeriod.AM_EndDate = today.AddDays(-1);
			AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
			FirstPeriod.AM_EndDate = today.AddDays(-2);
			AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
			FirstPeriod.AM_EndDate = today.AddDays(-3);
			AssertHasError("Error for INV and GJL", FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "17-Jul-99 and 20-Jul-99");

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				FirstPeriod.AM_EndDate = today;
				journal.AH_PostDate = today.AddDays(-1);
				journal.AH_DueDate = today.AddMonths(1);

				Factory.Save();
				AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);

				FirstPeriod.AM_EndDate = today.AddDays(-1);
				AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
				FirstPeriod.AM_EndDate = today.AddDays(-2);
				AssertNoErrors("Has No Error for GJL", FirstPeriod.AM_EndDateInfo);
				FirstPeriod.AM_EndDate = today.AddDays(-3);
				AssertHasError("Error for INV", FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "17-Jul-99 and 20-Jul-99");
			}
		}

		[TestDate(2021, 09, 19)]
		public void TestThereAreTaxGLMovementsInTheChangeablePeriod_AdvancingEndDate()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 07, 1));
			var sepPeriod = currentYearPeriodsManager.Periods[2];

			CreateTaxGLMovementsForTheDate(new ZDate(2021, 08, 10), ZDate.Today);
			Factory.Save();

			sepPeriod.AM_EndDate = new DateTime(2021, 09, 20);
			AssertNoErrors("Advancing end date of Sep period to a date after GL movements", sepPeriod.AM_EndDateInfo);

			sepPeriod.AM_EndDate = new DateTime(2021, 09, 19);
			AssertNoErrors("Advancing end date of Sep period to the date of GL movements", sepPeriod.AM_EndDateInfo);

			sepPeriod.AM_EndDate = new DateTime(2021, 09, 18);
			AssertHasError("Advancing end date of Sep period to a date before GL movements", sepPeriod.AM_EndDateInfo, ThereAreTaxGLMovementsBetween + "18-Sep-21 and 30-Sep-21");
		}

		[TestDate(2021, 09, 19)]
		public void TestThereAreTaxGLMovementsInTheChangeablePeriod_ForwardingEndDate()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2021, 07, 1));
			var augPeriod = currentYearPeriodsManager.Periods[1];

			CreateTaxGLMovementsForTheDate(new ZDate(2021, 08, 10), ZDate.Today);
			Factory.Save();

			augPeriod.AM_EndDate = new DateTime(2021, 09, 18);
			AssertNoErrors("Moving forward end date of Aug period to a date before GL movements", augPeriod.AM_EndDateInfo);

			augPeriod.AM_EndDate = new DateTime(2021, 09, 19);
			AssertHasError("Moving forward end date of Aug period to the date of GL movements", augPeriod.AM_EndDateInfo, ThereAreTaxGLMovementsBetween + "31-Aug-21 and 19-Sep-21");

			augPeriod.AM_EndDate = new DateTime(2021, 09, 20);
			AssertHasError("Moving forward end date of Aug period to a date after GL movements", augPeriod.AM_EndDateInfo, ThereAreTaxGLMovementsBetween + "31-Aug-21 and 20-Sep-21");
		}

		void CreateTaxGLMovementsForTheDate(ZDate invoicePostDate, ZDate taxGLMovementDate)
		{
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_PostDate = invoicePostDate;

			var invoiceLine = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.CC1.PK, 100);
			var taxTransaction = TaxFrameworkObjectCreator.CreateTaxTransaction(new CreateTaxTransactionParameters { CompanyPK = GlbCompany.CurrentCompany.PK, TransactionHeaderPK = invoice.PK, LocalTaxAmount = 12M, OsTaxAmount = 12M, Ledger = TaxConfigurationLedgers.AccountsReceivable.Code, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkObjectCreator.CreateTaxTransactionLinePivot(taxTransaction.PK, TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(invoiceLine));
			var glMovement = TaxFrameworkObjectCreator.CreateAccTaxGLMovement(taxTransaction.PK, date: taxGLMovementDate);
		}

		[TestDate(1999, 7, 20)]
		public void TestThereArePostedTransactionInTheChangeablePeriod_ChecksAL_ReverseDate()
		{
			var today = ZDateTime.Today;
			var invoice = Factory.NewWithValidTestData<APInvoice>();
			APInvoiceLine testAPInvoiceLine = (APInvoiceLine)invoice.Lines.AddNew();
			testAPInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			testAPInvoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;

			var journal = Factory.NewWithValidTestData<GLJournal>();
			journal.AH_TransactionType = TransactionTypes.GLReversingJournal;
			GLJournalLine line = (GLJournalLine)journal.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			FirstPeriod.AM_EndDate = today;
			invoice.AH_PostDate = today.AddDays(-2);
			testAPInvoiceLine.AL_PostDate = today.AddDays(-20);
			testAPInvoiceLine.AL_ReverseDate = today.AddDays(-2);
			journal.AH_PostDate = today.AddDays(-1);
			journal.AH_DueDate = today.AddMonths(1);

			Factory.Save();
			AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
			FirstPeriod.AM_EndDate = today.AddDays(-1);
			AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
			FirstPeriod.AM_EndDate = today.AddDays(-2);
			AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
			FirstPeriod.AM_EndDate = today.AddDays(-3);
			AssertHasError(FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "17-Jul-99 and 20-Jul-99");

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				FirstPeriod.AM_EndDate = today;
				journal.AH_PostDate = today.AddDays(-1);
				journal.AH_DueDate = today.AddMonths(1);

				Factory.Save();
				AssertNoErrors("Has No Error", FirstPeriod.AM_EndDateInfo);
				FirstPeriod.AM_EndDate = today.AddDays(-2);
				AssertHasError(FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "18-Jul-99 and 20-Jul-99");
				FirstPeriod.AM_EndDate = today.AddMonths(1);
				AssertHasError(FirstPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "20-Jul-99 and 20-Aug-99");
			}
		}

		[TestDate(1999, 7, 20)]
		public void TestThereArePostedTransactionInTheChangeablePeriod_OnlyValidatesChangedPeriods()
		{
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			Factory.Save();
			APInvoiceLine invoiceLine1 = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine1.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine1.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine1.AL_PostDate = FirstPeriod.AM_EndDate;
			APInvoiceLine invoiceLine2 = (APInvoiceLine)invoice.Lines.AddNew();
			invoiceLine2.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoiceLine2.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine2.AL_PostDate = SecondPeriod.AM_EndDate;
			Factory.Save();

			FirstPeriod.RunPreSaveValidation();
			SecondPeriod.RunPreSaveValidation();
			AssertNoErrors("First Period", FirstPeriod.AM_EndDateInfo);
			AssertNoErrors("Second Period", SecondPeriod.AM_EndDateInfo);

			SecondPeriod.AM_EndDate = SecondPeriod.AM_EndDate.AddDays(-1);
			FirstPeriod.RunPreSaveValidation();
			SecondPeriod.RunPreSaveValidation();
			AssertNoErrors("First Period", FirstPeriod.AM_EndDateInfo);
			AssertHasError("Second Period", SecondPeriod.AM_EndDateInfo, ThereAreTransactionPostedBetween + "30-Aug-99 and 31-Aug-99");
		}

		[TestDate(2023, 9, 12)]
		public void TestEndDateChangedWhenExistOnlyTransactionHeader()
		{
			TestObjectCreator.CreateARReceipt(ReceiptTypes.Cheque, TransactionTypes.Receipt, LedgerTypes.AccountsReceivable, 100m, TestObjectCreator.AUDBankAccount.PK);

			AssertEndDate(ThereAreTransactionHeaderBetween);
		}

		[SuspendCriticalValidation]
		[TestDate(2023, 9, 10)]
		public void TestEndDateChangedWhenExistCashBasisVAT()
		{
			var cashBasis = TestObjectCreator.CreateInvoiceWithCashBasisVAT(typeof(ARInvoice), "CA1110001", GlbCompany.CurrentCompany.LocalCurrency, 2m, 100m, 10m, 200m, 20m, 50m, 20m);
			cashBasis.YC_PostDate = ZDateTime.Today.AddDays(2);

			AssertEndDate(ThereAreCashBasisVATBetween);
		}

		void AssertEndDate(string message)
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2023, 07, 1));
			var period = currentYearPeriodsManager.Periods[2];
			Factory.Save();

			period.AM_EndDate = new DateTime(2023, 09, 13);
			AssertNoErrors("Should not have error after changing end date", period.AM_EndDateInfo);

			period.AM_EndDate = new DateTime(2023, 09, 12);
			AssertNoErrors("Should not have error after changing end date", period.AM_EndDateInfo);

			period.AM_EndDate = new DateTime(2023, 09, 11);
			AssertHasError("Should have error after changing end date", period.AM_EndDateInfo, message + "11-Sep-23 and 30-Sep-23");
		}

		public void TestThereAreNotPeriodsSetupForTheFolowingYear()
		{
			Factory.Save();
			FirstPeriod.AM_EndDate = FirstPeriod.AM_EndDate.AddDays(-1);
			AssertNoError(FirstPeriod.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			FirstPeriod.AM_EndDate = FirstPeriod.AM_EndDate.AddDays(2);
			AssertNoError(FirstPeriod.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			LastPeriod.AM_EndDate = LastPeriod.AM_EndDate.AddDays(-1);
			AssertNoError(LastPeriod.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			LastPeriod.AM_EndDate = LastPeriod.AM_EndDate.AddDays(2);
			AssertNoError(LastPeriod.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			FirstPeriodOfNextYear.AM_EndDate = FirstPeriodOfNextYear.AM_EndDate.AddDays(-1);
			AssertNoError(FirstPeriodOfNextYear.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			FirstPeriodOfNextYear.AM_EndDate = FirstPeriodOfNextYear.AM_EndDate.AddDays(2);
			AssertNoError(FirstPeriodOfNextYear.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			AssertNoError("There is not error for Original EndDate value.", LastPeriodOfNextYear.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			LastPeriodOfNextYear.AM_EndDate = LastPeriodOfNextYear.AM_EndDate.AddDays(-1);
			AssertHasError(LastPeriodOfNextYear.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			LastPeriodOfNextYear.AM_EndDate = LastPeriodOfNextYear.AM_EndDate.AddDays(2);
			AssertHasError(LastPeriodOfNextYear.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
			LastPeriodOfNextYear.AM_EndDate = LastPeriodOfNextYear.AM_EndDate.AddDays(-1);
			AssertNoError(LastPeriodOfNextYear.AM_EndDateInfo, PeriodValidation.ThereAreNotPeriodsSetupForTheFolowingYear);
		}

		public void TestYearCanNotBeEmpty()
		{
			FirstPeriod.AM_Year = 0;
			AssertHasError(FirstPeriod.AM_YearInfo, "Please enter a Year.");
		}

		public void TestYearMustAroundFinancialYear()
		{
			var year = FirstPeriod.PeriodManager.FinancialYear + 2;
			FirstPeriod.AM_Period = year * 100 + 1;
			FirstPeriod.AM_Year = year;
			AssertHasError(FirstPeriod.AM_YearInfo, PeriodValidation.YearMustAroundCurrentFinancialYear);
		}

		[TestDate(2024, 08, 12)]
		public void TestYearCanNotLessThanPerviousYear()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2023, 07, 1));
			var period1 = currentYearPeriodsManager.Periods[1];
			var period2 = currentYearPeriodsManager.Periods[2];

			period2.AM_Year = period1.AM_Year;
			AssertNoError(period2.AM_YearInfo, PeriodValidation.YearCanNotBeLessThanPreviousRecordYear);

			period2.AM_Year = period1.AM_Year - 1;
			AssertHasError(period2.AM_YearInfo, PeriodValidation.YearCanNotBeLessThanPreviousRecordYear);
		}

		public void TestPeriodCanNotBeEmpty()
		{
			FirstPeriod.AM_Period = 0;
			FirstPeriod.Validation.ValidateAM_Period();
			AssertHasError(FirstPeriod.AM_PeriodInfo, "Please enter a Period.");
		}

		public void TestYearOfPeriodMustBeEqualToYear()
		{
			FirstPeriod.AM_Period = 999901;
			FirstPeriod.AM_Year = 2023;
			AssertHasError(FirstPeriod.AM_PeriodInfo, PeriodValidation.YearOfPeriodMustBeEqualToYearField);
		}

		[TestDate(2024, 05, 12)]
		public void TestPeriodMustOneMorePreviousWhenSameYear()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2023, 07, 1));
			var period1 = currentYearPeriodsManager.Periods[1];
			var period2 = currentYearPeriodsManager.Periods[2];
			var period3 = currentYearPeriodsManager.Periods[3];

			period2.AM_Period = period1.AM_Period + 1;
			period2.Validation.ValidateAM_Period();
			AssertNoError(period2.AM_PeriodInfo, PeriodValidation.PeriodMustBeOneMoreThanPreviousPeriod);

			period2.AM_Period = period1.AM_Period + 2;
			period3.AM_Period = period3.AM_Period + 12;
			period2.Validation.ValidateAM_Period();
			AssertHasError(period2.AM_PeriodInfo, PeriodValidation.PeriodMustBeOneMoreThanPreviousPeriod);

			var property = ((ITypedList)currentYearPeriodsManager.Periods[0]).GetItemProperties(null)["AM_Period"];
			(currentYearPeriodsManager.Periods as IBindingList).ApplySort(property, ListSortDirection.Descending);

			var periodSelected = currentYearPeriodsManager.Periods[5];
			periodSelected.Validation.ValidateAM_Period();
			AssertNoError(periodSelected.AM_PeriodInfo, PeriodValidation.PeriodMustBeOneMoreThanPreviousPeriod);
		}

		[TestDate(2024, 05, 12)]
		public void TestPeriodMustBeUnique()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2023, 07, 1));
			var period1 = currentYearPeriodsManager.Periods[1];
			var period3 = currentYearPeriodsManager.Periods[3];
			period3.AM_Year = (ZShort)(period3.AM_Period / 100);

			AssertNoError(period3.AM_PeriodInfo, PeriodValidation.PeriodMustBeUnique);

			period1.AM_Period = period3.AM_Period;
			period3.Validation.ValidateAM_Period();
			AssertHasError(period3.AM_PeriodInfo, PeriodValidation.PeriodMustBeUnique);
		}

		[TestDate(2024, 05, 12)]
		public void TestPeriodStartFromOne()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2023, 07, 1));
			var period1 = currentYearPeriodsManager.Periods[1];
			var period2 = currentYearPeriodsManager.Periods[3];
			period2.AM_Year = (ZShort)(period2.AM_Period / 100);

			AssertNoError(period2.AM_PeriodInfo, PeriodValidation.PeriodMustStartFromOne);

			var property = ((ITypedList)currentYearPeriodsManager.Periods[0]).GetItemProperties(null)["AM_Period"];
			(currentYearPeriodsManager.Periods as IBindingList).ApplySort(property, ListSortDirection.Descending);

			var periodSelected = currentYearPeriodsManager.Periods[0];
			periodSelected.Validation.ValidateAM_Period();
			AssertNoError(periodSelected.AM_PeriodInfo, PeriodValidation.PeriodMustStartFromOne);

			period2.AM_Year = 3000;
			period2.AM_Period = period2.AM_Year * 100 + 5;
			period2.Validation.ValidateAM_Period();
			AssertHasError(period2.AM_PeriodInfo, PeriodValidation.PeriodMustStartFromOne);
		}

		public void TestGetLastPeriodOfPreviousFinancialYear()
		{
			var previousYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2019, 07, 1));
			previousYearPeriodsManager.FinancialYear = 2020;
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2020, 07, 1));
			currentYearPeriodsManager.FinancialYear = 2021;
			Factory.Save();

			currentYearPeriodsManager.Periods[0].Validation.ValidateAM_Period();
			AssertNoError(currentYearPeriodsManager.Periods[0].AM_PeriodInfo, PeriodValidation.PeriodMustBeOneMoreThanPreviousPeriod);

			var previousPeriodsSize = previousYearPeriodsManager.Periods.Count;
			currentYearPeriodsManager.Periods[0].AM_Year = previousYearPeriodsManager.Periods[previousPeriodsSize - 1].AM_Year;
			currentYearPeriodsManager.Periods[0].AM_Period = currentYearPeriodsManager.Periods[0].AM_Year * 100 + 2;
			currentYearPeriodsManager.Periods[0].Validation.ValidateAM_Period();
			AssertHasError(currentYearPeriodsManager.Periods[0].AM_PeriodInfo, PeriodValidation.PeriodMustBeOneMoreThanPreviousPeriod);

			previousPeriodsSize = previousYearPeriodsManager.Periods.Count;
			currentYearPeriodsManager.Periods[0].AM_Year = previousYearPeriodsManager.Periods[previousPeriodsSize - 1].AM_Year;
			currentYearPeriodsManager.Periods[0].AM_Period = previousYearPeriodsManager.Periods[previousPeriodsSize - 1].AM_Period + 1;
			currentYearPeriodsManager.Periods[0].Validation.ValidateAM_Period();
			AssertNoError(currentYearPeriodsManager.Periods[0].AM_PeriodInfo, PeriodValidation.PeriodMustBeOneMoreThanPreviousPeriod);
		}

		[TestDate(2024, 05, 12)]
		public void TestStartEndDateMustBeUnique()
		{
			var currentYearPeriodsManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2023, 07, 1));
			Factory.Save();
			var period1 = currentYearPeriodsManager.Periods[1];
			var period3 = currentYearPeriodsManager.Periods[3];

			AssertNoError(period3.AM_PeriodInfo, PeriodValidation.StartEndDateMustBeUnique);

			period3.AM_StartDate = period1.AM_StartDate;
			period3.AM_EndDate = period1.AM_EndDate;
			period3.Validation.ValidateAM_Period();
			AssertHasError(period3.AM_PeriodInfo, PeriodValidation.StartEndDateMustBeUnique);

			period1.AM_Year = 2025;
			period1.AM_Period = 202501;
			period1.Validation.ValidateAM_Period();
			AssertNoError(period1.AM_PeriodInfo, PeriodValidation.StartEndDateMustBeUnique);
		}

		void RecoverTestPeriodYear(PeriodManager periodYear)
		{
			foreach (Period period in periodYear.Periods)
			{
				period.AM_EndDate = (ZDateTime)period.AM_EndDateInfo.OriginalValue;

				AssertEquals("period end date is recovered.", false, period.AM_EndDateInfo.HasChanges);
				AssertEquals("next period start date is recovered.", false, period.NextPeriod.AM_StartDateInfo.HasChanges);
			}
		}

		PeriodManager TestCurrentYearPeriodManager;
		PeriodManager TestNextYearPeriodManager;
		Period FirstPeriod;
		Period SecondPeriod;
		Period ThirdPeriod;
		Period LastPeriod;
		Period FirstPeriodOfNextYear;
		Period LastPeriodOfNextYear;
		TestObjectCreator TestObjectCreator;

		String ThereAreTransactionPostedBetween => "There are transactions posted between ";
		String ThereAreTaxGLMovementsBetween => "There are tax GL movements between ";
		String ThereAreTransactionHeaderBetween => "There are transaction headers between ";
		String ThereAreCashBasisVATBetween => "There are Cash Basis VATs between ";

		TaxFrameworkTestObjectCreator TaxFrameworkObjectCreator => taxFrameworkObjectCreator ?? (taxFrameworkObjectCreator = new TaxFrameworkTestObjectCreator(Factory));
		TaxFrameworkTestObjectCreator taxFrameworkObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			TestCurrentYearPeriodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 7, 1));
			FirstPeriod = TestCurrentYearPeriodManager.Periods[0];
			SecondPeriod = TestCurrentYearPeriodManager.Periods[1];
			ThirdPeriod = TestCurrentYearPeriodManager.Periods[2];
			LastPeriod = TestCurrentYearPeriodManager.Periods[TestCurrentYearPeriodManager.Periods.Count - 1];
			TestNextYearPeriodManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(2000, 7, 1));
			FirstPeriodOfNextYear = TestNextYearPeriodManager.Periods[0];
			LastPeriodOfNextYear = TestNextYearPeriodManager.Periods[11];
		}
	}
}
