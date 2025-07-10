using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs.US
{
	public sealed class USPackageTypePairsRegistryItem : PackageTypePairsRegistryItem<USPackageTypePair>
	{
		public USPackageTypePairsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new USPackageTypesRegistryDataType(), storage, RegistryOptions.Default))
		{
		}

		public USPackageTypePairsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, USPackageTypePairCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new USPackageTypesRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
