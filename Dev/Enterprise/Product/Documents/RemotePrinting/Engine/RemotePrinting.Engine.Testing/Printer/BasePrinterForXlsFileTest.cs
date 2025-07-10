using CargoWise.IO;
using Enterprise.RemotePrinting.Engine.Testing;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.RemotePrinting.Engine
{
	public abstract class BasePrinterForXLSFileTest : BasePrinterTestCase
	{
		public void TestModifyFile_RemoveCompanyLogo()
		{
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.SuppressLetterhead = true;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);

				AssertEquals("Test file should have an image named CompanyLogo", true, BasePrinterForXLSFile.ObjectExists(xls, "CompanyLogo"));
				AssertEquals("Test file should have an image named BranchLogo", true, BasePrinterForXLSFile.ObjectExists(xls, "BranchLogo"));

				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("CompanyLogo should have been deleted by now!", false, BasePrinterForXLSFile.ObjectExists(xls, "CompanyLogo"));
				AssertEquals("BranchLogo should have been deleted by now!", false, BasePrinterForXLSFile.ObjectExists(xls, "BranchLogo"));
			}
		}

		public void TestModifyFile_Margins()
		{
			int marginOffset = 50;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.TopMargin = marginOffset;
			queue.LeftMargin = marginOffset;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);

				int originalRowHeight = xls.GetRowHeight(1);
				int originalColWidth = xls.GetColWidth(2);

				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("RowHeight should have changed", originalRowHeight + marginOffset, xls.GetRowHeight(1));
				AssertEquals("ColWidth should have changed", originalColWidth + marginOffset, xls.GetColWidth(2));
			}
		}

		public void TestModifyFile_Scaling()
		{
			decimal newScale = 90.69M;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.Scale = newScale;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);

				int originalRowHeight = xls.GetRowHeight(1);
				int originalColWidth = xls.GetColWidth(2);

				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("RowHeight should not have changed", originalRowHeight, xls.GetRowHeight(1));
				AssertEquals("ColWidth should not have changed", originalColWidth, xls.GetColWidth(2));
			}
		}

		public void TestModifyFile_TwoTypesOfScaling()
		{
			decimal scale = 90.69m;
			decimal verticalScale = 80.42m;
			decimal horizontalScale = 70.69m;

			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.Scale = scale;
			queue.RowScale = verticalScale;
			queue.ColumnScale = horizontalScale;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);

				int originalRowHeight = xls.GetRowHeight(1);
				int originalColWidth = xls.GetColWidth(2);

				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("RowHeight should have changed", (int)(originalRowHeight * verticalScale / 100), xls.GetRowHeight(1));
				AssertEquals("ColWidth should have changed", (int)(originalColWidth * horizontalScale / 100), xls.GetColWidth(2));
			}
		}

		public void TestModifyFile_VerticalRowScaling()
		{
			int newScale = 90;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.RowScale = newScale;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);

				int originalRowHeight = xls.GetRowHeight(1);
				int originalColWidth = xls.GetColWidth(2);

				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("RowHeight should have changed", originalRowHeight * newScale / 100, xls.GetRowHeight(1));
				AssertEquals("ColWidth should have stayed the same", originalColWidth, xls.GetColWidth(2));
			}
		}

		public void TestModifyFile_HorizontalColumnScaling()
		{
			int newScale = 90;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.ColumnScale = newScale;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);

				int originalRowHeight = xls.GetRowHeight(1);
				int originalColWidth = xls.GetColWidth(2);

				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("RowHeight should have stayed the same", originalRowHeight, xls.GetRowHeight(1));
				AssertEquals("ColWidth should have stayed the same", originalColWidth * newScale / 100, xls.GetColWidth(2));
			}
		}

		public void TestModifyFile_PrintDriverSettings()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var printDriverSettingsPath = resourceRetriever.SaveResourceToFile("Enterprise.RemotePrinting.Engine.Testing.TestFiles.PrintDriverSettings.xls", "PrintDriverSettings.xls");
				var printDriverXls = new XlsFile();
				printDriverXls.Open(printDriverSettingsPath);
				var printDriverSettings = new PrinterDriverAdapter(printDriverXls).GetPrinterDriverSettings();

				var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
				var queue = GetSerialisablePrintQueue();
				queue.XlsTemplate = printDriverSettings;

				var job = new PrintEngineJob(serialisableJob, queue, null);
				using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
				{
					var xls = new XlsFile();
					xls.Open(TestFileToPrint);

					AssertEquals("Default a4 page size", TPaperSize.A4, xls.PrintPaperSize);
					var oldDimensions = xls.PrintPaperDimensions;

					printer.ModifyFileIfChangesRequired(xls);

					AssertEquals("Page size should now be a5", TPaperSize.A5, xls.PrintPaperSize);
					Assert("Paper dimensions should change", oldDimensions.Height != xls.PrintPaperDimensions.Height);
					Assert("Paper dimensions should change", oldDimensions.Width != xls.PrintPaperDimensions.Width);
				}
			}
		}

		public void TestModifyFile_CaterForOutOfBoundValues_VerticalRowScaling()
		{
			var rowScale = ushort.MaxValue + 1;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.RowScale = rowScale;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);
				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("Row height should be FlxConsts.MaxRowHeight", FlxConsts.MaxRowHeight, xls.GetRowHeight(1));
			}
		}

		public void TestModifyFile_CaterForOutOfBoundValues_TopMargin_MinValue()
		{
			var topMargin = -1 - ushort.MaxValue;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.TopMargin = topMargin;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);
				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("Row height should be 0.", 0, xls.GetRowHeight(1));
			}
		}

		public void TestModifyFile_CaterForOutOfBoundValues_TopMargin_MaxValue()
		{
			var topMargin = ushort.MaxValue + 1;
			var serialisableJob = GetSerialisablePrintJob(TestFileToPrint);
			var queue = GetSerialisablePrintQueue();
			queue.TopMargin = topMargin;

			var job = new PrintEngineJob(serialisableJob, queue, null);
			using (var printer = (BasePrinterForXLSFile)GetPrinter(job))
			{
				var xls = new XlsFile();
				xls.Open(TestFileToPrint);
				printer.ModifyFileIfChangesRequired(xls);

				AssertEquals("Row height should be FlxConsts.MaxRowHeight", FlxConsts.MaxRowHeight, xls.GetRowHeight(1));
			}
		}
	}
}
