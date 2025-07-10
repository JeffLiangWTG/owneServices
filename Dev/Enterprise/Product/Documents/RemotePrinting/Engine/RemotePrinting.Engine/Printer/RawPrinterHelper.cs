using System;
using System.Runtime.InteropServices;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	public static class RawPrinterHelper
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public class DOCINFOA
		{
			[MarshalAs(UnmanagedType.LPStr)]
			public string pDocName;
			[MarshalAs(UnmanagedType.LPStr)]
			public string pOutputFile;
			[MarshalAs(UnmanagedType.LPStr)]
			public string pDataType;
		}
		[DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

		[DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool ClosePrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool StartDocPrinter(IntPtr hPrinter, int level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

		[DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool EndDocPrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool StartPagePrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool EndPagePrinter(IntPtr hPrinter);

		[DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
		public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, int dwCount, out int dwWritten);

		public static bool SendBytesToPrinter(string szPrinterName, byte[] bytes)
		{
			Argument.NotNull(szPrinterName, nameof(szPrinterName));
			Argument.NotNull(bytes, nameof(bytes));

			bool bSuccess = false;

			var nLength = bytes.Length;
			if (nLength > 0)
			{
				var pUnmanagedBytes = Marshal.AllocCoTaskMem(nLength);

				try
				{
					if (pUnmanagedBytes != IntPtr.Zero)
					{
						Marshal.Copy(bytes, 0, pUnmanagedBytes, nLength);
						bSuccess = SendBytesToPrinter(szPrinterName, pUnmanagedBytes, nLength);
					}
				}
				finally
				{
					Marshal.FreeCoTaskMem(pUnmanagedBytes);
				}
			}

			return bSuccess;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, int dwCount)
		{
			var bSuccess = false;

			if (OpenPrinter(szPrinterName.Normalize(), out var hPrinter, IntPtr.Zero))
			{
				try
				{
					var di = new DOCINFOA();
					di.pDocName = "CargoWise One RAW Document";
					di.pDataType = "RAW";

					if (StartDocPrinter(hPrinter, 1, di))
					{
						if (StartPagePrinter(hPrinter))
						{
							bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out _);
							EndPagePrinter(hPrinter);
						}
						EndDocPrinter(hPrinter);
					}
				}
				finally
				{
					ClosePrinter(hPrinter);
				}
			}

			if (!bSuccess)
			{
				var dwError = Marshal.GetLastWin32Error();
			}

			return bSuccess;
		}
	}
}
