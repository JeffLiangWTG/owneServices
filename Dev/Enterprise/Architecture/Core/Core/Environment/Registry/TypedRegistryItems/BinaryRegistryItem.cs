using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class BinaryRegistryItem : StronglyTypedRegistryItem<byte[]>
	{
		public BinaryRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public BinaryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, RegistryOptions.Default)
		{
		}

		public BinaryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, null, storage, options, null, true)
		{
		}

		public BinaryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, byte[] defaultValue)
			: this(name, category, caption, hint, null, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public BinaryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, byte[] defaultValue)
			: this(name, category, caption, hint, null, storage, options, defaultValue)
		{
		}

		public BinaryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, byte[] defaultValue)
			: this(name, category, caption, hint, editorInfo, storage, options, defaultValue, false)
		{
		}

		public BinaryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, byte[] defaultValue, bool useDefaultDefaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new BinaryRegistryDataType(), editorInfo, storage, options, defaultValue, useDefaultDefaultValue))
		{
		}
	}
}
