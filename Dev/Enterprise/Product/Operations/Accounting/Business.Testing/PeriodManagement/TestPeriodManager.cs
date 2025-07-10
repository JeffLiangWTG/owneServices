using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.PeriodManagement
{
	internal class TestPeriodManager : TestCaseWithFactory
	{
		[ExpectNoExceptions()]
		public void TestConcurrency()
		{
			Period nextUnClosedSubLedgerPeriod = TestManager.NextUnClosedSubLedgerPeriod;
			Period nextUnClosedGLPeriod = TestManager.NextUnClosedGLPeriod;
			Period nextUnClosedForAdjustmentsSubLedgerPeriod = TestManager.NextUnClosedForAdjustmentsSubLedgerPeriod;
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			PeriodManager newTestManager = new PeriodManager(newFactory);
			Factory.RefreshEnabled = false;
			AssertEquals(nextUnClosedSubLedgerPeriod.AM_Period, newTestManager.NextUnClosedSubLedgerPeriod.AM_Period);
			AssertEquals(nextUnClosedGLPeriod.AM_Period, TestManager.NextUnClosedGLPeriod.AM_Period);
			AssertEquals(nextUnClosedForAdjustmentsSubLedgerPeriod.AM_Period, TestManager.NextUnClosedForAdjustmentsSubLedgerPeriod.AM_Period);
			//the concurrency exception has been handle
			newTestManager.CloseSubLedgerPeriod();
			newTestManager.CloseGLPeriod();
			newTestManager.CloseGLPeriodForAdjustments();
			TestManager.CloseSubLedgerPeriod();
			TestManager.CloseGLPeriod();
			TestManager.CloseGLPeriodForAdjustments();
		}

		[TestDate(1999, 7, 2)]
		public void TestDeleteAllPeriodsInCurrentCompany()
		{
			TestManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 7, 1));
			Factory.Save();
			AssertEquals(12, TestManager.Periods.Count);
			TestManager.DeleteAllPeriodsInCurrentCompany();
			AssertEquals(0, TestManager.Periods.Count);
		}

		public void TestDeletePeriodsFromStartDateInCurrentCompany()
		{
			TestManager = TestObjectCreator.CreateTestPeriods(new ZDateTime(1999, 7, 1));
			Factory.Save();
			AssertEquals(12, TestManager.Periods.Count);
			TestManager.DeletePeriodsFromStartDateInCurrentCompany(new ZDateTime(1999, 12, 1));
			AssertEquals(5, TestManager.Periods.Count);
		}

		public void TestIsValidationSuspendedInCreateOnePeriod()
		{
			var periodManager = new PeriodManager(Factory);
			var period = periodManager.CreateOnePeriod(202303, new ZDateTime(2023, 3, 1), new ZDateTime(2023, 3, 31), Factory, true);
			AssertEquals(true, period.IsValidationSuspended);

			period = periodManager.CreateOnePeriod(202304, new ZDateTime(2023, 4, 1), new ZDateTime(2023,4, 30), Factory);
			AssertEquals(false, period.IsValidationSuspended);
		}

		public void TestExtendLastYearPeriod()
		{
			var periodManager = new PeriodManager(Factory);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2022);
			Factory.Save();

			var extendLastFinancialYearSettings  = new ExtendLastFinancialYearSettings();
			extendLastFinancialYearSettings.FinancialYear = 2022;
			extendLastFinancialYearSettings.StartDate = new ZDateTime(2022, 01, 01);
			extendLastFinancialYearSettings.EndDate = new ZDateTime(2023, 01, 31);
			extendLastFinancialYearSettings.PeriodFormat = ACPeriodFormat.Month;
			extendLastFinancialYearSettings.LastPeriodEndDate = new ZDateTime(2022, 12, 31);
			extendLastFinancialYearSettings.LastPeriod = 202212;

			periodManager.ExtendLastYearPeriod(extendLastFinancialYearSettings);

			var period = periodManager.Periods[periodManager.Periods.Count - 1];
			AssertEquals(202213,period.AM_Period);
			AssertEquals(new ZDateTime(2023, 01, 01).Date, period.AM_StartDate.Date);
			AssertEquals(extendLastFinancialYearSettings.EndDate.Date, period.AM_EndDate.Date);
			AssertEquals(2022, (int)period.AM_Year);
		}

		public void TestOutstandingReceiptsInCurrentCompanyStopSubLedgerClose()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();
			ARReceipt unbatchedReceiptInOtherCompany = Factory.NewWithValidTestData<ARReceipt>();
			unbatchedReceiptInOtherCompany.AH_PostDate = helper.PreviousOpenPeriod.AM_EndDate.AddDays(-2);
			unbatchedReceiptInOtherCompany.AH_GB = TestObjectCreator.NonCurrentCompanyBranch.PK;
			ARReceipt batchedReceipt = Factory.NewWithValidTestData<ARReceipt>();
			batchedReceipt.AH_PostDate = helper.PreviousOpenPeriod.AM_EndDate.AddDays(-2);
			batchedReceipt.AH_ReceiptBatchNo = "11111111";
			Factory.Save();
			TestManager = new PeriodManager(Factory);
			AssertEquals("", TestManager.CanClosePeriod(Factory.Load<Period>(helper.PreviousOpenPeriod.PK), false));
			ARReceipt cancelledReceipt = Factory.NewWithValidTestData<ARReceipt>();
			cancelledReceipt.AH_IsCancelled = true;
			((IMatching)cancelledReceipt).CurrentMatchGroup.AddNew().AP_AH = cancelledReceipt.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(cancelledReceipt);
			cancelledReceipt.AH_PostDate = helper.PreviousOpenPeriod.AM_EndDate.AddDays(-2);
			Factory.Save();
			AssertEquals("", TestManager.CanClosePeriod(Factory.Load<Period>(helper.PreviousOpenPeriod.PK), false));
			ARReceipt unbatchedReceiptInCurrentCompany = Factory.NewWithValidTestData<ARReceipt>();
			unbatchedReceiptInCurrentCompany.AH_PostDate = helper.PreviousOpenPeriod.AM_EndDate.AddDays(-2);
			Factory.Save();
			AssertNotEquals("", TestManager.CanClosePeriod(Factory.Load<Period>(helper.PreviousOpenPeriod.PK), false));
		}

		[TestDate(1999, 8, 2)]
		public void TestCanClosePeriod()
		{
			//Other Income Journal
			GLJournalLine oIJournalLine = TestJournal.GLJournalLines.AddNew();
			oIJournalLine.AL_AG = OtherIncomeGLAccount.PK;
			oIJournalLine.AL_OSExTaxAmount = 100m;
			oIJournalLine.DebitCreditSign = "DR";
			oIJournalLine.AL_GB = GlbBranch.CurrentBranch.PK;
			//Clearing Account Journal
			TestJournal.Balance();
			AggregateWrapper aggregator = new AggregateWrapper(TestJournal, TestJournal);
			BusinessObjectFactory.SaveTogether(Factory, aggregator);
			AccGLAggregate[] aggregates = (AccGLAggregate[])Factory.Load(typeof(AccGLAggregate), new ZQuery());
			Assert("Should be more than one aggregate", aggregates.Length > 0);
			ZQuery filter = new ZQuery(AccGLAggregateSchema.AA_AG, ClearingGLAccount.PK);
			aggregates = (AccGLAggregate[])Factory.Load(typeof(AccGLAggregate), filter);
			AssertEquals("Should be one record", 1, aggregates.Length);
			Period firstPeriod = Factory.New<Period>();
			firstPeriod.AM_Period = 200001;
			firstPeriod.AM_StartDate = new ZDateTime(1999, 7, 1);
			firstPeriod.AM_EndDate = new ZDateTime(1999, 7, 31);
			AssertEquals(string.Empty, TestManager.CanClosePeriod(firstPeriod, false));
			AssertEquals("Account Period " + firstPeriod.AM_Period + " cannot be closed as the year to date balance for GL Journal Clearing Account Number " + TestManager.GLClearingAccountNumber + " as at " + firstPeriod.AM_Period + " does not equal to zero.\r\n\r\n" + "Please run the GL Transaction Report for this GL Account up to " + firstPeriod.AM_Period + " and make the necessary adjustment entries before closing the period", TestManager.CanClosePeriod(firstPeriod, true));
		}

		[TestDate(1999, 7, 2)]
		public void TestDoesGLClearingAccountHaveZeroBalanceForPeriod()
		{
			//Other Income Journal
			GLJournalLine oIJournalLine = TestJournal.GLJournalLines.AddNew();
			oIJournalLine.AL_AG = OtherIncomeGLAccount.PK;
			oIJournalLine.AL_OSExTaxAmount = 100m;
			oIJournalLine.DebitCreditSign = "DR";
			oIJournalLine.AL_GB = GlbBranch.CurrentBranch.PK;
			//Clearing Account Journal
			TestJournal.Balance();
			AggregateWrapper aggregator = new AggregateWrapper(TestJournal, TestJournal);
			BusinessObjectFactory.SaveTogether(Factory, aggregator);
			AccGLAggregate[] aggregates = (AccGLAggregate[])Factory.Load(typeof(AccGLAggregate), new ZQuery());
			Assert("Should be more than one aggregate", aggregates.Length > 0);
			ZQuery filter = new ZQuery(AccGLAggregateSchema.AA_AG, ClearingGLAccount.PK);
			aggregates = (AccGLAggregate[])Factory.Load(typeof(AccGLAggregate), filter);
			AssertEquals("Should be one record", 1, aggregates.Length);
			Period firstPeriod = Factory.New<Period>();
			firstPeriod.AM_Period = 200001;
			AssertEquals("Should be false", false, TestManager.DoesGLClearingAccountHaveZeroBalanceForPeriod(firstPeriod));
		}

		[TestDate(1999, 7, 2)]
		public void TestDoesGLClearingAccountHaveZeroBalanceForPeriodWithZeroBalance()
		{
			//Other Income Journal
			GLJournalLine oIJournalLine = TestJournal.GLJournalLines.AddNew();
			oIJournalLine.AL_AG = OtherIncomeGLAccount.PK;
			oIJournalLine.AL_OSExTaxAmount = 100m;
			oIJournalLine.DebitCreditSign = "DR";
			oIJournalLine.AL_GB = GlbBranch.CurrentBranch.PK;
			GLJournalLine oIJournalLine2 = TestJournal.GLJournalLines.AddNew();
			oIJournalLine2.AL_AG = ClearingGLAccount.PK;
			oIJournalLine2.AL_OSExTaxAmount = 100m;
			oIJournalLine2.DebitCreditSign = "CR";
			oIJournalLine2.AL_GB = GlbBranch.CurrentBranch.PK;
			GLJournal secondJournal = Factory.NewWithValidTestData<GLJournal>();
			secondJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			secondJournal.AH_PostDate = new ZDateTime(1999, 7, 2);
			secondJournal.PostPeriod = 200001;
			GLJournalLine oIJournalLine3 = secondJournal.GLJournalLines.AddNew();
			oIJournalLine3.AL_AG = OtherIncomeGLAccount.PK;
			oIJournalLine3.AL_OSExTaxAmount = 100m;
			oIJournalLine3.DebitCreditSign = "CR";
			oIJournalLine3.AL_GB = GlbBranch.CurrentBranch.PK;
			GLJournalLine oIJournalLine4 = secondJournal.GLJournalLines.AddNew();
			oIJournalLine4.AL_AG = ClearingGLAccount.PK;
			oIJournalLine4.AL_OSExTaxAmount = 100m;
			oIJournalLine4.DebitCreditSign = "DR";
			oIJournalLine4.AL_GB = GlbBranch.CurrentBranch.PK;
			AggregateWrapper testJournalAggregator = new AggregateWrapper(TestJournal, TestJournal);
			AggregateWrapper secondJournalAggregator = new AggregateWrapper(secondJournal, secondJournal);
			BusinessObjectFactory.SaveTogether(Factory, testJournalAggregator, secondJournalAggregator);
			AccGLAggregate[] aggregates = (AccGLAggregate[])Factory.Load(typeof(AccGLAggregate), new ZQuery());
			Assert("Should be more than one aggregate", aggregates.Length > 0);
			ZQuery filter = new ZQuery(AccGLAggregateSchema.AA_AG, ClearingGLAccount.PK);
			aggregates = (AccGLAggregate[])Factory.Load(typeof(AccGLAggregate), filter);
			AssertEquals("Should be one record", 2, aggregates.Length);
			Period firstPeriod = Factory.New<Period>();
			firstPeriod.AM_Period = 200001;
			AssertEquals("Should be true", true, TestManager.DoesGLClearingAccountHaveZeroBalanceForPeriod(firstPeriod));
		}

		public void TestDoesTransactionExistInAccTransactionHeader()
		{
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("Should be false", false, TestManager.DoesTransactionExistInAccTransactionHeader);
			APPayment testPayment = Factory.NewWithValidTestData<APPayment>();
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.AH_ChequeOrReference = "10000";
			Factory.Save();
			AssertEquals("Should be true", true, TestManager.DoesTransactionExistInAccTransactionHeader);
		}

		public void TestDoesTransactionExistInAccTransactionLines()
		{
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);
			AssertEquals("Should be false", false, TestManager.DoesTransactionExistInAccTransactionLines);
			APInvoiceLine testAPInvoiceLine = (APInvoiceLine)Factory.NewWithValidTestData<APInvoice>().Lines.AddNew();
			testAPInvoiceLine.AL_GB = GlbBranch.CurrentBranch.PK;
			testAPInvoiceLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();
			AssertEquals("Should be true", true, TestManager.DoesTransactionExistInAccTransactionLines);
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateGJL_Overridden()
		{
			AssertEquals("PreCondition, EnablePostDateGLJournal is disabled.", false, AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.Value);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpTestJournalWithLines(TransactionTypes.GLStandardJournal, new ZDateTime(1999, 8, 25), null);
				AssertEquals("Post Date should be overridden", new ZDateTime(1999, 8, 25), TestJournal.AH_PostDate.Date);
			}

			var period = TestManager.Periods[1];
			AssertEquals("PreCondition, Period start date", new ZDateTime(1999, 8, 1), period.AM_StartDate.Date);
			AssertEquals("PreCondition, Period end date", new ZDateTime(1999, 8, 31), period.AM_EndDate.Date);

			period.AM_EndDate = new ZDateTime(1999, 8, 26);
			AssertNoErrors("2nd period end date changed", period.AM_EndDateInfo);
			TestManager.Factory.Save();

			var newTestJournal = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
			AssertEquals("2nd period end date changed 26/8/99", new ZDateTime(1999, 8, 26), period.AM_EndDate.Date);
			AssertEquals("Should be equal to new end date, because was overridden but not in the shift interval", period.AM_EndDate, newTestJournal.AH_PostDate);
			foreach (GLJournalLine journalLineCheck in newTestJournal.Lines)
			{
				AssertEquals("Should be equal to new end date", period.AM_EndDate, journalLineCheck.AL_PostDate);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateGJL()
		{
			AssertEquals("PreCondition, EnablePostDateGLJournal is disabled.", false, AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.Value);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			SetUpTestJournalWithLines(TransactionTypes.GLStandardJournal, new ZDateTime(1999, 8, 15), null);
			AssertEquals("Post Date should be standard (equal to Period end date)", new ZDateTime(1999, 8, 31), TestJournal.AH_PostDate.Date);

			var period = TestManager.Periods[1];
			AssertEquals("PreCondition,Period start date", new ZDateTime(1999, 8, 1), period.AM_StartDate.Date);
			AssertEquals("PreCondition,Period end date", new ZDateTime(1999, 8, 31), period.AM_EndDate.Date);

			period.AM_EndDate = new ZDateTime(1999, 9, 10);
			AssertNoErrors("2nd period end date changed", period.AM_EndDateInfo);
			TestManager.Factory.Save();

			var newJournalCheck = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
			AssertEquals("2nd period end date changed 10/9/99", new ZDateTime(1999, 9, 10), period.AM_EndDate.Date);
			AssertEquals("Should be equal to new end date", period.AM_EndDate, newJournalCheck.AH_PostDate);
			foreach (GLJournalLine journalLineCheck in newJournalCheck.Lines)
			{
				AssertEquals("Should be equal to new end date", period.AM_EndDate, journalLineCheck.AL_PostDate);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateGJL_Enabled()
		{
			AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var customizedPostDate_19990825 = new ZDateTime(1999, 8, 25);
			SetUpTestJournalWithLines(TransactionTypes.GLStandardJournal, customizedPostDate_19990825, null);
			AssertEquals("Post Date should be overridden", customizedPostDate_19990825, TestJournal.AH_PostDate);

			var period = TestManager.Periods[1];
			AssertEquals("PreCondition, 2nd Period start date", new ZDateTime(1999, 8, 1), period.AM_StartDate.Date);
			AssertEquals("PreCondition, 2nd Period end date", new ZDateTime(1999, 8, 31), period.AM_EndDate.Date);

			period.AM_EndDate = new ZDateTime(1999, 8, 26);
			AssertNoErrors("2nd period end date changed", period.AM_EndDateInfo);
			TestManager.Factory.Save();

			var newJournalCheck = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
			AssertEquals("2nd period end date changed 26/8/99", new ZDateTime(1999, 8, 26), period.AM_EndDate.Date);
			AssertEquals("Post Date should not be changed when EnablePostDateGLJournal is enabled.", customizedPostDate_19990825, newJournalCheck.AH_PostDate);
			foreach (GLJournalLine journalLineCheck in newJournalCheck.Lines)
			{
				AssertEquals("Post Date should not be changed when EnablePostDateGLJournal is enabled.", customizedPostDate_19990825, journalLineCheck.AL_PostDate);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateNJL()
		{
			TestCaseHelper.ClearTable(AccTransactionMatchLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var postDate = new ZDateTime(1999, 8, 15);
			var dueDate = new ZDateTime(1999, 9, 10);
			SetUpTestJournalWithLines(TransactionTypes.GLNoteJournal, postDate, null);

			var periods = TestManager.Periods;
			//Edit End dates of 2nd period
			var endDate2ndPeriod = periods[1].AM_EndDate;
			AssertEquals("2nd period end date is 31/8/99", new ZDateTime(1999, 8, 31), endDate2ndPeriod.Date);
			endDate2ndPeriod = periods[1].AM_EndDate = endDate2ndPeriod.AddDays(10);
			AssertEquals("2nd period end date changed 10/9/99", dueDate, endDate2ndPeriod.Date);
			//Edit End dates of 3rd period
			var end3rdPeriod = periods[2].AM_EndDate;
			AssertEquals("3rd period end date is 30/9/99", new ZDateTime(1999, 9, 30), end3rdPeriod.Date);
			end3rdPeriod = periods[2].AM_EndDate = end3rdPeriod.AddDays(10);
			AssertEquals("3rd period end date changed 10/10/99", dueDate.AddMonths(1), end3rdPeriod.Date);
			//Update TransactionHeader.AH_PostDate, TransactionLines.AL_PostDate
			TestManager.Factory.Save();

			//Check the amended PostDate
			var journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
			AssertEquals("Should be equal to new end date", endDate2ndPeriod, journalCheck.AH_PostDate);
			foreach (GLJournalLine journalLineCheck in journalCheck.Lines)
			{
				AssertEquals("Should be equal to new end date", endDate2ndPeriod, journalLineCheck.AL_PostDate);
			}

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				journalCheck.AH_PostDate = dueDate.AddDays(-5);
				TestManager.Factory.Save();

				//Edit End dates of 2nd period
				endDate2ndPeriod = periods[1].AM_EndDate = endDate2ndPeriod.AddDays(5);
				AssertEquals("2nd period end date changed 15/9/99", dueDate.AddDays(5), endDate2ndPeriod.Date);
				//Edit End dates of 3rd period
				end3rdPeriod = periods[2].AM_EndDate = end3rdPeriod.AddDays(5);
				AssertEquals("3rd period end date changed 15/10/99", dueDate.AddMonths(1).AddDays(5), end3rdPeriod.Date);
				//Update TransactionHeader.AH_PostDate, TransactionLines.AL_PostDate
				TestManager.Factory.Save();

				//Check the amended PostDate
				journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
				AssertNotEquals("Should NOT be equal to new end date", endDate2ndPeriod, journalCheck.AH_PostDate);
				foreach (GLJournalLine journalLineCheck in journalCheck.Lines)
				{
					AssertNotEquals("Should NOT be equal to new end date", endDate2ndPeriod, journalLineCheck.AL_PostDate);
				}
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateRJL_Overridden()
		{
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var postDate = new ZDateTime(1999, 8, 15);
			var dueDate = new ZDateTime(1999, 9, 10);
			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpTestJournalWithLines(TransactionTypes.GLReversingJournal, postDate, dueDate);
			}
			var journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
			AssertEquals("Post Date should be overridden", postDate, journalCheck.AH_PostDate);
			AssertEquals("Due Date should be overridden", dueDate, journalCheck.AH_DueDate);

			var periods = TestManager.Periods;
			var endDate2ndPeriod = periods[1].AM_EndDate;
			AssertEquals("PreCondition, 2nd Period start date", new ZDateTime(1999, 8, 1), periods[1].AM_StartDate.Date);
			AssertEquals("PreCondition, 2nd Period end date", new ZDateTime(1999, 8, 31), endDate2ndPeriod.Date);

			periods[1].AM_EndDate = postDate.AddDays(-1);
			AssertHasErrors("Should be prevented, because of overridden Post Date found in the shift interval (> 14/8/99)", periods[1].AM_EndDateInfo);
			periods[1].AM_EndDate = postDate;
			AssertNoErrors("2nd period end date changed 15/8/99", periods[1].AM_EndDateInfo);
			periods[1].AM_EndDate = endDate2ndPeriod.AddDays(-1);
			AssertNoErrors("2nd period end date changed 30/8/99", periods[1].AM_EndDateInfo);
			periods[1].AM_EndDate = endDate2ndPeriod.AddDays(1);
			AssertNoErrors("2nd period end date changed 1/9/99", periods[1].AM_EndDateInfo);
			var newEndDate2ndPeriod = periods[1].AM_EndDate = dueDate.AddDays(-1);
			AssertNoErrors("2nd period end date changed", periods[1].AM_EndDateInfo);
			AssertEquals("2nd period end date changed 9/9/99", dueDate.AddDays(-1), newEndDate2ndPeriod);
			periods[1].AM_EndDate = dueDate;
			AssertHasErrors("Should be prevented, because of overridden Due Date found in the shift interval (<= 10/9/99)", periods[1].AM_EndDateInfo);
			periods[1].AM_EndDate = newEndDate2ndPeriod;

			var newStartDate3rdPeriod = periods[2].AM_StartDate;
			AssertEquals("3rd period start date changed 10/9/99", dueDate, newStartDate3rdPeriod);

			TestManager.Factory.Save();

			var newJournalCheck = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
			AssertEquals("AH_PostDate should be equal to new end date, because was overridden but not in the shift interval", newEndDate2ndPeriod, newJournalCheck.AH_PostDate.Date);
			AssertEquals("AH_DueDate should be equal to new start date, because was overridden but not in the shift interval", newStartDate3rdPeriod, newJournalCheck.AH_DueDate.Date);
			foreach (GLJournalLine journalLineCheck in newJournalCheck.Lines)
			{
				AssertEquals("AL_PostDate should be equal to new end date", newEndDate2ndPeriod, journalLineCheck.AL_PostDate.Date);
				AssertEquals("AL_ReverseDate should be equal to new start date", newStartDate3rdPeriod, journalLineCheck.AL_ReverseDate.Date);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateRJL()
		{
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var postDate = new ZDateTime(1999, 8, 15);
			var dueDate = new ZDateTime(1999, 9, 10);
			SetUpTestJournalWithLines(TransactionTypes.GLReversingJournal, postDate, dueDate);

			var periods = TestManager.Periods;

			var endDate2ndPeriod = periods[1].AM_EndDate;
			AssertEquals("PreCondition, 2nd Period start date", new ZDateTime(1999, 8, 1), periods[1].AM_StartDate.Date);
			AssertEquals("PreCondition, 2nd Period end date", new ZDateTime(1999, 8, 31), endDate2ndPeriod.Date);

			var startDate3rdPeriod = periods[2].AM_StartDate;
			AssertEquals("PreCondition, 3rd Period start date", new ZDateTime(1999, 9, 1), startDate3rdPeriod.Date);
			AssertEquals("PreCondition, 3rd Period end date", new ZDateTime(1999, 9, 30), periods[2].AM_EndDate.Date);

			var journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
			AssertEquals("Should be standard (equal to 2nd Period end date)", endDate2ndPeriod, journalCheck.AH_PostDate);
			AssertEquals("Should be standard (equal to 3rd Period start date)", startDate3rdPeriod, journalCheck.AH_DueDate);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				periods[1].AM_EndDate = endDate2ndPeriod.AddDays(-1);
				AssertHasErrors("Should be prevented, because of Post Date found in the shift interval (> 30/8/99)", periods[1].AM_EndDateInfo);
				periods[1].AM_EndDate = endDate2ndPeriod.AddDays(1);
				AssertHasErrors("Should be prevented, because of Rev Date found in the shift interval (<= 1/9/99)", periods[1].AM_EndDateInfo);
				periods[1].AM_EndDate = endDate2ndPeriod;

				TestManager.Factory.Save();

				//Check the amended AH_PostDate, AH_DueDate, AL_PostDate, AL_ReverseDate
				var newJournalCheck = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
				AssertEquals("AH_PostDate Should be unchanged", endDate2ndPeriod, newJournalCheck.AH_PostDate);
				AssertEquals("AH_DueDate Should be unchanged", startDate3rdPeriod, newJournalCheck.AH_DueDate);
				foreach (GLJournalLine journalLineCheck in newJournalCheck.Lines)
				{
					AssertEquals("AL_PostDate Should be unchanged", endDate2ndPeriod, journalLineCheck.AL_PostDate);
					AssertEquals("AL_ReverseDate Should be unchanged", startDate3rdPeriod, journalLineCheck.AL_ReverseDate);
				}
			}

			periods[1].AM_EndDate = endDate2ndPeriod.AddDays(-1);
			AssertNoErrors("2nd period end date changed 30/8/99", periods[1].AM_EndDateInfo);
			var newEndDate2ndPeriod = periods[1].AM_EndDate = endDate2ndPeriod.AddDays(1);
			AssertNoErrors("2nd period end date changed", periods[1].AM_EndDateInfo);
			AssertEquals("2nd period end date changed 1/9/99", startDate3rdPeriod.Date, newEndDate2ndPeriod.Date);

			var newStartDate3rdPeriod = periods[2].AM_StartDate;
			AssertEquals("3rd period start date changed 2/9/99", newEndDate2ndPeriod.AddMinutes(1), newStartDate3rdPeriod);

			TestManager.Factory.Save();

			var newJournalCheck2 = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
			AssertEquals("AH_PostDate Should be equal to new end date", newEndDate2ndPeriod, newJournalCheck2.AH_PostDate);
			AssertEquals("AH_DueDate Should be equal to new start date", newStartDate3rdPeriod, newJournalCheck2.AH_DueDate);
			foreach (GLJournalLine journalLineCheck in newJournalCheck2.Lines)
			{
				AssertEquals("AL_PostDate Should be equal to new end date", newEndDate2ndPeriod, journalLineCheck.AL_PostDate);
				AssertEquals("AL_ReverseDate Should be equal to new start date", newStartDate3rdPeriod, journalLineCheck.AL_ReverseDate);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateRJL_Enabled()
		{
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var postDate = new ZDateTime(1999, 8, 15);
				var dueDate = new ZDateTime(1999, 9, 10);
				SetUpTestJournalWithLines(TransactionTypes.GLReversingJournal, postDate, dueDate);

				var journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
				AssertEquals("Post Date should be overridden", postDate, journalCheck.AH_PostDate);
				AssertEquals("Due Date should be overridden", dueDate, journalCheck.AH_DueDate);

				var periods = TestManager.Periods;

				var endDate2ndPeriod = periods[1].AM_EndDate;
				AssertEquals("PreCondition, 2nd Period start date", new ZDateTime(1999, 8, 1), periods[1].AM_StartDate.Date);
				AssertEquals("PreCondition, 2nd Period end date", new ZDateTime(1999, 8, 31), endDate2ndPeriod.Date);

				periods[1].AM_EndDate = postDate.AddDays(-1);
				AssertHasErrors("Should be prevented, because of Post Date found in the shift interval (> 14/8/99)", periods[1].AM_EndDateInfo);
				periods[1].AM_EndDate = postDate;
				AssertNoErrors("2nd period end date changed 15/8/99", periods[1].AM_EndDateInfo);
				periods[1].AM_EndDate = endDate2ndPeriod.AddDays(-1);
				AssertNoErrors("2nd period end date changed 30/8/99", periods[1].AM_EndDateInfo);
				periods[1].AM_EndDate = endDate2ndPeriod.AddDays(1);
				AssertNoErrors("2nd period end date changed 1/9/99", periods[1].AM_EndDateInfo);
				periods[1].AM_EndDate = dueDate.AddDays(-1);
				AssertNoErrors("2nd period end date changed", periods[1].AM_EndDateInfo);
				AssertEquals("2nd period end date changed 9/9/99", dueDate.AddDays(-1), periods[1].AM_EndDate.Date);
				periods[1].AM_EndDate = dueDate;
				AssertHasErrors("Should be prevented, because of Rev Date found in the shift interval (<= 10/9/99)", periods[1].AM_EndDateInfo);

				AssertEquals("3rd period start date changed 10/9/99", dueDate, periods[2].AM_StartDate.Date);

				TestManager.Factory.Save();

				var newJournalCheck = new BusinessObjectFactory().Load<GLJournal>(TestJournal.PK);
				AssertEquals("AH_PostDate should be unchanged 15/8/99", postDate, newJournalCheck.AH_PostDate);
				AssertEquals("AH_DueDate should be unchanged 10/9/99", dueDate, newJournalCheck.AH_DueDate);
				foreach (GLJournalLine journalLineCheck in newJournalCheck.Lines)
				{
					AssertEquals("AL_PostDate should be unchanged 15/8/99", postDate, journalLineCheck.AL_PostDate);
					AssertEquals("AL_ReverseDate should be unchanged 10/9/99", dueDate, journalLineCheck.AL_ReverseDate);
				}
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateRJLDueDateInNextYearPeriod()
		{
			TestCaseHelper.ClearTable(AccTransactionMatchLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var postDate = new ZDateTime(1999, 8, 15);
			var dueDate = new ZDateTime(2000, 9, 10);
			SetUpTestJournalWithLines(TransactionTypes.GLReversingJournal, postDate, dueDate);

			var periods = TestManager.Periods;
			//Edit End dates of 2nd period
			var endDate2ndPeriod = periods[1].AM_EndDate;
			AssertEquals("2nd period end date is 31/8/99", new ZDateTime(1999, 8, 31), endDate2ndPeriod.Date);
			endDate2ndPeriod = periods[1].AM_EndDate = endDate2ndPeriod.AddDays(10);
			AssertEquals("2nd period end date changed 10/9/99", dueDate.AddYears(-1), endDate2ndPeriod.Date);
			TestManager.Factory.Save();

			//Edit start date of 3rd Period Next Year
			TestManager.FinancialYear = 2001;
			var newStartDate3rdPeriodNextYear = periods[2].AM_StartDate;
			AssertEquals("3rd period start date is 1/9/2000", new ZDateTime(2000, 9, 1), newStartDate3rdPeriodNextYear.Date);
			newStartDate3rdPeriodNextYear = periods[2].AM_StartDate = newStartDate3rdPeriodNextYear.AddDays(10);
			AssertEquals("3rd period start date changed 11/9/2000", dueDate.AddDays(1), newStartDate3rdPeriodNextYear.Date);
			//Save TransactionHeader, TransactionLines
			TestManager.Factory.Save();

			//Check the amended AH_PostDate, AH_DueDate, AL_PostDate, AL_ReverseDate
			GLJournal journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
			AssertEquals("AH_PostDate Should be equal to new end date", endDate2ndPeriod.Date, journalCheck.AH_PostDate.Date);
			AssertEquals("AH_DueDate Should be equal to new start date", newStartDate3rdPeriodNextYear.Date, journalCheck.AH_DueDate.Date);
			foreach (GLJournalLine journalLineCheck in journalCheck.Lines)
			{
				AssertEquals("AL_PostDate Should be equal to new end date", endDate2ndPeriod.Date, journalLineCheck.AL_PostDate.Date);
				AssertEquals("AL_ReverseDate Should be equal to new start date", newStartDate3rdPeriodNextYear.Date, journalLineCheck.AL_ReverseDate.Date);
			}
		}

		[TestDate(1999, 7, 2)]
		public void TestUpdatePostDateAJL()
		{
			TestCaseHelper.ClearTable(AccTransactionMatchLinkSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionLinesSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccTransactionHeaderSchema.Constants.TableName);

			var postDate = new ZDateTime(1999, 8, 15);
			var dueDate = new ZDateTime(1999, 9, 10);
			SetUpTestJournalWithLines(TransactionTypes.GLAutoJournal, postDate, dueDate);

			var periods = TestManager.Periods;
			//Edit End date of 2nd period
			var endDate2ndPeriod = periods[1].AM_EndDate;
			AssertEquals("2nd period end date is 31/8/99", new ZDateTime(1999, 8, 31), endDate2ndPeriod.Date);
			endDate2ndPeriod = periods[1].AM_EndDate = endDate2ndPeriod.AddDays(10);
			AssertEquals("2nd period end date changed 10/9/99", dueDate, endDate2ndPeriod.Date);
			//Edit End date of 3rd period
			var endDate3rdPeriod = periods[2].AM_EndDate;
			AssertEquals("3rd period end date is 30/9/99", new ZDateTime(1999, 9, 30), endDate3rdPeriod.Date);
			endDate3rdPeriod = periods[2].AM_EndDate = endDate3rdPeriod.AddDays(10);
			AssertEquals("3rd period end date changed 10/10/99", dueDate.AddMonths(1), endDate3rdPeriod.Date);
			//Update TransactionHeader, TransactionLines
			TestManager.Factory.Save();

			//Check the amended AH_PostDate, AH_DueDate, AL_PostDate, AL_ReverseDate
			var journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
			AssertEquals("Should be equal to new end date", endDate2ndPeriod, journalCheck.AH_PostDate);
			AssertEquals("Should be equal to new end date", endDate3rdPeriod, journalCheck.AH_DueDate);
			foreach (GLJournalLine journalLineCheck in journalCheck.Lines)
			{
				AssertEquals("Should be equal to new end date", endDate2ndPeriod, journalLineCheck.AL_PostDate);
				AssertEquals("Should be equal to new end date", endDate3rdPeriod, journalLineCheck.AL_ReverseDate);
			}

			using (AccountingConfigurationRegistry.Instance.EnablePostDateGLJournal.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				//Edit End date of 2nd period
				endDate2ndPeriod = periods[1].AM_EndDate = endDate2ndPeriod.AddDays(5);
				AssertEquals("2nd period end date changed 15/9/99", dueDate.AddDays(5), endDate2ndPeriod.Date);
				//Edit End date of 3rd period
				endDate3rdPeriod = periods[2].AM_EndDate = endDate3rdPeriod.AddDays(5);
				AssertEquals("3rd period end date changed 15/10/99", dueDate.AddMonths(1).AddDays(5), endDate3rdPeriod.Date);
				//Update TransactionHeader, TransactionLines
				TestManager.Factory.Save();

				//Check the amended AH_PostDate, AH_DueDate, AL_PostDate, AL_ReverseDate
				journalCheck = ReadOnlyFactory.Load<GLJournal>(TestJournal.PK);
				AssertEquals("Should be equal to new end date", endDate2ndPeriod, journalCheck.AH_PostDate);
				AssertEquals("Should be equal to new end date", endDate3rdPeriod, journalCheck.AH_DueDate);
				foreach (GLJournalLine journalLineCheck in journalCheck.Lines)
				{
					AssertEquals("Should be equal to new end date", endDate2ndPeriod, journalLineCheck.AL_PostDate);
					AssertEquals("Should be equal to new end date", endDate3rdPeriod, journalLineCheck.AL_ReverseDate);
				}
			}
		}

		[TestDate(2015, 3, 1)]
		public void TestCanClosePeriodDefaultARInvoiceDate()
		{
			var firstPeriod = Factory.New<Period>();
			firstPeriod.AM_Period = 201501;
			firstPeriod.AM_StartDate = new ZDateTime(2015, 1, 1);
			firstPeriod.AM_EndDate = new ZDateTime(2015, 1, 31);
			var secondPeriod = Factory.New<Period>();
			secondPeriod.AM_Period = 201502;
			secondPeriod.AM_StartDate = new ZDateTime(2015, 2, 1);
			secondPeriod.AM_EndDate = new ZDateTime(2015, 2, 28);
			//no config
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));
			AssertEquals(string.Empty, TestManager.CanClosePeriod(firstPeriod, false));
			AssertEquals(string.Empty, TestManager.CanClosePeriod(secondPeriod, false));
			//default registry
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(ZDateTime.MinSmallDateTimeValue, ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));
			AssertEquals(string.Empty, TestManager.CanClosePeriod(firstPeriod, false));
			AssertEquals(string.Empty, TestManager.CanClosePeriod(secondPeriod, false));
			//MTH
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new ZDateTime(2015, 1, 3).ToDateTime());
			AssertEquals(new ZDateTime(2015, 1, 31), ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(ZDateTime.MinSmallDateTimeValue));
			AssertEquals(string.Empty, TestManager.CanClosePeriod(secondPeriod, false));
			var errormsg = TestManager.CanClosePeriod(firstPeriod, false);
			AssertEquals(@"You cannot close the period as current invoice date is within the period.
Please review or reinstate the invoice date before closing the period.", errormsg);
		}

		AccGLHeader OtherIncomeGLAccount;
		AccGLHeader ClearingGLAccount;
		AccGLHeader RentalGLAccount;
		GLJournal TestJournal;
		PeriodManager TestManager;
		TestObjectCreator TestObjectCreator;
		BusinessObjectFactory ReadOnlyFactory;
		protected override void SetUp()
		{
			base.SetUp();
			ReadOnlyFactory = new BusinessObjectFactory();
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			TestCaseHelper.ClearTable(AccGLAggregateSchema.Constants.TableName);
			TestObjectCreator = new TestObjectCreator(Factory);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(1999, 7, 1);
			TestManager = new PeriodManager(Factory);
			TestManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();
			testPeriodSettings.StartDate = new ZDateTime(2000, 7, 1);
			TestManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();
			TestManager.FinancialYear = 2000;
			//Setup Other Income GL Account
			OtherIncomeGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			OtherIncomeGLAccount.AG_AccountNum = "9100.00.00";
			OtherIncomeGLAccount.AG_AccountType = "P&L";
			OtherIncomeGLAccount.AG_DebitCredit = "DR";
			//Setup GL Account
			RentalGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			RentalGLAccount.AG_AccountNum = "9300.00.00";
			RentalGLAccount.AG_AccountType = "P&L";
			RentalGLAccount.AG_DebitCredit = "CR";
			//Setup Other Clearing Account
			ClearingGLAccount = Factory.NewWithValidTestData<AccGLHeader>();
			ClearingGLAccount.AG_AccountNum = "9200.00.00";
			ClearingGLAccount.AG_AccountType = "P&L";
			ClearingGLAccount.AG_DebitCredit = "DR";
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ClearingGLAccount.PK.ToGuid());
			TestJournal = Factory.NewWithValidTestData<GLJournal>();
			TestJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;
			TestJournal.AH_PostDate = new ZDateTime(1999, 7, 2);
			TestJournal.PostPeriod = 200001;
		}

		void SetUpTestJournalWithLines(ZString transType, ZDateTime postDate, ZDateTime? dueDate)
		{
			TestJournal.AH_TransactionType = transType;
			TestJournal.AH_PostDate = postDate;
			if (dueDate.HasValue)
			{
				TestJournal.AH_DueDate = dueDate.Value;
			}

			GLJournalLine oIJournalLine = TestJournal.GLJournalLines.AddNew();
			oIJournalLine.AL_PostDate = postDate;
			if (dueDate.HasValue)
			{
				oIJournalLine.AL_ReverseDate = dueDate.Value;
			}
			oIJournalLine.AL_AG = OtherIncomeGLAccount.PK;
			oIJournalLine.AL_OSExTaxAmount = 10m;
			oIJournalLine.DebitCreditSign = "DR";
			oIJournalLine.AL_GB = GlbBranch.CurrentBranch.PK;

			GLJournalLine oIJournalLine2 = TestJournal.GLJournalLines.AddNew();
			oIJournalLine2.AL_PostDate = postDate.AddDays(-4);
			if (dueDate.HasValue)
			{
				oIJournalLine2.AL_ReverseDate = dueDate.Value;
			}
			oIJournalLine2.AL_AG = RentalGLAccount.PK;
			oIJournalLine2.AL_OSExTaxAmount = 10m;
			oIJournalLine2.DebitCreditSign = "CR";
			oIJournalLine2.AL_GB = GlbBranch.CurrentBranch.PK;

			Factory.Save();
		}
	}

	[TestedType(typeof(PeriodManager))]
	public class PeriodManagerTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PeriodManager(Factory);
		}

		[TestDate(1999, 7, 2)]
		public void TestCreatePeriodData()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(1999, 7, 1);
			testPeriodSettings.EndDate = new ZDateTime(2000, 7, 5);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();
			PeriodCollection periodCollection = new PeriodCollection(Factory, new ZQuery(), testManager);
			periodCollection.Load();
			AssertEquals("There should be 12 Period objects in the database", 12, periodCollection.Count);
			AssertEquals("The end date of the last period should be 5/7/2000", new ZDateTime(2000, 7, 5), periodCollection[periodCollection.Count - 1].AM_EndDate.Date);
			Assert("All periods should have been created with the 2000 year", periodCollection.Cast<AccPeriodManagement>().All(x => x.AM_Year == 2000));
		}

		[TestDate(1999, 7, 2)]
		public void TestCreatePeriodDataWithYearOverride()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(1999, 7, 1);
			testPeriodSettings.EndDate = new ZDateTime(2000, 7, 5);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			int yearIdentifierOtherThanYearOfPeriodEndDate = 1997;
			testManager.CreatePeriodData(testPeriodSettings, Factory, yearIdentifierOtherThanYearOfPeriodEndDate);
			Factory.Save();
			PeriodCollection periodCollection = new PeriodCollection(Factory, new ZQuery(), testManager);
			periodCollection.Load();
			AssertEquals("There should be 12 Period objects in the database", 12, periodCollection.Count);
			AssertEquals("The end date of the last period should be 5/7/2000", new ZDateTime(2000, 7, 5), periodCollection[periodCollection.Count - 1].AM_EndDate.Date);
			Assert("All periods should have been created with the 1997 year", periodCollection.Cast<AccPeriodManagement>().All(x => x.AM_Year == yearIdentifierOtherThanYearOfPeriodEndDate));
		}

		[TestDate(1999, 7, 2)]
		public void TestNextUnClosedSubLedgerPeriod()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(1999, 7, 1);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();
			AssertEquals("Should be 200001", 200001, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
		}

		[TestDate(1999, 7, 2)]
		public void TestNextUnClosedGLPeriod()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(1999, 7, 1);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();
			AssertEquals("Should be 200001", 200001, testManager.NextUnClosedGLPeriod.AM_Period);
		}

		public void TestDoesTransactionExistInCurrentCompany()
		{
			PeriodManager testManager = new PeriodManager(Factory);
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			APPayment testPayment = Factory.NewWithValidTestData<APPayment>();
			testPayment.AH_GB = newBranch.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.AH_ChequeOrReference = "10000";
			Factory.Save();
			AssertEquals("Should be false", false, testManager.DoesTransactionExistInCurrentCompany);
			testPayment = Factory.NewWithValidTestData<APPayment>();
			testPayment.AH_GB = GlbBranch.CurrentBranch.PK;
			testPayment.AH_ReceiptType = ReceiptTypes.Cheque;
			testPayment.AH_ChequeOrReference = "10000";
			Factory.Save();
			AssertEquals("Should be true", true, testManager.DoesTransactionExistInCurrentCompany);
		}

		public void TestCloseSubLedgerPeriodWithParameter()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				using (var conn = Db.NewAdminConnection())
				{
					conn.ExecuteNonQuery("DBCC FREEPROCCACHE WITH NO_INFOMSGS");
				}

				testManager.CloseSubLedgerPeriod();

				var periodPk = testManager.NextUnClosedSubLedgerPeriod.PK;
				var unexpectedPlanCache = $"SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL FROM dbo.AccCurrencyAdjustmentQueue WHERE ACA_ParentID = '{periodPk.ToGuid()}') THEN 1 ELSE 0 END);";
				var expectedPlanCache = "(@PeriodPK uniqueidentifier)SELECT CONVERT(bit, CASE WHEN EXISTS(SELECT NULL FROM dbo.AccCurrencyAdjustmentQueue WHERE ACA_ParentID = @PeriodPK) THEN 1 ELSE 0 END);";

				var sqlGetPlanCacheCount = $@"
SELECT Count(1)
FROM sys.dm_exec_cached_plans AS c
CROSS APPLY sys.dm_exec_sql_text(plan_handle) AS t
WHERE TEXT = @Text";

				using (var conn = Db.NewAdminConnection())
				{
					using (var cmd = conn.Command(sqlGetPlanCacheCount))
					{
						cmd.CommandType = System.Data.CommandType.Text;
						cmd.AddParameter("@Text", System.Data.SqlDbType.VarChar, unexpectedPlanCache);
						var count = (int)cmd.ExecuteScalar();
						AssertEquals(0, count);
					}
				}

				using (var conn = Db.NewAdminConnection())
				{
					using (var cmd = conn.Command(sqlGetPlanCacheCount))
					{
						cmd.CommandType = System.Data.CommandType.Text;
						cmd.AddParameter("@Text", System.Data.SqlDbType.VarChar, expectedPlanCache);
						var count = (int)cmd.ExecuteScalar();
						AssertEquals(1, count);
					}
				}
			}
		}

		public void TestCloseSubLedgerPeriod()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();
			AssertEquals("Should be 200701", 200701, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
			testManager.CloseSubLedgerPeriod();
			AssertEquals("Should be 200702", 200702, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
		}

		public void TestInsertAccCurrencyAdjustmentQueueWhenCloseSubLedgerPeriod()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			var testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();

			AssertEquals("Should be 200701", 200701, testManager.NextUnClosedSubLedgerPeriod.AM_Period);

			testManager.CloseSubLedgerPeriod();
			var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
			var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

			Assert(!AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.Value);
			AssertEquals(0, results.Rows.Count);
			AssertEquals("Should be 200702", 200702, testManager.NextUnClosedSubLedgerPeriod.AM_Period);

			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var toCloseSubLedgerPeriod = testManager.NextUnClosedSubLedgerPeriod;
				testManager.CloseSubLedgerPeriod();
				results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

				Assert(AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.Value);
				AssertEquals("Should be 200703", 200703, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
				AssertEquals("Count should be 1", 1, results.Rows.Count);
				AssertEquals("ACA_ParentID", toCloseSubLedgerPeriod.PK, results.Rows[0]["ACA_ParentID"]);
				AssertEquals("ACA_ParentTableCode", AccPeriodManagementSchema.Constants.Prefix, results.Rows[0]["ACA_ParentTableCode"]);
				AssertEquals("ACA_GC", GlbCompany.CurrentCompany.PK, results.Rows[0]["ACA_GC"]);
			}
		}

		public void TestDeleteCurrencyAdjustmentQueueWhenDeleteAllPeriodsInCurrentCompany()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			var testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();

			AssertEquals("Should be 200701", 200701, testManager.NextUnClosedSubLedgerPeriod.AM_Period);

			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
				var toCloseSubLedgerPeriod = testManager.NextUnClosedSubLedgerPeriod;
				testManager.CloseSubLedgerPeriod();
				var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

				Assert(AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.Value);
				AssertEquals("Should be 200702", 200702, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
				AssertEquals("Count should be 1", 1, results.Rows.Count);
				AssertEquals("ACA_ParentID", toCloseSubLedgerPeriod.PK, results.Rows[0]["ACA_ParentID"]);

				testManager.DeleteAllPeriodsInCurrentCompany();

				results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
				AssertEquals(0, results.Rows.Count);
			}
		}

		public void TestDoNotInsertDuplicateRecordIntoAccCurrencyAdjustmentQueue()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			var testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			var testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			Factory.Save();

			AssertEquals("Should be 200701", 200701, testManager.NextUnClosedSubLedgerPeriod.AM_Period);

			var toCloseSubLedgerPeriod = testManager.NextUnClosedSubLedgerPeriod;
			var cmdText = FormattableString.Invariant($@"INSERT INTO dbo.AccCurrencyAdjustmentQueue (ACA_ParentID, ACA_ParentTableCode, ACA_GC, ACA_Date) VALUES (@ParentID, @ParentTableCode, @CompanyPK, @Date)");

			using (var cmd = ((IDbConnected)toCloseSubLedgerPeriod.Factory).Connection.Command(cmdText))
			{
				cmd.CommandType = System.Data.CommandType.Text;
				cmd.AddParameter("@ParentID", System.Data.SqlDbType.UniqueIdentifier, toCloseSubLedgerPeriod.PK.ToGuid());
				cmd.AddParameter("@ParentTableCode", System.Data.SqlDbType.VarChar, AccPeriodManagementSchema.Constants.Prefix);
				cmd.AddParameter("@CompanyPK", System.Data.SqlDbType.UniqueIdentifier, GlbCompany.CurrentCompany.PK.ToGuid());
				cmd.AddParameter("@Date", System.Data.SqlDbType.DateTime, ZDateTime.Now.ToDateTime());
				cmd.ExecuteNonQuery();
			}

			var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
			var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
			AssertEquals("Count should be 1", 1, results.Rows.Count);
			AssertEquals("ACA_ParentID", toCloseSubLedgerPeriod.PK, results.Rows[0]["ACA_ParentID"]);

			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				testManager.CloseSubLedgerPeriod();

				Assert(AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.Value);
				AssertEquals("Should be 200702", 200702, testManager.NextUnClosedSubLedgerPeriod.AM_Period);

				results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);
				AssertEquals("Count should be 1", 1, results.Rows.Count);
				AssertEquals("ACA_ParentID", toCloseSubLedgerPeriod.PK, results.Rows[0]["ACA_ParentID"]);
			}
		}

		public void TestAvoidCloseGLPeriodWithAccCurrencyAdjustmentQueueRecord()
		{
			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testPeriodSettings = new NewYearPeriodSettings();
				testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
				var testManager = (PeriodManager)GetNewBusinessObject();
				testManager.CreatePeriodData(testPeriodSettings, Factory);
				Factory.Save();

				var toCloseSubLedgerPeriod = testManager.NextUnClosedSubLedgerPeriod;
				testManager.CloseSubLedgerPeriod();
				var sql = string.Format(@"SELECT * FROM dbo.AccCurrencyAdjustmentQueue");
				var results = DataUtils.GetDataTableFromQuery(Db.Connection, sql);

				AssertEquals("Count should be 1", 1, results.Rows.Count);
				AssertEquals("ACA_ParentID", toCloseSubLedgerPeriod.PK, results.Rows[0]["ACA_ParentID"]);

				string actualErrorMessage = null;
				testManager.OnCloseSubLedgerError += delegate(object sender, EventArgs e)
				{
					actualErrorMessage = (string)sender;
				};

				var closeResult = testManager.CloseGLPeriod();

				AssertNull("Close should be avoided", closeResult);
				AssertEquals("Cannot close GL period for CurrencyAdjustmentQueue record", @"You cannot close the general ledger for this period as the automated A/R and A/P outstanding balances currency adjustment has not been created.

The automated currency adjustment journal may not be created due to one of the following reasons:
1. The 'CAQ - Currency Adjustment Queue Service Task' is not running.
2. The adjustment journal could not be created due to missing configuration, exchange rate not found or other reasons.

Please check that service task is running and/or rectify the error listed in this accounting period > change logs.", actualErrorMessage);
			}
		}

		public void TestPeriodDoesNotCloseWhenThereAreUnbatchedDirectReceipts()
		{
			TestCaseHelper.ClearTable(AccPeriodManagementSchema.Constants.TableName);
			NewYearPeriodSettings testPeriodSettings = new NewYearPeriodSettings();
			testPeriodSettings.StartDate = new ZDateTime(2006, 7, 1);
			PeriodManager testManager = (PeriodManager)GetNewBusinessObject();
			testManager.CreatePeriodData(testPeriodSettings, Factory);
			DirectReceipt receipt = Factory.NewWithValidTestData<DirectReceipt>();
			receipt.AH_PostDate = new ZDateTime(2006, 7, 5);
			Factory.Save();
			AssertEquals("Should be 200701", 200701, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
			string actualErrorMessage = null;
			testManager.OnCloseSubLedgerError += delegate(object sender, EventArgs e)
			{
				actualErrorMessage = (string)sender;
			}

			;
			testManager.CloseSubLedgerPeriod();
			AssertEquals("Period shouldn't close -- should still be 200701", 200701, testManager.NextUnClosedSubLedgerPeriod.AM_Period);
			AssertEquals("Correct message should be shown", @"You cannot close the sub-ledger for this period as there are receipts posted in this period that are not part of a deposit batch.
Please create deposit batches for these receipts.", actualErrorMessage);
		}
	}
}
