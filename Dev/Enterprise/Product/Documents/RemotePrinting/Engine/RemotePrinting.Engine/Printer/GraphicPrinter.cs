using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.IO;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	/// <summary>
	/// Prints an image file. Good for printing one file only. If you would like to print multiple files, instantiate 
	/// a GraphicPrinter for each file.
	/// </summary>
	public class GraphicPrinter : BasePrinter
	{
		public GraphicPrinter(PrintEngineJob printJob)
			: base(printJob)
		{
		}

		protected override void ProcessAndPrintDocument()
		{
			using (var document = GetPrintDocument())
			{
				try
				{
					Print(document);
				}
				finally
				{
					UnhookPrintDocument(document);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual PrintDocument GetPrintDocument()
		{
			Log("Preparing document to print");

			short nbOfCopies = (short)Copies;

			var document = new PrintDocument();
			document.PrintPage += FormatGraphicPageToPrint;
			document.QueryPageSettings += QueryPageSettingsForOrientation;
			document.PrinterSettings.PrinterName = PrinterName;
			document.PrinterSettings.Copies = nbOfCopies;
			if (nbOfCopies > 1 && PrinterSupportsCollation())
			{
				document.PrinterSettings.Collate = true;
			}
			document.DocumentName = DocumentName;
			document.DefaultPageSettings.Margins = new Margins(25, 25, 25, 25);
			return document;
		}

		void UnhookPrintDocument(PrintDocument document)
		{
			document.PrintPage -= FormatGraphicPageToPrint;
			document.QueryPageSettings -= QueryPageSettingsForOrientation;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void Print(PrintDocument document)
		{
			Log("Printing image document");
			document.Print();
		}

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				ClearOldImage();
			}

			base.Dispose(isDisposing);
		}

		#region Implementation

		/// <summary>
		/// The whole Image file to print. Each page is printed separately.
		/// The pages are changed in QueryPageSettingsForOrientation
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public Image ImageToPrint
		{
			get
			{
				if (imageToPrint == null)
				{
					if (Contents.Length > 0)
					{
						ClearOldImage(); // Just in case image or stream somehow is not null

						Log("Loading image document content");
						imageStream = new MemoryStream(Contents);
						imageToPrint = Image.FromStream(imageStream);
					}
					else
					{
						throw new InvalidOperationException("imageToPrint is null.");
					}
				}
				return imageToPrint;
			}
		}

		void ClearOldImage()
		{
			imageToPrint?.Dispose();
			imageToPrint = null;

			imageStream?.Dispose();
			imageStream = null;
		}

		Stream imageStream;
		Image imageToPrint;

		/// <summary>
		/// This event is raised for each page that gets printed.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void FormatGraphicPageToPrint(object sender, PrintPageEventArgs pagePrinted)
		{
			Argument.NotNull(pagePrinted.PageSettings, nameof(pagePrinted.PageSettings)); // Suggested By ReviewBot 
			Graphics pagePrintedGraphics = pagePrinted.Graphics;
			Rectangle newMargins = AdjustMargins(pagePrinted);

			CurrentPagePrinting++;
			pagePrinted.HasMorePages = (CurrentPagePrinting != TotalPagesToPrint);

			Log("Rendering document page " + CurrentPagePrinting);
			pagePrintedGraphics.DrawImage(ImageToPrint, newMargins);
		}

		/// <summary>
		/// Sets Landscape/Portrait page orientation.
		/// Page orientation needs to be set BEFORE the PrintPage() method is called.
		/// </summary>
		void QueryPageSettingsForOrientation(object sender, QueryPageSettingsEventArgs pageSettingsResult)
		{
			Argument.NotNull(pageSettingsResult.PageSettings, nameof(pageSettingsResult.PageSettings));
			ImageToPrint.SelectActiveFrame(FrameDimension.Page, CurrentPagePrinting);
			pageSettingsResult.PageSettings.Landscape = IsA4LandscapePage;
		}

		/// <summary>
		/// Whether the page is an A4 page and in landscape.
		/// For simplicity, non A4 page sizes are always treated as portrait orientation.
		/// </summary>
		public bool IsA4LandscapePage
		{
			get
			{
				return IsA4Page && (ImageToPrint.Width > ImageToPrint.Height);
			}
		}

		/// <summary>
		/// Is the current page an A4 page?
		/// </summary>
		bool IsA4Page
		{
			get
			{
				var widthToHeightRatio = ImageToPrint.Width / (double)ImageToPrint.Height;
				return widthToHeightRatio > 0.6 && widthToHeightRatio < 0.8 || widthToHeightRatio > 1.3 && widthToHeightRatio < 1.5;
			}
		}

		public int TotalPagesToPrint
		{
			get { return ImageToPrint.GetFrameCount(FrameDimension.Page); }
		}

		/// <summary>
		/// Adjust the print margins of the current page to suit the image
		/// dimensions and the print queue percentage ratio
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		Rectangle AdjustMargins(PrintPageEventArgs pagePrinted)
		{
			Log("Adjusting page margins");

			Argument.NotNull(pagePrinted, nameof(pagePrinted));
			Argument.NotNull(pagePrinted.PageSettings, nameof(pagePrinted.PageSettings));
			var currentMargins = pagePrinted.MarginBounds;
			currentMargins = ScaleMarginsToImageDimensions(currentMargins, pagePrinted.PageSettings.HardMarginX, pagePrinted.PageSettings.HardMarginY);
			currentMargins = ScaleMarginsToPrintQueueZoom(currentMargins);
			return currentMargins;
		}

		public Rectangle ScaleMarginsToImageDimensions(Rectangle currentMargins, float hardMarginX, float hardMarginY)
		{
			var scaledMargins = new Rectangle(currentMargins.Location, currentMargins.Size);

			var maxMarginWidth = currentMargins.Width - 2 * hardMarginX;
			var maxMarginHeight = currentMargins.Height - 2 * hardMarginY;

			var ratioWidth = (double)maxMarginWidth / ImageToPrint.Width;
			var ratioHeight = (double)maxMarginHeight / ImageToPrint.Height;

			double ratio = Math.Min(ratioWidth, ratioHeight);
			if (ratio > 1)
			{
				ratio = 1;
			}

			scaledMargins.Width = (int)Math.Round(ImageToPrint.Width * ratio, 0);
			scaledMargins.Height = (int)Math.Round(ImageToPrint.Height * ratio, 0);

			return scaledMargins;
		}

		public Rectangle ScaleMarginsToPrintQueueZoom(Rectangle currentScale)
		{
			return (Scale == 100) ? currentScale : new Rectangle(currentScale.Location, GetNewDimensions(currentScale.Size));
		}

		public Size GetNewDimensions(Size oldSize)
		{
			return new Size(ReScaleToZoom(oldSize.Width), ReScaleToZoom(oldSize.Height));
		}

		public int ReScaleToZoom(int oldValue)
		{
			var newScale = Scale / 100m;
			return (int)Math.Round(oldValue * newScale, 0);
		}

		int CurrentPagePrinting;

		#endregion
	}
}
