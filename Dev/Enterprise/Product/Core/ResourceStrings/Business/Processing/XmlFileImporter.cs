using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Business
{
	public sealed class XmlFileImporter : IDisposable
	{
		public XmlFileImporter()
				: this(null)
		{ }

		public XmlFileImporter(ImportLogger logger)
		{
			this.logger = logger;
		}

		public void Import(string path, string language)
		{
			var langaugeList = new CodeDescriptionPairList(OLookUpEditType.Language);
			if (!langaugeList.ContainsCode(language))
			{
				language = Culture.GetLanguageForCulture(new CultureInfo(language));
			}
			if (!langaugeList.ContainsCode(language) || Res.IsEnglish(language))
			{
				throw new InvalidOperationException("Unsupported language: " + language);
			}
			ImportTranslations(path, language);
		}

		string FindSourceDirectory(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				return null;
			}
			if (Directory.Exists(path))
			{
				foreach (var child in Directory.GetDirectories(path))
				{
					if (Path.GetFileName(child) == "en-US")
					{
						return child;
					}
				}
			}
			return FindSourceDirectory(Path.GetDirectoryName(path));
		}

		void ImportTranslations(string path, string language)
		{
			if (Directory.Exists(path))
			{
				foreach (var child in Directory.GetDirectories(path))
				{
					ImportTranslations(child, language);
				}
				foreach (var child in Directory.GetFiles(path))
				{
					ImportTranslations(child, language);
				}
			}
			else
			{
				if (Path.GetExtension(path).Equals(".xml", StringComparison.OrdinalIgnoreCase))
				{
					var dataStrings = new List<HelpDataString>();
					try
					{
						using (var reader = new XmlTextReader(path))
						{
							var datas = XmlResourceStringSource.ReadAll(reader, out _, true);
							foreach (var data in datas)
							{
								var resourceString = data;
								var status = GetStatus(ref resourceString);
								if (status == ImportStatus.Import)
								{
									var dataString = CheckOut(language, resourceString);
									if (dataString != null)
									{
										dataString.FromResourceStringData(resourceString);
										dataStrings.Add(dataString);
										if (!dataString.HasChanges)
										{
											status = ImportStatus.NotChanged;
										}
									}
									else
									{
										status = ImportStatus.NotChanged;
									}
								}
								Log(path, resourceString.Key, status);
							}
						}
					}
					catch (XmlException ex)
					{
						var message = $"Error processing {path}";
						if (ex.LineNumber > 0 && ex.LinePosition > 0)
						{
							var line = File.ReadAllLines(path)[ex.LineNumber - 1];
							var start = ex.LinePosition - 12;
							if (start < 0)
							{
								start = 0;
							}
							var end = ex.LinePosition + 9;
							if (end > line.Length)
							{
								end = line.Length;
							}
							message += " at " + line.Substring(start, end - start);
						}
						throw new XmlException(message, ex);
					}

					ResourceStringsFactory.Save(EditReason, dataStrings.ToArray());
				}
			}
		}

		ImportStatus GetStatus(ref ResourceStringData data)
		{
			var status = ImportStatus.Import;

			var englishString = ResourceStringsFactory.Lookup(Res.DefaultLanguage, data.Key, true) ?? ResourceStringsFactory.Lookup(Res.DefaultLanguage, data.Key, false);

			if (englishString != null)
			{
				if (!string.IsNullOrEmpty(data.SourceHash))
				{
					var englishStringSourceHash = ResHashCalculator.GetHash(englishString.ToResourceStringData());
					if (!data.SourceHash.Equals(englishStringSourceHash, StringComparison.OrdinalIgnoreCase))
					{
						status = ImportStatus.SourceChanged;
					}
				}
			}
			else
			{
				status = ImportStatus.NoMatchingSource;
			}

			return status;
		}

		HelpDataString CheckOut(string language, ResourceStringData data)
		{
			var checkedOut = ResourceStringsFactory.Lookup(language, data.Key, true);
			if (checkedOut == null)
			{
				var existing = ResourceStringsFactory.Lookup(language, data.Key, false);
				if (existing != null)
				{
					if ((data.Caption != null && data.Caption != existing.HD_Caption) ||
						 (data.ShortCaption != null && data.ShortCaption != existing.HD_ShortCaption) ||
						 (data.MediumCaption != null && data.MediumCaption != existing.HD_MidCaption) ||
						 (data.FullDescription != null && data.FullDescription != existing.HD_FullDescription))
					{
						checkedOut = existing.Clone();
					}
				}
				else
				{
					checkedOut = new HelpDataString();
					checkedOut.HD_Language = language;
				}
			}
			return checkedOut;
		}

		void Log(string file, string key, ImportStatus status)
		{
			if (logger != null)
			{
				logger.Log(file, key, status);
			}
		}

		public void Dispose()
		{
			resHashCalculator?.Dispose();
		}

		public string EditReason
		{
			get;
			set;
		}

		readonly ImportLogger logger;

		ResourceStringHashCalculator ResHashCalculator
		{
			get { return resHashCalculator = resHashCalculator ?? new ResourceStringHashCalculator(); }
		}

		ResourceStringHashCalculator resHashCalculator;
	}
}
