using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DefaultNumberOfDecimalsRegistryItem))]
	sealed class DefaultNumberOfDecimalsRegistryItemTest : StronglyTypedRegistryItemTestCase<DefaultNumberOfDecimalsCollection>
	{
		protected override StronglyTypedRegistryItem<DefaultNumberOfDecimalsCollection, DefaultNumberOfDecimalsCollection> GetNewRegistryItem()
		{
			return new DefaultNumberOfDecimalsRegistryItem("", null, null, null, RegistryStorageFlags.System, new DefaultNumberOfDecimalsCollection(Module.Freight));
		}
	}
}
