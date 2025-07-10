using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class GLJournalReversingTest : ReversingBaseTest
	{
		protected override Type GetTestingClassType()
		{
			return typeof(GLJournalReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIGeneralLedgerTransaction();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIGeneralLedgerTransaction();
		}

		public void TestReversingDescription()
		{
			SetupGLJournal();
			GLJournalReversing.SetReversingDescriptionOnTransactions_ForTestOnly();
			Assert("Reversing Journal should not have reversing description",
				!((GLJournal)GLJournalReversing.ReverseTransaction).AH_Desc.StartsWith("Reversal related to"));
		}

		public override void TestTransactionReversedEmailSentOnSaving()
		{
			SetupStaffMemberEmailAddress();
			AccountingConfigurationRegistry.Instance.TransactionReverseNotifyGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, StaffGroupPK.ToGuid());
			TestIReversingInstance.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			TestIReversingInstance.Factory.Save();

			AssertEquals("Should not send email", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSetTransactionBelongsToGroupOnTransactionsToReverse()
		{
			SetupGLJournal();
			GLJournalReversing.Reverse();
			Assert(!Journal.AH_TransactionBelongsToGroup.IsEmpty);
			AssertEquals(Journal.AH_TransactionBelongsToGroup, ((GLJournal)GLJournalReversing.ReverseTransaction).AH_TransactionBelongsToGroup);
		}

		[TestDate(2020, 5, 25)]
		public virtual void TestSetReverseJournalValues()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			Journal = Factory.NewWithValidTestData<GLJournal>();

			Journal.AH_PostDate = new ZDateTime(2020, 4, 30);
			Journal.AH_DueDate = new ZDateTime(2020, 5, 31);
			var postPeriod = Journal.PeriodCalculator.GetPeriodManagementFromDate(Journal.AH_PostDate);
			var duePeriod = Journal.PeriodCalculator.GetPeriodManagementFromDate(Journal.AH_DueDate);
			Journal.AH_TransactionNum = "00001000";
			Factory.Save();

			Reversing = new GLJournalReversing(Journal);
			GLJournalReversing.GenerateReverseTransactions_ForTestOnly();
			var reverseTransaction = (GLJournal)GLJournalReversing.ReverseTransaction;

			AssertNotNull(reverseTransaction);
			AssertEquals("default to original when period not close", 202004, reverseTransaction.PostPeriod);
			AssertEquals("default to original when period not close", 202005, reverseTransaction.AgePeriod);

			postPeriod.AM_IsGeneralLedgerClosed = true;
			duePeriod.AM_IsGeneralLedgerClosed = true;
			Reversing = new GLJournalReversing(Journal);
			GLJournalReversing.GenerateReverseTransactions_ForTestOnly();
			reverseTransaction = (GLJournal)GLJournalReversing.ReverseTransaction;

			AssertNotNull(reverseTransaction);
			AssertEquals("default to current when period closed", 202005, reverseTransaction.PostPeriod);
			AssertEquals("default to empty when period closed", AccountingPeriodCalculator.InvalidPeriod, reverseTransaction.AgePeriod);
		}

		protected GLJournalReversing GLJournalReversing
		{
			get { return Reversing as GLJournalReversing; }
		}

		protected void SetupGLJournal()
		{
			Journal = Factory.NewWithValidTestData<GLJournal>();
			Reversing = ReversingFactory.NewReversing(Journal);
			GLJournalReversing.GenerateReverseTransactions_ForTestOnly();
		}

		GLJournal Journal;
	}
}
