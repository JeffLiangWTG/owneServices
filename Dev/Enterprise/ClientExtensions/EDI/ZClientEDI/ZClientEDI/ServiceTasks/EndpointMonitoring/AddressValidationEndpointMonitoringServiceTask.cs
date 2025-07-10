using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Newtonsoft.Json.Linq;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	"AXM",
	"Address Validation Endpoint Monitoring",
	"SYS",
	typeof(AddressValidationEndpointMonitoringServiceTask),
	CanRunInAnyBranch = true,
	IsMandatory = true,
	AllowsMultipleInstances = false,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class AddressValidationEndpointMonitoringServiceTask : ServiceProviderImpl
	{
		public AddressValidationEndpointMonitoringServiceTask()
		{
		}

		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var endpointsToNotify = new Dictionary<string, Exception>();
			try
			{
				var endpoints = EDIDataRegistry.Instance.AvsMonitoringEndpoints.Select(x => new Uri(new Uri(x), "GetServiceStatus?comprehensiveCheck=true")).ToArray();
				if (endpoints.Length == 0)
				{
					return;
				}

				foreach (var endpoint in endpoints)
				{
					youMustReactToThisToken.ThrowIfCancellationRequested();

					try
					{
						CheckServiceUrl(endpoint.ToString()).Wait();
					}
					catch (AggregateException ex) when (
						ex.InnerException is AvsUnhealthyException ||
						ex.InnerException is HttpRequestException ||
						ex.InnerException is TaskCanceledException)
					{
						endpointsToNotify.Add(endpoint.Host, ex.InnerException);
						ServiceLogger.Error($"Endpoint [{endpoint.Host}] is unhealthy!");
					}
				}
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				ServiceLogger.ErrorAndReportException("Unexpected exception occurred in AVS endpoint monitoring!", exception);
			}

			if (endpointsToNotify.Count > 0)
			{
				var attachments = new Dictionary<string, string>();
				var content = new StringBuilder();
				content.AppendLine("The following AVS endpoint(s) were unhealthy!");
				foreach (var endpointExceptionPair in endpointsToNotify)
				{
					content
						.AppendLine()
						.AppendLine($"Endpoint [{endpointExceptionPair.Key}] is unhealthy:")
						.AppendLine($"\t{endpointExceptionPair.Value.Message}");
					attachments.Add(endpointExceptionPair.Key, endpointExceptionPair.Value.ToString());
				}

				try
				{
					GetNewStatusNotifier().Notify("[AVS Monitoring]: Unhealthy Endpoint(s)", content.ToString(), attachments);
				}
				catch (InvalidOperationException exception)
				{
					ServiceLogger.ErrorAndReportException("Cannot create and save notification email!", exception);
				}
			}
		}

		protected virtual IStatusNotifier GetNewStatusNotifier() => new EmailStatusNotifier();

		protected virtual HttpClient GetNewHttpClient() => new HttpClient();

		protected HttpClient Client
		{
			get
			{
				if (client is null)
				{
					client = GetNewHttpClient();
					client.Timeout = TimeSpan.FromSeconds(Env.Instance.Registry.AddressValidationWebServiceTimeout);
				}

				return client;
			}
		}
		HttpClient client;

		async Task CheckServiceUrl(string requestUrl)
		{
			var message = await Client.GetAsync(requestUrl).ConfigureAwait(false);
			var content = await message.EnsureSuccessStatusCode().Content.ReadAsStringAsync();
			var contentJson = JObject.Parse(content);

			var serviceAvailable = contentJson.Value<bool?>("ServiceAvailable") ?? false;
			var diagnosticMessage = contentJson.Value<string>("DiagnosticMessage") ?? string.Empty;
			if (!serviceAvailable || diagnosticMessage.Contains(ServiceStatusErrorKey.SpectrumInaccessible) || diagnosticMessage.Contains(ServiceStatusErrorKey.DatabaseError))
			{
				throw new AvsUnhealthyException(diagnosticMessage);
			}
		}

		static class ServiceStatusErrorKey
		{
			public const string SpectrumInaccessible = "[Spectrum Inaccessible]";
			public const string DatabaseError = "[Database Error]";
		}

		[Serializable]
		class AvsUnhealthyException : Exception
		{
			public AvsUnhealthyException(string message)
				: base(message)
			{
			}

#if NETFRAMEWORK
			protected AvsUnhealthyException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}
}
