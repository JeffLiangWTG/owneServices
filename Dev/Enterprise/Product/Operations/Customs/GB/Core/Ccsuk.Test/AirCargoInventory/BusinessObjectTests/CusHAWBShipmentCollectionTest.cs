using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	[TestedType(typeof(CusHAWBShipmentCollection))]
	sealed class CusHAWBShipmentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new CusHAWBShipmentCollection(Factory, shipment.PK);
		}

		public void TestConstructor()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			_ = CreateHawb(shipment1);
			_ = CreateHawb(shipment1);

			var shipment2 = Factory.New<ForwardingShipment>();
			var expectedHawbs = new CusHAWB[] { CreateHawb(shipment2), CreateHawb(shipment2) };

			var hawbsCollection = new CusHAWBShipmentCollection(Factory, shipment2.PK);
			hawbsCollection.Load();

			AssertContainsExactElementsInAnyOrder(expectedHawbs, hawbsCollection);
		}

		public void Test_AddNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var expectedHawbs = new List<CusHAWB> { CreateHawb(shipment) };

			var hawbsCollection = new CusHAWBShipmentCollection(Factory, shipment.PK);
			hawbsCollection.Load();

			AssertContainsExactElementsInAnyOrder("Pre-Condition", expectedHawbs, hawbsCollection);

			expectedHawbs.Add(hawbsCollection.AddNew());

			AssertContainsExactElementsInAnyOrder("Post-Condition", expectedHawbs, hawbsCollection);
		}

		CusHAWB CreateHawb(ForwardingShipment shipment)
		{
			var hawb = Factory.New<CusHAWB>();
			hawb.CS_JS = shipment.PK;
			hawb.CS_ApplicationCode = ApplicationCodeList.Codes.GbCcsuk;
			var mawb = Factory.New<CusMAWB>();
			hawb.CS_CM = mawb.PK;
			return hawb;
		}
	}
}
