using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI
{
	public static class TextRendererHelper
	{
		public static void UseTextRendererFromReg()
		{
			textRendererRegAvailable = true;
		}

		static bool IsTextRendererRegAvailable => !Db.DatabaseUpgradedExceptionHasBeenThrownInConnection && textRendererRegAvailable;

		[ThreadSafe]
		static bool textRendererRegAvailable;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception Message")]
		const string unsupportedExceptionMessage = "Selected Rendering Engine is not supported";

		public static TextRendererType RenderingEngine { get { return IsTextRendererRegAvailable ? (TextRendererType)Enum.Parse(typeof(TextRendererType), EnvProxy.Instance.Registry.GraphicRenderingEngineRegItem) : default(TextRendererType); } }

		public static void DrawText(Graphics graphic, string text, Font font, RectangleF boundsF, Brush brush, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			switch (renderingEngine)
			{
				case TextRendererType.GDI:
					TextRenderer.DrawText(graphic, text, font, Rectangle.Ceiling(boundsF), (brush as SolidBrush)?.Color ?? DefaultForeColor, ConvertToTextFormatFlags(format));
					break;
				case TextRendererType.GDIPlus:
					graphic.DrawString(text, font, brush, boundsF, format);
					break;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", renderingEngine.ToString(), unsupportedExceptionMessage));
			}
		}

		public static void DrawText(Graphics graphic, string text, Font font, RectangleF boundsF, Brush foreBrush, Brush backBrush, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			switch (renderingEngine)
			{
				case TextRendererType.GDI:
					TextRenderer.DrawText(graphic, text, font, Rectangle.Ceiling(boundsF), (foreBrush as SolidBrush)?.Color ?? DefaultForeColor, (backBrush as SolidBrush)?.Color ?? DefaultBackColor, ConvertToTextFormatFlags(format));
					break;
				case TextRendererType.GDIPlus:
					graphic.DrawString(text, font, foreBrush, boundsF, format);
					break;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", renderingEngine.ToString(), unsupportedExceptionMessage));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Params passed in are supposed to be scaled")]
		public static void DrawText(Graphics graphic, string text, Font font, PointF pointfScaled, Brush brush, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			switch (renderingEngine)
			{
				case TextRendererType.GDI:
					TextRenderer.DrawText(graphic, text, font, new Point((int)pointfScaled.X, (int)pointfScaled.Y), (brush as SolidBrush)?.Color ?? DefaultForeColor, ConvertToTextFormatFlags(format));
					break;
				case TextRendererType.GDIPlus:
					graphic.DrawString(text, font, brush, pointfScaled, format);
					break;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", renderingEngine.ToString(), unsupportedExceptionMessage));
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "Params passed in are supposed to be scaled")]
		public static void DrawText(Graphics graphic, string text, Font font, PointF pointfScaled, Brush foreBrush, Brush backBrush, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			switch (renderingEngine)
			{
				case TextRendererType.GDI:
					TextRenderer.DrawText(graphic, text, font, new Point((int)pointfScaled.X, (int)pointfScaled.Y), (foreBrush as SolidBrush)?.Color ?? DefaultForeColor, (backBrush as SolidBrush)?.Color ?? DefaultBackColor, ConvertToTextFormatFlags(format));
					break;
				case TextRendererType.GDIPlus:
					graphic.DrawString(text, font, foreBrush, pointfScaled, format);
					break;
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", renderingEngine.ToString(), unsupportedExceptionMessage));
			}
		}

		public static void DrawText(Graphics graphic, string text, Font font, float x, float y, Brush brush, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			DrawText(graphic, text, font, new PointF(x, y), brush, format, renderingEngine);
		}

		public static void DrawText(Graphics graphic, string text, Font font, float x, float y, Brush foreBrush, Brush backBrush, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			DrawText(graphic, text, font, new PointF(x, y), foreBrush, backBrush, format, renderingEngine);
		}

		public static SizeF MeasureText(Graphics graphic, string text, Font font, Size proposedSize, StringFormat format = null, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;

			switch (renderingEngine)
			{
				case TextRendererType.GDI:
					return TextRenderer.MeasureText(graphic, text, font, proposedSize, ConvertToTextFormatFlags(format));
				case TextRendererType.GDIPlus:
					return graphic.MeasureString(text, font, proposedSize, format);
				default:
					throw new NotSupportedException(string.Format(CultureInfo.InvariantCulture, "{0}: {1}", renderingEngine.ToString(), unsupportedExceptionMessage));
			}
		}

		public static SizeF MeasureText(Graphics graphic, string text, Font font, TextRendererType? renderingEngine = null)
		{
			renderingEngine = renderingEngine ?? RenderingEngine;
			return MeasureText(graphic, text, font, Size.Empty, null, renderingEngine);
		}

#if DEBUG
		public
#endif
		static TextFormatFlags ConvertToTextFormatFlags(Enum inputStringFormatFlags, Enum comparisonStringFormatFlags, TextFormatFlags matchedTextFormatFlags, bool isExpectedToMatch = true)
		{
			var isMatchedFlags = inputStringFormatFlags is StringFormatFlags ? inputStringFormatFlags.HasFlag(comparisonStringFormatFlags) : inputStringFormatFlags.Equals(comparisonStringFormatFlags);
			return isMatchedFlags == isExpectedToMatch
				? matchedTextFormatFlags
				: TextFormatFlags.Default;
		}

#if DEBUG
		public
#endif
		static TextFormatFlags ConvertToTextFormatFlags(StringFormat format)
		{
			if (format == null)
			{
				return TextFormatFlags.TextBoxControl | TextFormatFlags.PreserveGraphicsClipping | TextFormatFlags.NoPrefix;
			}

			foreach (var flag in unsupportedFormatFlags)
			{
				var isUnsupportedFormatFlag = flag is StringFormatFlags ? format.FormatFlags.HasFlag(flag) : format.Trimming.Equals(flag);
				if (isUnsupportedFormatFlag)
				{
					ErrorReporter.ReportOnce("UnsupportedConversionFormatUsed", "StringFormat passed to ConvertToTextFormatFlags() is not supported by GDI TextRenderingEngine. FormatFlag: " + flag);
				}
			}

			if (format.FormatFlags.HasFlag(StringFormatFlags.FitBlackBox))
			{
				format.FormatFlags &= ~StringFormatFlags.FitBlackBox;
				format.FormatFlags |= StringFormatFlags.NoClip;
			}

			//Default flags
			var result = TextFormatFlags.Default | TextFormatFlags.NoPrefix;

			// GDI default is NoWrap, GDI+ default is Wrap so we invert them here
			result |= ConvertToTextFormatFlags(format.FormatFlags, StringFormatFlags.NoWrap, TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl, false);
			// GDI default has no clipping setting when both PreserveGraphicsClipping and NoClipping are not enabled, GDI+ default is Clipping so we invert them here
			result |= ConvertToTextFormatFlags(format.FormatFlags, StringFormatFlags.NoClip, TextFormatFlags.PreserveGraphicsClipping, false);

			result |= ConvertToTextFormatFlags(format.FormatFlags, StringFormatFlags.NoClip, TextFormatFlags.NoClipping);
			result |= ConvertToTextFormatFlags(format.FormatFlags, StringFormatFlags.DirectionRightToLeft, TextFormatFlags.RightToLeft);
			result |= ConvertToTextFormatFlags(format.Trimming, StringTrimming.EllipsisCharacter, TextFormatFlags.EndEllipsis);
			result |= ConvertToTextFormatFlags(format.Trimming, StringTrimming.EllipsisPath, TextFormatFlags.PathEllipsis);
			result |= ConvertToTextFormatFlags(format.Trimming, StringTrimming.EllipsisWord, TextFormatFlags.WordEllipsis);
			result |= ConvertToTextFormatFlags(format.Alignment, StringAlignment.Center, TextFormatFlags.HorizontalCenter);
			result |= ConvertToTextFormatFlags(format.Alignment, StringAlignment.Near, TextFormatFlags.Left);
			result |= ConvertToTextFormatFlags(format.Alignment, StringAlignment.Far, TextFormatFlags.Right);
			result |= ConvertToTextFormatFlags(format.LineAlignment, StringAlignment.Center, TextFormatFlags.VerticalCenter);
			result |= ConvertToTextFormatFlags(format.LineAlignment, StringAlignment.Near, TextFormatFlags.Top);
			result |= ConvertToTextFormatFlags(format.LineAlignment, StringAlignment.Far, TextFormatFlags.Bottom);

			return result;
		}

		static readonly Color DefaultForeColor = Color.Black;
		static readonly Color DefaultBackColor = Color.Transparent;

		[ThreadSafe]
		static readonly List<Enum> unsupportedFormatFlags = new List<Enum>()
		{
			StringFormatFlags.DirectionVertical,
			StringFormatFlags.DisplayFormatControl,
			StringFormatFlags.NoFontFallback,
			StringFormatFlags.MeasureTrailingSpaces,
			StringFormatFlags.LineLimit,
			StringTrimming.None,
			StringTrimming.Word
			//StringFormatFlags.FitBlackBox, //Convert to NoClipping
			//StringTrimming.Character, //Default trimmingFlag, Do nothing
			//HotkeyPrefix.Hide //We don't care
		};
	}
}
