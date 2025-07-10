using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHDocAddressCollectionSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHDocAddressCollectionSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;

			var synchroniser = new CusCAeMHDocAddressCollectionSynchroniser(shipment, house);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals(4, house.DocAddresses.Count);
			AssertEquals(shipment.ConsigneeDocumentaryAddress.E2_OA_Address, house.DocAddresses.Find(DocAddressTypes.Codes.ConsigneeDocumentaryAddress).E2_OA_Address);
			AssertEquals(shipment.ConsignorDocumentaryAddress.E2_OA_Address, house.DocAddresses.Find(DocAddressTypes.Codes.ConsignorDocumentaryAddress).E2_OA_Address);
			AssertEquals(shipment.ConsigneeDeliveryAddress.E2_OA_Address, house.DocAddresses.Find(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress).E2_OA_Address);
			AssertEquals(shipment.NotifyPartyDocumentaryAddress.E2_OA_Address, house.DocAddresses.Find(DocAddressTypes.Codes.NotifyParty).E2_OA_Address);

			AssertEquals("CONSIGNEE ADDRESS LINE 1", house.DocAddresses.Find(DocAddressTypes.Codes.ConsigneeDocumentaryAddress).E2_Address1);
			AssertEquals("CONSIGNOR ADDRESS LINE 1", house.DocAddresses.Find(DocAddressTypes.Codes.ConsignorDocumentaryAddress).E2_Address1);
			AssertEquals("DELIVERY 1 ADDRESS LINE 1", house.DocAddresses.Find(DocAddressTypes.Codes.ConsigneePickupDeliveryAddress).E2_Address1);
			AssertEquals("ADDRESS LINE 1", house.DocAddresses.Find(DocAddressTypes.Codes.NotifyParty).E2_Address1);

			var docAddressPKs = house.DocAddresses.Cast<CAeMHDocAddress>().Select(x => x.PK);
			Factory.Save();

			var factory = new BusinessObjectFactory();
			house = factory.Load<CusCAeMHHouse>(house.PK);
			shipment = factory.Load<ForwardingShipment>(shipment.PK);
			synchroniser = new CusCAeMHDocAddressCollectionSynchroniser(shipment, house);
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise();

			AssertEquals(4, house.DocAddresses.Count);
			foreach (var docAddressPK in docAddressPKs)
			{
				AssertNotNull("JobDocAddresses should NOT be recreated", house.DocAddresses.Cast<CAeMHDocAddress>().FirstOrDefault(x => x.PK == docAddressPK));
			}

			var testAddress = house.DocAddresses.AddNew();
			testAddress.E2_AddressType = DocAddressTypes.Codes.ImportBroker;
			synchroniser.Synchronise();
			AssertEquals(5, house.DocAddresses.Count);

			shipment.DocAddresses.RemoveAndDeleteAll();
			AssertEquals(0, shipment.DocAddresses.Count);
			AssertEquals(1, house.DocAddresses.Count);

			var testAddress2 = house.DocAddresses.AddNew();
			testAddress2.E2_AddressType = DocAddressTypes.Codes.Carrier;

			house.DocAddresses.AddNew();

			factory.Save();
			AssertEquals(6, house.DocAddresses.Count);
		}
	}
}
