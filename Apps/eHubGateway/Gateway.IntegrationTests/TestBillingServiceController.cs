using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Service;
using CargoWise.eServices.TestHelpers.Database.Common;
using Confluent.Kafka;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;
using BillingTransaction = CargoWise.Billing.Service.BillingTransaction;
using UsageTransaction = CargoWise.Billing.Service.UsageTransaction;

namespace CargoWise.eHub.Gateway.IntegrationTests
{
	public class TestBillingServiceController
	{

		public TestBillingServiceController(Uri baseUri, bool sendBillingToKafka = false)
		{
			billingServiceHost = new ServiceHost(typeof(TestBillingWcfService), baseUri);
			billingServiceHost.AddServiceEndpoint(typeof(IBillingService), new BasicHttpBinding(BasicHttpSecurityMode.None), "BillingService");
			ServiceMetadataBehavior metadataBehavior = new ServiceMetadataBehavior { HttpGetEnabled = true };
			billingServiceHost.Description.Behaviors.Add(metadataBehavior);

			if (sendBillingToKafka)
			{
				mockKafkaClusters = new AdminClientBuilder(new ClientConfig(new Dictionary<string, string>()
				{
					{"bootstrap.servers", "localhost:9300"},
					{"test.mock.num.brokers", "3"}
				})).Build();
				kafkaBrokers = string.Join(",", mockKafkaClusters.GetMetadata(TimeSpan.FromMinutes(1)).Brokers.Select(b => $"{b.Host}:{b.Port}"));
				adminClient = new AdminClientBuilder(new ClientConfig(new Dictionary<string, string>()
				{
					{ "bootstrap.servers", kafkaBrokers }
				})).Build();
			}
		}
		public void StartBillingService()
		{
			try
			{
				if (billingServiceHost != null && (billingServiceHost.State == CommunicationState.Closed || billingServiceHost.State == CommunicationState.Created))
				{
					billingServiceHost.Open();
				}
			}
			catch
			{
				billingServiceHost?.Abort();
				throw;
			}
		}

		public void StartConsumers(params string[] topics)
		{
			consumerTasks = topics.Select(topic => Task.Run(() => { StartConsumer<APIBillingTransaction>(kafkaBrokers, topic, tokenSource.Token); }, tokenSource.Token)).ToArray();
			if (!IsSubscribingTopicsSuccessfully(topics).Wait(TimeSpan.FromMinutes(1)))
			{
				throw new TimeoutException("Consumers have not started or subscribed topics successfully");
			}
		}

		async Task IsSubscribingTopicsSuccessfully(params string[] topics)
		{

			var topicPartitions = adminClient.GetMetadata(TimeSpan.FromMinutes(1))
				.Topics.Where(t => topics.Contains(t.Topic)).SelectMany(topicMetadata => topicMetadata.Partitions, (topicMetadata, partitionMetadata) => new TopicPartition(topicMetadata.Topic, partitionMetadata.PartitionId))
				.ToList();
			while (true)
			{
				var results = await adminClient.ListConsumerGroupOffsetsAsync(new []
				{
					new ConsumerGroupTopicPartitions(ConsumerGroupName, topicPartitions)
				});

				if (results.First().Partitions.Any(x => x.Offset.Value >= 0))
				{
					break;
				}

				await Task.Delay(1000);
			}
		}

		public void Stop()
		{
			if (billingServiceHost != null)
			{
				if (billingServiceHost.State == CommunicationState.Opened)
				{
					billingServiceHost.Close();
				}
				else if (billingServiceHost.State == CommunicationState.Faulted)
				{
					billingServiceHost.Abort();
				}
			}

			if (consumerTasks != null && consumerTasks.Length > 0)
			{
				tokenSource.Cancel();
				Task.WaitAll(consumerTasks);
			}

			adminClient?.Dispose();
			mockKafkaClusters?.Dispose();
		}

		void StartConsumer<T>(string brokers, string topic, CancellationToken token) where T : class
		{
			var config = new Dictionary<string, string>
			{
				{ "bootstrap.servers", brokers },
				{ "group.id", "eservices-ehubgateway-billing-integration-test" },
				{ "auto.offset.reset", "Earliest" },
				{ "enable.auto.commit", "true" },
				{ "enable.partition.eof", "true" }
			};
			var consumerBuilder = new ConsumerBuilder<Ignore, APIBillingTransaction>(config).SetValueDeserializer(new BillingTransactionsDeserializer());
			using (var consumer = consumerBuilder.Build())
			{
				consumer.Subscribe(topic);
				while (!token.IsCancellationRequested)
				{
					try
					{
						var result = consumer.Consume(token);

						if (!result.IsPartitionEOF)
						{
							if (typeof(T) == typeof(APIBillingTransaction))
							{
								var transaction = result.Message.Value as APIBillingTransaction;
								var stg = new Staging
								{
									TX_BillableCount = transaction.BillableCount,
									TX_Branch = transaction.Branch,
									TX_Category = transaction.Category,
									TX_ClientID = transaction.ClientID,
									TX_ClientNumber = transaction.ClientNumber,
									TX_ClientStaffCode = transaction.ClientStaffCode,
									TX_PriceItemCode = transaction.PriceItemCode,
									TX_MessageTrackingID = transaction.MessageTrackingID,
									TX_Reference1 = transaction.Reference1,
									TX_Reference2 = transaction.Reference2,
									TX_Reference3 = transaction.Reference3,
									TX_Reference4 = transaction.Reference4,
									TX_Reference5 = transaction.Reference5,
									TX_ServiceOccuredUTC = transaction.ServiceOccuredUTC,
									TX_Version = transaction.Version,
									TX_ReportingSource = transaction.ReportingSource,
									TX_SystemCreateUTC = DateTime.UtcNow,

								};
								context.Stagings.Add(stg);
								context.SaveChanges();
							}
							else if (typeof(T) == typeof(string))
							{
								UsageTransactions.Add(result.Message.Value.ToString());
							}
						}
					}
					catch (OperationCanceledException)
					{
						if (!token.IsCancellationRequested)
						{
							throw;
						}
						// Ignore
					}
				}
			}
		}

		ServiceHost billingServiceHost;
		readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
		public List<string> UsageTransactions { get; } = new List<string>();
		readonly string kafkaBrokers;
		public string KafkaBrokers => kafkaBrokers;
		Task[] consumerTasks;
		readonly IAdminClient adminClient;
		readonly IAdminClient mockKafkaClusters;
		const string ConsumerGroupName = "eservices-ehubgateway-billing-integration-test";
		readonly BillingContext context = new BillingContext(SqlServerHelper.GetAdminConnectionString(DatabaseSetup.BillingDatabaseName));
	}

	internal class TestBillingWcfService : IBillingService
	{
		public void AddTransaction(CargoWise.Billing.Service.BillingTransaction transaction)
		{
			var stg = new Staging()
			{
				TX_BillableCount = transaction.BillableCount,
				TX_Branch = transaction.Branch,
				TX_Category = transaction.Category,
				TX_ClientID = transaction.ClientID,
				TX_ClientNumber = transaction.ClientNumber,
				TX_ClientStaffCode = transaction.ClientStaffCode,
				TX_PriceItemCode = transaction.PriceItemCode,
				TX_MessageTrackingID = transaction.MessageTrackingID,
				TX_Reference1 = transaction.Reference1,
				TX_Reference2 = transaction.Reference2,
				TX_Reference3 = transaction.Reference3,
				TX_Reference4 = transaction.Reference4,
				TX_Reference5 = transaction.Reference5,
				TX_ServiceOccuredUTC = transaction.ServiceOccuredUTC,
				TX_Version = transaction.Version,
				TX_ReportingSource = transaction.ReportingSource,
				TX_SystemCreateUTC = DateTime.UtcNow,

			};
			context.Stagings.Add(stg);
			context.SaveChanges();
		}

		public void AddTransactionRange(CargoWise.Billing.Service.BillingTransaction[] transactions)
		{
			throw new NotImplementedException();
		}

		public void AddUsageTransaction(UsageTransaction transaction)
		{
			throw new NotImplementedException();
		}

		public void AddUsageTransactionRange(UsageTransaction[] transactions)
		{
			throw new NotImplementedException();
		}

		public bool Ping()
		{
			return true;
		}

		readonly BillingContext context = new BillingContext(SqlServerHelper.GetAdminConnectionString(DatabaseSetup.BillingDatabaseName));
	}
}
