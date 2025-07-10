using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Cache;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.ResourceStrings.Business
{
	public class CustomizableDataTranslationPage : NonPersistentBusinessObject
	{
		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public CustomizableDataTranslationPage(CustomizableDataResourceStrings customizable, ResourceString initialValue, object context)
		{
			Customizable = customizable;
			CheckSecurity();

			var allLanguages = new HashSet<string>();
			var languages = LanguageHelper.GetAllActiveLanguages();
			foreach (var language in languages)
			{
				allLanguages.Add(language.FullLanguageCode);
			}
			var allEntries = new CustomizableDataTranslationCollection();
			using (Res.HoldLanguageInstances())
			{
				Res.PreBuildCaches(languages.Select(lan => lan.GetResourceLanguage()).ToArray());
				allEntries.AddRange(customizable.Source.GetRuntimeCaptions(initialValue, context).SelectMany(caption => CreateEntries((ResourceString)caption, allLanguages)));
				RegisterEditableChildObject(allEntries);

				all = allEntries;
				allTranslationsOfCurrentValue = new CustomizableDataTranslationCollection();
				allValuesInCurrentLanguage = new CustomizableDataTranslationCollection();

				CurrentLanguage = Res.IsSystemDefinedEnglish(Res.CurrentLanguage) ? allLanguages.First() : Res.CurrentLanguage;
				if (initialValue != null)
				{
					CurrentCaption = initialValue;
				}
				else if (all.Count > 0)
				{
					CurrentCaption = all[0].Caption;
				}
			}
		}

		void CheckSecurity()
		{
			var translatable = Customizable.Source as TranslatableDataFieldAttribute;
			if (translatable != null)
			{
				if (!string.IsNullOrEmpty(translatable.SecurityCheckpoint))
				{
					var checkpoint = Env.Security.FindCheckPoint(translatable.SecurityCheckpoint);
					ReadOnly = !checkpoint.IsAllowed;
				}
			}
		}

		IEnumerable<CustomizableDataTranslationEntry> CreateEntries(ResourceString caption, IEnumerable<string> languages)
		{
			return languages
				.Where(language => CargoWiseOne.ResourceStrings.ResourceStrings.Normalize(language) != Res.DefaultLanguage)
				.Select(language => new CustomizableDataTranslationEntry(this, language, caption) { HasChanges = false, ReadOnly = ReadOnly });
		}

		public Dictionary<string, ZStringBuilder> ExportEntries()
		{
			var dictionary = new Dictionary<string, ZStringBuilder>();

			foreach (CustomizableDataTranslationEntry entry in All)
			{
				ZStringBuilder sb;
				if (!dictionary.TryGetValue(entry.Language, out sb))
				{
					var header = new OCsvLine(new[] { "Language", "Original", "Language", "Translation" });

					sb = new ZStringBuilder();
					sb.Append(header.ToStringWithNewLine());
					dictionary.Add(entry.Language, sb);
				}

				var line = new OCsvLine(new string[] { Res.DefaultLanguage, entry.English.Replace("\t", ""), entry.Language, entry.Translation.Replace("\t", "") });

				sb.Append(line.ToStringWithNewLine());
			}

			return dictionary;
		}

		public virtual string ImportEntries(Stream importFileStream, string fileName)
		{
			if (ReadOnly)
			{
				return Res.GetString("3ADC5CB0-FBB2-4DBE-9F13-FED50DA8BF3A", "Editing is not allowed");
			}

			using (StreamReader reader = new StreamReader(importFileStream))
			{
				string csvLineRaw;
				int index = 0;
				var entriesPerLang = new Dictionary<string, IEnumerable<CustomizableDataTranslationEntry>>();

				while ((csvLineRaw = ImportWizard.GetNextLine(reader)) != null)
				{
					if (index++ == 0)
					{
						continue;
					}

					var csvEntry = ParseLine(csvLineRaw);

					if (csvEntry == null)
					{
						return Res.GetString("F2ACA4B9-ADDF-4CDD-B15C-378736B4FA82", "Unable to import {0}", fileName);
					}

					IEnumerable<CustomizableDataTranslationEntry> entries;
					var language = csvEntry.Language;

					if (!entriesPerLang.TryGetValue(language, out entries))
					{
						entries = All.Cast<CustomizableDataTranslationEntry>().Where(e => e.Language.EqualsIgnoringCase(language));
						entriesPerLang.Add(language, entries);
					}

					var entry = entries.FirstOrDefault(e => e.English.EqualsIgnoringCase(csvEntry.Original));
					if (entry != null && !entry.Translation.Equals(csvEntry.Translation))
					{
						entry.Translation = csvEntry.Translation;
					}
				}
			}

			return string.Empty;
		}

		CsvEntry ParseLine(string line)
		{
			if (string.IsNullOrEmpty(line))
			{
				return null;
			}

			var csvLine = new OCsvLine(line);

			if (csvLine.FieldValues.Length != 4)
			{
				return null;
			}

			return new CsvEntry(csvLine.FieldValues[1], csvLine.FieldValues[2], csvLine.FieldValues[3]);
		}

		class CsvEntry
		{
			public CsvEntry(ZString original, ZString language, ZString translation)
			{
				Original = original;
				Language = language;
				Translation = translation;
			}

			public ZString Original { get; }
			public ZString Language { get; }
			public ZString Translation { get; }
		}

		public ZString CurrentLanguage
		{
			get { return currentLanguage; }
			set
			{
				if (value != currentLanguage)
				{
					currentLanguage = value;
					AllValuesInCurrentLanguage.RemoveAll();
					AllValuesInCurrentLanguage.AddRange(All.Where(entry => entry.Language == currentLanguage));
				}
			}
		}
		ZString currentLanguage;

		public ResourceString CurrentCaption
		{
			get { return currentCaption; }
			set
			{
				if (value != currentCaption)
				{
					currentCaption = value;
					AllTranslationsOfCurrentValue.RemoveAll();
					AllTranslationsOfCurrentValue.AddRange(All.Where(entry => entry.Caption.ResourceKey == currentCaption.ResourceKey));
				}
			}
		}
		ResourceString currentCaption;

		public CustomizableDataTranslationCollection AllTranslationsOfCurrentValue
		{
			get { return allTranslationsOfCurrentValue; }
		}
		readonly CustomizableDataTranslationCollection allTranslationsOfCurrentValue;

		public CustomizableDataTranslationCollection AllValuesInCurrentLanguage
		{
			get { return allValuesInCurrentLanguage; }
		}
		readonly CustomizableDataTranslationCollection allValuesInCurrentLanguage;

		public CustomizableDataTranslationCollection All
		{
			get { return all; }
		}
		readonly CustomizableDataTranslationCollection all;

		public CustomizableDataResourceStrings Customizable
		{
			get;
			private set;
		}

		public ZString Description
		{
			get { return Customizable.Source.Description; }
		}

		public void Save()
		{
			using (ResourceStringsFactory.HoldCacheReferences())
			{
				var update = new List<HelpDataString>();
				foreach (CustomizableDataTranslationEntry entry in All)
				{
					if (entry.HasChanges)
					{
						string key = entry.Caption.ResourceKey;
						var match = ResourceStringsFactory.LookupWithLanguageFallback(entry.Language, key);
						if (match == null)
						{
							if (entry.English != entry.Translation)
							{
								match = new HelpDataString();
								match.HD_Language = entry.Language;
								match.HD_Code = key;
								match.HD_Caption = entry.Translation;
							}
						}
						else if (match.HD_IsCheckedOut)
						{
							var original = ResourceStringsFactory.Lookup(entry.Language, key, false);
							if (original != null && original.GetCaptionOrFullDescription() == entry.Translation)
							{
								match.Delete();
							}
							else if (original == null && entry.English == entry.Translation)
							{
								match.Delete();
							}
							else
							{
								match.HD_Caption = entry.Translation;
							}
						}
						else
						{
							match.HD_Caption = entry.Translation;
						}
						if (match != null && (match.IsDeleted || match.HasChanges))
						{
							var eng = ResourceStringsFactory.Lookup(Res.DefaultLanguage, key);
							if (eng == null)
							{
								eng = new HelpDataString();
								eng.HD_Language = Res.DefaultLanguage;
								eng.HD_Code = key;
								eng.HD_Caption = entry.English;
								update.Add(eng);
							}

							update.Add(match);
						}
					}
					entry.HasChanges = false;
				}
				ResourceStringsFactory.Save(EditReasons.Codes.CustomizableDataTranslation, update.ToArray());
			}
		}
	}
}
