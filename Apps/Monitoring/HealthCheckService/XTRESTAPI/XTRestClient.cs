using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.eServices.Encryption.Server.Decryptor;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using XH.Framework.XT.REST.API.Common;

namespace XH.XT.Monitoring.HealthCheckService.XTRESTAPI
{
	public class XTRestClient
	{
		readonly HttpMessageInvoker httpClient;
		readonly HttpClientSettings httpClientSettings;
		const string XTRestTokenKey = "XTRestToken";
		const string XTAlarmRestTokenKey = "XTAlarmRestToken";
		private readonly IOptionsMonitor<XTRestSettings> xTRestSettings;
		private readonly IDateTimeProvider dateTimeProvider;

		private readonly ConcurrentDictionary<string, (string, DateTime, DateTime)> authenticationTokenDict = new();

		public XTRestClient(
			IOptionsMonitor<XTRestSettings> settings,
			HttpMessageInvoker httpClient,
			HttpClientSettings httpClientSettings,
			IDateTimeProvider dateTimeProvider)
		{
			xTRestSettings = settings;
			this.httpClient = httpClient;
			this.httpClientSettings = httpClientSettings;
			this.dateTimeProvider = dateTimeProvider;
		}

		public async Task<bool> LogonRestApiAsync(string xTServerName, CancellationToken cancellationToken = default) => await LogonAsync(xTServerName, XTRestTokenKey, cancellationToken);
		public async Task<bool> LogonAlarmServerAsync(string xTServerName, CancellationToken cancellationToken = default) => await LogonAsync(xTServerName, XTAlarmRestTokenKey, cancellationToken);

		async Task<bool> LogonAsync(string xTServerName, string tokenKeyName, CancellationToken cancellationToken)
		{
			var authUrl = tokenKeyName switch
			{
				XTAlarmRestTokenKey => $"{xTRestSettings.Get(xTServerName).AlarmServerBaseUrl}/Auth/uat",
				XTRestTokenKey => $"{xTRestSettings.Get(xTServerName).XTRestBaseUrl}/Users/auth/uat",
				_ => throw new NotImplementedException($"{nameof(tokenKeyName)}. Not expected token key name: {tokenKeyName}"),
			};

			var key = $"{xTServerName}-{tokenKeyName}";
			if (authenticationTokenDict.TryGetValue(key, out (string Token, DateTime TokenExpiry, DateTime WorkspaceExpiry) value))
			{
				if (dateTimeProvider.UtcNow.AddSeconds(httpClientSettings.TimeoutInSeconds) >= value.TokenExpiry ||
					dateTimeProvider.UtcNow.AddSeconds(httpClientSettings.TimeoutInSeconds) >= value.WorkspaceExpiry)
				{
					authenticationTokenDict[key] = await RetrieveTokenAsync(xTServerName, authUrl, cancellationToken);
					return true;
				}
			}
			else
			{
				authenticationTokenDict.TryAdd(key, await RetrieveTokenAsync(xTServerName, authUrl, cancellationToken));
				return true;
			}
			return false;
		}

		public async Task<(string AuthResponseToken, DateTime Expiry, DateTime Timeout)> RetrieveTokenAsync(string xTServerName, string authUrl, CancellationToken cancellationToken)
		{
			var body = new AuthenticateRequestUserAccessToken
			{
				UserAccessToken = EhubServerDecryptor.Decrypt(xTRestSettings.Get(xTServerName).AccessToken),
				Timeout = xTRestSettings.Get(xTServerName).WorkspaceTimeoutInSeconds
			};

			using var logonRequest = new HttpRequestMessage(HttpMethod.Post, authUrl)
			{
				Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json"),
			};

			var response = await httpClient.SendAsync(logonRequest, cancellationToken);
			if (response.StatusCode == HttpStatusCode.OK)
			{
				var authenticateResponse = ProcessResponse<AuthenticateResponse>(response, cancellationToken).Result;
				var handler = new JwtSecurityTokenHandler();
				var jwtSecurityToken = handler.ReadJwtToken(authenticateResponse.Token);
				var expiry = DateTimeOffset.FromUnixTimeSeconds(long.Parse(jwtSecurityToken.Claims.First(x => x.Type == "exp").Value, CultureInfo.InvariantCulture));
				return (authenticateResponse.Token, expiry.UtcDateTime, dateTimeProvider.UtcNow.AddSeconds(xTRestSettings.Get(xTServerName).WorkspaceTimeoutInSeconds));
			}

			throw new InvalidOperationException($"Failed to log on Alarm server via REST API. StatusCode: ${response.StatusCode}");
		}

		public async Task<CTM[]> ListActiveCTMFolderWithDetailsAsync(string xTServerName, string folder, CancellationToken cancellationToken = default) => await ListCTM(xTServerName, AlarmServer.StructuralFilter.Folders, new[] { folder },
			AlarmServer.StateFilterCases.OnlyActive, Array.Empty<CTM_Type>(), AlarmServer.DetailsOption.WithDetails, cancellationToken);

		async Task<CTM[]> ListCTM(string xTServerName, AlarmServer.StructuralFilter structFilter, string[] structFilterArgs, AlarmServer.StateFilterCases stateFilterCases, CTM_Type[] ctmTypes, AlarmServer.DetailsOption detailsOption, CancellationToken cancellationToken)
		{
			bool details = (detailsOption == AlarmServer.DetailsOption.WithDetails);

			var kpl = new List<KeyValuePair<string, string>> { new("details", details.ToString()) };

			string filterCode = string.Empty;
			switch (structFilter)
			{
				case AlarmServer.StructuralFilter.Ids: { filterCode = "ids"; } break;
				case AlarmServer.StructuralFilter.Owners: { filterCode = "owners"; } break;
				case AlarmServer.StructuralFilter.Folders: { filterCode = "folders"; } break;
				case AlarmServer.StructuralFilter.NotUsed: { } break;
			}

			if (!string.IsNullOrEmpty(filterCode))
			{
				foreach (string argIterator in structFilterArgs)
				{
					var kp = new KeyValuePair<string, string>(filterCode, argIterator);
					kpl.Add(kp);
				}
			}

			switch (stateFilterCases)
			{
				case AlarmServer.StateFilterCases.OnlyActive:
					{
						kpl.Add(new KeyValuePair<string, string>("onlyActive", "True"));
					}
					break;
				case AlarmServer.StateFilterCases.HideSilent:
					{
						kpl.Add(new KeyValuePair<string, string>("hideSilent", "True"));
					}
					break;
				case AlarmServer.StateFilterCases.AllEnabled:
					{
						// not necessary
						kpl.Add(new KeyValuePair<string, string>("hideSilent", "False"));
					}
					break;
				case AlarmServer.StateFilterCases.ShowDisabled:
					{
						kpl.Add(new KeyValuePair<string, string>("showDisabled", "True"));
					}
					break;
			}

			foreach (CTM_Type s in ctmTypes)
			{
				var kp = new KeyValuePair<string, string>("ctmTypes", AlarmServer.CtmTypeDict[(int)s]);
				kpl.Add(kp);
			}

			var key = $"{xTServerName}-{XTAlarmRestTokenKey}";
			using FormUrlEncodedContent c = new FormUrlEncodedContent(kpl);
			var uri = $"{xTRestSettings.Get(xTServerName).AlarmServerBaseUrl}/CTM/v1/ListCTM?{await c.ReadAsStringAsync(cancellationToken)}";
			(string Token, DateTime TokenExpiry, DateTime WorkspaceExpiry) tokenValue = authenticationTokenDict[key];
			using var requestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
			{
				Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenValue.Token) }
			};
			var response = await SendWithWorkspaceExpiryUpdateAsync(requestMessage, key, cancellationToken);

			return await ProcessResponse<CTM[]>(response, cancellationToken);
		}

		internal async Task<XtObject[]> ListObjectsAsync(string xTServerName, string parent = "", bool inherit = true, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrEmpty(parent))
			{
				parent = "xt-folder:/";
			}

			Dictionary<string, string> dict = new Dictionary<string, string>
			{
				["parent"] = parent,
				["inheritedfields"] = inherit.ToString().ToLower(CultureInfo.InvariantCulture)
			};

			var key = $"{xTServerName}-{XTRestTokenKey}";
			using FormUrlEncodedContent formContent = new FormUrlEncodedContent(dict);
			(string Token, DateTime TokenExpiry, DateTime WorkspaceExpiry) tokenValue = authenticationTokenDict[key];
			using var requestMessage = new HttpRequestMessage(HttpMethod.Get,
				$"{xTRestSettings.Get(xTServerName).XTRestBaseUrl}/ConfigApi/v1/objects?{await formContent.ReadAsStringAsync(cancellationToken)}")
			{
				Headers = { Authorization = new AuthenticationHeaderValue("Bearer", tokenValue.Token) }
			};
			var response = await SendWithWorkspaceExpiryUpdateAsync(requestMessage, key, cancellationToken);

			return await ProcessResponse<XtObject[]>(response, cancellationToken);
		}

		Task<HttpResponseMessage> SendWithWorkspaceExpiryUpdateAsync(HttpRequestMessage request, string authTokenKey, CancellationToken cancellationToken)
		{
			return httpClient.SendAsync(request, cancellationToken).ContinueWith(sendTask =>
			{
				var response = sendTask.Result;
				if (response.IsSuccessStatusCode)
				{
					(string Token, DateTime TokenExpiry, DateTime WorkspaceExpiry) tokenValue = authenticationTokenDict[authTokenKey];
					var workspaceExpiry = dateTimeProvider.UtcNow.AddSeconds(xTRestSettings.Get(authTokenKey.Split("-")[0]).WorkspaceTimeoutInSeconds);
					tokenValue.WorkspaceExpiry = workspaceExpiry > tokenValue.TokenExpiry ? tokenValue.TokenExpiry : workspaceExpiry;
					authenticationTokenDict[authTokenKey] = tokenValue;
				}
				else if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Unauthorized)
				{
					authenticationTokenDict.TryRemove(authTokenKey, out _);
				}

				return response;
			}, cancellationToken);
		}

		[SuppressMessage("Design", "CA2201:Exception type is not sufficiently specific", Justification = "Please use more specific exception type if you update this function.")]
		async Task<T> ReadObjectResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
		{
			if (response?.Content == null)
			{
				return default;
			}

			var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
			try
			{
				var typedBody = JsonConvert.DeserializeObject<T>(responseText, new JsonSerializerSettings());
				return typedBody;
			}
			catch (JsonException exception)
			{
				var message = $"Could not deserialize the response body as {typeof(T).FullName}. ResponseStatus: {(int)response.StatusCode}. ExceptionMessage: {exception.Message}";
				throw new Exception(message);
			}
		}

		[SuppressMessage("Design", "CA2201:Exception type is not sufficiently specific", Justification = "Please use more specific exception type if you update this function.")]
		async Task<T> ProcessResponse<T>(HttpResponseMessage response, CancellationToken cancellationToken, [CallerMemberName] string caller = "")
		{
			if (response.IsSuccessStatusCode)
			{
				if (response.StatusCode == HttpStatusCode.OK)
				{
					return await ReadObjectResponseAsync<T>(response, cancellationToken);
				}

				return default;
			}

			var errorDetail = string.Empty;
			switch (response.StatusCode)
			{
				case HttpStatusCode.BadRequest:
					var problemDetail = await ReadObjectResponseAsync<ProblemDetails>(response, cancellationToken);
					errorDetail = $" Detail: \"{problemDetail?.Detail}\"";
					break;
				case HttpStatusCode.InternalServerError:
					var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
					errorDetail = string.IsNullOrEmpty(errorMessage) ? errorDetail : $" Detail: \"{errorMessage}\"";
					break;
			}

			throw new Exception($"Failed to process '{caller}' request. Status: {response.StatusCode}.{errorDetail}");
		}
	}
}
