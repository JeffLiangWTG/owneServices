using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using Newtonsoft.Json;
using WTG.TrustedMessaging.MyAccount.Models;

namespace Enterprise.Registry.Business
{
	public class TokenAuthOnboardingService : ITokenAuthOnboardingService
	{
		public TokenAuthOnboardingService() : this(new SystemToSystemTrustService())
		{
		}

		public TokenAuthOnboardingService(SystemToSystemTrustService trustService)
		{
			systemToSystemTrustService = trustService;
		}

		readonly SystemToSystemTrustService systemToSystemTrustService;

		public TokenAuthOnboardingDataResponse FetchOidcConfig()
		{
			if (GetRequestResult(FetchConfigEndpoint, out var responseContent))
			{
				return JsonConvert.DeserializeObject<TokenAuthOnboardingDataResponse>(responseContent);
			}
			else
			{
				throw new TokenAuthOnboardingApiException(responseContent, null);
			}
		}

		public bool EnableTokenAuthentication()
		{
			if (GetRequestResult(EnableOidcEndpoint, out var responseContent))
			{
				return true;
			}
			else
			{
				throw new TokenAuthOnboardingApiException(responseContent, null);
			}
		}

		bool GetRequestResult(string endpoint, out string responseContent)
		{
			try
			{
				using (var httpClient = new HttpClient())
				{
					var token = systemToSystemTrustService.GetAccessToken();
					httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue((NoResString)"Bearer", token);

					var httpResponseMessage = httpClient.GetAsync(endpoint).ConfigureAwait(false).GetAwaiter().GetResult();
					responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

					return httpResponseMessage.StatusCode == HttpStatusCode.OK;
				}
			}
			catch (HttpRequestException ex) when (ex.InnerException is WebException webException)
			{
				throw new TokenAuthOnboardingApiException(ex.Message, webException);
			}
		}

		string FetchConfigEndpoint => OnboardingEndpoint + "/fetch/" + DatabaseNumber;

		string EnableOidcEndpoint => OnboardingEndpoint + "/enable/" + DatabaseNumber;

		int DatabaseNumber => ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber;

		string OnboardingEndpoint => $"{WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value.TrimEnd('/')}/api/TokenAuthOnBoarding/oidcconfig";
	}

	public interface ITokenAuthOnboardingService
	{
		TokenAuthOnboardingDataResponse FetchOidcConfig();
		bool EnableTokenAuthentication();
	}

	[Serializable]
	public class TokenAuthOnboardingApiException : Exception
	{
		public TokenAuthOnboardingApiException(string message, Exception inner) : base(message, inner)
		{
		}

#if NETFRAMEWORK
		protected TokenAuthOnboardingApiException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{ }
#endif
	}
}
