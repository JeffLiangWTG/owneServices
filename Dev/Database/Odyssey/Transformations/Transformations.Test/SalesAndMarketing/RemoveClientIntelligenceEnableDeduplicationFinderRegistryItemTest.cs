using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.SalesAndMarketing
{
	[TestedType(typeof(RemoveClientIntelligenceEnableDeduplicationFinderRegistryItem))]
	class RemoveClientIntelligenceEnableDeduplicationFinderRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames() => new[] { "ClientIntelligenceEnableDeduplicationFinder" };
	}
}
