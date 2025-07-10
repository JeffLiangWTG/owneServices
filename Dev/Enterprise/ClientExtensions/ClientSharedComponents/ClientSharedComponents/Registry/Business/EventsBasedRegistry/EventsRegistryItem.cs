using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	public class EventsRegistryItem : StronglyTypedRegistryItem<EventRegistryBusinessObjectCollection>
	{
		public EventsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new EventsRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.ClientSharedComponents.Registry.EventsRegistryItemEditor, Enterprise.ClientSharedComponents.GUI")]
	class EventsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EventRegistryBusinessObjectCollection>
	{
		public EventsRegistryDataType()
		{
		}
	}
}
