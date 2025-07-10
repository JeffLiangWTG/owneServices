using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Registry.Testing
{
	[TestedType(typeof(DefaultFreightPercentagesRegistryItem))]
	sealed class DefaultFreightPercentagesRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultFreightPercentageCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultFreightPercentageCollection, DefaultFreightPercentageCollection> GetNewRegistryItem()
		{
			return new DefaultFreightPercentagesRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
