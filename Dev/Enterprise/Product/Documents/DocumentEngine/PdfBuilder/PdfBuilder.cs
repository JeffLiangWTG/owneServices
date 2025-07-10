using System;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Pdf;

namespace Enterprise.DocumentEngine.PdfBuilder
{
	public class PdfBuilder : IPdfBuilder
	{
		public void Build(PdfElement[] elements, Stream output)
		{
			var document = new PdfDocument();

			var page = document.AddPage();

			var gfx = XGraphics.FromPdfPage(page);

			var sectionOffset = 10;
			foreach (var element in elements)
			{
				if (sectionOffset > 10 && sectionOffset + RetrieveSectionHeight(element) > page.Height)
				{
					page = document.AddPage();

					sectionOffset = 10;
				}

				if (element is PdfTextSection textSection)
				{
					sectionOffset = DrawTextSection(sectionOffset, gfx, textSection, page);
				}
				else if (element is PdfTableSection tableSection)
				{
					sectionOffset = DrawTableSection(sectionOffset, gfx, tableSection, page);
				}
				else
				{
					throw new NotSupportedException($"The element {element.GetType().Name} is not supported");
				}
			}

			document.Save(output, false);
		}

		public static int DrawTextSection(int sectionOffset, XGraphics graph, PdfTextSection textSection, PdfPage page)
		{
			var tf = new XTextFormatter(graph);

			var format = new XStringFormat();
			format.LineAlignment = XLineAlignment.Near;
			format.Alignment = XStringAlignment.Near;

			if (textSection.IsCentered)
			{
				tf.Alignment = XParagraphAlignment.Center;
			}

			var font = new XFont(textSection.FontName, textSection.FontHeight, XFontStyle.Regular);

			var y = sectionOffset + textSection.MarginTop;
			foreach (var line in textSection.Texts)
			{
				var tabIndex = line.IndexOf('\t');
				if (tabIndex == -1)
				{
					tf.DrawString(line, font, XBrushes.Black,
						new XRect(textSection.MarginLeft + textSection.Padding, y + textSection.Padding, page.Width - textSection.Padding, textSection.LineHeight - textSection.Padding), format);
				}
				else
				{
					var subline1 = line.Substring(0, tabIndex);
					var subline2 = line.Substring(tabIndex + 1);

					tf.DrawString(subline1, font, XBrushes.Black,
						new XRect(textSection.MarginLeft + textSection.Padding, y + textSection.Padding, page.Width / 2, textSection.LineHeight - textSection.Padding), format);
					tf.DrawString(subline2, font, XBrushes.Black,
						new XRect(textSection.MarginLeft + textSection.Padding + page.Width / 2, y + textSection.Padding, page.Width / 2 - textSection.Padding, textSection.LineHeight - textSection.Padding), format);
				}

				y += textSection.LineHeight;
			}

			return y;
		}

		public static int DrawTableSection(int sectionOffset, XGraphics graph, PdfTableSection tableSection, PdfPage page)
		{
			var tf = new XTextFormatter(graph);

			// Text format
			var format = new XStringFormat();
			format.LineAlignment = XLineAlignment.Near;
			format.Alignment = XStringAlignment.Near;

			var header_style = new XSolidBrush(XColors.LightGray);
			var pen = new XPen(XColors.Black, 1);

			var font = new XFont(tableSection.FontName, tableSection.FontHeight, XFontStyle.Regular);

			var totalWidth = tableSection.Widths.Sum(x => x);

			var y = sectionOffset + tableSection.MarginTop;
			for (var i = 0; i <= tableSection.Rows.Count; i++)
			{
				var x = tableSection.MarginLeft;

				if (i == 0)
				{
					graph.DrawRectangle(header_style, x, y, totalWidth, tableSection.LineHeight);
				}

				for (var j = 0; j < tableSection.Headers.Count; j++)
				{
					graph.DrawRectangle(pen, new XRect(x, y, tableSection.Widths[j], tableSection.LineHeight));

					if (i == 0)
					{
						tf.DrawString(tableSection.Headers[j], font, XBrushes.Black,
							new XRect(x + tableSection.Padding, y + tableSection.Padding, tableSection.Widths[j] - tableSection.Padding, tableSection.LineHeight - tableSection.Padding), format);
					}
					else
					{
						if (tableSection.Alignments.Count > 0)
						{
							switch (tableSection.Alignments[j])
							{
								case ColumnAlignment.Left:
									tf.Alignment = XParagraphAlignment.Left;
									break;
								case ColumnAlignment.Center:
									tf.Alignment = XParagraphAlignment.Center;
									break;
								case ColumnAlignment.Right:
									tf.Alignment = XParagraphAlignment.Right;
									break;
							}
						}
						else
						{
							tf.Alignment = XParagraphAlignment.Left;
						}

						tf.DrawString(tableSection.Rows[i - 1][j], font, XBrushes.Black,
							new XRect(x + tableSection.Padding, y + tableSection.Padding, tableSection.Widths[j] - tableSection.Padding * 2, tableSection.LineHeight), format);
					}

					x += tableSection.Widths[j];
				}

				y += tableSection.LineHeight;
			}

			return y;
		}

		int RetrieveSectionHeight(PdfElement section)
		{
			if (section is PdfTextSection textSection)
			{
				return textSection.MarginTop + textSection.LineHeight * textSection.Texts.Count;
			}

			if (section is PdfTableSection tableSection)
			{
				return tableSection.MarginTop + tableSection.LineHeight * (tableSection.Rows.Count + 1);
			}

			return 0;
		}
	}
}
