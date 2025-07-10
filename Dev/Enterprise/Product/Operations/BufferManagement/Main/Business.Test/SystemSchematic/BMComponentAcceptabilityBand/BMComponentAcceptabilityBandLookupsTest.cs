using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentAcceptabilityBandLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComponents()
		{
			var acceptabilityBand = Factory.New<BMComponentAcceptabilityBand>();

			AssertEquals(0, acceptabilityBand.Lookups.Components.Count);

			Factory.NewWithValidTestData<BMComponent>();
			Factory.NewWithValidTestData<BMComponent>();

			AssertEquals(2, acceptabilityBand.Lookups.Components.Count);
		}
	}
}
