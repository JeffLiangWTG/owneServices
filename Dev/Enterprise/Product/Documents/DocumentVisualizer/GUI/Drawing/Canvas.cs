using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.GUI
{
	public sealed class Canvas : ICanvas
	{
		public Canvas(Graphics graphics, float scale)
		{
			Argument.NotNull(graphics, nameof(graphics));
			if (scale <= 0f)
			{
				throw new ArgumentOutOfRangeException(nameof(graphics));
			}

			this.graphics = graphics;
			Scale = scale;
		}

		readonly Graphics graphics;

		public float Scale { get; }

		public float DpiX => graphics.DpiX;
		public float DpiY => graphics.DpiY;

		public void DrawImage(Image image, RectangleF area)
		{
			graphics.DrawImage(image, area.X, area.Y, area.Width, area.Height);
		}

		public void DrawLine(IPen pen, PointF start, PointF end)
		{
			using (var drawingPen = GetDrawingPen(pen))
			{
				graphics.DrawLine(drawingPen, start, end);
			}
		}

		public void DrawRectangle(IPen pen, RectangleF rectangle)
		{
			using (var drawingPen = GetDrawingPen(pen))
			{
				graphics.DrawRectangle(drawingPen, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
			}
		}

		public void DrawString(IFont font, StringFormat stringFormat, RectangleF rectangle, float size, string text, bool printJustified)
		{
			using (var drawingFont = GetDrawingFont(font, size))
			using (var drawingBrush = GetDrawingBrush(font.Color))
			{
				var drawStringFormat = new StringFormat(stringFormat);
				drawStringFormat.FormatFlags &= ~StringFormatFlags.MeasureTrailingSpaces;

				if (printJustified)
				{
					DrawJustifiedText(drawingFont, drawingBrush, rectangle, text);
				}
				else
				{
					var isVerticalText = font.Rotation == 90 || font.Rotation == 180;

					if (drawStringFormat.LineAlignment == StringAlignment.Center)
					{
						if (isVerticalText && graphics.MeasureString(text, drawingFont, (int)rectangle.Width, stringFormat).Height > rectangle.Height
							|| !isVerticalText && graphics.MeasureString(text, drawingFont, (int)rectangle.Height, stringFormat).Width > rectangle.Width)
						{
							drawStringFormat.LineAlignment = StringAlignment.Near;
						}
					}

					if (isVerticalText)
					{
						var transform = graphics.Transform;
						var clip = graphics.Clip;
						var rectangleForVerticalString = new RectangleF(0, 0, rectangle.Height, rectangle.Width);

						TransformGraphics(rectangle, font.Rotation);
						graphics.SetClip(rectangleForVerticalString);

						graphics.DrawString(text, drawingFont, drawingBrush, rectangleForVerticalString, drawStringFormat);

						graphics.Clip = clip;
						graphics.Transform = transform;
					}
					else
					{
						var clip = graphics.Clip;
						graphics.SetClip(rectangle);
						graphics.DrawString(text, drawingFont, drawingBrush, rectangle, drawStringFormat);
						graphics.Clip = clip;
					}
				}
			}
		}

		void TransformGraphics(RectangleF rectangle, int rotation)
		{
			if (rotation == 90)
			{
				graphics.TranslateTransform(rectangle.X, rectangle.Y + rectangle.Height);
				graphics.RotateTransform(-90);
			}
			else if (rotation == 180)
			{
				graphics.TranslateTransform(rectangle.X + rectangle.Width, rectangle.Y);
				graphics.RotateTransform(90);
			}
		}

		public void FillRectangle(Color color, RectangleF rectangle)
		{
			using (var drawingBrush = GetDrawingBrush(color))
			{
				graphics.FillRectangle(drawingBrush, rectangle);
			}
		}

		public void FillRectangle(Color topColor, Color bottomBolor, RectangleF rectangle, LinearGradientMode mode)
		{
			using (var drawingBrush = GetDrawingBrush(topColor, bottomBolor, rectangle, mode))
			{
				graphics.FillRectangle(drawingBrush, rectangle);
			}
		}

		public void FillPolygon(Color color, PointF[] points)
		{
			using (var drawingBrush = GetDrawingBrush(color))
			{
				graphics.FillPolygon(drawingBrush, points);
			}
		}

		public float ShrinkToFit(string content, SizeF area, IFont font, StringFormat stringFormat)
		{
			using (var shrinkToFitCalculator = new ShrinkToFitCalculator(graphics))
			{
				return shrinkToFitCalculator.ShrinkToFit(content, area, font, stringFormat);
			}
		}

		System.Drawing.Pen GetDrawingPen(IPen pen)
		{
			var drawingPen = new System.Drawing.Pen(pen.Color, pen.Thickness);
			drawingPen.DashStyle = pen.DashStyle;

			if (pen.DashPattern != null)
			{
				drawingPen.DashPattern = pen.DashPattern.ToArray();
			}

			return drawingPen;
		}

		Brush GetDrawingBrush(Color color)
		{
			return new SolidBrush(color);
		}

		Brush GetDrawingBrush(Color topColor, Color bottomColor, RectangleF rectangle, LinearGradientMode mode)
		{
			return new LinearGradientBrush(rectangle, topColor, bottomColor, mode);
		}

		System.Drawing.Font GetDrawingFont(IFont font, float size)
		{
			var drawingFont = new System.Drawing.Font(font.Name, size, font.Style, GraphicsUnit.Point);
			return drawingFont;
		}

		void DrawJustifiedText(System.Drawing.Font drawingFont, Brush brush, RectangleF rectangle, string text)
		{
			var numberOfLines = 0;
			var maxLines = Convert.ToInt32(rectangle.Height / drawingFont.Height);
			var spaceWidth = graphics.MeasureString(" ", drawingFont).Width;

			Func<string, float> measureText = str => graphics.MeasureString(str, drawingFont).Width;
			DrawText drawText = (words, widths, spaceBetweenWords) =>
			{
				var x = rectangle.X;
				var y = rectangle.Top + numberOfLines * drawingFont.Height;
				var point = new PointF(x, y);

				foreach (var word in words)
				{
					graphics.DrawString(word, drawingFont, brush, point);
					x = x + widths[word] + spaceBetweenWords;
				}

				numberOfLines++;

				return numberOfLines < maxLines;
			};

			var helper = new JustifiedTextDrawHelper(measureText, drawText);
			helper.Draw(text, rectangle.Width, spaceWidth);
		}
	}
}
