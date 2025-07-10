using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.Registry.GUI.Testing
{
	[TestedType(typeof(DeclarationMilestoneEventUpdatesRegistryControl))]
	sealed class DeclarationMilestoneEventUpdatesRegistryControlTest : MilestoneEventUpdatesRegistryControlTest
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new DeclarationMilestoneEventUpdatesCollection();
		}
	}
}
