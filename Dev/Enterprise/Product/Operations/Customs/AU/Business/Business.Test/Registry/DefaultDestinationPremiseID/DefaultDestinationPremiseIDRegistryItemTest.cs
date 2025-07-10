using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultDestinationPremiseIDRegistryItem))]
	sealed class DefaultDestinationPremiseIDRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultDestinationPremiseIDCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultDestinationPremiseIDCollection, DefaultDestinationPremiseIDCollection> GetNewRegistryItem()
		{
			return new DefaultDestinationPremiseIDRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
