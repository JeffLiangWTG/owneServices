using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Registry.Testing
{
	[TestedType(typeof(RemoveGlowUseIndexingServiceForModuleFiltersRegistryItem))]
	class RemoveGlowUseIndexingServiceForModuleFiltersRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return ["GlowUseIndexingServiceForModuleFilters"];
		}
	}
}
