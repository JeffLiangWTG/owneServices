using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Dat.Integration;
using Dat.Integration.VersionControl;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Analysis;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using TestResult = Dat.Integration.TestResult;

namespace Enterprise.ResourceStrings.Maintenance.Test
{
	[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Baseline")]
	public class TranslationFileExporter : ITranslatableContentAdapter, ITradosFileExporter
	{
		#region Dispose Member

		public void Dispose()
		{
			try
			{
				Dispose(true);
			}
			finally
			{
				GC.SuppressFinalize(this);
			}
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				importer?.Dispose();
				importer = null;

				resHashCalculator?.Dispose();
				resHashCalculator = null;
			}
		}

		#endregion

		#region ITradosFileExporter

		public void Run(TradosProjectCreator options, string directory)
		{
			this.options = options;
			using (ResourceStringsFactory.HoldCacheReferences())
			{
				if (options.ProjectType == TradosProjectCreator.ProjectTypeCodes.DocBuilder)
				{
					ExportDocBuilderFiles(Path.Combine(directory, TradosProjectCreator.ProjectTypeDescription.DocBuilder));
				}
				else if (options.ProjectType == TradosProjectCreator.ProjectTypeCodes.Report)
				{
					ExportReportFiles(Path.Combine(directory, TradosProjectCreator.ProjectTypeDescription.Report));
				}
				else if (options.ProjectType == TradosProjectCreator.ProjectTypeCodes.WebTracker)
				{
					ExportCodeStrings(directory);
				}
				else
				{
					ExportAllFiles(directory);
				}
			}
		}

		#endregion

		#region ITranslatableContentAdapter

		public void ExportAllContent(string contentModule, string targetDirectory)
		{
			options = new TradosProjectCreator();
			options.ProjectType = options.ProjectTypesList.GetCodeFromDescription(contentModule);
			ExportContent(targetDirectory);
		}

		public void ExportUntranslatedContent(string contentModule, string language, string targetDirectory)
		{
			options = new TradosProjectCreator();
			options.ProjectType = options.ProjectTypesList.GetCodeFromDescription(contentModule);
			options.UntranslatedOnly = true;
			options.LanguagesList[options.LanguagesLookupList.GetDescriptionFromCode(Culture.GetLanguageForCulture(CultureInfo.GetCultureInfo(language)))].Value = true;
			ExportContent(targetDirectory);
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502")]
		void ExportContent(string targetDirectory)
		{
			exportedKeys.Clear();

			switch (options.ProjectType)
			{
				case TradosProjectCreator.ProjectTypeCodes.DocBuilder:
					ExportDocBuilderFiles(targetDirectory);
					break;

				case TradosProjectCreator.ProjectTypeCodes.GUI:
				case TradosProjectCreator.ProjectTypeCodes.Billing:
				case TradosProjectCreator.ProjectTypeCodes.PAVE:
				case TradosProjectCreator.ProjectTypeCodes.Customs:
					foreach (var docMacroKey in DocBuilderBizOFieldCodeStrings.Values.SelectMany(v => v.Data).Select(d => d.Key))
					{
						exportedKeys.Add(docMacroKey);
					}
					ExportBashedStrings(targetDirectory);
					ExportCodeStrings(targetDirectory);
					break;

				case TradosProjectCreator.ProjectTypeCodes.WebTracker:
					ExportCodeStrings(targetDirectory);
					break;

				case TradosProjectCreator.ProjectTypeCodes.Report:
					ExportReportFiles(targetDirectory);
					break;

				case TradosProjectCreator.ProjectTypeCodes.UpdateNotes:
					Export(Path.Combine(targetDirectory, "UpdateNoteSummaries.xml"), UpdateNotesSummaryKeyPrefix);
					break;

				default:
					throw new InvalidOperationException("Unknown content module");
			}
		}

		public void ImportTranslatedContent(string contentModule, string language, string contentDirectory, IWorkspaceAccess workspace)
		{
			if (importer == null)
			{
				importer = new XmlFileImporter(logger: null) { EditReason = EditReasons.Codes.TradosImport };
			}
			importer.Import(contentDirectory, Culture.GetLanguageForCulture(new CultureInfo(language)));
		}

		const string TranslationErrorPattern = @"(?<=<script id=""translationErrors"" type=""text/xmldata"">)(?<Errors>.*?)(?=</script>)";
		Regex TranslationErrorRegex
		{
			get
			{
				if (translationErrorRegex == null)
				{
					translationErrorRegex = new Regex(TranslationErrorPattern, RegexOptions.Compiled | RegexOptions.Singleline);
				}
				return translationErrorRegex;
			}
		}
		Regex translationErrorRegex;

		public string HandleFailures(string contentModule, string language, IWorkspaceAccess workspace, TestResult[] failedTests, out bool reshelve)
		{
			language = Culture.GetLanguageForCulture(new CultureInfo(language));
			var failResKeys = new HashSet<string>();
			var errorMessage = new StringBuilder();
			errorMessage.AppendLine($"The following {language} resource strings caused errors. The check-in will be attemtped again with these strings excluded.");
			errorMessage.AppendLine("<table><thead><tr><th>Resource String</th><th>Error</th></tr></thead>");

			foreach (var test in failedTests)
			{
				var failureDetail = string.Join(System.Environment.NewLine, test.FailureDetails);
				var errorMatches = TranslationErrorRegex.Matches(failureDetail);
				foreach (Match errorMatch in errorMatches)
				{
					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(errorMatch.Groups["Errors"].Value);
					var errorList = xmlDoc.DocumentElement.SelectNodes("TranslationError");

					foreach (XmlNode error in errorList)
					{
						var message = error.SelectSingleNode("ErrorMessage").InnerText;
						var resNode = error.SelectSingleNode("Translation").SelectSingleNode("Res");
						var key = resNode.SelectSingleNode("Key").InnerText;

						failResKeys.Add(key);
						errorMessage.Append("<tr><td><pre>").Append(HtmlFormatter.Html(resNode.OuterXml)).Append("</pre></td><td>").Append(message).Append("</td></tr>");
					}
				}
			}
			errorMessage.AppendLine("</table>");

			if (failResKeys.Count > 0)
			{
				var change = workspace.GetPendingChanges().Single(c => c.ServerItem.EndsWith("/" + language + "/ResourcesDelta.xml", StringComparison.OrdinalIgnoreCase));
				var resources = new XmlDocument();
				resources.Load(change.LocalItem);
				var resNodes = resources.DocumentElement.SelectNodes("Res");
				for (var i = resNodes.Count - 1; i >= 0; i--)
				{
					var child = resNodes[i];
					var key = child.SelectSingleNode("Key");
					if (key != null && failResKeys.Contains(key.InnerText))
					{
						resources.DocumentElement.RemoveChild(child);
					}
				}
				resources.Save(change.LocalItem);
				reshelve = true;
			}
			else
			{
				reshelve = false;
			}

			return reshelve ? errorMessage.ToString() : null;
		}

		XmlFileImporter importer;

		#endregion

		BusinessObjectFactory factory;
		BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory(Db.Connection);
				}
				return factory;
			}
		}

		public void ExportAllFiles(string tempDirectory)
		{
			ExportReportFiles(tempDirectory);
			ExportDocBuilderFiles(tempDirectory);
			ExportBashedStrings(tempDirectory);
			ExportCodeStrings(tempDirectory);
			LogNsMappings(tempDirectory);
		}

		public void ExportReportFiles(string reportBaseDirectory)
		{
			ExportReportLabels(reportBaseDirectory);
			Export(Path.Combine(reportBaseDirectory, "Titles.xml"), DocBuilderResourceStrings.ReportTitleKeyPrefix);
		}

		public void ExportDocBuilderFiles(string docBuilderDirectory)
		{
			ExportDocBuilderLabelStrings(Path.Combine(docBuilderDirectory, "Labels.xml"));
			Export(Path.Combine(docBuilderDirectory, "Titles.xml"), DocBuilderResourceStrings.ReportNameKeyPrefix);
			Export(Path.Combine(docBuilderDirectory, "Currencies.xml"), "RX_Desc$", "RX_UnitName$", "RX_SubUnitName$");
			ExportDocBuilderBizOFieldCodeStrings(Path.Combine(docBuilderDirectory, "MacroValues.xml"));
		}

		public void ExportDocBuilderBizOFieldCodeStrings(string file)
		{
			using (var writer = new TranslationFileExporterWriter(file, null))
			{
				var bizOFieldStrings = DocBuilderBizOFieldCodeStrings;
				var stringLookup = new Dictionary<string, List<string>>();
				foreach (var macroItem in bizOFieldStrings)
				{
					foreach (var res in macroItem.Value.Data)
					{
						if (!stringLookup.ContainsKey(res.Key))
						{
							stringLookup.Add(res.Key, new List<string>());
						}
						stringLookup[res.Key].Add(macroItem.Key);
					}
				}

				foreach (var lookupItem in stringLookup)
				{
					if (lookupItem.Value.Count > 1)
					{
						string bestMacro = null;
						lookupItem.Value.Sort();
						foreach (var macro in lookupItem.Value)
						{
							if (bestMacro == null || bizOFieldStrings[macro].Data.Length >= bizOFieldStrings[bestMacro].Data.Length)
							{
								bestMacro = macro;
							}
						}
						foreach (var macro in lookupItem.Value)
						{
							if (macro != bestMacro)
							{
								bizOFieldStrings[macro].Data = Array.FindAll(bizOFieldStrings[macro].Data, item => item.Key != lookupItem.Key);
							}
						}
					}
				}

				foreach (var macroItem in bizOFieldStrings)
				{
					if (macroItem.Value.Data.Length > 0)
					{
						writer.StartContainer(macroItem.Key);
						foreach (var res in macroItem.Value.Data)
						{
							var helpDataString = ResourceStringsFactory.Lookup(Res.DefaultLanguage, res.Key, true) ?? ResourceStringsFactory.Lookup(Res.DefaultLanguage, res.Key, false);
							if (helpDataString != null)
							{
								WriteString(helpDataString, writer);
							}
						}
						writer.EndContainer();
					}
				}
			}
		}

		void ExportDocBuilderLabelStrings(string file)
		{
			var templateTranslationHelper = new DocBuilderTemplateTranslationHelper();
			using (var writer = new TranslationFileExporterWriter(file, null))
			{
				foreach (var result in templateTranslationHelper.GetUniqueLabels(Factory))
				{
					HelpDataString helpDataString = new HelpDataString();
					helpDataString.FromResourceStringData(result.Data);
					WriteString(helpDataString, writer);
				}
			}
		}

		void Export(string file, params string[] keyPrefixes)
		{
			foreach (var keyPrefix in keyPrefixes)
			{
				ZQuery query = new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
				query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.StartsWith, keyPrefix);
				HelpDataString[] resourceStrings = ResourceStringsFactory.Load(query);
				using (var writer = new TranslationFileExporterWriter(file, null))
				{
					foreach (var resourceString in resourceStrings)
					{
						WriteString(resourceString, writer);
					}
				}
			}
		}

		public void ExportBashedStrings(string outputDirectory)
		{
			ZQuery query = new ZQuery(HelpDataStringSchema.HD_ControlPath, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_IsCheckedOut, false);
			var bashed = ResourceStringsFactory.Load(query);
			Array.Sort(bashed, (item1, item2) =>
			{
				int result = GUIStringContextInfo.Parse(item1.HD_ControlPath).GroupingKey.CompareTo(GUIStringContextInfo.Parse(item2.HD_ControlPath).GroupingKey);
				if (result == 0)
				{
					result = item1.HD_ControlIndexInParent.CompareTo(item2.HD_ControlIndexInParent);
					if (result == 0)
					{
						result = item1.HD_Code.CompareTo(item2.HD_Code);
					}
				}
				return result;
			});

			string currentGroupingKey = null;
			TranslationFileExporterWriter writer = null;
			List<string> currentSubContainerPath = new List<string>();
			foreach (var item in bashed)
			{
				var context = GUIStringContextInfo.Parse(item.HD_ControlPath);
				TranslationFileModuleMapping.ModuleTypes moduleType;
				string directory = GetDirectory(outputDirectory, "GUI", context, out moduleType);
				if (moduleType == TranslationFileModuleMapping.ModuleTypes.GUI && this.options.ProjectType == TradosProjectCreator.ProjectTypeCodes.GUI)
				{
					if (currentGroupingKey != context.GroupingKey)
					{
						if (writer != null)
						{
							currentSubContainerPath.ForEach(x => writer.EndContainer());
							writer.Dispose();
						}
						currentSubContainerPath.Clear();
						currentGroupingKey = context.GroupingKey;
						string fileName = GetFileName(context);
						writer = new TranslationFileExporterWriter(Path.Combine(directory, fileName + ".xml"), context.GroupingKey);
					}
					string[] subContainerPath = context.SubContainerPath.Split('\\');
					if (subContainerPath.Length == 1 && string.IsNullOrEmpty(subContainerPath[0]))
					{
						subContainerPath = Array.Empty<string>();
					}
					while (currentSubContainerPath.Count > 0 && !IsSubdirectory(currentSubContainerPath, subContainerPath))
					{
						writer.EndContainer();
						currentSubContainerPath.RemoveAt(currentSubContainerPath.Count - 1);
					}
					for (int i = currentSubContainerPath.Count; i < subContainerPath.Length; i++)
					{
						writer.StartContainer(subContainerPath[i]);
						currentSubContainerPath.Add(subContainerPath[i]);
					}

					WriteString(item, writer);
				}
			}

			if (writer != null)
			{
				writer.Dispose();
			}
		}

		void ExportReportLabels(string ourputDirectory)
		{
			var query = new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_IsCheckedOut, false);
			query.AddToFilter(HelpDataStringSchema.HD_ControlPath, string.Empty);
			query.AddToFilter(HelpDataStringSchema.HD_ContextClassName, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.StartsWith, DocBuilderResourceStrings.ReportLabelKeyPrefix);
			var strings = ResourceStringsFactory.Load(query);
			Array.Sort(strings, (item1, item2) =>
			{
				int result = item1.HD_ContextClassName.CompareTo(item2.HD_ContextClassName);
				if (result == 0)
				{
					result = item1.HD_ContextSourceFile.CompareTo(item2.HD_ContextSourceFile);
					if (result == 0)
					{
						result = item1.HD_Code.CompareTo(item2.HD_Code);
					}
				}
				return result;
			});

			string currentClassName = null;
			TranslationFileExporterWriter writer = null;
			try
			{
				foreach (var item in strings)
				{
					string className = item.HD_ContextClassName;
					if (className != currentClassName)
					{
						if (writer != null)
						{
							writer.EndContainer();
							writer.Dispose();
							writer = null;
						}
						currentClassName = className;

						var file = Path.Combine(ourputDirectory, currentClassName + ".xml");
						file = LimitFileSize(file);

						writer = new TranslationFileExporterWriter(file, null);
						writer.StartContainer(className);
					}

					if (writer != null)
					{
						WriteString(item, writer);
					}
				}
			}
			finally
			{
				if (writer != null)
				{
					writer.Dispose();
				}
			}
		}

		public void ExportCodeStrings(string outputDirectory)
		{
			var controlPathLookup = new Dictionary<string, string>();
			ZQuery query = new ZQuery(HelpDataStringSchema.HD_ControlPath, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_IsCheckedOut, false);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.DoesNotStartWith, DocBuilderResourceStrings.ReportTitleKeyPrefix);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.DoesNotStartWith, DocBuilderResourceStrings.ReportLabelKeyPrefix);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.DoesNotStartWith, UpdateNotesSummaryKeyPrefix);
			var strings = ResourceStringsFactory.Load(query);
			foreach (var item in strings)
			{
				if (!controlPathLookup.ContainsKey(item.HD_ContextClassName))
				{
					controlPathLookup.Add(item.HD_ContextClassName, item.HD_ControlPath);
				}
			}

			query = new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_IsCheckedOut, false);
			query.AddToFilter(HelpDataStringSchema.HD_ControlPath, string.Empty);
			query.AddToFilter(HelpDataStringSchema.HD_ContextClassName, SQLComparisonOperator.NotEqual, string.Empty);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.DoesNotStartWith, DocBuilderResourceStrings.ReportTitleKeyPrefix);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.DoesNotStartWith, DocBuilderResourceStrings.ReportLabelKeyPrefix);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.DoesNotStartWith, UpdateNotesSummaryKeyPrefix);
			strings = ResourceStringsFactory.Load(query);
			Array.Sort(strings, (item1, item2) =>
			{
				int result = item1.HD_ContextClassName.CompareTo(item2.HD_ContextClassName);
				if (result == 0)
				{
					result = item1.HD_ContextSourceFile.CompareTo(item2.HD_ContextSourceFile);
					if (result == 0)
					{
						result = item1.HD_ContextSourceFileLine.CompareTo(item2.HD_ContextSourceFileLine);
						if (result == 0)
						{
							result = item1.HD_ContextSourceFileCol.CompareTo(item2.HD_ContextSourceFileCol);
							if (result == 0)
							{
								result = item1.HD_Code.CompareTo(item2.HD_Code);
							}
						}
					}
				}
				return result;
			});

			string currentClassName = null;
			TranslationFileExporterWriter writer = null;
			foreach (var item in strings)
			{
				string className = item.HD_ContextClassName;
				if (className != currentClassName)
				{
					if (writer != null)
					{
						writer.EndContainer();
						writer.Dispose();
						writer = null;
					}
					currentClassName = className;

					TranslationFileModuleMapping.ModuleTypes moduleType = TranslationFileModuleMapping.ModuleTypes.DoNotTranslate;
					string file = null;
					string linkedControlPath;
					controlPathLookup.TryGetValue(item.HD_ContextClassName, out linkedControlPath);
					if (linkedControlPath != null)
					{
						var context = GUIStringContextInfo.Parse(linkedControlPath);
						file = Path.Combine(GetDirectory(outputDirectory, "GUI", context, out moduleType), GetFileName(context) + ".xml");
					}

					if (string.IsNullOrEmpty(file) || !File.Exists(file))
					{
						string directory = null;
						string ns = GetNamespaceFromClassName(className);
						directory = GetDirectory(outputDirectory, "Code", ns, out moduleType);
						file = Path.Combine(directory, CodeFileName(ns));
						file = LimitFileSize(file);
					}

					if (IsMatchingModuleType(moduleType))
					{
						writer = new TranslationFileExporterWriter(file, null);
						writer.StartContainer(className);
					}
				}

				if (writer != null)
				{
					WriteString(item, writer);
				}
			}

			if (writer != null)
			{
				writer.Dispose();
			}
		}

		bool IsMatchingModuleType(TranslationFileModuleMapping.ModuleTypes moduleType)
		{
			return (options.ProjectType == TradosProjectCreator.ProjectTypeCodes.ALL && moduleType != TranslationFileModuleMapping.ModuleTypes.DoNotTranslate) ||
				(options.ProjectType == TradosProjectCreator.ProjectTypeCodes.GUI && moduleType == TranslationFileModuleMapping.ModuleTypes.GUI) ||
				(options.ProjectType == TradosProjectCreator.ProjectTypeCodes.WebTracker && moduleType == TranslationFileModuleMapping.ModuleTypes.WebTracker) ||
				(options.ProjectType == TradosProjectCreator.ProjectTypeCodes.Billing && moduleType == TranslationFileModuleMapping.ModuleTypes.Billing) ||
				(options.ProjectType == TradosProjectCreator.ProjectTypeCodes.PAVE && moduleType == TranslationFileModuleMapping.ModuleTypes.PAVE) ||
				(options.ProjectType == TradosProjectCreator.ProjectTypeCodes.Customs && moduleType == TranslationFileModuleMapping.ModuleTypes.Customs) ||
				(options.ProjectType == TradosProjectCreator.ProjectTypeCodes.UpdateNotes && moduleType == TranslationFileModuleMapping.ModuleTypes.UpdateNotes)
				;
		}

		string CodeFileName(string ns)
		{
			string[] parts = TranslationFileModuleMapping.Instance.Filter(ns.Split('.'));
			if (parts.Length > 2)
			{
				Array.Resize(ref parts, 2);
			}
			return string.Join(".", parts) + ".xml";
		}

		public static string GetNamespaceFromClassName(string className)
		{
			var result = string.Empty;
			if (!string.IsNullOrEmpty(className))
			{
				var length = className.LastIndexOf(".");
				if (length != -1)
				{
					result = className.Substring(0, length);
				}
			}

			return result;
		}

		public static string GetNamespaceFromSourceFile(string sourceFile)
		{
			if (File.Exists(sourceFile))
			{
				using (var sourceFileReader = new StreamReader(sourceFile))
				{
					string line;
					while ((line = sourceFileReader.ReadLine()) != null)
					{
						if (line.StartsWith("namespace "))
						{
							return line.Substring(10).Trim();
						}
					}
				}
			}
			return null;
		}

		void LoadFileNameMappings(string directory, Dictionary<string, string> mappings)
		{
			foreach (string file in Directory.GetFiles(directory, "*.cs"))
			{
				string className = Path.GetFileNameWithoutExtension(file);
				string mapping;
				mappings.TryGetValue(className, out mapping);
				mappings[className] = mapping != null ? mapping + "," + file : file;
			}
			foreach (string subDirectory in Directory.GetDirectories(directory))
			{
				LoadFileNameMappings(subDirectory, mappings);
			}
		}

		string GetDirectory(string outputDirectory, string subdirectory, GUIStringContextInfo context, out TranslationFileModuleMapping.ModuleTypes moduleType)
		{
			return GetDirectory(outputDirectory, subdirectory, context.BasherNamespace, out moduleType);
		}

		string GetDirectory(string outputDirectory, string subdirectory, string ns, out TranslationFileModuleMapping.ModuleTypes moduleType)
		{
			var mapping = TranslationFileModuleMapping.Instance.Lookup(ns);
			allNsMappings[ns] = mapping;
			moduleType = mapping.ModuleType;
			return Path.Combine(Path.Combine(outputDirectory, mapping.ModuleName), subdirectory);
		}

		string GetFileName(GUIStringContextInfo context)
		{
			string fileName = context.BasherClassName;
			if (fileName.IndexOf('+') > -1)
			{
				fileName = fileName.Substring(0, fileName.IndexOf('+'));
			}
			fileName = fileName.Replace("BasherTest", "");
			fileName = fileName.Replace("Testing", "");
			fileName = fileName.Replace("Test", "");
			if (!string.IsNullOrEmpty(context.TabContainerPath))
			{
				string[] parsedTabContainerPath = context.TabContainerPath.Split('\\');
				fileName = fileName + "." + parsedTabContainerPath[parsedTabContainerPath.Length - 1];
			}
			if (fileName.EndsWith("."))
			{
				fileName = fileName.Remove(fileName.Length - 1);
			}
			fileName = fileName.Replace("..", ".");
			return fileName;
		}

		bool IsSubdirectory(List<string> currentSubContainerPath, string[] subContainerPath)
		{
			bool result = true;
			if (currentSubContainerPath.Count > subContainerPath.Length)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < currentSubContainerPath.Count; i++)
				{
					if (subContainerPath[i] != currentSubContainerPath[i])
					{
						result = false;
						break;
					}
				}
			}
			return result;
		}

		internal bool WriteString(HelpDataString item, TranslationFileExporterWriter writer)
		{
			bool write = !exportedKeys.Contains(item.HD_Code) && !ExcludeCodes.Contains(item.HD_Code);
			if (write)
			{
				var resourceStringData = item.ToResourceStringData();
				var sourceHash = ResHashCalculator.GetHash(resourceStringData);
				if (options.UntranslatedOnly || options.ExportGapList)
				{
					bool translationExists = TranslationExists(resourceStringData.Key, sourceHash);
					write = !translationExists || !options.UntranslatedOnly;
				}
				if (write)
				{
					writer.WriteString(item.ToResourceStringData(sourceHash, ""));
					RecordWordCount(item, writer.FilePath);
				}
				exportedKeys.Add(item.HD_Code);
			}
			return write;
		}

		void RecordWordCount(HelpDataString item, string path)
		{
			int wordCount = 0;
			wordCounts.TryGetValue(path, out wordCount);
			wordCounts[path] = wordCount + CountWords(item);
		}

		int CountWords(HelpDataString item)
		{
			return CountWords(item.HD_Caption) + CountWords(item.HD_ShortCaption) + CountWords(item.HD_MidCaption) + CountWords(item.HD_FullDescription);
		}

		int CountWords(string s)
		{
			int wordCount;
			if (string.IsNullOrEmpty(s))
			{
				wordCount = 0;
			}
			else
			{
				wordCount = s.CountMatches(' ') + s.CountMatches('\n');
				if (wordCount == 0)
				{
					wordCount = 1;
				}
				else
				{
					wordCount += 2;
				}
			}
			return wordCount;
		}

		string LimitFileSize(string file)
		{
			int wordCount = 0;
			wordCounts.TryGetValue(file, out wordCount);
			if (wordCount >= MaxWordsPerFile)
			{
				string name = Path.GetFileNameWithoutExtension(file);
				var numberedNameMatch = numberedFileName.Match(name);
				if (numberedNameMatch.Success)
				{
					name = name.Substring(0, numberedNameMatch.Index) + "." + (int.Parse(numberedNameMatch.Groups[1].Value) + 1).ToString();
				}
				else
				{
					name = name + ".2";
				}
				file = Path.Combine(Path.GetDirectoryName(file), name + Path.GetExtension(file));
				file = LimitFileSize(file);
			}
			return file;
		}
		readonly Regex numberedFileName = new Regex(@"\.(\d+)$", RegexOptions.Compiled);

		bool TranslationExists(string key, string sourceHash)
		{
			foreach (var language in options.SelectedLanguages)
			{
				ResourceStringData translationData;
				if (GetResourceStringData(language).TryGetValue(key, out translationData) && translationData.SourceHash == sourceHash)
				{
					return true;
				}
			}
			return false;
		}

		Dictionary<string, ResourceStringData> GetResourceStringData(string language)
		{
			Dictionary<string, ResourceStringData> resourceStringData;
			if (!allLanguageData.TryGetValue(language, out resourceStringData))
			{
				var xmlDataFileName = Path.Combine(DataFileSourcePath, language, "Resources.xml");
				resourceStringData = new Dictionary<string, ResourceStringData>();
				using (var xmlFile = File.OpenRead(xmlDataFileName))
				{
					foreach (var data in XmlResourceStringSource.ReadAll(xmlFile))
					{
						resourceStringData.Add(data.Key, data);
					}
				}
				allLanguageData.Add(language, resourceStringData);
			}
			return resourceStringData;
		}

		readonly Dictionary<string, Dictionary<string, ResourceStringData>> allLanguageData = new Dictionary<string, Dictionary<string, ResourceStringData>>();

		protected virtual string DataFileSourcePath
		{
			get { return Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Core\ResourceStrings\DataFiles\"); }
		}

		void LogNsMappings(string directory)
		{
			using (var writer = new StreamWriter(Path.Combine(directory, "mappings.log")))
			{
				foreach (var entry in allNsMappings)
				{
					writer.Write(entry.Key);
					writer.Write('\t');
					writer.Write(entry.Value.ModuleName);
					writer.Write('\t');
					writer.Write(IsMatchingModuleType(entry.Value.ModuleType) ? "Y" : "N");
					writer.WriteLine();
				}
			}
		}

		HashSet<string> excludeCodes;
		HashSet<string> ExcludeCodes
		{
			get
			{
				if (excludeCodes == null)
				{
					excludeCodes = GetExcludeCodes();
				}
				return excludeCodes;
			}
		}

		HashSet<string> GetExcludeCodes()
		{
			var result = new HashSet<string>();
			var asm = Assembly.GetExecutingAssembly();
			using (var reader = new StreamReader(asm.GetManifestResourceStream(asm.GetName().Name + "." + "DoNotTranslate.txt")))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					line = line.Trim();
					if (!string.IsNullOrEmpty(line))
					{
						result.Add(line);
					}
				}
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "Property exposed for mocking purposes only")]
		protected virtual Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>> DocBuilderBizOFieldCodeStrings
		{
			get
			{
				if (docBuilderBizOFieldCodeStrings == null)
				{
					var documentTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
					docBuilderBizOFieldCodeStrings = documentTemplateTranslationHelper.GetAllResStringsUsedByBizOFields(Factory);
				}
				return docBuilderBizOFieldCodeStrings;
			}
		}

		Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>> docBuilderBizOFieldCodeStrings;

		ResourceStringHashCalculator ResHashCalculator
		{
			get { return resHashCalculator = resHashCalculator ?? new ResourceStringHashCalculator(); }
		}
		ResourceStringHashCalculator resHashCalculator;

		const string UpdateNotesSummaryKeyPrefix = GlbReleaseNoteSchema.Constants.GF_Summary + "$";

		TradosProjectCreator options;
		readonly HashSet<string> exportedKeys = new HashSet<string>();
		readonly SortedList<string, TranslationFileModuleMapping.Node> allNsMappings = new SortedList<string, TranslationFileModuleMapping.Node>();
		readonly Dictionary<string, int> wordCounts = new Dictionary<string, int>();
		const int MaxWordsPerFile = 3000;
	}
}
