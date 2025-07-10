using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class InboundMessageNotificationsRegistryItem : StronglyTypedRegistryItem<InboundMessageNotificationsRule>
	{
		public InboundMessageNotificationsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, InboundMessageNotificationsRule defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InboundMessageNotificationsRegistryDataType(), storage, defaultValue))
		{
		}

		public InboundMessageNotificationsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, InboundMessageNotificationsRule defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InboundMessageNotificationsRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
