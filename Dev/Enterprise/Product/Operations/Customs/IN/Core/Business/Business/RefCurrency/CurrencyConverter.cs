using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IN.Business;

public class CurrencyConverter(BusinessObjectFactory factory, ICurrencyConverterDataProvider dataProvider, bool roundToTargetCurrencyDecimals = true)
	: CurrencyConverterWithDataProvider(factory, dataProvider, roundToTargetCurrencyDecimals)
{
	public override ZDecimal GetExchangeRate(ICurrency currency, out ZDateTime foundRateDate)
	{
		var lookupRate = ZDecimal.Zero;
		foundRateDate = ZDateTime.Empty;

		if (currency != null)
		{
			if (!string.IsNullOrEmpty(currency.Code)
				&& DataProvider is ICurrencyConverterDataProviderWithFixedExRates dataProviderWithFixedExRates
				&& dataProviderWithFixedExRates.FixedExchangeRateCurrencyCode.Equals(currency.Code, StringComparison.OrdinalIgnoreCase))
			{
				lookupRate = dataProviderWithFixedExRates.FixedExchangeRate;
			}
			else if (IsNonStandardCurrency(currency.Code)
				&& DataProvider.NonStandardExchangeRates?.GetByCurrencyCode(currency.Code) is { } rate)
			{
				foundRateDate = DateForRate;
				lookupRate = rate.CSI_Value;
			}
			else
			{
				lookupRate = base.GetExchangeRate(currency, out foundRateDate);
			}
		}
		return lookupRate;
	}

	public bool IsNonStandardCurrency(ZString currency)
	{
		return !currency.IsEmpty && !GetStandardCurrencyCodes().ContainsCode(currency);
	}

	CodeDescriptionPairList GetStandardCurrencyCodes()
	{
		return RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.India, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.INCustomsStandardCurrency, DataProvider.DateOfValuation);
	}

	new ICurrencyConverterDataProvider DataProvider => (ICurrencyConverterDataProvider)base.DataProvider;
}
