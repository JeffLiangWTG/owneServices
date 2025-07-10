using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.FR;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.FR
{
	[TestedType(typeof(RemoveSADGenerationConfigurationRegistryItem))]
	sealed class RemoveSADGenerationConfigurationRegistryItemTest : DeleteRegistryItemTest
	{
		protected override string[] GetRegistryItemNames()
		{
			return new[] { "SADGenerationOnConfirmedExitEnabled" };
		}
	}
}
