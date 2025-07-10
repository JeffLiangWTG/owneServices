using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class ColorThemeRegistryItem : StronglyTypedRegistryItem<ColorThemeSelector>
	{
		public ColorThemeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ColorThemeSelector defaultValue)
			: base(
				new RegistryItemImpl(
				name, category, caption, hint, new ColorThemeRegistryDataType(),
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				defaultValue)
				)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ColorThemeSelectorRegistryItemEditor, Enterprise.Registry.GUI")]
	sealed class ColorThemeRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ColorThemeSelector>
	{
	}
}
