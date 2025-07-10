using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	public sealed class FlexCelWorksheet : XlsFile, IWorksheet, IDimensionsCalculator, IResourceProvider
	{
		#region Ctor

		FlexCelWorksheet()
		{
		}

		public static FlexCelWorksheet FromStream(Stream stream)
		{
			FlexCelWorksheet worksheet = null;

			try
			{
				stream.Seek(0, SeekOrigin.Begin);
				worksheet = new FlexCelWorksheet();
				worksheet.Open(stream);
				worksheet.ActiveSheet = 1;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//eat
			}

			return worksheet;
		}

		public static FlexCelWorksheet FromFile(string filePath)
		{
			FlexCelWorksheet worksheet = null;

			try
			{
				worksheet = new FlexCelWorksheet();
				worksheet.Open(filePath);
				worksheet.ActiveSheet = 1;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				//eat
			}

			return worksheet;
		}

		#endregion

		#region IWorksheet members

		string IWorksheet.Name
		{
			get { return SheetName; }
		}

		PageDimensions IWorksheet.PageDimensions
		{
			get
			{
				if (pageDimensions == null)
				{
					pageDimensions = new PageDimensions();

					var flxPaperDimensions = PrintPaperDimensions;
					pageDimensions.PaperName = flxPaperDimensions.PaperName;
					
					if (PrintLandscape)
					{
						pageDimensions.Height = flxPaperDimensions.Width;
						pageDimensions.Width = flxPaperDimensions.Height;
					}
					else
					{
						pageDimensions.Height = flxPaperDimensions.Height;
						pageDimensions.Width = flxPaperDimensions.Width;
					}
				}

				return pageDimensions;
			}
		}

		PageDimensions pageDimensions;

		public bool PrintContentCenteredHorizontally => PrintHCentered;

		ICell IWorksheet.GetCell(int row, int column)
		{
			return GetOrCreateCell(row, column);
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Point is not used for rendering")]
		ICell GetOrCreateCell(int row, int column)
		{
			ICell result = null;

			var location = new Point(row, column);

			if (cells == null || !cells.ContainsKey(location))
			{
				result = CreateCell(row, column);

				if (cells == null)
				{
					cells = new Dictionary<Point, ICell>();
				}

				cells.Add(location, result);
			}
			else
			{
				result = cells[location];
			}

			return result;
		}

		Dictionary<Point, ICell> cells;

		ICell CreateCell(int row, int column)
		{
			var isLocationOutOfBounds = row <= 0
				|| row > RowCount
				|| column <= 0
				|| column > ColCount;

			if (isLocationOutOfBounds)
			{
				return new Cell
				{
					TopRow = row,
					LeftColumn = column,
					BottomRow = row,
					RightColumn = column
				};
			}

			var flxCellRange = CellMergedBounds(row, column);
			var range = new FlexCelXlsRangeAdapter(flxCellRange);

			return new FlexCelCell(this, range);
		}

		internal object GetCellValueEx(int row, int column)
		{
			object result = null;

			var data = GetCellValue(row, column);

			switch (TExcelTypes.ObjectToCellType(data))
			{
				case TCellType.Formula:
					var formula = (TFormula)data;
					result = formula.Result;
					break;

				case TCellType.Number:
				case TCellType.Bool:
				case TCellType.DateTime:
				case TCellType.String:
					result = data;
					break;
			}

			return result;
		}

		public IReadOnlyList<IRow> Rows
		{
			get { return rows ?? (rows = new FlexCelRowCollection(this)); }
		}

		FlexCelRowCollection rows;

		public IReadOnlyList<IColumn> Columns
		{
			get { return columns ?? (columns = new FlexCelColumnCollection(this)); }
		}

		FlexCelColumnCollection columns;

		public IReadOnlyList<IDrawing> Drawings
		{
			get { return drawings ?? (drawings = new FlexCelDrawingsCollection(this)); }
		}

		FlexCelDrawingsCollection drawings;

		internal IFormat GetCellFormatEx(int row, int column)
		{
			var flxCellRange = CellMergedBounds(row, column);
			var range = new FlexCelXlsRangeAdapter(flxCellRange);

			return new FlexCelFormatAdapter(this, range);
		}

		public Margins Margins
		{
			get { return margins ?? (margins = CalculateMargins()); }
		}

		Margins margins;

		Margins CalculateMargins()
		{
			var flxMargins = GetPrintMargins();

			var result = new Margins
			{
				Header = 0d,
				Footer = 0d,
				Top = flxMargins.Top * 100,
				Bottom = flxMargins.Bottom * 100,
				Left = flxMargins.Left * 100,
				Right = flxMargins.Right * 100
			};

			return result;
		}

		IEnumerable<int> IWorksheet.PageBreaks
		{
			get { return Enumerable.Empty<int>(); }
		}

		#endregion

		#region IDimensionsCalculator members

		public double CalculateRowHeight(double rowHeight)
		{
			var flxCelRowHeight = rowHeight * 20;

			return flxCelRowHeight / ExcelMetrics.RowMultDisplay(this);
		}

		public double CalculateColumnWidth(double columnWidth)
		{
			var flxCelColumnWidth = columnWidth * 20;

			return flxCelColumnWidth / ExcelMetrics.ColMultDisplay(this);
		}

		#endregion

		public IReadOnlyDictionary<string, Func<object>> Resources
		{
			get { return resources ?? (resources = GetResources()); }
		}

		IReadOnlyDictionary<string, Func<object>> resources;

		IReadOnlyDictionary<string, Func<object>> GetResources()
		{
			var result = new Dictionary<string, Func<object>>();

			using (RestoreActiveSheet())
			{
				for (int sheetNumber = 1; sheetNumber <= SheetCount; sheetNumber++)
				{
					ActiveSheet = sheetNumber;

					var sheetName = SheetName;

					if (string.IsNullOrEmpty(sheetName))
					{
						continue;
					}

					for (int imageIndex = 1; imageIndex <= ImageCount; imageIndex++)
					{
						var imageName = GetImageName(imageIndex);

						if (string.IsNullOrEmpty(imageName))
						{
							continue;
						}

						var path = string.Concat(sheetName, "/", imageName);

						var lazyImage = new Lazy<object>(CreateImageGetter(sheetNumber, imageIndex));

						result[path] = () => lazyImage.Value;
					}
				}
			}

			return result;
		}

		Func<IReadOnlyCollection<byte>> CreateImageGetter(int sheetNumber, int imageIndex)
		{
			return () =>
			{
				using (RestoreActiveSheet())
				{
					ActiveSheet = sheetNumber;

					var imageType = TXlsImgType.Unknown;
					var imageData = GetImage(imageIndex, ref imageType);

					return Array.AsReadOnly(imageData);
				}
			};
		}

		IDisposable RestoreActiveSheet()
		{
			var activeSheetIndex = ActiveSheet;
			return new DisposableAction(() => ActiveSheet = activeSheetIndex);
		}
	}
}