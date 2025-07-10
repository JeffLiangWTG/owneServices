using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(eHubMessagingRegistry))]
namespace Enterprise.RemotePrinting.Server.Testing
{
	class EHubTestGatewayServerAddressRegistryItemTest : TestCaseWithFactory
	{
		public void TestDefaultValue()
		{
			var testItem = new EHubTestGatewayServerAddressRegistryItemForTesting();
			AssertEquals("EHubTestGatewayServerAddressRegistryItem should return correct default value.", eHubMessagingRegistry.eHubGateway.TestServerName, testItem.DefaultValueExposed);
		}
	}

	class EHubTestGatewayServerAddressRegistryItemForTesting : EHubTestGatewayServerAddressRegistryItem
	{
		public string DefaultValueExposed => base.DefaultValue;
	}
}
