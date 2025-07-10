using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ZArchitecture.Environment.RegistryItemImplWithDynamicDefaultValue;

namespace Enterprise.ZArchitecture.Environment
{
	internal class OrgListRegistryItem : CodeDescriptionPairListRegistryItem
	{
		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, caption, hint, new CodeDescriptionPairListEditorInfo(), defaultValue)
		{
		}

		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlag, ReadOnlyCodeDescriptionPairList defaultValue)
			: this(name, caption, hint, new CodeDescriptionPairListEditorInfo(), storageFlag, defaultValue)
		{
		}

		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, CodeDescriptionPairListEditorInfo editorInfo, ReadOnlyCodeDescriptionPairList defaultValue)
			: base(name, RawDataRegistry.Categories.Organizations_CodeLists, caption, hint, 3, editorInfo, RegistryStorageFlags.System, RegistryOptions.Default, defaultValue, false)
		{
		}

		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, CodeDescriptionPairListEditorInfo editorInfo, RegistryStorageFlags storageFlag, ReadOnlyCodeDescriptionPairList defaultValue)
			: base(name, RawDataRegistry.Categories.Organizations_CodeLists, caption, hint, 3, editorInfo, storageFlag, RegistryOptions.Default, defaultValue, false)
		{
		}

		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue)
			: base(name, RawDataRegistry.Categories.Organizations_CodeLists, caption, hint, 3, RegistryStorageFlags.System, options, defaultValue)
		{
		}

		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlag, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue)
			: base(name, RawDataRegistry.Categories.Organizations_CodeLists, caption, hint, 3, storageFlag, options, defaultValue)
		{
		}

		public OrgListRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storageFlag, RegistryOptions options, ReadOnlyCodeDescriptionPairList defaultValue, DefaultValueGetter defaultValueGetter)
			: base(new RegistryItemImplWithDynamicDefaultValue(name, new[] { RawDataRegistry.Categories.Organizations_CodeLists }, caption, hint, new CodeDescriptionPairListRegistryDataType(3), new CodeDescriptionPairListEditorInfo(), storageFlag, options, defaultValueGetter), true, defaultValue)
		{
		}
	}
}
