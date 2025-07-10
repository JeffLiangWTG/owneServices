using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DefaultDestinationPremiseIDRegistryItem : StronglyTypedRegistryItem<DefaultDestinationPremiseIDCollection>
	{
		public DefaultDestinationPremiseIDRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultDestinationPremiseIDRegistryDataType(), storage))
		{
		}
	}
}
