using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class StringRegistryItem : StronglyTypedRegistryItem<string>
	{
		public StringRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, RegistryStorageFlags.All, RegistryOptions.Default, null, true)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, RegistryOptions.Default, null, true)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, RegistryStorageFlags storage)
			: this(name, category, caption, hint, dataType, null, storage, Enterprise.Integration.RegistryOptions.Default, null, true)
		{
		}

		public StringRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, categories, caption, hint, dataType, null, storage, options, null, true)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, dataType, null, storage, options, null, true)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, RegistryStorageFlags storage, string defaultValue)
		: this(name, category, caption, hint, dataType, null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, options, null, true)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultValue)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: this(name, category, caption, hint, new StringRegistryDataType(), null, storage, options, defaultValue)
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, editorInfo, storage, options, defaultValue))
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, dataType, editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}

		public StringRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, caption, hint, dataType, editorInfo, storage, options, defaultValue, useDefaultDefaultValue, categories))
		{
		}

		public StringRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, StringRegistryDataType dataType, TextRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, dataType, editorInfo, storage, options, defaultValue, false, categories))
		{
		}

		public StringRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, EmailDestinationOverrideDataType dataType, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, dataType, null, storage, options, null, true)
		{
		}
	}
}
