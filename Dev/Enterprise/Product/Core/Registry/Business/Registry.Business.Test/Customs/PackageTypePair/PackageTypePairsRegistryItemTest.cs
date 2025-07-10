using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(PackageTypePairsRegistryItemForTest))]
	sealed class PackageTypePairsRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<PackageTypePairCollection<PackageTypePairForTest>>
	{
		protected override StronglyTypedRegistryItem<PackageTypePairCollection<PackageTypePairForTest>, PackageTypePairCollection<PackageTypePairForTest>> GetNewRegistryItem()
		{
			return new PackageTypePairsRegistryItemForTest("", null, null, null, RegistryStorageFlags.System, new PackageTypePairCollectionForTest());
		}
	}
}
