using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CodeDescriptionBoolWithSingleTrueRegistryItem : CodeDescriptionBoolRegistryItem
	{
		public CodeDescriptionBoolWithSingleTrueRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, CodeDescriptionBoolRegistryEditorInfo editorInfo, CodeDescriptionBoolWithSingleTrueCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CodeDescriptionBoolWithSingleTrueRegistryDataType(), storage, options, defaultValue), editorInfo)
		{
		}

		internal class CodeDescriptionBoolWithSingleTrueRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CodeDescriptionBoolWithSingleTrueCollection>
		{
			public CodeDescriptionBoolWithSingleTrueRegistryDataType()
			{
			}
		}
	}
}
