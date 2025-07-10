using System.Collections.Generic;
using Enterprise.DocumentEngine.FlexCelInterface;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.DocBuilder
{
	internal class RightToLeftTransformer
	{
		public RightToLeftTransformer(ExcelWorkSheet worksheet)
		{
			this.worksheet = worksheet;
		}

		public void RightToLeft()
		{
			ReverseColumns();
			ReverseFormatting();
		}

		void ReverseColumns()
		{
			var originalMergedCells = new List<TXlsCellRange>();
			var xls = worksheet.ParentExcelInterface.Xls;
			var cellMergedListCount = xls.CellMergedListCount;
			for (var i = 1; i <= cellMergedListCount; i++)
			{
				originalMergedCells.Add(xls.CellMergedList(i));
			}

			var originalObjectAnchor = new Dictionary<int, TClientAnchor>();
			for (var i = 1; i <= xls.ObjectCount; i++)
			{
				var objectAnchor = xls.GetObjectProperties(i, true).Anchor;
				originalObjectAnchor[i] = objectAnchor;
			}

			if (worksheet.ColumnCount == 256)
			{
				worksheet.DeleteColumn(255);
			}
			for (var i = 1; i < ColumnCount - 1; i++)
			{
				worksheet.MoveColumns(ColumnCount - 1, i);
			}

			for (var i = 1; i <= xls.ObjectCount; i++)
			{
				var anchor = originalObjectAnchor[i];
				var originalCol1 = anchor.Col1;
				var originalCol2 = anchor.Col2;
				anchor.Col1 = 2 + (ColumnCount - originalCol2);
				anchor.Col2 = 2 + (ColumnCount - originalCol1);
				xls.SetObjectAnchor(i, anchor);
			}

			var stillMergedCells = new HashSet<TXlsCellRange>();
			cellMergedListCount = xls.CellMergedListCount;
			for (var i = 1; i < cellMergedListCount; i++)
			{
				stillMergedCells.Add(xls.CellMergedList(i));
			}
			foreach (var originalMerged in originalMergedCells)
			{
				if (!stillMergedCells.Contains(originalMerged))
				{
					var newLeft = 2 + ColumnCount - originalMerged.Right;
					var newRight = 2 + ColumnCount - originalMerged.Left;
					xls.SetCellValue(originalMerged.Top, newLeft, xls.GetCellValue(originalMerged.Top, newRight));
					xls.SetCellValue(originalMerged.Top, newRight, string.Empty);

					var oldLeftFormat = xls.GetFormat(xls.GetCellFormat(originalMerged.Top, newRight));
					var oldRightformat = xls.GetFormat(xls.GetCellFormat(originalMerged.Top, newLeft));
					var newLeftFormat = (TFlxFormat)oldLeftFormat.Clone();
					newLeftFormat.Borders = oldRightformat.Borders;
					xls.SetCellFormat(originalMerged.Top, newLeft, xls.AddFormat(newLeftFormat));

					xls.MergeCells(originalMerged.Top, newLeft, originalMerged.Bottom, newRight);
				}
			}

			worksheet.ReCalc();
		}

		void ReverseFormatting()
		{
			int formatCount = worksheet.ParentExcelInterface.Xls.FormatCount;
			for (int i = 0; i < formatCount; i++)
			{
				var format = worksheet.ParentExcelInterface.Xls.GetFormat(i);

				if (format.HAlignment == THFlxAlignment.left || format.HAlignment == THFlxAlignment.general)
				{
					format.HAlignment = THFlxAlignment.right;
				}
				else if (format.HAlignment == THFlxAlignment.right)
				{
					format.HAlignment = THFlxAlignment.left;
				}

				var right = format.Borders.Right;
				format.Borders.Right = format.Borders.Left;
				format.Borders.Left = right;

				worksheet.ParentExcelInterface.Xls.SetFormat(i, format);
			}
		}

		int ColumnCount
		{
			get
			{
				if (columnCount == -1)
				{
					for (int row = 0; row <= worksheet.RowCount; row++)
					{
						for (int col = 1; col < MaxVisibleColumn; col++)
						{
							if (!worksheet.IsCellEmpty(row, col) || !worksheet.GetCellFormat(row, col).Borders.IsEmpty)
							{
								var lastColOfCell = worksheet.GetCellsLastColumn(row, col) + 1;
								if (lastColOfCell > columnCount)
								{
									columnCount = lastColOfCell;
								}
							}
						}
					}
				}
				return columnCount;
			}
		}
		int columnCount = -1;

		int MaxVisibleColumn
		{
			get
			{
				if (maxVisibleColumn == -1)
				{
					for (int row = 0; row <= worksheet.RowCount; row++)
					{
						if (worksheet.GetCell(row, 0).ValueSourceText == "HideColumnIf")
						{
							for (int col = 1; col < worksheet.ColumnCount; col++)
							{
								if (!string.IsNullOrEmpty(worksheet.GetCell(row, col).ValueSourceText))
								{
									return maxVisibleColumn = col;
								}
							}
						}
					}
				}
				return maxVisibleColumn;
			}
		}
		int maxVisibleColumn = -1;

		readonly ExcelWorkSheet worksheet;
	}
}
