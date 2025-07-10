using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WebThemeUrlRegistryItem : StronglyTypedRegistryItem<WebThemeUrlCollection>
	{
		public WebThemeUrlRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				WebThemeUrlCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebThemeUrlRegistryDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WebUrlThemeRegistryItemEditor, Enterprise.Registry.GUI")]
	public class WebThemeUrlRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WebThemeUrlCollection>
	{
	}
}
