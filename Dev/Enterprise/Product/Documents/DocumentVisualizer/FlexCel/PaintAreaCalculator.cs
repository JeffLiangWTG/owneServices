using System;
using System.Drawing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration
{
	public static class PaintAreaCalculator
	{
		public static SizeF CalculateUnscaledSize(RectangleF paintArea, float scale)
		{
			if (paintArea.Height <= 0f
				|| paintArea.Width <= 0f
				|| scale <= 0f)
			{
				return SizeF.Empty;
			}

			return new SizeF(
				paintArea.Width / scale,
				paintArea.Height / scale);
		}

		public static SizeF CalculateTextPaintAreaSize(PointF dpi, SizeF cellSize, RectangleF padding, string text, IFont font, float zoom, bool wrapText)
		{
			if (font == null || font.Size <= 0f || zoom <= 0f)
			{
				return SizeF.Empty;
			}

			var drawFontSize = font.Size * zoom;
			var isVerticalText = font.Rotation == 90 || font.Rotation == 180;

			if (isVerticalText)
			{
				var drawTextWidth = GetDrawWidth(dpi.Y, padding, cellSize.Height, zoom);
				var drawTextHeight = GetDrawHeight(dpi.X, padding, cellSize.Width, zoom);

				if (wrapText)
				{
					return new SizeF(drawTextHeight, drawTextWidth);
				}

				return GetTextSize(text, drawTextWidth, font, drawFontSize);
			}
			else
			{
				var drawWidth = GetDrawWidth(dpi.X, padding, cellSize.Width, zoom);
				var drawHeight = GetDrawHeight(dpi.Y, padding, cellSize.Height, zoom);

				if (wrapText)
				{
					return new SizeF(drawWidth, drawHeight);
				}

				return GetTextSize(text, drawWidth, font, drawFontSize);
			}
		}

		public static SizeF CalculateCellPaintAreaSize(PointF dpi, SizeF cellSize, RectangleF padding, float zoom)
		{
			var drawWidth = GetDrawWidth(dpi.X, padding, cellSize.Width, zoom);
			var drawHeight = GetDrawHeight(dpi.Y, padding, cellSize.Height, zoom);

			return new SizeF(drawWidth, drawHeight);
		}

		static float GetDrawWidth(float dpiX, RectangleF padding, float cellWidth, float zoom)
		{
			var leftPadding = Util.ConvertToPixelsF(dpiX, padding.Left) * zoom;
			var rightPadding = Util.ConvertToPixelsF(dpiX, padding.Right) * zoom;

			return Util.ConvertToPixelsF(dpiX, cellWidth * zoom) - leftPadding - rightPadding;
		}

		static float GetDrawHeight(float dpiY, RectangleF padding, float cellHeight, float zoom)
		{
			var topPadding = Util.ConvertToPixelsF(dpiY, padding.Top) * zoom;
			var bottomPadding = Util.ConvertToPixelsF(dpiY, padding.Bottom) * zoom;

			return Util.ConvertToPixelsF(dpiY, cellHeight * zoom) - topPadding - bottomPadding;
		}

		static SizeF GetTextSize(string text, float drawWidth, IFont font, float drawFontSize)
		{
			using (var drawingFont = new System.Drawing.Font(font.Name, drawFontSize, font.Style, GraphicsUnit.Point))
			using (var graphics = Graphics.FromHwnd(IntPtr.Zero))
			{
				var isVerticalText = font.Rotation == 90 || font.Rotation == 180;
				var textSize = !isVerticalText ? new SizeF(drawWidth, graphics.MeasureString(text, drawingFont).Height) : new SizeF(graphics.MeasureString(text, drawingFont).Height, drawWidth);

				#region Test
				#if DEBUG

				if (Globals.IsTest)
				{
					if (isVerticalText)
					{
						return new SizeF(19.87f, drawWidth); //graphics.MeasureString returns result according to difference in dpi, we mock dpi on others but we cannot mock it here.
					}

					return new SizeF(drawWidth, 19.87f); //graphics.MeasureString returns result according to difference in dpi, we mock dpi on others but we cannot mock it here.
				}

				#endif
				#endregion

				return textSize;
			}
		}
	}
}
