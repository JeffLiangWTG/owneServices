using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BookingMilestoneEventUpdatesRegistryItem))]
	sealed class BookingMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<BookingMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<BookingMilestoneEventUpdatesCollection, BookingMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new BookingMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new BookingMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
