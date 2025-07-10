using System;
using System.Collections.Generic;
using CargoWise.ResourceStrings.Cache;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ResourceStrings.Business
{
	public class TranslatableDataTransformHelper : ITranslatableDataTransformHelper
	{
		public void AddResourceString(ResourceStringData data, string language)
		{
			if (!string.IsNullOrEmpty(language) && DataFile.IsLanguageFileExists(language))
			{
				var existing = ResourceStringsFactory.LookupWithLanguageFallback(language, data.Key);
				if (existing == null || !data.ContentEquals(existing.ToResourceStringData()))
				{
					if (update == null)
					{
						update = new List<HelpDataString>();
					}

					var item = HelpDataString.CreateFromResourceStringData(data, language);
					item.HD_Language = language;
					update.Add(item);
				}
			}
		}

		public void DeleteResourceString(string key, string language)
		{
			if (!string.IsNullOrEmpty(language) && DataFile.IsLanguageFileExists(language))
			{
				var existing = ResourceStringsFactory.Lookup(language, key, true);
				if (existing != null)
				{
					if (update == null)
					{
						update = new List<HelpDataString>();
					}
					existing.Delete();
					update.Add(existing);
				}
			}
		}

		public void SaveChanges()
		{
#if DEBUG
			// Do not save strings on developer machines as this will check out strings to resource delta files
			if (!Globals.IsTest)
			{
				return;
			}
#endif
			if (update != null)
			{
				ResourceStringsFactory.Save(EditReasons.Codes.CustomizableDataTranslation, update.ToArray());
			}
		}

#if DEBUG
		public IDisposable MockResourceStringSources()
		{
			return ResourceStringsFactory.MockSources();
		}

		public IMockResourceStringCache GetMockSource(string language)
		{
			return ResourceStringsFactory.GetMockSource(language);
		}

		internal static ResourceString realTestString { get { return ResString.GetMultilingualString("2d5e3407-46a2-479f-9a4e-432cfd4ea2da", "Test String"); } }
#endif

		List<HelpDataString> update;
	}
}
