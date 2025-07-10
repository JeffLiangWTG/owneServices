using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Registry;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Registry
{
	[TestedType(typeof(RemoveComplianceWiseFeatureDevelopment))]

	class RemoveComplianceWiseFeatureDevelopmentTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "ComplianceWiseFeatureDevelopment" };
		}
	}
}
