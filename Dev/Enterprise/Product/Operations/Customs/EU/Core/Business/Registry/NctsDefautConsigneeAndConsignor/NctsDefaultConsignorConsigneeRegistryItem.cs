using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	public class NctsDefaultConsignorConsigneeRegistryItem : StronglyTypedRegistryItem<NctsDefaultConsignorConsignee>
	{
		public NctsDefaultConsignorConsigneeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NctsDefaultConsignorConsignee defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NctsDefaultConsignorConsigneeRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
