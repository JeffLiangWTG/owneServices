using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWiseOne.ResourceStrings;
using WTG.DevTools.Definitions;

[assembly: AssemblyTitle("ZRS Generator")]
[assembly: AssemblyDescription("ZRS Generator")]

namespace ZRSGenerator
{
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

			var languages = GetAllLanguages();
			var subModules = GetAllSubModules(args);

			Parallel.ForEach(subModules, subModule =>
			{
				MergeDependencyStrings(subModule);
				var lookups = InitializeKeyLookups(subModule);
				Parallel.ForEach(languages, language => SplitDataFile(lookups, language, subModule));
			});
		}

		static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Console.Error.Write(e.ExceptionObject.ToString());
			Environment.Exit(-1);
		}

		static IEnumerable<string> GetAllSubModules(string[] args)
		{
			var subModuleFilter = GetSubModuleFilter(args);

			if (subModuleFilter.useFilter)
			{
				return new List<string>() { subModuleFilter.subModule };
			}

			var subModules = new List<string>() { string.Empty };

			foreach (var file in Directory.GetFiles(BinPath, "Enterprise.ResourceStrings.DataFiles.*.zrs", SearchOption.AllDirectories))
			{
				var fileNameParts = Path.GetFileName(file).Split('.');
				if (fileNameParts.Length == 6 && !subModules.Contains(fileNameParts[4]))
				{
					subModules.Add(fileNameParts[4]);
				}
			}

			return subModules;
		}

		static (bool useFilter, string subModule) GetSubModuleFilter(string[] args)
		{
			if (args.Length == 1)
			{
				return (true, args[0]); // Hard coded Sub-Module
			}

			if (args.Length != 2 || string.Compare(args[0], args[1], true) == 0 ||
				!Directory.Exists(args[0]) || !Directory.Exists(args[1]))
			{
				return (false, string.Empty); // No filter or Invalid filter
			}

			return (true, SubModuleHelper.GetSubModule(args[0], args[1])); // Calculated Sub-Module from root path & project path
		}

		static void MergeDependencyStrings(string subModule)
		{
			var entries = new ConcurrentBag<ZrsFileEntryData>();
			Parallel.ForEach(Directory.GetFiles(DependencyResPath, "*.zrs", SearchOption.AllDirectories), file =>
			{
				var fileNameParts = Path.GetFileName(file).Split('.');

				if ((subModule.Length == 0 && fileNameParts.Length == 5) ||
					(fileNameParts.Length == 6 && fileNameParts[4] == subModule))
				{
					foreach (var entry in ZrsFile.ListEntries(file))
					{
						entries.Add(new ZrsFileEntryData(entry, ZrsFile.Read(file, entry)));
					}
				}
			});
			ZrsFile.Save(ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, subModule), entries);
		}

		static IEnumerable<string> GetAllLanguages()
		{
			List<string> languages = new List<string>();
			foreach (string subDirectory in Directory.GetDirectories(DataFileSourcePath))
			{
				string name = Path.GetFileName(subDirectory);
				if (name != Res.DefaultLanguage && name != "Template" && name != "Testing")
				{
					languages.Add(name);
				}
			}
			return languages;
		}

		static string DataFileSourcePath
		{
			get { return Path.Combine(Path.GetDirectoryName(BinPath), @"Enterprise\Product\Core\ResourceStrings\DataFiles\"); }
		}

		static string BinPath
		{
			get { return binPath ?? (binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); }
		}
		static string binPath;

		static string DependencyResPath => Path.Combine(BinPath, "res");

		static Lookups InitializeKeyLookups(string subModule)
		{
			var lookups = new Lookups
			{
				KeyFileLookup = new Dictionary<string, ushort>(StringComparer.OrdinalIgnoreCase),
				KeyChecksumLookup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
				KeyDataLookup = new Dictionary<string, ResourceStringData>(StringComparer.OrdinalIgnoreCase)
			};
			var zrsFilePath = ZrsFile.GetDefaultFilePath(Res.DefaultLanguage, subModule, BinPath);
			using (var hashCalc = new ResourceStringHashCalculator())
			{
				foreach (var entryId in ZrsFile.ListEntries(zrsFilePath))
				{
					foreach (var data in ZrsFile.Read(zrsFilePath, entryId))
					{
						if (!lookups.KeyFileLookup.ContainsKey(data.Key))
						{
							lookups.KeyFileLookup.Add(data.Key, entryId);
							lookups.KeyChecksumLookup.Add(data.Key, hashCalc.GetHash(data));
							lookups.KeyDataLookup.Add(data.Key, data);
						}
					}
				}
			}

			return lookups;
		}

		static void SplitDataFile(Lookups lookups, string language, string subModule)
		{
			string file = ZrsFile.GetDefaultFilePath(language, subModule, binPath);
			if (File.Exists(file))
			{
				File.Delete(file);
			}

			var xmlDataFileName = Path.Combine(DataFileSourcePath, language, "Resources.xml");
			if (File.Exists(xmlDataFileName))
			{
				var splitData = new Dictionary<UInt16, List<ResourceStringData>>();
				var savedKeys = new HashSet<string>();

				foreach (var data in XmlResourceStringSource.ReadAll(File.OpenRead(xmlDataFileName)))
				{
					AssignToSplitData(lookups, language, data, splitData, savedKeys);
				}

				if (language == "EN-GB")
				{
					AutoGBData(lookups, language, splitData, savedKeys);
				}

				var xmlSerializerOptions = ResourceStringXmlSerializerOptions.ExcludeEditReason | ResourceStringXmlSerializerOptions.ExcludeSourceHash;
				var rFresourceStrings = new List<ResourceStringData>();
				var isRFSupportedLanguage = RFSupportLanguages.Contains(language);

				if (isRFSupportedLanguage)
				{
					foreach (var entry in splitData.Where(entry => RFAssemblyIDs.Contains(entry.Key)))
					{
						rFresourceStrings.AddRange(entry.Value);
					}
				}

				ZrsFile.Save(file, splitData.Select(entry => new ZrsFileEntryData(entry.Key, entry.Value.AsEnumerable())), xmlSerializerOptions);

				if (isRFSupportedLanguage)
				{
					GenerateWarehouseRFResourceStringsXML(rFresourceStrings, language, xmlSerializerOptions);
				}
			}
			else
			{
				Console.Error.WriteLine(string.Format("Data file for language {0} does not exist, skipping {1}", language, xmlDataFileName));
			}
		}

		#region RFSupportLanguages

		static void GenerateWarehouseRFResourceStringsXML(List<ResourceStringData> rFresourceStrings, string language, ResourceStringXmlSerializerOptions xmlOption)
		{
			var xmlPath = Path.Combine(binPath, "Warehouse.RF.ResourceStrings." + language + ".xml");
			if (File.Exists(xmlPath))
			{
				File.Delete(xmlPath);
			}

			using (var stream = File.Create(xmlPath))
			{
				var serialiser = new ResourceStringXmSerializer(stream, Res.DefaultLanguage, xmlOption);
				serialiser.Serialize(rFresourceStrings);
			}
		}

		readonly static ImmutableHashSet<UInt16> RFAssemblyIDs = GetRFAssemblyIDs();

		static ImmutableHashSet<UInt16> GetRFAssemblyIDs()
		{
			unchecked
			{
				return new[]
				{
					(UInt16)ZrsFile.CalculateAsmid("Enterprise.Warehouse.RF.Core.NonCF"),
					(UInt16)ZrsFile.CalculateAsmid("Enterprise.Warehouse.RF.NonCF"),
					(UInt16)ZrsFile.CalculateAsmid("Enterprise.Warehouse.RF.Shared.NonCF"),
				}.ToImmutableHashSet();
			}
		}

		readonly static ImmutableHashSet<string> RFSupportLanguages =
			BuildXmlFile.Deserialize(Path.Combine(Path.GetDirectoryName(BinPath), BuildXmlFile.FileName))
			.OtherFiles.Items.OfType<BuildXmlOtherFilesFilename>().Where(f => f.Value.StartsWith("Warehouse.RF.ResourceStrings.", StringComparison.Ordinal) && f.Value.EndsWith(".xml", StringComparison.Ordinal))
			.Select(n => n.Value.Substring(0, n.Value.Length - 4).Remove(0, 29)).ToImmutableHashSet();

		#endregion

		static void AssignToSplitData(Lookups lookups, string language, ResourceStringData data, Dictionary<UInt16, List<ResourceStringData>> splitData, HashSet<string> savedKeys)
		{
			if (lookups.KeyFileLookup.TryGetValue(data.Key, out var entryId))
			{
				splitData.TryGetValue(entryId, out var itemList);
				if (string.IsNullOrEmpty(data.SourceHash))
				{
					throw new InvalidOperationException("SourceHash is empty for resource string " + data.Key + " in " + language);
				}
				if (lookups.KeyChecksumLookup[data.Key] == data.SourceHash && !lookups.KeyDataLookup[data.Key].ContentEquals(data))
				{
					if (itemList == null)
					{
						itemList = new List<ResourceStringData>();
						splitData.Add(entryId, itemList);
					}
					itemList.Add(data);
					savedKeys.Add(data.Key);
				}
			}
		}

		static void AutoGBData(Lookups lookups, string language, Dictionary<UInt16, List<ResourceStringData>> splitData, HashSet<string> savedKeys)
		{
			var defaultStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			foreach (var key in lookups.KeyFileLookup.Keys)
			{
				if (!savedKeys.Contains(key))
				{
					var data = defaultStrings.Get(key);
					var replacer = new GBReplacer();
					data = new ResourceStringData(data.Key,
						replacer.Replace(data.ShortCaption),
						replacer.Replace(data.MediumCaption),
						replacer.Replace(data.Caption),
						replacer.Replace(data.FullDescription),
						sourceHash: lookups.KeyChecksumLookup[data.Key]);
					if (replacer.replacments > 0)
					{
						AssignToSplitData(lookups, language, data, splitData, savedKeys);
					}
				}
			}
		}

		class GBReplacer
		{
			static readonly Regex regex = new Regex("(" + string.Join(")|(", new string[] {
				@"[lmnrtdc][iy](?'z'z)(e(?!n)|a|i)", // z -> s, organization -> organisation
				@"(vi|col|vap|lab|fav|arm|harb)(?'or'or)(s|ed)?\b", // or -> our, behavior -> behaviour
				@"(cance|labe|channe|leve|equa|dia|tota|trave)(?'l'l)(ed|ing|er)", // l -> ll, canceled -> cancelled
				@"(lit|(?<!para|odo|dia)met|cent|theat)(?'er'er)s?\b", // er -> re, liter -> litre
				@"(off|def)e(?'nse'nse)", // nse -> nce, defense -> defence
				@"fulfi(?'ll'll)(?!ed)|insta(?'ll'll)ment", // ll -> l, fulfill -> fulfil
				@"(dia|cata)(?'log'log)(s|ed)?\b", // dialog -> dialogue
			}) + ")", RegexOptions.IgnoreCase | RegexOptions.Compiled);

			public string Replace(string text)
			{
				if (NoNeedToReplaceWords.Contains(text))
				{
					return text;
				}

				return regex.Replace(text, Replace);
			}

			public string Replace(Match match)
			{
				replacments++;
				Group group;
				if ((group = match.Groups["z"]).Success)
				{
					return Replace(match, group, "s");
				}
				if ((group = match.Groups["or"]).Success)
				{
					return Replace(match, group, "our");
				}
				if ((group = match.Groups["l"]).Success)
				{
					return Replace(match, group, "ll");
				}
				if ((group = match.Groups["er"]).Success)
				{
					return Replace(match, group, "re");
				}
				if ((group = match.Groups["nse"]).Success)
				{
					return Replace(match, group, "nce");
				}
				if ((group = match.Groups["ll"]).Success)
				{
					return Replace(match, group, "l");
				}
				if ((group = match.Groups["log"]).Success)
				{
					return Replace(match, group, "logue");
				}
				throw new InvalidOperationException("unknown match");
			}

			string Replace(Match match, Group group, string replacement)
			{
				int groupOffset = group.Index - match.Index;
				return match.Value.Substring(0, groupOffset) +
					(IsUpperCase(group.Value) ? replacement.ToUpperInvariant() : replacement) +
					match.Value.Substring(groupOffset + group.Value.Length);
			}

			bool IsUpperCase(string value)
			{
				return value.Any(c => char.IsUpper(c));
			}

			public int replacments;
			readonly string[] NoNeedToReplaceWords = { "Belize" };
		}

		class Lookups
		{
			public Dictionary<string, UInt16> KeyFileLookup { get; set; }
			public Dictionary<string, string> KeyChecksumLookup { get; set; }
			public Dictionary<string, ResourceStringData> KeyDataLookup { get; set; }
		}
	}
}
