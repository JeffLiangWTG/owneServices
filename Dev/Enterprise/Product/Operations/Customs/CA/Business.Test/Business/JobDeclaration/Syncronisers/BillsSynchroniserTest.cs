using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class BillsSynchroniserTest : TestCaseWithFactory
	{
		public void TestSychronise()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "H098342";
			shipment.JS_OuterPacks = 100;
			shipment.JS_F3_NKPackType = "PKG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Pack UQ", "PK", declaration.Bills[0].CU_PackType);

			declaration.CA_ServiceOption = "";
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("Pack UQ", "PKG", declaration.Bills[0].CU_PackType);
		}
	}
}
