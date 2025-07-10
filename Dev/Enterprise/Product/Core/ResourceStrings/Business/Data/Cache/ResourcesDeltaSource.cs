using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.IO;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ResourceStrings.Business
{
	public class ResourcesDeltaSource : XmlResourceStringSource
	{
		public static ResourceStringSource Create(string language)
		{
#if DEBUG
			if (!UseDatabaseStrings.Value)
			{
				var languageFile = GetSourceControlLanguageFile(language);
				if (File.Exists(languageFile))
				{
					return new ResourcesDeltaSource(GetSourceControlLanguageFile(language), DeltaSourceLocationType.SourceControl, language);
				}
			}
			if (DatabaseResourceStringSource.disabled)
			{
				return null;
			}
			else
#endif
			{
				if (language != Res.DefaultLanguage)
				{
					return new DatabaseResourceStringSource(language);
				}
				else
				{
					return null;
				}
			}
		}

		public static Dictionary<string, ResourceStringSourceDataPair> CreateAll(string[] languages)
		{
			var res = new Dictionary<string, ResourceStringSourceDataPair>();
			Dictionary<string, ResourceStringSourceDataPair> dataBaseSources = null;
#if DEBUG
			if (!DatabaseResourceStringSource.disabled)
			{
				dataBaseSources = DatabaseResourceStringSource.CreateAll(languages);
			}
			if (!UseDatabaseStrings.Value)
			{
				foreach (var language in languages)
				{
					var languageFile = GetSourceControlLanguageFile(language);
					if (File.Exists(languageFile))
					{
						var source = new ResourcesDeltaSource(GetSourceControlLanguageFile(language), DeltaSourceLocationType.SourceControl, language);
						res.Add(language, new ResourceStringSourceDataPair(source, source.ReadAll()));
					}
					else if (!DatabaseResourceStringSource.disabled)
					{
						res.Add(language, new ResourceStringSourceDataPair(dataBaseSources[language].Source, dataBaseSources[language].Data));
					}
					else
					{
						res.Add(language, new ResourceStringSourceDataPair(null, Enumerable.Empty<ResourceStringData>()));
					}
				}
				return res;
			}
			if (DatabaseResourceStringSource.disabled)
			{
				return null;
			}
			else
#endif
			{
				return dataBaseSources ?? DatabaseResourceStringSource.CreateAll(languages);
			}
		}

		public static ResourcesDeltaSource CreateMock(string language)
		{
			return new ResourcesDeltaSource(Temp.GetTempFileName(), DeltaSourceLocationType.Temp, language);
		}

		public enum DeltaSourceLocationType
		{
			SourceControl,
			Temp,
		}

#if DEBUG
		static string GetSourceControlLanguageFile(string language)
		{
			return Path.Combine(GetSourceControlDirectory(language), "ResourcesDelta.xml");
		}

		internal static string GetSourceControlDirectory(string language)
		{
			var sourcePath = ForcedSourcePath ?? new DirectoryInfo(AssemblyLoader.GetBinPath()).Parent.FullName;
			return Path.Combine(sourcePath, Path.GetDirectoryName(DataFile.GetRelativeFilePath(language)));
		}

		public static string GetSourceControlLanguageFile(string rootDirectory, string language)
		{
			return Path.Combine(Path.Combine(rootDirectory, Path.GetDirectoryName(DataFile.GetRelativeFilePath(language))), "ResourcesDelta.xml");
		}
#endif

		public ResourcesDeltaSource(string file, DeltaSourceLocationType locationType, string language)
			: base(language)
		{
			this.file = file;
			this.locationType = locationType;
		}

		protected override Stream GetXmlStream()
		{
			if (!File.Exists(file) || new FileInfo(file).Length == 0)
			{
				return null;
			}
			return File.OpenRead(file);
		}

		public override void WriteAll(IEnumerable<ResourceStringData> resources)
		{
			if (Globals.IsTest && !file.StartsWith(Temp.TempPath, StringComparison.InvariantCultureIgnoreCase))
			{
				throw new InvalidOperationException(string.Format("Attempted to save {0} in a unit test, use a mock source in unit tests", file));
			}

			if (locationType == DeltaSourceLocationType.SourceControl && SourceControl.EnterpriseDatabase.IsFileInSourceControl(file))
			{
				SourceControl.EnterpriseDatabase.CheckOut(file, false);
			}
			else if (!Directory.Exists(Path.GetDirectoryName(file)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(file));
			}
			using (var stream = File.Open(file, FileMode.Create, FileAccess.Write))
			{
				ResourceStringXmSerializer serializer = new ResourceStringXmSerializer(stream, Language);
				serializer.Serialize(resources);
			}
		}

		public void Delete()
		{
			if (File.Exists(file))
			{
				File.Delete(file);
			}
		}

#if DEBUG
		internal string FilePath
		{
			get { return file; }
		}

		public readonly static Overridable<bool> UseDatabaseStrings = new Overridable<bool>(false);

		readonly static Overridable<string> forcedSourcePathOverride = new Overridable<string>(null);

		public static string ForcedSourcePath
		{
			get => forcedSourcePathOverride.Value;
			set => forcedSourcePathOverride.Value = value;
		}
#endif

		readonly string file;
		readonly DeltaSourceLocationType locationType;
	}
}
