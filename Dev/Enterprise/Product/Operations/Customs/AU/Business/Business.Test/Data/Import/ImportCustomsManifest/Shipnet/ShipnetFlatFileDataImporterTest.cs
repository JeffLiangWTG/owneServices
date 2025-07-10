using System.IO;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ShipnetFlatFileDataImporterTest : FlatFileDataImporterTestCase
	{
		public void TestImport()
		{
			FlatFileDataImporter importer = GetDataImporter();
			Assert("Precondition: PathToTestFile must point to a valid test file that exists on the filesystem", File.Exists(PathToTestFile));
			NotificationBuffer buffer = new NotificationBuffer();
			importer.ImportData(PathToTestFile, buffer, SourceInfo.EmptySourceInfo);
			CusSeaManTranHead imported = CusSeaManTranHead.LoadByVoyageNumberVessel(Factory, "01P5S", "CAPE DARNLEY");
			AssertNotNull(imported);
			AssertEquals("OceanBills", 2, imported.OceanBills.Count);
			CusSeaManOBLHeader oceanBill = imported.OceanBills[0];

			AssertEquals("DLCBNE01P5BB01A", oceanBill.BO_OceanBill);
			AssertEquals("CNDLC", oceanBill.BO_RL_NKOriginPort);
			AssertEquals("CNDLC", oceanBill.BO_RL_NKLoadPort);
			AssertEquals("AUBNE", oceanBill.BO_RL_NKDestinationPort);
			AssertEquals("AUBNE", oceanBill.BO_RL_NKDischargePort);
			AssertEquals("China Emoto", oceanBill.BO_ConsignorName.SubstringSafe(0, 11));
			AssertEquals("Hitachi Australia", oceanBill.BO_ConsigneeName.SubstringSafe(0, 17));
			AssertEquals(1, oceanBill.Details.Count);
			AssertEquals(CMRCargoTypes.Codes.BreakBulk, oceanBill.Details[0].BD_LineCargoType);
			AssertEquals(318, oceanBill.Details[0].BD_NoOfPacks);
			AssertNull(oceanBill.Consignee);
			AssertNull(oceanBill.Consignor);
			AssertEquals(ZGuid.Empty, oceanBill.BO_OH_Consignee);
			AssertEquals(ZGuid.Empty, oceanBill.BO_OH_Consignor);

			oceanBill = imported.OceanBills[1];
			AssertEquals("NKGBNE01P5FF105A", oceanBill.BO_OceanBill);
			AssertEquals("CNNKG", oceanBill.BO_RL_NKOriginPort);
			AssertEquals("CNSHA", oceanBill.BO_RL_NKLoadPort);
			AssertEquals("AUBNE", oceanBill.BO_RL_NKDestinationPort);
			AssertEquals("AUBNE", oceanBill.BO_RL_NKDischargePort);
			AssertEquals("China Peace", oceanBill.BO_ConsignorName.SubstringSafe(0, 11));
			AssertEquals("Kashmiri", oceanBill.BO_ConsigneeName.SubstringSafe(0, 8));
			AssertEquals(1, oceanBill.Details.Count);
			AssertEquals(CMRCargoTypes.Codes.FullContainerLoad, oceanBill.Details[0].BD_LineCargoType);
			AssertEquals(20, oceanBill.Details[0].BD_NoOfPacks);
			AssertEquals("GESU2856217", oceanBill.Details[0].BD_ContainerNumber);
			AssertEquals("0000", oceanBill.Details[0].BD_ContainerSizeOrISOCode);
			AssertNull(oceanBill.Consignee);
			AssertNull(oceanBill.Consignor);
			AssertEquals(ZGuid.Empty, oceanBill.BO_OH_Consignee);
			AssertEquals(ZGuid.Empty, oceanBill.BO_OH_Consignor);

			AssertEquals("ArrivalPorts", 1, imported.Arrivals.Count);
			CusSeaManArrivalPort port = imported.Arrivals[0];
			AssertEquals("AUBNE", port.BA_RL_NKArrivalPort);
			AssertEquals(new ZDateTime(2005, 03, 17), port.BA_ArrivalPortETA);

			AssertEquals("CargoLines", 20, port.CargoLines.Count);
			CusSeaManOBLHeaderCargoLine cargoLine = port.CargoLines[0];
			AssertEquals("RWMU000006", cargoLine.CargoIdentifier);
			AssertEquals(1, cargoLine.NumberOfPackages);
		}

		protected override FlatFileDataImporter GetDataImporter() => new ShipnetFlatFileDataImporter(Factory.New<CusSeaManTranHead>());

		protected override string PathToTestFile => embeddedResourceRetriever.SaveResourceToFile("Enterprise.Customs.AU.Declaration.Business.Testing.Data.Import.TestFiles.ImportIMM2.TXT");

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever.Dispose();
		}
		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
