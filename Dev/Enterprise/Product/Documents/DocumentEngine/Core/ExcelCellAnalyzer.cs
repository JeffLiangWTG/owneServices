using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Visualisation;

namespace Enterprise.DocumentEngine
{
	internal class ExcelCellAnalyzer
	{
		readonly ExcelCell cell;

		public ExcelCellAnalyzer(ExcelCell cell)
		{
			this.cell = cell;
			this.translatableRoot = new TranslatablePart(cell.ValueSourceText.Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine));
		}

		internal ExcelCellAnalyzer(string cellValueSourceText)
		{
			this.translatableRoot = new TranslatablePart(cellValueSourceText.Replace(System.Environment.NewLine, "\n").Replace("\n", System.Environment.NewLine));
		}

		static readonly Regex stringBlobRegex = new Regex("\"[^\"]*?\"", RegexOptions.Compiled);
		static readonly Regex formatFunctionRegex = new Regex(@"<\s*[^<]*?(?<FormatFunction>Format\((?<ParameterString>.*?)\))\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex totalFunctionRegex = new Regex(@"<\s*[^<]+?.Total\(\s*(?<FieldToTotal>\""[^\""]+\""){1}\s*,?\s*(?<DecimalPlaces>[^,]+)?\s*,?\s*(?<Filter>[^,]+)?\s*\)\s*>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex booleanExpressionsRegex1 = new Regex(@"""?<(?<CodeMacro>[\w\.\[\]\(\)\{\}'""\s]+)>""?\s*(?<Comparator>=|==|!=|<|>|<=|>=)\s*""(?<Value>\w+)""", RegexOptions.Compiled);
		static readonly Regex booleanExpressionsRegex2 = new Regex(@"\""?\{.*\}\""?\s*(?<Comparator>=|==)\s*""[YN]""", RegexOptions.Compiled);
		static readonly Regex numberedParameterRegex = new Regex(@"\{[0-9]+\}", RegexOptions.Compiled);
		static readonly Regex macrosWithDocDataValueParameters = new Regex(@"^\<\s*DocDataValue\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex macrosWithFindFunctionParameters = new Regex(@"^\<[\s\w\.]+\.Find\s*\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		static readonly Regex formulaRegex = new Regex(@"<\w+\(", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		bool IsFormula
		{
			get { return (cell != null && cell.IsFormula) || (cell == null && TranslatableRoot.TranslatableText.StartsWith("=")); }
		}

		public TranslatablePart[] GetTextToBeTranslated()
		{
			var translatable = TranslatableRoot;
			translatable = RemoveMacrosUsedForFormattingOnly(translatable);
			if (ModifiableField.MacroRegex.IsMatch(translatable.TranslatableText) || CustomisedColumn.MacroRegex.IsMatch(translatable.TranslatableText.Trim()))
			{
				return Array.Empty<TranslatablePart>();
			}
			var result = new List<TranslatablePart>();
			translatable = RemoveModifiableMacro(translatable);
			translatable = ParseFormatFunctions(translatable, result);
			translatable = RemoveCellReferencesInFormulas(translatable);
			translatable = ParseTotalFunctions(translatable);
			translatable = RemoveNonTranslatableBooleanExpressions(translatable);
			translatable = RemoveBooleanExpressionsForTypesAndCodes(translatable);
			translatable = StripMacros(translatable, result);
			if (IsFormula || formulaRegex.IsMatch(translatable.TranslatableText))
			{
				result = new List<TranslatablePart>(Normalize(result.ToArray()));
				ParseCellFormula(translatable, result);
				return Trim(result.ToArray());
			}
			else
			{
				if (translatable.TranslatableText.ContainsLetters() || (translatable.TranslatableText.ContainsInnerWhitespace() && translatable.CellText.Contains("<ReportName")) || Res.IsRightToLeft(Res.CurrentLanguage))
				{
					result.Add(translatable);
				}
				return Normalize(result.ToArray());
			}
		}

#if DEBUG
		public
#endif
		TranslatablePart TranslatableRoot
		{
			get { return translatableRoot; }
		}
		readonly TranslatablePart translatableRoot;

		int replacementCount;

		TranslatablePart RemoveMacrosUsedForFormattingOnly(TranslatablePart translatable)
		{
			var regexes = NonVisualisableMacroCleaner.RegexesToRemove;
			Match match = Match.Empty;
			do
			{
				foreach (var regex in regexes)
				{
					match = regex.Match(translatable.TranslatableText);
					if (match.Success)
					{
						if (match.Index == 0)
						{
							translatable = new SubstringPart(translatable, match.Length, translatable.TranslatableText.Length - match.Length);
						}
						else if (match.Index + match.Length == translatable.TranslatableText.Length)
						{
							translatable = new SubstringPart(translatable, 0, match.Index);
						}
						else
						{
							translatable = new ReplacerPart(translatable, match.Index, match.Length, replacementCount++);
						}
						break;
					}
				}
			} while (match.Success);
			return translatable;
		}

		TranslatablePart RemoveBooleanExpressionsForTypesAndCodes(TranslatablePart translatable)
		{
			var match = booleanExpressionsRegex1.Match(translatable.TranslatableText);
			while (match.Success)
			{
				var valueGroup = match.Groups["Value"];
				if (valueGroup.Success)
				{
					translatable = new ReplacerPart(translatable, valueGroup.Index, valueGroup.Length, replacementCount++);
				}
				match = booleanExpressionsRegex1.Match(translatable.TranslatableText);
			}
			return translatable;
		}

		TranslatablePart RemoveCellReferencesInFormulas(TranslatablePart translatable)
		{
			if (IsFormula)
			{
				var match = ExcelCell.CellReferenceRegex.Match(translatable.TranslatableText);
				while (match.Success)
				{
					translatable = new ReplacerPart(translatable, match.Index, match.Length, replacementCount++);
					match = ExcelCell.CellReferenceRegex.Match(translatable.TranslatableText);
				}
			}
			return translatable;
		}

		TranslatablePart RemoveNonTranslatableBooleanExpressions(TranslatablePart translatable)
		{
			var match = booleanExpressionsRegex2.Match(translatable.TranslatableText);
			while (match.Success)
			{
				translatable = new ReplacerPart(translatable, match.Index, match.Length, replacementCount++);
				match = booleanExpressionsRegex2.Match(translatable.TranslatableText);
			}
			return translatable;
		}

		TranslatablePart StripMacros(TranslatablePart translatable, List<TranslatablePart> translatableParameters)
		{
			var match = RegexProvider.InnermostMacrosRegex.Match(translatable.TranslatableText);
			while (match.Success)
			{
				translatable = new ReplacerPart(translatable, match.Index, match.Length, replacementCount++);
				var matchPart = ((ReplacerPart)translatable).ReplacedPart;

				if (CanTranslateParameters(match.Value))
				{
					var stringMatch = stringBlobRegex.Match(matchPart.TranslatableText);
					while (stringMatch.Success)
					{
						matchPart = new ReplacerPart(matchPart, stringMatch.Index, stringMatch.Length, replacementCount++);
						if (stringMatch.Value.ContainsLetters() || Res.IsRightToLeft(Res.CurrentLanguage))
						{
							translatableParameters.Add(new SubstringPart(((ReplacerPart)matchPart).ReplacedPart, 1, stringMatch.Length - 2));
						}
						stringMatch = stringBlobRegex.Match(matchPart.TranslatableText);
					}
				}

				match = RegexProvider.InnermostMacrosRegex.Match(translatable.TranslatableText);
			}

			return translatable;
		}

		bool CanTranslateParameters(string matchText)
		{
			return !(macrosWithDocDataValueParameters.IsMatch(matchText)
					 || macrosWithFindFunctionParameters.IsMatch(matchText));
		}

		TranslatablePart RemoveModifiableMacro(TranslatablePart translatable)
		{
			var modifiableMacroMatch = Modifiable.MacroRegex.Match(translatable.TranslatableText);
			if (modifiableMacroMatch.Success)
			{
				var groupMatch = modifiableMacroMatch.Groups[Modifiable.MacrosGroupTag];
				if (groupMatch.Success)
				{
					translatable = new SubstringPart(translatable, groupMatch.Index, groupMatch.Length);
				}
			}
			return translatable;
		}

		TranslatablePart ParseTotalFunctions(TranslatablePart translatable)
		{
			var match = totalFunctionRegex.Match(translatable.TranslatableText);

			while (match.Success)
			{
				var fieldToTotalStringGroup = match.Groups["FieldToTotal"];
				if (fieldToTotalStringGroup.Success)
				{
					translatable = new ReplacerPart(translatable, fieldToTotalStringGroup.Index, fieldToTotalStringGroup.Length, replacementCount++);
				}
				match = totalFunctionRegex.Match(translatable.TranslatableText);
			}

			return translatable;
		}

		TranslatablePart ParseFormatFunctions(TranslatablePart translatable, List<TranslatablePart> translatableParameters)
		{
			var match = formatFunctionRegex.Match(translatable.TranslatableText);
			while (match.Success)
			{
				translatable = new ReplacerPart(translatable, match.Index, match.Length, replacementCount++);
				var formatFunctionPart = ((ReplacerPart)translatable).ReplacedPart;
				var parameterStringGroup = match.Groups["ParameterString"];
				if (parameterStringGroup.Success && !string.IsNullOrEmpty(parameterStringGroup.Value))
				{
					var formatStringGroup = FormatFunctionExtractor.ParamsRegex.Match(parameterStringGroup.Value).Groups["FormatString"];
					if (formatStringGroup.Success && HasTextOtherThanFieldPlaceholders(formatStringGroup.Value))
					{
						TranslatablePart translatableParameter = new ReplacerPart(formatFunctionPart, formatStringGroup.Index - match.Index + parameterStringGroup.Index + +1, formatStringGroup.Length - 2, replacementCount++).ReplacedPart;
						var fieldMatch = FormatStringInterpreter.FieldNamesRegex.Match(translatableParameter.TranslatableText);
						int fieldCount = 0;
						while (fieldMatch.Success)
						{
							translatableParameter = new ReplacerPart(translatableParameter, fieldMatch.Index, fieldMatch.Length, fieldCount++);
							fieldMatch = FormatStringInterpreter.FieldNamesRegex.Match(translatableParameter.TranslatableText, fieldMatch.Index + 1);
						}
						translatableParameter = StripMacros(translatableParameter, translatableParameters);
						translatableParameters.Add(translatableParameter);
					}
				}
				match = formatFunctionRegex.Match(translatable.TranslatableText);
			}
			return translatable;
		}

		bool HasTextOtherThanFieldPlaceholders(string text)
		{
			bool inBracket = false;
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '{')
				{
					inBracket = true;
				}
				else if (text[i] == '}')
				{
					inBracket = false;
				}
				else if (!inBracket && char.IsLetter(text[i]))
				{
					return true;
				}
			}
			return false;
		}

		void ParseCellFormula(TranslatablePart translatable, List<TranslatablePart> translatableParameters)
		{
			bool oneMatch = false;
			var stringMatch = stringBlobRegex.Match(translatable.TranslatableText);
			while (stringMatch.Success)
			{
				oneMatch = true;
				translatable = new ReplacerPart(translatable, stringMatch.Index, stringMatch.Length, replacementCount++);
				if (stringMatch.Value.ContainsLetters() || Res.IsRightToLeft(Res.CurrentLanguage))
				{
					TranslatablePart part = new SubstringPart(((ReplacerPart)translatable).ReplacedPart, 1, stringMatch.Length - 2);
					part = SwapWhitespaceIfRightToLeft(part);
					translatableParameters.Add(NormalizeNumberedParameters(new[] { part })[0]);
				}
				stringMatch = stringBlobRegex.Match(translatable.TranslatableText);
			}
			if (oneMatch && Res.IsRightToLeft(Res.CurrentLanguage))
			{
				if (translatable.TranslatableText.StartsWith("=CONCATENATE("))
				{
					translatable = new SubstringPart(translatable, "=CONCATENATE(".Length, translatable.TranslatableText.Length - "=CONCATENATE(".Length - 1);
				}
				else if (translatable.TranslatableText.StartsWith("="))
				{
					translatable = new SubstringPart(translatable, 1, translatable.TranslatableText.Length - 1);
				}
				translatableParameters.Add(translatable);
			}
		}

		TranslatablePart SwapWhitespaceIfRightToLeft(TranslatablePart part)
		{
			if (Res.IsRightToLeft(Res.CurrentLanguage))
			{
				var trimmed = part.TranslatableText.Trim();
				if (trimmed != part.TranslatableText)
				{
					var whitespaceSwappedPart = new TranslatablePart(part.TranslatableText.Substring(part.TranslatableText.IndexOf(trimmed) + trimmed.Length) + trimmed + part.TranslatableText.Substring(0, part.TranslatableText.IndexOf(trimmed)));
					part.Next = whitespaceSwappedPart;
					part = whitespaceSwappedPart;
				}
			}
			return part;
		}

		TranslatablePart[] Normalize(TranslatablePart[] result)
		{
			result = Trim(result);
			result = NormalizeNumberedParameters(result);
			return result;
		}

		TranslatablePart[] Trim(TranslatablePart[] result)
		{
			for (int i = 0; i < result.Length; i++)
			{
				string trimmed = result[i].TranslatableText.Trim();
				if (trimmed != result[i].TranslatableText)
				{
					result[i] = new SubstringPart(result[i], result[i].TranslatableText.IndexOf(trimmed), trimmed.Length);
				}
			}
			return result;
		}

		TranslatablePart[] NormalizeNumberedParameters(TranslatablePart[] result)
		{
			for (int i = 0; i < result.Length; i++)
			{
				int p = 0;
				Match match = numberedParameterRegex.Match(result[i].TranslatableText);
				while (match.Success)
				{
					if (int.Parse(match.Value.Substring(1, match.Value.Length - 2)) != p)
					{
						result[i] = new ReplacerPart(result[i], match.Index, match.Length, p);
					}
					p++;
					match = numberedParameterRegex.Match(result[i].TranslatableText, match.Index + 3);
				}
			}
			return result;
		}

		public void SetTranslation()
		{
			cell.SetValueDetectingFormulaeFromLeadingEqualsSign(TranslatableRoot.CellText);
		}

		internal string CellText
		{
			get { return TranslatableRoot.CellText; }
		}

		public class TranslatablePart
		{
			public TranslatablePart(string translatableText)
			{
				this.TranslatableText = translatableText;
			}

			public TranslatablePart Next { get; set; }

			public virtual string TranslatableText
			{
				get
				{
					if (Next != null)
					{
						return Next.CellText;
					}
					else
					{
						return translatableText;
					}
				}

				set
				{
					if (Next != null)
					{
						throw new InvalidOperationException("Cannot set TranslatableText on a chained TranslatablePart");
					}
					translatableText = value;
				}
			}
			string translatableText;

			public virtual string CellText { get { return TranslatableText; } }
		}

		public class SubstringPart : TranslatablePart
		{
			public SubstringPart(TranslatablePart translatable, int start, int length)
				: base(translatable.TranslatableText.Substring(start, length))
			{
				leftText = translatable.TranslatableText.Substring(0, start);
				rightText = translatable.TranslatableText.Substring(start + length);
				translatable.Next = this;
			}

			readonly string leftText;
			readonly string rightText;

			public override string CellText
			{
				get
				{
					return leftText + TranslatableText + rightText;
				}
			}
		}

		public class ReplacerPart : TranslatablePart
		{
			public ReplacerPart(TranslatablePart translatable, int index, int length, int replacementCount)
				: base(translatable.TranslatableText.Substring(0, index) + "{" + replacementCount + "}" + translatable.TranslatableText.Substring(index + length))
			{
				ReplacedPart = new TranslatablePart(translatable.TranslatableText.Substring(index, length));
				this.replacementCount = replacementCount;
				translatable.Next = this;
			}

			public TranslatablePart ReplacedPart { get; private set; }
			readonly int replacementCount;

			public override string CellText
			{
				get
				{
					var replacementRegex = new Regex(@"\{" + replacementCount + @"(\:(?<case>U|u|L|l|T|t))?\}");
					return replacementRegex.Replace(TranslatableText, match =>
					{
						if (match.Groups["case"].Success)
						{
							return string.Format("<ChangeCase(\"{0}\",{1})>", ReplacedPart.CellText,
								match.Groups["case"].Value);
						}
						else
						{
							return ReplacedPart.CellText;
						}
					}, 1);
				}
			}
		}
	}
}
