using System;
using System.Drawing;
using CargoWise.Common;
using FlexCel.Core;
using FlexCel.Pdf;

namespace Enterprise.RemotePrinting.Engine
{
	public class TextWatermark : Watermark
	{
		public TextWatermark(string text, WatermarkHorizontalAlign horizontalAlign, WatermarkVerticalAlign verticalAlign,
			float horizontalOffset, float verticalOffset, int rotation, Color textColor, string fontName, int fontSize, FontStyle fontStyle)
			: base(horizontalAlign, verticalAlign, horizontalOffset, verticalOffset, rotation)
		{
			Text = Argument.NotNull(text, nameof(text)); // Suggested By ReviewBot
			TextColor = textColor;
			FontName = fontName;
			FontSize = fontSize;
			FontStyle = fontStyle;
			TextDecoration = GetTextDecoration(FontStyle);
		}

		#region Properties

		public FontStyle FontStyle { get; }
		TUITextDecoration TextDecoration { get; }

		public string Text { get; }

		public Color TextColor { get; }
		public string FontName { get; }
		public int FontSize { get; }

		public override string AsText
		{
			get { return Text; }
		}

		#endregion

		public override void Draw(Graphics gr, SizeF pageSize)
		{
			Argument.NotNull(gr, nameof(gr));

			using (var textFont = new Font(FontName, FontSize, FontStyle))
			using (var textBrush = new SolidBrush(TextColor))
			{
				var textLines = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
				if (textLines.Length == 1)
				{
					GraphicsDrawSingleLine(gr, pageSize, textFont, textBrush);
				}
				else //if (textLines.Length > 1)
				{
					GraphicsDrawMultipleLines(gr, pageSize, textFont, textBrush, textLines);
				}
			}
		}

		void GraphicsDrawSingleLine(Graphics gr, SizeF pageSize, Font textFont, Brush textBrush)
		{
			Argument.NotNull(gr, nameof(gr)); // Suggested By ReviewBot

			var textSize = gr.MeasureString(Text, textFont);
			var textPos = GetAlign(textSize, pageSize);
			RotateCanvas(gr, textSize, textPos);

			gr.DrawString(Text, textFont, textBrush, textPos);
		}

		void GraphicsDrawMultipleLines(Graphics gr, SizeF pageSize, Font textFont, Brush textBrush, string[] lines)
		{
			Argument.NotNull(lines, nameof(lines)); // Suggested By ReviewBot

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				var textSize = gr.MeasureString(line, textFont);
				var textPos = GetAlign(textSize, pageSize);

				if (i == 0) // only need to rotate once
				{
					RotateCanvas(gr, textSize, textPos);
				}

				var verticalOffset = CalculateVerticalOffset(lines.Length, textSize.Height, i);
				gr.DrawString(line, textFont, textBrush, textPos.X, textPos.Y + verticalOffset);
			}
		}

		/// <summary>
		/// Calculates the vertical offset value relative to the value when there is only one line.
		/// </summary>
		float CalculateVerticalOffset(int totalLinesNumber, float lineHeight, int lineSequenceNumber)
		{
			switch (VerticalAlign)
			{
				// the lines will display below fist line
				case WatermarkVerticalAlign.Top: return lineHeight * lineSequenceNumber;
				// the lines will display above the last line
				case WatermarkVerticalAlign.Bottom: return lineHeight * (lineSequenceNumber - totalLinesNumber + 1);
				// the lines will display above or below the middle of the page symmetrically
				case WatermarkVerticalAlign.Middle:
				default: return lineHeight * 0.5f * (2 * lineSequenceNumber - totalLinesNumber + 1);
			}
		}

		public override void Draw(PdfWriter pdf, SizeF pageSize)
		{
			Argument.NotNull(pdf, nameof(pdf));

			using (var textFont = TUIFont.Create(FontName, FontSize, (TUIFontStyle)FontStyle))
			using (var textBrush = new SolidBrush(TextColor))
			{
				var textLines = Text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
				if (textLines.Length == 1)
				{
					PDFDrawSingleLine(pdf, pageSize, textFont, textBrush);
				}
				else
				{
					PDFDrawMultipleLines(pdf, pageSize, textFont, textBrush, textLines);
				}
			}
		}

		void PDFDrawSingleLine(PdfWriter pdf, SizeF pageSize, TUIFont textFont, Brush textBrush)
		{
			Argument.NotNull(pdf, nameof(pdf)); // Suggested By ReviewBot

			SizeF textSize = pdf.MeasureString(Text, textFont);
			var textPos = GetAlign(textSize, pageSize);
			RotatePdfCanvas(pdf, textSize, textPos);

			pdf.DrawString(Text, textFont, TextDecoration, textBrush, textPos.X, textPos.Y + textSize.Height);
		}

		void PDFDrawMultipleLines(PdfWriter pdf, SizeF pageSize, TUIFont textFont, Brush textBrush, string[] lines)
		{
			Argument.NotNull(lines, nameof(lines)); // Suggested By ReviewBot

			for (int i = 0; i < lines.Length; i++)
			{
				var line = lines[i];
				SizeF textSize = pdf.MeasureString(line, textFont);
				var textPos = GetAlign(textSize, pageSize);

				if (i == 0) // only need to rotate once
				{
					RotatePdfCanvas(pdf, textSize, textPos);
				}

				var verticalOffset = CalculateVerticalOffset(lines.Length, textSize.Height, i);
				pdf.DrawString(line, textFont, TextDecoration, textBrush, textPos.X, textPos.Y + verticalOffset + textSize.Height);
			}
		}

		TUITextDecoration GetTextDecoration(FontStyle baseFontStyle)
		{
			var underline = baseFontStyle.HasFlag(FontStyle.Underline) ? TUIUnderline.Single : TUIUnderline.None;
			var strikeout = baseFontStyle.HasFlag(FontStyle.Strikeout) ? TUIStrikeout.Single : TUIStrikeout.None;
			return new TUITextDecoration(underline, strikeout);
		}
	}
}
