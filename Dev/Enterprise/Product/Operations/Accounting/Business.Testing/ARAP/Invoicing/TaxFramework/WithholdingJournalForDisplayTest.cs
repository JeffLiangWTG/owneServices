using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework.Testing
{
	[TestedType(typeof(WithholdingJournalForDisplay))]
	class WithholdingJournalForDisplayTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor_ThrowsArgumentNullException_WhenParentNull()
		{
			AssertExceptionThrown<ArgumentNullException>("", "Value cannot be null.\r\nParameter name: parent", () => new WithholdingJournalForDisplay(null, ZDate.Empty));
		}

		[TestDate(2020, 06, 15)]
		public void TestWithholdingJournalForDisplay()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var apJournal = Factory.New<APJournal>();

			var journalForDisplay = new WithholdingJournalForDisplay(apJournal, ZDate.Empty);
			AssertEquals("JournalPK", apJournal.PK, journalForDisplay.JournalPK);
			var category = "XXX";
			apJournal.AH_TransactionCategory = category;
			AssertEquals("Transaction Category", category, journalForDisplay.TransactionCategory);

			var postDate = ZDate.Today;
			apJournal.AH_PostDate = postDate;
			AssertEquals("Post Date", postDate, journalForDisplay.PostDate);

			var invoiceDate = ZDate.Today.AddDays(-1);
			apJournal.AH_InvoiceDate = invoiceDate;
			AssertEquals("Invoice Date", invoiceDate, journalForDisplay.InvoiceDate);

			apJournal.AH_OH = ZGuid.Empty;
			AssertEquals("Organisation", ZString.Empty, journalForDisplay.Organisation);

			apJournal.AH_OH = testObjectCreator.ABIGAS.PK;
			AssertEquals("Organisation", testObjectCreator.ABIGAS.OH_Code, journalForDisplay.Organisation);

			var desc = "some desc";
			apJournal.AH_Desc = desc;
			AssertEquals("Description", desc, journalForDisplay.Description);

			var currency = "ABC";
			apJournal.AH_RX_NKTransactionCurrency = currency;
			AssertEquals("Currency", currency, journalForDisplay.Currency);

			var drcrsign = "CR";
			apJournal.DebitCreditSign = drcrsign;
			AssertEquals("Debit Credit Sign", drcrsign, journalForDisplay.DebitCreditSign);

			var osAmount = 100M;
			apJournal.AH_OSExTaxAmount = osAmount;
			AssertEquals("Amount", osAmount, journalForDisplay.Amount);

			var localAmount = 200M;
			apJournal.AH_LocalExTaxAmount = localAmount;
			AssertEquals("Local Amount", localAmount, journalForDisplay.LocalAmount);

			var exRate = 5M;
			apJournal.AH_ExchangeRate = exRate;
			AssertEquals("Exchange Rate", exRate, journalForDisplay.ExchangeRate);

			apJournal.AH_AG = testObjectCreator.GLHeader1.PK;
			AssertEquals("GL Account", testObjectCreator.GLHeader1.AccountNum, journalForDisplay.GLAccount);

			apJournal.AH_GB = testObjectCreator.NonCurrentBranch.PK;
			AssertEquals("Branch", testObjectCreator.NonCurrentBranch.GB_Code, journalForDisplay.Branch);

			apJournal.AH_GE = testObjectCreator.NonCurrentDepartment.PK;
			AssertEquals("Department", testObjectCreator.NonCurrentDepartment.GE_Code, journalForDisplay.Department);
		}

		[TestDate(2020, 05, 15)]
		public void TestWithholdingJournalForDisplay_Validation()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var invoicePostDate = ZDate.Today.AddDays(-10);
			var journal = Factory.NewWithValidTestData<APJournal>();
			var journalForDisplay = new WithholdingJournalForDisplay(journal, invoicePostDate);
			journalForDisplay.PostDate = ZDate.Empty;
			var expectedError = "Please enter an Invoice Post Date.";
			AssertHasError(journalForDisplay.PostDateInfo, expectedError);

			journalForDisplay.PostDate = ZDate.Today;
			AssertNoErrors(journalForDisplay.PostDateInfo);

			expectedError = string.Format("Journal Post Date cannot be earlier than associated Invoice Post Date. Invoice Post Date: {0}", invoicePostDate.ToShortDateString());
			journalForDisplay.PostDate = invoicePostDate.AddDays(-1);
			AssertHasError(journalForDisplay.PostDateInfo, expectedError);

			journalForDisplay.PostDate = invoicePostDate;
			AssertNoErrors(journalForDisplay.PostDateInfo);

			expectedError = "Please enter a Description.";
			journalForDisplay.Description = "";
			AssertHasError(journalForDisplay.DescriptionInfo, expectedError);

			journalForDisplay.Description = "something is better than nothing";
			AssertNoErrors(journalForDisplay.DescriptionInfo);
		}

		public void TestRefreshBindingOfParentIsWrappedProperly()
		{
			var parent = Factory.NewWithValidTestData<APJournal>();
			var journalForDisplay = new WithholdingJournalForDisplay(parent, ZDate.Empty);
			var isListChangedFired = false;
			((IBindingList)journalForDisplay).ListChanged += (x, y) => isListChangedFired = true;

			parent.RefreshBinding();
			Assert(isListChangedFired);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WithholdingJournalForDisplay(Factory.NewWithValidTestData<APJournal>(), ZDate.Empty);
		}
	}
}
