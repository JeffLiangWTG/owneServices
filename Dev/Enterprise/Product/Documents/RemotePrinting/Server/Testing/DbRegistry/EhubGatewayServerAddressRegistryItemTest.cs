using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.RemotePrinting.Server.RPSCore;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(eHubMessagingRegistry))]
namespace Enterprise.RemotePrinting.Server.Testing
{
	class EhubGatewayServerAddressRegistryItemTest : TestCaseWithFactory
	{
		public void TestDefaultEHubGatewayServerAddress()
		{
			var testItem = new EhubGatewayServerAddressRegistryItemForTesting();
			AssertEquals("EhubGatewayServerAddressRegistryItem should have the correct default value.", eHubMessagingRegistry.eHubGateway.ServerName, testItem.DefaultValueExposed);
		}

		class EhubGatewayServerAddressRegistryItemForTesting : EhubGatewayServerAddressRegistryItem
		{
			public string DefaultValueExposed => base.DefaultValue;
		}
	}
}
