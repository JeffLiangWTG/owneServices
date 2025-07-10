using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WebServicesConfigRegistryItem : StronglyTypedRegistryItem<WebServicesConfigCollection>
	{
		public WebServicesConfigRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			WebServicesConfigCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebServicesConfigRegistryDataType(new WebServicesConfigCollection()), storage, options, defaultValue))
		{
		}
	}
}
