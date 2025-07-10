using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngineIntegration;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Areas
{
	[System.Diagnostics.DebuggerDisplay("{GetType().Name} StartingRow = {StartingRow} End = {End}")]
	abstract class Area
	{
		protected Area() { }

		protected Area(int start, int end, Report report, string parameterText)
		{
			Argument.NotNull(report, "report");

			FormulaRelatedAreas = new List<Area>();

			fStart = start;
			fEnd = end;
			fUnitHeight = end - start;
			ParentReport = report;
			Parameters = SplitParameters(parameterText);
			this.AreaHeaderText = parameterText;
		}
		protected readonly string[] Parameters;

		protected readonly string AreaHeaderText;

		static readonly Regex NewLineRegexPattern = new Regex("[\r\n|\n]", RegexOptions.Compiled);

		protected CellReference GetAreaHeaderCellReference()
		{
			var workSheet = ParentReport.WorkSheetCurrentlyBeingProcessed;
			return new CellReference((workSheet != null ? workSheet.SheetName : "N/A"), "A" + (StartingRow + 1).ToString());
		}

		protected virtual string[] SplitParameters(string parameterText)
		{
			return parameterText.Split(new char[] { ':', ',' });
		}

		#region Rendering functionality
		public bool ShouldDelete;
		public int RenderedToPageNumber;
		List<Area> SplittedAreas;

		public virtual bool TryHardPutInOnePage => false;

		public virtual bool FitsInPage(int heightAvailableInPage, Page page)
		{
			return HeightInXls <= heightAvailableInPage;
		}

		int fColumnCount = -1;
		public int ColumnCount
		{
			get
			{
				if (fColumnCount == -1)
				{
					for (int row = StartOfBody; row <= End; row++)
					{
						for (int col = 1; col < ParentReport.WorkSheetCurrentlyBeingProcessed.ColumnCount; col++)
						{
							if (!ParentReport.WorkSheetCurrentlyBeingProcessed.IsCellEmpty(row, col)
								|| !ParentReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(row, col).Borders.IsEmpty)
							{
								var lastColOfCell = ParentReport.WorkSheetCurrentlyBeingProcessed.GetCellsLastColumn(row, col);
								if (lastColOfCell > fColumnCount)
								{
									fColumnCount = lastColOfCell;
								}
							}
						}
					}
				}
				return fColumnCount;
			}
		}

		public virtual bool IsDataArea
		{
			get { return false; }
		}
		public virtual bool SticksToBottomOfPage
		{
			get { return false; }
		}

		public virtual bool SplitIfNotFitInAPage
		{
			get { return false; }
		}

		public void MoveAreaBefore(Area areaToInsertBefore)
		{
			int originalAreaToMoveStartingRow = StartingRow;
			int originalAreaToMoveEnd = End;
			int startingRowNumber = areaToInsertBefore.StartingRow;
			int shiftForGroupBy = 0;

			for (int i = ParentReport.Analyser.Areas.IndexOf(areaToInsertBefore); i < ParentReport.Analyser.Areas.IndexOf(this); i++)
			{
				shiftForGroupBy -= ParentReport.Analyser.Areas[i].HeightInRowsIncludingStartingRow;
				ParentReport.Analyser.Areas[i].Shift(HeightInRowsIncludingStartingRow);
			}

			ParentReport.Analyser.Areas.Remove(this);
			ParentReport.Analyser.Areas.Insert(ParentReport.Analyser.Areas.IndexOf(areaToInsertBefore), this);
			Shift(shiftForGroupBy);
			ParentReport.WorkSheetCurrentlyBeingProcessed.DuplicateRows(originalAreaToMoveStartingRow, originalAreaToMoveEnd, startingRowNumber, 1);
			ParentReport.WorkSheetCurrentlyBeingProcessed.RemoveRows(originalAreaToMoveStartingRow + HeightInRowsIncludingStartingRow, originalAreaToMoveEnd + HeightInRowsIncludingStartingRow + 1);
		}

		internal void MoveAreaAfter(Area areaToInsertAfter)
		{
			int originalAreaToMoveStartingRow = StartingRow;
			int originalAreaToMoveEnd = End;
			int startingRowNumber = areaToInsertAfter.End + 1;
			int shiftForGroupBy = 0;

			for (int i = ParentReport.Analyser.Areas.IndexOf(areaToInsertAfter); i > ParentReport.Analyser.Areas.IndexOf(this); i--)
			{
				shiftForGroupBy += ParentReport.Analyser.Areas[i].HeightInRowsIncludingStartingRow;
				ParentReport.Analyser.Areas[i].Shift(-HeightInRowsIncludingStartingRow);
			}

			ParentReport.Analyser.Areas.Remove(this);
			ParentReport.Analyser.Areas.Insert(ParentReport.Analyser.Areas.IndexOf(areaToInsertAfter) + 1, this);
			Shift(shiftForGroupBy);
			ParentReport.WorkSheetCurrentlyBeingProcessed.DuplicateRows(originalAreaToMoveStartingRow, originalAreaToMoveEnd, startingRowNumber, 1);
			ParentReport.WorkSheetCurrentlyBeingProcessed.RemoveRows(originalAreaToMoveStartingRow, originalAreaToMoveEnd + 1);
		}

		internal void UpdateEndPosition(int newEndPosition)
		{
			fEnd = newEndPosition;
			fUnitHeight = fEnd - fStart;
		}

		public virtual void Shift(int rows)
		{
			fStart += rows;
			fEnd += rows;
		}

		internal void ShiftEnd(int rows)
		{
			fEnd += rows;
		}

		public virtual bool BreakPage
		{
			get { return false; }
			internal set { }
		}

		public int UnitHeight
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return fUnitHeight; }
		}

		public int HeightInRows
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return fEnd - StartOfBody + 1; }
		}

		public int HeightInRowsIncludingStartingRow
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return HeightInRows + 1; }
		}

		public virtual int HeightInXls
		{
			get
			{
				int result = 0;
				for (int row = StartOfBody; row <= End; row++)
				{
					if (ShouldRowContributeToHeight(row))
					{
						result += ParentReport.WorkSheetCurrentlyBeingProcessed.GetRowHeight(row);
					}
				}
				return result;
			}
		}

		bool ShouldRowContributeToHeight(int row) => !HasHPageBreakAtRow(row) && ShouldRowContributeToHeightCore(row);

		protected virtual bool ShouldRowContributeToHeightCore(int row) => true;

		public int StartOfBody
		{
			get { return fStart + 1; }
		}

		public int StartingRow
		{
			get { return fStart; }
		}

		public int End
		{
			get { return fEnd; }
		}

		internal int ReplaceTemplateCell(int rowIndex, int columnIndex, int linesNeededForThisRow, Dictionary<int, MultiLineCellInfo> multiLineRowColumnContents, Dictionary<int, string> contentsShouldCopyToNewLines, IEnumerable<int> hiddenColumns)
		{
			var cellContent = ParentReport.WorkSheetCurrentlyBeingProcessed[rowIndex, columnIndex];
			var cellReference = new CellReference(ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName, rowIndex, columnIndex);

			using (ParentReport.ErrorManager.EvaluatingCell(cellReference))
			using (ParentReport.ErrorManager.EvaluatingOuterContent(cellContent))
			{
				try
				{
					if (ShouldApplyFormatPatternOnCells)
					{
						SetupFormatPatternOnCell(columnIndex, rowIndex);
					}

					var cellFormula = cellContent as TFormula;
					if (cellFormula != null)
					{
						ReplaceFormulaInCell(cellFormula, rowIndex, columnIndex);
					}
					else
					{
						linesNeededForThisRow = Math.Max(linesNeededForThisRow, ReplaceNonFormulaInCell(rowIndex, columnIndex, multiLineRowColumnContents, contentsShouldCopyToNewLines, hiddenColumns));
					}
				}
				catch (FieldNotFoundException exception)
				{
					exception.AddAsWarningToReport(ParentReport);
				}
			}

			return linesNeededForThisRow;
		}

		internal void AddLinesToFitMultiLineRowColumnContents(int rowIndex, int linesNeededForThisRow, Dictionary<int, MultiLineCellInfo> multiLineRowColumnContents, Dictionary<int, string> contentsShouldCopyToNewLines, IReportRenderer renderer)
		{
			if (linesNeededForThisRow > 1)
			{
				bool alreadyFits = false;

				if (renderer.ExpandRowsForAutoHeight)
				{
					var cellInfo = multiLineRowColumnContents.Values
						.Where(c => c.Row == rowIndex)
						.OrderByDescending(c => c.MultiLineContents.Count)
						.First();
					alreadyFits = TryExpandRowForAutoHeight(rowIndex, linesNeededForThisRow, cellInfo);
				}

				if (!alreadyFits)
				{
					Expand(rowIndex, rowIndex, linesNeededForThisRow - 1);

					foreach (int columnIndex in multiLineRowColumnContents.Keys)
					{
						var multiLineContents = multiLineRowColumnContents[columnIndex].MultiLineContents;
						ParentReport.WorkSheetCurrentlyBeingProcessed[rowIndex, columnIndex] = multiLineContents.Count > 0 ? multiLineContents[0] : "";
					}
					for (int newRowIndex = rowIndex + 1; newRowIndex < rowIndex + linesNeededForThisRow; newRowIndex++)
					{
						for (int columnIndex = 0; columnIndex < ParentReport.MaxCol; columnIndex++)
						{
							var lineIndex = newRowIndex - rowIndex;
							if (multiLineRowColumnContents.ContainsKey(columnIndex) && lineIndex < multiLineRowColumnContents[columnIndex].MultiLineContents.Count)
							{
								ParentReport.WorkSheetCurrentlyBeingProcessed[newRowIndex, columnIndex] = multiLineRowColumnContents[columnIndex].MultiLineContents[lineIndex];
							}
							else if (contentsShouldCopyToNewLines.ContainsKey(columnIndex))
							{
								ParentReport.WorkSheetCurrentlyBeingProcessed[newRowIndex, columnIndex] = contentsShouldCopyToNewLines[columnIndex];
							}
							else
							{
								ParentReport.WorkSheetCurrentlyBeingProcessed[newRowIndex, columnIndex] = "";
							}
						}
					}
					for (int newRowIndex = rowIndex; newRowIndex < rowIndex + linesNeededForThisRow - 1; newRowIndex++)
					{
						ParentReport.WorkSheetCurrentlyBeingProcessed.ClearBottomLine(newRowIndex);
					}
					for (int newRowIndex = rowIndex + 1; newRowIndex < rowIndex + linesNeededForThisRow; newRowIndex++)
					{
						ParentReport.WorkSheetCurrentlyBeingProcessed.ClearTopLine(newRowIndex);
					}
				}
			}
		}

		bool TryExpandRowForAutoHeight(int rowIndex, int linesNeededForThisRow, MultiLineCellInfo cellinfo)
		{
			var worksheet = ParentReport.WorkSheetCurrentlyBeingProcessed;
			var rowHeight = worksheet.GetRowHeight(rowIndex);
			int rowHeightRequired = rowHeight * linesNeededForThisRow;
			if (rowHeightRequired <= ExcelWorkSheet.MaxRowHeight)
			{
				worksheet.SetRowHeight(rowIndex, rowHeightRequired);
				return true;
			}
			else if (cellinfo.CellContent.RemoveLineBreaksToFitAutoHeightOverflow)
			{
				var contents = worksheet[rowIndex, cellinfo.Column];
				if (contents != null)
				{
					var contentsAsString = NewLineRegexPattern.Replace(contents.ToString(), " ");
					worksheet[rowIndex, cellinfo.Column] = contentsAsString;
					var wrappedText = new TextWrapper(contentsAsString, worksheet.GetCellWidth(rowIndex, cellinfo.Column), worksheet.GetCellFont(rowIndex, cellinfo.Column));
					rowHeightRequired = rowHeight * wrappedText.WrappedTextLines.Count;
					if (rowHeightRequired <= ExcelWorkSheet.MaxRowHeight)
					{
						worksheet.SetRowHeight(rowIndex, rowHeightRequired);
						return true;
					}
				}
			}
			return false;
		}

		internal ZBool ShouldApplyFormatPatternOnCells
		{
			get
			{
				if (ParentReport.Analyser == null)
				{
					throw new InvalidOperationException("ParentReport.Analyser");
				}
				return (this is SectionBodyArea || this is GroupByArea) &&
						ParentReport.Renderer.CurrentPass == Passes.FirstPass &&
						ParentReport.Analyser.Config != null &&
						!string.IsNullOrEmpty(ParentReport.Analyser.Config.DocumentCurrencyPattern);
			}
		}

		void SetupFormatPatternOnCell(int colNumber, int rowNumber)
		{
			if (ParentReport.Analyser.Config.ColumnsWithCurrencySetup.Contains(colNumber))
			{
				CellFormat format = ParentReport.WorkSheetCurrentlyBeingProcessed.GetCellFormat(rowNumber, colNumber, HorizontalTextAlignment.Right);
				format.FormatPattern = ParentReport.Analyser.Config.DocumentCurrencyPattern;
				ParentReport.WorkSheetCurrentlyBeingProcessed.SetCellFormat(rowNumber, colNumber, format);
			}
		}

		internal int GetRealRowsToKeepConcerningMergedCell(int rowsToKeep)
		{
			var result = rowsToKeep;
			if (result > 0 && !CanSkipMergedRowsCheck)
			{
				while (result > 0 && ParentReport.WorkSheetCurrentlyBeingProcessed.DoesRowContainAMergedCellToNextRow(StartOfBody + result - 1))
				{
					result--;
				}
			}

			return result;
		}

		internal bool forceSplitting;

		public virtual Area SplitAndReturnNewArea(int maxHeightAvailable, Page page, PageBreakProcessor pageBreakProcessor)
		{
			Area result = null;
			var rowsThatCanFitOnPage = GetRowsToKeep(maxHeightAvailable, page);

			(Area Area, int RowsToBreakOn) doSplitResult = default;

			var rowsToKeep = rowsThatCanFitOnPage;
			var rowToBreakOn = 0;
			if (rowsToKeep > 0)
			{
				rowsToKeep = GetRealRowsToKeepConcerningMergedCell(rowsToKeep);

				if (rowsToKeep > 0)
				{
					doSplitResult = DoSplitArea(rowsToKeep);
				}
				else
				{
					if (forceSplitting || pageBreakProcessor != null && !pageBreakProcessor.IsSplittableInEmptyPage(this))
					{
						doSplitResult = DoSplitArea(rowsThatCanFitOnPage);
					}

					forceSplitting = true;
				}

				result = doSplitResult.Area;
				rowToBreakOn = doSplitResult.RowsToBreakOn;
			}
			else if (pageBreakProcessor != null && pageBreakProcessor.GetRowsToKeepInEmptyPage(this) <= 0 && !pageBreakProcessor.DoesFitInEmptyPage(this))
			{
				return this; // Means this area is not splittable in next page
			}

			if (result != null)
			{
				var areaNumber = ParentReport.Analyser.Areas.IndexOf(this);
				ParentReport.WorkSheetCurrentlyBeingProcessed.DuplicateRows(StartingRow, StartingRow, result.StartingRow, 1);
				result.ShiftEnd(1);
				ParentReport.Analyser.Areas.Insert(areaNumber + 1, result);

				for (var i = areaNumber + 2; i < ParentReport.Analyser.Areas.Count; i++)
				{
					ParentReport.Analyser.Areas[i].Shift(1);
					if (ParentReport.Analyser.Areas[i].FormulaRelatedAreas.IndexOf(this) > -1)
					{
						ParentReport.Analyser.Areas[i].FormulaRelatedAreas.Add(result);
					}
				}

				ParentReport.Analyser.UpdateSectionBodyAreaRowRangesInReport(rowToBreakOn, 1);
			}

			return result;
		}

		internal (Area Area, int RowsToBreakOn) DoSplitArea(int rowsToKeep)
		{
			var area = SplitArea(rowsToKeep);
			area.forceSplitting = forceSplitting;
			var rowsToBreakOn = StartingRow + rowsToKeep;
			return (area, rowsToBreakOn);
		}

		internal bool PageBroken { get; private set; }

		public virtual int GetRowsToKeep(int maxHeightAvailable, Page page)
		{
			var result = 0;

			var heightToKeep = 0;
			while (heightToKeep <= maxHeightAvailable)
			{
				var rowIndex = StartOfBody + result;

				if (HasHPageBreakAtRow(rowIndex))
				{
					ParentReport.WorkSheetCurrentlyBeingProcessed.SetRowHeight(rowIndex, 0);
					PageBroken = true;
					result++;
					break;
				}

				if (ShouldRowContributeToHeight(rowIndex))
				{
					heightToKeep += ParentReport.WorkSheetCurrentlyBeingProcessed.GetRowHeight(rowIndex);
				}

				result++;
			}

			result -= heightToKeep > maxHeightAvailable ? 1 : 0;

			return result;
		}

		protected bool HasHPageBreakAtRow(int row)
		{
			var firstColumnContent = ParentReport.WorkSheetCurrentlyBeingProcessed[row, 0].ToString();
			return HPageBreak.RegexToFindMacroAnyWhereInString.IsMatch(firstColumnContent);
		}

		protected internal virtual Area SplitArea(int rowsToKeep)
		{
			if (SplittedAreas == null)
			{
				SplittedAreas = new List<Area> { this };
			}

			var rowsInNextArea = End - StartingRow - rowsToKeep;
			var result = Clone(StartOfBody + rowsToKeep);
			SplittedAreas.Add(result);
			result.SplittedAreas = SplittedAreas;
			fEnd = StartOfBody + rowsToKeep - 1;
			result.fEnd = result.StartingRow + rowsInNextArea - 1;
			return result;
		}

		#endregion

		#region Data related
		protected List<Area> fParentAreas = new List<Area>();
		public Area DataParent;

		public virtual SectionBodyArea DataSourceArea
		{
			get { return null; }
		}

		internal Dictionary<string, List<string>> AllDatafields
		{
			get
			{
				if (datafields == null)
				{
					datafields = ReadAllDataSourceFields();
				}
				return datafields;
			}
		}
		Dictionary<string, List<string>> datafields;

		protected Dictionary<string, List<string>> ReadAllDataSourceFields()
		{
			var result = new Dictionary<string, List<string>>();
			for (int rowIndex = StartingRow; rowIndex <= End; rowIndex++)
			{
				for (int colIndex = 0; colIndex < ParentReport.MaxCol; colIndex++)
				{
					var cell = ParentReport.WorkSheetCurrentlyBeingProcessed[rowIndex, colIndex];
					string cellValue = cell as string;
					if (cellValue == null)
					{
						TFormula formula = cell as TFormula;
						if (formula != null)
						{
							cellValue = formula.Text;
						}
						else
						{
							var richText = cell as TRichString;
							if (richText != null)
							{
								cellValue = richText.Value;
							}
						}
					}
					if (!string.IsNullOrEmpty(cellValue))
					{
						cellValue = cellValue.Trim();
						foreach (Match innerMostMacroMatch in RegexProvider.InnermostMacrosRegex.Matches(cellValue))
						{
							if (innerMostMacroMatch.Success && !RegexProvider.SelectListMacroRegex.IsMatch(cellValue))
							{
								string singleMacro = innerMostMacroMatch.Groups[0].Value;
								if (RegexProvider.TableNamePlusColumnNameWithOptionalTotalRegex.IsMatch(singleMacro))
								{
									DataSourceField field = new DataSourceField(singleMacro);
									List<string> list;
									if (!result.TryGetValue(field.DataSourceName, out list))
									{
										list = new List<string>();
										result.Add(field.DataSourceName, list);
									}

									if (!list.Contains(field.FieldName))
									{
										list.Add(field.FieldName);
									}
								}
							}
						}
					}
				}
			}

			var additionalFields = ReadAdditionalFields();
			CombineDataSourceFields(result, additionalFields);

			return result;
		}

		protected void CombineDataSourceFields(Dictionary<string, List<string>> target, Dictionary<string, List<string>> source)
		{
			if (source == null || target == null)
			{
				return;
			}

			if (source != null)
			{
				foreach (var sourceKey in source.Keys)
				{
					List<string> targetList, sourceList = source[sourceKey];
					if (!target.TryGetValue(sourceKey.ToUpperInvariant().Trim(), out targetList))
					{
						target.Add(sourceKey, sourceList);
					}
					else
					{
						CombineDataSourceFields(targetList, sourceList);
					}
				}
			}
		}

		protected void CombineDataSourceFields(List<string> target, List<string> source)
		{
			if (source == null || target == null)
			{
				return;
			}

			if (source != null)
			{
				foreach (string sourceField in source)
				{
					if (!target.Contains(sourceField))
					{
						target.Add(sourceField);
					}
				}
			}
		}

		protected virtual Dictionary<string, List<string>> ReadAdditionalFields()
		{
			return null;
		}

		public virtual object GetColumnValue(int rowIndex, string fieldName)
		{
			IDataRowSource ds = null;

			if (DataSourceArea != null)
			{
				ds = DataSourceArea.DataRowSource;
			}
			return ParentReport.DataProvider.GetColumnValue(ds, rowIndex, fieldName, area: this);
		}

		public virtual int DBRowCount => 0;

		protected string ReplaceSingleMacrosForFormulas(Match aMatch)
		{
			if (RegexProvider.InnermostMacrosRegex.IsMatch(aMatch.Value)) //If no valid macro, nothing should be replaced
			{
				var result = "";
				try
				{
					result = ReplaceSingleMacrosForNonFormulas(aMatch).Replace("\"", "\"\"");
				}
				catch (FieldNotFoundException exception)
				{
					exception.AddAsWarningToReport(ParentReport);
				}
				return result;
			}
			else
			{
				return aMatch.Value;
			}
		}

		protected string ReplaceSingleMacrosForNonFormulas(Match aMatch)
		{
			var result = ParentReport.MacroTranslator.GetValue(aMatch.Groups[0].Value, ParentReport.Renderer.CurrentPass) ?? "";
			if (result is ZDateTimeOffset)
			{
				result = ((ZDateTimeOffset)result).ToZDateTime();
			}
			if (result is ZDateTime)
			{
				result = ((IZTypeInternals)result).GetValueForLogicalDataLayer(false);
			}
			return result.ToString();
		}

		internal void ReplaceFormulaInCell(TFormula originalFormula, int rowIndex, int columnIndex)
		{
			string originalFormulaText = originalFormula.Text;
			originalFormula.Text = Regex.Replace(originalFormula.Text, "\"([^\"]*)\"", match =>
			{
				var group = match.Groups[0].Value;
				return !ParentReport.Renderer.ReplaceTotalMacroInFormulaCell && RegexProvider.TotalMacroRegexAnyWhereInString.IsMatch(group) ? group : RegexProvider.OutermostMacroRegex.Replace(group, ReplaceSingleMacrosForFormulas);
			});
			if (ParentReport.Renderer.CurrentPass == Passes.SecondPass)
			{
				originalFormula.Text = originalFormula.Text.UnEscapeAngleBrackets();
			}

			if (originalFormula.Text != originalFormulaText)
			{
				originalFormula.Result = null;
				try
				{
					ParentReport.WorkSheetCurrentlyBeingProcessed[rowIndex, columnIndex] = originalFormula;
				}
				catch (ExcelInterfaceException ex)
				{
					ParentReport.WorkSheetCurrentlyBeingProcessed[rowIndex, columnIndex] = "";
					string message = ex.Message;

					if (ex.InnerException is FlexCelCoreException innerFlexCelException)
					{
						message = innerFlexCelException.Message;
					}

					message = Res.GetString("6a2e91ed-2959-484b-988e-a4ed9d4c7303", "Error Replacing Macros in [{0}] - {1}", originalFormulaText, message.ShrinkToMaxLength(200));
					ParentReport.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Warning, ex));
				}
			}
		}

		protected virtual bool ShouldReportErrorsWhenMacrosArePresentOnConfigLine(int currentRow)
		{
			return false;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:Avoid excessive complexity")]
		internal int ReplaceNonFormulaInCell(int rowNumber, int columnIndex, Dictionary<int, MultiLineCellInfo> multiLineRowColumnContents, Dictionary<int, string> contentsShouldCopyToNewLines, IEnumerable<int> hiddenColumns)
		{
			int linesNeededForThisRow = 1;

			try
			{
				var worksheetCurrentlyBeingProcessed = ParentReport.WorkSheetCurrentlyBeingProcessed;
				var cellContent = new CellContentReplacer(ParentReport, rowNumber, columnIndex);

				if (cellContent.StillContainsAtLeastOneMacro)
				{
					if (ShouldReportErrorsWhenMacrosArePresentOnConfigLine(rowNumber))
					{
						var message = Res.GetString("3e7a2522-9739-4aa8-9db2-42ad1097204f", "Macros {0} are not allowed on the same line as {1}/{2} as configuration lines are removed from the report/document.", cellContent.ContentAsString, Constants.SectionForeachTags.BeginForeach, Constants.SectionForeachTags.EndForeach);
						ParentReport.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Error));
						return linesNeededForThisRow;
					}

					using (ParentReport.ErrorManager.GetErrorCheckingSuspender(x => ParentReport.Analyser.NonExistantOptionalColumnsContains(columnIndex)))
					{
						cellContent.ReplaceMacros();
					}
				}

				if (!cellContent.StillContainsAtLeastOneNonFormattingMacro)
				{
					cellContent.FixOverlengthCellContent(rowNumber, columnIndex, worksheetCurrentlyBeingProcessed);

					bool isColumnHidden = hiddenColumns.Contains(columnIndex) || worksheetCurrentlyBeingProcessed.IsColumnHidden(columnIndex);
					if (!isColumnHidden)
					{
						var contentToBeFormatted = new Lazy<string>(() => cellContent.Content is TFormula formula
							? ParentReport.MacroTranslator.GetFormulaResult(formula.Text).ToString()
							: cellContent.Content is ExcelHyperlink hyperlink ? hyperlink.TextToShow
							: cellContent.CellContentWithoutFormattingOnlyMacros);

						if (cellContent.IsCellOverflow)
						{
							cellContent.TrimTrailingLineFeeds();
							var overflowManager = cellContent.CellOverflowManager;
							var cellInfo = new MultiLineCellInfo(rowNumber, columnIndex, cellContent, overflowManager.FirstPageText);
							multiLineRowColumnContents.Add(columnIndex, cellInfo);
							linesNeededForThisRow = Math.Max(linesNeededForThisRow, overflowManager.FirstPageText.Count);
							if (overflowManager.OverflowText.Length > 0)
							{
								ParentReport.OverflowNoteDocument.OverflowNotes.Add(new OverflowNote(overflowManager.NotesTitle, overflowManager.OverflowText));
							}
						}
						else if (cellContent.IsCellAutoHeight)
						{
							cellContent.TrimTrailingLineFeeds();

							int columnWidth = ShouldUseColumnWidthCache
								? TryGetColumnWidthFromCache(rowNumber, columnIndex)
								: worksheetCurrentlyBeingProcessed.GetCellWidth(rowNumber, columnIndex);
							var wrappedText = new TextWrapper(contentToBeFormatted.Value, columnWidth, worksheetCurrentlyBeingProcessed.GetCellFont(rowNumber, columnIndex)).WrappedTextLines;

							if (wrappedText.Count > 1)
							{
								var cellInfo = new MultiLineCellInfo(rowNumber, columnIndex, cellContent, wrappedText);
								multiLineRowColumnContents.Add(columnIndex, cellInfo);
								linesNeededForThisRow = Math.Max(linesNeededForThisRow, wrappedText.Count);
								linesNeededForThisRow = Math.Max(linesNeededForThisRow, cellContent.MinimumAutoHeightRows);
							}
						}
						else if (cellContent.IsCellShrinkToFit)
						{
							cellContent.TrimTrailingLineFeeds();
							ShrinkToFit.Shrink(contentToBeFormatted.Value, cellContent.MinimumFontSizeShrinkToFit, ParentReport);
						}
						else if (cellContent.IsCellShrinkToFitForBillOfLading)
						{
							cellContent.TrimTrailingLineFeeds();
							ShrinkToFitForBillOfLading.ShrinkForBillOfLading(contentToBeFormatted.Value, cellContent.MinimumFontSizeShrinkToFitForBillOfLading, ParentReport);
						}
						else if (cellContent.IsCellExpandToFit)
						{
							cellContent.TrimTrailingLineFeeds();
							ExpandToFit.Expand(
								worksheetCurrentlyBeingProcessed.ParentExcelInterface,
								worksheetCurrentlyBeingProcessed.WorkSheetNumber - 1,
								rowNumber,
								columnIndex,
								cellContent.RowToExpand,
								contentToBeFormatted.Value);
						}
					}

					if (cellContent.IsHideRowIfCellIsEmpty && string.IsNullOrEmpty(cellContent.CellContentWithoutFormattingOnlyMacros))
					{
						worksheetCurrentlyBeingProcessed.SetRowHeight(rowNumber, 0);
					}
				}

				if (cellContent.ShouldCellValueCopiedToNewLines)
				{
					contentsShouldCopyToNewLines.Add(columnIndex, cellContent.ContentToCopyToNewLines);
				}

				if (ParentReport.Renderer.CurrentPass == Passes.SecondPass)
				{
					cellContent.UnEscapeEscapedMacroSyntax();
				}

				if (cellContent.IsReplaced)
				{
					if (cellContent.Content is TFormula formula)
					{
						ProcessFormulaInCellContent(rowNumber, columnIndex, formula);
					}

					worksheetCurrentlyBeingProcessed[rowNumber, columnIndex] = cellContent.Content;
				}

				if (ParentReport.Renderer.CurrentPass == Passes.SecondPass && !cellContent.StillContainsAtLeastOneMacro && (cellContent.Content is string || cellContent.Content is ZString))
				{
					cellContent.ForceFormatForRightToLeftLanguage();
					worksheetCurrentlyBeingProcessed[rowNumber, columnIndex] = cellContent.Content;
				}
			}
			catch (ExcelInterfaceException ex)
			{
				if (ex.Type == ExcelInterfaceExceptionType.ErrorInsertingImage)
				{
					ParentReport.ErrorManager.Add(new ReportProcessingError(ex.Message, ReportProcessingErrorSeverity.Warning, ex));
				}
				else
				{
					throw;
				}
			}
			catch (DocumentEngineException ex)
			{
				var message = Res.GetString("c369907a-d717-4b17-9a87-ea52704523c5", "The cell could not be generated. ") + ex.Message;
				ParentReport.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.Error, ex));
			}

			return linesNeededForThisRow;
		}

		internal Dictionary<(int IndexOfUnit, int Column), int> AutoHeightColumnsWidthCache { get; set; } = new Dictionary<(int IndexOfUnit, int Column), int>();

		protected virtual bool ShouldUseColumnWidthCache => false;

		internal int TotalExpandedRows { get; set; }

		internal bool? ContainsMergedCellWithNextRow { get; set; }

		protected virtual bool CanSkipMergedRowsCheck => false;

		int TryGetColumnWidthFromCache(int rowIndex, int columnIndex)
		{
			int unitIndex = (rowIndex - StartOfBody - TotalExpandedRows) % UnitHeight;
			int cellWidth = AutoHeightColumnsWidthCache.ContainsKey((unitIndex, columnIndex))
				? AutoHeightColumnsWidthCache[(unitIndex, columnIndex)]
				: 0;

			if (cellWidth == 0)
			{
				cellWidth = ParentReport.WorkSheetCurrentlyBeingProcessed.GetColWidth(columnIndex);
			}

			return cellWidth;
		}

		internal void ClearColumnsWidthCache()
		{
			AutoHeightColumnsWidthCache.Clear();
			TotalExpandedRows = 0;
		}

		void ProcessFormulaInCellContent(int rowNumber, int columnIndex, TFormula formula)
		{
			var worksheetCurrentlyBeingProcessed = ParentReport.WorkSheetCurrentlyBeingProcessed;
			var formatPattern = GetCurrencyFormatString(formula);

			if (!string.IsNullOrEmpty(formatPattern))
			{
				var format = worksheetCurrentlyBeingProcessed.GetCellFormat(rowNumber, columnIndex);
				format.FormatPattern = formatPattern;
				worksheetCurrentlyBeingProcessed.SetCellFormat(rowNumber, columnIndex, format);
			}
		}

		string GetCurrencyFormatString(TFormula formula)
		{
			string format = "";

			var match = numberFormatRegex.Match(formula.Text);
			if (match.Length > 0)
			{
				int decimalPlaces;
				if (Int32.TryParse(match.Groups[2].Value, out decimalPlaces))
				{
					format = "#,##0.";

					while (decimalPlaces > 0)
					{
						decimalPlaces--;
						format += "0";
					}
				}
			}

			return format.TrimEnd('.');
		}

		static readonly Regex numberFormatRegex = new Regex(@"^=(?:[\s]*)round(?:[\s]*)\((?:[\s]*)(.*?)(?:[\s]*),(?:[\s]*)([^\)]*)(?:[\s]*)\)$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		internal object ReplaceMacros(string input)
		{
			CellContentReplacer cellContent = new CellContentReplacer(ParentReport, input);
			cellContent.ReplaceMacros();
			return cellContent.Content;
		}

		#endregion

		#region Formula providers and related stuff
		public readonly List<Area> FormulaRelatedAreas;
		FormulaProvider fFormulaProvider;

		public FormulaProvider FormulaProvider
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return fFormulaProvider; }
		}

		public virtual FormulaProviderList FormulaProviders
		{
			get
			{
				FormulaProviderList result = new FormulaProviderList();
				result.Add(fFormulaProvider);
				return result;
			}
		}

		public void SetWorksheetForFormulaProvider(ExcelWorkSheet workSheet)
		{
			fFormulaProvider = new FormulaProvider(workSheet);
		}

		public virtual RowRangeList GetRowRanges(int originalPositionOfFieldInArea, string columnName, int maxDBRowsToProcess)
		{
			return GetRowRanges(originalPositionOfFieldInArea, columnName);
		}

		public virtual RowRangeList GetRowRanges(int originalPositionOfFieldInArea, string columnName)
		{
			RowRangeList ranges = new RowRangeList();
			if (!ShouldDelete && FormulaProvider != null)
			{
				if (FormulaProvider.ContainsColumn(columnName))
				{
					if (originalPositionOfFieldInArea > fEnd - StartOfBody)
					{
						int partNumber = 1;
						originalPositionOfFieldInArea -= HeightInRows;
						while (originalPositionOfFieldInArea >= SplittedAreas[partNumber].HeightInRows)
						{
							originalPositionOfFieldInArea -= SplittedAreas[partNumber].HeightInRows;
							partNumber++;
						}
						ranges.Add(new RowRange(SplittedAreas[partNumber].StartOfBody + originalPositionOfFieldInArea + 1, SplittedAreas[partNumber].StartOfBody + originalPositionOfFieldInArea + 1));//Excel formulas are 1 based.
					}
					else
					{
						ranges.Add(new RowRange(StartOfBody + originalPositionOfFieldInArea + 1, StartOfBody + originalPositionOfFieldInArea + 1));//Excel formulas are 1 based.
					}
				}
				else
				{
					foreach (Area parent in Parents)
					{
						ranges.AddRange(parent.GetRowRanges(originalPositionOfFieldInArea, columnName));
					}
				}
			}
			return ranges;
		}

		#endregion

		#region Parenthood

		public Section OwnerSection;
		public Report ParentReport
		{
			get;
			private set;
		}

		internal void OverrideParentReportForNotesOverflowPage(Report report)
		{
			if (report == null)
			{
				throw new ArgumentNullException(nameof(report));
			}
			ParentReport = report;
		}

		#endregion

		#region Abstract methods and properties
		public abstract Area Clone(int position);
		public abstract bool CanCloseAPage { get; }
		public abstract List<Area> Parents { get; }

		public abstract ValueProviderDocumenter GetDocumentation();
		#endregion

		#region Implementation
		protected int fStart;
		protected int fEnd;
		protected int fUnitHeight;

		protected internal virtual void Expand(int startOfExpand, int endOfExpand, int multiplier, bool shouldUpdateSectionBodyAreaUpdateRowRangesWithIndex = true)
		{
			if (multiplier != 0)
			{
				if (ParentReport.Analyser == null)
				{
					throw new InvalidOperationException("ParentReport.Analyser");
				}

				ParentReport.WorkSheetCurrentlyBeingProcessed.DuplicateRows(startOfExpand, endOfExpand, endOfExpand + 1, multiplier);
				var areaIndex = ParentReport.Analyser.Areas.IndexOf(this);
				var delta = (endOfExpand - startOfExpand + 1) * multiplier;

				fEnd += (endOfExpand - startOfExpand + 1) * multiplier;

				for (var i = areaIndex + 1; i < ParentReport.Analyser.Areas.Count; i++)
				{
					ParentReport.Analyser.Areas[i].Shift(delta);
				}

				if (shouldUpdateSectionBodyAreaUpdateRowRangesWithIndex)
				{
					ParentReport.Analyser.UpdateSectionBodyAreaRowRangesInReport(endOfExpand, delta);
				}
			}
		}

		protected void CopyCommonMembers(Area cloned)
		{
			cloned.fUnitHeight = UnitHeight;
			cloned.OwnerSection = OwnerSection;
			cloned.DataParent = DataParent;
			if (FormulaProvider != null)
			{
				cloned.fFormulaProvider = FormulaProvider.Clone();
			}
		}

		protected bool ContainsParam(string parameter)
		{
			for (int i = 1; i < Parameters.Length; i++)
			{
				if (Parameters[i].Trim().Equals(parameter.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		internal virtual string Identifier
		{
			get
			{
				string name = GetType().Name;
				return "#" + name.Remove(name.Length - 4);
			}
		}

		internal class DataSourceField
		{
			public DataSourceField(string macro)
			{
				MatchCollection matches = Regex.Matches(macro);
				foreach (Match match in matches)
				{
					DataSourceName = match.Groups[1].Value.ToUpperInvariant().Trim();
					FieldName = match.Groups[2].Value.ToUpperInvariant().Trim();
					break;
				}
			}
			public readonly string DataSourceName;
			public readonly string FieldName;

			static readonly Regex Regex = new Regex(@"<?[\s]*(?:Total)?[\s]*([^>\.]+)\.([^>\r\n]+)[\s]*>?", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		}
	}

	[System.Diagnostics.DebuggerDisplay("Row={Row}, Col={Column}, Number of lines={MultiLineContents.Count}")]
	class MultiLineCellInfo
	{
		internal MultiLineCellInfo(int row, int column, CellContentReplacer cellContent, List<string> multiLineContents)
		{
			Argument.NotNull(cellContent, "cellContent");
			Argument.NotNull(multiLineContents, "multiLineContents");

			Row = row;
			Column = column;
			CellContent = cellContent;
			MultiLineContents = multiLineContents;
		}

		internal int Row { get; private set; }
		internal int Column { get; private set; }
		internal CellContentReplacer CellContent { get; private set; }
		internal List<string> MultiLineContents { get; private set; }
	}
}
