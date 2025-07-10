using System;
using System.Collections.Generic;
using System.Linq;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ResourceStrings.Business
{
	class TranslationFeedbackResourceStringDataBuilder
	{
		public TranslationFeedbackResourceStringDataBuilder(string language)
		{
			trnSystem = new Lazy<ISimpleResourceStringCache>(() => ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(language));
		}

		public void Add(string key, string level, string value, string originalSegment = null)
		{
			if (!resourceStrings.TryGetValue(key, out var data))
			{
				data = trnSystem.Value.Get(key);
				if (data == null)
				{
					data = engSystem.Value.Get(key);
				}
			}
			if (data != null)
			{
				if (string.IsNullOrEmpty(value))
				{
					var eng = engSystem.Value.Get(key);
					if (eng == null)
					{
						return;
					}
					value = HelpDataString.CreateFromResourceStringData(eng, Res.DefaultLanguage).GetCaptionAtLevel(level);
				}
				switch (level)
				{
					case ResourceStringDataLevels.Codes.Caption:
						data = new ResourceStringData(data.Key, data.ShortCaption, data.MediumCaption, Replace(data.Caption, originalSegment, value), data.FullDescription, editReason: EditReasons.Codes.TranslationFeedback);
						break;
					case ResourceStringDataLevels.Codes.ShortCaption:
						data = new ResourceStringData(data.Key, Replace(data.ShortCaption, originalSegment, value), data.MediumCaption, data.Caption, data.FullDescription, editReason: EditReasons.Codes.TranslationFeedback);
						break;
					case ResourceStringDataLevels.Codes.MediumCaption:
						data = new ResourceStringData(data.Key, data.ShortCaption, Replace(data.MediumCaption, originalSegment, value), data.Caption, data.FullDescription, editReason: EditReasons.Codes.TranslationFeedback);
						break;
					case ResourceStringDataLevels.Codes.FullDescription:
						data = new ResourceStringData(data.Key, data.ShortCaption, data.MediumCaption, data.Caption, Replace(data.FullDescription, originalSegment, value), editReason: EditReasons.Codes.TranslationFeedback);
						break;
				}

				resourceStrings[data.Key] = data;
			}
		}

		public ResourceStringData[] Data => resourceStrings.Values.ToArray();

		string Replace(string paragraph, string originalSegment, string value)
		{
			if (originalSegment == null)
			{
				return value;
			}
			else
			{
				if (Segmentation.Segment(originalSegment).Length > 1)
				{
					return paragraph.Replace(originalSegment, value);
				}
				else
				{
					return Segmentation.ReplaceSegment(paragraph, originalSegment, value);
				}
			}
		}

		readonly Lazy<ISimpleResourceStringCache> trnSystem;
		readonly Lazy<ISimpleResourceStringCache> engSystem = new Lazy<ISimpleResourceStringCache>(() => ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage));
		readonly Dictionary<string, ResourceStringData> resourceStrings = new Dictionary<string, ResourceStringData>();
	}
}
