using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ContainerMilestoneEventUpdatesRegistryItem))]
	sealed class ContainerMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<ContainerMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<ContainerMilestoneEventUpdatesCollection, ContainerMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new ContainerMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ContainerMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
