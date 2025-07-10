using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EventVisibilityRegistryItem : StronglyTypedRegistryItem<EventVisibilityCollection>, IEventVisibilityRegistryItem
	{
		public EventVisibilityRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, EventVisibilityCollection defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new EventVisibilityRegistryItemDataType(), null, storage, RegistryOptions.Default, defaultValue, false, new MultilingualString[] { category }))
		{
		}
	}

	public interface IEventVisibilityRegistryItemDataType : IRegistryDataType
	{
	}

	[RegistryEditor("Enterprise.Registry.GUI.EventsVisibilityRegistryItemEditor, Enterprise.Registry.GUI")]
	public class EventVisibilityRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<EventVisibilityCollection>, IEventVisibilityRegistryItemDataType
	{
		public EventVisibilityRegistryItemDataType()
		{
		}
	}
}
