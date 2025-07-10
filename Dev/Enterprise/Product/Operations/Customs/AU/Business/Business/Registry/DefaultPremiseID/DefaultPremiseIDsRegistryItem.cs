using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DefaultPremiseIDsRegistryItem : StronglyTypedRegistryItem<DefaultPremiseIDCollection>
	{
		public DefaultPremiseIDsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultPremiseIDsRegistryDataType(), storage))
		{
		}

		public DefaultPremiseIDsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultPremiseIDsRegistryDataType(), storage, options))
		{
		}
	}
}
