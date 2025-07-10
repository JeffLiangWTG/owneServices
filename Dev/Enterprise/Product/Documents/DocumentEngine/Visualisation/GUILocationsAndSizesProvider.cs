using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;

namespace Enterprise.DocumentEngine.Visualisation
{
	class GUILocationsAndSizesProvider
	{
		public GUILocationsAndSizesProvider(ExcelWorkSheet workSheet)
		{
			this.workSheet = workSheet;
		}
		readonly ExcelWorkSheet workSheet;

		const int MinimumCellHeightWhenUsingAutoHeight = 60;

		public Size GetCellSizeForGUI(int rowNumber, int columnNumber)
		{
			return GetCellSizeForGUI(rowNumber, rowNumber, columnNumber, columnNumber);
		}

		public Size GetCellSizeForGUI(int rowFrom, int rowTo, int columnFrom, int columnTo)
		{
			return GetCellSizeForGUI(rowFrom, rowTo, columnFrom, columnTo, false);
		}

		public Size GetCellSizeForGUI(int rowFrom, int rowTo, int columnFrom, int columnTo, bool ignoreMergedCells)
		{
			var cellRangeSizeInXL = GetCellRangeSizeInXL(rowFrom, rowTo, columnFrom, columnTo, ignoreMergedCells);

			var widthUnscaled = cellRangeSizeInXL.Width / ExcelWorkSheet.XlsWidthToFormWidthDivider;
			var heightUnscaled = (int)(cellRangeSizeInXL.Height / ExcelWorkSheet.XlsHeigthToFormHeightDivider);

			if (AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(workSheet[rowFrom, columnFrom].ToString()))
			{
				heightUnscaled = Math.Max(MinimumCellHeightWhenUsingAutoHeight, heightUnscaled);
			}

			return ControlDpiScalingHelper.NewScaledSize(widthUnscaled, heightUnscaled);
		}

		public Point GetUpperLeftCornerOfCellForGUI(int firstRowOfArea, int row, int column, int yOffsetScaled)
		{
			int xInXL = 0;
			int yInXL = yOffsetScaled;

			for (int rowIndex = firstRowOfArea; rowIndex < row; rowIndex++)
			{
				yInXL += workSheet.GetRowHeight(rowIndex);
			}

			for (int columnIndex = 1; columnIndex < column; columnIndex++)
			{
				xInXL += workSheet.GetColWidth(columnIndex);
			}

			var additionalYOffsetsScaled = GetSumOfCellHeightOffsets(0, row - 1);
			var xUnscaled = xInXL / ExcelWorkSheet.XlsWidthToFormWidthDivider;
			var yUnscaled = yInXL / ExcelWorkSheet.XlsHeigthToFormHeightDivider;

			return ControlDpiScalingHelper.NewScaledPoint(
				ControlDpiScalingHelper.ScaleToCurrentDpiX(xUnscaled),
				ControlDpiScalingHelper.ScaleToCurrentDpiY((int)yUnscaled) + additionalYOffsetsScaled, false);
		}

		public Size GetComponentSize(MacroTranslator macroTranslator, int rowNumber, int columnNumber, int columnFrom, int columnTo, string cellContent)
		{
			var resultScaled = GetCellSizeForGUI(rowNumber, rowNumber, columnFrom, columnTo);
			if (RegexProvider.SingleMacroOnlyRegex.IsMatch(cellContent.Trim()))
			{
				var valueProvider = macroTranslator.GetValueProvider(Passes.SecondPass, cellContent.Trim());
				var controlSizeProvider = valueProvider as IControlSizeProvider;
				if (controlSizeProvider != null)
				{
					var cellRange = controlSizeProvider.GetCellRange(cellContent.Trim());
					resultScaled = GetCellSizeForGUI(rowNumber, rowNumber + cellRange.Height - 1, columnNumber, columnNumber + cellRange.Width - 1);
				}
			}
			return resultScaled;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Excel's units are uniquely scaled")]
		Size GetCellRangeSizeInXL(int startRow, int endRow, int startCol, int endCol, bool ignoreMergedCells)
		{
			var result = new Size(0, 0);
			workSheet.ParentExcelInterface.Xls.ActiveSheet = workSheet.WorkSheetNumber;

			if (!ignoreMergedCells)
			{
				var startRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(startRow + 1, startCol + 1);
				var endRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(endRow + 1, endCol + 1);

				for (int x = startRange.Left; x <= endRange.Right; x++)
				{
					result.Width += workSheet.GetColWidth(x - 1);
				}

				for (int y = startRange.Top; y <= endRange.Bottom; y++)
				{
					result.Height += workSheet.GetRowHeight(y - 1);
				}
			}
			else
			{
				for (int x = (startCol + 1); x <= (endCol + 1); x++)
				{
					result.Width += workSheet.GetColWidth(x - 1);
				}

				for (int y = (startRow + 1); y <= (endRow + 1); y++)
				{
					result.Height += workSheet.GetRowHeight(y - 1);
				}
			}

			return result;
		}

		Dictionary<int, int> GuiCellHeightOffSets
		{
			get
			{
				if (guiCellHeightOffsets == null)
				{
					guiCellHeightOffsets = new Dictionary<int, int>();
					for (int rowIndex = 0; rowIndex < workSheet.RowCount; rowIndex++)
					{
						guiCellHeightOffsets.Add(rowIndex, 0);

						for (int columnIndex = 0; columnIndex < workSheet.ColumnCount; columnIndex++)
						{
							if (AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(workSheet[rowIndex, columnIndex].ToString()))
							{
								guiCellHeightOffsets[rowIndex] = Math.Max(guiCellHeightOffsets[rowIndex], GetCellSizeForGUI(rowIndex, columnIndex).Height);
							}
						}
					}
				}

				return guiCellHeightOffsets;
			}
		}
		Dictionary<int, int> guiCellHeightOffsets;

		internal int GetSumOfCellHeightOffsets(int rowFrom, int rowTo)
		{
			int result = 0;

			for (int rowIndex = rowFrom; rowIndex <= rowTo; rowIndex++)
			{
				if (GuiCellHeightOffSets.ContainsKey(rowIndex))
				{
					result += GuiCellHeightOffSets[rowIndex];
				}
			}

			return result;
		}
	}
}
