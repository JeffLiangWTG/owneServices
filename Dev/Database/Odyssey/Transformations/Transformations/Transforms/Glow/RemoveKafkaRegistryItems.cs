using Enterprise.DbUpgrader.Transformation.Common;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Glow;

sealed class RemoveKafkaRegistryItems : DeleteRegistryItem
{
	protected override string[] GetRegistryItemNames()
	{
		return new[]
		{
			GlowKafkaBootstrapServers,
			GlowKafkaTopic,
			GlowKafkaServerCertificate,
			GlowKafkaSecurity
		};
	}

	public const string GlowKafkaBootstrapServers = "GlowKafkaBootstrapServers";
	public const string GlowKafkaTopic = "GlowKafkaTopic";
	public const string GlowKafkaServerCertificate = "GlowKafkaServerCertificate";
	public const string GlowKafkaSecurity = "GlowKafkaSecurity";
}
