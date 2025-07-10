using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(FCBAdjustmentJournal))]
	class ForeignCurBalanceAdjstTest : GLJournal_InnerTest
	{
		public void TestReceiptType()
		{
			AssertEquals(ReceiptTypes.ForeignCurrencyBalance, Header.AH_ReceiptType);
		}

		public void TestDependentTransactionLineType()
		{
			AssertEquals(typeof(FCBAdjustmentJournalLine), ((FCBAdjustmentJournal)Header).DependentTransactionLineType);
		}

		public void TestBalancingSetsDefaultForeignCurrencyAndExchangeRate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			Journal.GLJournalLines.RemoveAndDeleteAll();

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			FCBAdjustmentJournalLine line1 = (FCBAdjustmentJournalLine)Journal.GLJournalLines.AddNew();
			line1.DebitCreditSign = "DR";
			line1.UnsignedLocalLineAmount = 200.00m;
			Assert("Precondition:", line1.IsUnrealizedLocalAmountLine);

			FCBAdjustmentJournalLine line2 = (FCBAdjustmentJournalLine)Journal.GLJournalLines.AddNew();
			line2.AL_AG = TestObjectCreator.GLHeader2.PK;
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			line2.AL_ExchangeRate = 0.70m;
			line2.UnsignedLocalLineAmount = 100.00m;
			Assert("Precondition:", line2.IsForeignCurrencyBalanceAdjustmentLine);

			Assert("Journal is not balanced.", !Journal.IsBalanced);

			var balancingJournal = Journal.Balance();
			Assert("Journal is balanced.", Journal.IsBalanced);
			AssertEquals("Balancing line should be a credit line", "CR", balancingJournal.DebitCreditSign);
			AssertEquals("unsigned local line amount should be 100", 100m, balancingJournal.UnsignedLocalLineAmount);
			AssertEquals("Balancing line should default to local currency", TestObjectCreator.AUD.RX_Code, balancingJournal.AL_RX_NKTransactionCurrency);
			AssertEquals("Balancing line exchange rate should default to 1", 1m, balancingJournal.AL_ExchangeRate);
		}

		protected override void ToggleSetCanUserPostToPreviousPeriods(bool flag)
		{
			Env.Security.GeneralLedgerPostToPreviousOpenPeriod.IsAllowed = flag;
		}

		protected override void AssertDescription()
		{
			AssertEquals("FOREIGN CURRENCY BALANCE ADJUSTMENT", Header.AH_Desc);
		}

		protected override void AssertTransactionTypeCategoryReadOnlyness()
		{
			Assert(Journal.AH_TransactionTypeInfo.ReadOnly);
			Assert(Journal.AH_TransactionCategoryInfo.ReadOnly);
		}

		public override void TestDefaultPostDateReadOnly()
		{
			Assert("AH_PostDate should be readonly", Header.AH_PostDateInfo.ReadOnly);
		}

		public override void TestBalancingDebitBalanceWithForeignCurrency()
		{
			Journal.GLJournalLines.RemoveAndDeleteAll();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			FCBAdjustmentJournalLine newLine = (FCBAdjustmentJournalLine)Journal.GLJournalLines.AddNew();
			newLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			newLine.UnsignedOSLineAmount = 180M;
			newLine.UnsignedLocalLineAmount = 180M;
			newLine.DebitCreditSign = nameof(DebitCredit.DR);

			newLine = (FCBAdjustmentJournalLine)Journal.GLJournalLines.AddNew();
			newLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			newLine.UnsignedLocalLineAmount = 200M;
			newLine.AL_ExchangeRate = 0.90M;
			newLine.DebitCreditSign = nameof(DebitCredit.CR);

			Assert("Should not balance", !Journal.IsBalanced);
			Journal.Balance();
			Assert("Should now balance", Journal.IsBalanced);
			AssertEquals("Should be balanced to balancing account", Journal.BalancingAccount.Value, Journal.GLJournalLines[2].AL_AG);
			AssertEquals("This would be a local currency line", TestObjectCreator.AUD.RX_Code, Journal.GLJournalLines[2].AL_RX_NKTransactionCurrency);
			AssertEquals("Unsigned OS amount should be (200 - 180) = 20", 20M, Journal.GLJournalLines[2].UnsignedOSLineAmount);
			AssertEquals("Unsigned Local amount should be (200 - 180) = 20", 20M, Journal.GLJournalLines[2].UnsignedLocalLineAmount);
			AssertEquals("Sign should now be DR", DebitCreditDataEntry.DR, Journal.GLJournalLines[2].DebitCreditSign.ToString());
		}

		public override void TestBalancingCreditBalanceForeignCurrency()
		{
			Journal.GLJournalLines.RemoveAndDeleteAll();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			FCBAdjustmentJournalLine newLine = (FCBAdjustmentJournalLine)Journal.GLJournalLines.AddNew();
			newLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			newLine.UnsignedOSLineAmount = 180M;
			newLine.UnsignedLocalLineAmount = 180M;
			newLine.DebitCreditSign = nameof(DebitCredit.CR);

			newLine = (FCBAdjustmentJournalLine)Journal.GLJournalLines.AddNew();
			newLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			newLine.UnsignedLocalLineAmount = 200M;
			newLine.AL_ExchangeRate = 0.90M;
			newLine.DebitCreditSign = nameof(DebitCredit.DR);

			Assert("Should not balance", !Journal.IsBalanced);
			Journal.Balance();
			Assert("Should now balance", Journal.IsBalanced);
			AssertEquals("Should be balanced to balancing account", Journal.BalancingAccount.Value, Journal.GLJournalLines[2].AL_AG);
			AssertEquals("This would be a local currency line", TestObjectCreator.AUD.RX_Code, Journal.GLJournalLines[2].AL_RX_NKTransactionCurrency);
			AssertEquals("Unsigned OS amount should be (200 - 180) = 20", 20M, Journal.GLJournalLines[2].UnsignedOSLineAmount);
			AssertEquals("Unsigned Local amount should be (200 - 180) = 20", 20M, Journal.GLJournalLines[2].UnsignedLocalLineAmount);
			AssertEquals("Sign should now be CR", DebitCreditDataEntry.CR, Journal.GLJournalLines[2].DebitCreditSign.ToString());
		}

		public override void TestBalancingRowUsesClearingAccount()
		{
			Journal.GLJournalLines.RemoveAndDeleteAll();

			GLJournalLine newLine = Journal.GLJournalLines.AddNew();
			newLine.UnsignedOSLineAmount = 200.00m;
			newLine.DebitCreditSign = nameof(DebitCredit.CR);

			Journal.Balance();
			AssertEquals("Balancing row should have GL Journal Clearing Account", TestObjectCreator.GLHeader1.PK, Journal.GLJournalLines[1].AL_AG);
		}

		public new void TestAgePeriodValidation()
		{
			Assert("Not applicable", true);
		}

		public new void TestAutoJournalPeriodsReadOnlyWhenItLoaded()
		{
			Assert("Not applicable", true);
		}

		public new void TestAutoPresentationJournalPeriodsReadOnlyWhenItLoaded()
		{
			Assert("Not applicable", true);
		}

		public new void TestDefaultDescriptionOnChangeOfJournalType()
		{
			Assert("Not applicable", true);
		}

		public new void TestReversingPresentationJournalPeriodsReadOnlyWhenItLoaded()
		{
			Assert("Not applicable", true);
		}

		public new void TestReversingJournalPeriodsReadOnlyWhenItLoaded()
		{
			Assert("Not applicable", true);
		}

		public new void TestObsoleteJournalCategoriesAreAddedToLookup()
		{
			Assert("Not applicable", true);
		}

		public new void TestChangeOfJournalType()
		{
			Assert("Not applicable", true);
		}

		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(FCBAdjustmentJournalLine);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(GetExpectedBusinessObjectType());
		}

		protected override GLJournal Journal
		{
			get { return (FCBAdjustmentJournal)Header; }
		}

		protected override void ReLoadHeaderForTest()
		{
			Header = (new BusinessObjectFactory()).Load<FCBAdjustmentJournal>(Journal.PK);
		}

		protected override void SetupHeaderForReversing(ZDecimal aH_OSExTaxAmount, ZDecimal aH_OSTaxAmount)
		{
			base.SetupHeaderForReversing(aH_OSExTaxAmount, aH_OSTaxAmount);
			Header.AH_ReceiptType = ReceiptTypes.ForeignCurrencyBalance;
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
		}
	}
}
