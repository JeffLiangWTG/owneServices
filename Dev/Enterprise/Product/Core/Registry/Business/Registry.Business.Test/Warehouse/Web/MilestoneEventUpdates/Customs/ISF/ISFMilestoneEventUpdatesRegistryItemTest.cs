using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ISFMilestoneEventUpdatesRegistryItem))]
	sealed class ISFMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<ISFMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<ISFMilestoneEventUpdatesCollection, ISFMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new ISFMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new ISFMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
