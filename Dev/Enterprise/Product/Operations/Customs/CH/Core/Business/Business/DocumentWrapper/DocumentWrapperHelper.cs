using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public static class DocumentWrapperHelper
{
	public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> collection) => collection ?? Enumerable.Empty<T>();

	public static CodeDescriptionPairList GetTaxesAndFeesCodeList(this BusinessObjectFactory factory, ZString documentLanguage) => factory.GetCachedValue($"CH.DocumentWrapperHelper.TaxesAndFees.{documentLanguage}", () =>
	{
		using (Res.TemporarilySwitchLanguage(documentLanguage))
		{
			var loadCriteria = new RateCodeLoadCriteria() { RateTypesToInclude = new ZString[] { RateTypes.AdditionalTaxes, RateTypes.AdditionalFees } };
			var taxesAndFees = CusRefRateCodeView.Loader.Load(factory, Core.Constants.CountryCodes.Switzerland, loadCriteria);
			var list = new CodeDescriptionPairList();
			var language = documentLanguage.GetLanguageCode();
			list.AddPair(AdditionalTaxesTypes.Duty, Res.GetString("FAAC5187-EF80-4ABA-92F2-56962AA805CA", "Duties"));
			foreach (var code in taxesAndFees)
			{
				list.AddPair(code.ZY1_RateCode, TranslationHelper.GetTranslatedValue(code, code.DescriptionNotTranslated, RefCusRateCodeLanguageSchema.ZXC_Description, language));
			}
			return list;
		}
	});

	public static ZString GetVATSuffixText(bool messageVATSuffix) => messageVATSuffix ? VATSuffixText : ZString.Empty;

	static string VATSuffixText => Res.GetString("A2F714B8-1DB4-47FF-B7CF-06C639F70E04", "VAT");

	public static string SelectedPrinterLanguage => Culture.Current.TwoLetterISOLanguageName;

	public static ZString GetDocumentLanguage(this BusinessObjectFactory factory, ZString customsLanguage) => factory.GetCachedValue($"CH.DocumentWrapperHelper.DocumentLanguage.{customsLanguage}", () =>
	{
		var language = customsLanguage;

		var i = language.IndexOf('-');
		if (i >= 0)
		{
			language = language.Substring(0, i);
		}

		if (!language.IsEmpty)
		{
			language = language.ToUpperInvariant();
			switch (language)
			{
				case SwissCustomsLanguageList.Codes.German:
				case SwissCustomsLanguageList.Codes.French:
				case SwissCustomsLanguageList.Codes.Italian:
					var chLanguage = language + "-CH";
					language = LanguageHelper.GetCustomLanguageByLanguageCode(chLanguage) != null ? chLanguage : language + "-" + language;
					break;
				default:
					language = Core.SharedConstants.Languages.EnglishAmerican;
					break;
			}
		}

		return language;
	});

	public static ZString GetLanguageCode(this ZString documentLanguage) => documentLanguage.SubstringSafe(0, 2);

	public static ZString GetMaxNumericValue(this ZString currentValue, ZString newValueToCompare)
	{
		if (int.TryParse(newValueToCompare, out int newValue))
		{
			if (int.TryParse(currentValue, out int maxValue))
			{
				if (newValue > maxValue)
				{
					return newValueToCompare;
				}
			}
			else
			{
				return newValueToCompare;
			}
		}
		return currentValue;
	}
}
