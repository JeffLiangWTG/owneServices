using Enterprise.DbUpgrader.Transformation.DataModification.Public.Registry.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Glow;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.PostUpgrade.Public.Glow;

[TestedType(typeof(RemoveKafkaRegistryItems))]
sealed class RemoveKafkaRegistryItemsTest : DeleteRegistryItemTest
{
	protected override string[] GetRegistryItemNames()
	{
		return new[]
		{
			RemoveKafkaRegistryItems.GlowKafkaBootstrapServers,
			RemoveKafkaRegistryItems.GlowKafkaTopic,
			RemoveKafkaRegistryItems.GlowKafkaServerCertificate,
			RemoveKafkaRegistryItems.GlowKafkaSecurity
		};
	}
}
