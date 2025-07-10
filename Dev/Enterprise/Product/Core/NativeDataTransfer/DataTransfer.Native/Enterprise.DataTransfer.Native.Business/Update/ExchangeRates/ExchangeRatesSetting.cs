using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.Interceptors;

namespace Enterprise.DataTransfer.Native.Business.Update.ExchangeRates;

public class ExchangeRatesSetting : BaseInterceptorSetting
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "IEnumerable string")]
	public override IEnumerable<string> EnableList => ["CurrencyExchangeRate"];

	public override IEnumerable<string> DisableList => [];
}
