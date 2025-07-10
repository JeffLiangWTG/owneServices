using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class RatesServiceUrlRegistryItem : StronglyTypedRegistryItem<string>
	{
		public RatesServiceUrlRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public RatesServiceUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public RatesServiceUrlRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RatesServiceUrlRegistryDataType(), null, storage, options, defaultValue, false))
		{
		}

		public RatesServiceUrlRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new RatesServiceUrlRegistryDataType(), editorInfo, storage, options, defaultValue, false, categories))
		{
		}
	}
}
