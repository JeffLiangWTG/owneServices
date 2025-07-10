using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.EU;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.EU
{
	[TestedType(typeof(RemoveSADGenerationConfigurationRegistryItem))]
	sealed class RemoveSADGenerationConfigurationRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "SADGenerationOnClearanceEnabled" };
		}
	}
}
