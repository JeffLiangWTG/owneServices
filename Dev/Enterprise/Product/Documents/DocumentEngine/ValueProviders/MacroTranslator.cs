using Enterprise.DocumentEngine.MacroValueProviders;

namespace Enterprise.DocumentEngine
{
	using System.Collections.Generic;
	using System.Text.RegularExpressions;
	using CargoWise.Types;
	using DataProviders;
	using DocumentEngineCore;
	using Exceptions;

	sealed class MacroTranslator : IMacroTranslator
	{
		internal MacroTranslator(Report report)
		{
			this.report = report;

			if (report != null && report.IsInTaskBuild)
			{
				providerCache = new ProviderCache();
			}
			else if (report != null && report.IsReportForTextMacroProcessor)
			{
				providerCache = ProviderCache.InitialValueProvidersCache.Clone();
			}
			else
			{
				providerCache = new ValueProviderCollector().ValueProviders;
			}
		}

		readonly Report report;

#if DEBUG
		internal
#endif
		readonly ProviderCache providerCache;
		readonly List<ValueProvider> usedProviders = new List<ValueProvider>();

		internal void ResetProviders()
		{
			foreach (var provider in providerCache.Providers)
			{
				provider.Reset();
			}
		}

		internal void ResetUsedProviders()
		{
			foreach (var provider in usedProviders)
			{
				provider.Reset();
			}
			usedProviders.Clear();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public object GetValue(string macro, Passes pass, bool shouldSkipEscapingAngleBrackets = false)
		{
			string macroWithAngleBrackets = macro.Trim();
			string macroWithoutAngleBrackets = macroWithAngleBrackets.Length > 2 ? macroWithAngleBrackets.Substring(1, macroWithAngleBrackets.Length - 2) : "";
			var isInnermostMacro = !RegexProvider.OutermostMacroRegex.IsMatch(macroWithoutAngleBrackets);
			ValueProvider valueProvider = GetValueProvider(pass, macroWithAngleBrackets);
			valueProvider?.PreSetupForGettingValue(macro, pass, report);
			bool isValueProviderOnlyWorkForNestedMacros = (valueProvider != null && valueProvider.ShouldReplaceNestedMacrosWhileIrrisponsible(macroWithAngleBrackets, pass));
			object result = "";

			if (report != null)
			{
				using (report.ErrorManager.EvaluatingOuterContent(macro))
				{
					if (!RegexProvider.TotalMacroRegex.IsMatch(macro) && (valueProvider != null && valueProvider.ShouldEvaluateInnerMacros(macro)))
					{
						string lastReplacedMacro = "";
						while (lastReplacedMacro != macroWithoutAngleBrackets)
						{
							lastReplacedMacro = macroWithoutAngleBrackets;
							if (RegexProvider.OutermostMacroRegex.IsMatch(macroWithoutAngleBrackets))
							{
								var evaluator = new MatchEvaluator(match => valueProvider.ReplaceNestedMacro(match, this, pass));

								macroWithoutAngleBrackets = RegexProvider.OutermostMacroRegex.Replace(macroWithoutAngleBrackets, evaluator);
								macroWithAngleBrackets = "<" + macroWithoutAngleBrackets + ">";
							}
						}
						if (isValueProviderOnlyWorkForNestedMacros)
						{
							valueProvider = null;
						}
					}

					if (RegexProvider.InnermostMacrosRegex.IsMatch(macroWithoutAngleBrackets)
						&& pass == Passes.FirstPass
						&& !(valueProvider is If)
						&& (valueProvider == null || !valueProvider.EvaluateAllInnerMacrosWhenGettingReplacement))
					{
						result = macroWithAngleBrackets;
					}
					else
					{
						if (valueProvider != null)
						{
							if (!report.TryGetCachingService(out var cachingService) || !cachingService.TryGetMacroValue(macroWithAngleBrackets, out result))
							{
								OnMacroCalled(macroWithoutAngleBrackets, valueProvider);

								result = valueProvider.GetReplacement(macroWithAngleBrackets, report);
								usedProviders.Add(valueProvider);
								if ((result is string || result is ZString) && !shouldSkipEscapingAngleBrackets && isInnermostMacro)
								{
									if ((report.Renderer.CurrentPass != Passes.FirstPass || !(valueProvider is If) || !valueProvider.Regex.IsMatch(result.ToString())) && valueProvider.ShouldEscapeAngleBrackets)
									{
										var hasHtmlBreakLine = RegexProvider.HtmlNewLineTagMacroRegex.IsMatch(result.ToString());
										result = result.ToString().EscapeAngleBrackets();

										if (hasHtmlBreakLine && !report.Renderer.IsProcessingMacros)
										{
											result = result.ToString().UnEscapeHtmlBreakLineAngleBrackets();
										}
									}
								}

								cachingService?.CacheMacroValue(macroWithAngleBrackets, result);
							}
						}
						else if (pass == Passes.FirstPass)
						{
							result = macroWithAngleBrackets;
						}
						else
						{
							if (macroWithoutAngleBrackets.Trim('.', ' ') == MasterFiles.Business.MacroDataSource.Constants.Prefix)
							{
								FieldNotFoundException.ReportDataSourcePrefixError(MasterFiles.Business.MacroDataSource.EmptyDataSourceTypeError);
							}
							else
							{
								DataProviderList topLevelDataSource = null;

								if (report.DataProvider is BusinessObjectDataProvider boDataProvider)
								{
									topLevelDataSource = boDataProvider.TopLevelDataSources;
								}
								FieldNotFoundException.ReportFieldNotFound(macroWithAngleBrackets, topLevelDataSource);
							}
							result = "";
						}

						if (result is IZType type && !type.IsValid || result == null)
						{
							result = "";
						}
					}
				}
			}
			return result;
		}

		internal ValueProvider GetValueProvider(Passes pass, string macro)
		{
			ValueProvider provider = providerCache.GetProviderResponsibleForIncludingDBProvider(macro, pass, report);
			return provider;
		}

		public object GetFormulaResult(string expression)
		{
			return report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.Xls.RecalcExpression(expression);
		}

		internal void RegisterValueProvider(ValueProvider provider)
		{
			providerCache.AddProvider(provider);
		}

		void OnMacroCalled(string macroWithoutAngleBrackets, ValueProvider valueProvider)
		{
			if (MacroCalled != null)
			{
				MacroCalled(macroWithoutAngleBrackets, valueProvider);
			}
		}

		internal delegate void MacroCalledDelegate(string macroWithoutAngleBrackets, ValueProvider valueProvider);

		internal event MacroCalledDelegate MacroCalled;
	}
}
