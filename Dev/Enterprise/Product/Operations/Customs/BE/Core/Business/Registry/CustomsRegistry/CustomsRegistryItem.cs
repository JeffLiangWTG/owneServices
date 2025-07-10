using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.BE.Business;

public class CustomsRegistryItem : StronglyTypedRegistryItem<CustomsRegistryCollection>
{
	public CustomsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CustomsRegistryCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new CustomsRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
	{
	}
}
