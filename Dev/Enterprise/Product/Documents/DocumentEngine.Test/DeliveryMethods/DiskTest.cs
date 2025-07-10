using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.PdfiumWrapper;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DeliveryMethods.Testing
{
	sealed class DiskTest : TransactionedTestCase
	{
		public void TestDeliver()
		{
			DeliveryInfo info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			DeliveryInfo info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			Disk method = new Disk();
			method.AddFile(info1);
			method.AddFile(info2);
			method.OutputDirectory = Env.GetTempFileName();
			File.Delete(method.OutputDirectory);
			DirectoryInfo testOutput = Directory.CreateDirectory(method.OutputDirectory);
			try
			{
				method.Deliver();
				AssertEquals("No other files should exist", 2, testOutput.GetFiles().Length);
			}
			finally
			{
				testOutput.Delete(true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWatermark()
		{
			DocumentsDataRegistry.Instance.EmbedFontsInPDF.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var xlsPath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\AreaTestFiles\Footers.xls");
			byte[] fileContent = File.ReadAllBytes(xlsPath);

			var info = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info.FileFormat = "XLS"; //only xls are converted
			info.FileContents.Write(fileContent, 0, fileContent.Length);

			var method = new Disk();
			method.AddFile(info);
			method.OutputFormatOverride = OutputFormatType.PDF;

			method.OutputDirectory = Env.GetTempFileName();
			File.Delete(method.OutputDirectory);
			var testOutput = Directory.CreateDirectory(method.OutputDirectory);
			try
			{
				method.Deliver();
				AssertEquals("No other files should exist", 1, testOutput.GetFiles().Length);
				var file = testOutput.GetFiles()[0];
				string extention = file.Extension.ToUpper();
				using (var stream = File.OpenRead(file.FullName))
				{
					var bytes = stream.CopyToByteArray();
					string expectedFileName = "Watermark" + extention;

					using (var expected = File.OpenRead(TestCaseWithFactory.BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\ExpectedDocuments\" + expectedFileName))
					using (var expectedDocument = new PdfDocument(expected))
					using (var actualStream = new MemoryStream(bytes))
					using (var actualDocument = new PdfDocument(actualStream))
					{
						var expectedContent = expectedDocument.GetAllText();
						var actualContent = actualDocument.GetAllText();

						AssertMultilineASCIIEquals("Output file should be the same.", expectedContent, actualContent);
					}
				}
			}
			finally
			{
				testOutput.Delete(true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliverAsPDF()
		{
			string xlsPath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\AreaTestFiles\Footers.xls");
			byte[] fileContent = File.ReadAllBytes(xlsPath);

			DeliveryInfo info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.FileFormat = "XLS"; //only xls are converted
			info1.FileContents.Write(fileContent, 0, fileContent.Length);

			DeliveryInfo info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			info2.AttachedFilename = "EDoc";
			info2.FileFormat = "PDF"; //edoc output should not be converted

			DeliveryInfo info3 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Report);
			info3.AttachedFilename = "Report";
			info3.FileFormat = "XLS"; //only xls are converted
			info3.FileContents.Write(fileContent, 0, fileContent.Length);

			DeliveryInfo info4 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.TIFF);
			info4.FileFormat = "TIF"; //tif stays

			DeliveryInfo info5 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.File);
			info5.AttachedFilename = "EDOC";
			info5.FileFormat = "XLS"; //edoc output should not be converted

			DeliveryInfo info6 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info6.AttachedFilename = "EDoc";
			info6.FileFormat = "XLS"; //only xls are converted
			info6.FileContents.Write(fileContent, 0, fileContent.Length);

			Disk method = new Disk();
			method.AddFile(info1);
			method.AddFile(info2);
			method.AddFile(info3);
			method.AddFile(info4);
			method.AddFile(info5);
			method.AddFile(info6);

			method.OutputFormatOverride = OutputFormatType.PDF;

			method.OutputDirectory = Env.GetTempFileName();
			File.Delete(method.OutputDirectory);
			DirectoryInfo testOutput = Directory.CreateDirectory(method.OutputDirectory);
			try
			{
				method.Deliver();
				AssertEquals("No other files should exist", 6, testOutput.GetFiles().Length);

				int pdfCount = 0;
				int tifCount = 0;
				int xlsCount = 0;
				foreach (FileInfo file in testOutput.GetFiles())
				{
					string extention = file.Extension.ToUpper();
					if (extention == ".TIF")
					{
						tifCount++;
					}
					else if (extention == ".PDF")
					{
						pdfCount++;
					}
					else if (extention == ".XLS")
					{
						xlsCount++;
					}
				}

				AssertEquals("pdf Count", 4, pdfCount);
				AssertEquals("tif count", 1, tifCount);
				AssertEquals("xls count", 1, xlsCount);

				string edocPDF = Path.Combine(testOutput.FullName, "EDOC.PDF");
				Assert(edocPDF, File.Exists(edocPDF));

				string edocPDF1 = Path.Combine(testOutput.FullName, "EDOC1.PDF");
				Assert(edocPDF1, File.Exists(edocPDF1));

				string edocXLS = Path.Combine(testOutput.FullName, "EDOC.XLS");
				Assert(edocXLS, File.Exists(edocXLS));
			}
			finally
			{
				testOutput.Delete(true);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilePathInvalidCharsRemoved()
		{
			string xlsPath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\AreaTestFiles\Footers.xls");
			byte[] fileContent = File.ReadAllBytes(xlsPath);

			DeliveryInfo info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.AttachedFilename = @"Recipient Created / Self Billed Invoice DPOALSLPQD  (23/04/2009 10:49:00 AM).tif";
			info1.FileFormat = "TIF"; //only xls are converted
			info1.FileContents.Write(fileContent, 0, fileContent.Length);

			Disk method = new Disk();
			method.AddFile(info1);

			method.OutputFormatOverride = OutputFormatType.PDF;

			method.OutputDirectory = Env.GetTempFileName();
			File.Delete(method.OutputDirectory);
			DirectoryInfo testOutput = Directory.CreateDirectory(method.OutputDirectory);
			try
			{
				method.Deliver();
				AssertEquals("No other files should exist", 1, testOutput.GetFiles().Length);
			}
			finally
			{
				testOutput.Delete(true);
			}
		}
	}
}
