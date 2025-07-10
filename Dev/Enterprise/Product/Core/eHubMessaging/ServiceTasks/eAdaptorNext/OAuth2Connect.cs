using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;

namespace Enterprise.eHubMessaging.ServiceTasks
{
	public static class OAuth2Connect
	{
		public async static Task<AuthToken> RequestAuthToken(IOAuth2Parameters config, CancellationToken cancellationToken)
		{
			Argument.NotNull(config, nameof(config));
			try
			{
				AuthToken token = null;
				using (var client = HttpClientProvider.GetClient())
				{
					var authorization = AuthorizationGrantType.Create(config);

					bool valid = Uri.TryCreate(authorization.AuthorizationURL, UriKind.Absolute, out var uriResult);
					bool isHttps = valid && (uriResult.Scheme == Uri.UriSchemeHttps);
					if (!valid)
					{
						throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|MalformedURL", "Malformed Authorization URL."));
					}
					else if (!isHttps)
					{
						throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|NonHttpsURL", "Authorization URL must be https."));
					}

					var request = new HttpRequestMessage(HttpMethod.Post, uriResult);
					request.Content = new FormUrlEncodedContent(authorization.GenerateContent());

					var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
					await response.EnsureSuccessStatusCodeAsync();

					var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
					token = JsonConvert.DeserializeObject<AuthToken>(payload);

					//For some reason its a 200 but they didnt respond properly
					if (token == null || token.AccessToken == null)
					{
						throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|InvalidSuccessRespone", "Invalid OAuth2.0 Success Response.\r\nResponse: {0}", payload));
					}
				}
				return token;
			}
			catch (OAuth2Exception ex)
			{
				ErrorResponseHandler(ex);
				throw;
			}
			catch (SimpleHttpResponseException ex)
			{
				ErrorResponseHandler(ex);
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|NonStandardErrorRespone", "Authorization URL returned a {0} {1} response.\r\nResponse content: {2}", (int)ex.StatusCode, ex.StatusCode, ex.Message), ex);
			}
			catch (InvalidOperationException ex) when (ex.Message.Contains((NoResString)"An invalid request URI was provided."))
			{
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|UnvalidURLProvided", "Authorization URL provided was invalid."), ex);
			}
			catch (HttpRequestException ex)
			{
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|ErrorSendingRequest", "Error sending request to Authorization URL."), ex);
			}
			catch (JsonSerializationException ex)
			{
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|InvalidSuccessRespone", "Invalid OAuth2.0 Success Response.\r\nResponse: {0}", ex.Message), ex);
			}
			catch (TaskCanceledException ex)
			{
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|RequestTimeout", "The request timed out."), ex);
			}
			catch (ArgumentException ex) when (ex.Message.Contains((NoResString)"unknown FlowCode"))
			{
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|InvalidGrantType", "An authorization flow code must be specified"), ex);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("OAuth2Connect|RequestAuthToken", "An unexpected error has occurred.", ex);
				throw new OAuth2Exception(ResString.GetMultilingualString("OAuth2Connect|UnexpectedError", "An unexpected error has occurred."), ex);
			}
		}

		static void ErrorResponseHandler(Exception ex)
		{
			try
			{
				var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(ex.Message);
				if (errorResponse != null)
				{
					throw new OAuth2Exception(errorResponse, ex);
				}
			}
			catch (JsonException)
			{
				//Do Nothing
			}
		}
	}
}
