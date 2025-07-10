using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	public static class FlexCelExtensions
	{
		#region FontStyle

		internal static FontStyle ToFontStyle(this TFlxFontStyles flxFontStyle)
		{
			switch (flxFontStyle)
			{
				case TFlxFontStyles.Bold:
					return FontStyle.Bold;

				case TFlxFontStyles.Italic:
					return FontStyle.Italic;

				case TFlxFontStyles.StrikeOut:
					return FontStyle.Strikeout;

				default:
					return FontStyle.Regular;
			}
		}

		internal static TFlxFontStyles ToFlxFontStyle(this FontStyle fontStyle)
		{
			switch (fontStyle)
			{
				case FontStyle.Bold:
					return TFlxFontStyles.Bold;

				case FontStyle.Italic:
					return TFlxFontStyles.Italic;

				case FontStyle.Strikeout:
					return TFlxFontStyles.StrikeOut;

				default:
					return TFlxFontStyles.None;
			}
		}

		#endregion

		#region Border

		internal static BorderStyle ToBorder(this TFlxBorderStyle flxBorderStyle)
		{
			switch (flxBorderStyle)
			{
				case TFlxBorderStyle.None:
					return BorderStyle.None;

				case TFlxBorderStyle.Thick:
					return BorderStyle.Thick;

				case TFlxBorderStyle.Medium:
					return BorderStyle.Medium;

				case TFlxBorderStyle.Dashed:
					return BorderStyle.Dashed;

				case TFlxBorderStyle.Dotted:
					return BorderStyle.Dotted;

				case TFlxBorderStyle.Dash_dot:
					return BorderStyle.DashDot;

				case TFlxBorderStyle.Dash_dot_dot:
					return BorderStyle.DashDotDot;

				default:
					return BorderStyle.Thin;
			}
		}

		internal static TFlxBorderStyle ToFlxBorder(this BorderStyle borderStyle)
		{
			switch (borderStyle)
			{
				case BorderStyle.None:
					return TFlxBorderStyle.None;

				case BorderStyle.Thick:
					return TFlxBorderStyle.Thick;

				case BorderStyle.Medium:
					return TFlxBorderStyle.Medium;

				case BorderStyle.Dashed:
					return TFlxBorderStyle.Dashed;

				case BorderStyle.Dotted:
					return TFlxBorderStyle.Dotted;

				case BorderStyle.DashDot:
					return TFlxBorderStyle.Dash_dot;

				case BorderStyle.DashDotDot:
					return TFlxBorderStyle.Dash_dot;

				default:
					return TFlxBorderStyle.Thin;
			}
		}

		#endregion

		#region Pattern

		internal static PatternStyle ToPattern(this TFlxPatternStyle flxPatternStyle)
		{
			if (flxPatternStyle == TFlxPatternStyle.Solid)
			{
				return PatternStyle.Solid;
			}

			return PatternStyle.None;
		}

		internal static TFlxPatternStyle ToTFlxPattern(this PatternStyle patternStyle)
		{
			if (patternStyle == PatternStyle.Solid)
			{
				return TFlxPatternStyle.Solid;
			}

			return TFlxPatternStyle.None;
		}

		#endregion

		#region Alignment

		internal static Alignment ToAlignment(this THFlxAlignment hFlxAlignment)
		{
			switch (hFlxAlignment)
			{
				case THFlxAlignment.center:
					return Alignment.Center;

				case THFlxAlignment.right:
					return Alignment.Right;

				case THFlxAlignment.justify:
					return Alignment.Justify;

				default:
					return Alignment.Left;
			}
		}

		internal static Alignment ToAlignment(this TVFlxAlignment vFlxAlignment)
		{
			switch (vFlxAlignment)
			{
				case TVFlxAlignment.top:
					return Alignment.Top;

				case TVFlxAlignment.bottom:
					return Alignment.Bottom;

				default:
					return Alignment.Center;
			}
		}

		internal static THFlxAlignment ToHFlxAlignment(this Alignment alignment)
		{
			switch (alignment)
			{
				case Alignment.Center:
					return THFlxAlignment.center;

				case Alignment.Right:
					return THFlxAlignment.right;

				case Alignment.Justify:
					return THFlxAlignment.justify;

				default:
					return THFlxAlignment.left;
			}
		}

		internal static TVFlxAlignment ToVFlxAlignment(this Alignment alignment)
		{
			switch (alignment)
			{
				case Alignment.Top:
					return TVFlxAlignment.top;

				case Alignment.Bottom:
					return TVFlxAlignment.bottom;

				default:
					return TVFlxAlignment.center;
			}
		}

		#endregion

		#region ToXlsFile

		public static XlsFile ToXlsFile(this IWorksheet worksheet)
		{
			Argument.NotNull(worksheet, nameof(worksheet));

			var builder = new XlsFileBuilder(worksheet);
			return builder.Build();
		}

		#endregion
	}
}
