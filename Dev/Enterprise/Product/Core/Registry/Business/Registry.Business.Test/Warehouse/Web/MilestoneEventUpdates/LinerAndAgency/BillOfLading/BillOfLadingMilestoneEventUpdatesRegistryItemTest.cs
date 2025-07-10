using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BillOfLadingMilestoneEventUpdatesRegistryItem))]
	sealed class BillOfLadingMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<BillOfLadingMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<BillOfLadingMilestoneEventUpdatesCollection, BillOfLadingMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new BillOfLadingMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new BillOfLadingMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
