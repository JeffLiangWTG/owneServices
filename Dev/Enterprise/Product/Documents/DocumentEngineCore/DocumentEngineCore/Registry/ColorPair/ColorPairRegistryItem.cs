using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public sealed class ColorPairRegistryItem : StronglyTypedRegistryItem<ColorPairSelector>
	{
		public ColorPairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlags, ColorPairSelector defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ColorPairRegistryDataType(), storageFlags, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.DocumentEngineCore.GUI.Registry.ColorPairSelectorRegistryItemEditor, Enterprise.DocumentEngineCore.GUI")]
	sealed class ColorPairRegistryDataType : NonPersistentBusinessObjectRegistryDataType<ColorPairSelector>
	{
	}
}
