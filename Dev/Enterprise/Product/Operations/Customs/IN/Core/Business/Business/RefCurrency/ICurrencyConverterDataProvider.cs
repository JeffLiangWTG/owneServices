namespace Enterprise.Customs.IN.Business;

public interface ICurrencyConverterDataProvider : MasterFiles.Business.ICurrencyConverterDataProvider
{
	NonStandardExchangeRateCollection NonStandardExchangeRates { get; }
}
