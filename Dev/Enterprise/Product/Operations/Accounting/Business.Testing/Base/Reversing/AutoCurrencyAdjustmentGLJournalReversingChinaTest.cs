using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class AutoCurrencyAdjustmentGLJournalReversingChinaTest : AutoCurrencyAdjustmentGLJournalReversingTest
	{
		protected override Type GetTestingClassType()
		{
			return typeof(AutoCurrencyAdjustmentGLJournalReversingChina);
		}

		public void TestReversingPairJournal()
		{
			using (AccountingConfigurationRegistry.Instance.AutoCreateARAndAPOutstandingBalancesCurrencyAdjustments.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				originalJournal = Factory.NewWithValidTestData<GLJournal>();
				Reversing = new AutoCurrencyAdjustmentGLJournalReversingChina(originalJournal);
				originalJournal.AH_TransactionBelongsToGroup = ZGuid.NewZGuid();
				var testPeriod = Factory.New<Period>();
				testPeriod.AM_Period = 202001;
				testPeriod.AM_StartDate = new ZDateTime(2019, 7, 1);
				testPeriod.AM_EndDate = new ZDateTime(2019, 7, 31);
				originalJournal.PeriodPK = testPeriod.PK;
				originalJournal.AH_TransactionType = TransactionTypes.GLStandardJournal;

				var pairJournal = Factory.NewWithValidTestData<GLJournal>();
				pairJournal.PeriodPK = originalJournal.PeriodPK;
				pairJournal.AH_TransactionType = originalJournal.AH_TransactionType;
				pairJournal.AH_TransactionBelongsToGroup = originalJournal.AH_TransactionBelongsToGroup;

				GLJournalReversing.Reverse();
				Factory.Save();

				ZQuery filter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, originalJournal.AH_TransactionBelongsToGroup);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, originalJournal.AH_TransactionType);
				filter.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, true);
				filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, originalJournal.PK);
				filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, pairJournal.PK);
				filter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, GLJournalReversing.ReverseTransaction.PK);
				var pairJournalreversed = originalJournal.Factory.LoadTop1<GLJournal>(filter);

				AssertNotNull("reverse journal for pair", pairJournalreversed);
				AssertEquals("The PairJournal of OriginalJoural should be pairJournal which has same AH_TransactionBelongsToGroup", pairJournal, originalJournal.PairTransaction);
			}
		}

		protected new AutoCurrencyAdjustmentGLJournalReversingChina GLJournalReversing => Reversing as AutoCurrencyAdjustmentGLJournalReversingChina;

		protected new void SetupGLJournal()
		{
			originalJournal = Factory.NewWithValidTestData<GLJournal>();
			Reversing = new AutoCurrencyAdjustmentGLJournalReversingChina(originalJournal);
			GLJournalReversing.GenerateReverseTransactions_ForTestOnly();
		}

		protected override void SetupReversingObject()
		{
			Reversing = new AutoCurrencyAdjustmentGLJournalReversingChina(TestIReversingInstance as IGeneralLedger);
		}

		GLJournal originalJournal;
	}
}
