using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Registry
{
	class RemoveComplianceWiseFeatureDevelopment : DeleteRegistryItem
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "ComplianceWiseFeatureDevelopment" };
		}
	}
}
