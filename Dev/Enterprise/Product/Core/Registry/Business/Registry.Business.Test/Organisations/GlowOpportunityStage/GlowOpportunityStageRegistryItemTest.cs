using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStageRegistryItem))]
	sealed class GlowOpportunityStageRegistryItemTest : StronglyTypedRegistryItemTestCase<GlowOpportunityStageCollection>
	{
		protected override StronglyTypedRegistryItem<GlowOpportunityStageCollection, GlowOpportunityStageCollection> GetNewRegistryItem()
		{
			var defaultValues = new GlowOpportunityStageCollection();
			defaultValues.Add("AAA", (NoResString)"AAA Description", true, 50);
			defaultValues.Add("ZZZ", (NoResString)"ZZZ Description", false);
			return new GlowOpportunityStageRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, defaultValues);
		}
	}
}
