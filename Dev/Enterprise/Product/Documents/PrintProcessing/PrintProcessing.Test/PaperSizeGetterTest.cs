using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.RemotePrinting.Engine;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.PrintProcessing.Testing
{
	sealed class PaperSizeGetterTest : TestCaseWithFactory
	{
		[RequiresSoftware(RequiredSoftware.XpsPrinter)]
		public void TestPaperSizeGetter()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting(XPSPrinterName))
			{
				PaperSizeGetter paperSize = new PaperSizeGetter(XPSPrinterName);

				if (paperSize.PaperName == "A4")
				{
					AssertEquals("paperSize.Height for A4", 1169, paperSize.Height);
					AssertEquals("paperSize.Width for A4", 827, paperSize.Width);
				}
				else if (paperSize.PaperName == "Letter")
				{
					AssertEquals("paperSize.Height for Letter", 1100, paperSize.Height);
					AssertEquals("paperSize.Width for Letter", 850, paperSize.Width);
				}
				else
				{
					Fail("paperSize.PaperName should be either 'A4' or 'Letter' for '" + XPSPrinterName + "' on all Developer Machines. Was: [" + paperSize.PaperName + "]");
				}
			}
		}

		[RequiresSoftware(RequiredSoftware.XpsPrinter)]
		public void TestThrowExceptionsWhenPrinterNameIsEmpty()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting(XPSPrinterName))
			{
				PaperSizeGetter paperSizeGetter = new PaperSizeGetter("");
				Assert("ErrorReporter.LastExceptionReported should be ArgumentNullException", ErrorReporter.LastExceptionReported is ArgumentNullException);
				ErrorReporter.Clear();
			}
		}

		[RequiresSoftware(RequiredSoftware.XpsPrinter)]
		public void TestThrowNoExceptionWhenUnableToGetPaperSize()
		{
			using (SafeInstalledPrinters.OverridePrintersForTesting(XPSPrinterName))
			{
				PaperSizeGetter paperSize;
				try
				{
					PaperSizeGetter.ThrowPaperSizeExceptionForTesting = true;
					paperSize = new PaperSizeGetter(XPSPrinterName);
				}
				finally
				{
					PaperSizeGetter.ThrowPaperSizeExceptionForTesting = false;
				}

				AssertEquals("paperSize.IsValid", false, paperSize.IsValid);
				AssertMultilineASCIIEquals("paperSize.ErrorMessage", string.Format(
@"Warning: Error occurred while trying to get the paper size for printer '{0}'.
Exception Type: System.Exception
Exception Message: Failure To Launch", XPSPrinterName), paperSize.ErrorMessage);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupNotificationEmail();
			Assert(XPSPrinterName + " printer should exist on all machines running this test.", PrinterExists(XPSPrinterName));
		}

		bool PrinterExists(string name)
		{
			foreach (var printer in SafeInstalledPrinters.AllPrinters)
			{
				if (name.Equals(printer.Name, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}

		void SetupNotificationEmail()
		{
			GlbStaff staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, "PM"));
			staff.GS_EmailAddress = "example@example";
			Factory.Save();
		}

		const string XPSPrinterName = RemotePrinting.Engine.Testing.FlexCelPrinterTest.XPSPrinterName;
	}
}
