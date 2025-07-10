using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryItem : CodeDescriptionBoolDisallowNewRegistryItem
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewCodeReadOnlyCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewCodeReadOnlyCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}

		protected CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryItem(RegistryItemImpl registryItemImpl, CodeDescriptionBoolRegistryEditorInfo editorInfo)
			: base(registryItemImpl, editorInfo)
		{
		}
	}

	class CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolDisallowNewCodeReadOnlyCollection>
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryDataType()
		{
		}
	}
}
