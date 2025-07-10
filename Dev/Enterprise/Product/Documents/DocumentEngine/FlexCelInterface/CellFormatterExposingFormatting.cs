#if DEBUG
using System.Drawing;
using CargoWise.Types;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.FlexCelInterface
{
	internal class CellFormatterExposingFormatting : CellFormatter
	{
		public CellFormatterExposingFormatting(ExcelFile xls)
		{
			this.xls = xls;
		}
		readonly ExcelFile xls;

		public override string Format(ExcelWorkSheet workSheet, int rowIndex, int columnIndex)
		{
			ZStringBuilder result = new ZStringBuilder();

			TFlxFormat format = xls.GetFormat(xls.GetCellFormat(rowIndex + 1, columnIndex + 1));

			result.AppendIfNotEmpty(GetFillPatternText(format.FillPattern));
			result.AppendIfNotEmpty(GetBordersText(format.Borders));

			result.AppendIfNotEmpty(base.Format(workSheet, rowIndex, columnIndex));

			result.AppendIfNotEmpty(format.Format);

			return result.ToStringWithDelimiterBetweenAppends(" - ");
		}

		string GetBordersText(TFlxBorders borders)
		{
			ZStringBuilder result = new ZStringBuilder();

			AddOneBorder(result, "Top", borders.Top);
			AddOneBorder(result, "Bottom", borders.Bottom);
			AddOneBorder(result, "Left", borders.Left);
			AddOneBorder(result, "Right", borders.Right);

			return result.ToStringWithDelimiterBetweenAppends(", ");
		}

		void AddOneBorder(ZStringBuilder result, string borderName, TFlxOneBorder border)
		{
			if (border.Style != TFlxBorderStyle.None)
			{
				result.Append(borderName + ": Style(" + border.Style.ToString() + ") " + GetColorString(border.Color));
			}
		}

		string GetFillPatternText(TFlxFillPattern fillPattern)
		{
			if (fillPattern.Pattern != TFlxPatternStyle.None)
			{
				return "Background: Fill(" + fillPattern.Pattern.ToString() + ") " + GetColorString(fillPattern.FgColor);
			}
			return "";
		}

		static void GetColorAsString(ZStringBuilder result, Color color)
		{
			if (color.IsNamedColor)
			{
				result.Append("Name:" + color.Name);
			}
			result.Append("R:" + color.R.ToString());
			result.Append("G:" + color.G.ToString());
			result.Append("B:" + color.B.ToString());
			result.Append("A:" + color.A.ToString());
		}

		string GetColorString(TExcelColor color)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (color.ColorType == TColorType.RGB)
			{
				Color rgbColor;
				unchecked
				{
					rgbColor = Color.FromArgb((int)((uint)(0xFF000000 | color.RGB)));
				}
				GetColorAsString(result, rgbColor);
				return "Color(" + result.ToStringWithDelimiterBetweenAppends(", ") + ")";
			}
			else if (color.ColorType == TColorType.Indexed)
			{
				return GetColorString(color.Index);
			}
			else if (color.ColorType == TColorType.Automatic)
			{
				return "Color(Automatic)";
			}
			else
			{
				return "Color(Unknown)";
			}
		}

		string GetColorString(int colourIndex)
		{
			ZStringBuilder result = new ZStringBuilder();
			if (colourIndex == 57) // Special "Auto" Colour.
			{
				result.Append("_Auto_");
			}
			else if (colourIndex == 0) // Special "None" Colour.
			{
				result.Append("_None_");
			}
			else
			{
				Color color = xls.GetColorPalette(colourIndex);
				GetColorAsString(result, color);
			}
			return "Color(" + result.ToStringWithDelimiterBetweenAppends(", ") + ")";
		}
	}
}

#endif