using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeclarationMilestoneEventUpdatesRegistryItem))]
	sealed class DeclarationMilestoneEventUpdatesRegistryItemTest : MilestoneEventUpdatesRegistryItemTest<DeclarationMilestoneEventUpdatesCollection>
	{
		#region Overrides

		protected override StronglyTypedRegistryItem<DeclarationMilestoneEventUpdatesCollection, DeclarationMilestoneEventUpdatesCollection> GetNewRegistryItem()
		{
			return new DeclarationMilestoneEventUpdatesRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new DeclarationMilestoneEventUpdatesCollection());
		}

		#endregion
	}
}
