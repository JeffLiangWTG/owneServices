using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.IO;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.FSH.TsManifest
{
	class FortuneShippingFlatFileDataImporterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			ForwardingConsolCollection collection = new ForwardingConsolCollection(Factory);
			ZQuery oceanBill1NumberFilter = new ZQuery(JobConsolSchema.JK_MasterBillNum, OceanBill1.OceanBillNumber);
			ZQuery oceanBill2NumberFilter = new ZQuery(JobConsolSchema.JK_MasterBillNum, OceanBill2.OceanBillNumber);
			collection.Load(oceanBill1NumberFilter);
			AssertEquals("precondition: no consol exists", 0, collection.Count);
			collection.Load(oceanBill2NumberFilter);
			AssertEquals("precondition: no consol exists", 0, collection.Count);
			TestFortuneShippingFlatFileDataImporter importer = new TestFortuneShippingFlatFileDataImporter(Factory);
			NotificationBuffer notification = new NotificationBuffer();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testFilePath = resourceRetriever.SaveResourceToFile("TsManifest.TestFiles.TestFile.txt");
				importer.ImportData(testFilePath, notification, SourceInfo.EmptySourceInfo);
			}
			collection.Load(oceanBill1NumberFilter);
			AssertEquals("Consol 1 exists", 1, collection.Count);
			ForwardingConsol consol = collection[0];
			AssertEquals(OceanBill1.OceanBillNumber, consol.JK_MasterBillNum);
			OrgHeader org = OrgHeader.LoadFromCode(Factory, "RICSHI");
			if (org != null)
			{
				org.Delete();
			}

			org = OrgHeader.LoadFromCode(Factory, "FORSHI");
			if (org != null)
			{
				org.Delete();
			}

			org = OrgHeader.LoadFromCode(Factory, "CHISHI");
			if (org == null)
			{
				org = OrgHeader.New(Factory);
				org.FillWithValidTestData();
				org.OH_Code = "CHISHI";
				Factory.Save();
			}

			AssertEquals("Defaults to AGT", "AGT", consol.JK_AgentType);
			AssertNull("No org created if not matched", consol.SendingForwarder);
			AssertNull("No org created if not matched", consol.ReceivingForwarder);
			AssertEquals("Defaults to CHISHI (matches when org exists)", "CHISHI", consol.ShippingLine.OH_Code);
			AssertEquals("Consol is FCL", "FCL", consol.JK_ConsolMode);
			AssertEquals("Container is FCL", "FCL", consol.Containers[0].JC_ContainerMode);
			AssertEquals("Shipment is FCL", "FCL", consol.Shipments[0].JS_PackingMode);
			ForwardingShipment shipment = consol.Shipments[0];
			AssertEquals("Shipment outer packs", 64, shipment.JS_OuterPacks);
			CMRSeaCargoSynchroniser synchroniser = new CMRSeaCargoSynchroniser(consol);
			CusSCAOceanBill oceanBill = synchroniser.OceanBill;
			AssertEquals("OceanBill number", OceanBill1.OceanBillNumber, oceanBill.CB_OceanBill);
			AssertEquals("Voyage number", OceanBill1.VoyageNumber, oceanBill.CB_Voyage);
			AssertEquals("Load", OceanBill1.PortOfLoad, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Discharge", OceanBill1.PortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("Has one container", 1, oceanBill.Containers.Count);
			CusSCAContainer container = oceanBill.Containers[0];
			AssertEquals("Has one housebill", 1, oceanBill.HouseBills.Count);
			CusSCAHouse house = oceanBill.HouseBills[0];
			AssertEquals("Has one pivot", 1, oceanBill.Pivots.Count);
			CusSCAPivot pivot = oceanBill.Pivots[0];
			AssertEquals("house attached to pivot", house, pivot.HouseBill);
			AssertEquals("container attached to pivot", container, pivot.Container);
			AssertEquals("container number", OceanBill1.ContainerNumber, container.CN_ContainerNumber);
			AssertEquals("container seal", OceanBill1.SealNo, container.CN_SealNumber);
			collection.Load(oceanBill2NumberFilter);
			AssertEquals("Consol 2 exists", 1, collection.Count);
			consol = collection[0];
			AssertEquals(OceanBill2.OceanBillNumber, consol.JK_MasterBillNum);
			synchroniser = new CMRSeaCargoSynchroniser(consol);
			oceanBill = synchroniser.OceanBill;
			AssertEquals("OceanBill number", OceanBill2.OceanBillNumber, oceanBill.CB_OceanBill);
			AssertEquals("Voyage number", OceanBill2.VoyageNumber, oceanBill.CB_Voyage);
			AssertEquals("Load", OceanBill2.PortOfLoad, oceanBill.CB_RL_NKPortOfLoading);
			AssertEquals("Discharge", OceanBill2.PortOfDischarge, oceanBill.CB_RL_NKPortOfDischarge);
			AssertEquals("Has one container", 1, oceanBill.Containers.Count);
			container = oceanBill.Containers[0];
			AssertEquals("Has one housebill", 1, oceanBill.HouseBills.Count);
			house = oceanBill.HouseBills[0];
			AssertEquals("Has one pivot", 1, oceanBill.Pivots.Count);
			pivot = oceanBill.Pivots[0];
			AssertEquals("house attached to pivot", house, pivot.HouseBill);
			AssertEquals("container attached to pivot", container, pivot.Container);
			AssertEquals("container number", OceanBill2.ContainerNumber, container.CN_ContainerNumber);
			AssertEquals("container seal", OceanBill2.SealNo, container.CN_SealNumber);
		}

		public void TestImportDataToFactoryCore()
		{
			FortuneShippingFlatFileDataImporter importer = new FortuneShippingFlatFileDataImporter(Factory);
			ITransactionParticipant[] notUsed = System.Array.Empty<ITransactionParticipant>();
			AssertEquals(true, importer.InternalImportDataToFactoryCore(new StringReader("foo"), "foo", new NotificationBuffer(), out notUsed));
		}

		public void TestExtractToDataAdapter()
		{
			FortuneShippingFlatFileDataImporter importer = new FortuneShippingFlatFileDataImporter(Factory);
			AssertEquals(false, importer.InternalExtractToDataAdapter(new Xsd.ConsolCollection(), new NotificationBuffer()));
		}

		class TestFortuneShippingFlatFileDataImporter : FortuneShippingFlatFileDataImporter
		{
			public TestFortuneShippingFlatFileDataImporter(BusinessObjectFactory factory) : base(factory)
			{
			}
		}

		#region Test Data
		abstract class OceanBillCommon
		{
			public const string VesselLloyds = "8707434";
			public const string VoyageNumber = "504S";
			public const string PortOfDischarge = "AUADL";
		}

		abstract class OceanBill1 : OceanBillCommon
		{
			public const string OceanBillNumber = "8PGUADL400700";
			public const string PortOfLoad = "MYPKG";
			public const string PortOfOrigin = "MYPGU";
			public const string ContainerNumber = "CCLU4398759";
			public const string SealNo = "D851255";
		}

		abstract class OceanBill2 : OceanBillCommon
		{
			public const string OceanBillNumber = "8SUBADL4A3091";
			public const string PortOfLoad = "IDSUB";
			public const string PortOfOrigin = "IDSUB";
			public const string ContainerNumber = "CCLU2023455";
			public const string SealNo = "D366967";
		}
		#endregion
	}
}
