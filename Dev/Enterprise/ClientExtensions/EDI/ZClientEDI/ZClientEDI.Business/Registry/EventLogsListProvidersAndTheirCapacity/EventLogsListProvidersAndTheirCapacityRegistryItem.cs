using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class EventLogsListProvidersAndTheirCapacityRegistryItem : StronglyTypedRegistryItem<EventLogsListProvidersAndTheirCapacityCollection>
	{
		public EventLogsListProvidersAndTheirCapacityRegistryItem(string itemName, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(
				itemName,
				category,
				caption,
				hint,
				new EventLogsListProvidersAndTheirCapacityDataType(),
				RegistryStorageFlags.System,
				RegistryOptions.NotCached))
		{
		}
	}
}

