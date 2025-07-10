using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.SWL.Business.Testing
{
	public class ShipnetCarrierFilterTestCase : ShipnetTestCase
	{
		public void TestGetFilter()
		{
			ShipnetCarrierFilter carrierFilter = new ShipnetCarrierFilter();
			ZQuery filter = carrierFilter.GetFilter();
			AssertEquals("No Result Filter", true, filter.IsNoResultQuery);
			ShipnetCarrier.Factory.Save();
			filter = carrierFilter.GetFilter();
			AssertEquals("Is not No Result Filter", false, filter.IsNoResultQuery);
			AssertEquals("Filter should contain ShipnetCarrier's PK: " + System.Environment.NewLine + filter.LiteralTextADO, true, filter.LiteralTextADO.Contains(ShipnetCarrier.PK.ToString()));
		}

		public void TestGetShipnetCarrierPKCollection()
		{
			ShipnetCarrierFilter carrierFilter = new ShipnetCarrierFilter();
			DynamicBusinessObjectCollection collection = carrierFilter.GetShipnetCarrierPKCollection();
			AssertEquals("Collection is empty", 0, collection.Count);
			ShipnetCarrier.Factory.Save();
			collection = carrierFilter.GetShipnetCarrierPKCollection();
			AssertEquals("Collection is not empty", 1, collection.Count);
			AssertEquals("Collection should contain ShipnetCarrier's PK", ShipnetCarrier.PK, collection[0][OrgCompanyDataSchema.OB_OH]);
		}
	}
}
