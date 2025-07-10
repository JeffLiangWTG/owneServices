using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DecimalArrayRegistryItem : StronglyTypedRegistryItem<decimal[]>
	{
		public DecimalArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, System.Array.Empty<decimal>())
		{
		}

		public DecimalArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: this(name, category, caption, hint, storage, options, System.Array.Empty<decimal>())
		{
		}

		public DecimalArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, decimal[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DecimalArrayRegistryDataType(), storage, defaultValue))
		{
		}

		public DecimalArrayRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, decimal[] defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DecimalArrayRegistryDataType(), storage, options, defaultValue))
		{
		}
		public new DecimalArrayRegistryDataType DataType
		{
			get { return (DecimalArrayRegistryDataType)base.DataType; }
			set { base.DataType = value; }
		}
	}
}
