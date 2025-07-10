using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

public class ArrivalCustomerReferenceFormatRegistryItem : StronglyTypedRegistryItem<ArrivalCustomerReferenceFormat>
{
	public ArrivalCustomerReferenceFormatRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
		: base(new RegistryItemImpl(name, category, caption, hint, new ArrivalCustomerReferenceFormatDataType(), storage))
	{
	}
}
