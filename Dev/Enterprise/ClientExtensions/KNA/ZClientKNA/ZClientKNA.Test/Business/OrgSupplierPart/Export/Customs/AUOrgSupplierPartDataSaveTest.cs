using System.IO;
using CargoWise.IO;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class AUOrgSupplierPartDataSaveTest : OrgSupplierPartDataSaveTest
	{
		protected override void TestImportThenExportCore()
		{
			AUOrgSupplierPartDataLoad loader = new AUOrgSupplierPartDataLoad();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFilePath = resourceRetriever.SaveResourceToFile("Business.OrgSupplierPart.Export.Customs.TestFiles.AUOrgSupplierPart.csv");
				loader.ImportProductData(testFilePath, false, false);
				using (TempFile file = TempFile.New())
				{
					AUOrgSupplierPartDataSave saver = new AUOrgSupplierPartDataSave();
					saver.ExportProductData(file.Filename);
					using (StreamReader testFileReader = new StreamReader(testFilePath))
					using (StreamReader exportedFileReader = new StreamReader(file.Filename))
					{
						AssertEquals("Exported file heading the same as original imported file", testFileReader.ReadLine(), exportedFileReader.ReadLine());
						AssertEquals("Exported file data line the same as original imported file", testFileReader.ReadLine(), exportedFileReader.ReadLine());
					}
				}
			}
		}
	}
}
