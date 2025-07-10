using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ReleaseTypesRegistryItem))]
	sealed class ReleaseTypesRegistryItemTest : StronglyTypedRegistryItemTestCase<ReleaseTypes>
	{
		protected override StronglyTypedRegistryItem<ReleaseTypes, ReleaseTypes> GetNewRegistryItem()
		{
			return new ReleaseTypesRegistryItem("", null, null, null, RegistryStorageFlags.System, new ReleaseTypes());
		}
	}
}
