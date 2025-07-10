using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ResourceStrings.Business
{
	class TranslationFeedbackResourceStringsSource : ResourceStringSource
	{
		public static ResourceStringSource Create(string language)
		{
#if DEBUG
			if (DatabaseResourceStringSource.disabled)
			{
				return null;
			}
#endif
			return language == Res.DefaultLanguage ? null : new TranslationFeedbackResourceStringsSource(language);
		}

		public static Dictionary<string, ResourceStringSourceDataPair> CreateAll(string[] languages)
		{
#if DEBUG
			if (DatabaseResourceStringSource.disabled)
			{
				return null;
			}
#endif
			var res = new Dictionary<string, ResourceStringSourceDataPair>();
			var allData = new Dictionary<string, TranslationFeedbackResourceStringDataBuilder>();
			using (var cmd = Db.Connection.Command(
				@"select XQ_ResourceStringKey, XQ_ResourceStringLevel, XT_SuggestedTranslation, XT_Language
				  from dbo.StmTranslationFeedback
				  inner join dbo.StmTranslationFeedbackResource on XQ_XT = XT_PK
				  where " + (TranslationFeedbackConfiguration.IsMasterDatabase ? "XT_Status = @status1" : "XT_Status in (@status, @status1)") +
				$@" and XT_Language in ({string.Join(",", languages.Select(language => language = $"'{language}'"))})"))
			{
				cmd.AddParameterBasedOnDbColumn("@status1", "APR", StmTranslationFeedbackSchema.XT_Status);
				if (!TranslationFeedbackConfiguration.IsMasterDatabase)
				{
					cmd.AddParameterBasedOnDbColumn("@status", "NEW", StmTranslationFeedbackSchema.XT_Status);
				}
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var curLanguage = reader[StmTranslationFeedback.Schema.XT_Language].ToString();
						if (!allData.ContainsKey(curLanguage))
						{
							allData[curLanguage] = new TranslationFeedbackResourceStringDataBuilder(LanguageHelper.GetBaseSystemLanguage(curLanguage));
						}
						allData[curLanguage].Add((string)reader[StmTranslationFeedbackResource.Schema.XQ_ResourceStringKey], (string)reader[StmTranslationFeedbackResource.Schema.XQ_ResourceStringLevel], (string)reader[StmTranslationFeedback.Schema.XT_SuggestedTranslation]);
					}
				}
			}
			foreach (var language in languages)
			{
				res[language] = allData.ContainsKey(language)
					? new ResourceStringSourceDataPair(new TranslationFeedbackResourceStringsSource(language),
						allData[language].Data)
					: new ResourceStringSourceDataPair(new TranslationFeedbackResourceStringsSource(language),
						Enumerable.Empty<ResourceStringData>());
			}
			return res;
		}

		TranslationFeedbackResourceStringsSource(string language)
			: base(language)
		{
			this.baseSystemLanguage = LanguageHelper.GetBaseSystemLanguage(language);
		}

		public override IEnumerable<ResourceStringData> ReadAll()
		{
			using (var cmd = Db.Connection.Command(
				@"select XQ_ResourceStringKey, XQ_ResourceStringLevel, XT_SuggestedTranslation
				  from dbo.StmTranslationFeedback
				  inner join dbo.StmTranslationFeedbackResource on XQ_XT = XT_PK
				  where " + (TranslationFeedbackConfiguration.IsMasterDatabase ? "XT_Status = @status1" : "XT_Status in (@status, @status1)") +
				@" and XT_Language = @language"))
			{
				cmd.AddParameterBasedOnDbColumn("@status1", "APR", StmTranslationFeedbackSchema.XT_Status);
				if (!TranslationFeedbackConfiguration.IsMasterDatabase)
				{
					cmd.AddParameterBasedOnDbColumn("@status", "NEW", StmTranslationFeedbackSchema.XT_Status);
				}
				cmd.AddParameter("language", SqlDbType.VarChar, Language);
				using (var reader = cmd.ExecuteReader())
				{
					var builder = new TranslationFeedbackResourceStringDataBuilder(baseSystemLanguage);
					while (reader.Read())
					{
						builder.Add((string)reader[StmTranslationFeedbackResource.Schema.XQ_ResourceStringKey], (string)reader[StmTranslationFeedbackResource.Schema.XQ_ResourceStringLevel], (string)reader[StmTranslationFeedback.Schema.XT_SuggestedTranslation]);
					}

					return builder.Data;
				}
			}
		}

		public override void WriteAll(IEnumerable<ResourceStringData> resources)
		{
			throw new NotImplementedException();
		}

#if DEBUG
		public string BaseSystemLanguage => baseSystemLanguage;
#endif

		readonly string baseSystemLanguage;
	}
}
