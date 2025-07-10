using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using JournalBase = Enterprise.Accounting.Business.ARAP.Journal.Journal;

namespace Enterprise.Accounting.GUI.ARAP.Journal.Testing
{
	[TestedType(typeof(BankFeeJournalForm))]
	public class BankFeeJournalFormTest : ZFormBasherTest
	{
		protected ARJournal BankFeeJournal;
		protected MatchingBase TestMatchingBase;

		protected override Form GetFormToBashCore()
		{
			TestMatchingBase = new ARMatchingBase(Factory);
			BankFeeJournal = Factory.New<ARJournal>();
			((IMiscellaneousTransaction)BankFeeJournal).MatchingBizO = TestMatchingBase;
			return new BankFeeJournalForm(BankFeeJournal);
		}

		BankFeeJournalForm GetBankFeeJournalForm(ARJournal arJournal, ARMatchingBase matchingBase)
		{
			((IMiscellaneousTransaction)arJournal).MatchingBizO = matchingBase;
			return new BankFeeJournalForm(arJournal);
		}

		public void TestSavingNewDoesNotSaveToDb()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.ValidateAndSave_ForTestOnly();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				JournalBase loadedJournal = newFactory.Load<JournalBase>(BankFeeJournal.PK);
				AssertNull("The journal should not be saved to the DB", loadedJournal);
				Assert("The journal should not be deleted since it was just saved", !BankFeeJournal.IsDeleted);
			}
		}

		public void TestCancelNewUnsavedFormDeletesBizO()
		{
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				AssertNoExceptionThrown(() =>
				{
					testForm.Show();
					testForm.FCancelButton_ForTestOnly.PerformClick();
				});
				Assert("Journal should be deleted since it was cancelled", BankFeeJournal.IsDeleted);
			}
		}

		public void TestARJournalHasRunDelete()
		{
			var journal = Factory.New<ARJournalForTest>();

			var matchingBase = new ARMatchingBase(Factory);
			matchingBase.AddMiscellaneousTransaction(journal);

			using (var testForm = GetBankFeeJournalForm(journal, matchingBase))
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				journal.AH_LocalExTaxAmount = 0M;
				AssertNoExceptionThrown(() =>
				{
					testForm.Show();
					testForm.FCancelButton_ForTestOnly.PerformClick();
				});
			}

			Assert("Journal should be deleted", journal.HasDeleteFromDB);
		}

		public void TestARJournalHasRunDeleteWithHeaderDifferentFromMisc()
		{
			var journal = Factory.New<ARJournalForTest>();
			var journalBankFee = Factory.New<ARJournalForTest>();

			var matchingBase = new ARMatchingBase(Factory);
			matchingBase.AddMiscellaneousTransaction(journalBankFee);

			using (var testForm = GetBankFeeJournalForm(journal, matchingBase))
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				journal.AH_LocalExTaxAmount = 0M;
				AssertNoExceptionThrown(() =>
				{
					testForm.Show();
					testForm.FCancelButton_ForTestOnly.PerformClick();
				});
			}

			Assert("Journal should be deleted", journal.HasDeleteFromDB);
			AssertEquals("journalBankFee should not be deleted,it's base on current logic", false, journalBankFee.HasDeleteFromDB);
		}

		public void TestCancelingEditDoesNotDelete()
		{
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				BankFeeJournal.AH_LocalExTaxAmount = 1M;
				AssertNoExceptionThrown(() =>
				{
					testForm.Show();
					testForm.FCancelButton_ForTestOnly.PerformClick();
				});
				Assert("Journal should not be deleted", !BankFeeJournal.IsDeleted);
			}
		}

		public void TestSavingEditDoesNotDelete()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				testForm.ValidateAndSave_ForTestOnly();
				BusinessObjectFactory newFactory = new BusinessObjectFactory();
				JournalBase loadedJournal = newFactory.Load<JournalBase>(BankFeeJournal.PK);
				AssertNull("The journal should not be saved to the DB", loadedJournal);
				Assert("The journal should not be deleted", !BankFeeJournal.IsDeleted);
			}
		}

		public void TestClosingEditDoesNotDelete()
		{
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.Edit;
				BankFeeJournal.AH_LocalExTaxAmount = 45M;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("Journal should not be deleted", !BankFeeJournal.IsDeleted);
			}
		}

		public void TestClosingNewDeletesBizO()
		{
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("Journal should be deleted", BankFeeJournal.IsDeleted);
			}
		}

		public void TestClosingAmountNonZeroDoesNotDelete()
		{
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				BankFeeJournal.AH_RX_NKTransactionCurrency = "USD";
				BankFeeJournal.AH_ExchangeRate = 0.5M;
				BankFeeJournal.AH_OSExTaxAmount = 45M;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("Journal should not be deleted", !BankFeeJournal.IsDeleted);
				AssertEquals(45M, ((IMatching)BankFeeJournal).OSPartialPaymentAmount);
			}
		}

		public void TestClosingAmountZeroDeletesBizO()
		{
			using (BankFeeJournalForm testForm = (BankFeeJournalForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				BankFeeJournal.AH_LocalExTaxAmount = 50M;
				BankFeeJournal.AH_LocalExTaxAmount = 0M;
				testForm.OnClosing_ForTestOnly(new CancelEventArgs());
				Assert("Journal should be deleted", BankFeeJournal.IsDeleted);
			}
		}

		public void TestIsNewUnsavedObject()
		{
			APMatchingBase testAPMatching = new APMatchingBase(Factory);

			APJournal testJournal = Factory.NewWithValidTestData<APJournal>();

			((IMiscellaneousTransaction)testJournal).MatchingBizO = null;
			using (BankFeeJournalForm testForm = new BankFeeJournalForm(testJournal))
			{
				Assert("Matching BizO is null so IsNewUnsavedObject_ForTestOnly should be false", !testForm.IsNewUnsavedObject_ForTestOnly);

				((IMiscellaneousTransaction)testJournal).MatchingBizO = testAPMatching;

				Assert("IsNewUnsavedObject_ForTestOnly should be true since MatchingBase doesnot refer to testJournal", testForm.IsNewUnsavedObject_ForTestOnly);

				testAPMatching.AddMiscellaneousTransaction(testJournal);
				Assert("IsNewUnsavedObject_ForTestOnly should be false since MatchingBase refers to testJournal", !testForm.IsNewUnsavedObject_ForTestOnly);
			}
		}

		public void TestIsCurrentContextMatching()
		{
			APJournal testJournal = Factory.NewWithValidTestData<APJournal>();
			((IMiscellaneousTransaction)testJournal).MatchingBizO = null;
			using (BankFeeJournalForm testForm = new BankFeeJournalForm(testJournal))
			{
				Assert("Current context is not matching since MatchingBizO is null", !testForm.IsCurrentContextMatching_ForTestOnly);
				testForm.Close();
			}

			((IMiscellaneousTransaction)testJournal).MatchingBizO = new ARMatchingBase(Factory);
			using (BankFeeJournalForm testForm = new BankFeeJournalForm(testJournal))
			{
				Assert("Current context is matching since MatchingBizO is not null", testForm.IsCurrentContextMatching_ForTestOnly);
				testForm.Close();
			}
		}

		class ARJournalForTest : ARJournal
		{
			public ARJournalForTest(BusinessObjectFactory factory, System.Data.DataRow row) : base(factory, row)
			{
			}

			public bool HasDeleteFromDB => DeleteProcessCount > 0;

			public int DeleteProcessCount { get; private set; }

			public override void DeleteFromDB()
			{
				base.DeleteFromDB();
				DeleteProcessCount++;
			}
		}
	}
}
