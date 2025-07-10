using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CountryDefaultLanguageBusinessObjectCollectionRegistryItem : StronglyTypedRegistryItem<CountryDefaultLanguageBusinessObjectCollection>
	{
		public CountryDefaultLanguageBusinessObjectCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CountryDefaultLanguageBusinessObjectCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CountryDefaultLanguageRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
