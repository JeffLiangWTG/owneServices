using System.IO;
using CargoWise.IO;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.KNA.Business.Testing
{
	public class KNAOrgSupplierPartDataLoaderTest : OrgSupplierPartDataSaveTest
	{
		protected override void TestImportThenExportCore()
		{
			KNAOrgSupplierPartDataLoader loader = new KNAOrgSupplierPartDataLoader();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFilePath = resourceRetriever.SaveResourceToFile("Business.OrgSupplierPart.TestFiles.KNAOrgSupplierPart.csv");
				loader.ImportProductData(testFilePath, false, false);
				using (TempFile file = TempFile.New())
				{
					KNAOrgSupplierPartDataSaver saver = new KNAOrgSupplierPartDataSaver();
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
