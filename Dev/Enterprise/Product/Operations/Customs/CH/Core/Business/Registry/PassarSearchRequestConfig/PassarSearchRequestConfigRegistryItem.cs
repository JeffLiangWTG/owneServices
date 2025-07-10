using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

public sealed class PassarSearchRequestConfigRegistryItem : StronglyTypedRegistryItem<PassarSearchRequestConfig>
{
	public PassarSearchRequestConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
		: base(new RegistryItemImpl(name, category, caption, hint, new PassarSearchRequestConfigRegistryDataType(), storage, new PassarSearchRequestConfig()))
	{
	}
}
