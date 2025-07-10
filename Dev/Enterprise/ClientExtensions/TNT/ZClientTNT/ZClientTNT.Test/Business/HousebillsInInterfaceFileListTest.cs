using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Client.TNT.Testing
{
	public class HousebillsInInterfaceFileListTest : TestCaseWithFactory
	{
		public void TestHousebillsInInterfaceFileList()
		{
			ZString consigneeRef = "C00001000";
			HousebillsInInterfaceFileList.Instance.Clear();
			AssertEquals("PreCondition: 0 shipments should be linked", 0, HousebillsInInterfaceFileList.Instance.NoOfShipmentLinkedToConsol(consigneeRef));
			ZGuid shipmentPK1 = Factory.New<ForwardingShipment>().PK;
			ZGuid shipmentPK2 = Factory.New<ForwardingShipment>().PK;
			HousebillsInInterfaceFileList.Instance.Add(consigneeRef, shipmentPK1);
			AssertEquals("List should contain " + shipmentPK1, true, HousebillsInInterfaceFileList.Instance.Contains(consigneeRef, shipmentPK1));
			AssertEquals("1 shipment should be linked", 1, HousebillsInInterfaceFileList.Instance.NoOfShipmentLinkedToConsol(consigneeRef));
			HousebillsInInterfaceFileList.Instance.Add(consigneeRef, shipmentPK2);
			AssertEquals("List should contain " + shipmentPK2, true, HousebillsInInterfaceFileList.Instance.Contains(consigneeRef, shipmentPK2));
			AssertEquals("2 shipments should be linked", 2, HousebillsInInterfaceFileList.Instance.NoOfShipmentLinkedToConsol(consigneeRef));
			HousebillsInInterfaceFileList.Instance.Clear();
			AssertEquals("0 shipments should be linked", 0, HousebillsInInterfaceFileList.Instance.NoOfShipmentLinkedToConsol(consigneeRef));
		}
	}
}
