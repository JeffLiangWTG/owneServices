using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusHAWBLoadTest : TestCaseWithFactory
	{
		public void TestLoadDetachedCusHAWB()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var cusHAWB = Factory.New<CusHAWB>();
			cusHAWB.CS_JS = shipment.PK;
			cusHAWB.CS_ApplicationCode = "CMR";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var shipment2 = factory2.Load<ForwardingShipment>(shipment.PK);
			var cusHAWB2 = CusHAWBBase.LoadFromQuery(new ZQuery(CusHAWBSchema.CS_JS, shipment2.PK), shipment.Factory);

			AssertEquals(cusHAWB.PK, cusHAWB2.PK);
		}
	}
}
