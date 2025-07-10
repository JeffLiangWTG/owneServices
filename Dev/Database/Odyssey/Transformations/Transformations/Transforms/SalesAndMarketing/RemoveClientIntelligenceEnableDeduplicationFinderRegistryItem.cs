using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.SalesAndMarketing
{
	class RemoveClientIntelligenceEnableDeduplicationFinderRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames() => new[] { "ClientIntelligenceEnableDeduplicationFinder" };
	}
}
