using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.IN.Business.Testing;

public class DummyCurrencyConverter : DummyCurrencyConverterDataProvider, ICurrencyConverterDataProviderWithFixedExRates, ICurrencyConverterDataProvider
{
	public NonStandardExchangeRateCollection NonStandardExchangeRates { get; set; }

	public string FixedExchangeRateCurrencyCode { get; set; }

	public decimal FixedExchangeRate { get; set; }
}
