using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core
{
	public sealed class LanguageHelper
	{
		LanguageHelper() { }

		public static string GetBaseSystemLanguage(string language)
		{
			if (DataFile.IsLanguageFileExists(language))
			{
				return language;
			}

			var baseLanguage = new BusinessObjectFactory().LoadTop1<IRefLocalLanguage>(GetSearchByLanguageFullCodeQuery(language));

			var baseLanguageCode = Res.DefaultLanguage;
			if (baseLanguage != null)
			{
				while (baseLanguage.ParentLanguage != null)
				{
					baseLanguage = baseLanguage.ParentLanguage;
				}
				if (baseLanguage.RA_IsSystem)
				{
					baseLanguageCode = baseLanguage.FullLanguageCode;
				}
			}
			return baseLanguageCode;
		}

		public static IRefLocalLanguage[] GetAllActiveLanguages()
		{
			using (Db.DisposableActionForDbConnection())
			{
				return new BusinessObjectFactory().Load<IRefLocalLanguage>(new ZQuery(RefLocalLanguageSchema.RA_IsActive, true));
			}
		}

		public static ICollection<ISecurityLanguageReference> GetSecurityLanguageReferences()
		{
			if (MemoryCache.Default.Get(nameof(GetSecurityLanguageReferences)) is ICollection<ISecurityLanguageReference> cachedResult)
			{
				return cachedResult;
			}
			else
			{
				var result = new List<ISecurityLanguageReference>();
				foreach (var language in GetAllActiveLanguages())
				{
					if (!Res.IsSystemDefinedEnglish(language.FullLanguageCode))
					{
						result.Add(new SecurityLanguageReference(language));
					}
				}
				MemoryCache.Default.Set(nameof(GetSecurityLanguageReferences), result, new CacheItemPolicy { AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(1) });
				return result;
			}
		}

		[Immutable]
		class SecurityLanguageReference : ISecurityLanguageReference
		{
			public SecurityLanguageReference(IRefLocalLanguage language)
			{
				FullLanguageCode = language.FullLanguageCode;
				Description = language.RA_DescriptionMultilingual;
			}

			public ZString FullLanguageCode { get; }
			public MultilingualString Description { get; }
		}

		public static IRefLocalLanguage GetCustomLanguageByFullLanguageCodeOrDescription(string language, BusinessObjectFactory factory)
		{
			var query = new ZQuery(RefLocalLanguageSchema.RA_IsActive, true).AddToFilter(RefLocalLanguageSchema.RA_IsSystem, false);
			var conditionQuery = new ZQuery();

			if (language.Length >= 4 && language[language.Length - 3] == '-')
			{
				conditionQuery.AddToFilter(new ZQuery(RefLocalLanguageSchema.RA_Code, language.Substring(0, language.Length - 3)).AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, language.Substring(language.Length - 2)), JoinCondition.Or);
			}
			else
			{
				conditionQuery.AddToFilter(new ZQuery(RefLocalLanguageSchema.RA_Code, language).AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, ""), JoinCondition.Or);
			}

			conditionQuery.AddToFilter(new ZQuery(RefLocalLanguageSchema.RA_Description, language), JoinCondition.Or);

			query.AddToFilter(conditionQuery);
			return factory.LoadTop1<IRefLocalLanguage>(query);
		}

		public static IRefLocalLanguage GetCustomLanguageByLanguageCode(string language)
		{
			if (!string.IsNullOrEmpty(language))
			{
				return new BusinessObjectFactory().LoadTop1<IRefLocalLanguage>(GetSearchByLanguageFullCodeQuery(language));
			}
			else
			{
				return null;
			}
		}

		public static Dictionary<string, MultilingualString> GetDefaultLanguageForOLookUpEditType() => new Dictionary<string, MultilingualString>
		{
			{ Constants.Languages.English, SourceGenerated.ResString.GetMultilingualString("Common|Languages|English", "English") },
			{ Constants.Languages.EnglishAmerican, SourceGenerated.ResString.GetMultilingualString("Common|Languages|EnglishUS", "English (American)") },
			{ Constants.Languages.EnglishBritish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|EnglishGB", "English (British)") },
			{ Constants.Languages.ChineseSimplified, SourceGenerated.ResString.GetMultilingualString("Common|Languages|ChineseSimplified", "Chinese - Simplified") },
			{ Constants.Languages.ChineseTraditional, SourceGenerated.ResString.GetMultilingualString("Common|Languages|ChineseTraditional", "Chinese - Traditional") },
			{ Constants.Languages.Afrikaans, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Afrikaans", "Afrikaans") },
			{ Constants.Languages.Albanian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Albanian", "Albanian") },
			{ Constants.Languages.Arabic, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Arabic", "Arabic") },
			{ Constants.Languages.Armenian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Armenian", "Armenian") },
			{ Constants.Languages.Azerbaijani, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Azerbaijan", "Azerbaijani") },
			{ Constants.Languages.Bangla, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Bangla", "Bangla") },
			{ Constants.Languages.Basque, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Basque", "Basque") },
			{ Constants.Languages.Belarusian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Belarusian", "Belarusian") },
			{ Constants.Languages.Bulgarian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Bulgarian", "Bulgarian") },
			{ Constants.Languages.Burmese, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Burmese", "Burmese") },
			{ Constants.Languages.Bosnian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Bosnian", "Bosnian") },
			{ Constants.Languages.Catalan, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Catalan", "Catalan") },
			{ Constants.Languages.Croation, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Croation", "Croatian") },
			{ Constants.Languages.Czech, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Czech", "Czech") },
			{ Constants.Languages.Danish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Danish", "Danish") },
			{ Constants.Languages.Divehi, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Divehi", "Dhivehi") },
			{ Constants.Languages.Dutch, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Dutch", "Dutch") },
			{ Constants.Languages.Dzongkha, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Dzongkha", "Dzongkha") },
			{ Constants.Languages.Estonian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Estonian", "Estonian") },
			{ Constants.Languages.Faeroese, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Faeroese", "Faeroese") },
			{ Constants.Languages.Farsi, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Farsi", "Farsi") },
			{ Constants.Languages.Finnish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Finnish", "Finnish") },
			{ Constants.Languages.French, SourceGenerated.ResString.GetMultilingualString("Common|Languages|French", "French") },
			{ Constants.Languages.Galician, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Galician", "Galician") },
			{ Constants.Languages.Georgian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Georgian", "Georgian") },
			{ Constants.Languages.German, SourceGenerated.ResString.GetMultilingualString("Common|Languages|German", "German") },
			{ Constants.Languages.Greek, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Greek", "Greek") },
			{ Constants.Languages.Gujarati, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Gujarati", "Gujarati") },
			{ Constants.Languages.Hebrew, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Hebrew", "Hebrew") },
			{ Constants.Languages.Hindi, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Hindi", "Hindi") },
			{ Constants.Languages.Hungarian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Hungarian", "Hungarian") },
			{ Constants.Languages.Icelandic, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Icelandic", "Icelandic") },
			{ Constants.Languages.Indonesian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Indonesian", "Bahasa Indonesia") },
			{ Constants.Languages.Italian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Italian", "Italian") },
			{ Constants.Languages.Japanese, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Japanese", "Japanese") },
			{ Constants.Languages.Kannada, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Kannada", "Kannada") },
			{ Constants.Languages.Kazakh, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Kazakh", "Kazakh") },
			{ Constants.Languages.Khmer, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Khmer", "Khmer") },
			{ Constants.Languages.Konkani, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Konkani", "Konkani") },
			{ Constants.Languages.Korean, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Korean", "Korean") },
			{ Constants.Languages.Kyrgyz, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Kyrgyz", "Kyrgyz") },
			{ Constants.Languages.Lao, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Lao", "Lao") },
			{ Constants.Languages.Latvian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Latvian", "Latvian") },
			{ Constants.Languages.Lithuanian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Lithuanian", "Lithuanian") },
			{ Constants.Languages.Macedonian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Macedonian", "Macedonian") },
			{ Constants.Languages.Malay, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Malay", "Malay") },
			{ Constants.Languages.Marathi, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Marathi", "Marathi") },
			{ Constants.Languages.Mongolian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Mongolian", "Mongolian") },
			{ Constants.Languages.Norwegian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Norwegian", "Norwegian") },
			{ Constants.Languages.Pashto, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Pashto", "Pashto") },
			{ Constants.Languages.Polish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Polish", "Polish") },
			{ Constants.Languages.Portuguese, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Portuguese", "Portuguese") },
			{ Constants.Languages.PortugueseBrazil, SourceGenerated.ResString.GetMultilingualString("Common|Languages|PortugueseBrazil", "Portuguese - Brazil") },
			{ Constants.Languages.Punjabi, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Punjabi", "Punjabi") },
			{ Constants.Languages.Romanian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Romanian", "Romanian") },
			{ Constants.Languages.Russian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Russian", "Russian") },
			{ Constants.Languages.Sanskrit, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Sanskrit", "Sanskrit") },
			{ Constants.Languages.Serbian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Serbian", "Serbian") },
			{ Constants.Languages.Slovak, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Slovak", "Slovak") },
			{ Constants.Languages.Slovenian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Slovenian", "Slovenian") },
			{ Constants.Languages.Spanish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Spanish", "Spanish") },
			{ Constants.Languages.SpanishLatin, SourceGenerated.ResString.GetMultilingualString("Common|Languages|SpanishSpanishLatin", "Spanish - Latin") },
			{ Constants.Languages.Swahili, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Swahili", "Swahili") },
			{ Constants.Languages.Swedish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Swedish", "Swedish") },
			{ Constants.Languages.Syriac, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Syriac", "Syriac") },
			{ Constants.Languages.Tamil, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Tamil", "Tamil") },
			{ Constants.Languages.Tatar, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Tatar", "Tatar") },
			{ Constants.Languages.Telugu, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Telugu", "Telugu") },
			{ Constants.Languages.Turkish, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Turkish", "Turkish") },
			{ Constants.Languages.Thai, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Thai", "Thai") },
			{ Constants.Languages.Ukrainian, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Ukranian", "Ukrainian") },
			{ Constants.Languages.Urdu, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Urdu", "Urdu") },
			{ Constants.Languages.Uzbek, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Uzbek", "Uzbek") },
			{ Constants.Languages.Vietnamese, SourceGenerated.ResString.GetMultilingualString("Common|Languages|Vietnamese", "Vietnamese") }
		};

		public static bool ActiveLanguageExistsForOLookUpEditType(string languageFullCode)
		{
			var result = GetDefaultLanguageForOLookUpEditType().ContainsKey(languageFullCode);
			if (!result && !string.IsNullOrEmpty(languageFullCode))
			{
				var filter = GetSearchByLanguageFullCodeQuery(languageFullCode);
				filter.AddToFilter(new ZQuery(RefLocalLanguageSchema.RA_IsActive, true));
				result = new ReadOnlyBusinessObjectFactory().ExistsInDatabase(RefLocalLanguageSchema.Constants.TableName, filter);
			}

			return result;
		}

		static ZQuery GetSearchByLanguageFullCodeQuery(string language)
		{
			var languageAndCountryCode = language.Split('-');
			var query = new ZQuery(RefLocalLanguageSchema.RA_Code, languageAndCountryCode[0]);
			if (languageAndCountryCode.Length > 1)
			{
				query.AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, languageAndCountryCode[1]);
			}
			else
			{
				query.AddToFilter(RefLocalLanguageSchema.RA_RN_NKCountryCode, "");
			}
			return query;
		}
	}
}
