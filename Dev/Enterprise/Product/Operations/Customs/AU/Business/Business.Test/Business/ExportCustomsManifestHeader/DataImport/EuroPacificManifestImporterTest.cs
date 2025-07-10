using System.IO;
using System.Text;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.Common.AU;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EuroPacificManifestImporterTest : ManifestImporterTest
	{
		public void TestImportFileEndToEnd5()
		{
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SampleImport5.txt"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var success = importer.ImportData(data, "SampleImport5.txt", buffer, SourceInfo.EmptySourceInfo);
				Assert("Success", success);
				Assert("No Errors", !buffer.HasErrors);
				var importedHeader = importer.GeneratedHeader;
				var expectedHeader = GetExpectedHeader5();
				AssertHeadersTheSame(expectedHeader, importedHeader);
			}
		}

		public void TestImportFileEndToEnd6()
		{
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SampleImport6.txt"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var expectedHeader = GetExpectedHeader6();
				var success = importer.ImportData(data, "SampleImport6.txt", buffer, SourceInfo.EmptySourceInfo);
				Assert("Success", success);
				Assert("No Errors", !buffer.HasErrors);
				var importedHeader = importer.GeneratedHeader;

				importedHeader.CalculateTotalContainersFromLines();
				importedHeader.CalculateTotalPackagesFromLines();

				AssertHeadersTheSame(expectedHeader, importedHeader);
			}
		}

		public void TestDontOverwriteAlreadySetValues()
		{
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SampleImport6.txt"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var expectedHeader = GetExpectedHeader6();
				var importedHeader = GetExistingHeader6();
				var success = importer.ImportDataToHeader(importedHeader, data, buffer);
				Assert("Success", success);
				Assert("No Errors", !buffer.HasErrors);
				importedHeader.Lines.RemoveAndDeleteAll();
				// Totals now get automatically recalculated... so if you delete all the lines...
				importedHeader.ED_NoOfPacks = 32;

				AssertHeadersTheSame(GetExistingHeader6(), importedHeader);
			}
		}

		public void TestImportFileEndToEnd7()
		{
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SampleImport7.txt"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var expectedHeader = GetExpectedHeader6();
				var success = importer.ImportData(data, "SampleImport7.txt", buffer, SourceInfo.EmptySourceInfo);
				Assert("Success", success);
				Assert("No Errors", !buffer.HasErrors);
				var importedHeader = importer.GeneratedHeader;
				//AssertHeadersTheSame(ExpectedHeader, ImportedHeader);
			}
		}

		public void TestImportFileEndToEnd8()
		{
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("mermnv26.cmr"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var expectedHeader = GetExpectedHeader8();
				var success = importer.ImportData(data, "mermnv26.cmr", buffer, SourceInfo.EmptySourceInfo);
				Assert("Success", success);
				Assert("No Errors", !buffer.HasErrors);
				var importedHeader = importer.GeneratedHeader;
				AssertHeadersTheSame(expectedHeader, importedHeader);
			}
		}

		public void TestImportFileEndToEnd9()
		{
			var pathToTestFile = embeddedResourceRetriever.SaveResourceToFile(GetEmbeddedResourcePath("SampleImport8.txt"));
			using (var data = new StreamReader(pathToTestFile, Encoding.ASCII))
			{
				var expectedHeader = GetExpectedHeader9();
				var success = importer.ImportData(data, "SampleImport8.txt", buffer, SourceInfo.EmptySourceInfo);
				Assert("Success", success);
				Assert("No Errors", !buffer.HasErrors);
				var importedHeader = importer.GeneratedHeader;
				AssertHeadersTheSame(expectedHeader, importedHeader);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			importer = new EuroPacificManifestImporter(Factory);
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.Business.ExportCustomsManifestHeader.DataImport.TestFiles." + fileName;

		ExportCustomsManifestHeader GetExpectedHeader5()
		{
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			result.ED_TransportMode = Core.Constants.TransportModes.Sea;
			result.ED_NoOfContainer = 7;
			result.ED_NoOfEmptyContainers = 0;
			result.ED_NoOfPacks = 557;
			result.ED_DepartureDate = new ZDateTime(2004, 10, 18);

			AddNewLine(result, "CEFGHJKLT", 2, 300);
			AddNewLine(result, "EXPE", 1, 1);
			result.Lines[1].EL_GoodsDescription = "PERSONAL EFFECTS";
			result.Lines[1].EL_GoodsOwner = "WAKEFIELD TRANSPORT";
			result.Lines[1].EL_RN_NKCountryOfDestination = "SG";

			AddNewLine(result, "ACEFGHJKH", 1, 55);
			AddNewLine(result, "A76GG9FEG", 2, 200);
			AddNewLine(result, "CY46W3XAH", 1, 1);

			return result;
		}

		ExportCustomsManifestHeader GetExpectedHeader6()
		{
			var newCTO = Factory.New<OrgHeader>();
			newCTO.OH_FullName = "Full Name";
			newCTO.OH_RL_NKClosestPort = "AUSYD";
			newCTO.Addresses[0].OA_Address1 = "ADDRESS1";
			newCTO.Addresses[0].OA_City = "SYDNEY";
			newCTO.Addresses[0].LocalControlledPremisesID = "ADL01";

			Factory.Save();
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			result.ED_TransportMode = Core.Constants.TransportModes.Sea;
			result.ED_NoOfContainer = 154;
			result.ED_NoOfEmptyContainers = 152;
			result.ED_NoOfPacks = 32;
			result.ED_OA_CTOAddress = newCTO.MainAddress.PK;
			result.ED_VoyageNumber = "04Q4";
			result.ED_RL_NKPortOfDeparture = "AUBNE";
			result.ED_DepartureDate = new ZDateTime(2004, 5, 1, 1, 0, 0);

			AddNewLine(result, "MELADLEE01", 152, 0);
			AddNewLine(result, "MELHKGBB01", 2, 32);

			return result;
		}

		ExportCustomsManifestHeader GetExistingHeader6()
		{
			var newCTO = Factory.New<OrgHeader>();
			newCTO.OH_FullName = "Full Name2";
			newCTO.OH_RL_NKClosestPort = "AUBNE";
			newCTO.Addresses[0].OA_Address1 = "ADDRESS2";
			newCTO.Addresses[0].OA_City = "BRISBANE";
			newCTO.Addresses[0].LocalControlledPremisesID = "ADL02";

			Factory.Save();
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			result.ED_TransportMode = Core.Constants.TransportModes.Air;
			result.ED_NoOfContainer = 154;
			result.ED_NoOfEmptyContainers = 152;
			result.ED_NoOfPacks = 32;
			result.ED_OA_CTOAddress = newCTO.MainAddress.PK;
			result.ED_VoyageNumber = "05Q5";
			result.ED_RL_NKPortOfDeparture = "AUSYD";
			result.ED_DepartureDate = new ZDateTime(2004, 6, 1, 1, 0, 0);

			return result;
		}

		ExportCustomsManifestHeader GetExpectedHeader8()
		{
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			result.ED_TransportMode = Core.Constants.TransportModes.Sea;
			result.ED_NoOfContainer = 7;
			result.ED_NoOfEmptyContainers = 0;
			result.ED_NoOfPacks = 557;
			result.ED_DepartureDate = new ZDateTime(2004, 10, 18, 17, 0, 0);

			AddNewLine(result, "CEFGHJKLT", 2, 300);
			AddNewLine(result, "EXPE", 1, 1);
			result.Lines[1].EL_GoodsDescription = "PERSONAL EFFECTS";
			result.Lines[1].EL_RN_NKCountryOfDestination = "SG";
			result.Lines[1].EL_GoodsOwner = "WAKEFIELD TRANSPORT";
			AddNewLine(result, "ACEFGHJKH", 1, 55);
			AddNewLine(result, "A76GG9FEG", 2, 200);
			AddNewLine(result, "CY46W3XAH", 1, 1);

			return result;
		}

		ExportCustomsManifestHeader GetExpectedHeader9()
		{
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			result.ED_TransportMode = Core.Constants.TransportModes.Sea;
			result.ED_NoOfContainer = 11;
			result.ED_NoOfEmptyContainers = 0;
			result.ED_NoOfPacks = 706;
			result.ED_VoyageNumber = "17B4";

			result.ED_RL_NKPortOfDeparture = "AUPKL";
			result.ED_RL_NKPortOfDestination = "THSRI";
			result.ED_RN_NKCountryOfDestination = "TH";

			AddNewLine(result, "AAAAAARHH", 0, 26);
			AddNewLine(result, "AAAAANK4S", 0, 77);
			AddNewLine(result, "AAAAA6GLK", 11, 0);
			AddNewLine(result, "AAAAANMYJ", 0, 236);
			AddNewLine(result, "AAAAARAEH", 0, 113);
			AddNewLine(result, "AAAAARA97", 0, 254);

			return result;
		}
	}
}
