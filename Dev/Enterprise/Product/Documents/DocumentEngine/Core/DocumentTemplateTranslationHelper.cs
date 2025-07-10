using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Analysis;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine
{
	public abstract class DocumentTemplateTranslationHelper
	{
		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		protected Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> GetUniqueLabels(ExcelWorkSheet workSheet, string templateFilePath, int rowStart, int rowEnd)
		{
			var results = new Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>();
			var templateFileName = GetFileNameByPath(templateFilePath);

			for (var row = rowStart; row <= rowEnd; row++)
			{
				var cell = workSheet.GetCell(row, 0);

				GroupName = AnalyseGroupName(workSheet, cell, row);
				for (var column = 1; column < workSheet.ColumnCount; column++)
				{
					cell = workSheet.GetCell(row, column);
					if (!cell.IsEmpty)
					{
						var analyzer = new ExcelCellAnalyzer(cell);
						foreach (var translatable in analyzer.GetTextToBeTranslated())
						{
							var key = AnalyzeResourceStringKey(templateFileName, GroupName, KeyPrefix, translatable.TranslatableText);
							if (!results.ContainsKey(key))
							{
								results.Add(key, GenerateDataWithDocumentMacroUsage(GenerateResourceStringData(key, translatable.TranslatableText, templateFileName, templateFilePath)));
							}
							results[key].DocBuilderUsages.Add(GenerateDocumentMacroUsage(analyzer.CellText, templateFileName, GroupName));
						}
					}
				}
			}

			return results;
		}

		protected abstract string AnalyzeResourceStringKey(string templateFileName, string groupName, string keyPrefix, string translatableText);

		protected virtual ResourceStringData GenerateResourceStringData(string key, string captionOrFullDescription, string contextClassName, string contextFile)
		{
			return new ResourceStringData(key, captionOrFullDescription, new ResourceStringMetaData(contextClassName, contextFile));
		}

		public DataWithDocumentMacroUsage<ResourceStringData>[] GetUniqueLabels(BusinessObjectFactory factory, string templateFilePath, byte[] template)
		{
			return GetUniqueLabelsInDictionary(factory, templateFilePath, template).Values.ToArray();
		}

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		protected Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> GetUniqueLabelsInDictionary(BusinessObjectFactory factory, string templateFilePath, byte[] template)
		{
			var results = new Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>();

			bool needTranslate;
			var nonContentSheetResourceString = GetUniquelabelsForNonContentSheet(factory, templateFilePath, template, out needTranslate);

			if (needTranslate)
			{
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(template);
					var avaliableWorkSheetNames = GetAvaliableWorkSheetNames(excelInterface);
					foreach (var workSheetName in avaliableWorkSheetNames)
					{
						var workSheet = excelInterface.WorkSheets.Find(workSheetName);
						var rowEnd = workSheet.RowCount - 1;
						var rowStart = GetSheetContentRowStart(workSheet, rowEnd);

						MergeDictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>(results, GetUniqueLabels(workSheet, templateFilePath, rowStart, rowEnd));
					}
				}

				if (nonContentSheetResourceString != null)
				{
					MergeDictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>(results, nonContentSheetResourceString);
				}
			}

			return results;
		}

		protected void MergeDictionary<TKey, TValue>(Dictionary<TKey, TValue> target, Dictionary<TKey, TValue> source)
		{
			foreach (var item in source)
			{
				if (!target.ContainsKey(item.Key))
				{
					target.Add(item.Key, item.Value);
				}
			}
		}

		internal string GetFileNameByPath(string filePath)
		{
			var fileName = filePath;
			if (filePath.StartsWith(MenuCustomisation.TemplatesDirectory, StringComparison.OrdinalIgnoreCase))
			{
				var extension = Path.GetExtension(filePath);
				var directoryLength = MenuCustomisation.TemplatesDirectory.Length + SubRootDirectory.Length;
				fileName = filePath.Substring(directoryLength, filePath.Length - directoryLength - extension.Length).Replace('\\', '+');
			}
			else
			{
				fileName = Path.GetFileNameWithoutExtension(filePath);
			}
			return fileName;
		}

		protected abstract string SubRootDirectory { get; }

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Development constants will only be English")]
		public Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>> GetAllResStringsUsedByBizOFields(BusinessObjectFactory factory)
		{
			var results = new Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>>();
			var functionsToAnalyze = new Dictionary<string, DataWithDocumentMacroUsage<FunctionToAnalyze>>();

			foreach (var report in ForEachReport(factory))
			{
				using (report)
				{
					var macroCalledDelegate = new MacroTranslator.MacroCalledDelegate(delegate(string macroWithoutAngleBrackets, ValueProvider valueProvider)
						{
							macroWithoutAngleBrackets = TransformFormatMacro(macroWithoutAngleBrackets);
							if (!functionsToAnalyze.ContainsKey(macroWithoutAngleBrackets) && !results.ContainsKey(macroWithoutAngleBrackets))
							{
								if (valueProvider is DBOrBOValueProvider)
								{
									var methodInfoChain = ((BusinessObjectDataProvider)report.DataProvider).FindColumn(macroWithoutAngleBrackets);
									if (methodInfoChain != null && methodInfoChain.Length > 0)
									{
										var methodInfo = methodInfoChain[methodInfoChain.Length - 1].MethodInfo;
										Type parentType = methodInfoChain[methodInfoChain.Length - 1].MethodInfo.DeclaringType;
										if (methodInfoChain.Length > 1 && !typeof(IBODocDataProviderCollectionHelper).IsAssignableFrom(parentType))
										{
											parentType = methodInfoChain[0].MethodInfo.ReturnType;
											if (typeof(IBODocDataProviderCollection).IsAssignableFrom(parentType))
											{
												parentType = methodInfoChain[methodInfoChain.Length - 1].MethodInfo.DeclaringType;
											}
											else
											{
												for (int i = 1; i < methodInfoChain.Length - 1; i++)
												{
													parentType = methodInfoChain[i].MethodInfo.ReturnType;
													if (typeof(IBODocDataProviderCollection).IsAssignableFrom(parentType))
													{
														parentType = methodInfoChain[methodInfoChain.Length - 1].MethodInfo.DeclaringType;
														break;
													}
												}
											}
										}

										if (parentType.Name == "DocumentWrapper" && methodInfo.Name == "ToString" && methodInfoChain.Length > 1)
										{
											parentType = methodInfoChain[methodInfoChain.Length - 2].MethodInfo.ReturnType;
											string defaultPropertyName = DefaultFieldAttribute.GetDefaultFieldName(parentType);
											if (!string.IsNullOrEmpty(defaultPropertyName))
											{
												var defaultProperty = parentType.GetProperty(defaultPropertyName);
												if (defaultProperty != null)
												{
													methodInfo = defaultProperty.GetGetMethod();
												}
											}
										}

										if (methodInfoChain.Length > 1 && (
												(parentType.Name == "CodeAndDescriptionWrapper" && (methodInfo.Name == "get_Description" || methodInfo.Name == "get_CodeAndDescription")) ||
												(parentType.Name == "LabelValuePairWrapper" && methodInfo.Name == "get_Label"))
										)
										{
											methodInfo = methodInfoChain[methodInfoChain.Length - 2].MethodInfo;
											parentType = methodInfoChain[methodInfoChain.Length - 2].MethodInfo.DeclaringType;
										}

										string assemblyName = parentType.Assembly.GetName().Name;
										if (CodeStringFinder.IsSupportedReturnType(methodInfo.ReturnType) && assemblyName != "mscorlib")
										{
											string typeName = parentType.FullName;
											if (typeName.IndexOf('`') > -1)
											{
												typeName = typeName.Substring(0, typeName.IndexOf('`'));
											}

											functionsToAnalyze.Add(macroWithoutAngleBrackets, new DataWithDocBuilderUsage<FunctionToAnalyze>(new FunctionToAnalyze(assemblyName, typeName, methodInfo.Name, true)));
										}
									}
									if (!functionsToAnalyze.ContainsKey(macroWithoutAngleBrackets) && !results.ContainsKey(macroWithoutAngleBrackets))
									{
										results.Add(macroWithoutAngleBrackets, new DataWithDocBuilderUsage<CodeStringFinder.ResourceStringReference[]>(Array.Empty<CodeStringFinder.ResourceStringReference>()));
									}
								}
								else
								{
									if (valueProvider is RegistryItem)
									{
										var demandedKeyDescriptionPair = new Dictionary<string, string>();
										var resourceDemanded = new EventHandler<ResourceStringDemandedEventArgs>(delegate(object sender, ResourceStringDemandedEventArgs eventArgs)
										{
											if (!demandedKeyDescriptionPair.ContainsKey(eventArgs.Key))
											{
												var captionOrDescription = ResourceStringsFactory.Lookup(Res.DefaultLanguage, eventArgs.Key)?.GetCaptionOrFullDescription();
												if (!string.IsNullOrEmpty(captionOrDescription))
												{
													demandedKeyDescriptionPair.Add(eventArgs.Key, captionOrDescription);
												}
											}
										});
										Res.ResourceDemanded += resourceDemanded;
										try
										{
											using (var tempReport = new Report(DocumentPack.EmptyPack, null))
											{
												valueProvider.GetReplacement(macroWithoutAngleBrackets, tempReport);
											}
										}
										finally
										{
											Res.ResourceDemanded -= resourceDemanded;
										}

										var resoucesStringReference = new List<CodeStringFinder.ResourceStringReference>();
										foreach (var item in demandedKeyDescriptionPair)
										{
											resoucesStringReference.Add(new CodeStringFinder.ResourceStringReference(item.Key, item.Value, null, null));
										}
										results.Add(macroWithoutAngleBrackets, new DataWithDocBuilderUsage<CodeStringFinder.ResourceStringReference[]>(resoucesStringReference.ToArray()));
									}
									else
									{
										var valueProviderType = valueProvider.GetType();
										if (excludedMacrosForResStringUsages.Contains(valueProviderType))
										{
											results.Add(macroWithoutAngleBrackets, new DataWithDocBuilderUsage<CodeStringFinder.ResourceStringReference[]>(Array.Empty<CodeStringFinder.ResourceStringReference>()));
										}
										else
										{
											var assemblyName = valueProviderType.Assembly.GetName().Name;
											var ignoreReturnType = !macroWithoutAngleBrackets.StartsWith("ReportName", StringComparison.OrdinalIgnoreCase);
											functionsToAnalyze.Add(macroWithoutAngleBrackets, new DataWithDocBuilderUsage<FunctionToAnalyze>(new FunctionToAnalyze(assemblyName, valueProviderType.FullName, "GetReplacementCore", ignoreReturnType)));
										}
									}
								}
							}

							DocumentMacroUsageCollection usages;
							if (functionsToAnalyze.ContainsKey(macroWithoutAngleBrackets))
							{
								usages = functionsToAnalyze[macroWithoutAngleBrackets].DocBuilderUsages;
							}
							else
							{
								usages = results[macroWithoutAngleBrackets].DocBuilderUsages;
							}
							usages.Add(new DocBuilderUsage("<" + macroWithoutAngleBrackets + ">", new TemplateSection(currentSection.Length > 21 ? currentSection.Substring(21) : string.Empty, 0, -1).SectionName));
						}
					);

					try
					{
						report.MacroTranslator.MacroCalled += macroCalledDelegate;
						ReplaceAllMacros(factory, report);
					}
					finally
					{
						report.MacroTranslator.MacroCalled -= macroCalledDelegate;
					}
				}
			}
			functionsToAnalyze.Add((NoResString)"Format", new DataWithDocBuilderUsage<FunctionToAnalyze>(new FunctionToAnalyze(typeof(FormatStringInterpreter).Assembly.GetName().Name, typeof(FormatStringInterpreter).FullName, "GetValue")));

			CodeStringFinder codeStringFinder = null;
			try
			{
				foreach (var pair in functionsToAnalyze.OrderBy(pair => pair.Value.Data.assemblyName))
				{
					if (codeStringFinder == null || codeStringFinder.AssemblyName != pair.Value.Data.assemblyName)
					{
						codeStringFinder?.Dispose();
						codeStringFinder = new CodeStringFinder(pair.Value.Data.assemblyName);
					}
					results[pair.Key] = new DataWithDocBuilderUsage<CodeStringFinder.ResourceStringReference[]>(codeStringFinder.FindStringUsages(pair.Value.Data.typeName, pair.Value.Data.memberName, pair.Value.Data.ignoreReturnType));
					results[pair.Key].DocBuilderUsages.AddRange(pair.Value.DocBuilderUsages);
				}
			}
			finally
			{
				codeStringFinder?.Dispose();
			}

			return results;
		}

		string currentSection;

		readonly Type[] excludedMacrosForResStringUsages = new Type[] { typeof(TranslateDBField) };

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Development constants will only be English")]
		internal void ReplaceAllMacros(BusinessObjectFactory factory, Report report)
		{
			factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			report.Renderer.CurrentAreaToProcess = AreaFactory.InstantiateArea(0, 1, report, Constants.AreaIdentifierTags.Config);

			ExcelWorkSheet currentWorkSheet = report.WorkSheetCurrentlyBeingProcessed;

			currentSection = (NoResString)"#Config";

			for (int row = 0; row < currentWorkSheet.RowCount; row++)
			{
				string headerContent = currentWorkSheet[row, 0] as string;
				if (!string.IsNullOrEmpty(headerContent) && headerContent.ToUpperInvariant().StartsWith(Constants.AreaIdentifierTags.ConfigurableSection, StringComparison.OrdinalIgnoreCase))
				{
					currentSection = headerContent;
				}

				for (int col = 0; col < currentWorkSheet.ColumnCount; col++)
				{
					object cellContent = currentWorkSheet[row, col];
					using (report.ErrorManager.EvaluatingCell(new CellReference(currentWorkSheet.SheetName + " - " + currentSection, row, col)))
					{
						TFormula cellFormula = cellContent as TFormula;
						if (cellFormula != null)
						{
							cellContent = cellFormula.Text;
						}

						string cellContentText = cellContent.ToString();

						if (!String.IsNullOrEmpty(cellContentText) && !cellContentText.StartsWith("#SectionBody:Data", StringComparison.OrdinalIgnoreCase))
						{
							using (report.ErrorManager.EvaluatingOuterContent(cellContent))
							{
								CellContentReplacer cellContentReplacer = new CellContentReplacer(report, cellContent);
								if (cellContentReplacer.StillContainsAtLeastOneMacro)
								{
									cellContentReplacer.ReplaceMacros();
								}
							}
						}
					}
				}
			}
		}

		internal string TransformFormatMacro(string macroWithoutAngleBrackets)
		{
			var match = formatMacroMatch.Match(macroWithoutAngleBrackets);
			if (match.Success)
			{
				macroWithoutAngleBrackets = macroWithoutAngleBrackets.Substring(0, match.Index) + "." + match.Groups[1].Value;
			}
			return macroWithoutAngleBrackets;
		}
		readonly Regex formatMacroMatch = new Regex(@"\.Format\(""\{(.*?)\}[^\)]*\)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		public abstract IEnumerable<Report> ForEachReport(BusinessObjectFactory factory);

		protected class FunctionToAnalyze
		{
			public FunctionToAnalyze(string assemblyName, string typeName, string memberName, bool ignoreReturnType = false)
			{
				this.assemblyName = assemblyName;
				this.typeName = typeName;
				this.memberName = memberName;
				this.ignoreReturnType = ignoreReturnType;
			}

			public readonly string assemblyName;
			public readonly string typeName;
			public readonly string memberName;
			public readonly bool ignoreReturnType;
		}

		public abstract List<string> GetAvaliableWorkSheetNames(ExcelInterface excelInterface);

		[SuppressMessage("Microsoft.Design", "CA1006:Do not nest generic types in member signatures")]
		[SuppressMessage("Microsoft.Design", "CA1021")]
		public virtual Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> GetUniquelabelsForNonContentSheet(BusinessObjectFactory factory, string templateFilePath, byte[] template, out bool needTranslate)
		{
			needTranslate = true;
			return null;
		}

		public abstract int GetSheetContentRowStart(ExcelWorkSheet workSheet, int rowEnd);

		public abstract string KeyPrefix { get; }

		protected string GroupName { get; set; }

		public abstract DataWithDocumentMacroUsage<ResourceStringData> GenerateDataWithDocumentMacroUsage(ResourceStringData data);
		public abstract DocumentMacroUsage GenerateDocumentMacroUsage(string macro, string fileName, string templateName);

		public abstract string AnalyseGroupName(ExcelWorkSheet workSheet, ExcelCell cell, int row);
	}
}
