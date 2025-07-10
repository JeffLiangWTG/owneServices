using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.ZArchitecture.Core
{
	public static class Culture
	{
		/// <summary>
		/// The current culture for the application
		/// </summary>
		public static CultureInfo Current
		{
			get { return Thread.CurrentThread.CurrentCulture; }
		}

		/// <summary>
		/// The culture for the current login company's country
		/// </summary>
		public static CultureInfo CurrentCompanyCountryCulture
		{
			get
			{
				// fallback is for DBUpgrader.Startup when current company is not yet set
				ICompany currentCompany = EnvProxy.Instance.CurrentCompany;
				if (currentCompany == null) { return Current; }
				var culture = currentCompany.Country.Culture;
				if (fCurrentCompanyCountryCulture != null && currentCompany.PK == historyCompanyPK && culture.Name ==  fCurrentCompanyCountryCulture.Name) { return fCurrentCompanyCountryCulture; }

				culture.NumberFormat.NumberGroupSeparator = RawDataRegistry.Instance.NumberGroupSeparator.GetValueWithoutFallback(currentCompany.PK, Guid.Empty, Guid.Empty);
				culture.NumberFormat.NumberDecimalSeparator = RawDataRegistry.Instance.NumberDecimalSeparator.GetValueWithoutFallback(currentCompany.PK, Guid.Empty, Guid.Empty);
				culture.NumberFormat.NumberGroupSizes = JsonSerializer.Deserialize<int[]>($"[{RawDataRegistry.Instance.NumberGroupSizes.GetValueWithoutFallback(currentCompany.PK, Guid.Empty, Guid.Empty)}]");
				culture.NumberFormat.CurrencyGroupSeparator = RawDataRegistry.Instance.CurrencyGroupSeparator.GetValueWithoutFallback(currentCompany.PK, Guid.Empty, Guid.Empty);
				culture.NumberFormat.CurrencyDecimalSeparator = RawDataRegistry.Instance.CurrencyDecimalSeparator.GetValueWithoutFallback(currentCompany.PK, Guid.Empty, Guid.Empty);
				culture.NumberFormat.CurrencyGroupSizes = JsonSerializer.Deserialize<int[]>($"[{RawDataRegistry.Instance.CurrencyGroupSizes.GetValueWithoutFallback(currentCompany.PK, Guid.Empty, Guid.Empty)}]");
				fCurrentCompanyCountryCulture = culture;
				historyCompanyPK = currentCompany.PK;

				return culture;
			}
		}
		//TODO: You can technically have two different threads be on different companies (see UserContextSecurity), so ThreadStatic instead of ThreadSafe?
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Just a cache - no errors will occur if it's nulled just before or after an access")]
		[ThreadStatic]
		public static CultureInfo fCurrentCompanyCountryCulture;

		[ThreadStatic]
		public static Guid historyCompanyPK;

		/// <summary>
		/// Microsoft's culture tied to no particular country.
		/// </summary>
		public static CultureInfo Invariant
		{
			get { return CultureInfo.InvariantCulture; }
		}

		public static CultureInfo Default
		{
			get { return DefaultCulture.Instance; }
		}

		#region Set Culture

		/// <summary>
		/// Sets the current culture for application process. 
		/// </summary>
		public static void Set(CultureInfo culture)
		{
			if (Current != culture)
			{
				if (CultureChanged != null)
				{
					CultureChanged(null, new CultureChangedEventArgs(Current, culture));
				}

				Thread.CurrentThread.CurrentCulture = culture;
			}
		}

		public static IDisposable SetTemporarily(CultureInfo culture)
		{
			var originalCulture = Current;
			Set(culture);
			return new DisposableAction(delegate
			{
				Set(originalCulture);
			});
		}

		internal static Calendar CurrentCalendar => Thread.CurrentThread.CurrentCulture.Calendar;

		public static void SetCalendar(Calendar calendar)
		{
			if (CurrentCalendar != calendar)
			{
				CultureInfo.CurrentCulture.DateTimeFormat.Calendar = calendar;
			}
		}

		public static IDisposable SetTemporarilyCalendar(Calendar calendar)
		{
			var originalCalendar = Thread.CurrentThread.CurrentCulture.Calendar;
			SetCalendar(calendar);
			return new DisposableAction(delegate
			{
				SetCalendar(originalCalendar);
			});
		}

		public static event EventHandler<CultureChangedEventArgs> CultureChanged;

		public sealed class CultureChangedEventArgs : EventArgs
		{
			public CultureChangedEventArgs(CultureInfo oldCulture, CultureInfo newCulture)
			{
				OldCulture = oldCulture;
				NewCulture = newCulture;
			}

			public readonly CultureInfo OldCulture;
			public readonly CultureInfo NewCulture;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch statement")]
		public static Calendar GetCalendar(string calendar)
		{
			Calendar result = null;
			switch (calendar)
			{
				case "ChineseLunisolarCalendar":
					result = new ChineseLunisolarCalendar();
					break;
				case "GregorianCalendar":
					result = new GregorianCalendar();
					break;
				case "HebrewCalendar":
					result = new HebrewCalendar();
					break;
				case "HijriCalendar":
					result = new HijriCalendar();
					break;
				case "JapaneseCalendar":
					result = new JapaneseCalendar();
					break;
				case "JapaneseLunisolarCalendar":
					result = new JapaneseLunisolarCalendar();
					break;
				case "JulianCalendar":
					result = new JulianCalendar();
					break;
				case "KoreanCalendar":
					result = new KoreanCalendar();
					break;
				case "KoreanLunisolarCalendar":
					result = new KoreanLunisolarCalendar();
					break;
				case "PersianCalendar":
					result = new PersianCalendar();
					break;
				case "TaiwanCalendar":
					result = new TaiwanCalendar();
					break;
				case "TaiwanLunisolarCalendar":
					result = new TaiwanLunisolarCalendar();
					break;
				case "ThaiBuddhistCalendar":
					result = new ThaiBuddhistCalendar();
					break;
				case "UmAlQuraCalendar":
					result = new UmAlQuraCalendar();
					break;
				default:
					result = CultureInfo.InvariantCulture.Calendar;
					break;
			}

			return result;
		}

		#endregion

		public static CultureInfo GetCulture(string countryCode) => CultureInfoHelper.GetCulture(countryCode);

		public static CultureInfo GetCultureForLanguage(string languageCode)
		{
			var result = CultureInfoHelper.GetCultureForLanguage(languageCode);

			if (result == null)
			{
				result = new CultureInfo("en-US");
				var language = LanguageHelper.GetCustomLanguageByLanguageCode(languageCode);
				if (language != null && language.RA_IsSystem && languageCode != Constants.Languages.Dzongkha)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"{languageCode} is an invalid culture identifier."));
				}
				else if (LanguageCodeMapping.ContainsKey(languageCode))
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($@"{languageCode} is an old culture code and should be updated. 
Current system language: {Res.CurrentLanguage}
Current English Spelling value: {RawDataRegistry.Instance.EnglishSpelling.Value}"));
					result = GetCultureForLanguage(LanguageCodeMapping[languageCode]);
				}
			}

			return result;
		}

		[ThreadSafe]
		public readonly static Dictionary<string, string> LanguageCodeMapping = new Dictionary<string, string>()
		{
			{ "ENG", "EN" }, { "AFK", "AF-ZA" }, { "ALB", "SQ-AL" }, { "ARB", "AR-AE" }, { "ARM", "HY-AM" },
			{ "AZR", "AZ-AZ" }, { "BSQ", "EU-ES" }, { "BLR", "BE-BY" }, { "BLG", "BG-BG" }, { "BEN", "BN-BD" },
			{ "BOS", "BS-BA" }, { "BUR", "MY-MM" }, { "CAT", "CA-ES" }, { "CHS", "ZH-CN" }, { "CHT", "ZH-TW" },
			{ "CRT", "HR-HR" }, { "CZE", "CS-CZ" }, { "DAN", "DA-DK" }, { "DIV", "DV-MV" }, { "DCH", "NL-NL" },
			{ "DZO", "DZ-BT" }, { "EUS", "EN-US" }, { "EGB", "EN-GB" }, { "EST", "ET-EE" }, { "FAE", "FO-FO" },
			{ "FRS", "FA-IR" }, { "FIN", "FI-FI" }, { "FRN", "FR-FR" }, { "GAL", "GL-ES" }, { "GRG", "KA-GE" },
			{ "GRM", "DE-DE" }, { "GRK", "EL-GR" }, { "GUJ", "GU-IN" }, { "HBW", "HE-IL" }, { "HND", "HI-IN" },
			{ "HUN", "HU-HU" }, { "ICE", "IS-IS" }, { "IND", "ID-ID" }, { "ITL", "IT-IT" }, { "JPN", "JA-JP" },
			{ "KAN", "KN-IN" }, { "KAZ", "KK-KZ" }, { "KHM", "KM-KH" }, { "KNK", "KOK-IN" }, { "KOR", "KO-KR" },
			{ "KYR", "KY-KG" }, { "LAO", "LO-LA" }, { "LTV", "LV-LV" }, { "LTH", "LT-LT" }, { "MAC", "MK-MK" },
			{ "MAL", "MS-MY" }, { "MAR", "MR-IN" }, { "MNG", "MN-MN" }, { "NOR", "NB-NO" }, { "PUS", "PS-AF" },
			{ "POL", "PL-PL" }, { "PRT", "PT-PT" }, { "PBR", "PT-BR" }, { "PJB", "PA-IN" }, { "ROM", "RO-RO" },
			{ "RSN", "RU-RU" }, { "SAN", "SA-IN" }, { "SER", "SR-RS" }, { "SLK", "SK-SK" }, { "SLN", "SL-SI" },
			{ "SLA", "ES-LA" }, { "SPN", "ES-ES" }, { "SWA", "SW-KE" }, { "SWE", "SV-SE" }, { "SYR", "SYR-SY" },
			{ "TAM", "TA-IN" }, { "TAT", "TT-RU" }, { "TEL", "TE-IN" }, { "TRK", "TR-TR" }, { "THA", "TH-TH" },
			{ "UKR", "UK-UA" }, { "URD", "UR-PK" }, { "UZB", "UZ-UZ" }, { "VTN", "VI-VN" }
		};

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ISO language code, ISO language name")]
		public static string GetLanguageForCulture(CultureInfo culture)
		{
			string language;
			if (culture.Name == "zh-CN")
			{
				language = Enterprise.Core.Constants.Languages.ChineseSimplified;
			}
			else if (culture.Name.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
			{
				language = Enterprise.Core.Constants.Languages.ChineseTraditional;
			}
			else if (culture.Name == "nn-NO" || culture.Name == "nb-NO")
			{
				language = Enterprise.Core.Constants.Languages.Norwegian;
			}
			else if (culture.Name == "pt-BR")
			{
				language = Enterprise.Core.Constants.Languages.PortugueseBrazil;
			}
			else if (culture.TwoLetterISOLanguageName == "en")
			{
				language = culture.IsNeutralCulture ? Constants.Languages.English : GetDefaultEnglishSpellingForCountry(new RegionInfo(culture.Name).TwoLetterISORegionName);
			}
			else if (culture.Name == "hr-HR")
			{
				language = Enterprise.Core.Constants.Languages.Croation;
			}
			else if (culture.Name == "es-ES")
			{
				language = Enterprise.Core.Constants.Languages.Spanish;
			}
			else if (culture.Name == "es-LA")
			{
				language = Enterprise.Core.Constants.Languages.SpanishLatin;
			}
			else
			{
				string languageName = culture.EnglishName.IndexOf('(') > -1
					? culture.EnglishName.Substring(0, culture.EnglishName.IndexOf('(')).Trim()
					: culture.EnglishName;
				var languageField = typeof(Enterprise.Core.SharedConstants.Languages).GetField(languageName)
					?? throw new CultureNotFoundException(nameof(culture), "Culture is not supported");
				language = (string)languageField.GetValue(null);
			}
			return language;
		}

		public static string GetDefaultEnglishSpellingForCountry(string countryCode)
		{
			switch (countryCode)
			{
				case Constants.CountryCodes.AntiguaAndBarbuda:
				case Constants.CountryCodes.Australia:
				case Constants.CountryCodes.Bahamas:
				case Constants.CountryCodes.Bangladesh:
				case Constants.CountryCodes.Barbados:
				case Constants.CountryCodes.Belize:
				case Constants.CountryCodes.Botswana:
				case Constants.CountryCodes.Brunei:
				case Constants.CountryCodes.Cameroon:
				case Constants.CountryCodes.Cyprus:
				case Constants.CountryCodes.Dominica:
				case Constants.CountryCodes.Ghana:
				case Constants.CountryCodes.Grenada:
				case Constants.CountryCodes.Guyana:
				case Constants.CountryCodes.HongKong:
				case Constants.CountryCodes.India:
				case Constants.CountryCodes.Ireland:
				case Constants.CountryCodes.Jamaica:
				case Constants.CountryCodes.Kenya:
				case Constants.CountryCodes.Kiribati:
				case Constants.CountryCodes.Lesotho:
				case Constants.CountryCodes.Malawi:
				case Constants.CountryCodes.Malaysia:
				case Constants.CountryCodes.Maldives:
				case Constants.CountryCodes.Malta:
				case Constants.CountryCodes.Mauritius:
				case Constants.CountryCodes.Mozambique:
				case Constants.CountryCodes.Namibia:
				case Constants.CountryCodes.Nauru:
				case Constants.CountryCodes.NewZealand:
				case Constants.CountryCodes.Nigeria:
				case Constants.CountryCodes.Pakistan:
				case Constants.CountryCodes.PapuaNewGuinea:
				case Constants.CountryCodes.Rwanda:
				case Constants.CountryCodes.SaintKittsAndNevis:
				case Constants.CountryCodes.SaintLucia:
				case Constants.CountryCodes.SaintVincentAndTheGrenadin:
				case Constants.CountryCodes.WesternSamoa:
				case Constants.CountryCodes.Seychelles:
				case Constants.CountryCodes.SierraLeone:
				case Constants.CountryCodes.Singapore:
				case Constants.CountryCodes.SolomonIslands:
				case Constants.CountryCodes.SouthAfrica:
				case Constants.CountryCodes.SriLanka:
				case Constants.CountryCodes.Swaziland:
				case Constants.CountryCodes.Tanzania:
				case Constants.CountryCodes.Gambia:
				case Constants.CountryCodes.Tonga:
				case Constants.CountryCodes.TrinidadAndTobago:
				case Constants.CountryCodes.Tuvalu:
				case Constants.CountryCodes.Uganda:
				case Constants.CountryCodes.UnitedKingdom:
				case Constants.CountryCodes.Vanuatu:
				case Constants.CountryCodes.Zambia:
					return Constants.Languages.EnglishBritish;

				default:
					return Constants.Languages.EnglishAmerican;
			}
		}
	}
}
