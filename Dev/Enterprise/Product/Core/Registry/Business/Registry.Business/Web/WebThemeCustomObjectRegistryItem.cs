using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class WebThemeCustomObjectRegistryItem : StronglyTypedRegistryItem<WebThemeCustomObjectCollection>
	{
		public WebThemeCustomObjectRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, WebThemeCustomObjectCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new WebThemeCustomObjectRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.WebThemeSelectorRegistryItemEditor, Enterprise.Registry.GUI")]
	public class WebThemeCustomObjectRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WebThemeCustomObjectCollection>
	{
	}
}
