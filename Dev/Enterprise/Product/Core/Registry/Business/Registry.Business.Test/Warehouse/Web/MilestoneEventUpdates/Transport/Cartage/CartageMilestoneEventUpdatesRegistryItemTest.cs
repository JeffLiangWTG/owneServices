using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CartageMilestoneEventUpdatesRegistryItem))]
	sealed class CartageMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<CartageMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<CartageMilestoneEventUpdatesCollection, CartageMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new CartageMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new CartageMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
