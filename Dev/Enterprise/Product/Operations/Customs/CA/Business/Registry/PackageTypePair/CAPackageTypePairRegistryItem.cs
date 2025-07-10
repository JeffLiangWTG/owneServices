using Enterprise.Integration;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	public class CAPackageTypePairsRegistryItem : PackageTypePairsRegistryItem<CAPackageTypePair>
	{
		public CAPackageTypePairsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CAPackageTypesRegistryDataType(), storage, RegistryOptions.Default))
		{
		}

		public CAPackageTypePairsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CAPackageTypePairCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CAPackageTypesRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
