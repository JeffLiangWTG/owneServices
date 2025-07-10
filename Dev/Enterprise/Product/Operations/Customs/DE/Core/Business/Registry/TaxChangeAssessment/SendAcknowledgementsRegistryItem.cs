using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	public class SendAcknowledgementsRegistryItem : StronglyTypedRegistryItem<SendAcknowledgementsRegistryCollection>
	{
		public SendAcknowledgementsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new SendAcknowledgementsRegistryDataType(), storage, RegistryOptions.Default))
		{
		}
	}
}
