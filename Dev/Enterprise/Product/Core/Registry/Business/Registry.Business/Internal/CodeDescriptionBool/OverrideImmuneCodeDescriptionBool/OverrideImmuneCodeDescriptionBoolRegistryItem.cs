using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class OverrideImmuneCodeDescriptionBoolRegistryItem : CodeDescriptionBoolRegistryItem
	{
		public OverrideImmuneCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, OverrideImmuneCodeDescriptionBoolCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OverrideImmuneCodeDescriptionBoolRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public OverrideImmuneCodeDescriptionBoolRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, OverrideImmuneCodeDescriptionBoolCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OverrideImmuneCodeDescriptionBoolRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}

		internal class OverrideImmuneCodeDescriptionBoolRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OverrideImmuneCodeDescriptionBoolCollection>
		{
			public OverrideImmuneCodeDescriptionBoolRegistryDataType()
			{
			}
		}
	}
}
