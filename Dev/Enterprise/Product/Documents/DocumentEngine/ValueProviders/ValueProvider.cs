using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.Integration.DocumentEngine;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine
{
	public enum Passes { FirstPass, SecondPass }

	public abstract class ValueProvider : IVisualiserComponentProvider, ITextMacroValueProvider
	{
		protected ValueProvider()
		{
		}

		protected internal virtual void PreSetupForGettingValue(string macro, Passes pass, Report report)
		{
		}

		public bool IsResponsibleForReplacing(string macro, Passes currentPass)
		{
			return IsResponsibleForReplacingCore(macro, currentPass);
		}

		protected virtual bool IsResponsibleForReplacingCore(string macro, Passes currentPass)
		{
			return PassToStartReplacingOn <= currentPass && Regex.IsMatch(macro);
		}

		public bool ShouldReplaceNestedMacrosWhileIrrisponsible(string macro, Passes currentPass)
		{
			return ShouldReplaceNestedMacrosWhileIrrisponsibleCore(macro, currentPass);
		}

		protected virtual bool ShouldReplaceNestedMacrosWhileIrrisponsibleCore(string macro, Passes currentPass)
		{
			return false;
		}

		public virtual bool IsDocumentationVisible => true;

		public virtual bool CanBeEvaluatedWithoutCurrentWorkSheet => true;

		internal virtual bool ShouldEscapeAngleBrackets => true;

		protected abstract object GetReplacementCore(string macro, Report report);
		protected virtual void PerformPreReplacementSetup()
		{
		}

		public object GetReplacement(string macro, Report report)
		{
			if (report == null)
			{
				throw new ArgumentNullException(nameof(report), "You must specify the Report you are evaluating this macro on.");
			}
			if (macro == null)
			{
				throw new ArgumentNullException(nameof(macro), "You must supply a macro to replace. Otherwise there's no point trying to get a replacement, there's nothing to replace!");
			}
			if (!CanBeEvaluatedWithoutCurrentWorkSheet && report.WorkSheetCurrentlyBeingProcessed == null)
			{
				throw new ArgumentException(FormattableString.Invariant($"Macro {macro} can not be evaluated without a real template."));
			}

			using (Db.DisposableActionForDbConnection())
			{
				object value = null;
				using (report.ErrorManager.EvaluatingInnerMacro(macro))
				{
					PerformPreReplacementSetup();
					value = GetReplacementCore(macro, report);
					value = LocalizeValue(value, report);
					if (value is string)
					{
						value = ((string)value).HandleBrokenFontsSymbols(report);
					}
					else if (value is ZString)
					{
						value = new ZString(value.ToString().HandleBrokenFontsSymbols(report));
					}
				}
				return value;
			}
		}

		internal string ReplaceNestedMacro(Match match, IMacroTranslator macroTranslator, Passes currentPass)
		{
			return ReplaceNestedMacro(match.Groups[0].Value, macroTranslator, currentPass);
		}

		protected virtual bool NeedCheckForScientificNotation => false;

#if DEBUG
		internal bool NeedCheckForScientificNotationExposed()
		{
			return NeedCheckForScientificNotation;
		}
#endif

		protected virtual string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			var result = macroTranslator.GetValue(macro, currentPass) ?? string.Empty;

			if (result is TFormula formula)
			{
				var formulaText = formula.Text;
				if (PassNestedMacroFormulaAsText)
				{
					return formulaText;
				}
				result = macroTranslator.GetFormulaResult(formulaText);
			}

			if (NeedCheckForScientificNotation && (result is double || result is float))  // Need to parse to normal format when it is to be evaluated by ExpressionEvaluator
			{
				result = decimal.Parse(result.ToString(), NumberStyles.Float).ToString(CultureInfo.InvariantCulture.NumberFormat);
			}
			return result.ToString();
		}

		protected virtual bool PassNestedMacroFormulaAsText { get { return false; } }

		protected void ReportMacroError(Report report, string exceptionMessage, ReportProcessingErrorSeverity severity = ReportProcessingErrorSeverity.Warning)
		{
			var fullMacro = report.ErrorManager.OuterContent;
			var message = "";
			if (fullMacro != null)
			{
				message = Res.GetString("52f70732-417b-4cc3-9525-572e191eb2fd", "Error in {0} Macro: {1} Input macro: [{2}]", GetType().Name, exceptionMessage, fullMacro);
			}
			else
			{
				message = Res.GetString("C1B3FF35-BF19-4865-9F80-1C812FB0222F", "Error in {0} Macro: {1}", GetType().Name, exceptionMessage);
			}
			var error = new ReportProcessingError(message, severity);
			report.ErrorManager.Add(error);
		}

		protected string GetLanguageCodeErrorMessage(string macro, string oldLanguageCode, string newLanguageCode) => Res.GetString("8367e787-c78f-4933-a46d-44562d404b5d"
			, "Error when processing the macro {0}: the language code {1} has been depreciated. Please use the ISO language code instead: {2}."
			, macro, oldLanguageCode, newLanguageCode);

		object LocalizeValue(object value, Report report)
		{
			IMultilingual multiLingual = value as IMultilingual;
			if (multiLingual != null)
			{
				value = multiLingual.GetLocalizedValue(report.Language);
			}
			return value;
		}

		readonly object lockObj = new object();

		public void Reset()
		{
			lock (lockObj)
			{
				ResetCore();
			}
		}

		protected virtual void ResetCore() { }

		public abstract Regex Regex { get; }

		public virtual Passes PassToStartReplacingOn
		{
			get { return Passes.FirstPass; }
		}

		public bool ShouldEvaluateInnerMacros(string macro)
		{
			return ShouldEvaluateInnerMacrosCore(macro);
		}

		protected virtual bool ShouldEvaluateInnerMacrosCore(string macro)
		{
			return true;
		}

		public virtual bool EvaluateAllInnerMacrosWhenGettingReplacement => false;

		internal void IncrementUsageCount()
		{
			usageCount++;
		}

		internal int GetUsageCount()
		{
			return usageCount;
		}

		int usageCount;

		public virtual VisualiserComponentTypes ComponentType
		{
			get { return VisualiserComponentTypes.StaticText; }
		}

		public ValueProviderDocumenter Documentation
		{
			get { return GetDocumentation(); }
		}
		protected abstract ValueProviderDocumenter GetDocumentation();

		protected virtual ZString AdditionalDocumentation
		{
			get
			{
				var result = ZString.Empty;

				if (this is INonVisualisableValueProviderThatModifyDocumentLayout)
				{
					var tFormulaMacros = new List<ZString>();

					new ValueProviderCollector().ValueProviders.Providers.Where(v => v is ITFormulaProvider).ForEach(v =>
					{
						tFormulaMacros.Add(v.Documentation.Useage);
					});

					result = System.Environment.NewLine + System.Environment.NewLine + ResString.GetMultilingualString("a657d8c9-4369-4d4e-8525-e5ef91a9c3c1", @"This macro will not work if it is used together with {0} macro ({1}).", "TFormula", string.Join(",", tFormulaMacros)) + System.Environment.NewLine + System.Environment.NewLine;
				}

				return result;
			}
		}

		protected T TryParseWithReportOnFail<T>(Report report, T defaultValue, Func<T> parseFunc, string errorMessage = null, bool useExceptionMessage = true)
		{
			try
			{
				return parseFunc();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				if (!useExceptionMessage)
				{
					ReportMacroError(report, errorMessage);
				}
				else if (!string.IsNullOrEmpty(errorMessage))
				{
					ReportMacroError(report, string.Format(CultureInfo.InvariantCulture, errorMessage, e.Message));
				}
				else
				{
					ReportMacroError(report, e.Message);
				}

				return defaultValue;
			}
		}
	}
}
