using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class BookingMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<BookingMilestoneEventUpdatesCollection>
	{
		public BookingMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new BookingMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.BookingMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class BookingMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<BookingMilestoneEventUpdatesCollection>
	{
		public BookingMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}
