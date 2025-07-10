using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Visualisation;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Areas
{
	class CellContentReplacer
	{
		internal CellContentReplacer(Report parentReport, int rowNo, int columnNo)
			: this(
			parentReport,
			parentReport.WorkSheetCurrentlyBeingProcessed[rowNo, columnNo],
			() => parentReport.WorkSheetCurrentlyBeingProcessed.GetCellFont(rowNo, columnNo),
			() => parentReport.WorkSheetCurrentlyBeingProcessed.GetCellWidth(rowNo, columnNo))
		{
		}

		internal CellContentReplacer(Report parentReport, object content)
			: this(parentReport, content, () => null, () => int.MaxValue)
		{
		}

		CellContentReplacer(Report parentReport, object content, Func<Font> getCellFontStrategy, Func<int> getCellWidthStrategy)
		{
			ParentReport = parentReport;
			ReplaceContent(content);
			IsReplaced = false;

			if (StillContainsAtLeastOneMacro)
			{
				IsCellAutoHeight = AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString)
							|| SignOff.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString)
							|| Html.RegexToFindMacroInString.IsMatch(ContentAsString);
				if (IsCellAutoHeight)
				{
					MinimumAutoHeightRows = AutoHeight.GetMinimumRows(ContentAsString);
					RemoveLineBreaksToFitAutoHeightOverflow = AutoHeight.ShouldRemoveLineBreaksToFit(ContentAsString);
				}

				IsCellShrinkToFit = !IsCellAutoHeight && ShrinkToFit.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString);
				if (IsCellShrinkToFit)
				{
					MinimumFontSizeShrinkToFit = ShrinkToFit.GetMinimumFontSize(ContentAsString);
				}

				IsCellShrinkToFitForBillOfLading = !IsCellAutoHeight && !IsCellShrinkToFit && ShrinkToFitForBillOfLading.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString);
				if (IsCellShrinkToFitForBillOfLading)
				{
					MinimumFontSizeShrinkToFitForBillOfLading = ShrinkToFitForBillOfLading.GetMinimumFontSize(ContentAsString);
				}

				IsCellExpandToFit = !IsCellAutoHeight && !IsCellShrinkToFit && !IsCellShrinkToFitForBillOfLading && ExpandToFit.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString);
				if (IsCellExpandToFit)
				{
					RowToExpand = ExpandToFit.GetRowToExpand(ContentAsString);
				}

				IsCellOverflow = !IsCellAutoHeight && !IsCellShrinkToFit && !IsCellShrinkToFitForBillOfLading && !IsCellExpandToFit && OverFlowToFollowPage.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString);
				if (IsCellOverflow)
				{
					CellOverflowManager = new OverflowManager(ContentAsString, getCellFontStrategy(), getCellWidthStrategy());
				}

				IsHideRowIfCellIsEmpty = HideRowIfCellIsEmpty.RegexToFindMacroAnyWhereInString.IsMatch(ContentAsString);

				foreach (var item in macrosShouldBeCopiedIfNewRowAdded)
				{
					if (item.RegexToFindMacro.IsMatch(ContentAsString))
					{
						ShouldCellValueCopiedToNewLines = true;
						ContentToCopyToNewLines = ContentAsString;
						break;
					}
				}
			}
		}

		readonly Report ParentReport;
		internal readonly bool IsCellAutoHeight;
		internal readonly int MinimumAutoHeightRows;
		internal readonly bool RemoveLineBreaksToFitAutoHeightOverflow;
		internal readonly bool IsCellShrinkToFit;
		internal readonly float MinimumFontSizeShrinkToFit;
		internal readonly bool IsCellShrinkToFitForBillOfLading;
		internal readonly float MinimumFontSizeShrinkToFitForBillOfLading;
		internal readonly bool IsHideRowIfCellIsEmpty;
		internal readonly bool IsCellExpandToFit;
		internal readonly RowToExpand RowToExpand = RowToExpand.First;

		internal readonly bool IsCellOverflow;
		internal readonly OverflowManager CellOverflowManager;
		internal readonly bool ShouldCellValueCopiedToNewLines;

		internal bool IsReplaced { get; private set; }

		object content;
		internal object Content
		{
			get { return content; }
			set
			{
				content = value;
				ContentAsString = content.ToString().RemoveInvalidCharacters();
			}
		}
		internal string ContentAsString { get; private set; }

		readonly IValueProviderThatShouldBeCopiedIfNewRowAdded[] macrosShouldBeCopiedIfNewRowAdded =
			new IValueProviderThatShouldBeCopiedIfNewRowAdded[]
			{
				new HideRowIf(),
				new HideRowIfCellIsEmpty()
			};

		internal readonly string ContentToCopyToNewLines;

		internal void TrimTrailingLineFeeds()
		{
			TRichString richString = Content as TRichString;
			if (richString != null)
			{
				ReplaceContent(new TRichString(richString.Value.TrimEnd('\n', '\r'), richString, 0));
			}
			else
			{
				var trimmedString = ContentAsString.TrimEnd('\n', '\r');
				//Trim only if required as we will lose the content type and replace it with a String
				if (ContentAsString != trimmedString)
				{
					ReplaceContent(trimmedString);
				}
			}
		}

		internal void ReplaceMacros()
		{
			if (StillContainsAtLeastOneMacro)
			{
				string originalCellContent = ContentAsString;
				string cellContentWithoutFormattingOnlyMacros = NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly(ContentAsString);
				string overridingFieldName = cellContentWithoutFormattingOnlyMacros.Replace(".", "").Trim();

				if (ParentReport.Style == Report.Styles.Document && ParentReport.OverridingDataSet != null && ParentReport.OverridingDataSet.MainTable.Columns.Contains(overridingFieldName)
					&& !ParentReport.Renderer.IsCurrentSectionBodyAreaWithMoreThanOneRowData)
				{
					ReplaceContent(ParentReport.OverridingDataSet.MainRow[overridingFieldName]);
				}
				else
				{
					string trimmedMacro = ContentAsString.Trim();
					if (IsSingleMacro(trimmedMacro, ParentReport.IsSingleMacroCache))
					{
						object cellValue = GetValueForSingleMacro(trimmedMacro);
						if (cellValue is string && ContentAsString != trimmedMacro)
						{
							string value = cellValue.ToString().EscapeDollarSignForRegexReplace();
							ReplaceContent(RegexProvider.OutermostMacroRegex.Replace(ContentAsString, value));
						}
						else
						{
							ReplaceContent(cellValue);
						}
					}
					else
					{
						object cellValue = null;
						var isAccumulativeTotalReplaced = false;

						if (!string.IsNullOrEmpty(cellContentWithoutFormattingOnlyMacros) && IsSingleMacro(cellContentWithoutFormattingOnlyMacros.Trim(), ParentReport.IsSingleMacroCache))
						{
							var singleMacro = cellContentWithoutFormattingOnlyMacros.Trim();
							if (ParentReport.Renderer.CurrentPass == Passes.SecondPass)
							{
								isAccumulativeTotalReplaced = RegexProvider.InnermostMacrosRegex.Matches(singleMacro).OfType<Match>().Any(x => RegexProvider.AccumulativeTotalMacroRegex.Match(x.Value).Success);
							}
							cellValue = GetValueForSingleMacro(singleMacro);
						}

						if (cellValue is TFormula || cellValue is ExcelHyperlink || isAccumulativeTotalReplaced)
						{
							ReplaceContent(cellValue);
						}
						else
						{
							var dataTypeMacro = string.Empty;

							if (ParentReport.Renderer.CurrentPass == Passes.FirstPass && string.IsNullOrEmpty(DataType.RegexToFindMacroAnyWhereInString.Match(ContentAsString).Value))
							{
								dataTypeMacro = DataType.CreateDataTypeMacro(cellValue);
							}

							var replacer = new MultiMixedMacroReplacer(this);
							ReplaceContent(replacer.GetMergedContent(dataTypeMacro));
						}
					}

					if (ParentReport.Renderer.CurrentPass == Passes.SecondPass && IsReplaced)
					{
						ReplaceContent(DataType.ConvertCellValueType(DataType.RegexToFindMacroAnyWhereInString.Match(originalCellContent).Value, Content));
					}
				}
			}

			if (!StillContainsAtLeastOneMacro && IsCellOverflow)
			{
				CellOverflowManager.ManagerOverflow(ContentAsString);
				if (IsCellOverflowing)
				{
					ReplaceContent(string.Join("\r\n", CellOverflowManager.FirstPageText.ToArray()));
				}
			}
		}

		internal void ForceFormatForRightToLeftLanguage()
		{
			if (!string.IsNullOrEmpty(ContentAsString) && Res.IsRightToLeft(Res.CurrentLanguage))
			{
				var rightToLeftMark = (char)0x200F;
				var strValue = (rightToLeftMark + ContentAsString);
				if (Content is string)
				{
					Content = strValue;
				}
				else if (Content is ZString)
				{
					Content = new ZString(strValue);
				}
			}
		}

		bool IsCellOverflowing
		{
			get { return CellOverflowManager.FirstPageText.Count > 1 || !string.IsNullOrEmpty(CellOverflowManager.OverflowText); }
		}

		internal bool StillContainsAtLeastOneMacro
		{
			get
			{
				bool? result = containsAtLeastOneMacro ?? (containsAtLeastOneMacro = RegexProvider.InnermostMacrosRegex.IsMatch(ContentAsString));
				return result.Value;
			}
		}
		bool? containsAtLeastOneMacro;

		internal bool StillContainsAtLeastOneNonFormattingMacro
		{
			get
			{
				bool? result = containsAtLeastOneNonFormattingMacro ?? (containsAtLeastOneNonFormattingMacro = RegexProvider.InnermostMacrosRegex.IsMatch(CellContentWithoutFormattingOnlyMacros));
				return result.Value;
			}
		}
		bool? containsAtLeastOneNonFormattingMacro;

		internal string CellContentWithoutFormattingOnlyMacros
		{
			get { return NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly(ContentAsString); }
		}

		internal void UnEscapeEscapedMacroSyntax()
		{
			if (ContentAsString.IndexOf("\\<") > -1 || ContentAsString.IndexOf("\\>") > -1)
			{
				if (Content is TRichString)
				{
					TRichString cellContent = Content as TRichString;
					ReplaceContent(cellContent.Replace("\\<", "<").Replace("\\>", ">"));
				}
				else
				{
					ReplaceContent(ContentAsString.Replace("\\<", "<").Replace("\\>", ">"));
				}
			}
		}

		void ReplaceContent(object content)
		{
			this.Content = content;
			IsReplaced = true;
			containsAtLeastOneMacro = null;
		}

		internal void FixOverlengthCellContent(int rowNumber, int col, ExcelWorkSheet worksheetBeingProcessed)
		{
			string contentAsString = Content as string;
			if (contentAsString != null && contentAsString.Length > FlxConsts.Max_StringLenInCell)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(
					Res.GetString("f0ae8da8-30c0-456d-ae9e-3579e6647620", "Macro {0} returned {1} characters which is more than the maximum allowable by Excel ({2}). Cell Content has been truncated to {2} characters."
						, worksheetBeingProcessed[rowNumber, col]
						, contentAsString.Length.ToString()
						, FlxConsts.Max_StringLenInCell.ToString())
					, ReportProcessingErrorSeverity.WarningWithoutErrorReport));
				ReplaceContent(contentAsString.Substring(0, FlxConsts.Max_StringLenInCell));
			}
		}

		object GetValueForSingleMacro(string macro)
		{
			object result = "";
			try
			{
				Passes pass = ParentReport.Renderer != null ? ParentReport.Renderer.CurrentPass : Passes.FirstPass;
				result = ParentReport.MacroTranslator.GetValue(macro, pass);
			}
			catch (FieldNotFoundException exception)
			{
				exception.AddAsWarningToReport(ParentReport);
			}
			catch (BoxOutOfSectionBodyException exception)
			{
				exception.AddAsWarningToReport(ParentReport);
			}

			return result;
		}

		static bool IsSingleMacro(string macro, ConcurrentDictionary<string, bool> isSingleMacroCache)
		{
			return isSingleMacroCache.GetOrAdd(macro, key => RegexProvider.IsSingleMacro(macro.Trim()));
		}

		class MultiMixedMacroReplacer
		{
			internal MultiMixedMacroReplacer(CellContentReplacer cell)
			{
				Cell = cell;
				ParentReport = cell.ParentReport;
				MacroValueReplacementCache = new Dictionary<string, string>();
				CellContentWithMacrosReplaced = ReplaceMacrosBuildingListOfMacroValuePairs(cell.ContentAsString);
			}

			string ReplaceMacrosBuildingListOfMacroValuePairs(string cellContent)
			{
				string result = cellContent;
				string lastOutput = null;
				while (lastOutput != result)
				{
					lastOutput = result;
					result = RegexProvider.OutermostMacroRegex.Replace(result, new MatchEvaluator(GetStringReplacementForOneMacro));
				}
				return result;
			}
			readonly CellContentReplacer Cell;
			readonly Report ParentReport;
			readonly Dictionary<string, string> MacroValueReplacementCache;
			readonly string CellContentWithMacrosReplaced;

			string GetStringReplacementForOneMacro(Match aMatch)
			{
				var macroKey = aMatch.Groups[0].Value;
				if (!MacroValueReplacementCache.TryGetValue(macroKey, out var replacementValue))
				{
					var rawReplacementValue = Cell.GetValueForSingleMacro(macroKey);
					if (rawReplacementValue is ZDateTime)
					{
						rawReplacementValue = ((ZDateTime)rawReplacementValue).ToLongTimeString();
					}
					else if (rawReplacementValue is ZDateTimeOffset)
					{
						rawReplacementValue = ((ZDateTimeOffset)rawReplacementValue).ToZDateTime().ToLongTimeString();
					}
					else if (rawReplacementValue is TFormula formula)
					{
						rawReplacementValue = ParentReport.MacroTranslator.GetFormulaResult(formula.Text);
					}
					replacementValue = rawReplacementValue.ToString();
					MacroValueReplacementCache.Add(macroKey, replacementValue);
				}
				return replacementValue;
			}

			internal object GetMergedContent(string prefix)
			{
				TRichString cellContent = Cell.Content as TRichString;
				if (cellContent != null)
				{
					foreach (KeyValuePair<string, string> replacement in MacroValueReplacementCache)
					{
						cellContent = cellContent.Replace(replacement.Key, replacement.Value);
					}

					if (!string.IsNullOrEmpty(prefix))
					{
						cellContent = new TRichString(prefix).Add(cellContent);
					}
					return cellContent;
				}
				return prefix + CellContentWithMacrosReplaced;
			}
		}
	}
}
