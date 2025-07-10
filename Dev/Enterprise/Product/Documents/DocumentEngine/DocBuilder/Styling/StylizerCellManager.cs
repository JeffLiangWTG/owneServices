using System;
using System.Drawing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineIntegration;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.DocBuilder.Styling
{
	class StylizerCellManager
	{
		internal StylizerCellManager(ExcelWorkSheet workSheet, int row, int column)
		{
			this.WorkSheet = workSheet;
			this.excelFile = workSheet.ParentExcelInterface.Xls;
			this.sheet = workSheet.WorkSheetNumber;
			this.Row = row;
			this.Column = column;
			this.flxFormat = excelFile.GetFormat(excelFile.GetCellFormat(sheet, row + 1, column + 1));
		}

		internal readonly ExcelWorkSheet WorkSheet;
		readonly ExcelFile excelFile;
		readonly int sheet;
		internal readonly int Row;
		internal readonly int Column;
		readonly TFlxFormat flxFormat;
		internal bool ShouldApply;

		internal Color BackgroundColor
		{
			get { return flxFormat.FillPattern.FgColor.ToColor(excelFile, Color.White); }
			set
			{
				flxFormat.FillPattern.Pattern = TFlxPatternStyle.Solid;
				flxFormat.FillPattern.FgColor = value;
			}
		}

		internal FillPatternStyle BackgroundPattern
		{
			get { return (FillPatternStyle)flxFormat.FillPattern.Pattern; }
			set { flxFormat.FillPattern.Pattern = (TFlxPatternStyle)value; }
		}

		internal bool IsBackgroundColorAutomatic
		{
			get { return flxFormat.FillPattern.FgColor.IsAutomatic; }
		}

		internal TFlxBorders Borders
		{
			get { return flxFormat.Borders; }
		}

		internal string FontName
		{
			get { return flxFormat.Font.Name; }
			set { flxFormat.Font.Name = value; }
		}

		internal float FontSize
		{
			get { return flxFormat.Font.Size20 / 20; }
			set { flxFormat.Font.Size20 = Convert.ToInt32(value * 20); }
		}

		internal FontStyle FontStyle
		{
			get { return flxFormat.Font.Style.ToFontStyle(); }
			set
			{
				flxFormat.Font.Style = TFlxFontStyles.None;

				if ((value & FontStyle.Bold) != 0)
				{
					flxFormat.Font.Style |= TFlxFontStyles.Bold;
				}

				if ((value & FontStyle.Italic) != 0)
				{
					flxFormat.Font.Style |= TFlxFontStyles.Italic;
				}
			}
		}

		internal Color FontColor
		{
			get { return flxFormat.Font.Color.ToColor(excelFile, Color.Black); }
			set
			{
				if (value.IsEmpty)
				{
					flxFormat.Font.Color = TExcelColor.Automatic;
				}
				else
				{
					flxFormat.Font.Color = value;
				}
			}
		}

		internal void Apply()
		{
			var cellFormat = excelFile.AddFormat(flxFormat);
			excelFile.ActiveSheet = sheet;
			excelFile.SetCellFormat(Row + 1, Column + 1, cellFormat);
		}
	}
}
