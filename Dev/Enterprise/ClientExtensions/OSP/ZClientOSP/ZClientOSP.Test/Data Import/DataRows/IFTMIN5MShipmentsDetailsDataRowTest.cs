using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MShipmentsDetailsDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "SD MI08105981          1080506PPD   SEA   ME    10805071516mediumremark                                                      AT434";
			IFTMIN5MShipmentsDetailsDataRow dataRow = new IFTMIN5MShipmentsDetailsDataRow(testString);
			AssertEquals("MI08105981", dataRow.ShipmentNumber);
			AssertEquals(new ZDateTime(2008, 5, 6), dataRow.ShipmentDate);
			AssertEquals("PPD", dataRow.TermsOfDelivery);
			AssertEquals("SEA", dataRow.TransportType);
			AssertEquals("ME", dataRow.DepartmentCode);
			AssertEquals(new ZDateTime(2008, 5, 7, 15, 16, 0), dataRow.RequestedDeliveryDateTime);
			AssertEquals("medium", dataRow.DeliveryPriority);
			AssertEquals("remark", dataRow.Remark1);
			AssertEquals("A", dataRow.DeliveryOnWheels);
			AssertEquals("T", dataRow.DeliveryLimitCompulsory);
			AssertEquals(434, dataRow.BarcodesQuantity);
		}
	}
}
