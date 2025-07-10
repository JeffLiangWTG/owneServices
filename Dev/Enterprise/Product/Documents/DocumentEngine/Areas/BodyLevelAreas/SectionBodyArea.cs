using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Renderer;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class SectionBodyArea : TreeDataArea
	{
		[SuppressMessage("Microsoft.Maintainability", "CA1502:Avoid excessive complexity")]
		public SectionBodyArea(int start, int end, Report report, string parameterText)
			: base(start, end, report, parameterText, null)
		{
			fNumberOfRowsToShow = -1;
			fStartingRowToShow = -1;
			fMaximumNumberOfRowsToShow = -1;

			string param1 = Parameters.Length > 1 ? Parameters[1].Trim() : null;
			if (param1 == null || param1.StartsWith("DATA=" + OneRowDataSource.TableIdentifier, StringComparison.OrdinalIgnoreCase))
			{
				fDataRowSource = new OneRowDataSource();
				fTableName = OneRowDataSource.TableIdentifier;
			}
			else
			{
				if (!param1.StartsWith("DATA=", StringComparison.OrdinalIgnoreCase))
				{
					throw new DocumentEngineException("Malformed SectionBody Constructor. Should be: " + GetDocumentation().Useage + " But was: " + parameterText);
				}
				fTableName = param1.Substring(5);

				for (int i = 2; i < Parameters.Length; i++)
				{
					string parameter = Parameters[i].Trim();
					if (parameter.StartsWith("MINIMUMROWSINPAGE=", StringComparison.OrdinalIgnoreCase))
					{
						throw new DocumentEngineException("MinimumRowsInPage is not supported anymore!");
					}
					if (parameter.StartsWith(Constants.SectionBodyAreaParameters.MaximumNumberOfRowsToShow, StringComparison.OrdinalIgnoreCase))
					{
						fMaximumNumberOfRowsToShow = ParseNumberOfRows(parameter, Constants.SectionBodyAreaParameters.MaximumNumberOfRowsToShow);
					}
					if (parameter.StartsWith(Constants.SectionBodyAreaParameters.RowCountAMultipleof, StringComparison.OrdinalIgnoreCase))
					{
						fRowCountAMultipleOf = ParseNumberOfRows(parameter, Constants.SectionBodyAreaParameters.RowCountAMultipleof);
					}
					if (parameter.StartsWith(Constants.SectionBodyAreaParameters.NumberOfRowsToShow, StringComparison.OrdinalIgnoreCase))
					{
						fNumberOfRowsToShow = ParseNumberOfRows(parameter, Constants.SectionBodyAreaParameters.NumberOfRowsToShow);
					}
					if (parameter.StartsWith(Constants.SectionBodyAreaParameters.StartingRowToShow, StringComparison.OrdinalIgnoreCase))
					{
						fStartingRowToShow = ParseNumberOfRows(parameter, Constants.SectionBodyAreaParameters.StartingRowToShow);
					}
					if (parameter.Equals(Constants.CommonAreaParameters.Sticky, StringComparison.OrdinalIgnoreCase))
					{
						Sticky = true;
					}
					if (parameter.Equals(Constants.SectionBodyAreaParameters.CustomSorting, StringComparison.OrdinalIgnoreCase))
					{
						ShouldApplyCustomSorting = true;
					}
					if (parameter.StartsWith(Constants.SectionBodyAreaParameters.FilterDataSource, StringComparison.OrdinalIgnoreCase))
					{
						var filterDataSourceRegex = new Regex(Constants.SectionBodyAreaParameters.FilterDataSource + @"[\s]*\((?<expression>.*)\)[\s]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
						var match = filterDataSourceRegex.Match(parameter);
						if (match.Success)
						{
							FilteredDataSourceBy = match.Groups["expression"].Value;
						}
					}
					if (parameter.StartsWith(Constants.SectionBodyAreaParameters.SortDataSource, StringComparison.OrdinalIgnoreCase))
					{
						var sortByDataSourceRegex = new Regex(Constants.SectionBodyAreaParameters.SortDataSource + @"[\s]*\((?<sort>.*)\)[\s]*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
						var match = sortByDataSourceRegex.Match(parameter);
						if (match.Success)
						{
							string[] parameters = match.Groups["sort"].Value.Split(',');
							sortByColumn = parameters[0];
							sortDirection = parameters.Length > 1 ? parameters[1].Trim() : Constants.SectionBodySortByDirections.Ascending;
						}
					}
				}

				if (fMaximumNumberOfRowsToShow != -1 && fRowCountAMultipleOf != -1)
				{
					if (fRowCountAMultipleOf != 0)
					{
						if (fMaximumNumberOfRowsToShow % fRowCountAMultipleOf != 0)
						{
							ParentReport.ErrorManager.Add(new ReportProcessingError(
								Res.GetString("0d9582df-e2c5-4eb1-baa7-1ef846825716", "{0} must be a multiple of {1}", "MaximumNumberOfRowsToShow", "RowCountAMultipleOf")
								, new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, "A" + start)
								, ReportProcessingErrorSeverity.Error));
						}
					}
				}
				if (fStartingRowToShow != -1 && fMaximumNumberOfRowsToShow != -1)
				{
					if (fStartingRowToShow > fMaximumNumberOfRowsToShow)
					{
						ParentReport.ErrorManager.Add(new ReportProcessingError(
							Res.GetString("ee291ee1-bcf7-4629-82fc-100bb8672991", "The {0} can not be more than {1}", "StartingRowToShow", "MaximumNumberOfRowsToShow")
							, new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, "A" + start)
							, ReportProcessingErrorSeverity.Error));
					}
				}

				if (ShouldApplyCustomSorting && !string.IsNullOrEmpty(sortByColumn))
				{
					ParentReport.ErrorManager.Add(new ReportProcessingError(
						Res.GetString("213CF1D3-DED5-4408-B7D9-C17AEA5183A0", "{0} cannot be used together with {1} in the same {2} Area", "CustomSorting", "OrderBy", "SectionBody")
						, new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, "A" + start)
						, ReportProcessingErrorSeverity.Error));
				}
			}

			AreasSplittedAcrossPages.Add(this);
			FirstDataRowInArea = 0;
		}

		SectionBodyArea() { }

		readonly int fNumberOfRowsToShow;
		readonly int fRowCountAMultipleOf;
		readonly int fStartingRowToShow;
		readonly internal int fMaximumNumberOfRowsToShow;

		public override ValueProviderDocumenter GetDocumentation()
		{
			var summary = ResString.GetMultilingualString("a64cccdd-e508-4de1-a2bb-b5ce52c084be",
				@"{0} Areas are used to output single or multiple rows from a given Data Row Source. A Data Row Source can be a Data Source Name for Reports, or a Child Collection for Documents.

The contents of this Area will be replicated once for every row in the specified Data Row Source. As many rows as possible will be output on each page taking note of the amount of space available, the amount of space the {1} Area and {2} Areas will take, and the relevant optional parameters shown below.

If you just want to output one Body Section unrelated to a particular Data Row Source, use the Data Row Source '{3}'. i.e:
{4}", "#SectionBody", "#SectionPageFooter", "#PageFooter", "DummyCollection", "#SectionBody:Data=DummyCollection");

			var parameters = ResString.GetMultilingualString("2c598d1d-6297-4222-9be5-69037c550e50",
				@"Optional Parameters:
{0}(Logical Expression) - Filters the Data source to include only the records which match the Logical Expression. NB: This only applies to Documents with a Business Object data source, not to Reports that have an SQL data source.
{1}(column) - Sort the collection by a column of the type of collection business object and the direction. The direction parameter is optional, either ‘ASC’(ascending) or ‘DESC’(descending). NB: This only applies to Documents with a Business Object data source, not applicable to Reports which have SQL data source.
{2}={{number}} - Specifies the maximum number of rows to show on each page this Body Section is shown on. {2} must be a multiple of {3} if specified.
{4}={{number}} - Specifies the actual number of rows that will be output on each page the Body Section appears on. If there are less actual rows than what you specify for a page, blank rows are inserted into the output to make up the difference.
{3}={{number}} - Ensure the number of rows returned are a multiple of the number specified. If the actual number of rows is not a multiple of the number specified, blank rows are inserted into the output to bring it up to a multiple.
{5}={{number}} - Indicates the first row the Body Section should output. The {5} must not be greater than the {2}. NB: 1 indicates the first row, not 0.
{6} - Will try and stop an individual Body Section being split across multiple pages.
{7} - Indicates its {8} collection will not be re-ordered by the {8} column(s). Please note this parameter is only available in documents and cannot be used together with {1}.",
				"FilterBy", "OrderBy", "MaximumNumberOfRowsToShow", "RowCountAMultipleOf", "NumberOfRowsToShow", "StartingRowToShow", "Sticky", "CustomSorting", "GroupBy");

			return new ValueProviderDocumenter("#SectionBody:Data={DataRowSource}[:OptionalParam1[:OptionalParam..n]]",
				MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, summary, parameters));
		}

		internal ZString FilteredDataSourceBy
		{
			get;
			private set;
		}

		public List<Area> AreasSplittedAcrossPages = new List<Area>();
		public bool Sticky;
		public int FirstDataRowInArea;
		public readonly bool ShouldApplyCustomSorting;

		protected override string GetTableName()
		{
			return fTableName;
		}
		readonly string fTableName;

		public int RowCount
		{
			get
			{
				int result = DataRowSource.RowCount;

				if (fRowCountAMultipleOf != 0)
				{
					if (DataRowSource.RowCount % fRowCountAMultipleOf != 0)
					{
						result = ((DataRowSource.RowCount / fRowCountAMultipleOf) + 1) * fRowCountAMultipleOf;
					}
				}
				if (fStartingRowToShow != 0 && fRowCountAMultipleOf != 0)
				{
					if (fStartingRowToShow > fRowCountAMultipleOf)
					{
						result = 0;
					}
				}
				return result;
			}
		}

		public IDataRowSource[] GetGroups(string[] fieldNames)
		{
			return DataRowSource.GroupBy(fieldNames);
		}

		public override SectionBodyArea DataSourceArea
		{
			get { return this; }
		}

		int ParseNumberOfRows(string parameter, string parameterType)
		{
			int fNumberOfRows;
			int.TryParse(parameter.Substring(parameter.IndexOf("=") + 1), out fNumberOfRows);
			if (fNumberOfRows <= 0)
			{
				throw new DocumentEngineException(string.Format("Malformed SectionBody {0}. Value was: {1}", parameterType, fNumberOfRows));
			}
			else
			{
				return fNumberOfRows;
			}
		}

		public void SortForBusinessObjectDataSourceIfNeeded()
		{
			if (!string.IsNullOrEmpty(sortByColumn) && DataRowSource is BusinessObjectDataSource)
			{
				BusinessObjectDataSource source = (BusinessObjectDataSource)DataRowSource;

				try
				{
					var notNullSourceLines = source.GroupedOrFilteredCollection.Where(e => e != null);
					var sortedSource = sortDirection.Equals(Constants.SectionBodySortByDirections.Descending, StringComparison.OrdinalIgnoreCase) ?
						notNullSourceLines.OrderByDescending(e => e[sortByColumn]).ToList() :
						notNullSourceLines.OrderBy(e => e[sortByColumn]).ToList();
					sortedSource.AddRange(source.GroupedOrFilteredCollection.Where(e => e == null)); // Add the empty (null) rows excluded from sorting above to avoid NullReferenceException

					SetDataSource(new BusinessObjectDataSource(source.Name, sortedSource, true));
				}
				catch (ArgumentException ex)
				{
					ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("6893cfdf-2b49-4b20-b3cd-6518acc0da85", "Column [{0}] specified in {1} is invalid.", sortByColumn, "OrderBy"), ReportProcessingErrorSeverity.Warning, ex));
				}
			}
		}

#if DEBUG
		internal
#endif
 string sortByColumn;
#if DEBUG
		internal
#endif
 string sortDirection;

		public override IDataRowSource DataRowSource
		{
			get
			{
				if (fDataRowSource == null)
				{
					try
					{
						if (ParentReport?.SpecifiedDataRowSource != null)
						{
							fDataRowSource = ParentReport.SpecifiedDataRowSource;
						}
						else
						{
							fDataRowSource = ParentReport.DataProvider.GetDataRowSource(TableName, IsDataArea, fMaximumNumberOfRowsToShow);
						}
					}
					catch (FieldNotFoundException)
					{
						var message = string.Empty;
						var dataSourceType = ParentReport.BODocDataProvider.BusinessObjectToLogAgainst.GetType().FullName;
						var sectionBodyMessage = FormattableString.Invariant($"#SectionBody:Data={TableName}");

						if (ParentReport.Template != null && ParentReport.Template.ContainsCustomisedSections)
						{
							message = Res.GetString("d0d4e7f1-9e5d-4e01-9878-0ee764ab158b", "The data source for this document ({0}) does not contain a collection called [{1}]. Please check all your {2} templates for a section containing [{1}] as its data source. Typical syntax would be '{3}'.", dataSourceType, TableName, SectionRepositoryTemplateNames.User, sectionBodyMessage);
						}
						else
						{
							message = Res.GetString("f9ffc8d2-2865-44b9-9321-b74eb84c5007", "The data source for this document ({0}) does not contain a collection called [{1}]. Please check the template '{2}'. Typical syntax would be '{3}'.", dataSourceType, TableName, ParentReport.Template?.TemplateName, sectionBodyMessage);
						}

						throw new DataProviderException(message);
					}
				}

				if (IsFirstAccessToDataRowSource)
				{
					IsFirstAccessToDataRowSource = false;
					using (ParentReport.ErrorManager.EvaluatingCell(GetAreaHeaderCellReference()))
					using (ParentReport.ErrorManager.EvaluatingOuterContent(AreaHeaderText))
					{
						if (fDataRowSource != null && fStartingRowToShow > -1)
						{
							fDataRowSource = fDataRowSource.Split(fStartingRowToShow - 1);
						}
						if (fDataRowSource != null && fMaximumNumberOfRowsToShow > -1)
						{
							if (fDataRowSource.RowCount > fMaximumNumberOfRowsToShow)
							{
								fDataRowSource = fDataRowSource.GetFirstNRows(fMaximumNumberOfRowsToShow);
							}
						}
						if (fDataRowSource != null && !FilteredDataSourceBy.IsEmpty)
						{
							try
							{
								using (ParentReport.ErrorManager.EvaluatingInnerMacro(FilteredDataSourceBy))
								{
									fDataRowSource = fDataRowSource.Filter(FilteredDataSourceBy);
								}
							}
							catch (FieldNotFoundException ex)
							{
								ParentReport.ErrorManager.Add(new ReportProcessingError(ex.Message, ReportProcessingErrorSeverity.Warning, ex));
							}
						}
						if (fDataRowSource != null && fNumberOfRowsToShow > -1)
						{
							fDataRowSource = fDataRowSource.GetFirstNRows(fNumberOfRowsToShow);
						}
					}
				}

				if (fDataRowSource == null)
				{
					fDataRowSource = new EmptyDataSource();
				}

				if (fDataRowSource is BusinessObjectDataSource businessObjectDataSource)
				{
					businessObjectDataSource.IsApplyingCustomSorting |= ShouldApplyCustomSorting;
				}

				return fDataRowSource;
			}
		}

		IDataRowSource fDataRowSource;
		bool IsFirstAccessToDataRowSource = true;
		public void SetDataSource(IDataRowSource ds)
		{
			fDataRowSource = ds;
		}

		protected override string[] SplitParameters(string parameterText)
		{
			return AreaHelper.SplitParametersForSectionBody(parameterText);
		}

		public class RowTracker
		{
			public RowTracker(int originalPosition, int xlsRowCount)
			{
				this.OriginalPosition = originalPosition;
				this.XlsRowCount = xlsRowCount;
				CurrentPosition = -1;
			}

			public int OriginalPosition;
			public int XlsRowCount;
			public int CurrentPosition;
		}

		internal List<RowTracker> RowHeightTracker = new List<RowTracker>();

		protected override bool ShouldUseColumnWidthCache => !Children.OfType<SectionForeachArea>().Any();

		protected override bool CanSkipMergedRowsCheck => ContainsMergedCellWithNextRow.HasValue && !ContainsMergedCellWithNextRow.Value;

		internal bool IsClonedForPageBreaking { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "no error code to check on.")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no error code to check on., No need to translate.")]
		public override void ExpandForDataRows(int multiplier)
		{
			DataRowIndexWithRowRanges.Clear();

			try
			{
				for (var i = 0; i <= multiplier; i++)
				{
					DataRowIndexWithRowRanges.Add(i, new RowRange(StartOfBody + i * UnitHeight, End + i * UnitHeight));
				}

				base.Expand(StartOfBody, End, multiplier, false);
				ExpandForeachAreas();

				RowHeightTracker = new List<RowTracker>();
				for (var i = 0; i < DataRowIndexWithRowRanges.Count; i++)
				{
					var rowRange = DataRowIndexWithRowRanges[i];
					for (var j = 0; j <= rowRange.End - rowRange.Start; j++)
					{
						RowHeightTracker.Add(new RowTracker(j, 1));
					}
				}
			}
			catch (FlexCelException ex)
			{
				if (ex.Message == "Invalid arguments to call InsertAndCopyRange")
				{
					var message = Res.GetString("21b908bb-6b33-4858-b4c5-f1ce783dd1ce", "Exception happened trying to expand for data rows") + "\r\n";
					message += FormattableString.Invariant($"StartOfBody = {StartOfBody}, End = {End}, Multiplier = {multiplier}{System.Environment.NewLine}");
					throw new DocumentEngineException(message, ex);
				}

				throw;
			}
		}

		internal void ExpandForeachAreas()
		{
			ProcessSectionForeachAreas(this);

			Children.OfType<SectionForeachArea>().ForEach(f =>
			{
				f.ExpandIncludingChildren();
			});
		}

		internal List<SectionForeachArea> AllChildForeachAreas { get; } = new List<SectionForeachArea>();

		internal SectionForeachArea GetClosestForeachArea(int row)
		{
			return AllChildForeachAreas.Where(c => c.StartingRow <= row && c.End >= row).OrderBy(f => f.StartingRow).LastOrDefault();
		}

		internal void ShiftForAllChildForeachAreas(int row, int delta)
		{
			AllChildForeachAreas.Where(a => a.StartingRow > row).ForEach(a => a.Shift(delta));
			AllChildForeachAreas.Where(a => a.StartingRow <= row && a.End >= row).ForEach(a => a.ShiftEnd(delta));
		}

		internal void UpdateRowRangesWithIndexIncludeAllChildren(int row, int delta)
		{
			AllChildForeachAreas.ForEach(a => a.UpdateRowRangesWithIndex(row, delta));
			UpdateRowRangesWithIndex(row, delta);

			if (ParentReport.Renderer.IsProcessingMacros && row <= End && row >= StartOfBody)
			{
				TotalExpandedRows += delta;
			}
		}

		internal int GetRealDataRowsCount(int worksheetDataRowsCount)
		{
			int rowTrackerNumber = 0;
			int realRowsCounter = 0;
			while (rowTrackerNumber < RowHeightTracker.Count && realRowsCounter < worksheetDataRowsCount)
			{
				realRowsCounter += RowHeightTracker[rowTrackerNumber++].XlsRowCount;
			}
			return rowTrackerNumber;
		}

		public override object GetColumnValue(int rowIndex, string fieldName)
		{
			if (Children.Any())
			{
				TreeDataArea closestForeachArea = GetClosestForeachArea(ParentReport.Renderer.CurrentRow);

				while (closestForeachArea != null)
				{
					var rowIndexOfForeachArea = closestForeachArea.GetRowIndex(ParentReport.Renderer.CurrentRow);
					var result = ParentReport.DataProvider.GetColumnValue(closestForeachArea.DataRowSource, rowIndexOfForeachArea, fieldName, closestForeachArea is SectionForeachArea);
					if (result != null)
					{
						return result;
					}

					closestForeachArea = closestForeachArea.Parent;
				}
			}

			return base.GetColumnValue(rowIndex, fieldName);
		}

		internal void ReportRowIndexIsMinusOneOrUnmatchedWithStartEndRow(string fieldIdentifier = "")
		{
			var errorMessage = new ZStringBuilder();
			var tab = "\t\t";
			errorMessage.Append(FormattableString.Invariant($"****Assign this issue to JNC****"));
			errorMessage.Append(FormattableString.Invariant($"- StartingRow: {StartingRow}, StartOfBody: {StartOfBody}, End: {End}"));
			errorMessage.Append(FormattableString.Invariant($"- AreaHeaderText: {AreaHeaderText}"));
			if (!string.IsNullOrEmpty(fieldIdentifier))
			{
				errorMessage.Append(FormattableString.Invariant($"- Macro that is being translated: {fieldIdentifier}"));
			}
			errorMessage.Append(FormattableString.Invariant($"- Has children: {Children.Count > 0}"));

			var currentRow = ParentReport.Renderer.CurrentRow;
			var currentColumn = ParentReport.Renderer.CurrentColumn;
			errorMessage.Append(FormattableString.Invariant($"- CurrentRow: {currentRow}"));
			errorMessage.Append(FormattableString.Invariant($"- CurrentColumn: {currentColumn}"));
			errorMessage.Append(FormattableString.Invariant($"- CellContent: {ParentReport.WorkSheetCurrentlyBeingProcessed[currentRow, currentColumn]}"));

			errorMessage.Append(FormattableString.Invariant($"- LastRowIndex: {LastRowIndex}"));
			errorMessage.Append(FormattableString.Invariant($"- IsProcessingMacro: {ParentReport.Renderer.IsProcessingMacros}"));
			errorMessage.Append(FormattableString.Invariant($"- DBRowCount: {DBRowCount}"));
			errorMessage.Append(FormattableString.Invariant($"- CurrentPass: {ParentReport.Renderer.CurrentPass}"));
			errorMessage.Append(FormattableString.Invariant($"- IsClonedForPageBreaking: {IsClonedForPageBreaking}"));
			errorMessage.Append($"- DataRowIndexWithRowRanges:");
			DataRowIndexWithRowRanges.ForEach(pair => errorMessage.Append(FormattableString.Invariant($"{tab}key: {pair.Key}, range: {pair.Value.Start} -> {pair.Value.End}")));
			errorMessage.Append((NoResString)"- Report:");
			errorMessage.Append($"{ParentReport.ToString()}");

			var businessObjectTypeDescription = ParentReport?.Parent?.BizObject?.ToString() ?? string.Empty;
			var businessObjectPK = ((BusinessObject)ParentReport?.Parent?.BizObject)?.PK ?? ZGuid.Empty;

			errorMessage.Append(FormattableString.Invariant($" - Business Object: {businessObjectTypeDescription}, {businessObjectPK}"));

			ErrorReporter.ReportOnce("TreeDataArea.GetRowIndex returned -1", errorMessage.ToStringWithNewLineBetweenAppends());
		}

		internal bool IsDataRowIndexWithRowRangesMatchedWithStartEndRow()
		{
			var count = DataRowIndexWithRowRanges.Count;
			if (count > 0)
			{
				var firstStart = DataRowIndexWithRowRanges[0].Start;
				var lastEnd = DataRowIndexWithRowRanges[count - 1].End;

				return StartOfBody >= firstStart && End <= lastEnd;
			}

			return true;
		}

		protected internal override void Expand(int startOfExpand, int endOfExpand, int multiplier, bool shouldUpdateSectionBodyAreaUpdateRowRangesWithIndex = true)
		{
			var currentRow = StartOfBody;
			for (var rowTrackerNumber = 0; rowTrackerNumber < RowHeightTracker.Count; rowTrackerNumber++)
			{
				var tracker = RowHeightTracker[rowTrackerNumber];
				if (currentRow == startOfExpand)
				{
					RowHeightTracker[rowTrackerNumber] = new RowTracker(tracker.OriginalPosition, tracker.XlsRowCount + multiplier);
					break;
				}
				currentRow += tracker.XlsRowCount;
			}
			base.Expand(startOfExpand, endOfExpand, multiplier);
		}

		public override List<Area> Parents
		{
			get { return new List<Area>(); }
		}

		public override FormulaProviderList FormulaProviders
		{
			get
			{
				FormulaProviderList result = new FormulaProviderList();
				result.Add(FormulaProvider);
				foreach (SectionBodyArea area in OwnerSection.SplitSectionBodyAreaInstances)
				{
					result.Add(area.FormulaProvider);
				}
				return result;
			}
		}

		public override Area Clone(int position)
		{
			var cloned = new SectionBodyArea(position, position + fEnd - fStart, ParentReport, AreaHeaderText);
			CopyCommonMembers(cloned);
			cloned.IsForeachProcessed = IsForeachProcessed;
			cloned.ContainsMergedCellWithNextRow = ContainsMergedCellWithNextRow;
			cloned.AutoHeightColumnsWidthCache = new Dictionary<(int IndexOfUnit, int Column), int>(AutoHeightColumnsWidthCache);
			OwnerSection.SplitSectionBodyAreaInstances.Add(cloned);

			if (ParentReport.Renderer.IsProcessingPageBreaks)
			{
				// Cloned DataRowIndexWithRowRanges is a different instance from original one, but they share the same Values(RowRanges)
				cloned.DataRowIndexWithRowRanges = DataRowIndexWithRowRanges;
				cloned.IsClonedForPageBreaking = true;
				CloneChildren(cloned);
			}

			return cloned;
		}

		void CloneChildren(SectionBodyArea clonedArea)
		{
			foreach (var foreachArea in Children.OfType<SectionForeachArea>())
			{
				foreachArea.CopySelfToParent(clonedArea);
			}
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override bool SplitIfNotFitInAPage
		{
			get { return true; }
		}

		int XlsRowFromTracker(int trackerNumber)
		{
			SetupCurrentPosition(trackerNumber);
			return RowHeightTracker[trackerNumber].CurrentPosition;
		}

		void SetupCurrentPosition(int trackerNumber)
		{
			if (RowHeightTracker[trackerNumber].CurrentPosition == -1)
			{
				if (trackerNumber == 0)
				{
					RowHeightTracker[0].CurrentPosition = StartOfBody;
				}
				else if (trackerNumber > 0)
				{
					if (RowHeightTracker[trackerNumber - 1].CurrentPosition == -1)
					{
						SetupCurrentPosition(trackerNumber - 1);
					}
					RowHeightTracker[trackerNumber].CurrentPosition = RowHeightTracker[trackerNumber - 1].CurrentPosition + RowHeightTracker[trackerNumber - 1].XlsRowCount;
				}
			}
		}

		public override RowRangeList GetRowRanges(int originalPositionOfFieldInArea, string columnName)
		{
			return GetRowRanges(originalPositionOfFieldInArea, columnName, RowHeightTracker.Count);
		}

		public override RowRangeList GetRowRanges(int originalPositionOfFieldInArea, string columnName, int maxDBRowsToProcess)
		{
			var ranges = new RowRangeList();

			for (int rowTrackerNumber = 0; rowTrackerNumber < maxDBRowsToProcess; rowTrackerNumber++)
			{
				if (rowTrackerNumber < RowHeightTracker.Count)
				{
					if (RowHeightTracker[rowTrackerNumber].OriginalPosition == originalPositionOfFieldInArea)
					{
						int excelRowNumber = XlsRowFromTracker(rowTrackerNumber) + 1;
						ranges.Add(new RowRange(excelRowNumber, excelRowNumber));
					}
				}
			}

			return ranges;
		}

		public override int GetRowsToKeep(int maxHeightAvailable, Page page)
		{
			if (Sticky && page.ContainsAnySectionBodyOrDocumentHeaderArea)
			{
				return 0;
			}

			return base.GetRowsToKeep(maxHeightAvailable, page);
		}

		int GetDBRowsToKeep(int excelRowsToKeep)
		{
			var excelRowsKept = 0;
			var result = 0;

			foreach (var rowTracker in RowHeightTracker)
			{
				result++;

				excelRowsKept += rowTracker.XlsRowCount;
				if (excelRowsKept > excelRowsToKeep)
				{
					break;
				}
			}

			return (int)Math.Ceiling((double)result / UnitHeight);
		}

		protected internal override Area SplitArea(int rowsToKeep)
		{
			int rowToBreakOn = StartingRow + rowsToKeep;
			if (rowToBreakOn > StartingRow)
			{
				var dataRowsToKeep = GetDBRowsToKeep(rowsToKeep);
				var rowsInNextArea = End - StartingRow - rowsToKeep;
				var result = (SectionBodyArea)Clone(StartOfBody + rowsToKeep);
				fEnd = StartOfBody + rowsToKeep - 1;
				result.fEnd = result.StartingRow + rowsInNextArea - 1;
				result.SetDataSource(DataRowSource);

				result.RowHeightTracker = RowHeightTracker;
				RowHeightTracker = new List<RowTracker>();
				var numberOfRowsMoved = 0;

				try
				{
					while (numberOfRowsMoved < rowsToKeep)
					{
						RowHeightTracker.Add(result.RowHeightTracker[0]);
						numberOfRowsMoved += result.RowHeightTracker[0].XlsRowCount;
						result.RowHeightTracker.RemoveAt(0);
					}
				}
				catch (ArgumentOutOfRangeException e)
				{
					ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("5D810D49-58B7-431B-B4A4-7B1A9AEAC1A0", "If this issue can be reproduced, please raise an incident and provide essential information for the replication."), ReportProcessingErrorSeverity.Error, e));
				}

				if (numberOfRowsMoved > rowsToKeep)
				{
					var bigTracker = RowHeightTracker[RowHeightTracker.Count - 1];
					var extraXlRowsInBigTracker = numberOfRowsMoved - rowsToKeep;
					var trackerPart1 = new RowTracker(bigTracker.OriginalPosition, bigTracker.XlsRowCount - extraXlRowsInBigTracker);
					var trackerPart2 = new RowTracker(-1, extraXlRowsInBigTracker);
					RowHeightTracker[RowHeightTracker.Count - 1] = trackerPart1;
					result.RowHeightTracker.Insert(0, trackerPart2);
				}

				result.FirstDataRowInArea = FirstDataRowInArea + dataRowsToKeep - 1;

				AreasSplittedAcrossPages.Add(result);
				result.AreasSplittedAcrossPages = AreasSplittedAcrossPages;

				ParentReport.WorkSheetCurrentlyBeingProcessed.CopyBorderSettings(OwnerSection.SplitSectionBodyAreaInstances[0].End, fEnd);

				return result;
			}

			return null;
		}

		protected override bool ShouldReportErrorsWhenMacrosArePresentOnConfigLine(int currentRow)
		{
			var firstCellInCurrentRow = ParentReport.WorkSheetCurrentlyBeingProcessed[currentRow, 0].ToString();
			if (SectionForeachArea.IsSectionForeachAreaBeginStart(firstCellInCurrentRow) ||
					SectionForeachArea.IsSectionForeachAreaEndStart(firstCellInCurrentRow))
			{
				return true;
			}

			return false;
		}

		protected override bool ShouldRowContributeToHeightCore(int row)
		{
			if (IsForeachProcessed && Children.Count == 0)
			{
				return true;
			}

			var firstColumnContent = ParentReport.WorkSheetCurrentlyBeingProcessed[row, 0].ToString();
			return !SectionForeachArea.IsSectionForeachAreaBeginStart(firstColumnContent) && !SectionForeachArea.IsSectionForeachAreaEndStart(firstColumnContent);
		}

		public override bool FitsInPage(int heightAvailableInPage, Page page)
		{
			return base.FitsInPage(heightAvailableInPage, page) && !HasHPageBreak;
		}

		bool HasHPageBreak
		{
			get
			{
				var rowIndex = StartOfBody;
				while (rowIndex <= End)
				{
					if (HasHPageBreakAtRow(rowIndex))
					{
						return true;
					}
					rowIndex++;
				}
				return false;
			}
		}
	}
}
