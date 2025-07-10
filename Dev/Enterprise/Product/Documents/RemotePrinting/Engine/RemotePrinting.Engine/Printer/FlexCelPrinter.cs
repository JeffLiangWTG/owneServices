using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using FlexCel.Core;
using FlexCel.Render;
using FlexCel.XlsAdapter;

namespace Enterprise.RemotePrinting.Engine
{
	/// <summary>
	/// This class replaces ExcelPrinter for a native printing using FlexCel. 
	/// </summary>
	/// <remarks>
	/// This class directly uses FlexCel methods instead of Enterprise's ExcelInterface
	/// because we don't have a reference to Enterprise.  
	/// </remarks>
	public class FlexCelPrinter : BasePrinterForXLSFile
	{
		public FlexCelPrinter(PrintEngineJob printJob)
			: base(printJob)
		{
			LineSpacing = Convert.ToDouble(printJob.LineSpacing);
			Watermark = printJob.Watermark;
		}

		readonly double LineSpacing;
		readonly Watermark Watermark;

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected override void ProcessAndPrintDocument()
		{
			Log("Loading Excel file into memory");
			using (var memoryStream = new MemoryStream(Contents))
			{
				var xls = new XlsFile { Linespacing = LineSpacing };
				xls.Open(memoryStream);
				xls.ActiveFileName = DocumentName;

				ModifyFileIfChangesRequired(xls);
				ApplyScalingSheet(xls);
				PrintFile(xls);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void ApplyScalingSheet(ExcelFile excelFile)
		{
			float xScale = 1;
			float yScale = 1;

			int scaleSheetIndex = excelFile.GetSheetIndex(Constants.FlexCelScalingSheetName, false);
			if (scaleSheetIndex > 0)
			{
				Log("Applying scaling sheet to Excel file");

				int currentActiveSheet = excelFile.ActiveSheet;
				try
				{
					excelFile.ActiveSheet = scaleSheetIndex;
					xScale = GetAndValidateScale(excelFile, 1, 2, "X", "B1");
					yScale = GetAndValidateScale(excelFile, 2, 2, "Y", "B2");
				}
				finally
				{
					excelFile.ActiveSheet = currentActiveSheet;
				}
			}

			excelFile.WidthCorrection *= 1 / xScale;
			excelFile.HeightCorrection *= 1 / yScale;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		float GetAndValidateScale(ExcelFile excelFile, int row, int column, string expectedScaleName, string expectedCell)
		{
			object value = excelFile.GetCellValue(row, column);
			if (!(value is double))
			{
				string message = string.Format("Error on {0} sheet. {1} Scale (Cell {2}) is not a number.", Constants.FlexCelScalingSheetName, expectedScaleName, expectedCell);
				throw new FlexCelException(message);
			}

			float scale = (float)(double)value; // the (float)(double) cast is important, don't change it!

			if (scale <= 0.5)
			{
				throw new FlexCelException(string.Format("Error on {0} sheet. Invalid {1} Scale: \"{2}\" . Scale must be larger than 0.5.", Constants.FlexCelScalingSheetName, expectedScaleName, scale));
			}
			return scale;
		}

		protected virtual IPrintDocument GetPrintDocument(ExcelFile excelFile)
		{
			return new FlexCelPrintDocumentCore(excelFile);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void PrintFile(ExcelFile excelFile)
		{
			Argument.NotNull(excelFile.PrintPaperDimensions, nameof(excelFile.PrintPaperDimensions));
			if (excelFile.SheetCount < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(excelFile));
			}

			short noOfCopies = (short)Copies;
			bool usePrinterCollation = noOfCopies > 1 && PrinterSupportsCollation();
			var sheetCount = excelFile.SheetCount;
			int?[] printScales = new int?[sheetCount];

			var copyPasses = usePrinterCollation ? 1 : Copies;
			for (int iCopy = 0; iCopy < copyPasses; iCopy++)
			{
				Log("Preparing Excel document to print" + (copyPasses > 1 ? (" - copy " + iCopy) : string.Empty));
				PrintFileWithRetry(excelFile, usePrinterCollation, noOfCopies, sheetCount, printScales);
			}
		}

		bool shouldRetry;
		const int maxRetry = 3;
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void PrintFileWithRetry(ExcelFile excelFile, bool usePrinterCollation, short noOfCopies, int sheetCount, int?[] printScales)
		{
			shouldRetry = false;
			for (var i = 1; i <= maxRetry; i++)
			{
				try
				{
					PrintFileCore(excelFile, usePrinterCollation, noOfCopies, sheetCount, printScales);
					return;
				}
				catch (Win32Exception ex)
				{
					if (!shouldRetry || i >= maxRetry)
					{
						Log(@$"System exception could not be handled. Exception messages: {ex.Message}
Retries enabled: {shouldRetry}, Retry attempted: {i}.");
						throw;
					}
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void PrintFileCore(ExcelFile excelFile, bool usePrinterCollation, short noOfCopies, int sheetCount, int?[] printScales)
		{
			using (IPrintDocument printDocument = GetPrintDocument(excelFile))
			{
				try
				{
					var noDialogController = new FlexCelStandardPrintController();
					noDialogController.RefreshPrinter += (sender, args) => shouldRetry = RefreshPrinter();
					printDocument.PrintController = noDialogController;
					printDocument.PrinterSettings.PrinterName = PrinterName;
					bool printerDefaultIsLetter = printDocument.DefaultPaperSize.Kind == PaperKind.Letter;
					IsRollPaper = IsRollPaper && excelFile.PrintPaperSize == TPaperSize.Undefined;

					if (usePrinterCollation)
					{
						printDocument.PrinterSettings.Copies = noOfCopies;
						printDocument.PrinterSettings.Collate = true;
					}
					printDocument.BeforePrintPage += BeforePrintPage;
					if (Watermark != null)
					{
						printDocument.PrintPage += PrintPage;
					}

					if (!IsRollPaper)
					{
						printDocument.GetPrinterHardMargins += GetPrinterHardMargins;
					}

					for (int iSheet = 1; iSheet <= sheetCount; iSheet++)
					{
						currentSheet = iSheet;

						int printScalesIndex = iSheet - 1;
						excelFile.ActiveSheet = iSheet;
						if (excelFile.SheetVisible == TXlsSheetVisible.Visible)
						{
							bool landscape = (excelFile.PrintOptions & TPrintOptions.Orientation) == 0;
							bool paperIsA4ButPrinterIsLetter = (excelFile.PrintPaperSize == TPaperSize.A4) && printerDefaultIsLetter;

							if (!paperIsA4ButPrinterIsLetter)
							{
								var paperSize = GetPaperSize(excelFile, printDocument);
								printDocument.DefaultPaperSize = paperSize;

								if (IsRollPaper)
								{
									printDocument.DefaultPageSettings.PaperSize = paperSize;
								}
							}

							if (!printScales[printScalesIndex].HasValue)
							{
								double a4LetterScalingFactor = 1;
								if (paperIsA4ButPrinterIsLetter)
								{
									// Special case - automatically resize A4 docs to Letter.
									// We keep the margins the same size and shrink the content to fit.
									// The scaling must be proportional so letterheads look correct.

									TXlsMargins margins = excelFile.GetPrintMargins();
									double marginSizeInches = landscape ? margins.Left + margins.Right : margins.Top + margins.Bottom;
									const double a4LongEdgeInches = 297D / 25.4D;
									const double letterLongEdgeInches = 11D;
									double printableRegionA4LongEdgeInches = a4LongEdgeInches - marginSizeInches;
									double printableRegionLetterLongEdgeInches = letterLongEdgeInches - marginSizeInches;
									a4LetterScalingFactor = printableRegionLetterLongEdgeInches / printableRegionA4LongEdgeInches;
								}
								printScales[printScalesIndex] = (int)(a4LetterScalingFactor * excelFile.PrintScale * (double)Scale / 100D);
							}

							excelFile.PrintScale = printScales[printScalesIndex].Value;
							printDocument.DefaultPageSettings.Landscape = landscape;
							printDocument.DocumentName = sheetCount == 1 ? excelFile.ActiveFileName : excelFile.ActiveFileName + " - Sheet " + excelFile.ActiveSheetByName;

							Log("Printing Excel document sheet " + iSheet);
							printDocument.Print();
						}
					}
				}
				finally
				{
					printDocument.BeforePrintPage -= BeforePrintPage;
					printDocument.PrintPage -= PrintPage;
					printDocument.GetPrinterHardMargins -= GetPrinterHardMargins;
				}
			}
		}

		#region Copied from Flexcel PrintPreview sample

		// Flexcel does not do P/Invoke because it is designed to be 100% managed code.
		// It provides a hook, namely the GetPrinterHardMargins event, to allow unmanaged applications
		// to use P/Invoke to get more precise margin information.

		[DllImport("gdi32.dll")]
		static extern int GetDeviceCaps(IntPtr hdc, int capindex);

		void GetPrinterHardMargins(object sender, PrintHardMarginsEventArgs e)
		{
			Argument.NotNull(e.Graphics, nameof(e.Graphics));
			const int PHYSICALOFFSETX = 112;
			const int PHYSICALOFFSETY = 113;

			double dpiX = e.Graphics.DpiX;
			double dpiY = e.Graphics.DpiY;

			IntPtr hdc = e.Graphics.GetHdc();
			try
			{
				e.XMargin = (float)(GetDeviceCaps(hdc, PHYSICALOFFSETX) * 100.0 / dpiX);
				e.YMargin = (float)(GetDeviceCaps(hdc, PHYSICALOFFSETY) * 100.0 / dpiY);
			}

			finally
			{
				e.Graphics.ReleaseHdc(hdc);
			}
		}

		#endregion

		void BeforePrintPage(object sender, PrintPageEventArgs e)
		{
			Argument.NotNull(e.Graphics, nameof(e.Graphics));
			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
		}

		int currentPage;
		int currentSheet;
		int previousSheet;
		const float ExcelUnitToPointRatio = 0.72f; // Page sizes are in 1/100th's of an inch, images are drawn in points or 1/72s of an inch.

		void PrintPage(object sender, PrintPageEventArgs e)
		{
			Argument.NotNull(e.Graphics, nameof(e.Graphics));

			if (previousSheet != currentSheet)
			{
				currentPage = 1;
				previousSheet = currentSheet;
			}
			else
			{
				currentPage++;
			}

			e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
			e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
			if (Watermark != null)
			{
				// This may change the rotation of the canvas. (It usually will - our watermarks are on a 45 degree angle
				// by default.) Therefore, it should be the last thing to happen in printing the page, and it can't go in the
				// BeforePrintPage event handler.
				e.Graphics.PageUnit = GraphicsUnit.Point;

				// For printers using paper roll the page height is variable.
				// The logic below is to calculate the current height of each page.
				// The watermark will be drawn on each logical page.
				if (IsRollPaper)
				{
					var fixedRollPaperWidth = e.PageBounds.Width * ExcelUnitToPointRatio;
					float currentPageHeight;
					if (e.PageBounds.Height <= e.PageSettings.PrintableArea.Height) // Custom page height is less than the default page height defined in the printer setting, we use the real page height.
					{
						currentPageHeight = e.PageBounds.Height * ExcelUnitToPointRatio;
					}
					else
					{
						currentPageHeight = e.HasMorePages
							? e.PageSettings.PrintableArea.Height * ExcelUnitToPointRatio // Not last page, we use the default height defined in the printer setting.
							: (e.PageBounds.Height - e.PageSettings.PrintableArea.Height * (currentPage - 1)) * ExcelUnitToPointRatio; // Last page, we calculate the real height.
					}

					Watermark.Draw(e.Graphics, new SizeF(fixedRollPaperWidth, currentPageHeight));
				}
				else
				{
					Watermark.Draw(e.Graphics, e.Graphics.VisibleClipBounds.Size);
				}
			}
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-169-0")]
		PaperSize GetPaperSize(ExcelFile excelFile, IPrintDocument document)
		{
			Argument.NotNull(excelFile, nameof(excelFile));
			Argument.NotNull(document, nameof(document));
			Argument.NotNull(excelFile.PrintPaperDimensions, nameof(excelFile.PrintPaperDimensions));
			TPaperDimensions t = excelFile.PrintPaperDimensions;
			foreach (PaperSize p in document.PrinterSettings.PaperSizes)
			{
				if (string.Compare(p.PaperName, t.PaperName, true) == 0 ||
					p.Width == Convert.ToInt32(t.Width) && p.Height == Convert.ToInt32(t.Height))
				{
					return p;
				}
			}

			//If no standard paper size was found, return a custom one.
			return new PaperSize(t.PaperName, Convert.ToInt32(t.Width), Convert.ToInt32(t.Height));
		}

		#region RefreshPrinter

		const int PRINTER_CONTROL_SET_STATUS = 4;
		const int PRINTER_STATUS_ERROR = 0x00000002;
		const int PRINTER_ACCESS_ADMINISTER = 0x00000004;

		[StructLayout(LayoutKind.Sequential)]
		public struct PRINTER_DEFAULTS
		{
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pDatatype;
			public IntPtr pDevMode;
			public int DesiredAccess;
		}

		[DllImport("winspool.drv", SetLastError = true, CharSet = CharSet.Auto)]
		static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault);
		protected virtual bool OpenPrinterCore(string pPrinterName, out IntPtr phPrinter, ref PRINTER_DEFAULTS pDefault) => OpenPrinter(pPrinterName, out phPrinter, ref pDefault);

		[DllImport("winspool.drv", SetLastError = true)]
		static extern bool SetPrinter(IntPtr hPrinter, int level, IntPtr pPrinter, int command);
		protected virtual bool SetPrinterCore(IntPtr hPrinter, int level, IntPtr pPrinter, int command) => SetPrinter(hPrinter, level, pPrinter, command);

		protected virtual bool ClosePrinterCore(IntPtr hPrinter) => ClosePrinter(hPrinter);

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		bool RefreshPrinter()
		{
			var errorMessage = string.Empty;
			if (PrinterName != null)
			{
				IntPtr printerHandle;
				var defaults = new PRINTER_DEFAULTS
				{
					pDatatype = null,
					pDevMode = IntPtr.Zero,
					DesiredAccess = PRINTER_ACCESS_ADMINISTER
				};

				if (OpenPrinterCore(PrinterName, out printerHandle, ref defaults))
				{
					try
					{
						IntPtr pStatus = Marshal.AllocHGlobal(sizeof(int));
						Marshal.WriteInt32(pStatus, PRINTER_STATUS_ERROR);

						if (SetPrinterCore(printerHandle, 0, pStatus, PRINTER_CONTROL_SET_STATUS))
						{
							Log($"The printer [{PrinterName}] has been refreshed.");
							return true;
						}
						else
						{
							var error = Marshal.GetLastWin32Error();
							errorMessage += $"Refresh printer [{PrinterName}] failed with error code: {error}";
						}

						Marshal.FreeHGlobal(pStatus);
					}
					finally
					{
						ClosePrinterCore(printerHandle);
					}
				}
				else
				{
					errorMessage += $"The printing job failed because the printer [{PrinterName}] could not be accessed.";
				}
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				Log(errorMessage);

				var subject = $"Printing failed on printer {PrinterName}";
				var body = @$"A system-level error occurred a document on printer {PrinterName}.
This error may indicate an issue with printer or its connection to print server machine.
Internal refreshing list of printers did not resolve this error.

Document which was being printed: {DocumentName}.
Error message: {errorMessage}

Please contact your administrator to check if the printer is working properly, and restart this WebPrint Client.
";
				SendNotificationEmailHelper.DailySendNotificationEmailOnce(subject, body);
			}

			return false;
		}

		#endregion

		#region FlexCelPrintDocumentCore

		sealed class FlexCelPrintDocumentCore : FlexCelPrintDocument, IPrintDocument
		{
			public FlexCelPrintDocumentCore(ExcelFile aWorkbook)
				: base(aWorkbook)
			{
			}

			#region IPrintDocument Members

			PaperSize IPrintDocument.DefaultPaperSize
			{
				get
				{
					return PrinterSettings.DefaultPageSettings.PaperSize;
				}
				set
				{
					PrinterSettings.DefaultPageSettings.PaperSize = value;
				}
			}

			#endregion
		}

		#endregion

		#region FlexCelStandardPrintController

		public class FlexCelStandardPrintController : StandardPrintController
		{
			public EventHandler RefreshPrinter;
			public override Graphics OnStartPage(PrintDocument document, PrintPageEventArgs e)
			{
				try
				{
#if DEBUG
					if (ExceptionForRefreshPrinterTest != null)
					{
						throw ExceptionForRefreshPrinterTest;
					}
#endif
					return base.OnStartPage(document, e);
				}
				catch (Win32Exception)
				{
					RefreshPrinter?.Invoke(this, EventArgs.Empty);

					throw;
				}
			}

#if DEBUG
			public Win32Exception ExceptionForRefreshPrinterTest { get; set; }
#endif
		}

		#endregion
	}
}
