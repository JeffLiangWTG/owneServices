using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	sealed class XlsFileBuilder
	{
		public XlsFileBuilder(IWorksheet worksheet)
		{
			this.worksheet = Argument.NotNull(worksheet, nameof(worksheet));
		}

		readonly IWorksheet worksheet;

		const int maxLengthOfSheetName = 31;
		const int maxLengthOfSheetAddOnName = 4;

		PointF Dpi
		{
			get
			{
				if (!dpi.HasValue)
				{
					dpi = Util.GetDpi();
				}

				return dpi.Value;
			}
		}

		PointF? dpi;

		public XlsFile Build()
		{
			var xls = new XlsFile(true);

			//Create a new Excel file with 1 sheet
			xls.NewFile(1, TExcelFileFormat.v2010);

			var sheetNames = new List<string>();

			var worksheetName = !string.IsNullOrWhiteSpace(worksheet.Name)
				? worksheet.Name
				: "doc"; // SuppressCodeSmell Reason = developer constant

			var availableWorksheetNameLength = maxLengthOfSheetName - maxLengthOfSheetAddOnName;

			if (worksheetName.Length > availableWorksheetNameLength)
			{
				worksheetName = worksheetName.Substring(0, availableWorksheetNameLength);
			}

			sheetNames.Add(worksheetName);

			var sheetsWithTheSameName = sheetNames
				.Count(s => s == worksheetName);

			if (sheetsWithTheSameName > 1)
			{
				worksheetName = string.Format(CultureInfo.InvariantCulture, "{0}({1})", worksheetName, sheetsWithTheSameName);
			}

			if (string.Compare(xls.SheetName, worksheetName, StringComparison.OrdinalIgnoreCase) != 0)
			{
				xls.SheetName = worksheetName;
			}

			CreatePrinterSettings(xls);
			CreateMargins(xls);
			CreateRows(xls);
			CreateColumns(xls);
			CreateCells(xls);

			foreach (var pageBreak in worksheet.PageBreaks)
			{
				xls.InsertHPageBreak(pageBreak);
			}

			xls.ScrollWindow(1, 1);
			xls.SheetZoom = 100;
			xls.PrintLandscape = worksheet.IsLandscape();
			xls.PrintHCentered = worksheet.PrintContentCenteredHorizontally;

			return xls;
		}

		#region PrinterSettings

		void CreatePrinterSettings(XlsFile xls)
		{
			var headersAndFooters = new THeaderAndFooter();
			headersAndFooters.AlignMargins = false;
			headersAndFooters.ScaleWithDoc = true;
			headersAndFooters.DiffFirstPage = false;
			headersAndFooters.DiffEvenPages = false;
			headersAndFooters.DefaultHeader = "";
			headersAndFooters.DefaultFooter = "";
			headersAndFooters.FirstHeader = "";
			headersAndFooters.FirstFooter = "";
			headersAndFooters.EvenHeader = "";
			headersAndFooters.EvenFooter = "";
			xls.SetPageHeaderAndFooter(headersAndFooters);
		}

		#endregion

		#region Margins

		void CreateMargins(XlsFile xls)
		{
			xls.SetPrintMargins(
				new TXlsMargins(
					worksheet.Margins.Left / 100d,
					worksheet.Margins.Top / 100d,
					worksheet.Margins.Right / 100d,
					worksheet.Margins.Bottom / 100d,
					worksheet.Margins.Header / 100d,
					worksheet.Margins.Footer / 100d));

			xls.PrintXResolution = 600;
			xls.PrintYResolution = 600;
			xls.PrintOptions = TPrintOptions.Orientation;

			TPaperSize paperSize;
			xls.PrintPaperSize = Enum.TryParse(worksheet.PageDimensions.PaperName, out paperSize)
				? paperSize
				: TPaperSize.A4;
		}

		#endregion

		#region Rows

		void CreateRows(XlsFile xls)
		{
			for (int rowNumber = 1; rowNumber <= worksheet.Rows.Count; rowNumber++)
			{
				const double rowHeightCorrectionMultiplier = 0.9999;

				var row = worksheet.Rows.GetAt(rowNumber);
				var rowHeight = row.Height * ExcelMetrics.RowMultDisplay(xls) * rowHeightCorrectionMultiplier;
				xls.SetAutoRowHeight(rowNumber, false);
				xls.SetRowHeight(rowNumber, (int)rowHeight);
			}
		}

		#endregion

		#region Columns

		void CreateColumns(XlsFile xls)
		{
			for (int columnNumber = 1; columnNumber <= worksheet.Columns.Count; columnNumber++)
			{
				var column = worksheet.Columns.GetAt(columnNumber);
				var columnWidth = column.Width * ExcelMetrics.ColMultDisplay(xls);
				xls.SetColWidth(columnNumber, (int)columnWidth);
			}
		}

		#endregion

		#region Cells

		void CreateCells(XlsFile xls)
		{
			using (var shrinkToFitCalculator = new ShrinkToFitCalculator())
			{
				for (int rowNumber = 1; rowNumber <= worksheet.Rows.Count; rowNumber++)
				{
					for (int columnNumber = 1; columnNumber <= worksheet.Columns.Count; columnNumber++)
					{
						var cell = worksheet.GetCell(rowNumber, columnNumber);

						CreateCellFormat(xls, shrinkToFitCalculator, rowNumber, columnNumber, cell);

						var hasBeenProcessed = cell.TopRow != rowNumber || cell.LeftColumn != columnNumber;

						if (hasBeenProcessed)
						{
							continue;
						}

						var isMergedCell = cell.BottomRow > cell.TopRow || cell.RightColumn > cell.LeftColumn;

						if (isMergedCell)
						{
							xls.MergeCells(cell.TopRow, cell.LeftColumn, cell.BottomRow, cell.RightColumn);
						}

						switch (cell.Value)
						{
							case string str:
								var value = str.Replace("\r\n", "\n"); // Excel doesn't like \r\n, likes only char(10) which is \n
								xls.SetCellValue(rowNumber, columnNumber, value);
								break;

							default:
								xls.SetCellValue(rowNumber, columnNumber, cell.Value);
								break;
						}

						if (cell.Drawing != null)
						{
							CreateImage(xls, cell.Drawing);
						}
					}
				}
			}
		}

		void CreateCellFormat(XlsFile xls, ShrinkToFitCalculator shrinkToFitCalculator, int rowNumber, int columnNumber, ICell cell)
		{
			if (cell.Format?.IsEmpty() ?? true)
			{
				return;
			}

			var format = xls.GetCellVisibleFormatDef(rowNumber, columnNumber);

			if (cell.Format.Font != null)
			{
				format.Font = new TFlxFont();
				format.Font.Name = cell.Format.Font.Name;

				if (cell.HasDynamicContent
					&& cell.Value != null)
				{
					var stringFormat = WorksheetExtensions.GetStringFormat(cell.Format.HAlignment,
						cell.Format.VAlignment,
						cell.Format.WrapText);

					var cellSize = new SizeF((float)cell.Width, (float)cell.Height);
					var cellContent = Convert.ToString(cell.Value, CultureInfo.InvariantCulture);
					var availableTextArea = PaintAreaCalculator.CalculateTextPaintAreaSize(Dpi, cellSize, cell.Padding, cellContent, cell.Format.Font, 1, cell.Format.WrapText);
					var fontSize = shrinkToFitCalculator.ShrinkToFit(cellContent, availableTextArea, cell.Format.Font, stringFormat);

					format.Font.Size20 = (int)(fontSize * 20);
				}
				else
				{
					format.Font.Size20 = (int)cell.Format.Font.Size * 20;
				}

				format.Font.Family = 3;
				format.Font.Style = cell.Format.Font.Style.ToFlxFontStyle();
				format.Font.Color = cell.Format.Font.Color;
				format.Rotation = cell.Format.Font.Rotation;
			}

			format.HAlignment = cell.Format.HAlignment.ToHFlxAlignment();
			format.VAlignment = cell.Format.VAlignment.ToVFlxAlignment();

			if (cell.Format.Borders != null)
			{
				format.Borders = new TFlxBorders();
				format.Borders.Top.Style = cell.Format.Borders.Top.Style.ToFlxBorder();
				format.Borders.Top.Color = cell.Format.Borders.Top.Color;

				format.Borders.Left.Style = cell.Format.Borders.Left.Style.ToFlxBorder();
				format.Borders.Left.Color = cell.Format.Borders.Left.Color;

				format.Borders.Bottom.Style = cell.Format.Borders.Bottom.Style.ToFlxBorder();
				format.Borders.Bottom.Color = cell.Format.Borders.Bottom.Color;

				format.Borders.Right.Style = cell.Format.Borders.Right.Style.ToFlxBorder();
				format.Borders.Right.Color = cell.Format.Borders.Right.Color;

				if (cell.Format.Borders.DiagonalUp.Style != BorderStyle.None
					&& cell.Format.Borders.DiagonalDown.Style != BorderStyle.None)
				{
					format.Borders.Diagonal = new TFlxOneBorder(
						cell.Format.Borders.DiagonalUp.Style.ToFlxBorder(),
						cell.Format.Borders.DiagonalUp.Color);

					format.Borders.DiagonalStyle = TFlxDiagonalBorder.Both;
				}
				else if (cell.Format.Borders.DiagonalUp.Style != BorderStyle.None)
				{
					format.Borders.Diagonal = new TFlxOneBorder(
						cell.Format.Borders.DiagonalUp.Style.ToFlxBorder(),
						cell.Format.Borders.DiagonalUp.Color);

					format.Borders.DiagonalStyle = TFlxDiagonalBorder.DiagUp;
				}
				else if (cell.Format.Borders.DiagonalDown.Style != BorderStyle.None)
				{
					format.Borders.Diagonal = new TFlxOneBorder(
						cell.Format.Borders.DiagonalDown.Style.ToFlxBorder(),
						cell.Format.Borders.DiagonalDown.Color);

					format.Borders.DiagonalStyle = TFlxDiagonalBorder.DiagDown;
				}
			}

			var backgroundColor = cell.Format.BackgroundColor;

			format.FillPattern.FgColor = Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
			format.FillPattern.Pattern = cell.Format.Pattern.ToTFlxPattern();

			format.Format = cell.Format.TextFormat ?? string.Empty;
			format.WrapText = cell.Format.WrapText;

			xls.SetCellFormat(rowNumber, columnNumber, xls.AddFormat(format));
		}

		#endregion

		#region Images

		void CreateImage(XlsFile xls, IDrawing drawing)
		{
			if (drawing?.ImageData == null)
			{
				return;
			}

			var imageProperties = new TImageProperties();

			imageProperties.ShapeName = drawing.Name;

			var dx1 = Convert.ToInt32((drawing.LeftColumnOffset * FlexCelDrawing.MaxColumnOffset) / 100d);
			var dx2 = Convert.ToInt32((drawing.RightColumnOffset * FlexCelDrawing.MaxColumnOffset) / 100d);

			var dy1 = Convert.ToInt32((drawing.TopRowOffset * FlexCelDrawing.MaxRowOffset) / 100d);
			var dy2 = Convert.ToInt32((drawing.BottomRowOffset * FlexCelDrawing.MaxRowOffset) / 100d);

			imageProperties.Anchor = new TClientAnchor(TFlxAnchorType.MoveAndResize,
				drawing.TopRow, dy1,
				drawing.LeftColumn, dx1,
				drawing.BottomRow, dy2,
				drawing.RightColumn, dx2);

			try
			{
				xls.AddImage(drawing.ImageData.ToArray(), imageProperties);
			}
			catch (System.ArgumentException)
			{
				return;
			}
		}

		#endregion
	}
}
