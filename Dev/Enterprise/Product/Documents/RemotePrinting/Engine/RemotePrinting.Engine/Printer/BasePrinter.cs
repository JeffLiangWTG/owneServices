using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.RemotePrinting.Types;

namespace Enterprise.RemotePrinting.Engine
{
	public abstract class BasePrinter : IDisposable
	{
		protected BasePrinter(PrintEngineJob printJob)
		{
			Argument.NotNull(printJob, nameof(printJob)); // Suggested By ReviewBot
			PrinterName = Argument.NotNullOrEmpty(printJob.PrinterName, nameof(printJob.PrinterName));
			Contents = printJob.Contents;
			DocumentName = printJob.DocumentName;
			this.Copies = printJob.Copies;
			this.Scale = printJob.Scale;
			this.EscapeSequence = printJob.EscapeSequence;
			this.IsRollPaper = printJob.IsRollPaper;
		}

		protected byte[] Contents { get; }
		protected string DocumentName { get; }

		public event EventHandler<LogEventArgs> Logged;

		protected void Log(string log)
		{
			Logged?.Invoke(this, new LogEventArgs(log));
		}

		public void Print()
		{
			ProcessAndPrintDocument();
			PrintTrailingEscapeSequence();
		}

		protected abstract void ProcessAndPrintDocument();

		void IDisposable.Dispose()
		{
			Dispose(true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void Dispose(bool isDisposing)
		{
			Log("Disposing printer");
		}

		~BasePrinter()
		{
			Dispose(false);
		}

		#region Trailing Escape Sequence

		// Based on http://msdn.microsoft.com/library/default.asp?url=/library/en-us/gdi/prntspol_93g2.asp 
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void PrintTrailingEscapeSequence()
		{
			if (EscapeSequence.Length > 0)
			{
				Log("Printing trailing escape sequences");
				PrintTrailingEscapeSequenceCore();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		protected virtual void PrintTrailingEscapeSequenceCore()
		{
			IntPtr printerHandle;
			DOC_INFO_1 docInfo;
			uint bytesWritten;

			//get printer handle
			if (!OpenPrinter(PrinterName, out printerHandle, IntPtr.Zero))
			{
				throw new Exception("OpenPrinter failed", new Win32Exception());
			}

			try
			{
				//fill in docinfo
				docInfo = new DOC_INFO_1();
				docInfo.pDocName = "Trailing Escape Sequence";
				docInfo.pOutputFile = null;
				docInfo.pDatatype = "RAW";

				//inform the spooler that the document is beginning

				if (StartDocPrinter(printerHandle, 1, ref docInfo) == 0)
				{
					Win32Exception startDocPrinterFailed = new Win32Exception();
					throw new Exception("StartDocPrinter failed", startDocPrinterFailed);
				}

				// Start a page
				if (!StartPagePrinter(printerHandle))
				{
					EndDocPrinter(printerHandle);
					Win32Exception startPagePrinterFailed = new Win32Exception();
					throw new Exception("StartPagePrinter failed", startPagePrinterFailed);
				}

				if (!WritePrinter(printerHandle, EscapeSequence, (uint)EscapeSequence.Length, out bytesWritten))
				{
					EndPagePrinter(printerHandle);
					EndDocPrinter(printerHandle);
					Win32Exception writePrinterFailed = new Win32Exception();
					throw new Exception("WritePrinter failed", writePrinterFailed);
				}

				if (!EndPagePrinter(printerHandle))
				{
					EndDocPrinter(printerHandle);
					Win32Exception endPagePrinterFailed = new Win32Exception();
					throw new Exception("EndPagePrinter failed", endPagePrinterFailed);
				}

				if (!EndDocPrinter(printerHandle))
				{
					Win32Exception endDocPrinterFailed = new Win32Exception();
					throw new Exception("EndDocPrinter failed", endDocPrinterFailed);
				}
			}
			finally
			{
				ClosePrinter(printerHandle);
			}

			if (bytesWritten != EscapeSequence.Length)
			{
				throw new Exception(string.Format("Bytes written ({0}) not equal to job length ({1})", bytesWritten, EscapeSequence.Length));
			}
		}

		#region WinSpool wrappers

		[DllImport("winspool.drv", CharSet = CharSet.Auto)]
		static extern bool OpenPrinter(string pPrinterName, out IntPtr phPrinter, IntPtr @null);

		[StructLayout(LayoutKind.Sequential)]
		struct DOC_INFO_1
		{
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pDocName;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pOutputFile;
			[MarshalAs(UnmanagedType.LPTStr)]
			public string pDatatype;
		}

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern uint StartDocPrinter(
			IntPtr hPrinter,  // handle to printer object
			uint level,      // information level
			ref DOC_INFO_1 pDocInfo   // information buffer
			);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		protected static extern bool ClosePrinter(
			IntPtr hPrinter   // handle to printer object
			);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool StartPagePrinter(
			IntPtr hPrinter   // handle to printer object
			);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool EndPagePrinter(
			IntPtr hPrinter   // handle to printer object
			);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool EndDocPrinter(
			IntPtr hPrinter   // handle to printer object
			);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool WritePrinter(
			IntPtr hPrinter,   // handle to printer object
			byte[] pBuf,       // array of printer data
			uint cbBuf,       // size of array
			out uint pcWritten  // bytes received
			);

		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool GetPrinter(IntPtr hPrinter, int level, IntPtr printer, int buf, out int needed);

		#endregion

		#endregion

		protected virtual bool PrinterSupportsCollation()
		{
			// Some printer drivers seems to have bugs when reporting whether they support collation
			// with DeviceCapabilities - e.g. they return true but they don't actually collate. DEVMODE
			// seems more reliable.
			bool result = false;
			try
			{
				if (OpenPrinter(PrinterName, out var hPrinter, IntPtr.Zero))
				{
					IntPtr buffer = IntPtr.Zero;
					try
					{
						GetPrinter(hPrinter, 2, IntPtr.Zero, 0, out var bytesNeeded);
						buffer = Marshal.AllocCoTaskMem(bytesNeeded);
						if (GetPrinter(hPrinter, 2, buffer, bytesNeeded, out bytesNeeded))
						{
							var devModePtr = Marshal.ReadIntPtr(buffer, IntPtr.Size * 7);
							const int CCHDEVICENAME = 32;
							const int BCHAR_SIZE = 2;
							const int WORD_SIZE = 2;
							uint devModeFields = (uint)Marshal.ReadInt32(devModePtr, CCHDEVICENAME * BCHAR_SIZE + 4 * WORD_SIZE);

							const uint DM_COLLATE = 0x00008000;
							result = (devModeFields & DM_COLLATE) == DM_COLLATE;
						}
					}
					finally
					{
						if (buffer != IntPtr.Zero)
						{
							Marshal.FreeCoTaskMem(buffer);
						}
						ClosePrinter(hPrinter);
					}
				}
			}
			catch
			{
				// Result stays false. In some cases (e.g. sometimes on Vista), it will throw an access violation exception when ReadIn32, 
				// but we cant reproduce it here and dont know how to fix it properly without lots of work, and given that this happens in a very small number of
				// cases and that by not using the collation, it works around the problem anyway, then I've decided to just eat this exception here, and 
				// then assume collation is not supported.  (besides, the collation feature is to make the system run faster by sending less files to printer, so it's just
				// an optimisation that wont run in small number of cases)  Henry.
			}

			return result;
		}

		protected readonly int Copies; //= 1
		protected readonly string PrinterName;
		protected readonly decimal Scale; //= 100
		protected readonly byte[] EscapeSequence;
		protected bool IsRollPaper;
	}
}
