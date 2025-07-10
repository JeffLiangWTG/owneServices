using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(ZAccExchangeRate))]
	public class ZAccExchangeRate_InnerTest : NonPersistentBusinessObjectTestCase
	{
		#region ExchangeRateFilter Test

		public void TestExchangeRateFilter()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			GlbCompany companyDifferent = Factory.NewWithValidTestData<GlbCompany>();

			RefExchangeRate exRateCurrentCompany = GetExchangeRateForTest(currency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);

			RefExchangeRate exRateDifferentCompany = GetExchangeRateForTest(currency, companyDifferent, Constants.ExchangeRateTypes.Code.BuyRate);

			RefExchangeRate exRateDifferentCurrency = GetExchangeRateForTest(GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRateDifferentCurrency.RE_StartDate = ZDateTime.Today;

			RefExchangeRate exRateEarlierDate = GetExchangeRateForTest(GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRateEarlierDate.RE_StartDate = ZDateTime.Today.AddDays(-1);

			RefExchangeRate exRateLaterDate = GetExchangeRateForTest(GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRateLaterDate.RE_StartDate = ZDateTime.Today.AddDays(1);

			RefExchangeRate exRateLatestDate = GetExchangeRateForTest(GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRateLatestDate.RE_StartDate = ZDateTime.Today.AddDays(2);

			RefExchangeRate exRateSellType = GetExchangeRateForTest(GlbCompany.CurrentCompany.LocalCurrency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.SellRate);

			DummyWithRate.Z0_Code = currency.RX_Code;
			RefExchangeRateCollection exRates = new RefExchangeRateCollection(Factory);
			exRates.AdditionalFilter = ExchangeRate.GetExchangeRateFilter_ForTestOnly(currency.RX_Code, false);

			AssertEquals("There should be 1 exchange rate in the collection", 1, exRates.Count);
			Assert("ExRateCurrentCompany should be included", exRates.Contains(exRateCurrentCompany));

			DummyWithRate.Z0_Code = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRates = new RefExchangeRateCollection(Factory);
			exRates.AdditionalFilter = ExchangeRate.GetExchangeRateFilter_ForTestOnly(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, false);

			AssertEquals("There should be 3 exchangeRates in the collection", 2, exRates.Count);
			Assert("ExRateDifferentCurrency should be included", exRates.Contains(exRateDifferentCurrency));
			Assert("ExRateEarlierDate should be included", exRates.Contains(exRateEarlierDate));
			Assert("ExRateLaterDate should NOT be included", !exRates.Contains(exRateLaterDate));
		}

		#endregion

		#region GetTodaysRate Test

		public void TestGetTodaysRate()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			RefExchangeRate exRateExpired = GetExchangeRateForTest(currency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRateExpired.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRateExpired.RE_ExpiryDate = ZDateTime.Today.AddDays(-1);
			exRateExpired.RE_SellRate = 0.789m;
			RefExchangeRate tomorrowsExRate = GetExchangeRateForTest(currency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			tomorrowsExRate.RE_StartDate = ZDateTime.Today.AddDays(1);
			tomorrowsExRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			tomorrowsExRate.RE_SellRate = 0.456m;

			SetExchangeRateFallbackRegistryValue(false);
			AssertEquals("Rate should be 0 since fallback is false", 0m, ExchangeRate.GetTodaysRate_ForTestOnly(currency.RX_Code));
			SetExchangeRateFallbackRegistryValue(true);
			AssertEquals("Rate should be 0.789 since fallback is true", 0.789m, ExchangeRate.GetTodaysRate_ForTestOnly(currency.RX_Code));

			ExchangeRateReader.GetReaderInstance().ClearCache();
			RefExchangeRate exRateValid = GetExchangeRateForTest(currency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRateValid.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exRateValid.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
			exRateValid.RE_SellRate = 0.123m;
			Factory.Save();

			SetExchangeRateFallbackRegistryValue(false);
			AssertEquals("Rate should be 0.123 because it is the most recent valid rate", 0.123m, ExchangeRate.GetTodaysRate_ForTestOnly(currency.RX_Code));
			SetExchangeRateFallbackRegistryValue(true);
			AssertEquals("Rate should be 0.123 because it is the most recent valid rate", 0.123m, ExchangeRate.GetTodaysRate_ForTestOnly(currency.RX_Code));
		}

		#endregion

		#region ValidateRate Test

		public void TestValidateRate()
		{
			bool previousExchangeRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				SetExchangeRateFallbackRegistryValue(true);
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate expiredRate = GetExchangeRateForTest(currency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
				expiredRate.RE_StartDate = ZDateTime.Today.AddDays(-4);
				expiredRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-3);
				expiredRate.RE_SellRate = 0.777m;

				RefCurrency currency2 = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate validRate = GetExchangeRateForTest(currency2, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
				validRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
				validRate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
				validRate.RE_SellRate = 0.666m;

				AssertNoNotifications("ExchangeRate should not have warnings", Dummy.Z0_AnotherDecimalInfo);

				ExchangeRate.Currency = currency.RX_Code;
				DummyWithRate.Validation.ValidateZ0_AnotherDecimal();
				AssertHasWarning(DummyWithRate.Z0_AnotherDecimalInfo, ExchangeRate.ExpiryDateWarning);

				ExchangeRate.Rate = 0.888m;
				DummyWithRate.Validation.ValidateZ0_AnotherDecimal();
				AssertNoWarning(DummyWithRate.Z0_AnotherDecimalInfo, ExchangeRate.ExpiryDateWarning);

				ExchangeRate.Currency = currency2.RX_Code;
				DummyWithRate.Validation.ValidateZ0_AnotherDecimal();
				AssertNoWarning(DummyWithRate.Z0_AnotherDecimalInfo, ExchangeRate.ExpiryDateWarning);
				AssertEquals("Rate should be 0.666", 0.666m, DummyWithRate.Z0_AnotherDecimal);
			}
			finally
			{
				SetExchangeRateFallbackRegistryValue(previousExchangeRateFallback);
			}
		}

		#endregion

		#region GetMostRecentExchangeRate Test

		public void TestGetMostRecentExchangeRate()
		{
			RefCurrency foreignCurrency = Factory.NewWithValidTestData<RefCurrency>();
			RefExchangeRate exRate = GetExchangeRateForTest(foreignCurrency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-4);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-3);
			AssertNull("Most recent exchange rate for local currency should be null", DummyWithRate.ExchangeRate.GetMostRecentExchangeRate_ForTestOnly(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
			AssertEquals("Most recent exchange rate for foreign currency should not be null", exRate.PK, DummyWithRate.ExchangeRate.GetMostRecentExchangeRate_ForTestOnly(foreignCurrency.RX_Code).PK);
		}

		#endregion

		#region IsExchangeRateFallbackRequired Test

		public void TestIsExchangeRateFallbackRequired()
		{
			bool previousExRateFallback = AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.Value;
			try
			{
				SetExchangeRateFallbackRegistryValue(true);
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate exRate = GetExchangeRateForTest(currency, GlbCompany.CurrentCompany, Constants.ExchangeRateTypes.Code.BuyRate);
				exRate.RE_StartDate = ZDateTime.Today.AddDays(-3);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);

				ExchangeRate.Currency = currency.RX_Code;
				Assert("IsExchangeRateFallbackRequired should be false", !ExchangeRate.IsExchangeFallbackRequired);
				exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(-2);
				Assert("IsExchangeRateFallbackRequired should be true", ExchangeRate.IsExchangeFallbackRequired);
			}
			finally
			{
				SetExchangeRateFallbackRegistryValue(previousExRateFallback);
			}
		}

		#endregion

		#region Implementation

		void SetExchangeRateFallbackRegistryValue(bool value)
		{
			AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
					Guid.Empty,
					Guid.Empty,
					value);
		}

		RefExchangeRate GetExchangeRateForTest(RefCurrency currency, GlbCompany company, ZString rateType)
		{
			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_RX_NKExCurrency = currency.RX_Code;
			exRate.RE_GC = company.PK;
			exRate.RE_ExRateType = rateType;
			return exRate;
		}

		DummyBusinessObject Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyBusinessObject>()); }
		}
		DummyBusinessObject dummy;

		ZAccExchangeRate ExchangeRate
		{
			get { return DummyWithRate.ExchangeRate; }
		}

		DummyWithAccExchangeRateBusinessObject DummyWithRate
		{
			get
			{
				if (dummyWithRate == null)
				{
					dummyWithRate = Factory.New<DummyWithAccExchangeRateBusinessObject>();
				}
				return dummyWithRate;
			}
		}
		DummyWithAccExchangeRateBusinessObject dummyWithRate;

		protected override BusinessObject GetNewBusinessObject()
		{
			return DummyWithRate.ExchangeRate;
		}

		#endregion
	}
}
