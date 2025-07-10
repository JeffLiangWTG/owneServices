using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class StringArrayRegistryItem : StronglyTypedRegistryItem<string[]>
	{
		public StringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, System.Array.Empty<string>())
		{
		}

		public StringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, System.Array.Empty<string>())
		{
		}

		public StringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelimitedStringArrayRegistryDataType(), storage, defaultValue))
		{
		}

		public StringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelimitedStringArrayRegistryDataType(), storage, options, defaultValue))
		{
		}

		public StringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string[] defaultValue, IRegistryEditorInfo editorInfo)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelimitedStringArrayRegistryDataType(), editorInfo, storage, options, defaultValue))
		{
		}

		public new DelimitedStringArrayRegistryDataType DataType
		{
			get { return (DelimitedStringArrayRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}
	}
}
