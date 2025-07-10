using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	public sealed class ExcelWorkSheet : IExcelWorkSheet
	{
		public ExcelWorkSheet(ExcelInterface parent, string sheetName)
		{
			ParentExcelInterface = parent;
			this.SheetName = sheetName;
			ImageStreams = new Dictionary<int, Stream>();
			insertedImages = new HashSet<Image>();
		}

		internal readonly ExcelInterface ParentExcelInterface;
		public string SheetName { get; private set; }

#if DEBUG
		internal
#endif
 readonly Dictionary<int, Stream> ImageStreams;

		internal HashSet<Image> InsertedImages => insertedImages;
		readonly HashSet<Image> insertedImages;

		int fWorkSheetNumber = -1;
		public const int Max_Rows = FlxConsts.Max_Rows2007;
		public const int Max_Columns = FlxConsts.Max_Columns2007;
		public const double XlsHeigthToFormHeightDivider = 12;
		public const int XlsWidthToFormWidthDivider = 30;
		const int MaxHPageBreaks = 1021; ////Excel 2007 has 1023 pagebreaks; newer have 1026. -2 to includes the top and bottom of sheet as page breaks

		string IExcelWorkSheet.SheetName
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return SheetName; }
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public double GetTopMargin()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.GetTopMargin();
		}

		[System.Diagnostics.DebuggerStepThrough()]
		public double GetBottomMargin()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.GetBottomMargin();
		}

		public bool HasHPageBreak(int rowNumber)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.HasHPageBreak(rowNumber + 1);
		}

		public int HPageBreakCount
		{
			get
			{
				ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
				return ParentExcelInterface.Xls.HPageBreakCount;
			}
		}

		public bool HasTooManyHPageBreak => (HPageBreakCount > ExcelWorkSheet.MaxHPageBreaks);

		public int GetFirstPageBreakOverflowRow()
		{
			int currentHPageBreakCount = 0;
			int hPageBreakOverflowRow = -1;
			for (int row = 0; row < RowCount; row++)
			{
				if (HasHPageBreak(row))
				{
					currentHPageBreakCount++;
					if (currentHPageBreakCount > ExcelWorkSheet.MaxHPageBreaks)
					{
						hPageBreakOverflowRow = row;
						break;
					}
				}
			}

			return hPageBreakOverflowRow;
		}

		public int WorkSheetNumber
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get
			{
				if (fWorkSheetNumber == -1)
				{
					fWorkSheetNumber = ParentExcelInterface.GetSheetIndex(SheetName);
				}
				return fWorkSheetNumber;
			}
		}

		internal void ResetWorksheetNumber()
		{
			fWorkSheetNumber = -1;
		}

		public void Hide()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.SheetVisible = TXlsSheetVisible.VeryHidden;
		}

		public bool IsHidden
		{
			get
			{
				ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
				return ParentExcelInterface.Xls.SheetVisible != TXlsSheetVisible.Visible;
			}
		}

		public bool IsRendered { get; set; }

		public void Clear()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.ClearSheet();
		}

		public void InsertHPageBreak(int row)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.InsertHPageBreak(row + 1);
		}

		public void DeleteHPageBreak(int row)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.DeleteHPageBreak(row + 1);
		}

		public void MoveColumns(int fromColumn, int toColumn)
		{
			if (toColumn != fromColumn)
			{
				ExcelFile excelFile = ParentExcelInterface.Xls;
				excelFile.ActiveSheet = WorkSheetNumber;
				TXlsCellRange range = new TXlsCellRange(1, fromColumn + 1, Max_Rows - 1, fromColumn + 1);
				excelFile.MoveRange(range, 1, toColumn + (fromColumn < toColumn ? 2 : 1), TFlxInsertMode.ShiftColRight);
			}
		}

		public void DuplicateRows(int firstRow, int lastRow, int destinationRow, int count)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			var range = new TXlsCellRange(firstRow + 1, 1, lastRow + 1, Max_Columns);
			if (count > 0)
			{
				if ((destinationRow - 1) + (lastRow - firstRow + 1) * count > Max_Rows)
				{
					throw new DocumentEngineTooManyRowsException();
				}

				if (firstRow >= 0 && lastRow >= 0 && destinationRow >= 0 && firstRow <= lastRow && (destinationRow > lastRow || destinationRow < firstRow))
				{
					ParentExcelInterface.Xls.InsertAndCopyRange(range, destinationRow + 1, 1, count, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.All);
#if TraceFlexcel
					System.Diagnostics.Debug.WriteLine("Xls.InsertAndCopyRange(new TXlsCellRange(" + (FirstRow + 1) + ", 1, " + (LastRow + 1) + ", ColumnCount), " + (DestinationRow + 1) + ", 1, " + count + ", TFlxInsertMode.Row, TRangeCopyMode.All);");// I have my Diagnostics.Debug.Write in a #if. It is only executed if I set the conditional pre-processor. 
#endif
				}
				else
				{
					throw new DocumentEngineException(string.Format("Could not duplicate rows with parameters: WorkSheetNumber = {0}, FirstRow = {1}, LastRow = {2}, DestinationRow = {3}, count = {4}", WorkSheetNumber, firstRow, lastRow, destinationRow, count));
				}
			}
		}

		#region Indexers

		public object this[int row, int column]
		{
			get
			{
				if (ParentExcelInterface.Xls.ActiveSheet != WorkSheetNumber) // Optimization: ActiveSheet setter is slow
				{
					ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
				}

				return ParentExcelInterface.Xls.GetCellValue(row + 1, column + 1) ?? "";
			}
			set
			{
				if (ParentExcelInterface.Xls.ActiveSheet != WorkSheetNumber) // Optimization: ActiveSheet setter is slow
				{
					ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
				}

				object valueToPutInCell = value;

				if (value is FormattedCellValue)
				{
					FormattedCellValue formattedCellValue = value as FormattedCellValue;
					int cellFormatCode = ParentExcelInterface.Xls.GetCellFormat(row + 1, column + 1);
					TFlxFormat format = cellFormatCode <= 0 ? ParentExcelInterface.Xls.GetDefaultFormat : ParentExcelInterface.Xls.GetFormat(cellFormatCode);

					format.Format = formattedCellValue.Format;
					ParentExcelInterface.Xls.SetCellFormat(row + 1, column + 1, ParentExcelInterface.Xls.AddFormat(format));
					valueToPutInCell = formattedCellValue.Value;
				}

				if (value is ZBool)
				{
					valueToPutInCell = ((ZBool)value) ? "Y" : "N";
				}
				else if (value is bool)
				{
					valueToPutInCell = ((bool)value) ? "Y" : "N";
				}
				else if (value is IZType)
				{
					if (value is ZString && ((ZString)value).StartsWith((NoResString)"edient:", StringComparison.OrdinalIgnoreCase))
					{
						System.Collections.Specialized.NameValueCollection qString = System.Web.HttpUtility.ParseQueryString(value.ToString());
						string textToShow = qString["TextToShow"];
						ExcelHyperlink hyperlink = new ExcelHyperlink(THyperLinkType.URL, (textToShow ?? (textToShow = value.ToString())), value.ToString(), "", "", Res.GetString("11130cfe-0982-426e-8289-522abae0e4f2", "Click here to view in {0}", Core.Constants.ProductName));
						valueToPutInCell = hyperlink.TextToShow;
						if (hyperlink.LinkLocation.Trim().Length > 0)
						{
							InsertHyperlink(row, column, hyperlink);
						}
					}
					else if (((IZType)value).IsValid)
					{
						valueToPutInCell = ((IZTypeInternals)value).GetValueForLogicalDataLayer(false);
					}
					else
					{
						valueToPutInCell = null;
					}
				}
				else if (value is string)
				{
					valueToPutInCell = ((string)value).RemoveInvalidCharactersAndCarriageReturn();
				}
				else if (value is FlexHPageBreak)
				{
					valueToPutInCell = "";
					InsertHPageBreak(row);
				}
				else if (value is RowHider)
				{
					valueToPutInCell = "";
					SetRowHeight(row, 0);
				}
				else if (value is byte[])
				{
					valueToPutInCell = System.Text.Encoding.UTF8.GetString((byte[])value);
					if (Enterprise.ZArchitecture.Core.ORtfTextUtil.IsRtf((string)valueToPutInCell))
					{
						valueToPutInCell = Enterprise.ZArchitecture.Core.ORtfTextUtil.RtfToText((byte[])value);
					}
				}
				else if (value is TFormula)
				{
					TFormula formula = (TFormula)value;
					formula.Text = formula.Text.Replace("\r", "");

					if (formula.Result == null)
					{
						formula.Result = "";
					}

					if (string.IsNullOrEmpty(formula.Text))
					{
						formula.Text = "=0";
					}

					if (formula.Text.Trim() == "=")
					{
						valueToPutInCell = "";
					}
					else
					{
						valueToPutInCell = formula;
					}
				}
				else if (value is ExcelImage)
				{
					valueToPutInCell = "";

					var excelImage = (ExcelImage)value;
					if (!excelImage.Image.IsDisposed())
					{
						lock (excelImage.Image)
						{
							InsertPicture(row, column, excelImage.Height, excelImage.Width, excelImage);
						}
					}
					else
					{
						var cellReference = CellReference.GetCellRef(row, column);
						var cellContents = this[row, column];

						throw new InvalidOperationException(
							string.Format("You can not insert a disposed image into a worksheet. [{0}]-[{1}]-[{2}]", cellReference, cellContents, excelImage.Image.Tag));
					}
				}
				else if (value is ExcelHyperlink)
				{
					ExcelHyperlink hyperlink = (ExcelHyperlink)value;
					valueToPutInCell = hyperlink.TextToShow;
					if (hyperlink.LinkLocation.Trim().Length > 0)
					{
						InsertHyperlink(row, column, hyperlink);
					}
				}

				try
				{
					ParentExcelInterface.Xls.SetCellValue(row + 1, column + 1, valueToPutInCell);
				}
				catch (Exception exception)
				{
					if (exception.IsCriticalException())
					{ throw; }

					string valueInfo = valueToPutInCell.ToString();
					if (valueToPutInCell is TFormula)
					{
						TFormula formula = valueToPutInCell as TFormula;
						valueInfo = string.Format((NoResString)"Formula: Text = '{0}', Result = '{1}", formula.Text, formula.Result);
					}
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorSettingCellValue, string.Format("Error trying to set Cell[{0},{1}] value to '{2}' of type '{3}'", row, column, valueInfo, valueToPutInCell.GetType()), exception);
				}
			}
		}

		public object this[string cellName]
		{
			get
			{
				Point position = GetCellPositionFromName(cellName);
				return this[position.Y, position.X];
			}
			set
			{
				Point position = GetCellPositionFromName(cellName);
				this[position.Y, position.X] = value;
			}
		}

		public ExcelCell GetCell(int row, int column)
		{
			return new ExcelCell(this, row, column);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1017", Justification = "Not working in pixels")]
		public Point GetCellPositionFromName(string cellName)
		{
			TXlsNamedRange range = this.ParentExcelInterface.Xls.GetNamedRange(cellName, ParentExcelInterface.Xls.ActiveSheet) ?? throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorFindingCellRange, "Could not find range with name '" + cellName + "'");
			if (range.Top != range.Bottom ||
				range.Left != range.Right)
			{
				ErrorReporter.ReportOnce("Named cell '" + cellName + "' refers to a range of cells.", "Named cell '" + cellName + "' refers to a range of cells.");
			}
			return new Point(range.Left - 1, range.Bottom - 1);
		}

		#endregion

		public int GetCellsLastColumn(int row, int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TXlsCellRange range = ParentExcelInterface.Xls.CellMergedBounds(row + 1, col + 1);

			return range.Right - 1;
		}

		public void ReCalc()
		{
			ParentExcelInterface.Xls.Recalc(false);
		}

		public object RecalcCell(int row, int col, bool force)
		{
			return ParentExcelInterface.Xls.RecalcCell(WorkSheetNumber, row + 1, col + 1, force);
		}

		public int GetCharCountFittingInCell(int row, int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			int fontSize = GetCellFontSize(row, col);
			int cellWidth = GetCellWidth(row, col);
			return (int)(cellWidth / fontSize / 1.4);//this multiplier is found in a try/error basis.
		}

		int fRowCount = -1;
		public int RowCount
		{
			get
			{
				if (fRowCount == -1)
				{
					ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
					fRowCount = ParentExcelInterface.Xls.RowCount;
				}
				return fRowCount;
			}
		}

		int fColCount = -1;

		public void RefreshRowCount()
		{
			fRowCount = -1;
		}

		public int ColumnCount
		{
			get
			{
				if (fColCount == -1)
				{
					ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
					fColCount = ParentExcelInterface.Xls.ColCount;
				}
				return fColCount;
			}
		}

		public int ColCountInRow(int rowIndex)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.ColCountInRow(rowIndex + 1);
		}

		public int GetCellFontSize(int row, int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

			int cellFormat = ParentExcelInterface.Xls.GetCellFormat(row + 1, col + 1);
			return ParentExcelInterface.Xls.GetFormat(cellFormat).Font.Size20;
		}

		public TFlxFont GetCellFlxFont(int row, int col)
		{
			int cellFormat = ParentExcelInterface.Xls.GetCellFormat(row + 1, col + 1);
			TFlxFormat flxFormat = ParentExcelInterface.Xls.GetFormat(cellFormat);
			return flxFormat.Font;
		}

		struct FontCacheKey
		{
			public FontCacheKey(string name, int size, FontStyle style)
			{
				this.name = name;
				this.size = size;
				this.style = style;
			}
			readonly string name;
			readonly int size;
			readonly FontStyle style;

			public override int GetHashCode()
			{
				return name.GetHashCode() + 23 * size.GetHashCode() + 23 * 23 * style.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				return obj is FontCacheKey fck && fck.name.Equals(name) && fck.size.Equals(size) && fck.style.Equals(style);
			}
		}

		[ThreadStatic]
		static CargoWise.EntityFramework.MRUCache<FontCacheKey, Font> FontCache;

		public Font GetCellFont(int row, int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TFlxFont flxFont = GetCellFlxFont(row, col);
			FontStyle style = FontStyle.Regular;

			if ((flxFont.Style & TFlxFontStyles.Bold) != 0)
			{
				style |= FontStyle.Bold;
			}
			if ((flxFont.Style & TFlxFontStyles.Italic) != 0)
			{
				style |= FontStyle.Italic;
			}

			if (FontCache == null)
			{
				FontCache = new CargoWise.EntityFramework.MRUCache<FontCacheKey, Font>(20);
			}
			var fontCacheKey = new FontCacheKey(flxFont.Name, flxFont.Size20 / 20, style);
			if (FontCache.TryGetValue(fontCacheKey, out Font value))
			{
				return value;
			}
			var result = new Font(flxFont.Name, flxFont.Size20 / 20, style);
			FontCache.SetValue(fontCacheKey, result);
			return result;
		}

		public int GetCellWidth(int row, int column)
		{
			int result = 0;

			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TXlsCellRange range = ParentExcelInterface.Xls.CellMergedBounds(row + 1, column + 1);
			for (int i = range.Left; i <= range.Right; i++)
			{
				result += GetColWidth(i - 1);
			}

			return result;
		}

		public int GetCellHeight(int row, int column)
		{
			int result = 0;

			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TXlsCellRange range = ParentExcelInterface.Xls.CellMergedBounds(row + 1, column + 1);
			for (int i = range.Top; i <= range.Bottom; i++)
			{
				result += GetRowHeight(i - 1);
			}

			return result;
		}

		public void SetColWidth(int columnNumber, int width)
		{
			var trimmedWidth = Math.Min(width, UInt16.MaxValue);
			ParentExcelInterface.Xls.SetColWidth(columnNumber + 1, trimmedWidth);
		}

		public void CopyBorderSettings(int from, int to)
		{
			for (int col = 0; col < ColumnCount; col++)
			{
				TFlxFormat formatFrom = ParentExcelInterface.Xls.GetFormat(ParentExcelInterface.Xls.GetCellFormat(to + 1, col + 1));
				TFlxFormat formatTo = ParentExcelInterface.Xls.GetFormat(ParentExcelInterface.Xls.GetCellFormat(from + 1, col + 1));
				formatFrom.Borders = (TFlxBorders)formatTo.Borders.Clone();
				ParentExcelInterface.Xls.SetCellFormat(to + 1, col + 1, ParentExcelInterface.Xls.AddFormat(formatFrom));
			}
		}

		public CellFormat GetCellFormat(int row, int column)
		{
			return GetCellFormat(row, column, HorizontalTextAlignment.General);
		}

		internal CellFormat GetCellFormat(int row, int col, HorizontalTextAlignment defaultHorizontalAlignment)
		{
			var result = new CellFormat();

			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

			var fmt = ParentExcelInterface.Xls.GetFormat(ParentExcelInterface.Xls.GetCellFormat(row + 1, col + 1));

			result.FontName = fmt.Font.Name;
			result.FontSize = fmt.Font.Size20 / 20.0f;
			result.FontStyle = fmt.Font.Style.ToFontStyle();

			var backGroundColor = fmt.FillPattern.FgColor;
			result.BackgroundColor = backGroundColor.ToColor(ParentExcelInterface.Xls, Color.Empty);
			result.IsBackgroundColorAutomatic = backGroundColor.IsAutomatic;

			result.TextColor = fmt.Font.Color.ToColor(ParentExcelInterface.Xls, Color.Black);
			result.FillPattern = (FillPatternStyle)fmt.FillPattern.Pattern;
			result.Borders.Bottom.BorderStyle = (CellBorderStyle)fmt.Borders.Bottom.Style;
			result.Borders.Top.BorderStyle = (CellBorderStyle)fmt.Borders.Top.Style;
			result.Borders.Left.BorderStyle = (CellBorderStyle)fmt.Borders.Left.Style;
			result.Borders.Right.BorderStyle = (CellBorderStyle)fmt.Borders.Right.Style;

			result.Borders.Bottom.BorderColor = fmt.Borders.Bottom.Color.ToColor(ParentExcelInterface.Xls);
			result.Borders.Top.BorderColor = fmt.Borders.Top.Color.ToColor(ParentExcelInterface.Xls);
			result.Borders.Left.BorderColor = fmt.Borders.Left.Color.ToColor(ParentExcelInterface.Xls);
			result.Borders.Right.BorderColor = fmt.Borders.Right.Color.ToColor(ParentExcelInterface.Xls);

			result.HTextAlign = defaultHorizontalAlignment;
			if (fmt.HAlignment == THFlxAlignment.center)
			{
				result.HTextAlign = HorizontalTextAlignment.Centre;
			}
			else if (fmt.HAlignment == THFlxAlignment.right)
			{
				result.HTextAlign = HorizontalTextAlignment.Right;
			}
			else if (fmt.HAlignment == THFlxAlignment.left)
			{
				result.HTextAlign = HorizontalTextAlignment.Left;
			}
			else if (fmt.HAlignment == THFlxAlignment.justify)
			{
				result.HTextAlign = HorizontalTextAlignment.Justify;
			}

			result.VTextAlign = VerticalTextAlignment.Bottom;
			if (fmt.VAlignment == TVFlxAlignment.center)
			{
				result.VTextAlign = VerticalTextAlignment.Middle;
			}
			else if (fmt.VAlignment == TVFlxAlignment.top)
			{
				result.VTextAlign = VerticalTextAlignment.Top;
			}

			result.FormatPattern = fmt.Format;
			result.WrapText = fmt.WrapText;

			return result;
		}

		public bool IsCellEmpty(int row, int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

			if (!string.IsNullOrEmpty(this[row, col].ToString()))
			{
				return false;
			}

			if (!string.IsNullOrEmpty(GetCellFormula(row, col)))
			{
				return false;
			}

			if (IsCellMerged(row, col))
			{
				return false;
			}

			return true;
		}

		internal bool IsCellMerged(int row, int column)
		{
			var range = GetMergedCellRange(row, column);
			return range.Top < row || range.Bottom > row || range.Right > column || range.Left < column;
		}

		internal ExcelCellRange GetMergedCellRange(int row, int column)
		{
			var range = ParentExcelInterface.Xls.CellMergedBounds(row + 1, column + 1);
			return new ExcelCellRange(range.Top - 1, range.Left - 1, range.Bottom - 1, range.Right - 1);
		}

		public void SetCellFormat(int row, int col, CellFormat format)
		{
			var fmt = ParentExcelInterface.Xls.GetDefaultFormat;

			fmt.Font = new TFlxFont();
			fmt.Font.Name = format.FontName;
			fmt.Font.Size20 = (int)(format.FontSize * 20);
			if (format.FontBold)
			{
				fmt.Font.Style |= TFlxFontStyles.Bold;
			}

			if (format.FontItalic)
			{
				fmt.Font.Style |= TFlxFontStyles.Italic;
			}

			if (format.FontStrikeout)
			{
				fmt.Font.Style |= TFlxFontStyles.StrikeOut;
			}

			fmt.FillPattern.Pattern = (TFlxPatternStyle)format.FillPattern;
			fmt.FillPattern.FgColor = format.BackgroundColor;
			fmt.Borders = new TFlxBorders();
			fmt.Borders.Top.Style = (TFlxBorderStyle)format.Borders.Top.BorderStyle;
			fmt.Borders.Bottom.Style = (TFlxBorderStyle)format.Borders.Bottom.BorderStyle;
			fmt.Borders.Left.Style = (TFlxBorderStyle)format.Borders.Left.BorderStyle;
			fmt.Borders.Right.Style = (TFlxBorderStyle)format.Borders.Right.BorderStyle;

			fmt.Borders.Top.Color = TExcelColor.FromArgb(format.Borders.Top.BorderColor.ToArgb());
			fmt.Borders.Bottom.Color = TExcelColor.FromArgb(format.Borders.Bottom.BorderColor.ToArgb());
			fmt.Borders.Left.Color = TExcelColor.FromArgb(format.Borders.Left.BorderColor.ToArgb());
			fmt.Borders.Right.Color = TExcelColor.FromArgb(format.Borders.Right.BorderColor.ToArgb());

			if (format.HTextAlign == HorizontalTextAlignment.Centre)
			{
				fmt.HAlignment = THFlxAlignment.center;
			}
			else if (format.HTextAlign == HorizontalTextAlignment.Right)
			{
				fmt.HAlignment = THFlxAlignment.right;
			}
			else if (format.HTextAlign == HorizontalTextAlignment.Left)
			{
				fmt.HAlignment = THFlxAlignment.left;
			}
			else if (format.HTextAlign == HorizontalTextAlignment.Justify)
			{
				fmt.HAlignment = THFlxAlignment.justify;
			}
			else
			{
				fmt.HAlignment = THFlxAlignment.general;
			}

			if (format.VTextAlign == VerticalTextAlignment.Middle)
			{
				fmt.VAlignment = TVFlxAlignment.center;
			}
			else if (format.VTextAlign == VerticalTextAlignment.Top)
			{
				fmt.VAlignment = TVFlxAlignment.top;
			}
			else
			{
				fmt.VAlignment = TVFlxAlignment.bottom;
			}

			if (format.TextColor != Color.Empty)
			{
				fmt.Font.Color = format.TextColor;
			}

			fmt.Format = format.FormatPattern;
			fmt.WrapText = format.WrapText;

			int formatNumber = ParentExcelInterface.Xls.AddFormat(fmt);
			ParentExcelInterface.Xls.SetCellFormat(row + 1, col + 1, formatNumber);
		}

		public void ClearTopLine(int row)
		{
			TFlxFormat newRowFormat = ParentExcelInterface.Xls.GetDefaultFormat;
			newRowFormat.Borders.Top.Style = TFlxBorderStyle.None;
			var applyFormat = new TFlxApplyFormat();
			applyFormat.Borders.Top = true;
			ParentExcelInterface.Xls.SetRowFormat(row + 1, newRowFormat, applyFormat, true);
		}

		public void ClearBottomLine(int row)
		{
			TFlxFormat newRowFormat = ParentExcelInterface.Xls.GetDefaultFormat;
			newRowFormat.Borders.Bottom.Style = TFlxBorderStyle.None;
			var applyFormat = new TFlxApplyFormat();
			applyFormat.Borders.Bottom = true;
			ParentExcelInterface.Xls.SetRowFormat(row + 1, newRowFormat, applyFormat, true);
		}

		public void MergeCells(int firstRow, int firstCol, int lastRow, int lastCol)
		{
			ParentExcelInterface.Xls.MergeCells(firstRow + 1, firstCol + 1, lastRow + 1, lastCol + 1);
		}

		public void UnMergeCells(int firstRow, int firstCol, int lastRow, int lastCol)
		{
			ParentExcelInterface.Xls.UnMergeCells(firstRow + 1, firstCol + 1, lastRow + 1, lastCol + 1);
		}

		public bool DoesRowContainAMergedCellToNextRow(int row)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			bool result = false;

			for (int col = 0; col < ColumnCount; col++)
			{
				TXlsCellRange range = ParentExcelInterface.Xls.CellMergedBounds(row + 1, col + 1);
				if (range.Bottom > row + 1)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public int GetColWidth(int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			int result = ParentExcelInterface.Xls.GetColHidden(col + 1) ? 0 : ParentExcelInterface.Xls.GetColWidth(col + 1);
			return result;
		}

		public string GetCellFormula(int row, int col)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			var formula = ParentExcelInterface.Xls.GetCellValue(row + 1, col + 1) as TFormula;

			return formula != null ? formula.Text : "";
		}

		public void SetCellFormula(int row, int col, string formula, double formulaResult)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TFormula formulaToInsert = new TFormula(formula, formulaResult);
			this[row, col] = formulaToInsert;
		}

		/// <summary>
		/// This method is called when either Width or Height are greater than 0, and it calculates
		/// the needed Height/Width for the negative parameters.
		/// It will:
		///   1) If Height and Width are less than 0, return the original image size.
		///   2) If only Height is less than 0, return the image size with the specified Width, and the Height needed to keep the correct aspect ratio.
		///   3) If only Width is less than 0, return the image size with the specified Height, and the Width needed to keep the correct aspect ratio.
		/// </summary>
		TImageProperties GetAutoImageProperties(int row, int col, int rowCount, int excelUnitWidth, ExcelImage excelImage)
		{
			int imagePixelWidth = excelImage.Image.Width;
			int imagePixelHeight = excelImage.Image.Height;
			int targetImageWidth = 0;
			int targetImageHeight = 0;

			if (excelImage.IsDimensionInPixel)
			{
				targetImageWidth = imagePixelWidth;
				targetImageHeight = imagePixelHeight;
			}
			else
			{
				int excelUnitHeight = GetRowRangeHeight(row, row + rowCount - 1);
				int actualPixelWidth = (int)Utilities.Round((decimal)(excelUnitWidth / ExcelMetrics.ColMult(ParentExcelInterface.Xls)), 0);
				int actualPixelHeight = (int)Utilities.Round((decimal)(excelUnitHeight / (float)FlxConsts.RowMult), 0);

				// Both negative, return image size
				if (actualPixelWidth <= 0 && actualPixelHeight <= 0)
				{
					targetImageWidth = imagePixelWidth;
					targetImageHeight = imagePixelHeight;
				}
				// Both positive and no image aspect ratio lock, return target area size
				else if (!excelImage.IsAspectRatioLocked && actualPixelWidth > 0 && actualPixelHeight > 0)
				{
					targetImageWidth = actualPixelWidth;
					targetImageHeight = actualPixelHeight;
				}
				//Preserve image aspect ratio
				else
				{
					var useRatio = excelImage.IsAspectRatioLocked || ((actualPixelWidth <= 0) ^ (actualPixelHeight <= 0));

					double ratioWidth = actualPixelWidth / (double)imagePixelWidth;
					double ratioHeight = actualPixelHeight / (double)imagePixelHeight;

					double targetImageRatioWidth = useRatio ? Math.Min(ratioWidth, ratioHeight) : ratioWidth;
					double targetImageRatioHeight = useRatio ? Math.Min(ratioWidth, ratioHeight) : ratioHeight;

					targetImageWidth = (int)Utilities.Round((decimal)(imagePixelWidth * targetImageRatioWidth), 0);
					targetImageHeight = (int)Utilities.Round((decimal)(imagePixelHeight * targetImageRatioHeight), 0);
				}
			}

			var anchor = new TClientAnchor(TFlxAnchorType.MoveAndDontResize, row + 1, 0, col + 1, 0, targetImageHeight, targetImageWidth, ParentExcelInterface.Xls);
			return new TImageProperties(anchor, excelImage.ImageName, excelImage.ImageName);
		}

#if DEBUG
		[ThreadStatic]
		public static Exception ExceptionToThrowWhenInsertingPictureForTesting;
#endif

		public void InsertPicture(int row, int col, int height, int widthInXls, ExcelImage excelImage)
		{
			insertedImages.Add(excelImage.Image);
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TImageProperties properties;
			properties = GetAutoImageProperties(row, col, height, widthInXls, excelImage);

			try
			{
#if DEBUG
				if (ExceptionToThrowWhenInsertingPictureForTesting != null)
				{
					throw ExceptionToThrowWhenInsertingPictureForTesting;
				}
#endif
				ParentExcelInterface.Xls.AddImage(excelImage.Image, properties);
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				string message = Res.GetString("16b9dbe6-b608-4d19-93e8-4f006329a964", "{0} Inserting Image: {1}", exception.Message, excelImage.ToString());
				throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorInsertingImage, message);
			}
		}

		public void InsertHyperlink(int row, int col, ExcelHyperlink hyperlink)
		{
			THyperLink tHyperlink = new THyperLink(hyperlink.Type, hyperlink.LinkLocation, hyperlink.TextToShow, hyperlink.TargetFrame, hyperlink.TextMark);
			tHyperlink.Hint = hyperlink.Tooltip;
			TXlsCellRange cellRange = new TXlsCellRange(row + 1, col + 1, row + 1, col + 1);
			ParentExcelInterface.Xls.AddHyperLink(cellRange, tHyperlink);

			TFlxFormat format = ParentExcelInterface.Xls.GetCellVisibleFormatDef(row + 1, col + 1);
			format.Font.Underline = TFlxUnderline.Single;
			format.Font.Color = Color.Blue;
			ParentExcelInterface.Xls.SetCellFormat(row + 1, col + 1, ParentExcelInterface.Xls.AddFormat(format));
		}

		public void RemoveRow(int rowIndex)
		{
			rowIndex++;
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			var range = new TXlsCellRange(rowIndex, 1, rowIndex, Max_Columns);
			ParentExcelInterface.Xls.DeleteRange(range, TFlxInsertMode.ShiftRowDown);
		}

		public void RemoveRows(int startRow, int endRow)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			var range = new TXlsCellRange(startRow + 1, 1, endRow, Max_Columns);
			ParentExcelInterface.Xls.DeleteRange(range, TFlxInsertMode.ShiftRowDown);
		}

		public void ClearRange(int startRow, int startCol, int endRow, int endCol)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TXlsCellRange range = new TXlsCellRange(startRow + 1, startCol + 1, endRow, endCol + 1);
			ParentExcelInterface.Xls.DeleteRange(range, TFlxInsertMode.ShiftRowDown);
		}

		public void HideColumn(int column)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.SetColHidden(column + 1, true);
			ParentExcelInterface.Xls.SetColWidth(column + 1, 0);
		}

		public void DeleteColumn(int column)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TXlsCellRange cellRange = new TXlsCellRange(1, column + 1, 1, column + 1);
			ParentExcelInterface.Xls.DeleteRange(cellRange, TFlxInsertMode.ShiftColRight);
		}

		public bool IsColumnHidden(int column)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.GetColHidden(column + 1);
		}

		public int GetColumnRangeWidth(int columnIndex1, int columnIndex2)
		{
			var result = 0;

			for (var columnIndex = columnIndex1; columnIndex < columnIndex2; columnIndex++)
			{
				result += GetColWidth(columnIndex);
			}

			return result;
		}

		public int GetRowRangeHeight(int startRow, int endRow)
		{
			int totalHeight = 0;
			for (int i = startRow; i <= endRow; i++)
			{
				totalHeight += GetRowHeight(i);
			}
			return totalHeight;
		}

		public int GetRowHeight(int rowNumber)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.GetRowHeight(rowNumber + 1, true);
		}

		public void SetRowHeight(int rowNumber, int height)
		{
			if (height < 0 || height > MaxRowHeight)
			{
				throw new InvalidOperationException(string.Format("Row height being set must be between 0 and {0}. Invalid Value: {1}", MaxRowHeight.ToString(), height.ToString()));
			}

			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.SetRowHeight(rowNumber + 1, height);
		}

		internal int SetRowHeightInsertingExtraRowsIfRequiredToGetTheRightTotalHeightReturningNumberOfAdditionalRows(int rowNumber, int height)
		{
			if (height < 0 || height > 99999)
			{
				throw new InvalidOperationException("Row height being set must be between 0 and 99999. Invalid Value: " + height.ToString());
			}

			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return SetRowHeightInsertingExtraRowsIfRequiredToGetTheRightTotalHeightCore(rowNumber, height);
		}

		int SetRowHeightInsertingExtraRowsIfRequiredToGetTheRightTotalHeightCore(int rowNumber, int height)
		{
			int result = 0;
			if (height <= MaxRowHeight)
			{
				ParentExcelInterface.Xls.SetRowHeight(rowNumber + 1, height);
			}
			else
			{
				CopyAndInsertRows(this, rowNumber, 1, rowNumber + 1);
				fRowCount++;
				result++;

				for (int i = 0; i < this.ColumnCount; i++)
				{
					int cellFormatOriginalRow = ParentExcelInterface.Xls.GetCellFormat(rowNumber + 1, i + 1);
					TFlxFormat formatOriginalRow = cellFormatOriginalRow <= 0 ? ParentExcelInterface.Xls.GetDefaultFormat : ParentExcelInterface.Xls.GetFormat(cellFormatOriginalRow);
					formatOriginalRow.Borders.Bottom.Style = TFlxBorderStyle.None;
					ParentExcelInterface.Xls.SetCellFormat(rowNumber + 1, i + 1, ParentExcelInterface.Xls.AddFormat(formatOriginalRow));

					int cellFormatCopiedRow = ParentExcelInterface.Xls.GetCellFormat(rowNumber + 2, i + 1);
					TFlxFormat formatCopiedRow = cellFormatCopiedRow <= 0 ? ParentExcelInterface.Xls.GetDefaultFormat : ParentExcelInterface.Xls.GetFormat(cellFormatCopiedRow);
					formatCopiedRow.Borders.Top.Style = TFlxBorderStyle.None;
					ParentExcelInterface.Xls.SetCellFormat(rowNumber + 2, i + 1, ParentExcelInterface.Xls.AddFormat(formatCopiedRow));
				}

				ParentExcelInterface.Xls.SetRowHeight(rowNumber + 1, MaxRowHeight);
				result += SetRowHeightInsertingExtraRowsIfRequiredToGetTheRightTotalHeightCore(rowNumber + 1, height - MaxRowHeight);
			}
			return result;
		}

		public static int MaxRowHeight { get { return FlxConsts.MaxRowHeight; } }

		public void SetComment(int row, int col, string comment)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			ParentExcelInterface.Xls.SetComment(row + 1, col, comment);
		}

		public string GetComment(int row, int col)
		{
			return ParentExcelInterface.Xls.GetComment(row + 1, col + 1).ToString();
		}

		#region Object manipulation
		public bool ObjectExists(string objectName)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

			return GetObjectIndex(objectName) > -1;
		}

		public void RemoveAllObjectsByName(string objectName)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

			int objectIndex = GetObjectIndex(objectName);
			if (objectIndex == -1)
			{
				throw new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorRemovingObject, Res.GetString("b3447208-c5f3-43e7-a6cf-078a81a2e1aa", "Object [{0}] does not exist.", objectName));
			}

			while (objectIndex > -1)
			{
				ParentExcelInterface.Xls.DeleteObject(objectIndex);
				objectIndex = GetObjectIndex(objectName);
			}
		}

		public string[] GetObjectNames()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			string[] result = new string[ParentExcelInterface.Xls.ObjectCount];
			for (int i = 0; i < ParentExcelInterface.Xls.ObjectCount; i++)
			{
				result[i] = ParentExcelInterface.Xls.GetObjectName(i + 1);
			}

			return result;
		}

		int GetObjectIndex(string objectName)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			int objectIndex = -1;
			for (int i = 1; i <= ParentExcelInterface.Xls.ObjectCount; i++)
			{
				if (ParentExcelInterface.Xls.GetObjectName(i) != null && ParentExcelInterface.Xls.GetObjectName(i).ToLower() == objectName.ToLower())
				{
					objectIndex = i;
					break;
				}
			}

			return objectIndex;
		}

		public int GetObjectCount()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.ObjectCount;
		}

		public TClientAnchor GetObjectAnchor(string objectName)
		{
			var objectIndex = GetObjectIndex(objectName);
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

			return ParentExcelInterface.Xls.GetObjectAnchor(objectIndex);
		}

		#endregion

		#region ImagesInTheXls

		public int GetImageCount()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.ImageCount;
		}

		public TImageProperties GetImageProperties(int imageIndex)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			return ParentExcelInterface.Xls.GetImageProperties(imageIndex + 1);
		}

		public Image GetImage(int imageIndex)
		{
			Stream imageStream;
			if (ImageStreams.ContainsKey(imageIndex))
			{
				imageStream = ImageStreams[imageIndex];
			}
			else
			{
				ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
				TXlsImgType imageType = new TXlsImgType();
				imageStream = new MemoryStream();
				ParentExcelInterface.Xls.GetImage(imageIndex + 1, ref imageType, imageStream);
				ImageStreams[imageIndex] = imageStream;
			}

			if (imageStream.Length == 0)
			{
				return null;
			}

			imageStream.Position = 0;
			try
			{
				return Image.FromStream(imageStream);
			}
			//GDI+ throws 'OutOfMemoryException' for a generic category of errors. So, OOM here is usually due to GDI+ error not true OOM
			//Example: http://msdn.microsoft.com/en-us/library/4sahykhd.aspx OutOfMemoryException The file does not have a valid image format. -or- GDI+ does not support the pixel format of the file.
			//But it can also produce ArgumentException, see http://msdn.microsoft.com/en-us/library/93z9ee4x.aspx - so we should cover both bases just to be sure
			catch (Exception ex)
			{
				if (!(ex is OutOfMemoryException || ex is ArgumentException))
				{
					throw;
				}

				string path = Path.Combine(CargoWise.IO.Temp.TempPath, "InvalidImages");
				Directory.CreateDirectory(path);
				string imageSavedPath = CargoWise.IO.Temp.GetTempFileName(path);
				imageStream.CopyToFile(imageSavedPath);

				Globals.Message.ShowError(Res.GetString("bfc7143e-3b49-4c32-8449-4e1626facdac", @"An exception was thrown trying to display an image in this document. It may be in an unsupported format or corrupt. It has been saved to {0} if you wish to perform further analysis."
					, imageSavedPath), Res.GetString("0373bdeb-f550-4305-b0e2-e2cdbae0fd13", "Bad image format"));

				//8x8 bmp, white with a red cross on it
				byte[] redXBMP =
								{ 0x42, 0x4D, 0xF6, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x36, 0x00, 0x00, 0x00, 0x28, 0x00,
								 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x01, 0x00, 0x18, 0x00, 0x00, 0x00,
								 0x00, 0x00, 0xC0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
								 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0xFF,
								 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C,
								 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED,
								 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24,
								 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
								 0xFF, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED,
								 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED, 0x24,
								 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
								 0xFF, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED,
								 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0xFF,
								 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED, 0x24, 0x1C,
								 0xED, 0x24, 0x1C, 0xED, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
								 0x24, 0x1C, 0xED, 0x24, 0x1C, 0xED };
				Stream redXBMPStream = new MemoryStream(redXBMP);
				ImageStreams[imageIndex] = redXBMPStream;

				return Image.FromStream(redXBMPStream);
			}
		}

		#endregion

		public void Dispose()
		{
			insertedImages.Where(image => image != null && !image.IsDisposed()).ForEach(image => image.Dispose());
			insertedImages.Clear();

			foreach (var imageStream in ImageStreams.Values)
			{
				imageStream.Dispose();
			}
			ImageStreams.Clear();
		}

		public override string ToString()
		{
			return ToString(new CellFormatter());
		}

		public string ToString(bool includeHPageBreak)
		{
			var cellFormatter = new CellFormatter();
			return ToString(0, RowCount - 1, cellFormatter, true, false, includeHPageBreak);
		}

		public string ToString(int startRow, int lastRow)
		{
			var cellFormatter = new CellFormatter();
			return ToString(startRow, lastRow, cellFormatter, false);
		}

		public string ToString(ICellFormatter cellFormatter)
		{
			return ToString(0, RowCount - 1, cellFormatter);
		}

		public string ToString(int startRow, int lastRow, ICellFormatter cellFormatter)
		{
			return ToString(startRow, lastRow, cellFormatter, true);
		}

		public string ToString(int startRow, int lastRow, ICellFormatter cellFormatter, bool trim)
		{
			return ToString(startRow, lastRow, cellFormatter, trim, false, false);
		}

		public string ToString(ICellFormatter cellFormatter, bool includeRowsWithZeroHeight)
		{
			return ToString(0, RowCount - 1, cellFormatter, true, includeRowsWithZeroHeight, false);
		}

		public string ToString(int startRow, int lastRow, ICellFormatter cellFormatter, bool trim, bool includeRowsWithZeroHeight, bool includeHPageBreak)
		{
			const string hPageBreakDelimiter = "---------HPageBreak---------";

			var result = new ZStringBuilder();
			for (int rowIndex = startRow; rowIndex <= lastRow; rowIndex++)
			{
				if (includeRowsWithZeroHeight || GetRowHeight(rowIndex) > 0)
				{
					var rowResult = new ZStringBuilder();
					if (includeHPageBreak && HasHPageBreak(rowIndex))
					{
						result.Append(hPageBreakDelimiter);
					}

					for (int columnIndex = 0; columnIndex < ColumnCount; columnIndex++)
					{
						if (!IsColumnHidden(columnIndex))
						{
							ZString cellString = cellFormatter.Format(this, rowIndex, columnIndex);
							if (!cellString.IsEmpty)
							{
								rowResult.Append("{" + GetColumnTitle(columnIndex) + "}-[" + cellString + "]");
							}
						}
					}
					result.Append(rowResult.ToStringWithDelimiterBetweenAppends("   "));
				}
			}

			if (trim)
			{
				return result.ToStringWithNewLineBetweenAppends().Trim();
			}
			else
			{
				return result.ToStringWithNewLineBetweenAppends();
			}
		}

		internal static string GetColumnTitle(int columnIndex)
		{
			int firstCharIndex = System.Convert.ToInt32(columnIndex / 26);
			int secondCharIndex = columnIndex % 26;
			if (firstCharIndex <= 26)
			{
				string secondChar = ColumnTitleCharList.Substring(secondCharIndex, 1);
				if (firstCharIndex == 0)
				{
					return secondChar;
				}
				return ColumnTitleCharList.Substring(firstCharIndex - 1, 1) + secondChar;
			}
			return columnIndex.ToString();
		}
		const string ColumnTitleCharList = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static string GetCellName(int rowIndex, int colIndex)
		{
			return GetColumnTitle(colIndex) + (rowIndex + 1).ToString();
		}

		public void MoveRows(int firstRow, int lastRow, int destinationRow)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			if (destinationRow + (lastRow - firstRow + 1) > Max_Rows)
			{
				throw new DocumentEngineTooManyRowsException();
			}

			if (firstRow >= 0 && lastRow >= 0 && destinationRow >= 0 && firstRow <= lastRow && (destinationRow > lastRow || destinationRow < firstRow))
			{
				var cellRange = new TXlsCellRange(firstRow + 1, 1, lastRow + 1, Max_Columns);
				try
				{
					ParentExcelInterface.Xls.MoveRange(cellRange, destinationRow + 1, 1, TFlxInsertMode.ShiftRowDown);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					throw new DocumentEngineException($"Could not move rows with the following parameters because FlexCel threw an exception." +
						$"WorkSheetNumber: {WorkSheetNumber}. FirstRow: {firstRow}. LastRow: {lastRow}. DestinationRow: {destinationRow}. " +
						$"Inner Exception Message: {ex.Message}. SheetName: {SheetName}", ex);
				}
			}
			else
			{
				throw new DocumentEngineException(string.Format("Could not move rows with parameters: WorkSheetNumber = {0}, FirstRow = {1}, LastRow = {2}, DestinationRow = {3}", WorkSheetNumber, firstRow, lastRow, destinationRow));
			}
		}

		public void InsertRows(int rowToInsertBefore, int rowCount)
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			TXlsCellRange emptyRange = new TXlsCellRange(1, 1, 1, 1);
			// emptyRange not really used, but the flexcel interface wants it even if not copying.
			ParentExcelInterface.Xls.InsertAndCopyRange(emptyRange, rowToInsertBefore + 1, 1, rowCount, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.None);
		}

		public string SheetNameOverride;

		public string UpdateSheetName()
		{
			ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;
			var translatedSheetName = Report.GetLocalizedSheetName(!string.IsNullOrEmpty(SheetNameOverride) ? SheetNameOverride : ParentExcelInterface.Xls.SheetName);
			ParentExcelInterface.Xls.SheetName = translatedSheetName;
			return translatedSheetName;
		}

		void IExcelWorkSheet.CopyAndInsertRows(IExcelWorkSheet source, int sourceRowIndex, int sourceRowCount, int destinationRowIndex)
		{
			CopyAndInsertRows((ExcelWorkSheet)source, sourceRowIndex, sourceRowCount, destinationRowIndex);
		}

		public void CopyAndInsertRows(ExcelWorkSheet source, int sourceRowIndex, int sourceRowCount, int destinationRowIndex)
		{
			if (sourceRowCount > 0)
			{
				sourceRowIndex++;
				destinationRowIndex++;

				var sourceRange = new TXlsCellRange(sourceRowIndex, 1, sourceRowIndex + sourceRowCount - 1, source.ColumnCount);

				ParentExcelInterface.Xls.ActiveSheet = WorkSheetNumber;

				try
				{
					ParentExcelInterface.Xls.InsertAndCopyRange(sourceRange, destinationRowIndex, 1, 1, TFlxInsertMode.ShiftRowDown, TRangeCopyMode.All, source.ParentExcelInterface.Xls, source.WorkSheetNumber);
				}
				catch (FlexCel.XlsAdapter.FlexCelXlsAdapterException ex) when (ex.ErrorCode == FlexCel.XlsAdapter.XlsErr.ErrCantCopyPictFmla)
				{
					throw new ExcelInterfaceException(ExcelInterfaceExceptionType.CopyOLEObjectInTemplateException, ex.Message);
				}
			}
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	using Enterprise.DocumentEngine.FlexCelInterface;

	class BoldResponsiveCellFormatter : CellFormatter
	{
		public override string Format(ExcelWorkSheet cellSource, int row, int column)
		{
			TFlxFont cellFont = cellSource.GetCellFlxFont(row, column);
			if (cellFont != null)
			{
				BoldChunkBuilder resultBuilder = new BoldChunkBuilder(cellFont);
				object cellContent = cellSource[row, column];
				TRichString richContent = cellContent as TRichString;
				if (richContent != null)
				{
					string cellText = richContent.Value;
					int startingIndex = 0;
					for (int segmentIndex = 0; segmentIndex < richContent.RTFRunCount; segmentIndex++)
					{
						TRTFRun segment = richContent.RTFRun(segmentIndex);
						int length = segment.FirstChar - startingIndex;
						resultBuilder.AddSegment(cellText.Substring(startingIndex, length));

						startingIndex += length;
						resultBuilder.Font = richContent.GetFont(segment.FontIndex);
					}
					resultBuilder.AddSegment(cellText.Substring(startingIndex));
				}
				else
				{
					resultBuilder.AddSegment(base.Format(cellSource, row, column));
				}

				return resultBuilder.GetResult();
			}

			return base.Format(cellSource, row, column);
		}

		class BoldChunkBuilder
		{
			public BoldChunkBuilder(TFlxFont startingFont)
			{
				Font = startingFont;
				ResultBuilder = new ZStringBuilder();
			}

			public TFlxFont Font;
			readonly ZStringBuilder ResultBuilder;

			bool CurrentlyBold;

			public void AddSegment(string text)
			{
				bool isBold = (Font != null && Font.Style == TFlxFontStyles.Bold);
				if (isBold != CurrentlyBold)
				{
					ResultBuilder.Append(isBold ? BoldStartIndicator : BoldEndIndicator);
					CurrentlyBold = isBold;
				}
				ResultBuilder.AppendIfNotEmpty(text);
			}

			public string GetResult()
			{
				Font = null;
				AddSegment("");
				return ResultBuilder.ToString();
			}

			const string BoldStartIndicator = "<B>";
			const string BoldEndIndicator = "<\\B>";
		}
	}
}

#endif
#endregion
