using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	public class RemoveGlowUseIndexingServiceForModuleFiltersRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["GlowUseIndexingServiceForModuleFilters"];
		}
	}
}
