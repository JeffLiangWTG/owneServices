using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class FilterLayoutCodePairRegistryItem : StronglyTypedRegistryItem<string>
	{
		public FilterLayoutCodePairRegistryItem(IRegistryItem inner)
			: base(inner)
		{
		}

		public FilterLayoutCodePairRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, bool allowBlank, bool validate, ComboBoxFilterLayoutRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new StringRegistryDataType(), editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}
	}
}
