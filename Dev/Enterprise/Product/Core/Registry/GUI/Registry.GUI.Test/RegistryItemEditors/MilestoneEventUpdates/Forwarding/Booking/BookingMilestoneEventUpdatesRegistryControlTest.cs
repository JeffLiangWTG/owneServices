using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(BookingMilestoneEventUpdatesRegistryControl))]
	sealed class BookingMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new BookingMilestoneEventUpdatesCollection();
		}
	}
}
