using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.PeriodManagement;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class AutoCurrencyAdjustmentGLJournalReversingTest : GLJournalReversingTest
	{
		protected override Type GetTestingClassType()
		{
			return typeof(AutoCurrencyAdjustmentGLJournalReversing);
		}

		public void TestSetCancellationFlagOnTransactionsToReverse()
		{
			SetupGLJournal();
			GLJournalReversing.Reverse();
			Assert(originalJournal.AH_IsCancelled);
			Assert(((GLJournal)GLJournalReversing.ReverseTransaction).AH_IsCancelled);
		}

		[TestDate(2020, 4, 20)]
		public override void TestSetReverseJournalValues()
		{
			new TestObjectCreator(Factory).CreateTestPeriodsForEntireYear(2020);
			originalJournal = Factory.NewWithValidTestData<GLJournal>();

			originalJournal.AH_PostDate = new ZDateTime(2020, 4, 30);
			originalJournal.AH_DueDate = new ZDateTime(2020, 5, 31);
			var currentPeriod = originalJournal.PeriodCalculator.GetPeriodManagementFromDate(originalJournal.AH_PostDate);
			originalJournal.PeriodPK = currentPeriod.PK;
			originalJournal.AH_TransactionNum = "00001000";
			originalJournal.AH_Desc = "A/R AND A/P OUTSTANDING BALANCE CURRENCY ADJUSTMENT JOURNAL";
			Factory.Save();

			Reversing = ReversingFactory.NewReversing(originalJournal);
			GLJournalReversing.GenerateReverseTransactions_ForTestOnly();

			var reverseTransaction = (GLJournal)GLJournalReversing.ReverseTransaction;
			AssertNotNull(reverseTransaction);
			AssertEquals(originalJournal.AH_PostDate, reverseTransaction.AH_PostDate);
			AssertEquals(originalJournal.AH_DueDate, reverseTransaction.AH_DueDate);
			AssertEquals(202004, reverseTransaction.PostPeriod);
			AssertEquals(202005, reverseTransaction.AgePeriod);
			AssertEquals(currentPeriod.PK, reverseTransaction.PeriodPK);
			AssertEquals($"REVERSAL OF JOURNAL NUMBER 00001000 – A/R AND A/P OUTSTANDING BALANCE CURRENCY ADJUSTMENT JOURNAL", reverseTransaction.AH_Desc);
		}

		protected new AutoCurrencyAdjustmentGLJournalReversing GLJournalReversing => Reversing as AutoCurrencyAdjustmentGLJournalReversing;

		protected new void SetupGLJournal()
		{
			originalJournal = Factory.NewWithValidTestData<GLJournal>();
			var testPeriod = Factory.New<Period>();
			testPeriod.AM_Period = 202001;
			testPeriod.AM_StartDate = new ZDateTime(2019, 7, 1);
			testPeriod.AM_EndDate = new ZDateTime(2019, 7, 31);
			originalJournal.PeriodPK = testPeriod.PK;
			Reversing = ReversingFactory.NewReversing(originalJournal);
			GLJournalReversing.GenerateReverseTransactions_ForTestOnly();
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIAutoCurrencyAdjustmentGLJournal();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIAutoCurrencyAdjustmentGLJournal();
		}

		protected override void SetupReversingObject()
		{
			Reversing = ReversingFactory.NewReversing(TestIReversingInstance as IGeneralLedger);
		}

		GLJournal originalJournal;
	}
}
