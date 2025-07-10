using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Architecture;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Architecture
{
	[TestedType(typeof(DeleteProcessControllerElasticLogViewerEnabledRegistryItem))]
	class DeleteProcessControllerElasticLogViewerEnabledRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "ProcessControllerElasticLogViewerEnabled" };
		}
	}
}
