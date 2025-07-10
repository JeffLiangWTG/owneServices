using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Printing;

namespace CargoWise.PdfiumWrapper
{
	/// <summary>
	/// This is heavily based off the PdfiumViewer implementation, which is released under the Apache 2.0 license (meaning we can freely modify)
	/// </summary>
	internal class PdfPrintDocument : PrintDocument
	{
		readonly PdfDocument document;
		int currentPage;

		public PdfPrintDocument(PdfDocument document)
		{
			this.document = Argument.NotNull(document, nameof(document));
		}

		protected override void OnBeginPrint(PrintEventArgs e)
		{
			currentPage = Math.Max(0, PrinterSettings.FromPage - 1);

			base.OnBeginPrint(e);
		}

		protected override void OnQueryPageSettings(QueryPageSettingsEventArgs e)
		{
			// Some printers misreport landscape. The below check verifies
			// whether the page rotation matches the landscape setting.
			bool inverseLandscape = e.PageSettings.Bounds.Width > e.PageSettings.Bounds.Height != e.PageSettings.Landscape;

			if (currentPage < document.PageCount)
			{
				bool landscape = GetOrientation(document.GetPageSizeInPoints(currentPage)) == Orientation.Landscape;

				if (inverseLandscape)
				{
					landscape = !landscape;
				}

				e.PageSettings.Landscape = landscape;
			}

			base.OnQueryPageSettings(e);
		}

		protected override void OnPrintPage(PrintPageEventArgs e)
		{
			PrintSinglePage(e);

			base.OnPrintPage(e);
		}

		void PrintSinglePage(PrintPageEventArgs e)
		{
			if (currentPage < document.PageCount)
			{
				var pageOrientation = GetOrientation(document.GetPageSizeInPoints(currentPage));
				var printOrientation = GetOrientation(e.PageBounds.Size);

				e.PageSettings.Landscape = pageOrientation == Orientation.Landscape;

				double left = -e.PageSettings.HardMarginX;
				double top = -e.PageSettings.HardMarginY;
				double width = e.PageBounds.Width;
				double height = e.PageBounds.Height;

				if (pageOrientation != printOrientation)
				{
					Swap(ref height, ref width);
					Swap(ref left, ref top);
				}

				RenderPage(e, currentPage, left, top, width, height);
				currentPage++;
			}

			int pageCount = PrinterSettings.ToPage == 0
				? document.PageCount
				: Math.Min(PrinterSettings.ToPage, document.PageCount);

			e.HasMorePages = currentPage < pageCount;
		}

		[SuppressMessage("CargoWiseOne", "CW1017", Justification = "This is a printer, not a screen. DPI has been considered already.")]
		void RenderPage(PrintPageEventArgs e, int page, double left, double top, double width, double height)
		{
			var size = document.GetPageSizeInPoints(page);

			double pageScale = size.Height / size.Width;
			double printScale = height / width;

			double scaledWidth = width;
			double scaledHeight = height;

			if (pageScale > printScale)
			{
				scaledWidth = width * (printScale / pageScale);
			}
			else
			{
				scaledHeight = height * (pageScale / printScale);
			}

			left += (width - scaledWidth) / 2;
			top += (height - scaledHeight) / 2;

			document.RenderPage(
				e.Graphics,
				page,
				new Rectangle(
					AdjustDpi(e.Graphics.DpiX, left),
					AdjustDpi(e.Graphics.DpiY, top),
					AdjustDpi(e.Graphics.DpiX, scaledWidth),
					AdjustDpi(e.Graphics.DpiY, scaledHeight)
				),
				fillBackgroundWithWhite: false
			);
		}

		static void Swap(ref double a, ref double b)
		{
			var tmp = a;
			a = b;
			b = tmp;
		}

		static int AdjustDpi(double value, double dpi)
		{
			return (int)((value / 100.0) * dpi);
		}

		Orientation GetOrientation(SizeF pageSize)
		{
			if (pageSize.Height > pageSize.Width)
			{
				return Orientation.Portrait;
			}
			return Orientation.Landscape;
		}

		enum Orientation
		{
			Portrait,
			Landscape
		}
	}
}