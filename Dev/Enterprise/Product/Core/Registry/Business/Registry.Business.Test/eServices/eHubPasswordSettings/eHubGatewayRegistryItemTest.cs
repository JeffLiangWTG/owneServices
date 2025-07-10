using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(eHubGatewayRegistryItem))]
	sealed class eHubGatewayRegistryItemTest : StronglyTypedRegistryItemTestCase<string>
	{
		protected override StronglyTypedRegistryItem<string, string> GetNewRegistryItem()
		{
			return new eHubGatewayRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.NotCached, "", isProduction: true);
		}
	}
}
