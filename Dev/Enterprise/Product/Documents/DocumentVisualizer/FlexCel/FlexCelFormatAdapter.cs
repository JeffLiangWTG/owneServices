using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	public class FlexCelFormatAdapter : IFormat
	{
		public FlexCelFormatAdapter(FlexCelWorksheet worksheet, IRange range)
		{
			this.worksheet = Argument.NotNull(worksheet, "worksheet");
			this.palette = worksheet;
			this.range = Argument.NotNull(range, "range");
		}

		readonly FlexCelWorksheet worksheet;
		readonly IFlexCelPalette palette;
		readonly IRange range;

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Point on an Excel spreadsheet, not winforms code")]
		TFlxFormat GetFormat(int row, int column)
		{
			var location = new Point(row, column);

			if (!flxFormats.ContainsKey(location))
			{
				var flxFormat = worksheet.GetCellVisibleFormatDef(row, column);

				flxFormats.Add(location, flxFormat);
			}

			return flxFormats[location];
		}

		readonly Dictionary<Point, TFlxFormat> flxFormats = new Dictionary<Point, TFlxFormat>();

		public IFont Font
		{
			get
			{
				if (font == null)
				{
					var flxFormat = GetFormat(range.TopRow, range.LeftColumn);

					if (flxFormat != null)
					{
						font = new FlexCelFontAdapter(palette, flxFormat.Font, flxFormat.Rotation);
					}
				}

				return font;
			}
		}

		IFont font;

		public IBorders Borders
		{
			get
			{
				if (borders == null)
				{
					// needed for merged cells
					var flxTopLeftFormat = GetFormat(range.TopRow, range.LeftColumn);
					var flxBottomRightFormat = GetFormat(range.BottomRow, range.RightColumn);

					if (flxTopLeftFormat != null && flxBottomRightFormat != null)
					{
						borders = new FlexCelBordersAdapter(palette, flxTopLeftFormat.Borders, flxBottomRightFormat.Borders);
					}
				}

				return borders;
			}
		}

		IBorders borders;

		public Alignment HAlignment
		{
			get
			{
				var flxFormat = GetFormat(range.TopRow, range.LeftColumn);

				return flxFormat != null
					? flxFormat.HAlignment.ToAlignment()
					: Alignment.None;
			}
		}

		public Alignment VAlignment
		{
			get
			{
				var flxFormat = GetFormat(range.TopRow, range.LeftColumn);

				return flxFormat != null
					? flxFormat.VAlignment.ToAlignment()
					: Alignment.None;
			}
		}

		public Color BackgroundColor
		{
			get
			{
				Color result = Color.Empty;

				var flxFormat = GetFormat(range.TopRow, range.LeftColumn);

				if (flxFormat != null && flxFormat.FillPattern.BgColor != TExcelColor.Automatic)
				{
					result = flxFormat.FillPattern.Pattern == TFlxPatternStyle.Automatic
						? flxFormat.FillPattern.BgColor.ToColor(palette)
						: flxFormat.FillPattern.FgColor.ToColor(palette);
				}

				return result;
			}
		}

		public PatternStyle Pattern
		{
			get
			{
				var flxFormat = GetFormat(range.TopRow, range.LeftColumn);
				var pattern = flxFormat != null ? flxFormat.FillPattern.Pattern : TFlxPatternStyle.None;
				return pattern.ToPattern();
			}
		}

		public bool WrapText
		{
			get
			{
				var flxFormat = GetFormat(range.TopRow, range.LeftColumn);

				return flxFormat != null && flxFormat.WrapText;
			}
		}

		public string TextFormat
		{
			get
			{
				var flxFormat = GetFormat(range.TopRow, range.LeftColumn);
				return flxFormat != null ? flxFormat.Format : string.Empty;
			}
		}
	}
}
