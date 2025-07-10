using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ScanHouseBillBillProviderExtensionsTest : TestCaseWithFactory
	{
		public void TestIsHVLVShipment()
		{
			var houseBill = Factory.New<CusHAWB>();
			Assert(!houseBill.IsHVLVShipment());
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValueLegacy;
			houseBill.CS_JS = shipment.PK;
			Assert(houseBill.IsHVLVShipment());
		}
	}
}
