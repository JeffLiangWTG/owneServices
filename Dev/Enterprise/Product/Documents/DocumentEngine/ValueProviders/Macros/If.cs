using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class If : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<If({LogicalTest}, \"{ValueIfTrue}\", \"{ValueIfFalse}\")>",
				ResString.GetMultilingualString("4dbb4886-51ba-4fd6-8324-f0d7bc9f74cf", @"This macro evaluates the expression in {0} and returns {1} or {2} depending on the result.
The Document Engine uses Microsoft JScript to evaluate the {0}, so any JScript expressions that evaluate to a boolean result will work.
with the exception of greater than or less than.
To do 'greater than' use '{3}' instead of '>'.
To do 'greater than or equals to' use '{3}=' instead of '>='.
To do 'less than' use '{4}' instead of '<'.
To do 'less than or equals to' use '{4}=' instead of '<='.",
"{LogicalTest}", "{ValueIfTrue}", "{ValueIfFalse}", "&gt;", "&lt;"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<If(<CurrentPage>&gt;2, \"other pages\", \"page 1 to 2\")>", (NoResString)"page 1 to 2"),
					((NoResString)"<If(<CurrentPage>&lt;=1, \"page 1\", \"some other page\")>", (NoResString)"page 1") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			object result = "";
			try
			{
				result = GetReplace(macro, report);
			}
			catch (ExpressionEvaluationException ex)
			{
				ReportMacroError(report, ex.Message);
			}
			return result;
		}

		/// <summary>
		/// This method is to make If macro only evaluate the match condition.
		/// Coz If macro might have nested If macro or some other macros, and we cannot write a perfect Regex to match this situation, so we do below:
		/// 1. Replace the innermost(complex) macro to a placeholder (something like {12345678}) to make the macro can be matched by If Regex
		/// 2. Evaluate the LogicalTest group of macro, get the logicalValue
		/// 3. Translate the ReturnValue according to above logicalValue
		/// </summary>
		object GetReplace(string macro, Report report)
		{
			var stack = new Stack<(string realValue, string replacedValue)>();
			var simplifiedMacro = SimplifyMacros(macro);
			var logicalTest = regex.Match(simplifiedMacro).Groups["LogicalTest"].Value;
			var logicalValue = GetLogicalValueLocal(logicalTest);

			if (logicalValue.shouldBeDelayedToSecondPass)
			{
				return macro;
			}

			var valueIfTrue = regex.Match(simplifiedMacro).Groups["ValueIfTrue"].Value;
			var valueIfFalse = regex.Match(simplifiedMacro).Groups["ValueIfFalse"].Value;

			var returnedResult = GetReturnValueLocal(logicalValue.result ? valueIfTrue : valueIfFalse);

			if (returnedResult.shouldBeDelayedToSecondPass)
			{
				return macro;
			}

			if (returnedResult.result is string || returnedResult.result is ZString)
			{
				return returnedResult.result.ToString().UnEscapeForJScript();
			}

			return returnedResult.result;

			(object result, bool shouldBeDelayedToSecondPass) GetReturnValueLocal(string returnMacroLocal)
			{
				var restoredReturnMacro = RestoreMacros(returnMacroLocal);
				var replacedMacroForReturn = ReplaceMacros(restoredReturnMacro, isReturnValue: true);
				return replacedMacroForReturn;
			}

			(bool result, bool shouldBeDelayedToSecondPass) GetLogicalValueLocal(string logicalMacroLocal)
			{
				var restoredLogicalMacro = RestoreMacros(logicalMacroLocal);
				var replacedMacroForLogical = ReplaceMacros(restoredLogicalMacro, true, true);
				var logicalValueResult = replacedMacroForLogical.shouldBeDelayedToSecondPass || ExpressionEvaluator.Evaluate(replacedMacroForLogical.result.ToString(), report.UseJsEvaluator);

				return (logicalValueResult, replacedMacroForLogical.shouldBeDelayedToSecondPass);
			}

			(object result, bool shouldBeDelayedToSecondPass) ReplaceMacros(string macroToBeReplaced, bool needCheckForScientificNotation = false, bool isLogicalValue = false, bool isReturnValue = false)
			{
				object objResult = null;
				var resultOfReplaced = macroToBeReplaced;
				var shouldBeDelayedToSecondPass = false;
				ReplaceMacrosLocal(resultOfReplaced);

				if (isReturnValue && objResult != null && !(objResult is string || objResult is ZString))
				{
					return (objResult, shouldBeDelayedToSecondPass);
				}

				return (resultOfReplaced, shouldBeDelayedToSecondPass);

				void ReplaceMacrosLocal(string macroToBeReplacedLocal)
				{
					var firstMatches = RegexProvider.OutermostMacroRegex.Matches(macroToBeReplacedLocal).OfType<Match>().ToList();

					foreach (var match in firstMatches)
					{
						var escapeForJs = true;
						var realValue = report.MacroTranslator.GetValue(match.Value, report.Renderer.CurrentPass);
						if (realValue is TFormula formula)
						{
							realValue = report.MacroTranslator.GetFormulaResult(formula.Text);
						}

						if (needCheckForScientificNotation && (realValue is double || realValue is float)) // Need to parse to normal format when it is to be evaluated by ExpressionEvaluator
						{
							realValue = decimal.Parse(realValue.ToString(), NumberStyles.Float).ToString(CultureInfo.InvariantCulture.NumberFormat);
						}

						if (!(realValue is string || realValue is ZString) && firstMatches.Count == 1 && match.Value == macroToBeReplacedLocal)
						{
							objResult = realValue;
						}

						var realValueString = realValue.ToString();

						if (isLogicalValue)
						{
							realValueString = realValueString.UnEscapeAngleBrackets();
						}

						if (RegexProvider.OutermostMacroRegex.IsMatch(realValueString))
						{
							if (realValueString == match.Value && report.Renderer.CurrentPass == Passes.FirstPass)
							{
								shouldBeDelayedToSecondPass = true;
								return;
							}

							var providerCache = new ValueProviderCollector().ValueProviders;
							var originalValueProvider = report.MacroTranslator.GetValueProvider(report.Renderer.CurrentPass, match.Value);
							var currentValueProvider = report.MacroTranslator.GetValueProvider(report.Renderer.CurrentPass, realValueString);

							if (originalValueProvider != null && originalValueProvider == currentValueProvider && providerCache.Providers.Select(p => p.GetType()).Contains(originalValueProvider.GetType()))
							{
								if (report.Renderer.CurrentPass == Passes.FirstPass)
								{
									shouldBeDelayedToSecondPass = true;
									return;
								}

								escapeForJs = false;
							}
						}

						resultOfReplaced = resultOfReplaced.Replace(match.Value, escapeForJs ? realValueString.EscapeForJScript() : realValueString);
					}

					//Second match for the nested macros
					var secondMatches = RegexProvider.OutermostMacroRegex.Matches(resultOfReplaced).OfType<Match>().ToList();
					bool AreEqual(Match l, Match r) => l.Value == r.Value;
					if (secondMatches.Any() && !secondMatches.ContainsSameElementsInAnyOrder(firstMatches, AreEqual))
					{
						ReplaceMacrosLocal(resultOfReplaced);
					}
				}
			}

			string SimplifyMacros(string macroToBeSimplified)
			{
				stack.Clear();
				var resultOfSimplified = macroToBeSimplified.StripOutMostAngleBrackets();
				SimplifyMacrosLocal(resultOfSimplified);
				return "<" + resultOfSimplified + ">";

				void SimplifyMacrosLocal(string macroToBeSimplifiedLocal)
				{
					var firstMatches = RegexProvider.InnermostMacrosRegex.Matches(macroToBeSimplifiedLocal);
					foreach (Match match in firstMatches)
					{
						resultOfSimplified = resultOfSimplified.Replace(match.Value, $"{{{match.GetHashCode()}}}");
						stack.Push((match.Value, $"{{{match.GetHashCode()}}}"));
					}

					if (RegexProvider.InnermostMacrosRegex.Matches(resultOfSimplified).OfType<Match>().Any())
					{
						SimplifyMacrosLocal(resultOfSimplified);
					}
				}
			}

			string RestoreMacros(string macroToBeRestored)
			{
				var newStack = new Stack<(string realValue, string replacedValue)>(stack.Reverse());
				while (newStack.Count > 0)
				{
					var pop = newStack.Pop();
					macroToBeRestored = macroToBeRestored.Replace(pop.replacedValue, pop.realValue);
				}
				return macroToBeRestored;
			}
		}

		protected override bool ShouldEvaluateInnerMacrosCore(string macro)
		{
			return false;
		}

		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(
				@"^<(?:[\s]*)If(?:[\s]*)\((?:[\s]*)(?<LogicalTest>.+)(?:[\s]*)\,(?:[\s]*)""(?<ValueIfTrue>.*)""(?:[\s]*)\,(?:[\s]*)""(?<ValueIfFalse>.*)""(?:[\s]*)\)(?:[\s]*)>$",
				RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.Singleline);

		public override VisualiserComponentTypes ComponentType => VisualiserComponentTypes.TextEdit;
	}
}
