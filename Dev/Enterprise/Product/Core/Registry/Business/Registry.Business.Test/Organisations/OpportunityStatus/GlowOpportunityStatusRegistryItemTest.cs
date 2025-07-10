using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlowOpportunityStatusRegistryItem))]
	sealed class GlowOpportunityStatusRegistryItemTest : StronglyTypedRegistryItemTestCase<GlowOpportunityStatusCollection>
	{
		protected override StronglyTypedRegistryItem<GlowOpportunityStatusCollection, GlowOpportunityStatusCollection> GetNewRegistryItem()
		{
			var defaultStatuses = new GlowOpportunityStatusCollection();
			defaultStatuses.Add("AAA", (NoResString)"AAA Description", value: true);
			defaultStatuses.Add("ZZZ", (NoResString)"ZZZ Description", value: false);
			return new GlowOpportunityStatusRegistryItem("name", (NoResString)"category", (NoResString)"caption", (NoResString)"hint", RegistryStorageFlags.System, RegistryOptions.Default, defaultStatuses);
		}
	}
}
