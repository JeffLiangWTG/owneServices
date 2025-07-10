using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Engine.Testing
{
	public class SafeInstalledPrintersTest : TestCase
	{
		public void TestPrinters()
		{
			// Real virtual printers

			AssertPrinter("Microsoft Office Document Image Writer", "Microsoft Document Imaging Writer Port:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: true, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("OneNote for Windows 10",
				"Microsoft.Office.OneNote_16001.13530.20492.0_x64__8wekyb3d8bbwe_microsoft.onenoteim_S-1-5-21-1944075937-2199493625-146292569-5114",
				PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL | PrinterAttributes.PRINTER_ATTRIBUTE_PUSHED_MACHINE,
				expectedIsValid: true, expectedIsSuspectedSurrogate: true, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("Send to OneNote",
				"Microsoft.SendtoOneNote_1.0.5.0_x86__8wekyb3d8bbwe_SendToOneNote_S-1-5-21-1944075937-2199493625-146292569-5114",
				PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL | PrinterAttributes.PRINTER_ATTRIBUTE_PUSHED_MACHINE,
				expectedIsValid: true, expectedIsSuspectedSurrogate: true, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("Microsoft Print to PDF", "PORTPROMPT:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL | PrinterAttributes.PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST,
				expectedIsValid: true, expectedIsSuspectedSurrogate: true, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("Microsoft XPS Document Writer", "PORTPROMPT:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL | PrinterAttributes.PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST,
				expectedIsValid: true, expectedIsSuspectedSurrogate: true, expectedInInstalledLocalPrinterNames: true);

			// Real virtual Fax printers

			AssertPrinter("Fax", "SHRFAX:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL | PrinterAttributes.PRINTER_ATTRIBUTE_FAX,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			AssertPrinter("Microsoft Fax", "COM1:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL | PrinterAttributes.PRINTER_ATTRIBUTE_FAX,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			// Real valid network printers

			AssertPrinter(@"\\SYDCO-SADS-2\SYDCO-C6004ex-74GroundFloor",
				"SYDCO-C6004ex-74GroundFloor",
				PrinterAttributes.PRINTER_ATTRIBUTE_SHARED | PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK | PrinterAttributes.PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST | PrinterAttributes.PRINTER_ATTRIBUTE_ENABLE_BIDI | PrinterAttributes.PRINTER_ATTRIBUTE_PUBLISHED,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter(@"\\SYDCO-SADS-2\SYDCO-C6004ex-MainArea",
				"10.61.162.24",
				PrinterAttributes.PRINTER_ATTRIBUTE_SHARED | PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK | PrinterAttributes.PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST | PrinterAttributes.PRINTER_ATTRIBUTE_ENABLE_BIDI | PrinterAttributes.PRINTER_ATTRIBUTE_PUBLISHED,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter(@"\\SYDCO-SADS-2\SYDCO-C6004ex-NorthArea",
				"SYDCO-C6004ex-NorthArea",
				PrinterAttributes.PRINTER_ATTRIBUTE_SHARED | PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK | PrinterAttributes.PRINTER_ATTRIBUTE_DO_COMPLETE_FIRST | PrinterAttributes.PRINTER_ATTRIBUTE_ENABLE_BIDI | PrinterAttributes.PRINTER_ATTRIBUTE_PUBLISHED,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			// Valid local printer examples 

			AssertPrinter("Local Physical Printer 1", "USB_21:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("Local Physical Printer 2", "USBPORT7:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("Local Physical Printer 3", "LPT", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("Local Physical Printer 4", "COMPORT", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			// Suspected virtual printer examples

			AssertPrinter("hello", "", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			AssertPrinter("My super virtual printer", "VIRTUALPORT:", PrinterAttributes.PRINTER_ATTRIBUTE_LOCAL,
				expectedIsValid: true, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: true);

			// Invalid printer connected via terminal service example

			AssertPrinter("HP LASERJET 4050", "IP_0.0.0.0", PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK | PrinterAttributes.PRINTER_ATTRIBUTE_TS,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			// Invalid remote printer examples
			// M.K: I don't know if we still need this - it was added in 2009 in WI00017769 and there are no notes, eDocs, or related incidents or issues

			AssertPrinter("HP LaserFoo (from OtherMachine)", "IP_0.0.0.0", PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			AssertPrinter("HP LaserFoo/OtherMachine/Session 1", "IP_0.0.0.0", PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			AssertPrinter(@"Client\Username\HP LaserFoo", "IP_0.0.0.0", PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			AssertPrinter("Client/DOMAIN-USER#/Printer)", "IP_0.0.0.0", PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);

			AssertPrinter("SALES - HP LASERJET 4050 SERIES PCL6 ON XC (FROM BP273) IN SESSION 0", "IP_0.0.0.0", PrinterAttributes.PRINTER_ATTRIBUTE_NETWORK,
				expectedIsValid: false, expectedIsSuspectedSurrogate: false, expectedInInstalledLocalPrinterNames: false);
		}

		void AssertPrinter(string printerName, string portName, PrinterAttributes attributes, bool expectedIsValid, bool expectedIsSuspectedSurrogate, bool expectedInInstalledLocalPrinterNames)
		{
			var safeInstalledPrinters = new SafeInstalledPrintersForTesting();
			safeInstalledPrinters.Add(printerName, portName, attributes);

			var printers = safeInstalledPrinters.Printers;
			AssertEquals("There should be ", 1, printers.Count);
			AssertEquals(printerName, printers[0].Name);
			AssertEquals($"{printerName}:{portName}.IsValid", expectedIsValid, printers[0].IsValid);
			AssertEquals($"{printerName}:{portName}.IsSuspectedSurrogate", expectedIsSuspectedSurrogate, printers[0].IsSuspectedSurrogate);

			var installedLocalPrinterNames = safeInstalledPrinters.InstalledLocalPrinterNamesCoreExposed.ToList();
			if (expectedInInstalledLocalPrinterNames)
			{
				AssertEquals($"Printer {printerName}:{portName} should be included in InstalledLocalPrinterNames", 1, installedLocalPrinterNames.Count);
				AssertEquals(printerName, installedLocalPrinterNames[0]);
			}
			else
			{
				AssertEquals($"Printer {printerName}:{portName} should not be included in InstalledLocalPrinterNames", 0, installedLocalPrinterNames.Count);
			}
		}

		public void TestIsValidPrinter()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting("TEST", "Blah"))
			{
				AssertEquals(true, SafeInstalledPrinters.IsValidPrinter("TEST"));
				AssertEquals(true, SafeInstalledPrinters.IsValidPrinter("Blah"));
				AssertEquals(false, SafeInstalledPrinters.IsValidPrinter("HP LaserFoo"));
			}
		}

		public void TestIsValidPrinter2()
		{
			AssertEquals(false, SafeInstalledPrinters.IsValidPrinter("HP LaserFoo"));
		}

		class SafeInstalledPrintersForTesting : SafeInstalledPrinters
		{
			public IEnumerable<string> InstalledLocalPrinterNamesCoreExposed => InstalledLocalPrinterNamesCore;

			public void Add(string printerName, string portName, PrinterAttributes attributes)
			{
				printers.Add(new PRINTER_INFO_2() { pPrinterName = printerName, pPortName = portName, Attributes = attributes });
			}

			protected override PRINTER_INFO_2[] EnumPrinters()
			{
				return printers.ToArray();
			}

			readonly List<PRINTER_INFO_2> printers = new List<PRINTER_INFO_2>();
		}
	}
}
