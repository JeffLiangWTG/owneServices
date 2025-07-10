using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class ShipmentSortComparerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCompareOnNotForwardingShipment()
		{
			Consol.Containers.AddNew();
			Consol.Containers.AddNew();
			AssertEquals(2, Consol.Containers.Count);
			Consol.Containers.Sort(new ShipmentHBLSorter());
		}

		public void TestCompareOnHBL()
		{
			Shipment1.JS_HouseBill = "HSE123";
			Shipment2.JS_HouseBill = "HSE321";

			DocForwardingConsol consolWrapper = DocForwardingConsol.New(Consol, Factory);
			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			AssertEquals("Shipment1", consolWrapper.Shipments[0].GoodsDescription);
			AssertEquals("Shipment2", consolWrapper.Shipments[1].GoodsDescription);

			Shipment1.JS_HouseBill = "HSE555";
			Shipment2.JS_HouseBill = "HSE444";
			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			AssertEquals("Shipment2", consolWrapper.Shipments[0].GoodsDescription);
			AssertEquals("Shipment1", consolWrapper.Shipments[1].GoodsDescription);

			Shipment1.JS_HouseBill = "82";
			Shipment2.JS_HouseBill = "231";
			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			AssertEquals("Shipment1", consolWrapper.Shipments[0].GoodsDescription);
			AssertEquals("Shipment2", consolWrapper.Shipments[1].GoodsDescription);
		}

		public void TestCompareOnShipmentNumber()
		{
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(Consol, Factory);

			Shipment1.JS_UniqueConsignRef = "S000976";
			Shipment2.JS_UniqueConsignRef = "S000890";
			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			AssertEquals("Shipment2", consolWrapper.Shipments[0].GoodsDescription);
			AssertEquals("Shipment1", consolWrapper.Shipments[1].GoodsDescription);

			Shipment2.JS_HouseBill = "222";
			Shipment1.JS_UniqueConsignRef = "S000976";
			Shipment2.JS_UniqueConsignRef = "S000890";
			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			AssertEquals("Shipment1", consolWrapper.Shipments[0].GoodsDescription);
			AssertEquals("Shipment2", consolWrapper.Shipments[1].GoodsDescription);
		}

		public void TestCompareFiveShipments()
		{
			var shipment3 = Factory.New<ForwardingShipment>();
			var shipment4 = Factory.New<ForwardingShipment>();
			var shipment5 = Factory.New<ForwardingShipment>();
			shipment3.JS_GoodsDescription = "Shipment3";
			shipment4.JS_GoodsDescription = "Shipment4";
			shipment5.JS_GoodsDescription = "Shipment5";

			Shipment1.JS_UniqueConsignRef = "S000111";
			Shipment2.JS_UniqueConsignRef = "S000222";
			shipment3.JS_UniqueConsignRef = "S000333";
			shipment4.JS_UniqueConsignRef = "S000444";
			shipment5.JS_UniqueConsignRef = "S000555";

			Shipment1.JS_HouseBill = "1";
			Shipment2.JS_HouseBill = "11";
			shipment3.JS_HouseBill = "2";
			shipment4.JS_HouseBill = "";
			shipment5.JS_HouseBill = "123";

			Consol.Shipments.Add(shipment3);
			Consol.Shipments.Add(shipment4);
			Consol.Shipments.Add(shipment5);

			DocForwardingConsol consolWrapper = DocForwardingConsol.New(Consol, Factory);
			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			DocShipmentCollection collection = consolWrapper.Shipments;
			AssertEquals("Shipment4", collection[0].GoodsDescription);
			AssertEquals("Shipment1", collection[1].GoodsDescription);
			AssertEquals("Shipment3", collection[2].GoodsDescription);
			AssertEquals("Shipment2", collection[3].GoodsDescription);
			AssertEquals("Shipment5", collection[4].GoodsDescription);

			Shipment1.JS_HouseBill = "1";
			Shipment2.JS_HouseBill = "";
			shipment3.JS_HouseBill = "2";
			shipment4.JS_HouseBill = "";
			shipment5.JS_HouseBill = "123";

			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			collection = consolWrapper.Shipments;
			AssertEquals("Shipment2", collection[0].GoodsDescription);
			AssertEquals("Shipment4", collection[1].GoodsDescription);
			AssertEquals("Shipment1", collection[2].GoodsDescription);
			AssertEquals("Shipment3", collection[3].GoodsDescription);
			AssertEquals("Shipment5", collection[4].GoodsDescription);

			Shipment1.JS_HouseBill = "1TEST";
			Shipment2.JS_HouseBill = "";
			shipment3.JS_HouseBill = "2";
			shipment4.JS_HouseBill = "0 BASED NUMBER";
			shipment5.JS_HouseBill = "123";

			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			collection = consolWrapper.Shipments;
			AssertEquals("Shipment2", collection[0].GoodsDescription);
			AssertEquals("Shipment4", collection[1].GoodsDescription);
			AssertEquals("Shipment1", collection[2].GoodsDescription);
			AssertEquals("Shipment3", collection[3].GoodsDescription);
			AssertEquals("Shipment5", collection[4].GoodsDescription);

			Shipment1.JS_HouseBill = "WK56789";
			Shipment2.JS_HouseBill = "WK12345";
			shipment3.JS_HouseBill = "2WK3948";
			shipment4.JS_HouseBill = "0WK9385";
			shipment5.JS_HouseBill = "";

			consolWrapper.Shipments.Sort(new ShipmentHBLSorter());
			collection = consolWrapper.Shipments;
			AssertEquals("Shipment5", collection[0].GoodsDescription);
			AssertEquals("Shipment4", collection[1].GoodsDescription);
			AssertEquals("Shipment3", collection[2].GoodsDescription);
			AssertEquals("Shipment2", collection[3].GoodsDescription);
			AssertEquals("Shipment1", collection[4].GoodsDescription);
		}

		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		ForwardingConsol Consol;

		protected override void SetUp()
		{
			Shipment1 = Factory.New<ForwardingShipment>();
			Shipment2 = Factory.New<ForwardingShipment>();
			Shipment1.JS_GoodsDescription = "Shipment1";
			Shipment2.JS_GoodsDescription = "Shipment2";
			Consol = Factory.New<ForwardingConsol>();
			Consol.Shipments.Add(Shipment1);
			Consol.Shipments.Add(Shipment2);
			base.SetUp();
		}
	}
}
