using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class CultureTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCountryCodeBW()
		{
			AssertEquals("Culture for Botswana should be en-GB", Culture.GetCulture("BW").ToString(), "en-GB");
		}

		public void TestGetMaldivesLCID()
		{
			AssertEquals(Culture.GetCulture("MV"), new CultureInfo("dv-MV"));
		}

		public void TestAbbreviatedMonthNamesForCzech()
		{
			var originalCulture = Culture.Current;
			try
			{
				var czechCulture = Culture.GetCulture("CZ");
				Culture.Set(czechCulture);

				AssertEquals("led.", new DateTime(2010, 1, 1).ToString("MMM"));
				AssertEquals("un.", new DateTime(2010, 2, 1).ToString("MMM"));
				AssertEquals("brez.", new DateTime(2010, 3, 1).ToString("MMM"));
				AssertEquals("dub.", new DateTime(2010, 4, 1).ToString("MMM"));
				AssertEquals("kvet.", new DateTime(2010, 5, 1).ToString("MMM"));
				AssertEquals("cerv.", new DateTime(2010, 6, 1).ToString("MMM"));
				AssertEquals("cerven.", new DateTime(2010, 7, 1).ToString("MMM"));
				AssertEquals("srp.", new DateTime(2010, 8, 1).ToString("MMM"));
				AssertEquals("zari.", new DateTime(2010, 9, 1).ToString("MMM"));
				AssertEquals("rij.", new DateTime(2010, 10, 1).ToString("MMM"));
				AssertEquals("list.", new DateTime(2010, 11, 1).ToString("MMM"));
				AssertEquals("pros.", new DateTime(2010, 12, 1).ToString("MMM"));
			}
			finally
			{
				Culture.Set(originalCulture);
			}
		}

		public void TestGetCultureForLanguageMustReturnSomethingForSupportedLanguage()
		{
			CombineAssertions(() =>
			{
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					AssertNotNull(
						string.Format("Culture.GetCultureForLanguage(languageCode) should not return null for language code [{0}].", language),
						Culture.GetCultureForLanguage(language));
				}
			});
		}

		public void TestGetCalendar()
		{
			var list = new List<string>() {
			"ChineseLunisolarCalendar",
				"GregorianCalendar",
				 "HebrewCalendar",
				 "HijriCalendar",
				 "JapaneseCalendar",
				 "JapaneseLunisolarCalendar",
				 "JulianCalendar",
				 "KoreanCalendar",
				 "KoreanLunisolarCalendar",
				 "PersianCalendar",
				 "TaiwanCalendar",
				 "TaiwanLunisolarCalendar",
				 "ThaiBuddhistCalendar",
				 "UmAlQuraCalendar" };
			CombineAssertions(() =>
			{
				foreach (var calendar in list)
				{
					AssertNotNull(
						string.Format("Culture.GetCalendar(calendar) should not return null for calendar [{0}].", calendar),
						Culture.GetCalendar(calendar));
				}
			});
		}

		public void TestSetTemporarilyCalendar()
		{
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Enterprise.Core.Constants.Languages.Arabic)))
			using (var arabicCache = Res.GetLanguageInstance(Enterprise.Core.Constants.Languages.Arabic).UseMockData())
			{
				var currentCalendar = CultureInfo.CurrentCulture.DateTimeFormat.Calendar;

				using (Culture.SetTemporarilyCalendar(Culture.GetCalendar("GregorianCalendar")))
				{
					AssertEquals("Calendar has changd", typeof(GregorianCalendar), CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetType());
				}

				AssertEquals("Temporary change has been disposed", currentCalendar.GetType(), CultureInfo.CurrentCulture.DateTimeFormat.Calendar.GetType());
			}
		}

		public void TestGetLanguageForCulture()
		{
			CombineAssertions(() =>
			{
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					//the culture for the language Dzongkha is not supported in windows version earlier than windows 10 and windows server 2016, so return default culture for those windows version
					//https://msdn.microsoft.com/en-us/library/cc233982.aspx
					if (language != Res.DefaultLanguage && language != SharedConstants.Languages.Dzongkha)
					{
						AssertEquals(language, Culture.GetLanguageForCulture(Culture.GetCultureForLanguage(language)));
					}
				}
			});
		}

		public void TestGetCultureForLanguage()
		{
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Portuguese).Name", "pt-PT", Culture.GetCultureForLanguage(Constants.Languages.Portuguese).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.German).Name", "de-DE", Culture.GetCultureForLanguage(Constants.Languages.German).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Dutch).Name", "nl-NL", Culture.GetCultureForLanguage(Constants.Languages.Dutch).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.French).Name", "fr-FR", Culture.GetCultureForLanguage(Constants.Languages.French).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.ChineseTraditional).Name", "zh-HK", Culture.GetCultureForLanguage(Constants.Languages.ChineseTraditional).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.ChineseSimplified).Name", "zh-CN", Culture.GetCultureForLanguage(Constants.Languages.ChineseSimplified).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.EnglishAmerican).Name", "en-US", Culture.GetCultureForLanguage(Constants.Languages.EnglishAmerican).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Finnish).Name", "fi-FI", Culture.GetCultureForLanguage(Constants.Languages.Finnish).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Greek).Name", "el-GR", Culture.GetCultureForLanguage(Constants.Languages.Greek).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Spanish).Name", "es-ES", Culture.GetCultureForLanguage(Constants.Languages.Spanish).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.SpanishLatin).Name", "es-LA", Culture.GetCultureForLanguage(Constants.Languages.SpanishLatin).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Vietnamese).Name", "vi-VN", Culture.GetCultureForLanguage(Constants.Languages.Vietnamese).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Swedish).Name", "sv-SE", Culture.GetCultureForLanguage(Constants.Languages.Swedish).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Russian).Name", "ru-RU", Culture.GetCultureForLanguage(Constants.Languages.Russian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Bulgarian).Name", "bg-BG", Culture.GetCultureForLanguage(Constants.Languages.Bulgarian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Hungarian).Name", "hu-HU", Culture.GetCultureForLanguage(Constants.Languages.Hungarian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Romanian).Name", "ro-RO", Culture.GetCultureForLanguage(Constants.Languages.Romanian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Hebrew).Name", "he-IL", Culture.GetCultureForLanguage(Constants.Languages.Hebrew).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Khmer).Name", "km-KH", Culture.GetCultureForLanguage(Constants.Languages.Khmer).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Albanian).Name", "sq-AL", Culture.GetCultureForLanguage(Constants.Languages.Albanian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Armenian).Name", "hy-AM", Culture.GetCultureForLanguage(Constants.Languages.Armenian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Azerbaijani).Name", "az", Culture.GetCultureForLanguage(Constants.Languages.Azerbaijani).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Bangla).Name", "bn-BD", Culture.GetCultureForLanguage(Constants.Languages.Bangla).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Bosnian).Name", "bs-Latn-BA", Culture.GetCultureForLanguage(Constants.Languages.Bosnian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Malay).Name", "ms-MY", Culture.GetCultureForLanguage(Constants.Languages.Malay).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Croation).Name", "hr-HR", Culture.GetCultureForLanguage(Constants.Languages.Croation).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Lao).Name", "lo-LA", Culture.GetCultureForLanguage(Constants.Languages.Lao).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Macedonian).Name", "mk-MK", Culture.GetCultureForLanguage(Constants.Languages.Macedonian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Mongolian).Name", "mn-MN", Culture.GetCultureForLanguage(Constants.Languages.Mongolian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Serbian).Name", "sr-Cyrl-RS", Culture.GetCultureForLanguage(Constants.Languages.Serbian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Burmese).Name", "my-MM", Culture.GetCultureForLanguage(Constants.Languages.Burmese).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Slovenian).Name", "sl-SI", Culture.GetCultureForLanguage(Constants.Languages.Slovenian).Name);
			AssertEquals("provider.GetCultureForLanguage(Constants.Languages.Pashto).Name", "ps-AF", Culture.GetCultureForLanguage(Constants.Languages.Pashto).Name);
		}

		public void TestGetCultureForInvalidLanguage()
		{
			AssertNoExceptionThrown("Get culture for invalid language should not crash the system", () => { Culture.GetCultureForLanguage("XXXXX"); });

			var language = Factory.New<IRefLocalLanguage>();
			language.RA_Code = "#$%";
			language.RA_RN_NKCountryCode = "CN";
			language.RA_IsSystem = true;
			Factory.Save();

			AssertNotNull(LanguageHelper.GetCustomLanguageByLanguageCode(language.FullLanguageCode));
			var cultureName = Culture.GetCultureForLanguage(language.FullLanguageCode).Name;
			AssertEquals("Culture should be defaulted to english if identify is invalid", "en-US", cultureName);
			AssertEquals("Should have reported error for system defined language", $"{language.FullLanguageCode} is an invalid culture identifier.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLanguageCodeMappingShouldContainSameElementsAsDefaultLanguage()
		{
			AssertContainsExactElementsInAnyOrder("LanguageCodeMapping should stay same as DefaultLanguage", Culture.LanguageCodeMapping.Values, LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys);
		}

		public void TestCurrent()
		{
			AssertEquals("Maps to thread's current", Thread.CurrentThread.CurrentCulture, Culture.Current);
		}

		public void TestCurrentCompanyCountryCulture()
		{
			AssertEquals("Maps to current country's culture", EnvProxy.Instance.CurrentCompany.Country.Culture, Culture.CurrentCompanyCountryCulture);
		}

		public void TestCultureBeCacheByfCurrentCompanyCountryCulture()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;
			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("en-ZA")))
			{
				RawDataRegistry.Instance.NumberGroupSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, ",,");
				Assert(Culture.fCurrentCompanyCountryCulture == null);
				AssertEquals(",,", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator);
				Assert(Culture.fCurrentCompanyCountryCulture != null);
				AssertEquals("en-ZA", Culture.fCurrentCompanyCountryCulture.Name);
				AssertEquals(companyPK, Culture.historyCompanyPK);

				RawDataRegistry.Instance.NumberGroupSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, "*");
				Assert(Culture.fCurrentCompanyCountryCulture == null);
				AssertEquals("*", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator);
				Assert(Culture.fCurrentCompanyCountryCulture != null);
				AssertEquals("en-ZA", Culture.fCurrentCompanyCountryCulture.Name);
				AssertEquals(companyPK, Culture.historyCompanyPK);
			}

			using (EnvProxy.Instance.CurrentCompany.Country.SetCultureForTest(CultureInfo.CreateSpecificCulture("de-DE")))
			{
				Assert(Culture.fCurrentCompanyCountryCulture != null);
				AssertEquals("en-ZA", Culture.fCurrentCompanyCountryCulture.Name);
				AssertEquals(companyPK, Culture.historyCompanyPK);

				AssertEquals("*", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator);
				Assert(Culture.fCurrentCompanyCountryCulture != null);
				AssertEquals("de-DE", Culture.fCurrentCompanyCountryCulture.Name);
				AssertEquals(companyPK, Culture.historyCompanyPK);
			}
		}

		public void TestDefaultCulture()
		{
			CultureInfo currentCutlureInfo = Culture.Current;
			CultureInfo defaultCultureInfo = DefaultCulture.Instance;

			AssertEquals("Decimal point", defaultCultureInfo.NumberFormat.NumberDecimalSeparator, currentCutlureInfo.NumberFormat.NumberDecimalSeparator);
			AssertEquals("Decimal point", defaultCultureInfo.NumberFormat.CurrencyDecimalSeparator, currentCutlureInfo.NumberFormat.CurrencyDecimalSeparator);
			AssertEquals("Decimal point", defaultCultureInfo.NumberFormat.PercentDecimalSeparator, currentCutlureInfo.NumberFormat.PercentDecimalSeparator);

			AssertEquals("Group separator", defaultCultureInfo.NumberFormat.NumberGroupSeparator, currentCutlureInfo.NumberFormat.NumberGroupSeparator);
			AssertEquals("Group separator", defaultCultureInfo.NumberFormat.CurrencyGroupSeparator, currentCutlureInfo.NumberFormat.CurrencyGroupSeparator);
			AssertEquals("Group separator", defaultCultureInfo.NumberFormat.PercentGroupSeparator, currentCutlureInfo.NumberFormat.PercentGroupSeparator);

			AssertEquals("Currency symbol", defaultCultureInfo.NumberFormat.CurrencySymbol, currentCutlureInfo.NumberFormat.CurrencySymbol);
			AssertEquals("Positive sign", defaultCultureInfo.NumberFormat.PositiveSign, currentCutlureInfo.NumberFormat.PositiveSign);
			AssertEquals("Negative sign", defaultCultureInfo.NumberFormat.NegativeSign, currentCutlureInfo.NumberFormat.NegativeSign);

			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[0].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[0].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[1].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[1].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[2].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[2].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[3].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[3].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[4].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[4].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[5].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[5].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[6].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[6].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[7].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[7].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[8].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[8].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[9].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[9].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[10].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[10].ToUpper());
			AssertEquals("Date months", defaultCultureInfo.DateTimeFormat.AbbreviatedMonthNames[11].ToUpper(), currentCutlureInfo.DateTimeFormat.AbbreviatedMonthNames[11].ToUpper());
		}

		public void TestInvariantCulture()
		{
			AssertEquals("Invariant returned correctly", CultureInfo.InvariantCulture, Culture.Invariant);
		}

		public void TestCongoCulture()
		{
			var culture = Culture.GetCulture(Enterprise.Core.Constants.CountryCodes.DemocraticRepublicOfCongo);
			AssertEquals(",", culture.NumberFormat.CurrencyDecimalSeparator);
			AssertEquals(".", culture.NumberFormat.CurrencyGroupSeparator);
		}

		public void TestEstoniaCulture()
		{
			var culture = Culture.GetCulture(Enterprise.Core.Constants.CountryCodes.Estonia);
			AssertEquals(".", culture.NumberFormat.CurrencyDecimalSeparator);
			AssertEquals(",", culture.NumberFormat.CurrencyGroupSeparator);
		}

		public void TestCurrentCompanyCountryCultureWithRegistrySettings_NumberGroupSeparator()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;

			RawDataRegistry.Instance.NumberGroupSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, ",");
			AssertEquals(",", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator);

			RawDataRegistry.Instance.NumberGroupSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("*", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSeparator);
		}

		public void TestCurrentCompanyCountryCultureWithRegistrySettings_NumberDecimalSeparator()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;

			RawDataRegistry.Instance.NumberDecimalSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, ".");
			AssertEquals(".", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator);

			RawDataRegistry.Instance.NumberDecimalSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("*", Culture.CurrentCompanyCountryCulture.NumberFormat.NumberDecimalSeparator);
		}

		public void TestCurrentCompanyCountryCultureWithRegistrySettings_NumberGroupSizes()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;

			RawDataRegistry.Instance.NumberGroupSizes.SetValue(companyPK, Guid.Empty, Guid.Empty, "4");
			AssertEquals("[4]", JsonSerializer.Serialize(Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSizes));

			RawDataRegistry.Instance.NumberGroupSizes.SetValue(companyPK, Guid.Empty, Guid.Empty, "3,4");
			AssertEquals("[3,4]", JsonSerializer.Serialize(Culture.CurrentCompanyCountryCulture.NumberFormat.NumberGroupSizes));
		}

		public void TestCurrentCompanyCountryCultureWithRegistrySettings_CurrencyGroupSeparator()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;

			RawDataRegistry.Instance.CurrencyGroupSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, ",");
			AssertEquals(",", Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyGroupSeparator);

			RawDataRegistry.Instance.CurrencyGroupSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("*", Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyGroupSeparator);
		}

		public void TestCurrentCompanyCountryCultureWithRegistrySettings_CurrencyDecimalSeparator()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;

			RawDataRegistry.Instance.CurrencyDecimalSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, ".");
			AssertEquals(".", Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator);

			RawDataRegistry.Instance.CurrencyDecimalSeparator.SetValue(companyPK, Guid.Empty, Guid.Empty, "*");
			AssertEquals("*", Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyDecimalSeparator);
		}

		public void TestCurrentCompanyCountryCultureWithRegistrySettings_CurrencyGroupSizes()
		{
			var companyPK = EnvProxy.Instance.CurrentCompany.PK;

			RawDataRegistry.Instance.CurrencyGroupSizes.SetValue(companyPK, Guid.Empty, Guid.Empty, "4");
			AssertEquals("[4]", JsonSerializer.Serialize(Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyGroupSizes));

			RawDataRegistry.Instance.CurrencyGroupSizes.SetValue(companyPK, Guid.Empty, Guid.Empty, "3,4");
			AssertEquals("[3,4]", JsonSerializer.Serialize(Culture.CurrentCompanyCountryCulture.NumberFormat.CurrencyGroupSizes));
		}

		public void TestSet()
		{
			Culture.CultureChanged += new EventHandler<Culture.CultureChangedEventArgs>(Culture_CultureChanged);

			CultureInfo oldCulture = Culture.Current;

			try
			{
				AssertEquals(0, OldCultures.Count);
				AssertEquals(0, NewCultures.Count);

				CultureInfo canadianCulture = new CultureInfo("en-CA");
				Culture.Set(canadianCulture);

				AssertEquals(1, OldCultures.Count);
				AssertEquals(1, NewCultures.Count);
				AssertEquals("en-AU", OldCultures[0].Name);
				AssertEquals("en-CA", NewCultures[0].Name);

				AssertEquals("Set Current", Culture.Current, canadianCulture);
				AssertEquals("Set thread", Thread.CurrentThread.CurrentCulture, canadianCulture);

				Culture.Set(canadianCulture);
				AssertEquals(1, OldCultures.Count);
				AssertEquals(1, NewCultures.Count);

				Culture.Set(Culture.Invariant);
				AssertEquals("Set Current", Culture.Current, Culture.Invariant);
				AssertEquals("Set thread", Thread.CurrentThread.CurrentCulture, Culture.Invariant);

				AssertEquals(2, OldCultures.Count);
				AssertEquals(2, NewCultures.Count);
			}
			finally
			{
				Culture.Set(oldCulture);
			}
		}

		void Culture_CultureChanged(object sender, Culture.CultureChangedEventArgs e)
		{
			OldCultures.Add(e.OldCulture);
			NewCultures.Add(e.NewCulture);
		}

		readonly List<CultureInfo> OldCultures = new List<CultureInfo>();
		readonly List<CultureInfo> NewCultures = new List<CultureInfo>();
	}
}
