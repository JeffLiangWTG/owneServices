#if DEBUG
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.TranslationMemoryExchange;

namespace Enterprise.ResourceStrings.Business
{
	public static class TranslationFeedbackContentAdapter
	{
		public static void Export(string language, string targetDirectory)
		{
			var cw1Language = Culture.GetLanguageForCulture(CultureInfo.GetCultureInfo(language));
			var query = new ZQuery(StmTranslationFeedbackSchema.XT_Language, cw1Language);
			query.AddToFilter(StmTranslationFeedbackSchema.XT_Status, TranslationFeedbackStatusList.Codes.Approved);
			query.AddToFilter(StmTranslationFeedbackSchema.XT_Application, TranslationFeedbackApplicationsList.Codes.Cargowise);
			var feedbacks = new BusinessObjectFactory().Load<StmTranslationFeedback>(query);
			if (feedbacks.Any())
			{
				using (var writer = new TmxWriter(
					Path.Combine(targetDirectory, TranslationFeedbackFileName),
					TranslationFeedbackApplicationsList.Codes.Cargowise,
					Core.Constants.ProductName,
					ReleaseInfo.Instance.VersionNumber.ToString()))
				{
					foreach (var feedback in feedbacks)
					{
						var contexts = feedback.SavedContexts.Cast<StmTranslationFeedbackResource>().Select(c => c.XQ_ResourceStringLevel + ":" + c.XQ_ResourceStringKey).ToArray();
						var sourceSegments = Segmentation.Segment(feedback.XT_Source);
						if (sourceSegments.Length == 1)
						{
							writer.Write(new TmxTu(contexts, language, feedback.XT_Source, feedback.XT_OriginalTranslation, feedback.XT_SuggestedTranslation));
						}
						else
						{
							var originalTranslationSegments = Segmentation.Segment(feedback.XT_OriginalTranslation);
							var suggestedTranslationSegments = Segmentation.Segment(feedback.XT_SuggestedTranslation);
							if (sourceSegments.Length == originalTranslationSegments.Length && sourceSegments.Length == suggestedTranslationSegments.Length)
							{
								for (int i = 0; i < sourceSegments.Length; i++)
								{
									writer.Write(new TmxTu(contexts, language, sourceSegments[i].Text, originalTranslationSegments[i].Text, suggestedTranslationSegments[i].Text));
								}
							}
							else
							{
								writer.Write(new TmxTu(contexts, language, feedback.XT_Source, feedback.XT_OriginalTranslation, feedback.XT_SuggestedTranslation));
							}
						}
					}
				}
			}
		}

		public static void Import(string language, string contentDirectory)
		{
			var cw1Language = Culture.GetLanguageForCulture(CultureInfo.GetCultureInfo(language));
			var builder = new TranslationFeedbackResourceStringDataBuilder(cw1Language);
			var translationFeedbackFilePath = Path.Combine(contentDirectory, TranslationFeedbackFileName);
			if (!File.Exists(translationFeedbackFilePath))
			{
				return;
			}
			foreach (var tu in TmxReader.Read(translationFeedbackFilePath, TranslationFeedbackApplicationsList.Codes.Cargowise))
			{
				foreach (var context in tu.ResourceStringContexts)
				{
					var splitIndex = context.IndexOf(':');
					if (splitIndex > -1)
					{
						var level = context.Substring(0, splitIndex);
						var key = context.Substring(splitIndex + 1);
						builder.Add(key, level, tu.NewTranslation, tu.OriginalTranslation);
					}
				}
			}

			ResourceStringsFactory.Save(EditReasons.Codes.TradosImport, builder.Data.Select(d => HelpDataString.CreateFromResourceStringData(d, cw1Language)).ToArray());
		}

		const string TranslationFeedbackFileName = "TranslationFeedback.tmx";
	}
}
#endif
