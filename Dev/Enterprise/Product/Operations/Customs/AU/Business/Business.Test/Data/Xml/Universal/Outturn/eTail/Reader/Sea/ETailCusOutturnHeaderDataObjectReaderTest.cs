using System.Linq;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ETailCusOutturnHeaderDataObjectReaderTest : DataTransfer.Universal.Outturn.Testing.OutturnDataObjectReaderTestHelper<CusOutturnHeader, DepotCusOutturn>
	{
		public void TestFillSubShipmentCore()
		{
			var logger = new DummyLogger();
			var reader = new ETailCusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			AssertEquals(2, outturnHeader.Outturns.Count);
		}

		public void TestUpdateOutturns()
		{
			var logger = new DummyLogger();
			var reader = new ETailCusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader1 = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			var outturnShipment = CreateTestOutturnShipment("125", "125", "MB4", "HB4");
			shipment.SubShipmentCollection.Single().SubShipmentCollection.Add(outturnShipment);
			reader = new ETailCusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader2 = reader.ReadIntoBusinessObject();
			AssertEquals(outturnHeader1.PK, outturnHeader2.PK);
			AssertEquals(3, outturnHeader1.Outturns.Count);

			Factory.SaveForTesting();

			shipment.SubShipmentCollection.Single().SetSubShipmentCollection(() => new DataObjectList<Shipment>() { outturnShipment });
			reader = new ETailCusOutturnHeaderDataObjectReader(shipment, logger, Factory);
			var outturnHeader3 = reader.ReadIntoBusinessObject();
			AssertEquals(outturnHeader1.PK, outturnHeader3.PK);
			AssertEquals(1, outturnHeader1.Outturns.Count);
		}

		public void TestDataMergeFromDifferentShipments()
		{
			var forwardingShipment1 = Factory.New<ForwardingShipment>();
			forwardingShipment1.JS_UniqueConsignRef = "S00001791";

			var shipment1 = CreateTestOutturnHeaderShipment(refVessel, premise, "Q123", "AU", null, "S00001791");
			var outturnShipment1 = CreateTestOutturnShipment("123", "123", "MB2", "HB1");
			shipment1.SetSubShipmentCollection(() => CreateTestHVLVOutturnShipment(shipment1, new Shipment[] { outturnShipment1 }));

			var reader = new ETailCusOutturnHeaderDataObjectReader(shipment1, logger, Factory);
			var outturnHeader = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Outturn imported", outturnHeader.Outturns);
				AssertEquals("Outturn count", 1, outturnHeader.Outturns.Count);
			});

			Factory.SaveForTesting();

			var forwardingShipment2 = Factory.New<ForwardingShipment>();
			forwardingShipment2.JS_UniqueConsignRef = "S00001792";

			var shipment2 = CreateTestOutturnHeaderShipment(refVessel, premise, "Q123", "AU", null, "S00001792");
			var outturnShipment2 = CreateTestOutturnShipment("123", "123", "MB2", "HB2");
			shipment2.SetSubShipmentCollection(() => CreateTestHVLVOutturnShipment(shipment2, new Shipment[] { outturnShipment2 }));

			reader = new ETailCusOutturnHeaderDataObjectReader(shipment2, logger, Factory);
			outturnHeader = reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertNotNull("Outturn imported", outturnHeader.Outturns);
				AssertEquals("Outturn count", 2, outturnHeader.Outturns.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			refVessel = CreateRefVesselForTest();
			premise = CreatePremiseAddressForTest();

			shipment = CreateTestOutturnHeaderShipment(refVessel, premise, "Q124", "AU", null, "S00001781");
			var outturnShipment1 = CreateTestOutturnShipment("123", "123", "MB2", "HB2");
			var outturnShipment2 = CreateTestOutturnShipment("124", "124", "MB3", "HB3");
			shipment.SetSubShipmentCollection(() => CreateTestHVLVOutturnShipment(shipment, new Shipment[] { outturnShipment1, outturnShipment2 }));

			Factory.SaveForTesting();
		}

		DataObjectList<Shipment> CreateTestHVLVOutturnShipment(Shipment shipment, Shipment[] outturnShipmentArr)
		{
			var subShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ShipmentType = new CodeDescriptionPair() { Code = Core.Constants.ShipmentTypes.HighVolumeLowValue },
			};
			subShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>(outturnShipmentArr));
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>() { subShipment });
			return shipment.SubShipmentCollection;
		}

		OrgHeader premise;
		RefVessel refVessel;
		Shipment shipment;
	}
}
