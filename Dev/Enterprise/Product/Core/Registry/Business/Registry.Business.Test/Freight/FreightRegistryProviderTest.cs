using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class FreightRegistryProviderTest : TestCaseWithFactory
	{
		public void TestEnableBoleroEBLIntegration()
		{
			var freightRegistryProvider = new FreightRegistryProvider();

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = true, GalileoEndPointUrl = "http://test.test", GalileoAudience = Guid.NewGuid().ToString(), GalileoTestEndPointUrl = "http://test.test", GalileoTestAudience = Guid.NewGuid().ToString() }))
			{
				Assert(freightRegistryProvider.EnableBoleroEBLIntegration);
			}

			using (FreightDataRegistry.Instance.EnableBoleroEBLIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new BoleroEBLConfiguration() { EnableEBLIntegration = false }))
			{
				Assert(!freightRegistryProvider.EnableBoleroEBLIntegration);
			}
		}
	}
}
