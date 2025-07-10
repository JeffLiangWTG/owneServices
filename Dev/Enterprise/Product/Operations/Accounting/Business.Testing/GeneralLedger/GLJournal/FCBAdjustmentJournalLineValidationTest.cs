using System;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(FCBAdjustmentJournalLine))]
	public class FCBAdjustmentJournalLineValidationTest : GLJournalLineValidationTest
	{
		public override void TestCheckAL_RX_NKTransactionCurrency()
		{
			base.TestCheckAL_RX_NKTransactionCurrency();

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var fcbJournal = Factory.NewWithValidTestData<FCBAdjustmentJournal>();
			FCBAdjustmentJournalLine line1 = (FCBAdjustmentJournalLine)fcbJournal.GLJournalLines.AddNew();

			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, line1.AL_RX_NKTransactionCurrency);
			AssertNoErrors(line1.AL_RX_NKTransactionCurrencyInfo);

			Assert("Precondition:", line1.IsUnrealizedLocalAmountLine);

			line1.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;

			AssertHasError(line1.AL_RX_NKTransactionCurrencyInfo, "Please select 'AUD' as currency.");

			FCBAdjustmentJournalLine line2 = (FCBAdjustmentJournalLine)fcbJournal.GLJournalLines.AddNew();
			line2.AL_AG = TestObjectCreator.GLHeader2.PK;

			Assert("Precondition:", line2.IsForeignCurrencyBalanceAdjustmentLine);

			AssertEquals("The new line is defaulted to have local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, line2.AL_RX_NKTransactionCurrency);
			AssertHasError("This default value will display validation error", line2.AL_RX_NKTransactionCurrencyInfo, "Please select a Foreign currency.");

			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			AssertNoErrors(line2.AL_RX_NKTransactionCurrencyInfo);
		}

		public override void TestCheckAL_AG()
		{
			base.TestCheckAL_AG();

			var fcbJournal = Factory.NewWithValidTestData<FCBAdjustmentJournal>();
			var line = fcbJournal.GLJournalLines.AddNew();

			AssertNoErrors(line.AL_AGInfo);

			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			fcbJournal.GLJournalLines.RemoveAndDeleteAll();

			line = fcbJournal.GLJournalLines.AddNew();

			line.Validation.ValidateAL_AG();

			AssertNoErrors(line.AL_AGInfo);
		}

		public void TestValidateUnsignedOSLineAmount()
		{
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			FCBAdjustmentJournal fcbJournal = Factory.NewWithValidTestData<FCBAdjustmentJournal>();
			FCBAdjustmentJournalLine line = (FCBAdjustmentJournalLine)fcbJournal.GLJournalLines.AddNew();

			AssertEquals("Precondition:", TestObjectCreator.GLHeader1.PK, line.AL_AG);
			Assert("Precondition:", line.IsUnrealizedLocalAmountLine);

			line.UnsignedOSLineAmount = 0m;

			var validation = line.Validation as FCBAdjustmentJournalLineValidation;
			validation.ValidateUnsignedOSLineAmount();
			validation.ValidateAL_OSExTaxAmount();

			AssertHasError(line.UnsignedOSLineAmountInfo, "Please enter a value.");
			AssertHasError(line.AL_OSExTaxAmountInfo, "Please enter an Amount.");

			line.AL_AG = TestObjectCreator.GLHeader2.PK;

			validation.ValidateUnsignedOSLineAmount();
			validation.ValidateAL_OSExTaxAmount();

			Assert("Precondition:", line.IsForeignCurrencyBalanceAdjustmentLine);

			AssertNoErrors(line.UnsignedOSLineAmountInfo);
			AssertNoErrors(line.AL_OSExTaxAmountInfo);
		}

		public override void TestCheckUnsignedLocalLineAmount()
		{
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());

			var journal = TestObjectCreator.CreateGLJournal<FCBAdjustmentJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			var line = (FCBAdjustmentJournalLine)TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.GLHeader2.PK);

			line.AL_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			Assert(line.IsForeignCurrencyBalanceAdjustmentLine);
			Assert(!line.IsUnrealizedLocalAmountLine);

			line.AL_ExchangeRate = 1m;
			line.UnsignedLocalLineAmount = 123m;
			Assert(line.UnsignedOSLineAmountInfo.ReadOnly);
			AssertEquals(0m, line.UnsignedOSLineAmount); //OS amount not updated for foreign currency line
			Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
			Assert("Local Amount and OS Amount is different", line.UnsignedOSLineAmount != line.UnsignedLocalLineAmount);
			((FCBAdjustmentJournalLineValidation)line.Validation).ValidateUnsignedLocalLineAmount();
			AssertNoError(line.UnsignedLocalLineAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

			//Changing current GL account to Unrealized Exchange Differences Account will update transaction currency, 
			//but it will not update the UnsignedOSLineAmount
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			AssertEquals(Core.Constants.CurrencyCodes.Australia, line.AL_RX_NKTransactionCurrency);
			Assert(!line.IsForeignCurrencyBalanceAdjustmentLine);
			Assert(line.IsUnrealizedLocalAmountLine);
			AssertEquals(0m, line.UnsignedOSLineAmount);

			Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
			Assert("Local Amount and OS Amount is different", line.UnsignedOSLineAmount != line.UnsignedLocalLineAmount);
			((FCBAdjustmentJournalLineValidation)line.Validation).ValidateUnsignedLocalLineAmount();
			AssertHasError(line.UnsignedLocalLineAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");

			line.AL_ExchangeRate = 1m;
			line.UnsignedLocalLineAmount = 123m;
			AssertEquals(123m, line.UnsignedOSLineAmount);
			Assert("Exchange Rate is 1", line.AL_ExchangeRate == 1);
			Assert("Local Amount and OS Amount is equal", line.UnsignedOSLineAmount == line.UnsignedLocalLineAmount);
			((FCBAdjustmentJournalLineValidation)line.Validation).ValidateUnsignedLocalLineAmount();
			AssertNoError(line.UnsignedLocalLineAmountInfo, "The Local Amount should be equal to OS Amount when Local Currency is used");
		}

		protected override Type GetExpectedParentBusinessObjectType()
		{
			return typeof(FCBAdjustmentJournal);
		}
	}
}
