using System;
using CargoWise.Common;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	public class PayablesAndReceivablesReversingTest : ReversingBaseTest
	{
		public virtual void TestCantReverseMatchedIPayablesAndReceivables()
		{
			TestIPayablesAndReceivables.SetIsReversed(false);
			TestIPayablesAndReceivables.SetIsMatched(true);
			Assert("Should never allow reverse on an unreversed but matched transaction", !Reversing.CanReverseTransaction);
			ZString alreadyMatchedError = PayablesAndReceivablesReversing.MatchedAndCantReverseErrorMessage_ForTestOnly;
			AssertEquals("Can't reverse error", alreadyMatchedError, PayablesAndReceivablesReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public void TestReverseErrorMessageForReversedAndThereforeMatchedTransaction()
		{
			TestIPayablesAndReceivables.SetIsReversed(true);
			TestIPayablesAndReceivables.SetIsMatched(true);
			Assert("Should never allow reverse on a reversed and matched transaction", !Reversing.CanReverseTransaction);
			ZString alreadyReversedError = PayablesAndReceivablesReversing.AlreadyReversedErrorMessage_ForTestOnly;
			AssertEquals("Can't reverse error", alreadyReversedError, PayablesAndReceivablesReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		[TestDate(2010, 02, 01)]
		public void TestMatchingPayablesAndReceivablesTransactions()
		{
			TransactionMatchLinkGroup transactionMatchLinkGroup = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink originalTransactionMatchLink = transactionMatchLinkGroup.AddNew();
			originalTransactionMatchLink.AP_Amount = 50.00m;

			TransactionMatchLink reverseTransactionMatchLink = transactionMatchLinkGroup.AddNew();
			reverseTransactionMatchLink.AP_Amount = -50.00m;

			((IPayablesAndReceivablesForTests)TestReversingIReversingInstance).SetMatchLinksToBeGenerated(transactionMatchLinkGroup);
			TestIPayablesAndReceivables.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			TestIPayablesAndReceivables.SetMatchLinksToBeGenerated(transactionMatchLinkGroup);

			var journalForSettingMatchLinkHeadersOnly = Factory.NewWithValidTestData<APJournal>();
			originalTransactionMatchLink.AP_AH = journalForSettingMatchLinkHeadersOnly.PK;
			reverseTransactionMatchLink.AP_AH = journalForSettingMatchLinkHeadersOnly.PK;

			TestReversingIReversingInstance.PostDate = ZDateTime.Now.AddDays(-3).AddHours(-7);

			PayablesAndReceivablesReversing.Reverse();
			if (PayablesAndReceivablesReversing.ReverseTransaction.TransactionNumber.IsEmpty)
			{
				PayablesAndReceivablesReversing.ReverseTransaction.TransactionNumber = "REVERSE0001";
			}

			ZString nextMatchGroupNumber = Env.NumberFountains.MatchNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);

			ZDateTime expectedDate = ZDateTime.Now.AddDays(-7).AddHours(-3);
			TestReversingIReversingInstance.PostDate = expectedDate;

			ITransaction reversedTransaction = TestReversingIReversingInstance;
			if (TestIPayablesAndReceivables.ReverseTransaction != TestReversingIReversingInstance)
			{
				reversedTransaction = TestIPayablesAndReceivables.ReverseTransaction;
				originalTransactionMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestIPayablesAndReceivables.PK));
				reverseTransactionMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversedTransaction.PK));
				((TransactionHeader)reversedTransaction).AH_PostDate = expectedDate;
			}

			Factory.Save();

			ZDateTime fullyPaidDate = TestReversingIReversingInstance.FullyPaidDate;
			if (reversedTransaction is TransactionHeader)
			{
				fullyPaidDate = ((TransactionHeader)reversedTransaction).AH_FullyPaidDate;
			}
			AssertEquals("Reversing invoice should have fully paid date set to post date", expectedDate, fullyPaidDate);

			AssertEquals("All matchlinks should have match group number set to next", nextMatchGroupNumber, originalTransactionMatchLink.AP_MatchGroupNum);
			AssertEquals("All matchlinks should have match date set to today", expectedDate, originalTransactionMatchLink.AP_MatchDate);

			AssertEquals("All matchlinks should have match group number set to next", nextMatchGroupNumber, reverseTransactionMatchLink.AP_MatchGroupNum);
			AssertEquals("All matchlinks should have match date set to today", expectedDate, reverseTransactionMatchLink.AP_MatchDate);
		}

		[TestDate(2010, 02, 01)]
		public void TestTimeoutWhereMatchlinkIsNotSavedWithMatchGroup()
		{
			TransactionMatchLinkGroup transactionMatchLinkGroup = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink originalTransactionMatchLink = transactionMatchLinkGroup.AddNew();
			originalTransactionMatchLink.AP_Amount = 50.00m;

			TransactionMatchLink reverseTransactionMatchLink = transactionMatchLinkGroup.AddNew();
			reverseTransactionMatchLink.AP_Amount = -50.00m;

			((IPayablesAndReceivablesForTests)TestReversingIReversingInstance).SetMatchLinksToBeGenerated(transactionMatchLinkGroup);
			TestIPayablesAndReceivables.SetReverseTransactionToBeGenerated(TestReversingIReversingInstance);
			TestIPayablesAndReceivables.SetMatchLinksToBeGenerated(transactionMatchLinkGroup);

			var journalForSettingMatchLinkHeadersOnly = Factory.NewWithValidTestData<APJournal>();
			originalTransactionMatchLink.AP_AH = journalForSettingMatchLinkHeadersOnly.PK;
			reverseTransactionMatchLink.AP_AH = journalForSettingMatchLinkHeadersOnly.PK;

			TestReversingIReversingInstance.PostDate = ZDateTime.Now.AddDays(-3).AddHours(-7);

			PayablesAndReceivablesReversingWithException.Reverse();

			ZString nextMatchGroupNumber = Env.NumberFountains.MatchNo.GetTodaysPeriodFountain().PeekPreliminaryFormatted(Factory);

			ZDateTime expectedDate = ZDateTime.Now.AddDays(-7).AddHours(-3);
			TestReversingIReversingInstance.PostDate = expectedDate;

			ITransaction reversedTransaction = TestReversingIReversingInstance;
			if (TestIPayablesAndReceivables.ReverseTransaction != TestReversingIReversingInstance)
			{
				reversedTransaction = TestIPayablesAndReceivables.ReverseTransaction;
				originalTransactionMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, TestIPayablesAndReceivables.PK));
				reverseTransactionMatchLink = Factory.LoadTop1<TransactionMatchLink>(new ZQuery(AccTransactionMatchLinkSchema.AP_AH, reversedTransaction.PK));
				((TransactionHeader)reversedTransaction).AH_PostDate = expectedDate;
				if (((TransactionHeader)reversedTransaction).AH_TransactionNum.IsEmpty)
				{
					((TransactionHeader)reversedTransaction).AH_TransactionNum = "REVERSE0001";
				}
			}

			NUnit.Framework.Assert.That(() => Factory.Save(), CustomConstraints.InnermostExceptionThrown(typeof(System.Data.Common.DbException)));

			//Below Factory.Save() is equivalent to clicking Reverse button again

			AssertNoExceptionThrown(() => Factory.Save());

			AssertEquals("No error reported.", 0, ErrorReporter.TotalErrorCount);

			// Asserting that SetMatchGroupNumberAndMatchDateOnSaving has been removed as BusinessObjectFactory.SavingEventHandler

			AssertNoExceptionThrown(() => Factory.Save());
		}

		#region TestUnmatchRelatedMatchingSessions

		public void TestReverseRelatedMatchingSessions()
		{
			// Use a ARJournal
			ARJournal aRJnl = Factory.NewWithValidTestData<ARJournal>();
			PayablesAndReceivablesReversing testPRRev = new PayablesAndReceivablesReversing(aRJnl);

			aRJnl.AH_LocalExTaxAmount = 100M;
			aRJnl.AH_OSExTaxAmount = 100M;
			aRJnl.AH_LocalOutstandingAmount = 0M;

			APJournal aPJnl1 = Factory.NewWithValidTestData<APJournal>();
			aPJnl1.AH_LocalExTaxAmount = 50M;
			aPJnl1.AH_OSExTaxAmount = 50M;
			aPJnl1.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLinkGroup matchlinks = new TransactionMatchLinkGroup(Factory);
			TransactionMatchLink aPJnl1Match = matchlinks.AddNew();
			aPJnl1Match.AP_AH = aPJnl1.PK;
			aPJnl1Match.AP_Amount = -50M;
			aPJnl1Match.AP_MatchGroupNum = "M00001333";

			APJournal aPJnl2 = Factory.NewWithValidTestData<APJournal>();
			aPJnl2.AH_LocalExTaxAmount = 50M;
			aPJnl2.AH_OSExTaxAmount = 50M;
			aPJnl2.AH_LocalOutstandingAmount = 0M;

			TransactionMatchLink aPJnl2Match = matchlinks.AddNew();
			aPJnl2Match.AP_AH = aPJnl2.PK;
			aPJnl2Match.AP_Amount = -50M;
			aPJnl2Match.AP_MatchGroupNum = "M00001444";

			// Matchlinks for the ARJournal passed into the PayablesAndReceivablesReversing Object
			TransactionMatchLink aRJnl1Match = matchlinks.AddNew();
			aRJnl1Match.AP_AH = aRJnl.PK;
			aRJnl1Match.AP_Amount = 50M;
			aRJnl1Match.AP_MatchGroupNum = "M00001333";

			TransactionMatchLink aRJnl2Match = matchlinks.AddNew();
			aRJnl2Match.AP_AH = aRJnl.PK;
			aRJnl2Match.AP_Amount = 50M;
			aRJnl2Match.AP_MatchGroupNum = "M00001444";
			TestObjectCreator.SetupMatchLinkMatchDate(matchlinks);

			Factory.Save();

			Assert("Should not be able to reverse the Journal", !testPRRev.CanReverseTransaction);
		}

		#endregion

		#region TransactionCreationRestriction

		public void TestTransactionCreationRestriction_INV_Reverse()
		{
			AccTransactionHeader transaction;
			if ((transaction = TestReversingIReversingInstance as AccTransactionHeader) != null)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				transaction.AH_OH = org.PK;
				Factory.Save();
				Assert(transaction.IsInDatabase);

				transaction.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
				transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.Invoice;
				Factory.Save();

				PayablesAndReceivablesReversing.Reverse();
				AccTransactionHeader reverseTransaction;
				if ((reverseTransaction = PayablesAndReceivablesReversing.ReverseTransaction as AccTransactionHeader) != null &&
					reverseTransaction.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					reverseTransaction.AH_TransactionNum = "Test";
				}
				Factory.Save();
				Assert(PayablesAndReceivablesReversing.ReverseTransaction.IsInDatabase);

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert("Not support", true);
			}
		}

		public void TestTransactionCreationRestriction_ALL_Reverse()
		{
			AccTransactionHeader transaction;
			if ((transaction = TestReversingIReversingInstance as AccTransactionHeader) != null)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				transaction.AH_OH = org.PK;
				Factory.Save();
				Assert(transaction.IsInDatabase);

				transaction.Header.CompanyData.OB_APTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
				transaction.Header.CompanyData.OB_ARTransactionCreationRestriction = Core.Constants.TransactionCreationRestriction.All;
				Factory.Save();

				PayablesAndReceivablesReversing.Reverse();
				AccTransactionHeader reverseTransaction;
				if ((reverseTransaction = PayablesAndReceivablesReversing.ReverseTransaction as AccTransactionHeader) != null &&
					reverseTransaction.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					reverseTransaction.AH_TransactionNum = "Test";
				}

				Factory.Save();
				Assert(PayablesAndReceivablesReversing.ReverseTransaction.IsInDatabase);

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				Assert("Not support", true);
			}
		}

		#endregion

		#region Implementation

		protected override Type GetTestingClassType()
		{
			return typeof(PayablesAndReceivablesReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIPayablesAndReceivables();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIPayablesAndReceivables();
		}

		protected PayablesAndReceivablesReversing PayablesAndReceivablesReversing
		{
			get { return (PayablesAndReceivablesReversing)Reversing; }
		}

		protected IPayablesAndReceivablesForTests TestIPayablesAndReceivables
		{
			get { return (IPayablesAndReceivablesForTests)TestIReversingInstance; }
		}

		protected PayablesAndReceivablesReversing PayablesAndReceivablesReversingWithException
		{
			get { return new PayablesAndReceivablesReversingWithExceptionOnSaving((IPayablesAndReceivablesForTests)TestIReversingInstance); }
		}

		class PayablesAndReceivablesReversingWithExceptionOnSaving : PayablesAndReceivablesReversing
		{
			public PayablesAndReceivablesReversingWithExceptionOnSaving(IPayablesAndReceivables payablesAndReceivablesTransaction) : base(payablesAndReceivablesTransaction)
			{
			}

			protected override ZString GetMatchGroupNumber()
			{
				if (!sqlExceptionThrown)
				{
					sqlExceptionThrown = true;
					var timeoutException = SqlExceptionBuilder.CreateSqlException(
						SqlExceptionBuilder.CreateSqlErrorCollection(
						SqlExceptionBuilder.CreateSqlError(-2, 0, 11, CargoWise.Data.Db.ServerName, "Server is taking too long to respond. Please try again.", "", 0)));
					throw timeoutException;
				}
				else
				{
					sqlExceptionThrown = false;
					return base.GetMatchGroupNumber();
				}
			}

			bool sqlExceptionThrown;
		}

		#endregion
	}
}
