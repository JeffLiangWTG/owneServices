using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ETailDepotCusOutturnDataObjectReaderTest : DataTransfer.Universal.Outturn.Testing.OutturnDataObjectReaderTestHelper<CusOutturnHeader, DepotCusOutturn>
	{
		[TestDate(2019, 10, 31)]
		public void TestImportingData()
		{
			var logger = new DummyLogger();
			var subShipment = shipment.SubShipmentCollection.Single();
			var outturnShipment = subShipment.SubShipmentCollection.Single();
			var outturnHeader = Factory.New<CusOutturnHeader>();
			var reader = new ETailDepotCusOutturnDataObjectReader(outturnShipment, logger, Factory, outturnHeader, shipment, subShipment);
			var outturn = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("outturn.C5_C6", outturnHeader.PK, outturn.C5_C6);
				AssertEquals("outturn.C5_CargoType", "FCL", outturn.C5_CargoType);
				AssertEquals("outturn.C5_ContainerNumber", "CON123", outturn.C5_ContainerNumber);
				AssertEquals("outturn.C5_ContainerSeal", "ContainerSeal", outturn.C5_ContainerSeal);
				AssertEquals("outturn.C5_SealIntactIndicator", true, outturn.C5_SealIntactIndicator);
				AssertEquals("outturn.C5_HouseBill", "HB123", outturn.C5_HouseBill);
				AssertEquals("outturn.C5_MasterBill", "MB123", outturn.C5_MasterBill);

				AssertEquals("outturn.C5_OuterPacks", 10, outturn.C5_OuterPacks);
				AssertEquals("outturn.C5_OuterPackUnits", "BAG", outturn.C5_OuterPackUnits);
				AssertEquals("outturn.C5_PackagesOutturned", 1, outturn.C5_PackagesOutturned);
				AssertEquals("outturn.C5_PackagesUnits", "BAG", outturn.C5_PackagesUnits);

				AssertEquals("outturnHeader.C5_CargoUnpackDate", ZDateTime.Now, outturn.C5_CargoUnpackDate);
				AssertEquals("outturnHeader.C5_CargoReceiptDate", new ZDateTime(2019, 10, 31), outturn.C5_CargoReceiptDate);

				AssertEquals("outturn.C5_DamageIndicator", true, outturn.C5_DamageIndicator);
				AssertEquals("outturn.C5_PillageIndicator", false, outturn.C5_PillageIndicator);
				AssertEquals("outturn.C5_GoodsDescription", "GoodsDescription TEXT", outturn.C5_GoodsDescription);
			});

			outturnShipment.PackingLineCollection.Single().ContainerNumber = "";
			subShipment.ContainerCollection.Single().ContainerNumber = "CON124";
			Factory.SaveForTesting();
			reader = new ETailDepotCusOutturnDataObjectReader(outturnShipment, logger, Factory, outturnHeader, shipment, subShipment);
			outturn = reader.ReadIntoBusinessObject();

			AssertEquals("outturn.C5_ContainerNumber", "CON124", outturn.C5_ContainerNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			refVessel = CreateRefVesselForTest();
			premise = CreatePremiseAddressForTest();
			shipment = CreateTestOutturnHeaderShipment(refVessel, premise, "Q123", "S00001781");
			shipment.ContainerMode = new ContainerMode() { Code = "FCL" };
			shipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master };
			shipment.WayBillNumber = "MB123";

			var outturnShipment = CreateTestOutturnShipment("123", "123", "MB2", "HB2");
			outturnShipment.WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			outturnShipment.WayBillNumber = "HB123";
			outturnShipment.TotalNoOfPieces = 10;
			outturnShipment.GoodsDescription = "GoodsDescription TEXT";
			outturnShipment.PackingLineCollection.FirstOrDefault().PackType = new PackageType() { Code = "BAG" };
			outturnShipment.PackingLineCollection.FirstOrDefault().OutturnDamagedQty = 1;
			outturnShipment.PackingLineCollection.FirstOrDefault().ContainerNumber = "CON123";
			outturnShipment.PackingLineCollection.FirstOrDefault().OutturnQty = 1;

			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue },
				LocalProcessing = new LocalProcessing(DefaultDataObjectWriterStrategy.TestInstance) { LCLAvailable = new ZDateTime(2019, 10, 31) },
			};
			subShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { outturnShipment });
			subShipment.SetContainerCollection(() => new DataObjectList<Container>()
			{
				new Container(DefaultDataObjectWriterStrategy.TestInstance)
				{
					Seal = "ContainerSeal",
					IsSealOk = true
				}
			});

			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });
			Factory.SaveForTesting();
		}

		OrgHeader premise;
		RefVessel refVessel;
		Shipment shipment;
	}
}
