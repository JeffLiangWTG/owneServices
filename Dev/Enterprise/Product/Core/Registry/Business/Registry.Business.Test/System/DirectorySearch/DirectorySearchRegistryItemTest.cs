using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DirectorySearchRegistryItem))]
	sealed class DirectorySearchRegistryItemTest : StronglyTypedRegistryItemTestCase<IDirectorySearch, DirectorySearch>
	{
		protected override StronglyTypedRegistryItem<IDirectorySearch, DirectorySearch> GetNewRegistryItem()
		{
			return new DirectorySearchRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
