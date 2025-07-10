using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem))]
	sealed class LinerAndAgencyContainerMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<LinerAndAgencyContainerMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<LinerAndAgencyContainerMilestoneEventUpdatesCollection, LinerAndAgencyContainerMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new LinerAndAgencyContainerMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
