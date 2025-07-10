using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs.Testing
{
	sealed class PackageTypePairsRegistryItemForTest : PackageTypePairsRegistryItem<PackageTypePairForTest>
	{
		public PackageTypePairsRegistryItemForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PackageTypesRegistryDataTypeForTest(), storage, RegistryOptions.Default))
		{
		}

		public PackageTypePairsRegistryItemForTest(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PackageTypePairCollectionForTest defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PackageTypesRegistryDataTypeForTest(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
