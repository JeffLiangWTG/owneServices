using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class WhaAreaFilterProvideTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			BusinessObject whs = (BusinessObject)Factory.New<Enterprise.Warehouse.Integration.IWhsWarehouse>();
			Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();

			LookupField master = new LookupField(Factory);
			LookupField detail = new LookupField(Factory);
			master.Value = whs.PK.ToGuid();

			WhsAreaFilterProvider provider = new WhsAreaFilterProvider(master, detail);
			BusinessObject[] whss = (BusinessObject[])Factory.Load<Enterprise.Warehouse.Integration.IWhsWarehouse>(provider.Filter);

			AssertEquals(1, whss.Length);
			AssertEquals(whs, whss[0]);
		}
	}
}
