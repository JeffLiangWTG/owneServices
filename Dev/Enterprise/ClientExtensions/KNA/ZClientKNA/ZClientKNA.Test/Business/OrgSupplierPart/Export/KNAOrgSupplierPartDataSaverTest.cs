using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.KNA.Business.Testing
{
	public class KNAOrgSupplierPartDataSaverTest : TestCaseWithFactory
	{
		public void TestImportThenExport()
		{
			TestHelper.ClearCustomsRecordsBeforeTesting();
			TestHelper.SetupEnvironment();
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

		KNATestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new KNATestHelper(Factory));
			}
		}

		KNATestHelper testHelper;
	}
}
