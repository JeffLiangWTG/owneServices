using System.Drawing;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	class TextPainter : Painter
	{
		public TextPainter(IText text)
		{
			Argument.NotNull(text, nameof(text));

			this.text = text;
		}

		readonly IText text;

		protected override RectangleF CalculatePaintArea()
		{
			var textPaintSize = PaintAreaCalculator.CalculateTextPaintAreaSize(Dpi, text.Size, text.Padding, text.Content, text.Font, Zoom, text.Wrap);
			var location = CalculatePaintLocation(textPaintSize);
			return new RectangleF(location, textPaintSize);
		}

		PointF CalculatePaintLocation(SizeF textPaintSize)
		{
			var leftPadding = Util.ConvertToPixelsF(Dpi.X, text.Padding.Left) * Zoom;
			var textX = Util.ConvertToPixelsF(Dpi.X, text.Location.X * Zoom) + leftPadding;

			var topPadding = Util.ConvertToPixelsF(Dpi.X, text.Padding.Top) * Zoom;
			var textY = Util.ConvertToPixelsF(Dpi.Y, text.Location.Y * Zoom) + topPadding;

			var cellArea = PaintAreaCalculator.CalculateCellPaintAreaSize(Dpi, text.Size, text.Padding, Zoom);
			var isVerticalText = text.Font.Rotation == 90 || text.Font.Rotation == 180;

			if (isVerticalText)
			{
				if (text.HAlignment == Alignment.Center)
				{
					textX += cellArea.Width / 2 - textPaintSize.Width / 2;
				}
				else if (text.HAlignment == Alignment.Right)
				{
					textX += cellArea.Width - textPaintSize.Width;
				}
			}
			else
			{
				if (text.VAlignment == Alignment.Center)
				{
					textY += cellArea.Height / 2 - textPaintSize.Height / 2;
				}
				else if (text.VAlignment == Alignment.Bottom)
				{
					textY += cellArea.Height - textPaintSize.Height;
				}
			}

			return new PointF(textX, textY);
		}

		protected override void Paint(ICanvas canvas, bool isDiagnosticsEnabled)
		{
			var stingFormat = WorksheetExtensions.GetStringFormat(text.HAlignment, text.VAlignment, text.Wrap);

			DrawText(canvas,
				text.Content,
				text.Font,
				PaintArea,
				stingFormat,
				Zoom,
				text.HAlignment == Alignment.Justify);
		}

		internal void UpdateStringFormatForVerticalText(StringFormat stingFormat)
		{
			if (text.Font.Rotation == 90)
			{
				if (text.HAlignment != Alignment.Center)
				{
					stingFormat.LineAlignment = text.HAlignment == Alignment.Left ? StringAlignment.Near : StringAlignment.Far;
				}
				else
				{
					stingFormat.LineAlignment = StringAlignment.Center;
				}

				if (text.VAlignment != Alignment.Center)
				{
					stingFormat.Alignment = text.VAlignment == Alignment.Top ? StringAlignment.Far : StringAlignment.Near;
				}
				else
				{
					stingFormat.Alignment = StringAlignment.Center;
				}
			}
			else if (text.Font.Rotation == 180)
			{
				if (text.HAlignment != Alignment.Center)
				{
					stingFormat.LineAlignment = text.HAlignment == Alignment.Left ? StringAlignment.Far : StringAlignment.Near;
				}
				else
				{
					stingFormat.LineAlignment = StringAlignment.Center;
				}

				if (text.VAlignment != Alignment.Center)
				{
					stingFormat.Alignment = text.VAlignment == Alignment.Top ? StringAlignment.Near : StringAlignment.Far;
				}
				else
				{
					stingFormat.Alignment = StringAlignment.Center;
				}
			}
		}

		protected void DrawText(ICanvas canvas, string content, IFont font, RectangleF paintArea, StringFormat stringFormat, float zoom, bool printJustified)
		{
			var fontSize = font.Size * zoom;

			UpdateStringFormatForVerticalText(stringFormat);

			canvas.DrawString(font, stringFormat, paintArea, fontSize, content, printJustified);
		}
	}
}
