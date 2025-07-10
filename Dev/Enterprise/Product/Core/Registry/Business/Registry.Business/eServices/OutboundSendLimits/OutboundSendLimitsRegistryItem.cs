using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OutboundSendLimitsRegistryItem : StronglyTypedRegistryItem<OutboundSendLimitsRule>
	{
		public OutboundSendLimitsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new OutboundSendLimitsRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OutboundSendLimitsRegistryItemEditor, Enterprise.Registry.GUI")]
	class OutboundSendLimitsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OutboundSendLimitsRule>
	{
		public OutboundSendLimitsRegistryDataType()
		{
		}
	}
}
