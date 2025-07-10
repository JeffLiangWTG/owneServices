using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class ShippingBookingMilestoneEventUpdatesRegistryItem : MilestoneEventUpdatesRegistryItem<ShippingBookingMilestoneEventUpdatesCollection>
	{
		public ShippingBookingMilestoneEventUpdatesRegistryItem(string name, MultilingualString caption, MultilingualString hint, MultilingualString category, RegistryStorageFlags storage, RegistryOptions options, MilestoneEventUpdatesCollection defaultValue)
			: base(name, caption, hint, category, new ShippingBookingMilestoneEventUpdatesRegistryDataType(), storage, options, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.ShippingBookingMilestoneEventUpdatesRegistryItemEditor, Enterprise.Registry.GUI")]
	class ShippingBookingMilestoneEventUpdatesRegistryDataType : MilestoneEventUpdatesRegistryDataType<ShippingBookingMilestoneEventUpdatesCollection>
	{
		public ShippingBookingMilestoneEventUpdatesRegistryDataType()
		{
		}
	}
}
