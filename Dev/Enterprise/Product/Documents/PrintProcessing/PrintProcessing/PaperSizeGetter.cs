using System;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.PrintProcessing
{
	class PaperSizeGetter
	{
		public PaperSizeGetter(string printerName)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(printerName))
				{
					throw new ArgumentNullException(nameof(printerName), Res.GetString("ae242627-ddb0-41f8-ab34-bbb4bee4bd1d", "Printer name must not be empty."));
				}

				PrinterSettings printer = new PrinterSettings();
				printer.PrinterName = printerName;

				if (printer.IsValid)
				{
					PageSettings pageSettings = printer.DefaultPageSettings;
					if (pageSettings != null)
					{
						PopulatePaperSizeGetter(pageSettings);
					}
				}
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				HandlePaperSizeGetterException(exception, printerName);
			}
		}

		void PopulatePaperSizeGetter(PageSettings pageSettings)
		{
			PaperSize paperSize = null;

			try
			{
				paperSize = pageSettings.PaperSize;
#if DEBUG
				if (ThrowPaperSizeExceptionForTesting)
				{
					paperSize = null;
					throw new Exception("Failure To Launch");
				}
#endif
			}
			catch (Exception exception)
			{
				if (exception.IsCriticalException())
				{
					throw;
				}

				ErrorMessage = Res.GetString("13dedcd1-9a18-448e-a1a2-b7a2b8df4070", "Warning: Error occurred while trying to get the paper size for printer '{0}'.\r\nException Type: {1}\r\nException Message: {2}", pageSettings.PrinterSettings.PrinterName, exception.GetType().ToString(), exception.Message);
			}

			if (paperSize != null)
			{
				PaperName = paperSize.PaperName;
				Height = paperSize.Height;
				Width = paperSize.Width;
				IsValid = true;
			}
		}

		void HandlePaperSizeGetterException(Exception ex, string printerName)
		{
			Win32Exception win32Exception = ex as Win32Exception;
			if (win32Exception != null && win32Exception.NativeErrorCode == 0)
			{
				ErrorMessage = Res.GetString("6b4c1d28-87cc-4f72-808f-6ee5e2398425", "Failed to get paper settings for {0}", printerName);
			}
			else
			{
				ErrorReporter.ReportOnce("PrintQueueManager.PrinterPaperSettingsGetter", string.Format(CultureInfo.InvariantCulture, "{0} could not get the paper size for the printer '{1}'.", Core.Constants.ProductName, printerName), ex);
			}
		}

		public bool IsValid { get; private set; }
		public ZString PaperName { get; private set; }
		public int Height { get; private set; }
		public int Width { get; private set; }
		public string ErrorMessage { get; private set; }

#if DEBUG
		[ThreadStatic]
		internal static bool ThrowPaperSizeExceptionForTesting;
#endif
	}
}
