using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.DocumentEngine.Constants;

namespace Enterprise.DocumentEngine.Visualisation
{
	class RendererSectionBody : RendererGeneral
	{
		public RendererSectionBody(SectionBodyArea sectionBodyArea)
			: base(sectionBodyArea)
		{
			Argument.NotNull(sectionBodyArea, "sectionBodyArea");

			this.sectionBodyArea = sectionBodyArea;
			this.dataTableName = VisualiserDataSet.GetTableName(sectionBodyArea.TableName.Replace(".", "_"));

			var dataRowSource = sectionBodyArea.DataRowSource;
			var businessObjectDataSource = dataRowSource as BusinessObjectDataSource;
			if (businessObjectDataSource != null)
			{
				this.dataTableRowCount = GetRowCountExcludingNull(businessObjectDataSource);
			}
			else
			{
				this.dataTableRowCount = dataRowSource.RowCount;
			}
		}

		readonly SectionBodyArea sectionBodyArea;
		readonly string dataTableName;
		readonly int dataTableRowCount;

		int heightForXls;
		bool createComponentsAndDataContainersHasBeenRun;

		int GetRowCountExcludingNull(BusinessObjectDataSource businessObjectDataSource)
		{
			return businessObjectDataSource.GroupedOrFilteredCollection.Count((item) => { return item != null; });
		}

		protected virtual Area AreaContainingFields
		{
			get { return sectionBodyArea; }
		}

		public override int GetHeightInXL()
		{
			if (!createComponentsAndDataContainersHasBeenRun)
			{
				throw new InvalidOperationException("You need to call CreateComponentsAndDataContainers before calling GetHeight on RendererSectionBody.");
			}

			int height = heightForXls;
			if (heightForXls > 0 && AreaContainingFields != null && AreaContainingFields.HeightInXls > heightForXls)
			{
				height = AreaContainingFields.HeightInXls + (AreaContainingFields.ParentReport == null ? 0 : AreaContainingFields.ParentReport.WorkSheetCurrentlyBeingProcessed.GetRowHeight(sectionBodyArea.StartingRow)); //real height + header row
			}

			return height;
		}

		protected override List<VisualiserComponent> CreateComponentsAndDataContainers(ExcelWorkSheet excelWorkSheet, VisualiserDataSet visualiserDataSet, int yOffset, List<string> addedDataTableNames)
		{
			var result = new List<VisualiserComponent>();

			CreateTableInDataSetIfNotAlreadyThere(visualiserDataSet);

			var columnCount = excelWorkSheet.ColumnCount;
			var firstColumnInThisAreaWithSomethingInIt = columnCount - 1;
			var lastColumnInThisAreaWithSomethingInIt = 0;
			var containsCellWithContents = false;

			var foreachAreas = new List<(string cell, int row)>();
			var currentForeachArea = (cell: string.Empty, row: -1);
			if (AreaContainingFields is SectionBodyArea sectionBodyArea)
			{
				foreachAreas = sectionBodyArea.GetSectionForeachTags();
				if (foreachAreas.Any())
				{
					var tagMessage = string.Join(System.Environment.NewLine, foreachAreas.Select(a => a.cell + (NoResString)", Row: " + a.row).ToArray());
					currentForeachArea = foreachAreas.First();

					if (!sectionBodyArea.ValidateSectionForeachTags(foreachAreas))
					{
						sectionBodyArea.ParentReport.ErrorManager.Add(
							new ReportProcessingError(Res.GetString("A0A31F91-6D58-4E97-908F-36E78DFC644E", "Error when processing {0}, please check documentation of {0} for how to use it.{1}{2}", nameof(SectionForeachArea), System.Environment.NewLine, tagMessage),
							ReportProcessingErrorSeverity.Error));
					}
					else
					{
						sectionBodyArea.ParentReport.ErrorManager.Add(
							new ReportProcessingError(Res.GetString("F26D009F-BB5C-467E-8AAD-15601C654C99", "This feature can not allow users to modify data in {0}. {1}{2}", nameof(SectionFooterArea), System.Environment.NewLine, tagMessage),
							ReportProcessingErrorSeverity.Warning));
					}
				}
			}

			var foreachAreaVisitedCount = 0;
			for (var row = AreaContainingFields.StartingRow; row <= AreaContainingFields.End; row++)
			{
				if (currentForeachArea.row == row)
				{
					if (currentForeachArea.cell.Contains(SectionForeachTags.BeginForeach))
					{
						foreachAreaVisitedCount++;
					}

					if (currentForeachArea.cell.Contains(SectionForeachTags.EndForeach))
					{
						foreachAreaVisitedCount--;
					}

					foreachAreas.RemoveAt(0);
					if (foreachAreas.Any())
					{
						currentForeachArea = foreachAreas.First();
					}
					continue;
				}

				if (foreachAreaVisitedCount > 0)
				{
					continue;
				}

				for (var column = 1; column < columnCount; column++)
				{
					var cellContent = excelWorkSheet[row, column].ToString();
					if (!string.IsNullOrEmpty(cellContent))
					{
						containsCellWithContents = true;
						firstColumnInThisAreaWithSomethingInIt = Math.Min(column, firstColumnInThisAreaWithSomethingInIt);
						var mergedCellRange = excelWorkSheet.GetMergedCellRange(row, column);
						lastColumnInThisAreaWithSomethingInIt = Math.Max(mergedCellRange.Right, lastColumnInThisAreaWithSomethingInIt);

						var cellFormat = excelWorkSheet.GetCellFormat(row, column);
						var cellSize = SizeProvider.GetCellSizeForGUI(row, row, column, column);

						AddColumnToDataTableIfMacroEvaluatesToDBOrBOValueProviderAndNotAddedAlready(visualiserDataSet, cellContent, cellFormat, cellSize);
					}
				}
			}

			if (!addedDataTableNames.Contains(dataTableName))
			{
				if (!containsCellWithContents)
				{
					firstColumnInThisAreaWithSomethingInIt = 1;
					lastColumnInThisAreaWithSomethingInIt = 72;
				}

				var grid = GetNewVisualComponentGrid(
					visualiserDataSet,
					excelWorkSheet,
					yOffset,
					firstColumnInThisAreaWithSomethingInIt,
					lastColumnInThisAreaWithSomethingInIt);

				result.Add(grid);
				addedDataTableNames.Add(dataTableName);
			}

			createComponentsAndDataContainersHasBeenRun = true;

			return result;
		}

		void CreateTableInDataSetIfNotAlreadyThere(VisualiserDataSet visualiserDataSet)
		{
			if (!visualiserDataSet.Tables.Contains(dataTableName))
			{
				visualiserDataSet.Tables.Add(dataTableName);
				for (int i = 0; i < dataTableRowCount; i++)
				{
					visualiserDataSet.Tables[dataTableName].Rows.Add(Array.Empty<object>());
				}
			}
		}

		VisualiserComponentGrid GetNewVisualComponentGrid(VisualiserDataSet visualiserDataSet, ExcelWorkSheet excelWorkSheet, int yOffset, int firstColumnInThisAreaWithSomethingInIt, int lastColumnInThisAreaWithSomethingInIt)
		{
			int heightInPixels = 120;
			heightForXls = (int)(heightInPixels * ExcelWorkSheet.XlsHeigthToFormHeightDivider);

			var topLeftCorner = SizeProvider.GetUpperLeftCornerOfCellForGUI(AreaContainingFields.StartOfBody, AreaContainingFields.StartOfBody, firstColumnInThisAreaWithSomethingInIt, yOffset);
			var topRightCorner = SizeProvider.GetUpperLeftCornerOfCellForGUI(AreaContainingFields.StartOfBody, AreaContainingFields.StartOfBody, lastColumnInThisAreaWithSomethingInIt + 1, yOffset);
			var gridHeightOffset = SizeProvider.GetSumOfCellHeightOffsets(AreaContainingFields.StartOfBody, AreaContainingFields.End);
			var unscalesTopRightWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(topRightCorner.X);
			var unscaleTopLeftWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(topLeftCorner.X);
			var unscalesgridHeightOffsetHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(gridHeightOffset);
			var gridSize = ControlDpiScalingHelper.NewScaledSize(unscalesTopRightWidth - unscaleTopLeftWidth, heightInPixels + unscalesgridHeightOffsetHeight);

			return new VisualiserComponentGrid(topLeftCorner, gridSize, visualiserDataSet, dataTableName, gridHeightOffset != 0);
		}

		void AddColumnToDataTableIfMacroEvaluatesToDBOrBOValueProviderAndNotAddedAlready(VisualiserDataSet visualiserDataSet, string cellContent, CellFormat format, Size cellSize)
		{
			foreach (Match match in RegexProvider.InnermostMacrosRegex.Matches(cellContent))
			{
				ValueProvider valueProvider = AreaContainingFields.ParentReport.MacroTranslator.GetValueProvider(Passes.SecondPass, match.Value);
				if (valueProvider is DBOrBOValueProvider)
				{
					string macroText = match.Value.Trim();
					AddColumnToDataTableIfNotAddedAlready(visualiserDataSet, macroText.Substring(1, macroText.Length - 2), format, cellSize); // Remove outer < and >.
				}
				else if (valueProvider is ModifiableField)
				{
					AddColumnToDataTableIfNotAddedAlready(visualiserDataSet, match.Value.Trim(), format, cellSize);
				}
				else if (valueProvider is Total)
				{
					var regex = RegexProvider.TotalMacroRegexMatchingToFirstEndingBracket;
					var totalMacroMatch = regex.Match(cellContent);
					if (totalMacroMatch.Success)
					{
						var columnDataSource = totalMacroMatch.Groups[1].Value;
						AddColumnToDataTableIfNotAddedAlready(visualiserDataSet, columnDataSource, format, cellSize);
					}
				}
			}
		}

		protected void AddColumnToDataTableIfNotAddedAlready(VisualiserDataSet visualiserDataSet, string columnDataSource, CellFormat format, Size cellSize)
		{
			visualiserDataSet.AddColumnToDataTableIfNotAddedAlreadyWithStyle(dataTableName, AreaContainingFields, dataTableRowCount, columnDataSource, format, cellSize);
		}
	}
}
