
using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.ElectronicPayment.Universal
{
	internal static class AccEPaymentQuoteMessageConstants
	{
		internal static class XUEFieldNames
		{
			public static ZString CompanyCode => "CompanyCode";
			public static ZString ProviderRef => "ProviderRef";
			public static ZString LedgerType => "LedgerType";
			public static ZString FromCurrency => "FromCurrency";
			public static ZString FromAmount => "FromAmount";
			public static ZString ToCurrency => "ToCurrency";
			public static ZString ToAmount => "ToAmount";
			public static ZString ExchangeRate => "ExchangeRate";
			public static ZString ExchangeRateInverted => "ExchangeRateInverted";

			public static ZString[] GetRequiredFields(ZString messageMode)
			{
				var fields = new List<ZString> { CompanyCode, LedgerType, FromCurrency, FromAmount, ToCurrency, ToAmount, ExchangeRate, ExchangeRateInverted };
				if (messageMode == MessageModes.GetQuote)
				{
					fields.Add(ProviderRef);
				}
				return fields.ToArray();
			}

			public static ZString FeeCurrency => "FeeCurrency";
			public static ZString FeeAmount => "FeeAmount";
			public static ZString ErrorMessage => "ErrorMessage";
		}

		internal static class MessageModes
		{
			public static ZString GetQuote => "GAQ";
			public static ZString GetRate => "GRT";
		}
	}
}
