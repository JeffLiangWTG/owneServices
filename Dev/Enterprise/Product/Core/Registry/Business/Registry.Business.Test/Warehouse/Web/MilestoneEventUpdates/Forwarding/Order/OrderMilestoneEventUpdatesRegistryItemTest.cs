using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrderMilestoneEventUpdatesRegistryItem))]
	sealed class OrderMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<OrderMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<OrderMilestoneEventUpdatesCollection, OrderMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new OrderMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new OrderMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
