using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CusCAeMHDocAddressSynchroniserTest : TestCaseWithFactory
	{
		public void TestCusCAeMHJobDocAddressSynchroniser()
		{
			var helper = new CusCAeMHTestHelper(Factory);
			var shipment = helper.Shipment;
			var house = helper.House;
			var docAddress = house.DocAddresses.AddNew();
			var synchronsier = new CusCAeMHDocAddressSynchroniser(docAddress, shipment.ConsigneeDocumentaryAddress);
			synchronsier.SetEnabled(true, false);
			synchronsier.Synchronise();
			AssertEquals(DocAddressTypes.Codes.ConsigneeDocumentaryAddress, docAddress.E2_AddressType);
			AssertEquals(shipment.ConsigneeDocumentaryAddress.E2_OA_Address, docAddress.E2_OA_Address);
		}
	}
}
