using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.eServices.Authentication.ServiceClient;
using CargoWise.Billing.Kafka.API;
using CargoWise.Billing.Client;
using CargoWise.eServices.Monitoring.HealthCheck.API;
using Confluent.Kafka;
using APIBillingTransaction = CargoWise.Billing.API.BillingTransaction;

namespace CargoWise.eHub.Gateway.HealthCheckService
{
	public class GatewayServiceHealthCheckItemProvider : IHealthCheckItemProvider
	{
		public string healthCheckError = string.Empty;
		public string healthCheckWarning = string.Empty;

		#region Name

		public string Name
		{
			get { return "GatewayWebService"; }
		}

		#endregion

		#region CheckHealthAsync

		public Task<HealthCheckItem> CheckHealthAsync()
		{
			try
			{
				if (!StreamedWebService.Ping())
				{
					healthCheckError += "eHubStreamedWebService is not active. ";
				}

				if (!IsAuthenticationEnabled())
				{
					healthCheckError += "Authentication web service is not active. ";
				}

				if (!IsDatabaseConnected())
				{
					healthCheckError += "Gateway is not connected to database. ";
				}

				if (!IsBillingHealthCheckSuccess(out string error))
				{
					if (!BillingMessageHandler.SendBillingToKafka)
					{
						healthCheckError += $"{error}";
					}
					else if (IsBillingEnabled())
					{
						healthCheckWarning += $"Direct service fallback successful. Original Kafka Error: {error}";
					}
					else
					{
						healthCheckError += $"Original Kafka Error: {error}";
					}
				}

				if (healthCheckError != string.Empty)
					return Task.FromResult(HealthCheckItem.Error(healthCheckError));
				else if (healthCheckWarning != string.Empty)
					return Task.FromResult(HealthCheckItem.Warning(healthCheckWarning));
				else
					return Task.FromResult(HealthCheckItem.Info("Service is alive."));
			}
			catch (Exception ex)
			{
				var errorDescription = string.Format(CultureInfo.InvariantCulture, "An exception '{0}' was thrown during health check.", ex.GetType().FullName);
				return Task.FromResult(HealthCheckItem.Error(errorDescription));
			}
		}

		#endregion

		public virtual eHubStreamedService StreamedWebService
		{
			get { return new eHubStreamedService(); }
		}

		public virtual bool IsAuthenticationEnabled()
		{
			var authentication = new AuthWebServiceApi();
			return authentication.Ping();
		}

		public bool IsBillingHealthCheckSuccess(out string error)
		{
			error = string.Empty;
			if (!BillingMessageHandler.SendBillingToKafka)
			{
				if (!IsBillingEnabled())
				{
					error = "Billing web service is not active.";
					return false;
				}
			}
			else
			{
				try
				{
					using (var kafkaClient = CreateKafkaClient())
					{
						kafkaClient.SendBillingInfoToKafka(ConfigurationManager.AppSettings["BillingKafkaTopic"],
							Guid.NewGuid().ToString().ToUpper(), new APIBillingTransaction
							{
								BillableCount = 1,
								ClientID = "TSTTSTTST",
								Category = "TST",
								PriceItemCode = "TST",
								Reference1 = "TEST",
								Reference2 = "TEST",
								Reference3 = "TEST",
								Reference4 = "TEST",
								ReportingSource = "CHK",
								ServiceOccuredUTC = DateTime.UtcNow,
								MessageTrackingID = "55B2D0DA-8230-43BE-83BF-5C7D5766343E",
							}, HandlerBillingDeliveryReport);
					}
					error = billingProducerError;
					return string.IsNullOrEmpty(error);
				}
				catch (Exception exception)
				{
					error =
						$"Failed to send health check message to Kafka queues. Message: {(exception.InnerException ?? exception).Message}";
					return false;
				}
			}

			return true;
		}

		public virtual bool IsBillingEnabled()
		{
			var billing = new BillingServiceClient();
			return billing.Ping();
		}

		public virtual void HandlerBillingDeliveryReport(DeliveryReport<string, APIBillingTransaction> report)
		{
			if (report.Error.Code != ErrorCode.NoError)
			{
				billingProducerError = $"Failed to send health check message to Kafka queues. ErrorCode: {report.Error.Code}. Reason: {report.Error.Reason}";
			}
		}

		public virtual bool IsDatabaseConnected()
		{
			try
			{
				var connectionString = ConfigurationManager.ConnectionStrings["eHubTransactionsContext"].ConnectionString;
				using (var connection = new SqlConnection(connectionString))
				{
					connection.Open();
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		private string billingProducerError = string.Empty;

		public virtual BillingKafkaClient CreateKafkaClient()
		{
			var config = ServiceHelper.GetKafkaProducerConfig();
			config.MessageTimeoutMs = 5000;
			return new BillingKafkaClient(config);
		}
	}
}
