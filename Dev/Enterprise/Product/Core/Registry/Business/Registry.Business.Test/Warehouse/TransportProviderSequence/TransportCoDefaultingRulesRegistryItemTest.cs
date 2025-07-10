using Enterprise.Integration;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TransportCoDefaultingRulesRegistryItem))]
	sealed class TransportCoDefaultingRulesRegistryItemTest : StronglyTypedRegistryItemTestCase<TransportCoDefaultingRules>
	{
		protected override StronglyTypedRegistryItem<TransportCoDefaultingRules, TransportCoDefaultingRules> GetNewRegistryItem()
		{
			return new TransportCoDefaultingRulesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
