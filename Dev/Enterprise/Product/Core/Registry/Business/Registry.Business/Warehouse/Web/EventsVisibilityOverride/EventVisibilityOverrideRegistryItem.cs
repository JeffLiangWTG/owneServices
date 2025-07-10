using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EventVisibilityOverrideRegistryItem : StronglyTypedRegistryItem<EventVisibilityOverrideCollection, EventVisibilityOverrideCollection>, IEventVisibilityOverrideRegistryItem
	{
		public EventVisibilityOverrideRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, EventVisibilityOverrideCollection defaultValue)
			: base(new RegistryItemImpl(name, caption, hint, new EventVisibilityOverrideRegistryItemDataType(), null, storage, options, defaultValue, false, new MultilingualString[] { category }))
		{
		}
	}

	public interface IEventVisibilityOverrideRegistryItemDataType : IRegistryDataType
	{
	}

	[RegistryEditor("Enterprise.Registry.GUI.EventsVisibilityOverrideRegistryItemEditor, Enterprise.Registry.GUI")]
	public class EventVisibilityOverrideRegistryItemDataType : NonPersistentBusinessObjectRegistryDataType<EventVisibilityOverrideCollection>, IEventVisibilityOverrideRegistryItemDataType
	{
	}
}
