using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolDisallowNewRegistryItem : CodeDescriptionBoolRegistryItem
	{
		public CodeDescriptionBoolDisallowNewRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolDisallowNewRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public CodeDescriptionBoolDisallowNewRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolDisallowNewCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolDisallowNewRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}

		protected CodeDescriptionBoolDisallowNewRegistryItem(RegistryItemImpl registryItemImpl, CodeDescriptionBoolRegistryEditorInfo editorInfo)
			: base(registryItemImpl, editorInfo)
		{
		}

		public override IEnumerable<string> GetCaptions(CodeDescriptionBoolCollection value)
		{
			foreach (RegistryBusinessObject item in value)
			{
				yield return item.EnglishDescription;
			}
		}
	}

	class CodeDescriptionBoolDisallowNewRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolDisallowNewCollection>
	{
		public CodeDescriptionBoolDisallowNewRegistryDataType()
		{
		}
	}

	class CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolDisallowNewWithDefaultDisabledCollection>
	{
		public CodeDescriptionBoolDisallowNewWithDefaultDisabledRegistryDataType()
		{
		}
	}
}
