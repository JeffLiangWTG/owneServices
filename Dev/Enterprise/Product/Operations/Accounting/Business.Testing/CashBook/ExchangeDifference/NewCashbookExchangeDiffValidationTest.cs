using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference.Testing
{
	public class NewCashbookExchangeDiffValidationTest : CashbookExchangeDiffValidationTest
	{
		[TestDate(2020, 2, 6)]
		public void TestCheckAH_ExchangeRate()
		{
			var exchangeRates = Factory.Load<RefExchangeRate>(new ZQuery());
			exchangeRates.ForEach((x) => { x.Delete(); });
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var bank = TestObjectCreator.InsertBankAccount(Core.Constants.CurrencyCodes.Australia);
				var newExchangeRate = 4.6m;
				TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, "BUY", newExchangeRate, new ZDateTime(2020, 2, 1), new ZDateTime(2020, 2, 29));
				var header = Factory.New<NewCashbookExchangeDiffHeader>();
				header.AH_PostDate = ZDateTime.Now;

				var modifyNewExchangeRateSecurity = Environment.Env.Security.ModifyCashBookBankCurrencyAdjustmentNewExchangeRate;
				var newCashbookExchangeDiff = header.NewCashbookExchangeDiffCollection.AddNew();
				var securityRightsToModifyTheNewExchangeRateMessage = $"You have not been granted security rights to {modifyNewExchangeRateSecurity.DisplayTextPathToSecurityRight.ToString()}. Please set this to {newExchangeRate.ToString($"F{newCashbookExchangeDiff.ExchangeRateDecimalPlaces}")}.";
				newCashbookExchangeDiff.AH_AB = bank.PK;

				modifyNewExchangeRateSecurity.IsAllowed = false;
				newCashbookExchangeDiff.AH_ExchangeRate = 4;
				AssertHasError("You have not been granted the security right and new exchange rate is not equals to default rate.", newCashbookExchangeDiff.AH_ExchangeRateInfo, securityRightsToModifyTheNewExchangeRateMessage);

				newCashbookExchangeDiff.AH_ExchangeRate = newExchangeRate;
				AssertNoError("You have not been granted the security right and new exchange rate is not equals to default rate.", newCashbookExchangeDiff.AH_ExchangeRateInfo, securityRightsToModifyTheNewExchangeRateMessage);

				modifyNewExchangeRateSecurity.IsAllowed = true;
				newCashbookExchangeDiff.AH_ExchangeRate = 4;
				AssertNoError("You have been granted the security right and new exchange rate is not equals to default rate.", newCashbookExchangeDiff.AH_ExchangeRateInfo, securityRightsToModifyTheNewExchangeRateMessage);

				newCashbookExchangeDiff.AH_ExchangeRate = newExchangeRate;
				AssertNoError("You have been granted the security right and new exchange rate is equals to default rate.", newCashbookExchangeDiff.AH_ExchangeRateInfo, securityRightsToModifyTheNewExchangeRateMessage);

				var newCashbookExchangeDiff2 = header.NewCashbookExchangeDiffCollection.AddNew();
				newCashbookExchangeDiff2.AH_AB = bank.PK;
				using (AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.ExchangeRateTypes.Code.PeriodEndRate))
				{
					var expectedPERAndBUYRateNotFoundMessage = "Period End Rate and BUY exchange rate not found with reference to post date.";
					header.AH_PostDate = ZDateTime.Today.AddMonths(1);
					AssertHasError("Period End Rate and BUY exchange rate not found with reference to post date", newCashbookExchangeDiff2.AH_ExchangeRateInfo, expectedPERAndBUYRateNotFoundMessage);

					header.AH_PostDate = ZDateTime.Now;
					AssertNoError("Found Period End Rate and BUY exchange rate with reference to post date", newCashbookExchangeDiff2.AH_ExchangeRateInfo, expectedPERAndBUYRateNotFoundMessage);
					AssertEquals("Period End Rate and BUY exchange rate", newExchangeRate, newCashbookExchangeDiff2.AH_ExchangeRate);
				}

				var exchangeRateTypeCodeList = new ExchangeRateTypeListProvider().CodeDescriptionPairList.GetAllCodes();
				for (int i = 0; i < exchangeRateTypeCodeList.Length; i++)
				{
					var rateType = exchangeRateTypeCodeList[i];
					var rate = newExchangeRate + 0.1m * i;
					using (AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, rateType))
					{
						if (rateType == Constants.ExchangeRateTypes.Code.PeriodEndRate)
						{
							continue;
						}

						TestObjectCreator.CreateExchangeRate(TestObjectCreator.AUD, rateType, rate, new ZDateTime(2020, 4, 1), new ZDateTime(2020, 4, 30));
						var expectedNotPERRateNotFoundMessage = $"{rateType} exchange rate not found with reference to post date.";

						header.AH_PostDate = ZDateTime.Today.AddMonths(1);
						AssertHasError($"{rateType} exchange rate not found with reference to post date", newCashbookExchangeDiff2.AH_ExchangeRateInfo, expectedNotPERRateNotFoundMessage);

						header.AH_PostDate = ZDateTime.Today.AddMonths(2);
						AssertNoError($"Found {rateType} exchange rate with reference to post date.", newCashbookExchangeDiff2.AH_ExchangeRateInfo, expectedNotPERRateNotFoundMessage);
						AssertEquals($"{rateType} exchange rate", rate, newCashbookExchangeDiff2.AH_ExchangeRate);
					}
				}
			}
		}

		[TestDate(2020, 2, 6)]
		public override void TestCheckAH_AB()
		{
			base.TestCheckAH_AB();
			var newCashbookExchangeDiff1 = Factory.New<NewCashbookExchangeDiff>();
			var bank = TestObjectCreator.InsertBankAccount(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			var inactiveMessage = "This bank account cannot be chosen because it is inactive. Please un-select this bank account by unticked the ‘Include’ checkbox.";

			bank.AB_IsActive = false;
			newCashbookExchangeDiff1.AH_AB = bank.PK;
			AssertHasError("It is has errors when bank is active.", newCashbookExchangeDiff1.AH_ABInfo, inactiveMessage);

			bank.AB_IsActive = true;
			newCashbookExchangeDiff1.Validation.ValidateAH_AB();
			AssertNoError("It is has no errors when bank is active.", newCashbookExchangeDiff1.AH_ABInfo, inactiveMessage);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
			{
				var newCashbookExchangeDiff2 = Factory.New<NewCashbookExchangeDiff>();
				var localCurrencyBank = TestObjectCreator.InsertBankAccount(Core.Constants.CurrencyCodes.China);
				var foreighCurrencyBank = TestObjectCreator.InsertBankAccount(Core.Constants.CurrencyCodes.Australia);
				var bankCurrencyIsNotApplicableMessage = "This bank account cannot be chosen because it is a local currency bank account of which currency adjustment is not applicable. Please un-select this bank account by unticked the ‘Include’ checkbox.";

				newCashbookExchangeDiff2.AH_AB = localCurrencyBank.PK;
				AssertHasError("The bank currency is local shoule be error.", newCashbookExchangeDiff2.AH_ABInfo, bankCurrencyIsNotApplicableMessage);

				newCashbookExchangeDiff2.AH_AB = foreighCurrencyBank.PK;
				AssertNoError("The bank currency is foreigh shoule be no error.", newCashbookExchangeDiff2.AH_ABInfo, bankCurrencyIsNotApplicableMessage);
			}
		}
	}
}
