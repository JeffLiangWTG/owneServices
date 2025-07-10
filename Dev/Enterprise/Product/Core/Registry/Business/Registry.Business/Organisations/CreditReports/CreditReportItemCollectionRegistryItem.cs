using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CreditReportItemCollectionRegistryItem : StronglyTypedRegistryItem<CreditReportItemCollection>
	{
		public CreditReportItemCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions registryOption, CreditReportItemCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new CreditReportItemDataType(), storage, registryOption, defaultValue))
		{
		}
	}
}
