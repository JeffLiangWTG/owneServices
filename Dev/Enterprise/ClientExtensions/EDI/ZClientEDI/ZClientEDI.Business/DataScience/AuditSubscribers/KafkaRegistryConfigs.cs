#nullable enable

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Confluent.Kafka;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers
{
	sealed class ProducerCache(byte[] hash, IProducer<string, string> producer)
	{
		public byte[] Hash => hash;
		public IProducer<string, string> Producer => producer;
	}

	public abstract class KafkaRegistryConfigsBase<TConfig>
		where TConfig : KafkaRegistryConfigsBase<TConfig>, new()
	{
		public abstract bool EnableSubscriber { get; }
		public abstract bool UseTransaction { get; }
		public abstract string KafkaTopic { get; }
		protected abstract string EdiKafkaBootstrapServers { get; }
		protected abstract string KafkaSaslUsername { get; }
		protected abstract string KafkaSaslPassword { get; }

		static readonly object kafkaTopicLockStatic = new ();
		[ThreadSafe] static ProducerCache? producerCacheStatic = null;

		public object KafkaTopicLock => kafkaTopicLockStatic;

		protected byte[] ComputeHash()
		{
			using var buffer = new MemoryStream(
				64
				+ KafkaTopic.Length + 2
				+ EdiKafkaBootstrapServers.Length + 2
				+ KafkaSaslUsername.Length + 2
				+ KafkaSaslPassword.Length + 2
			);

			using (var writer = new StreamWriter(buffer, Encoding.UTF8))
			{
				writer.WriteLine($"{UseTransaction}");
				writer.WriteLine(KafkaTopic);
				writer.WriteLine(EdiKafkaBootstrapServers);
				writer.WriteLine(KafkaSaslUsername);
				writer.WriteLine(KafkaSaslPassword);
			}

			using var sha256 = SHA256.Create();
			return sha256.ComputeHash(buffer.ToArray());
		}

		internal static void ResetKafkaProducerCache()
		{
			producerCacheStatic?.Producer.Dispose();
			producerCacheStatic = null;
		}

		public virtual IProducer<string, string> GetOrCreateProducer(ILogger logger)
		{
			var currentHash = ComputeHash();
			var producerCache = producerCacheStatic;
			if (producerCache == null || !producerCache.Hash.SequenceEqual(currentHash))
			{
				producerCache?.Producer.Dispose();
				producerCache = new (currentHash, CreateProducer(logger));
				producerCacheStatic = producerCache;
			}

			return producerCache.Producer;
		}

		protected virtual IProducer<string, string> CreateProducer(ILogger logger)
		{
			var producerConfig = new ProducerConfig
			{
				BootstrapServers = EdiKafkaBootstrapServers,
				CompressionType = CompressionType.Snappy,
				EnableIdempotence = true,
				LingerMs = 20,
				SaslMechanism = SaslMechanism.Plain,
				SaslUsername = KafkaSaslUsername,
				SaslPassword = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt(KafkaSaslPassword),
				SecurityProtocol = SecurityProtocol.SaslSsl,
				SslCaLocation = KafkaConfig.SslCaLocation,
			};

			if (UseTransaction)
			{
				producerConfig.TransactionalId = $"{KafkaTopic}-txn-id";
			}

			IProducer<string, string> kafkaProducer;
			try
			{
				kafkaProducer = new ProducerBuilder<string, string>(producerConfig).Build();
				if (UseTransaction)
				{
					kafkaProducer.InitTransactions(TimeSpan.FromSeconds(50));
				}
			}
			catch (Exception error)
			{
				logger?.Error($"{GetType().FullName}.CreateProducer(UseTransaction={UseTransaction}) failed with error [{error.GetType().FullName}]({error.Message})", error);
				throw;
			}

			return kafkaProducer;
		}
	}

	public static class KafkaRegistryConfigs
	{
		static EDIDataRegistry Rego => EDIDataRegistry.Instance;

		public sealed class Core : KafkaRegistryConfigsBase<Core>
		{
			public override bool EnableSubscriber { get; } = Rego.EnableDataScienceCoreSubscribers.Value;
			public override bool UseTransaction { get; } = Rego.DataScienceCoreKafkaEnableTransactions.Value;
			public override string KafkaTopic { get; } = Rego.DataScienceCoreKafkaTopic.Value;
			protected override string EdiKafkaBootstrapServers { get; } = Rego.EdiKafkaBootstrapServers.Value;
			protected override string KafkaSaslPassword { get; } = Rego.DataScienceCoreKafkaSaslPassword.Value;
			protected override string KafkaSaslUsername { get; } = Rego.DataScienceCoreKafkaSaslUsername.Value;
		}

		public sealed class Billing : KafkaRegistryConfigsBase<Billing>
		{
			public override bool EnableSubscriber { get; } = Rego.EnableDataScienceBillingSubscribers.Value;
			public override bool UseTransaction { get; } = Rego.DataScienceBillingKafkaEnableTransactions.Value;
			public override string KafkaTopic { get; } = Rego.DataScienceBillingKafkaTopic.Value;
			protected override string EdiKafkaBootstrapServers { get; } = Rego.EdiKafkaBootstrapServers.Value;
			protected override string KafkaSaslPassword { get; } = Rego.DataScienceBillingKafkaSaslPassword.Value;
			protected override string KafkaSaslUsername { get; } = Rego.DataScienceBillingKafkaSaslUsername.Value;
		}

		public sealed class IncidentRelated : KafkaRegistryConfigsBase<IncidentRelated>
		{
			public override bool EnableSubscriber { get; } = Rego.EnableDataScienceIncidentRelatedSubscribers.Value;
			public override bool UseTransaction { get; } = Rego.DataScienceIncidentRelatedKafkaEnableTransactions.Value;
			public override string KafkaTopic { get; } = Rego.DataScienceIncidentRelatedKafkaTopic.Value;
			protected override string EdiKafkaBootstrapServers { get; } = Rego.EdiKafkaBootstrapServers.Value;
			protected override string KafkaSaslPassword { get; } = Rego.DataScienceIncidentRelatedKafkaSaslPassword.Value;
			protected override string KafkaSaslUsername { get; } = Rego.DataScienceIncidentRelatedKafkaSaslUsername.Value;
		}

		public sealed class Productivity : KafkaRegistryConfigsBase<Productivity>
		{
			public override bool EnableSubscriber { get; } = Rego.EnableDataScienceProductivitySubscribers.Value;
			public override bool UseTransaction { get; } = Rego.DataScienceProductivityKafkaEnableTransactions.Value;
			public override string KafkaTopic { get; } = Rego.DataScienceProductivityKafkaTopic.Value;
			protected override string EdiKafkaBootstrapServers { get; } = Rego.EdiKafkaBootstrapServers.Value;
			protected override string KafkaSaslPassword { get; } = Rego.DataScienceProductivityKafkaSaslPassword.Value;
			protected override string KafkaSaslUsername { get; } = Rego.DataScienceProductivityKafkaSaslUsername.Value;
		}

		public sealed class Issues : KafkaRegistryConfigsBase<Issues>
		{
			public override bool EnableSubscriber { get; } = Rego.EnableDataScienceIssuesSubscribers.Value;
			public override bool UseTransaction { get; } = Rego.DataScienceIssuesKafkaEnableTransactions.Value;
			public override string KafkaTopic { get; } = Rego.DataScienceIssuesKafkaTopic.Value;
			protected override string EdiKafkaBootstrapServers { get; } = Rego.EdiKafkaBootstrapServers.Value;
			protected override string KafkaSaslPassword { get; } = Rego.DataScienceIssuesKafkaSaslPassword.Value;
			protected override string KafkaSaslUsername { get; } = Rego.DataScienceIssuesKafkaSaslUsername.Value;
		}
	}
}
