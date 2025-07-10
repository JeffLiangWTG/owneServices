using System.Collections.Generic;
using System.IO;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Test.DeliveryMethods
{
	sealed class MemoryTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDeliver()
		{
			var xlsPath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\AreaTestFiles\Footers.xls");
			var fileContent = File.ReadAllBytes(xlsPath);

			var info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info1.FileFormat = "PDFA";
			info1.FileContents.Write(fileContent, 0, fileContent.Length);

			var info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info2.FileFormat = "TIF";
			info2.FileContents.Write(fileContent, 0, fileContent.Length);

			var info3 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
			info3.FileFormat = "PDF";
			info3.FileContents.Write(fileContent, 0, fileContent.Length);

			var output = new List<(string fileName, string docType, byte[] imageBytes)>();
			var method = new Memory(output);
			method.AddFile(info1);
			method.AddFile(info2);
			method.AddFile(info3);

			method.Deliver();

			AssertEquals("All 3 files should be part of the output", 3, output.Count);

			foreach (var outputFile in output)
			{
				using (var excelFile = new ExcelInterface())
				{
					excelFile.LoadExcelFile(outputFile.imageBytes);

					AssertEquals("Should contain only 1 worksheet", 1, excelFile.WorkSheets.Count);
					AssertEquals("Worksheet should contain 40 rows", 40, excelFile.WorkSheets[0].RowCount);
					AssertEquals("File name is correct", "Footers", excelFile.WorkSheets[0].SheetName);
					CombineAssertions("Check first few rows to confirm text data is sensible and not corrupted", () =>
					{
						AssertEquals("#config", excelFile.WorkSheets[0][0, 0].ToString());
						AssertEquals("PageStyle=Portrait", excelFile.WorkSheets[0][1, 0].ToString());
						AssertEquals("Name=Footers", excelFile.WorkSheets[0][2, 0].ToString());
					});
				}
			}
		}
	}
}
