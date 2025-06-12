using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Serilog;

namespace XH.XT.Monitoring.HealthCheckService.HealthChecks
{
	public class CustomsServersHealthCheck : HealthCheckBase
	{
		readonly (CustomsWebServer ServerInfo, bool IsUrlValid)[] customsWebServers;
		readonly (CustomsMailServer ServerInfo, string SettingErrorMessage)[] customsMailServers;
		readonly HttpClient httpClient;
		readonly ISmtpClient smtpClient;

		public CustomsServersHealthCheck(ILogger logger, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ISmtpClient smtpClient, [NotNull] IOptionsMonitor<HealthCheckSettings> settingsOptions, IConfiguration config) : base(logger, httpContextAccessor, config)
		{
			this.smtpClient = smtpClient;
			httpClient = httpClientFactory.CreateClient();
			customsWebServers = settingsOptions.CurrentValue.CustomsWebServers?.Select(ws => (ws, IsValidUrl(ws.Url)))?.ToArray() ?? Array.Empty<(CustomsWebServer ServerInfo, bool IsUrlValid)>();
			customsMailServers = settingsOptions.CurrentValue.CustomsMailServers?.Select(s => (s, ValidateMailServerSettings(s))).ToArray() ?? Array.Empty<(CustomsMailServer ServerInfo, string SettingErrorMessage)>();
		}

		protected override async Task<HealthCheckResult> ExecuteHealthCheckAsync(HealthCheckContext context, CancellationToken cancellationToken)
		{
			{
				var healthyResult = new Dictionary<string, object>();
				var unhealthyResult = new Dictionary<string, object>();
				foreach (var webServer in customsWebServers)
				{
					var errorReason = await CheckWebServerHealth(webServer, cancellationToken);
					if (string.IsNullOrEmpty(errorReason))
					{
						healthyResult.Add(webServer.ServerInfo.Name, "Healthy");
					}
					else
					{
						unhealthyResult.Add(webServer.ServerInfo.Name, $"UnHealthy:{errorReason}");
					}
				}

				foreach (var mailServer in customsMailServers)
				{
						var errorReason = await CheckMailServer(mailServer, cancellationToken);
						if (string.IsNullOrEmpty(errorReason))
						{
							healthyResult.Add(mailServer.ServerInfo.Name, "Healthy");
						}
						else
						{
							unhealthyResult.Add(mailServer.ServerInfo.Name, $"UnHealthy:{errorReason}");
						}
				}

				if (unhealthyResult.Count > 0)
				{
					return HealthCheckResult.Unhealthy(description: healthyResult.Count > 0
							? $"Healthy interface(s): {string.Join(", ", healthyResult.Keys)}"
							: null, data: unhealthyResult);
				}

				return HealthCheckResult.Healthy("All configured servers are healthy.");
			}
		}

		async Task<string> CheckWebServerHealth((CustomsWebServer WebServer, bool IsUrlValid) serverInfo, CancellationToken cancellationToken)
		{
			var url = serverInfo.WebServer.Url;
			if (!serverInfo.IsUrlValid)
			{
				return $"The web server setting is invalid. The invalid url: {url}.";
			}

			try
			{
				using var request = new HttpRequestMessage(HttpMethod.Head, url);
				var response = await httpClient.SendAsync(request, cancellationToken);
				return response.IsSuccessStatusCode
					? string.Empty
					: $"The web server response code not in expected:{(int)response.StatusCode} {response.StatusCode}.";
			}
			catch (Exception ex) when (ex is TaskCanceledException or HttpRequestException)
			{
				return $"The web server is not reachable. Error: {ex.Message}";
			}
		}

		[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Please use more specific exception type if you update this function.")]
		async Task<string> CheckMailServer((CustomsMailServer MailServer, string SettingErrorMessage) serverInfo, CancellationToken cancellationToken)
		{
			if (!string.IsNullOrEmpty(serverInfo.SettingErrorMessage))
			{
				return $"Invalid mail server configuration: {serverInfo.SettingErrorMessage}";
			}
			var mailServer = serverInfo.MailServer;
			try
			{
				cancellationToken.ThrowIfCancellationRequested();

				await smtpClient.ConnectAsync(mailServer.Host, mailServer.Port, mailServer.EnableSSL, cancellationToken);
				if (!string.IsNullOrWhiteSpace(mailServer.Username) && !string.IsNullOrWhiteSpace(mailServer.Password))
				{
					await smtpClient.AuthenticateAsync(mailServer.Username, mailServer.Password, cancellationToken);
				}
				await smtpClient.DisconnectAsync(quit: true, cancellationToken);

				return string.Empty;
			}
			catch (AuthenticationException ex)
			{
				return $"Authentication failed:{GetNestedExceptionMessages(ex)}";
			}
			catch (OperationCanceledException ex)
			{
				return cancellationToken.IsCancellationRequested
					? "The current health check is cancelled."
					: $"The health check is cancelled due to the Exception and it will check in next run: {GetNestedExceptionMessages(ex)}";
			}
			catch (Exception ex)
			{
				return $"Exception during health check and it will check in next run. {ex.GetType()}: {GetNestedExceptionMessages(ex)}";
			}
		}

		static string GetNestedExceptionMessages(Exception exception)
		{
			return exception.Message + (exception.InnerException == null ? string.Empty : " - " + GetNestedExceptionMessages(exception.InnerException));
		}

		static bool IsValidUrl(string url)
		{
			return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
		}

		static string ValidateMailServerSettings(CustomsMailServer mailServer)
		{
			var errorMessage = string.Empty;
			try
			{
				Validator.ValidateObject(mailServer, new ValidationContext(mailServer), validateAllProperties: true);
			}
			catch (ValidationException vex)
			{
				errorMessage = vex.Message;
			}

			return errorMessage;
		}
	}
}
