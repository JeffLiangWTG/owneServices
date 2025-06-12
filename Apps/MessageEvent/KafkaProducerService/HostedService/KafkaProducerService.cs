using System;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Logging;
using CargoWise.eHub.MessageEvent.KafkaProducerService.Options;
using CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.HostedService
{
	internal class KafkaProducerService : BackgroundService
	{
		private readonly ILogger<KafkaProducerService> _logger;
		private readonly IServiceProvider _serviceProvider;

		public KafkaProducerService(IServiceProvider services, ILogger<KafkaProducerService> logger)
		{
			_logger = logger;
			_serviceProvider = services;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.eHubLog(LogLevel.Information, "", "", "", "{className} starting", nameof(KafkaProducerService));
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var producer = _serviceProvider.GetRequiredService<IProducer<string, XmlDocument>>();
					_logger.eHubLog(LogLevel.Information, "", "", "", "{producerName} is created", nameof(IProducer<string, XmlDocument>));
					while (!stoppingToken.IsCancellationRequested)
					{
						try
						{
							var count = await DoWorkCoreAsync(producer).ConfigureAwait(false);
							if (count == 0 && !stoppingToken.IsCancellationRequested)
							{
								var serviceBrokerOptions = _serviceProvider.GetRequiredService<IOptions<ServiceBrokerOptions>>().Value;
								_logger.eHubLog(LogLevel.Information, "", "", "", "Retry in {interval} seconds.", serviceBrokerOptions.RetryIntervalSeconds);
								await Task.Delay(TimeSpan.FromSeconds(serviceBrokerOptions.RetryIntervalSeconds), stoppingToken);
							}

						}
						catch (KafkaException ex)
						{
							_logger.eHubLog(LogLevel.Error, "", "", "", "Caught Kafka exception: {code}-{reason}", ex.Error.Code, ex.Error.Reason);
							await LogExceptionAndWait(ex, stoppingToken).ConfigureAwait(false);
							break;
						}
						catch (Exception ex)
						{
							await LogExceptionAndWait(ex, stoppingToken).ConfigureAwait(false);
						}
					}
				}
				catch (Exception ex)
				{
					await LogExceptionAndWait(ex, stoppingToken).ConfigureAwait(false);
				}
			}
		}

		internal virtual async Task LogExceptionAndWait(Exception ex, CancellationToken cancellationToken)
		{
			_logger.eHubLog(LogLevel.Error, "", "", "", ex, "{methodName} caught exception", nameof(ExecuteAsync));

			var serviceBrokerOptions = _serviceProvider.GetRequiredService<IOptions<ServiceBrokerOptions>>().Value;
			_logger.eHubLog(LogLevel.Information, "", "", "", "Retry in {interval} seconds.", serviceBrokerOptions.RetryIntervalSeconds);
			await Task.Delay(TimeSpan.FromSeconds(serviceBrokerOptions.RetryIntervalSeconds), cancellationToken);
		}

		internal virtual async Task<int> DoWorkCoreAsync(IProducer<string, XmlDocument> producer)
		{
			_logger.eHubLog(LogLevel.Information, "", "", "", "{methodName} started", nameof(DoWorkCoreAsync));

			using var serviceBroker = _serviceProvider.GetRequiredService<IMessageEventServiceBroker>();
			var kafkaOptions = _serviceProvider.GetRequiredService<IOptions<KafkaOptions>>().Value;

			_logger.eHubLog(LogLevel.Trace, "", "", "", "Topic: {topic}", kafkaOptions.Topic);

			try
			{
				_logger.eHubLog(LogLevel.Trace, "", "", "", "BeginTransaction for producer started");
				producer.BeginTransaction();
				_logger.eHubLog(LogLevel.Trace, "", "", "", "BeginTransaction for producer finished");

				_logger.eHubLog(LogLevel.Trace, "", "", "", "BeginTransaction for service broker started");
				serviceBroker.BeginTransaction();
				_logger.eHubLog(LogLevel.Trace, "", "", "", "BeginTransaction for service broker finished");

				var count = 0;
				await foreach (var message in serviceBroker.GetNextBatchMessagesAsync().ConfigureAwait(false))
				{
					var inboxPk = message.SelectSingleNode(Constants.InboxPkXPath)?.InnerText.ToUpperInvariant() ?? string.Empty;
					_logger.eHubLog(LogLevel.Trace, "", "", "", "Queried InboxPK: {pk}", inboxPk);
					producer.Produce(kafkaOptions.Topic, new Message<string, XmlDocument> { Key = inboxPk, Value = message });
					count++;
				}

				_logger.eHubLog(LogLevel.Trace, "", "", "", "CommitTransaction for producer started");
				producer.CommitTransaction();
				_logger.eHubLog(LogLevel.Trace, "", "", "", "CommitTransaction for producer finished");

				_logger.eHubLog(LogLevel.Information, "", "", "", $"Produced {count} messages.");

				_logger.eHubLog(LogLevel.Trace, "", "", "", "Commit for service broker started");
				serviceBroker.Commit();
				_logger.eHubLog(LogLevel.Trace, "", "", "", "Commit for service broker finished");
				return count;
			}
			catch (Exception)
			{
				AbortTransaction(serviceBroker, producer);
				throw;
			}
			finally
			{
				_logger.eHubLog(LogLevel.Information, "", "", "", "{methodName} finished", nameof(DoWorkCoreAsync));
			}
		}

		private void AbortTransaction(IMessageEventServiceBroker serviceBroker, IProducer<string, XmlDocument> producer)
		{
			try
			{
				_logger.eHubLog(LogLevel.Trace, "", "", "", "Rollback for service broker started");
				serviceBroker.Rollback();
				_logger.eHubLog(LogLevel.Trace, "", "", "", "Rollback for service broker finished");
			}
			catch (Exception ex)
			{
				_logger.eHubLog(LogLevel.Error, "", "", "", ex, "{param} transaction rollback failed", nameof(serviceBroker));
			}

			try
			{
				_logger.eHubLog(LogLevel.Trace, "", "", "", "AbortTransaction for producer started");
				producer.AbortTransaction();
				_logger.eHubLog(LogLevel.Trace, "", "", "", "AbortTransaction for producer finished");

			}
			catch (Exception ex)
			{
				_logger.eHubLog(LogLevel.Error, "", "", "", ex, "{param} transaction rollback failed", nameof(producer));
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.eHubLog(LogLevel.Information, "", "", "", "{className} stopping", nameof(KafkaProducerService));
			return base.StopAsync(cancellationToken);
		}
	}
}
