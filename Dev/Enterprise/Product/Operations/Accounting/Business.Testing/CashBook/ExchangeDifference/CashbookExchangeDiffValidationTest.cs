using System;
using CargoWise.Common;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	public class CashbookExchangeDiffValidationTest : TransactionHeaderValidationTest
	{
		protected override Type HeaderType => typeof(CashbookExchangeDiff);

		public void TestCheckAH_AGWithEmptyCurrencyAdjustmentExchangeGainAccount()
		{
			TestCheckAH_AGWithEmptyCurrencyAdjustmentExchangeGainAccount_CheckCondition();

			TestCheckAH_AGWithEmptyCurrencyAdjustmentExchangeGainAccount_SkipCondition();
		}

		void TestCheckAH_AGWithEmptyCurrencyAdjustmentExchangeGainAccount_CheckCondition()
		{
			AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeGainAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.DefaultValue
					);
			AccountingConfigurationRegistry.Instance
				.CurrencyAdjustmentExchangeLossAccount.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.DefaultValue
				);

			var cashBookEXX = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			AssertEquals(true, cashBookEXX.ShouldSetAH_AG);
			AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value);
			AssertNotEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value);
			new CashbookExchangeDiffValidation(cashBookEXX).ValidateAH_AG();
			AssertNoError(cashBookEXX.AH_AGInfo, potentialEmptyAH_AGDueToRegistriesErrorMessage);

			var registryOptionGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Options;
			using (new DisposableAction(
				() =>
				{
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Options = Enterprise.Integration.RegistryOptions.Default;
				},
				() =>
				{
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Options = registryOptionGain;
				}))
			{
				AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeGainAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty
					);
			}
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Value);
			new CashbookExchangeDiffValidation(cashBookEXX).ValidateAH_AG();
			AssertHasError(cashBookEXX.AH_AGInfo, potentialEmptyAH_AGDueToRegistriesErrorMessage);

			var registryOptionLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Options;
			using (new DisposableAction(
				() =>
				{
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Options = Enterprise.Integration.RegistryOptions.Default;
				},
				() =>
				{
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Options = registryOptionLoss;
				}))
			{
				AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeLossAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty
					);
			}
			AssertEquals(Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Value);
			new CashbookExchangeDiffValidation(cashBookEXX).ValidateAH_AG();
			AssertHasError(cashBookEXX.AH_AGInfo, potentialEmptyAH_AGDueToRegistriesErrorMessage);
		}

		void TestCheckAH_AGWithEmptyCurrencyAdjustmentExchangeGainAccount_SkipCondition()
		{
			AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeGainAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.DefaultValue
					);
			AccountingConfigurationRegistry.Instance
				.CurrencyAdjustmentExchangeLossAccount.SetValue(
					Guid.Empty, Guid.Empty, Guid.Empty, AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.DefaultValue
				);

			var cashbookInDb = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			Factory.Save();
			AssertEquals(false, cashbookInDb.ShouldSetAH_AG);

			var cashBookCancelled = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashBookCancelled.AH_IsCancelled = true;
			AssertEquals(false, cashBookCancelled.ShouldSetAH_AG);

			var cashBookREA = Factory.NewWithValidTestData<CashbookExchangeDiff>();
			cashBookREA.AH_TransactionCategory = TransactionCategory.Codes.RealizedExchangeGainLoss;
			AssertEquals(false, cashBookREA.ShouldSetAH_AG);

			var registryOptionLoss = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Options;
			var registryOptionGain = AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Options;
			using (new DisposableAction(
				() =>
				{
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Options = Enterprise.Integration.RegistryOptions.Default;
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Options = Enterprise.Integration.RegistryOptions.Default;
				},
				() =>
				{
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount.Options = registryOptionLoss;
					AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount.Options = registryOptionGain;
				}))
			{
				AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeGainAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty
					);
				AccountingConfigurationRegistry.Instance
					.CurrencyAdjustmentExchangeLossAccount.SetValue(
						Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty
					);
			}

			new CashbookExchangeDiffValidation(cashbookInDb).ValidateAH_AG();
			AssertNoError(cashbookInDb.AH_AGInfo, potentialEmptyAH_AGDueToRegistriesErrorMessage);

			new CashbookExchangeDiffValidation(cashBookCancelled).ValidateAH_AG();
			AssertNoError(cashBookCancelled.AH_AGInfo, potentialEmptyAH_AGDueToRegistriesErrorMessage);

			new CashbookExchangeDiffValidation(cashBookREA).ValidateAH_AG();
			AssertNoError(cashBookREA.AH_AGInfo, potentialEmptyAH_AGDueToRegistriesErrorMessage);
		}

		readonly string potentialEmptyAH_AGDueToRegistriesErrorMessage = @"The transaction does not have a GL account specified.

Please check and fill below registry settings:
1.Accounting -> General Ledger Defaults -> Link Account -> Currency Adjustment Exchange Gain Account
2.Accounting -> General Ledger Defaults -> Link Account -> Currency Adjustment Exchange Loss Account";
	}
}
