using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class EHubSendInterchangesToTestGatewayRegistryItemTest : TestCaseWithFactory
	{
		public void TestDefaultValue()
		{
			var testItem = new EHubSendInterchangesToTestGatewayRegistryItemForTesting();
			AssertEquals("EHubSendInterchangesToTestGatewayRegistryItem should return false without value inserted in DB.", false, testItem.DefaultValueExposed);
		}
	}

	class EHubSendInterchangesToTestGatewayRegistryItemForTesting : EHubSendInterchangesToTestGatewayRegistryItem
	{
		public bool DefaultValueExposed => base.DefaultValue;
	}
}
