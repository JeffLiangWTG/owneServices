using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public abstract class CountryLabelTranslatorTest : TestCaseWithFactory
	{
		protected ICountrySpecificLabelTranslator GetCountryLabelTranslator => ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(CountryCode) as ICountrySpecificLabelTranslator);

		protected abstract string CountryCode { get; }

		protected abstract Dictionary<LabelsEnum, (string[] languages, object[] parametersForTranslation, string translation)[]> ExpectedLabelTranslations { get; }

		public void TestAccountingCountryFactory_GetTranslation_With_Invalid_Label()
		{
			var countryLabelTranslator = GetCountryLabelTranslator;

			foreach (var language in AllLanguages)
			{
				using (Res.TemporarilySwitchLanguage(language))
				{
					AssertNull(countryLabelTranslator.GetTranslation(null));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestGetTranslationForAllLabels()
		{
			var countryLabelTranslator = GetCountryLabelTranslator;
			var expectedLabelTranslations = ExpectedLabelTranslations;

			foreach (LabelsEnum label in Enum.GetValues(typeof(LabelsEnum)))
			{
				foreach (var language in AllLanguages)
				{
					using (Res.TemporarilySwitchLanguage(language))
					{
						expectedLabelTranslations.TryGetValue(label, out var expectedLabelTranslation);
						if (expectedLabelTranslation != null)
						{
							var expectedLabelTranslationFound = expectedLabelTranslation.FirstOrDefault(x => x.languages.Contains(language));
							var result = countryLabelTranslator.GetTranslation(label, expectedLabelTranslationFound.parametersForTranslation);

							if (expectedLabelTranslationFound.translation != null)
							{
								AssertEquals($"The label {label} of {language} should be correct", expectedLabelTranslationFound.translation, result);
							}
							else
							{
								AssertNull($"the translated label {label} of language {language} should be null", result);
							}
						}
						else
						{
							var result = countryLabelTranslator.GetTranslation(label, "XXX", "XXX");
							AssertNull($"the translated label {label} of language {language} should be null", result);
						}
					}
				}
			}
		}

		protected static string[] SpanishLanguages = new[]
		{
				Core.SharedConstants.Languages.Spanish,
				Core.SharedConstants.Languages.SpanishLatin
		};

		protected static string[] EnglishLanguages = new[]
		{
				Core.SharedConstants.Languages.English,
				Core.SharedConstants.Languages.EnglishAmerican,
				Core.SharedConstants.Languages.EnglishBritish
		};

		public static string[] AllLanguages = typeof(SharedConstants.Languages).GetFields().Select(x => x.GetValue(null).ToString()).ToArray();

		public const object[] EmptyParametersForTranslation = null;
	}
}
