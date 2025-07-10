using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem : CodeDescriptionBoolDisallowNewCodeReadOnlyRegistryItem
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}
	}

	class CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolDisallowNewCodeReadOnlyValidationCollection>
	{
		public CodeDescriptionBoolDisallowNewCodeReadOnlyValidationRegistryDataType()
		{
		}
	}
}
