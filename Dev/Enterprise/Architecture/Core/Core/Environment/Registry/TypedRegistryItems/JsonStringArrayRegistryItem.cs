using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class JsonStringArrayRegistryItem : StronglyTypedRegistryItem<string[]>
	{
		public JsonStringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, System.Array.Empty<string>())
		{
		}

		public JsonStringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, System.Array.Empty<string>())
		{
		}

		public JsonStringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JsonStringArrayRegistryDataType(), storage, defaultValue))
		{
		}

		public JsonStringArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new JsonStringArrayRegistryDataType(), storage, options, defaultValue))
		{
		}

		public new JsonStringArrayRegistryDataType DataType
		{
			get { return (JsonStringArrayRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}
	}
}
