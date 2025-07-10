using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Test
{
	public class EuPackingSynchroniserTest : TestCaseWithFactory
	{
		public void TestPackConversion()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "HOUSE123";
			var pack = shipment.OuterPackLines.AddNew();
			pack.JL_F3_NKPackType = "BAG";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals("BG", declaration.Packages[0].CW_PackType);
		}
	}
}
