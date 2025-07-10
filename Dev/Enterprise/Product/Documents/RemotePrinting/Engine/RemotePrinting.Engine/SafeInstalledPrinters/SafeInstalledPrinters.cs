using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace Enterprise.RemotePrinting.Engine
{
	public class SafeInstalledPrinters
	{
		/// <summary>
		/// All explicitly installed printer names (i.e. not mapped from citrix/ts/rdp), excluding the Microsoft Office Document Image Writer, etc
		/// </summary>
		public static List<string> InstalledLocalPrinterNames => new SafeInstalledPrinters().InstalledLocalPrinterNamesCore.ToList();

		protected IEnumerable<string> InstalledLocalPrinterNamesCore => InstalledLocalPrintersCore.Select(printer => printer.Name);

		public static List<PrinterInfo> InstalledLocalPrinters => new SafeInstalledPrinters().InstalledLocalPrintersCore.ToList();

		protected IEnumerable<PrinterInfo> InstalledLocalPrintersCore => RemoveDuplicatePrinters(Printers.Where(printer => printer.IsValid));

		static IEnumerable<PrinterInfo> RemoveDuplicatePrinters(IEnumerable<PrinterInfo> printers)
		{
			var uniquePrinterNames = new HashSet<string>();
			foreach (var printer in printers)
			{
				if (!uniquePrinterNames.Contains(printer.Name))
				{
					uniquePrinterNames.Add(printer.Name);
					yield return printer;
				}
			}
		}

		/// <summary>
		/// Return all local and mapped printers
		/// </summary>
		[SuppressMessage("Microsoft.Contracts", "RequiresAtCall-input != null")]
		[SuppressMessage("Microsoft.Contracts", "Requires-13-93")]
		public static ReadOnlyCollection<PrinterInfo> AllPrinters
		{
			get
			{
				return new SafeInstalledPrinters().Printers.AsReadOnly();
			}
		}

		public static bool IsValidPrinter(string printerName)
		{
			Argument.NotNull(printerName, nameof(printerName));
#if DEBUG
			if (mockResultsForTesting != null)
			{
				return mockResultsForTesting.FirstOrDefault(mockResult => mockResult.Name.Equals(printerName, StringComparison.OrdinalIgnoreCase))?.IsValid ?? false;
			}
#endif
			try
			{
				return new SafeInstalledPrinters().IsValidPrinter(PrinterApi.GetPrinter<PRINTER_INFO_2>(printerName));
			}
			catch (Win32Exception ex)
			{
				if (ex.NativeErrorCode == 1801) //ERROR_INVALID_PRINTER_NAME
				{
					return false;
				}
				else
				{
					throw;
				}
			}
		}

		public List<PrinterInfo> Printers
		{
			get
			{
#if DEBUG
				if (mockResultsForTesting != null)
				{
					return mockResultsForTesting;
				}
#endif

				List<PrinterInfo> result = new List<PrinterInfo>();
				foreach (PRINTER_INFO_2 printerInfo in EnumPrinters())
				{
					result.Add(new PrinterInfo(printerInfo.pPrinterName, IsValidPrinter(printerInfo), IsSuspectedSurrogate(printerInfo)));
				}

				return result;
			}
		}

		protected virtual PRINTER_INFO_2[] EnumPrinters()
		{
			return PrinterApi.EnumPrinters<PRINTER_INFO_2>(EnumPrintersFlags.PRINTER_ENUM_LOCAL | EnumPrintersFlags.PRINTER_ENUM_CONNECTIONS);
		}

		bool IsValidPrinter(PRINTER_INFO_2 printerInfo)
		{
			Argument.NotNull(printerInfo.pPrinterName, nameof(printerInfo.pPrinterName));

			bool invalid = (printerInfo.Attributes & PrinterAttributes.PRINTER_ATTRIBUTE_FAX) != 0 ||
						(printerInfo.Attributes & PrinterAttributes.PRINTER_ATTRIBUTE_TS) != 0 ||
						isRemotePrinterRegex.IsMatch(printerInfo.pPrinterName);

			return !invalid;
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		static bool IsKnownSurrogatePort(string portName)
		{
			string[] knownSurrogatePorts =
			{
				"Microsoft Document Imaging Writer Port:",
				"XPSPort:",
				"Send To Microsoft OneNote Port:",
				"PORTPROMPT:"
			};

			string[] knownSurrogatePortsPrefixes =
			{
				"Microsoft.Office.OneNote_",
				"Microsoft.SendtoOneNote_"
			};

			return knownSurrogatePorts.Any(p => portName.Equals(p, StringComparison.OrdinalIgnoreCase))
				|| knownSurrogatePortsPrefixes.Any(p => portName.StartsWith(p, StringComparison.OrdinalIgnoreCase));
		}

		// Disabled because it picks remote printers set up as 'Local printer' via TCP/IP port.
		// This logic should be refined and tested before re-enabling
		//
		//bool IsSuspectedSurrogateCore(PRINTER_INFO_2 printerInfo)
		//{
		//	string[] realLocalPorts = { "LPT", "COM", "USB" };
		//
		//	bool isSuspectedSurrogate = (printerInfo.Attributes & PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL) != 0 &&
		//		!Array.Exists(realLocalPorts, port => printerInfo.pPortName.StartsWith(port, StringComparison.OrdinalIgnoreCase));
		//
		//	return isSuspectedSurrogate || IsKnownSurrogatePort(printerInfo.pPortName);
		//}

		bool IsSuspectedSurrogate(PRINTER_INFO_2 printerInfo)
		{
			//return IsSuspectedSurrogateCore(printerInfo) || IsKnownSurrogatePort(printerInfo.pPortName);
			return IsKnownSurrogatePort(printerInfo.pPortName);
		}

		readonly Regex isRemotePrinterRegex = new Regex(@"
			(
				\(from\ .*\)	# WinXP Remote Desktop, e.g. 'HP LaserJet (from ComputerName) in session 42'
			|
				/.*/Session\ \d+$	# Win2K Terminal Services, e.g. 'HP LaserJet/ComputerName/Session 3'
			|
				^Client\\.*\\	# Citrix, e.g. 'Client\Username\HP LaserJet'
			|
				^Client/.*/	# Citrix, e.g. 'Client/domain-user/Printer'
			)
		", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase | RegexOptions.Compiled);

		#region Mock
#if DEBUG

		static List<PrinterInfo> mockResultsForTesting;

		public static IDisposable OverridePrintersForTesting(params string[] printerNames)
		{
			return OverridePrintersForTesting(printerNames.Select(name => new PrinterInfo(name, true, false)));
		}

		public static IDisposable OverridePrintersForTesting(params PrinterInfo[] printers)
		{
			return OverridePrintersForTesting((IEnumerable<PrinterInfo>)printers);
		}

		public static IDisposable OverridePrintersForTesting(IEnumerable<PrinterInfo> printers)
		{
			mockResultsForTesting = printers.ToList();
			return new DisposableAction(() =>
			{
				mockResultsForTesting = null;
			});
		}

#endif
		#endregion
	}
}
