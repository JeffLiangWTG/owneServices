using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class IntRegistryItem : StronglyTypedRegistryItem<int>
	{
		public IntRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, 0)
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, 0)
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, int defaultValue)
			: this(name, category, caption, hint, null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public IntRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, int defaultValue, params MultilingualString[] categories)
			: base(new RegistryItemImpl(name, caption, hint, new IntRegistryDataType(), null, storage, RegistryOptions.Default, defaultValue, false, categories))
		{
		}

		public IntRegistryItem(string name, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int defaultValue, params MultilingualString[] categories)
			: base(new RegistryItemImpl(name, caption, hint, new IntRegistryDataType(), null, storage, options, defaultValue, false, categories))
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int defaultValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue)
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, int defaultValue, int minValue, int maxValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue, minValue, maxValue)
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NumericRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, int defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IntRegistryDataType(), editorInfo, storage, options, defaultValue))
		{
		}

		public IntRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NumericRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, int defaultValue, int minValue, int maxValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IntRegistryDataType(minValue, maxValue), editorInfo, storage, options, defaultValue))
		{
		}
	}
}
