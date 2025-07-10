using System.IO;
using System.Text;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class RCLShippingManifestImporterTest : ManifestImporterTest
	{
		public void TestEndToEndImport1()
		{
			//ExportCustomsManifestHeader  = GetExpectedHeader1();
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				TestEndToEndImport1(data);
			}
		}

		public void TestImportWorksWithoutTrailingCommas()
		{
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var stringInMemory = new MemoryStream();
				var encode = new ASCIIEncoding();

				var fileInMemory = data.ReadToEnd();
				fileInMemory.Replace(",\r\n", "\r\n");

				var doc = encode.GetBytes(fileInMemory);
				stringInMemory.Write(doc, 0, doc.Length);
				stringInMemory.Position = 0;

				using (var newData = new StreamReader(stringInMemory))
				{
					TestEndToEndImport1(newData);
				}
			}
		}

		public void TestThatWeDontOverrideSetManifestType()
		{
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var header = Factory.New<ExportCustomsManifestHeader>();
				header.ED_ManifestType = ManifestTypeList.Codes.ConsolidationExportSubManifest;
				var success = importer.ImportDataToHeader(header, data, buffer);
				Assert("Success", success);
				AssertEquals("ManifestType", ManifestTypeList.Codes.ConsolidationExportSubManifest, header.ED_ManifestType);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			importer = new RCLShippingManifestImporter(Factory);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
			pathToTestFile = embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.Business.ExportCustomsManifestHeader.DataImport.TestFiles.MNV 918.csv");
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;
		string pathToTestFile;

		void TestEndToEndImport1(StreamReader data)
		{
			var success = importer.ImportData(data, "MNV 918.csv", buffer, SourceInfo.EmptySourceInfo);
			Assert("Success", success);
			Assert("No Errors", !buffer.HasErrors);
			_ = importer.GeneratedHeader;
			AssertEquals("LineCount", 22, importer.GeneratedHeader.Lines.Count);
			AssertEquals("ED_NoOfContainer", (short)106, importer.GeneratedHeader.ED_NoOfContainer);
			AssertEquals("ED_NoOfEmptyContainers", (short)4, importer.GeneratedHeader.ED_NoOfEmptyContainers);
			AssertEquals("ED_NoOfPacks", 0, importer.GeneratedHeader.ED_NoOfPacks);
		}
	}
}
