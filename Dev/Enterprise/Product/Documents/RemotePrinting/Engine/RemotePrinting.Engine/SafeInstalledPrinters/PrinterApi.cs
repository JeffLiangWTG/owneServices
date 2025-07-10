using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Enterprise.RemotePrinting.Engine
{
	[Flags]
	public enum EnumPrintersFlags : uint
	{
		/// <summary>
		/// If the PRINTER_ENUM_NAME flag is not also passed, the function ignores the Name parameter, and enumerates the locally installed printers. If PRINTER_ENUM_NAME is also passed, the function enumerates the local printers on Name. 
		/// </summary>
		PRINTER_ENUM_LOCAL = 0x02,

		/// <summary>
		/// The function enumerates the list of printers to which the user has made previous connections.
		/// </summary>
		PRINTER_ENUM_CONNECTIONS = 0x04,

		/// <summary>
		/// The function enumerates the printer identified by Name. This can be a server, a domain, or a print provider. If Name is NULL, the function enumerates available print providers. 
		/// </summary>
		PRINTER_ENUM_NAME = 0x08,

		/// <summary>
		/// The function enumerates network printers and print servers in the computer's domain. This value is valid only if Level is 1.
		/// </summary>
		PRINTER_ENUM_REMOTE = 0x10,

		/// <summary>
		/// The function enumerates printers that have the shared attribute. Cannot be used in isolation; use an OR operation to combine with another PRINTER_ENUM type.
		/// </summary>
		PRINTER_ENUM_SHARED = 0x20,

		/// <summary>
		/// The function enumerates network printers in the computer's domain. This value is valid only if Level is 1.
		/// </summary>
		PRINTER_ENUM_NETWORK = 0x40,
	}

	[Flags]
	public enum PrinterAttributes : uint
	{
		/// <summary>
		/// If set, the printer spools and starts printing after the last page is spooled. If not set and PRINTER_ATTRIBUTE_DIRECT is not set, the printer spools and prints while spooling.
		/// </summary>
		PRINTER_ATTRIBUTE_QUEUED = 0x00000001,

		/// <summary>
		/// Job is sent directly to the printer (it is not spooled).
		/// </summary>
		PRINTER_ATTRIBUTE_DIRECT = 0x00000002,

		/// <summary>
		/// Windows 95/98/Me: Indicates the printer is the default printer in the system.
		/// </summary>
		PRINTER_ATTRIBUTE_DEFAULT = 0x00000004,

		/// <summary>
		/// Printer is shared.
		/// </summary>
		PRINTER_ATTRIBUTE_SHARED = 0x00000008,

		/// <summary>
		/// Printer is a network printer connection.
		/// </summary>
		PRINTER_ATTRIBUTE_NETWORK = 0x00000010,

		/// <summary>
		/// Reserved.
		/// </summary>
		PRINTER_ATTRIBUTE_HIDDEN = 0x00000020,

		/// <summary>
		/// Printer is a local printer.
		/// </summary>
		PRINTER_ATTRIBUTE_LOCAL = 0x00000040,

		/// <summary>
		/// If set, DevQueryPrint is called. DevQueryPrint may fail if the document and printer setups do not match. Setting this flag causes mismatched documents to be held in the queue.
		/// </summary>
		PRINTER_ATTRIBUTE_ENABLE_DEVQ = 0x00000080,

		/// <summary>
		/// If set, jobs are kept after they are printed. If unset, jobs are deleted.
		/// </summary>
		PRINTER_ATTRIBUTE_KEEPPRINTEDJOBS = 0x00000100,

		/// <summary>
		/// If set and printer is set for print-while-spooling, any jobs that have completed spooling are scheduled to print before jobs that have not completed spooling. 
		/// </summary>
		PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST = 0x00000200,

		/// <summary>
		/// Windows 95/98/Me: Indicates whether the printer is currently connected. If the printer is not currently connected, print jobs will continue to spool.
		/// </summary>
		PRINTER_ATTRIBUTE_WORK_OFFLINE = 0x00000400,

		/// <summary>
		/// Indicates whether bi-directional communications are enabled for the printer.
		/// </summary>
		PRINTER_ATTRIBUTE_ENABLE_BIDI = 0x00000800,

		/// <summary>
		/// Indicates that only raw data type print jobs can be spooled.
		/// </summary>
		PRINTER_ATTRIBUTE_RAW_ONLY = 0x00001000,

		/// <summary>
		/// Windows 2000/XP: Indicates whether the printer is published in the directory service.
		/// </summary>
		PRINTER_ATTRIBUTE_PUBLISHED = 0x00002000,

		/// <summary>
		/// Windows XP: If set, printer is a fax printer. This can only be set by AddPrinter, but it can be retrieved by EnumPrinters and GetPrinter.
		/// </summary>
		PRINTER_ATTRIBUTE_FAX = 0x00004000,

		/// <summary>
		/// Windows Server 2003: Indicates the printer is currently connected through a terminal server.
		/// </summary>
		PRINTER_ATTRIBUTE_TS = 0x00008000,

		/// <summary>
		/// Windows Vista: The printer was was installed by using the Push Printer Connections user policy.
		/// </summary>
		PRINTER_ATTRIBUTE_PUSHED_USER = 0x00020000,

		/// <summary>
		/// Windows Vista: The printer was installed by using the Push Printer Connections computer policy. See Step-by-Step Guide for Print Management.
		/// </summary>
		PRINTER_ATTRIBUTE_PUSHED_MACHINE = 0x00040000,

		/// <summary>
		/// Windows Vista: Printer is a per-machine connection.
		/// </summary>
		PRINTER_ATTRIBUTE_MACHINE = 0x00080000,

		/// <summary>
		/// Windows Vista: A computer has connected to this printer and given it a friendly name.
		/// </summary>
		PRINTER_ATTRIBUTE_FRIENDLY_NAME = 0x00100000,
	}

	/// <summary>
	/// Specifies detailed printer information. 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct PRINTER_INFO_2
	{
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pServerName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pPrinterName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pShareName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pPortName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pDriverName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pComment;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pLocation;
		readonly IntPtr pDevMode; //LPDEVMODEW
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pSepFile;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pPrintProcessor;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pDatatype;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pParameters;
		readonly IntPtr pSecurityDescriptor; //PSECURITY_DESCRIPTOR
		public PrinterAttributes Attributes;
		public uint Priority;
		public uint DefaultPriority;
		public uint StartTime;
		public uint UntilTime;
		public uint Status;
		public uint cJobs;
		public uint AveragePPM;
	}

	/// <summary>
	/// Specifies general printer information.
	/// The structure can be used to retrieve minimal printer information on a call to EnumPrinters.
	/// Such a call is a fast and easy way to retrieve the names and attributes of all locally installed printers on a system and all remote printer connections that a user has established.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public struct PRINTER_INFO_4
	{
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pPrinterName;
		[MarshalAs(UnmanagedType.LPTStr)]
		public string pServerName;
		public PrinterAttributes Attributes;
	}

	public static class PrinterApi
	{
		public static T[] EnumPrinters<T>(EnumPrintersFlags flags) where T : struct
		{
			uint level;
			if (typeof(T).Equals(typeof(PRINTER_INFO_2)))
			{
				level = 2;
			}
			else if (typeof(T).Equals(typeof(PRINTER_INFO_4)))
			{
				level = 4;
			}
			else
			{
				throw new NotSupportedException(String.Format("Unknown Buffer Type '{0}'", typeof(T).Name));
			}

			T[] result = null;
			uint bytesNeeded;
			uint returned;
			if (!EnumPrinters(flags, null, level, IntPtr.Zero, 0, out bytesNeeded, out returned))
			{
				IntPtr resultBuf = Marshal.AllocHGlobal((int)bytesNeeded);
				if (EnumPrinters(flags, null, level, resultBuf, bytesNeeded, out bytesNeeded, out returned))
				{
					result = GetArrayFromIntPtr<T>(resultBuf, (int)returned);
					Marshal.FreeHGlobal(resultBuf);
				}
				else
				{
					Exception ex = new Win32Exception(Marshal.GetLastWin32Error());
					Marshal.FreeHGlobal(resultBuf);

					throw ex;
				}
			}

			if (result == null)
			{
				result = Array.Empty<T>();
			}

			return result;
		}

		[SuppressMessage("Microsoft.Contracts", "Nonnull-161-0")]
		public static T GetPrinter<T>(string printerName) where T : struct
		{
			uint level;
			if (typeof(T).Equals(typeof(PRINTER_INFO_2)))
			{
				level = 2;
			}
			else if (typeof(T).Equals(typeof(PRINTER_INFO_4)))
			{
				level = 4;
			}
			else
			{
				throw new NotSupportedException(String.Format("Unknown Buffer Type '{0}'", typeof(T).Name));
			}

			IntPtr printerHandle;
			if (OpenPrinter(printerName, out printerHandle, IntPtr.Zero))
			{
				uint bytesNeeded;
				GetPrinter(printerHandle, level, IntPtr.Zero, 0, out bytesNeeded);
				IntPtr resultBuf = Marshal.AllocHGlobal((int)bytesNeeded);
				if (GetPrinter(printerHandle, level, resultBuf, bytesNeeded, out bytesNeeded))
				{
					T result = (T)Marshal.PtrToStructure(resultBuf, typeof(T));
					Marshal.FreeHGlobal(resultBuf);
					ClosePrinter(printerHandle);

					return result;
				}
				else
				{
					Exception ex = new Win32Exception(Marshal.GetLastWin32Error());
					Marshal.FreeHGlobal(resultBuf);
					ClosePrinter(printerHandle);

					throw ex;
				}
			}
			else
			{
				throw new Win32Exception(Marshal.GetLastWin32Error());
			}
		}

		/// <summary>
		/// Enumerates available printers, print servers, domains, or print providers. 
		/// </summary>
		/// <param name="flags">printer object types</param>
		/// <param name="name">name of printer object</param>
		/// <param name="level">information level</param>
		/// <param name="pPrinterEnum">printer information buffer</param>
		/// <param name="cbBuf">size of printer information buffer</param>
		/// <param name="pcbNeeded">bytes received or required</param>
		/// <param name="pcReturned">number of printers enumerated</param>
		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool EnumPrinters(
			EnumPrintersFlags flags,
			string name,
			uint level,
			IntPtr pPrinterEnum,
			uint cbBuf,
			out uint pcbNeeded,
			out uint pcReturned);

		/// <summary>
		/// Retrieves a handle to the specified printer or print server or other types of handles in the print subsystem. 
		/// </summary>
		/// <param name="printerName">printer or server name</param>
		/// <param name="phPrinter">printer or server handle</param>
		/// <param name="pDefault">printer defaults</param>
		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool OpenPrinter(
			string printerName,
			out IntPtr phPrinter,
			IntPtr pDefault  //LPPRINTER_DEFAULTS
			);

		/// <summary>
		/// Closes the specified printer object.
		/// </summary>
		/// <param name="hPrinter">handle to printer object</param>
		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool ClosePrinter(IntPtr hPrinter);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="hPrinter"> handle to printer</param>
		/// <param name="level">information level</param>
		/// <param name="pPrinter">printer information buffer</param>
		/// <param name="cbBuf">size of buffer</param>
		/// <param name="pcbNeeded">bytes received or required</param>
		[DllImport("winspool.drv", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool GetPrinter(
			IntPtr hPrinter,
			uint level,
			IntPtr pPrinter,
			uint cbBuf,
			out uint pcbNeeded
			);

		[SuppressMessage("Microsoft.Contracts", "Nonnull-47-0")]
		static T[] GetArrayFromIntPtr<T>(IntPtr ptr, int length) where T : struct
		{
			T[] array = new T[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = (T)Marshal.PtrToStructure(GetElementAddress<T>(ptr, i), typeof(T));
			}

			return array;
		}
		[SuppressMessage("Microsoft.Contracts", "TestAlwaysEvaluatingToAConstant")]
		static IntPtr Add(IntPtr ptr, int value)
		{
			switch (IntPtr.Size)
			{
				case 4:
					return new IntPtr(ptr.ToInt32() + value);

				case 8:
					return new IntPtr(ptr.ToInt64() + value);

				default:
					throw new NotSupportedException(String.Format("Unknown IntPtr.Size = {0}", IntPtr.Size));
			}
		}

		static IntPtr GetElementAddress<T>(IntPtr ptr, int i)
		{
			return Add(ptr, Marshal.SizeOf(typeof(T)) * i);
		}
	}
}
