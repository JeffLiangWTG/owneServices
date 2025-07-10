using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(FCBAdjustmentJournalLine))]
	public class FCBAdjustmentJournalLineTest : GLJournalLineTest
	{
		public override void TestAL_LocalTaxAmountCalculatedUsingLineTaxRatesWhenExchangeRateSet()
		{
			Assert("Not applicable as recalculation is suspended here.", true);
		}

		public override void TestLocalExtraAmountIsUpdatedOnInvalidTaxID()
		{
			Assert("Not applicable as recalculation is suspended here.", true);
		}

		public override void TestOSAndLocalAmountsFromOSExTaxAmountForMexico_TestingCases()
		{
			Assert("Not applicable as recalculation is suspended here.", true);
		}

		public override void TestAL_OSExtraTaxAmount_CalculateAL_LocalExtraTaxAmountAfterTaxIDUpdate()
		{
			Assert("Not applicable as recalculation is suspended here.", true);
		}

		public override void TestGetNewValidationCore()
		{
			AssertEquals(typeof(FCBAdjustmentJournalLineValidation), JournalLine.Validation.GetType());
		}

		public override void TestAL_AG()
		{
			base.TestAL_AG();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			JournalLine.AL_AG = TestObjectCreator.GLHeader1.PK;

			Assert(JournalLine.IsUnrealizedLocalAmountLine);

			AssertEquals("Currency is set to local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, JournalLine.AL_RX_NKTransactionCurrency);

			JournalLine.Validation.ValidateAll();
			AssertHasError(JournalLine.UnsignedLocalLineAmountInfo, "Please enter a Local Amount.");
			AssertHasError(JournalLine.UnsignedOSLineAmountInfo, "Please enter a value.");

			JournalLine.UnsignedOSLineAmount = 100M;
			JournalLine.Validation.ValidateAll();
			AssertNoErrors(JournalLine.UnsignedOSLineAmountInfo);
			AssertNoErrors(JournalLine.UnsignedLocalLineAmountInfo);

			JournalLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			Assert("Precondition:", JournalLine.IsForeignCurrencyBalanceAdjustmentLine);

			AssertEquals("UnsignedOSLineAmount is set to zero", 0M, JournalLine.UnsignedOSLineAmount);
			AssertEquals("UnsignedLocalLineAmount remains at 100M", 100M, JournalLine.UnsignedLocalLineAmount);

			AssertHasError(JournalLine.AL_RX_NKTransactionCurrencyInfo, "Please select a Foreign currency.");
			AssertNoErrors(JournalLine.UnsignedOSLineAmountInfo);
			AssertNoErrors(JournalLine.UnsignedLocalLineAmountInfo);
		}

		public void TestAL_RX_NKTransactionCurrency()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.PeriodEndRate, 0.76m);

			JournalLine.AL_AG = AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.Value;
			JournalLine.UnsignedOSLineAmount = 100M;
			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;

			AssertEquals(100M, JournalLine.UnsignedLocalLineAmount);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals(100M, JournalLine.UnsignedLocalLineAmount);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;

			AssertEquals(100M, JournalLine.UnsignedLocalLineAmount);
		}

		public override void TestAL_ExchangeRate()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.EUR.RX_Code;
			JournalLine.AL_ExchangeRate = 0.50m;
			JournalLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			JournalLine.AL_OSExTaxAmount = 0m;
			JournalLine.AL_LocalExTaxAmount = 20m;

			JournalLine.AL_ExchangeRate = 0.75m;
			AssertEquals("Local ex tax amount is the same, updating exchange rate did not update local ex tax amount", 20m, JournalLine.AL_LocalExTaxAmount);
			AssertEquals("OS ex tax amount is same too", 0m, JournalLine.AL_OSExTaxAmount);
		}

		public override void TestSettingAL_RXValidatesAL_AG()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			var profitAndLossAccount = Factory.NewWithValidTestData<AccGLHeader>();
			profitAndLossAccount.AG_AccountNum = "9999.99.99";
			profitAndLossAccount.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;

			JournalLine.AL_AG = profitAndLossAccount.PK;

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertHasErrors(JournalLine.AL_AGInfo);

			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;

			AssertNoErrors(JournalLine.AL_AGInfo);
		}

		public void TestUpdaingLocalExTaxAmountDoesNotUpdateOSExTaxAmount_ForForeignCurrencyAmount()
		{
			//When we update local amount for foreign currency lines we do not want to update OSExTaxAmount as OSExTaxAmount should be 0 for foreign currency lines
			JournalLine.AL_AG = TestObjectCreator.GLHeader1.PK;
			Assert("Precondition:", JournalLine.IsUnrealizedLocalAmountLine);

			AssertEquals("Precondition:", 0m, JournalLine.AL_OSExTaxAmount);
			JournalLine.UnsignedLocalLineAmount = 200m;
			AssertEquals(200m, JournalLine.AL_OSExTaxAmount);

			JournalLine.AL_AG = TestObjectCreator.GLHeader2.PK;
			Assert("Precondition:", JournalLine.IsForeignCurrencyBalanceAdjustmentLine);
			JournalLine.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertEquals("Precondition:", 0m, JournalLine.AL_OSExTaxAmount);
			JournalLine.UnsignedLocalLineAmount = 200m;
			AssertEquals(0m, JournalLine.AL_OSExTaxAmount);
		}

		public void TestIfCorrectValidationClassIsInstantiated()
		{
			AssertEquals(typeof(FCBAdjustmentJournalLineValidation), JournalLine.Validation.GetType());
		}

		public void TestUnrealizedLocalAmountLine_And_ForeignCurrencyBalanceAdjustmentLine()
		{
			JournalLine.AL_AG = Guid.Empty;

			Assert(!JournalLine.IsUnrealizedLocalAmountLine);
			Assert(JournalLine.IsForeignCurrencyBalanceAdjustmentLine);

			var unrealizedControlAccountGuid = Guid.NewGuid();
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unrealizedControlAccountGuid);

			JournalLine.AL_AG = unrealizedControlAccountGuid;

			Assert(JournalLine.IsUnrealizedLocalAmountLine);
			Assert(!JournalLine.IsForeignCurrencyBalanceAdjustmentLine);

			JournalLine.AL_AG = Guid.NewGuid();

			Assert(!JournalLine.IsUnrealizedLocalAmountLine);
			Assert(JournalLine.IsForeignCurrencyBalanceAdjustmentLine);
		}

		public void TestUnsignedOSLineAmount_ReadOnly()
		{
			var unrealizedControlAccountGuid = Guid.NewGuid();
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unrealizedControlAccountGuid);

			JournalLine.AL_AG = unrealizedControlAccountGuid;
			Assert(!JournalLine.UnsignedOSLineAmountInfo.ReadOnly);

			JournalLine.AL_AG = Guid.NewGuid();
			Assert(JournalLine.UnsignedOSLineAmountInfo.ReadOnly);
		}

		public override void TestAL_ExchangeRateRecalculatesLocalAmounts()
		{
			Assert("Not applicable", true);
		}

		public override void TestUnsignedOSLineAmountValidation()
		{
			Assert("UnsignedOSLineAmount is tested in the validation class", true);
		}

		public override void TestGetNewValidation()
		{
			Assert("Validation should be FCBAdjustmentJournalLineValidation", typeof(FCBAdjustmentJournalLineValidation).IsAssignableFrom(Line.Validation.GetType()));
			Line.AL_ReverseDate = ZDateTime.Now;

			Factory.Save();
			Assert("Validation should still be FCBAdjustmentJournalLineValidation", typeof(FCBAdjustmentJournalLineValidation).IsAssignableFrom(Line.Validation.GetType()));
		}

		protected new FCBAdjustmentJournalLine JournalLine
		{
			get { return (FCBAdjustmentJournalLine)Line; }
		}

		protected override Type MasterHeaderType
		{
			get { return typeof(FCBAdjustmentJournal); }
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
		}
	}
}
