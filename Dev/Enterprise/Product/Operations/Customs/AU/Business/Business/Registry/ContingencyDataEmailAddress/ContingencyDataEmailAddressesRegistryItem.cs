using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ContingencyDataEmailAddressesRegistryItem : StronglyTypedRegistryItem<ContingencyDataEmailAddressCollection>
	{
		public ContingencyDataEmailAddressesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ContingencyDataEmailAddressCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ContingencyDataEmailAddressesRegistryDataType(), storage, defaultValue))
		{
		}

		public ContingencyDataEmailAddressesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, ContingencyDataEmailAddressCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ContingencyDataEmailAddressesRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
