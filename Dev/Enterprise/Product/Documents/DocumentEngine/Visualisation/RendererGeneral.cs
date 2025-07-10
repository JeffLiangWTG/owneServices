using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngineIntegration;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Visualisation
{
	class RendererGeneral
	{
		public RendererGeneral(Area areaToRender)
		{
			Argument.NotNull(areaToRender, "areaToRender");

			AreaToRender = areaToRender;
			SizeProvider = new GUILocationsAndSizesProvider(areaToRender.ParentReport.WorkSheetCurrentlyBeingProcessed);
		}

		readonly Area AreaToRender;
		protected readonly GUILocationsAndSizesProvider SizeProvider;

		public virtual int GetHeightInXL()
		{
			return AreaToRender.HeightInXls + 5;
		}

		public List<VisualiserComponent> RenderSection(ExcelWorkSheet workSheet, VisualiserDataSet dSToBindTo, int yOffset, List<string> addedDataTableNames)
		{
			List<VisualiserComponent> result = new List<VisualiserComponent>();
			result.AddRange(CreateComponentsAndDataContainers(workSheet, dSToBindTo, yOffset, addedDataTableNames));
			result.AddRange(GetImagesPresentInArea(workSheet, yOffset));
			return result;
		}

		protected virtual List<VisualiserComponent> CreateComponentsAndDataContainers(ExcelWorkSheet workSheet, VisualiserDataSet dsToBindTo, int yOffset, List<string> addedDataTableNames)
		{
			List<VisualiserComponent> result = new List<VisualiserComponent>();

			for (int rowNumber = AreaToRender.StartOfBody; rowNumber <= AreaToRender.End; rowNumber++)
			{
				var adjacentCombinedBorders = new List<VisualiserComponentBorder>();
				VisualiserComponentBorder lastBorderInRow = null;

				for (int columnNumber = 1; columnNumber <= AreaToRender.ColumnCount; columnNumber++)
				{
					AreaToRender.ParentReport.Renderer.CurrentColumn = columnNumber;

					var range = workSheet.ParentExcelInterface.Xls.CellMergedBounds(rowNumber + 1, columnNumber + 1);
					int columnFrom = range.Left - 1;
					int columnTo = range.Right - 1;
					if (columnTo > AreaToRender.ColumnCount)
					{
						columnTo = AreaToRender.ColumnCount;
					}

					if (!workSheet.IsColumnHidden(columnNumber))
					{
						if (!workSheet.IsCellEmpty(rowNumber, columnNumber))
						{
							if (!workSheet.IsCellMerged(rowNumber, columnNumber))
							{ // Take up the cells to the right or left if this cell's value CAN overflow into them.
								HorizontalTextAlignment alignment = workSheet.GetCellFormat(rowNumber, columnNumber).HTextAlign;
								if (alignment != HorizontalTextAlignment.Right) // General, Left, or Center.
								{
									while (columnTo < AreaToRender.ColumnCount && workSheet.IsCellEmpty(rowNumber, columnTo + 1) && workSheet.GetCellFormat(rowNumber, columnTo).Borders.Right.BorderStyle == CellBorderStyle.None)
									{
										columnTo++;
									}
								}
								if (alignment == HorizontalTextAlignment.Right || alignment == HorizontalTextAlignment.Centre)
								{
									while (columnFrom > 1 && workSheet.IsCellEmpty(rowNumber, columnFrom - 1) && workSheet.GetCellFormat(rowNumber, columnTo).Borders.Left.BorderStyle == CellBorderStyle.None)
									{
										columnFrom--;
									}
								}
							}

							VisualiserComponent cellComponent = GetVisualiserComponentForCell(workSheet, AreaToRender.StartOfBody, rowNumber, columnNumber, columnFrom, columnTo, dsToBindTo, yOffset);
							if (cellComponent != null)
							{
								result.Add(cellComponent);
							}
						}

						var borderComponent = GetVisualiserComponentBorderForCell(workSheet, AreaToRender.StartOfBody, rowNumber, columnNumber, columnFrom, columnTo, dsToBindTo, yOffset);
						if (borderComponent != null)
						{
							result.Add(borderComponent);

							if (lastBorderInRow != null)
							{
								if (CombineAdjacentBorderComponentsIfTheyMatch(lastBorderInRow, borderComponent))
								{
									foreach (var border in adjacentCombinedBorders)
									{
										ControlDpiScalingHelper.SetHeight(ref border.Size, borderComponent.Size.Height, false);
									}
									adjacentCombinedBorders.Add(lastBorderInRow);
								}
								else
								{
									adjacentCombinedBorders.Clear();
								}
							}
							lastBorderInRow = borderComponent;
						}
					}

					columnNumber = columnTo;
				}
			}

			ReadVisualiserLabelCaptions(workSheet, result);

			return result;
		}

		bool CombineAdjacentBorderComponentsIfTheyMatch(VisualiserComponentBorder leftBorder, VisualiserComponentBorder rightBorder)
		{
			if (((rightBorder.CellFormat.Borders.Left.BorderStyle == CellBorderStyle.None && leftBorder.CellFormat.Borders.Right.BorderStyle == CellBorderStyle.None)))
			{
				ControlDpiScalingHelper.SetHeight(ref leftBorder.Size, Math.Max(rightBorder.Size.Height, leftBorder.Size.Height), false);
				ControlDpiScalingHelper.SetHeight(ref rightBorder.Size, Math.Max(rightBorder.Size.Height, leftBorder.Size.Height), false);
				return true;
			}

			if ((leftBorder.CellFormat.Borders.Bottom.BorderStyle != rightBorder.CellFormat.Borders.Bottom.BorderStyle)
				&& (leftBorder.CellFormat.Borders.Bottom.BorderStyle != CellBorderStyle.None || rightBorder.CellFormat.Borders.Bottom.BorderStyle != CellBorderStyle.None)
				&& (leftBorder.CellFormat.Borders.Right.BorderStyle != rightBorder.CellFormat.Borders.Left.BorderStyle)
				&& (leftBorder.CellFormat.Borders.Right.BorderStyle != CellBorderStyle.None || rightBorder.CellFormat.Borders.Left.BorderStyle != CellBorderStyle.None))
			{
				ControlDpiScalingHelper.SetHeight(ref leftBorder.Size, Math.Max(rightBorder.Size.Height, leftBorder.Size.Height), false);
				ControlDpiScalingHelper.SetHeight(ref rightBorder.Size, Math.Max(rightBorder.Size.Height, leftBorder.Size.Height), false);
				return true;
			}

			return false;
		}

		class CellValueAndLocation
		{
			public CellValueAndLocation(int row, int column, object value)
			{
				Row = row;
				Column = column;
				Value = value;
			}
			readonly int Row;
			readonly int Column;
			readonly object Value;

			public void ResetCellValueOnWorksheet(ExcelWorkSheet workSheet)
			{
				workSheet[Row, Column] = Value;
			}
		}

		void ReadVisualiserLabelCaptions(ExcelWorkSheet workSheet, List<VisualiserComponent> components)
		{
			var originalCellValues = new List<CellValueAndLocation>();

			for (int rowNumber = AreaToRender.StartOfBody; rowNumber <= AreaToRender.End; rowNumber++)
			{
				for (int columnNumber = 0; columnNumber < workSheet.ColumnCount; columnNumber++)
				{
					AreaToRender.ParentReport.Renderer.CurrentColumn = columnNumber;

					object originalCellValue = workSheet[rowNumber, columnNumber];
					string cellValueAsString = originalCellValue.ToString();
					if (!string.IsNullOrEmpty(cellValueAsString))
					{
						if (originalCellValue is TFormula)
						{
							AreaToRender.ReplaceFormulaInCell((TFormula)originalCellValue, rowNumber, columnNumber);
						}
						else
						{
							CellFormat cellFormat = workSheet.GetCellFormat(rowNumber, columnNumber);
							workSheet[rowNumber, columnNumber] = GetEvaluatedCellValueAsString(cellValueAsString, cellFormat);
						}

						if (workSheet[rowNumber, columnNumber] != originalCellValue)
						{
							originalCellValues.Add(new CellValueAndLocation(rowNumber, columnNumber, originalCellValue));
						}
					}
				}
			}

			workSheet.ReCalc();

			foreach (VisualiserComponent component in components)
			{
				VisualiserComponentLabel label = component as VisualiserComponentLabel;
				if (label != null)
				{
					label.ReadCaption();
				}
			}

			foreach (CellValueAndLocation originalCellValue in originalCellValues)
			{
				originalCellValue.ResetCellValueOnWorksheet(workSheet);
			}
		}

		List<VisualiserComponent> GetImagesPresentInArea(ExcelWorkSheet currentWorkSheet, int yOffset)
		{
			List<VisualiserComponent> result = new List<VisualiserComponent>();

			int imageCount = currentWorkSheet.GetImageCount();
			for (int imageNumber = 0; imageNumber < imageCount; imageNumber++)
			{
				System.Drawing.Image img = currentWorkSheet.GetImage(imageNumber);
				if (img != null)
				{
					TImageProperties imageProperties = currentWorkSheet.GetImageProperties(imageNumber);
					TClientAnchor anchorOneBased = imageProperties.Anchor;
					int row1 = anchorOneBased.Row1 - 1;
					int col1 = anchorOneBased.Col1 - 1;
					int row2 = anchorOneBased.Row2 - 1;
					int col2 = anchorOneBased.Col2 - 1;
					if (row1 >= AreaToRender.StartOfBody && row1 <= AreaToRender.End)
					{
						Point guiLocation = SizeProvider.GetUpperLeftCornerOfCellForGUI(AreaToRender.StartOfBody, row1, col1, yOffset);
						Size guiSize = SizeProvider.GetCellSizeForGUI(row1, row2, col1, col2, true);
						result.Add(new VisualiserComponentImage(guiLocation, guiSize, img));
					}
				}
			}
			return result;
		}

		VisualiserComponent GetVisualiserComponentForCell(ExcelWorkSheet workSheet, int startOfArea, int rowNumber, int columnNumber, int columnFrom, int columnTo, VisualiserDataSet visualiserDataSet, int horizontalOffset)
		{
			object originalCellContent = workSheet[rowNumber, columnNumber];
			string originalCellContentAsString = originalCellContent.ToString();

			var locationScaled = SizeProvider.GetUpperLeftCornerOfCellForGUI(startOfArea, rowNumber, columnFrom, horizontalOffset);
			var sizeScaled = SizeProvider.GetComponentSize(AreaToRender.ParentReport.MacroTranslator, rowNumber, columnNumber, columnFrom, columnTo, originalCellContentAsString);
			var cellFormat = workSheet.GetCellFormat(rowNumber, columnNumber);

			originalCellContentAsString = NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly(originalCellContentAsString);

			switch (GetControlType(originalCellContent, cellFormat))
			{
				case VisualiserComponentTypes.StaticText:
					return new VisualiserComponentLabel(locationScaled, sizeScaled, delegate
					{ return GetEvaluatedCellValueAsString(workSheet[rowNumber, columnNumber].ToString(), cellFormat); }, cellFormat);

				case VisualiserComponentTypes.TextEdit:
					if (cellFormat.HTextAlign == HorizontalTextAlignment.General)
					{
						string cellContentWithoutOuterBrackets = originalCellContentAsString.Substring(1, originalCellContentAsString.Length - 2);
						Type cellValueType = VisualiserDataSet.GetColumnDataSourceType(AreaToRender, cellContentWithoutOuterBrackets);
						cellFormat.HTextAlign = VisualiserDataSet.UsesFlexCelNumericFormatting(cellValueType) ? HorizontalTextAlignment.Right : HorizontalTextAlignment.Left;
					}

					string evaluatedCellContentAsString = GetEvaluatedCellValueAsString(originalCellContentAsString, cellFormat);
					string dataSetFieldName = originalCellContentAsString.Replace(".", "");
					if (!visualiserDataSet.MainTable.Columns.Cast<DataColumn>().Any(x => x.ColumnName.Equals(dataSetFieldName, StringComparison.OrdinalIgnoreCase)))
					{
						visualiserDataSet.MainTable.Columns.Add(dataSetFieldName);
						using (visualiserDataSet.SuspendChangeTracking)
						{
							visualiserDataSet.MainRow[dataSetFieldName] = evaluatedCellContentAsString;
						}
					}
					return new VisualiserComponentTextBox(locationScaled, sizeScaled, visualiserDataSet, dataSetFieldName, cellFormat);

				case VisualiserComponentTypes.Image:
					object excelImage = AreaToRender.ReplaceMacros(originalCellContentAsString);
					if (excelImage != null && !string.IsNullOrEmpty(excelImage.ToString()))
					{
						var image = ((ExcelImage)excelImage).Image;
						if (image != null)
						{
							return new VisualiserComponentImage(locationScaled, sizeScaled, image);
						}
					}
					break;
			}
			return null;
		}

		VisualiserComponentBorder GetVisualiserComponentBorderForCell(ExcelWorkSheet workSheet, int startOfArea, int rowNumber, int columnNumber, int columnFrom, int columnTo, VisualiserDataSet visualiserDataSet, int yOffset)
		{
			object originalCellContent = workSheet[rowNumber, columnNumber];
			string originalCellContentAsString = originalCellContent.ToString();

			var componentLocation = SizeProvider.GetUpperLeftCornerOfCellForGUI(startOfArea, rowNumber, columnFrom, yOffset);
			var componentSize = SizeProvider.GetComponentSize(AreaToRender.ParentReport.MacroTranslator, rowNumber, columnNumber, columnFrom, columnTo, originalCellContentAsString);
			var cellFormat = GetCellFormatWithNeighbouringBorders(workSheet, rowNumber, columnFrom, columnTo);

			var range = workSheet.ParentExcelInterface.Xls.CellMergedBounds(rowNumber + 1, columnNumber + 1);
			if (range.Top == rowNumber + 1 && range.Left == columnNumber + 1)
			{
				if (!cellFormat.Borders.IsEmpty)
				{
					return new VisualiserComponentBorder(componentLocation, componentSize, cellFormat);
				}
			}

			return null;
		}

		CellFormat GetCellFormatWithNeighbouringBorders(ExcelWorkSheet workSheet, int row, int startCol, int endCol)
		{
			var startRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(row + 1, startCol + 1);
			var endRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(row + 1, endCol + 1);
			var cellFormat = workSheet.GetCellFormat(row, startCol);
			var endCellFormat = workSheet.GetCellFormat(row, endCol);
			cellFormat.Borders.Right = endCellFormat.Borders.Right;
			cellFormat.Borders.Bottom = endCellFormat.Borders.Bottom;

			if (cellFormat.Borders.Top.BorderStyle == CellBorderStyle.None && startRange.Top - 1 > AreaToRender.StartingRow)
			{
				var topNeighbourRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(startRange.Top - 1, startCol + 1);
				if (startRange.Left >= topNeighbourRange.Left && startRange.Right <= topNeighbourRange.Right)
				{
					cellFormat.Borders.Top = workSheet.GetCellFormat(topNeighbourRange.Bottom - 1, startCol).Borders.Bottom;
				}
			}
			if (cellFormat.Borders.Left.BorderStyle == CellBorderStyle.None && startCol > 1)
			{
				var leftNeighbourRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(row + 1, startRange.Left - 1);
				if (startRange.Top >= leftNeighbourRange.Top && startRange.Bottom <= leftNeighbourRange.Bottom)
				{
					cellFormat.Borders.Left = workSheet.GetCellFormat(row, leftNeighbourRange.Right - 1).Borders.Right;
				}
			}

			if (cellFormat.Borders.Bottom.BorderStyle == CellBorderStyle.None && row < AreaToRender.End)
			{
				var bottomNeighbourRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(endRange.Bottom + 1, endCol + 1);
				if (endRange.Left >= bottomNeighbourRange.Left && endRange.Right <= bottomNeighbourRange.Right)
				{
					cellFormat.Borders.Bottom = workSheet.GetCellFormat(bottomNeighbourRange.Top - 1, endCol).Borders.Top;
				}
			}
			if (cellFormat.Borders.Right.BorderStyle == CellBorderStyle.None && endCol < AreaToRender.ColumnCount)
			{
				var rightNeighbourRange = workSheet.ParentExcelInterface.Xls.CellMergedBounds(row + 1, endRange.Right + 1);
				if (endRange.Top >= rightNeighbourRange.Top && endRange.Bottom <= rightNeighbourRange.Bottom)
				{
					cellFormat.Borders.Right = workSheet.GetCellFormat(row, rightNeighbourRange.Left - 1).Borders.Left;
				}
			}
			return cellFormat;
		}

		string GetEvaluatedCellValueAsString(string cellContent, CellFormat cellFormat)
		{
			var result = cellContent;

			if (RegexProvider.InnermostMacrosRegex.IsMatch(cellContent) && !RegexProvider.TotalMacroRegexAnyWhereInString.IsMatch(cellContent))
			{
				result = GetMacroReplacementFormattedUsingCellFormatting(cellContent, cellFormat);
			}

			result = result.Replace("\n", System.Environment.NewLine);

			return result;
		}

		string GetMacroReplacementFormattedUsingCellFormatting(string cellContent, CellFormat cellFormat)
		{
			object result = AreaToRender.ReplaceMacros(cellContent);

			if (result is ZDecimal)
			{
				result = (decimal)(ZDecimal)result;
			}
			if (result is decimal)
			{
				var textColor = (TUIColor)cellFormat.TextColor;
				return TFlxNumberFormat.FormatValue(result, cellFormat.FormatPattern, ref textColor, null).ToString();
			}

			if (result is ZDateTimeOffset)
			{
				result = ((ZDateTimeOffset)result).ToDateTime();
			}
			if (result is ZDateTime)
			{
				result = ((ZDateTime)result).ToDateTime();
			}
			if (result is DateTime)
			{
				var textColor = (TUIColor)cellFormat.TextColor;
				return TFlxNumberFormat.FormatValue(result, cellFormat.FormatPattern, ref textColor, null).ToString();
			}

			return result.ToString();
		}

		internal VisualiserComponentTypes GetControlType(object cellContent, CellFormat cellFormat)
		{
			VisualiserComponentTypes result = VisualiserComponentTypes.Border;

			if (cellContent is TFormula)
			{
				result = VisualiserComponentTypes.StaticText;
			}
			else if (!string.IsNullOrEmpty(cellContent.ToString()))
			{
				result = VisualiserComponentTypes.StaticText;
				string cellContentAsString = NonVisualisableMacroCleaner.RemoveMacrosUsedForFormattingOnly(cellContent.ToString());
				if (RegexProvider.IsSingleMacro(cellContentAsString))
				{
					ValueProvider valueProvider = AreaToRender.ParentReport.MacroTranslator.GetValueProvider(Passes.SecondPass, cellContentAsString);
					if (valueProvider != null)
					{
						var visualiserControlProvider = valueProvider as IVisualiserComponentProvider;
						if (visualiserControlProvider != null)
						{
							result = visualiserControlProvider.ComponentType;
							if (result == VisualiserComponentTypes.TextEdit && cellFormat.FillPattern != FillPatternStyle.None && !IsModifiable(cellContentAsString))
							{
								result = VisualiserComponentTypes.StaticText;
							}
						}
					}
				}
			}
			return result;
		}

		bool IsModifiable(string cellContent)
		{
			return Modifiable.MacroRegex.IsMatch(cellContent) || ModifiableField.MacroRegex.IsMatch(cellContent);
		}
	}
}
