using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class ExchangeRateSerializerHelperTest : TestCaseWithFactory
	{
		public void TestAddRates()
		{
			var helper = new ExchangeRateSerializerHelperForTest();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.00000;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			exchangeRate2.ExCurrency.RX_ISOSubUnitRatio = 1000;
			Factory.Save();

			AssertEquals("Should be successful", true, string.IsNullOrEmpty(helper.AddRates(new[] { item })));
			var rates = helper.ElectronicProcessingFeesExposed;
			AssertEquals("Should have exported 2 rates for the item", 2, rates.Count);
			var rate1 = rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item.L7_Category}{item.L7_Code}{item.L7_RN_NKDisbursementCountry}{item.L7_DisbursementDirection}{exchangeRate2.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")];
			AssertEquals(priceHeader.L6_SystemCode, rate1.EPF_SystemCode);
			AssertEquals(item.L7_Code, rate1.EPF_Code);
			AssertEquals(item.L7_Description, rate1.EPF_Description);
			AssertEquals(exchangeRate2.RE_RX_NKExCurrency, rate1.EPF_Currency);
			AssertEquals(item.L7_DisbursementDirection, rate1.EPF_JobDirection);
			AssertEquals(item.L7_RN_NKDisbursementCountry, rate1.EPF_CountryCode);
			AssertEquals(item.L7_Price, rate1.EPF_Price);
			AssertEquals(priceHeader.L6_ValidFrom.ToDateTime(), rate1.EPF_ValidFrom);
		}

		public void TestAddRates_MultipleItems()
		{
			var helper = new ExchangeRateSerializerHelperForTest();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item1 = priceHeader.Items.AddNew();
			var item2 = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();
			var referenceExchangeRate = Factory.New<RefExchangeRate>();
			var invalidDateSellRate = Factory.New<RefExchangeRate>();
			var validDateBuyRate = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = referenceExchangeRate.RE_StartDate = validDateBuyRate.RE_StartDate = new ZDateTime(2010, 1, 1);
			priceHeader.L6_RX_NKCurrency = "USD";
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = referenceExchangeRate.RE_ExpiryDate = validDateBuyRate.RE_ExpiryDate = invalidDateSellRate.RE_StartDate = new ZDateTime(2010, 2, 1);
			invalidDateSellRate.RE_ExpiryDate = new ZDateTime(2010, 3, 1);
			item1.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item1.L7_Code = "BBB";
			item1.L7_Description = "Hellooooooooo";
			item1.L7_Price = 5.23;
			item1.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			item2.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item2.L7_Code = "CCC";
			item2.L7_Description = "Oh";
			item2.L7_Price = 7.23;
			item2.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "NZD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.087800;
			exchangeRate1.ExCurrency.RX_ISOSubUnitRatio = 100;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 0.674600;
			exchangeRate2.ExCurrency.RX_ISOSubUnitRatio = 1000;
			referenceExchangeRate.RE_RX_NKExCurrency = "AUD";
			referenceExchangeRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			referenceExchangeRate.RE_SellRate = 1.00000;
			referenceExchangeRate.ExCurrency.RX_ISOSubUnitRatio = 1;

			invalidDateSellRate.RE_RX_NKExCurrency = "AUD";
			invalidDateSellRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			invalidDateSellRate.RE_SellRate = 3.212;
			validDateBuyRate.RE_RX_NKExCurrency = "USD";
			validDateBuyRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			validDateBuyRate.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should be successful", true, string.IsNullOrEmpty(helper.AddRates(new[] { item1, item2 })));
			var rates = helper.ElectronicProcessingFeesExposed;
			AssertEquals("Should have exported 3 rates for each item. Invalid date sell rate and valid date buy rate should not be exported", 6, rates.Count);
			AssertEquals("Should divide base price to get to base rate then multiply by exchange rate. Should round to sub unit ratio", Enterprise.ZArchitecture.Core.Utilities.Round((5.23m / 0.674600m) * 1.087800m, 2), rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item1.L7_Category}{item1.L7_Code}{item1.L7_RN_NKDisbursementCountry}{item1.L7_DisbursementDirection}{exchangeRate1.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")].EPF_Price);
			AssertEquals("Should divide base price to get to base rate then multiply by exchange rate. Should round to sub unit ratio", 5.23m, rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item1.L7_Category}{item1.L7_Code}{item1.L7_RN_NKDisbursementCountry}{item1.L7_DisbursementDirection}{exchangeRate2.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")].EPF_Price);
			AssertEquals("Should divide base price to get to base rate then multiply by exchange rate. Should round to sub unit ratio", Enterprise.ZArchitecture.Core.Utilities.Round(5.23m / 0.674600m, 0), rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item1.L7_Category}{item1.L7_Code}{item1.L7_RN_NKDisbursementCountry}{item1.L7_DisbursementDirection}{referenceExchangeRate.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")].EPF_Price);
			AssertEquals("Should divide base price to get to base rate then multiply by exchange rate. Should round to sub unit ratio", Enterprise.ZArchitecture.Core.Utilities.Round((7.23m / 0.674600m) * 1.087800m, 2), rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item2.L7_Category}{item2.L7_Code}{item2.L7_RN_NKDisbursementCountry}{item2.L7_DisbursementDirection}{exchangeRate1.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")].EPF_Price);
			AssertEquals("Should divide base price to get to base rate then multiply by exchange rate. Should round to sub unit ratio", 7.23m, rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item2.L7_Category}{item2.L7_Code}{item2.L7_RN_NKDisbursementCountry}{item2.L7_DisbursementDirection}{exchangeRate2.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")].EPF_Price);
			AssertEquals("Should divide base price to get to base rate then multiply by exchange rate. Should round to sub unit ratio", Enterprise.ZArchitecture.Core.Utilities.Round(7.23m / 0.674600m, 0), rates[FormattableString.Invariant($"{priceHeader.L6_SystemCode}{item2.L7_Category}{item2.L7_Code}{item2.L7_RN_NKDisbursementCountry}{item2.L7_DisbursementDirection}{referenceExchangeRate.RE_RX_NKExCurrency}{priceHeader.L6_ValidFrom.ToDateTime()}")].EPF_Price);
		}

		public void TestAddRates_ShouldReturnErrorMessageIfCurrencyIsPopulated()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_RX_NKCurrency = "NZD";
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since L7_RX_NKCurrency is populated", "Exchange rates cannot be exported for Price Items with Currency overrides.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);
		}

		public void TestAddRates_ShouldReturnErrorMessageIfCategoryIsNotSet()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			item.L7_Category = string.Empty;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since L7_Category is empty", "Price Item Category must be entered.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);
		}

		public void TestAddRates_ShouldReturnErrorMessageIfCodeIsNotSet()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			item.L7_Category = "ABC";
			item.L7_Code = string.Empty;
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since L7_Code is empty", "Price Item Code must be entered.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);
		}

		public void TestAddRates_ShouldReturnErrorMessageIfSystemCodeIsNotCWN()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "GGG";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since L6_SystemCode is ODM", "System Code must be CargoWise Next.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);
		}

		public void TestAddRates_MissingBaseCurrencyExchangeRate_ShouldReturnErrorMessage()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AED";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.0m;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since the base rate is missing", "There is no exchange rate for the Price Header Currency in the Exchange Rates Module.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);

			exchangeRate1.RE_RX_NKExCurrency = "AED";
			Factory.Save();
			AssertNotEquals("Error message should not show anymore", "There is no exchange rate for the Price Header Currency in the Exchange Rates Module.", helper.AddRates(new[] { item }));
		}

		public void TestAddRates_CountryNotNull_ShouldReturnErrorMessage()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_RN_NKDisbursementCountry = "AU";
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.0m;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since the disbursement country is not empty", "Disbursement country must be empty for Price Items.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);
		}

		public void TestAddRates_DisbursementDirectionNotAll_ShouldReturnErrorMessage()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.0m;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			var helper = new ExchangeRateSerializerHelperForTest();
			AssertEquals("Should fail since the disbursement direction is not ALL", "Disbursement direction must be ALL for Price Items.", helper.AddRates(new[] { item }));
			AssertEquals("Should not have added any rates", 0, helper.ElectronicProcessingFeesExposed.Count);

			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			Factory.Save();
			AssertEquals("Should pass since the disbursement direction is ALL", true, string.IsNullOrEmpty(helper.AddRates(new[] { item })));
			AssertEquals("Should add rates", 2, helper.ElectronicProcessingFeesExposed.Count);
		}

		public void TestAddRates_ShouldFailIfHasChanges()
		{
			var helper = new ExchangeRateSerializerHelper();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;

			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", helper.AddRates(new[] { item }));

			Factory.Save();
			helper = new ExchangeRateSerializerHelper();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(helper.AddRates(new[] { item })));

			Factory.Save();
			item.L7_Description = "Change";
			helper = new ExchangeRateSerializerHelper();
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", helper.AddRates(new[] { item }));

			Factory.Save();
			priceHeader.L6_DiscountCode = "V1";
			helper = new ExchangeRateSerializerHelper();
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", helper.AddRates(new[] { item }));

			Factory.Save();
			helper = new ExchangeRateSerializerHelper();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(helper.AddRates(new[] { item })));
		}

		public void TestRunValidation_ShouldReturnErrorMessageIfNoPriceItems()
		{
			AssertEquals("Should fail since no price items were passed in", "Please select at least one Price Item.", ExchangeRateSerializerHelper.RunValidation(new List<ClientLicencePriceItem>()));
		}

		public void TestRunValidation_ShouldReturnErrorMessageIfCurrencyIsPopulated()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = "AAA";
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_RX_NKCurrency = "NZD";
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since L7_RX_NKCurrency is populated", "Exchange rates cannot be exported for Price Items with Currency overrides.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));
		}

		public void TestRunValidation_ShouldReturnErrorMessageIfCategoryIsNotSet()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = string.Empty;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since L7_Category is empty", "Price Item Category must be entered.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));
		}

		public void TestRunValidation_ShouldReturnErrorMessageIfCodeIsNotSet()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = "ABC";
			item.L7_Code = string.Empty;
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since L7_Code is empty", "Price Item Code must be entered.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));
		}

		public void TestRunValidation_ShouldReturnErrorMessageIfSystemCodeIsNotCWN()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.BillingSystem.ODM;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = "ABC";
			item.L7_Code = string.Empty;
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since L6_SystemCode is ODM", "System Code must be CargoWise Next.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));
		}

		public void TestRunValidation_MissingBaseCurrencyExchangeRate_ShouldReturnErrorMessage()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AED";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 1.0m;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since the base rate is missing", "There is no exchange rate for the Price Header Currency in the Exchange Rates Module.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));

			exchangeRate1.RE_RX_NKExCurrency = "AED";
			Factory.Save();
			AssertNotEquals("Error message should not show anymore", "There is no exchange rate for the Price Header Currency in the Exchange Rates Module.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));
		}

		public void TestRunValidation_CountryNotNull_ShouldReturnErrorMessage()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_RN_NKDisbursementCountry = "AU";
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since the disbursement country is not empty", "Disbursement country must be empty for Price Items.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));
		}

		public void TestRunValidation_DisbursementDirectionNotAll_ShouldReturnErrorMessage()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.Domestic;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Should fail since the disbursement direction is not ALL", "Disbursement direction must be ALL for Price Items.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));

			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			Factory.Save();

			AssertEquals("Should pass since the disbursement direction is ALL", true, string.IsNullOrEmpty(ExchangeRateSerializerHelper.RunValidation(new[] { item })));
		}

		public void TestRunValidation_ShouldFailIfHasChanges()
		{
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;

			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));

			Factory.Save();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(ExchangeRateSerializerHelper.RunValidation(new[] { item })));

			item.L7_Description = "Change";
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));

			Factory.Save();
			priceHeader.L6_DiscountCode = "V1";
			AssertEquals("Should prompt save before generation if it has changes", "Please save changes before generating the XML.", ExchangeRateSerializerHelper.RunValidation(new[] { item }));

			Factory.Save();
			AssertEquals("Should succeed since changes are saved", true, string.IsNullOrEmpty(ExchangeRateSerializerHelper.RunValidation(new[] { item })));
		}

		public void TestSerializeData()
		{
			var helper = new ExchangeRateSerializerHelper();
			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			var item = priceHeader.Items.AddNew();
			var exchangeRate1 = Factory.New<RefExchangeRate>();
			var exchangeRate2 = Factory.New<RefExchangeRate>();

			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.CargoWiseNext;
			priceHeader.L6_ValidFrom = exchangeRate1.RE_StartDate = exchangeRate2.RE_StartDate = new ZDateTime(2010, 1, 1);
			exchangeRate1.RE_ExpiryDate = exchangeRate2.RE_ExpiryDate = new ZDateTime(2010, 2, 1);
			priceHeader.L6_RX_NKCurrency = "AUD";
			item.L7_Category = BillingConstants.PriceHeaderType.CargoWiseNext;
			item.L7_Code = "BBB";
			item.L7_Description = "Hellooooooooo";
			item.L7_Price = 5.23;
			item.L7_DisbursementDirection = OrgDocumentLookups.FilterDirectionConstants.Codes.All;
			exchangeRate1.RE_RX_NKExCurrency = "AUD";
			exchangeRate1.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate1.RE_SellRate = 3.212;
			exchangeRate2.RE_RX_NKExCurrency = "USD";
			exchangeRate2.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
			exchangeRate2.RE_SellRate = 7.423;
			Factory.Save();

			AssertEquals("Precondition: Should be successful", true, string.IsNullOrEmpty(helper.AddRates(new[] { item })));
			var result = helper.SerializeRates();

			AssertEquals("Serialization should be successful", true, result.Success);
			AssertEquals("Error message should be empty", true, string.IsNullOrEmpty(result.ErrorMessage));
			AssertEquals("Rates should be serialized", false, string.IsNullOrEmpty(result.SeralizedValue.OuterXml));
		}

		public void TestSerializeData_NoRates()
		{
			var helper = new ExchangeRateSerializerHelper();
			var result = helper.SerializeRates();
			AssertEquals("Should not succeed since there are no rates", false, result.Success);
			AssertEquals("Should not succeed since there are no rates", "No rates were available to be serialized.", result.ErrorMessage);
		}

		class ExchangeRateSerializerHelperForTest : ExchangeRateSerializerHelper
		{
			internal Dictionary<string, CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType.RefAccElectronicProcessingFee> ElectronicProcessingFeesExposed => ElectronicProcessingFees;
		}
	}
}
