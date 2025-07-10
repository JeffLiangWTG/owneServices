using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business.DocumentWrappers
{
	static class DocCusHelper
	{
		public static string ToStringRounded(this ZDecimal d, int numberOfDecimals) => d.Round(numberOfDecimals).ToString(numberOfDecimals, true);

		public static string GetGermanRateCodeDescription(this BusinessObjectFactory factory, string rateCode, string dataGrouping)
		{
			var cusRefRateCodeViews = CusRefRateCodeView.Loader.Load(factory, dataGrouping);
			var result = GetGermanTranslation(cusRefRateCodeViews.SingleOrDefault(rcv => rcv.ZY1_RateCode == rateCode && rcv.ZY1_ZZZ_NKDataGrouping == dataGrouping))
				?? GetGermanTranslation(cusRefRateCodeViews.SingleOrDefault(rcv => rcv.ZY1_RateCode == rateCode && rcv.ZY1_ZZZ_NKDataGrouping == Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN));

			if (string.IsNullOrWhiteSpace(result) && rateCode == Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat)
			{
				result = GermanDescriptionForChargeTypeB00;
			}
			return result;
		}

		static string GetGermanTranslation(CusRefRateCodeView rateCode)
		{
			if (rateCode != null)
			{
				var germanLanguageCode = TranslationHelper.GetLanguageCodeForCountry(Core.Constants.CountryCodes.Germany);
				var translatedValue = TranslationHelper.GetTranslatedValue(rateCode, ZString.Empty, RefCusRateCodeLanguageSchema.ZXC_Description, germanLanguageCode);
				if (!translatedValue.IsEmpty)
				{
					return translatedValue;
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Description String")]
		const string GermanDescriptionForChargeTypeB00 = "Einfuhrumsatzsteuer (EUSt)";
	}
}
