using System.Drawing;
using System.IO;
using CargoWise.IO;
using Enterprise.Barcode.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class DocumentImagePageWriterUtilsTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInsertMultiPageImageToFile()
		{
			string outputFile = Temp.GetTempFileNameWithExtension("tif");
			File.Copy(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_3pages.tif", outputFile, true);
			File.SetAttributes(outputFile, System.IO.FileAttributes.Normal);
			try
			{
				string imageFile = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_2pages.tif";
				DocumentImagePageWriterUtils.InsertMultiPageImageToFile(imageFile, outputFile, 1);

				using (var reader2 = new ImageFileReaderWithLock(outputFile))
				{
					AssertEquals("Num pages", 5, reader2.PageSelector.TotalPages);
				}
			}
			finally
			{
				File.Delete(outputFile);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCopyMultiPageImageToFileWithReplace()
		{
			var inputFile = Temp.GetTempFileNameWithExtension("tif");
			var outputFile = Temp.GetTempFileNameWithExtension("tif");
			var singlePageAfterReplacementFile = Temp.GetTempFileNameWithExtension("tif");

			var singlePageFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\barcode_hires.tif";
			var multiPageFilePath = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_3pages.tif";
			File.Copy(multiPageFilePath, inputFile, true);
			File.SetAttributes(inputFile, System.IO.FileAttributes.Normal);

			try
			{
				using (var mainImage = Image.FromFile(inputFile))
				{
					using (var replacementPage = Image.FromFile(singlePageFilePath))
					{
						using (var main = new StandardImagePageSelector(mainImage))
						{
							using (var replacement = new StandardImagePageSelector(replacementPage))
							{
								DocumentImagePageWriterUtils.CopyMultiPageImageToFileWithReplace(main, outputFile, 0, replacement);

								using (var modifiedFile = Image.FromFile(outputFile))
								{
									modifiedFile.Save(singlePageAfterReplacementFile); // saves one page
								}
							}
						}
					}
				}

				// Very difficult to actually assert which page got replaced.
				// Since I replaced it with a barcoded page, i'm going to try to scan the doc for
				// barcodes. if it finds one, then the replacement was successful.

				using (var result = Image.FromFile(outputFile))
				{
					var barcodes = new BarcodeScanner().ExtractBarcodes((Bitmap)result);
					AssertEquals("Barcode page was replaced, should find a barcode", "^DOC=CIV|", barcodes[0]);
				}
			}
			finally
			{
				if (File.Exists(inputFile))
				{
					File.Delete(inputFile);
				}

				if (File.Exists(outputFile))
				{
					File.Delete(outputFile);
				}

				if (File.Exists(singlePageAfterReplacementFile))
				{
					File.Delete(singlePageAfterReplacementFile);
				}
			}
		}
	}
}
