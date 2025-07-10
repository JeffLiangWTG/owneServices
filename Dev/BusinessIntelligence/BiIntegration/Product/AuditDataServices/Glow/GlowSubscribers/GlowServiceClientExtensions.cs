using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Authentication.Primitives;
using Enterprise.Integration;
using Enterprise.ZArchitecture.GlowInterop;
using Newtonsoft.Json;

namespace Enterprise.AuditDataServices.Glow
{
	static class GlowServiceClientExtensions
	{
		static async Task<HttpResponseMessage> CallServiceAsync(string uri, Func<Task<HttpResponseMessage>> action, ILogger logger = null)
		{
			string errorMessage = null;
			HttpResponseMessage actionResult = null;
			try
			{
				actionResult = await action().ConfigureAwait(false);

				if (!actionResult.IsSuccessStatusCode)
				{
					var content = actionResult.Content != null ? await actionResult.Content.ReadAsStringAsync().ConfigureAwait(false) : string.Empty;
					errorMessage = FormattableString.Invariant($"Error sending request to {actionResult.RequestMessage?.RequestUri}... Code: {(int)actionResult.StatusCode}, Phrase: {actionResult.ReasonPhrase}, Content: {content}"); // System Notification
				}
			}
			catch (AuthorizationFailureException ex) when (ex.AuthenticationResult == AuthenticationResult.LogonDetailsIncorrect)
			{
				errorMessage = FormattableString.Invariant($"Authentication Error sending request to {uri}... Message: {ex.Message}"); // System Notification
			}
			catch (HttpRequestException ex)
			{
				errorMessage = FormattableString.Invariant($"Error sending request to {uri}... Message: {ex.Message}"); // System Notification
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				LogWarningOrThrow(errorMessage, logger);
			}

			return actionResult;
		}

		static void LogWarningOrThrow(string message, ILogger logger = null)
		{
			if (logger != null)
			{
				logger.Log(LogType.Warning, message);
			}
			else
			{
				throw new HttpRequestException(message);
			}
		}

		public static Task<HttpResponseMessage> PostJsonAsync(this IGlowServiceClient client, string url, object data, ILogger logger = null)
		{
			var json = JsonConvert.SerializeObject(data);
			return CallServiceAsync(
				url,
				async () =>
				{
					using var content = new StringContent(json, Encoding.UTF8, "application/json");
					return await client.PostAsync(url, content).ConfigureAwait(false);
				},
				logger);
		}
	}
}
