using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture
{
	sealed class DeleteProcessControllerElasticLogViewerEnabledRegistryItem : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "ProcessControllerElasticLogViewerEnabled" };
		}
	}
}
