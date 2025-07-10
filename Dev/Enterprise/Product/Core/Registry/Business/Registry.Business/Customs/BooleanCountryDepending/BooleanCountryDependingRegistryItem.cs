using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	public sealed class BooleanCountryDependingRegistryItem : StronglyTypedRegistryItem<bool>
	{
		public BooleanCountryDependingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, bool defaultValue, string[] explicitCountries)
			: base(new BooleanCountryDependingRegistryItemImpl(name, category, caption, hint, new BooleanRegistryDataType(), storage, RegistryOptions.Default, defaultValue, explicitCountries, !defaultValue))
		{
		}

		public BooleanCountryDependingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, bool defaultValue, string[] explicitCountries)
			: base(new BooleanCountryDependingRegistryItemImpl(name, category, caption, hint, new BooleanRegistryDataType(), storage, options, defaultValue, explicitCountries, !defaultValue))
		{
		}
	}
}
