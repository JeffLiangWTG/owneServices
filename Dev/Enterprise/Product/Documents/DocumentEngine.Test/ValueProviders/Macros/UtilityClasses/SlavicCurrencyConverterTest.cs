using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Utilities.Testing
{
	internal abstract class SlavicCurrencyConverterTest : TestCaseWithFactory
	{
		[DeveloperOnlyTest]
		public void TestAllCurrenciesHaveTranslations()
		{
			var currencies = new RefCurrencyCollection(Factory, new ZQuery(RefCurrencySchema.RX_IsSystem, true));
			var currencyCodes = currencies.Cast<RefCurrency>().Select(c => c.Code);
			var currenciesWithMissingTranslations = currencyCodes.Except(Converter.CurrenciesDictionary.Keys).OrderBy(s => s);
			var translationsWithMissingCurrencies = Converter.CurrenciesDictionary.Keys.Except(currencyCodes).OrderBy(s => s);
			CombineAssertions(() =>
			{
				AssertEquals(string.Format("The following currencies have missing translation for language {0}:\r\n{1}", Language, string.Join(", ", currenciesWithMissingTranslations)), 0, currenciesWithMissingTranslations.Count());
				AssertEquals(string.Format("The following currencies no longer exist. Please remove their translation from the {0} CurrencyToWord Converter:\r\n{1}", Language, string.Join(", ", translationsWithMissingCurrencies)), 0, translationsWithMissingCurrencies.Count());
			});
		}

		[DeveloperOnlyTest]
		public void TestCurrenciesWithTranslationMatchRefCurrencyTranslation()
		{
			var currencies = new RefCurrencyCollection(Factory, new ZQuery(RefCurrencySchema.RX_IsSystem, true));
			var currenciesWithChangedUnitTranslations = new List<string>();
			var currenciesWithChangedSubUnitTranslations = new List<string>();

			foreach (var currency in currencies)
			{
				var isTranslated = currency.RX_UnitNameMultilingual.ToString(Language) != currency.RX_UnitName || currency.RX_SubUnitNameMultilingual.ToString(Language) != currency.RX_SubUnitName;
				if (isTranslated)
				{
					SlavicCurrencyConverter.SlavicCurrencyForms converterTransalation;
					if (Converter.CurrenciesDictionary.TryGetValue(currency.Code, out converterTransalation))
					{
						if (converterTransalation.UnitForm1 != currency.RX_UnitNameMultilingual.ToString(Language))
						{
							currenciesWithChangedUnitTranslations.Add(currency.Code);
						}

						if (converterTransalation.SubUnitForm1 != currency.RX_SubUnitNameMultilingual.ToString(Language))
						{
							currenciesWithChangedSubUnitTranslations.Add(currency.Code);
						}
					}
				}
			}

			CombineAssertions(() =>
			{
				AssertEquals(string.Format("The following currencies have unmatched unit translation for language {0}:\r\n{1}", Language, string.Join(", ", currenciesWithChangedUnitTranslations)), 0, currenciesWithChangedUnitTranslations.Count);
				AssertEquals(string.Format("The following currencies have unmatched sub unit translation for language {0}:\r\n{1}", Language, string.Join(", ", currenciesWithChangedSubUnitTranslations)), 0, currenciesWithChangedSubUnitTranslations.Count);
			});
		}

		protected abstract SlavicCurrencyConverter Converter { get; }

		protected abstract string Language { get; }
	}
}
