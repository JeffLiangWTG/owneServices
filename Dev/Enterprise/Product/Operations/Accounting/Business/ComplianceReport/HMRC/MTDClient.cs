using System;
#if NETFRAMEWORK
using System.Linq;
#endif
using System.Net;
using System.Net.Http;
using CargoWise.Application;
using CargoWise.Common.JSON.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core.Diagnostics;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	#region SuppressResourceStringsCheckRegion

	public class MTDClient
	{
		public MTDClient(AccComplianceReport report)
		{
			ComplianceReport = report;
			HasAlreadyAttemptedObtainTokenEnquire = false;
			Tracer = ObjectFactory.Get<ITracer>();
		}

		public TResponseContent SendRequest<TResponseContent>(MTDRequestBase request)
		{
			var response = default(TResponseContent);

			if (IsMTDApplicable)
			{
				if (!request.NeedAccessTokenToExecuteTheRequest || !string.IsNullOrEmpty(AccessToken))
				{
					response = SendRequestCore<TResponseContent>(request);
				}

				if (request.NeedAccessTokenToExecuteTheRequest && (string.IsNullOrEmpty(AccessToken) || (WasLastRequestUnauthorized && !HasAlreadyAttemptedObtainTokenEnquire)))
				{
					var result = EnquireAccessAndRefreshTokens();
					if (result.IsSuccessful)
					{
						response = SendRequest<TResponseContent>(request);
					}
					else
					{
						LastErrorInfo = new MTDErrorInfo()
						{
							code = "No Authorization Code",
							message = result.ErrorMessage
						};

						WasLastRequestUnauthorized = true;
					}
				}
			}

			return response;
		}

		TResponseContent SendRequestCore<TResponseContent>(MTDRequestBase mtdRequest)
		{
			LastErrorInfo = null;
			WasLastRequestUnauthorized = false;
			LastHttpStatusCode = null;

			HttpResponseMessage httpResponse = null;

			if (mtdRequest.Method == HttpMethod.Get || mtdRequest.Method == HttpMethod.Post)
			{
				using (var httpRequest = mtdRequest.GetAsHttpRequest(Client))
				{
					Tracer.TraceInformation(AccountingTraceSourceCodes.Http, () => httpRequest.GetAsString());

					httpResponse = Client.SendAsync(httpRequest).Result;

					Tracer.TraceInformation(AccountingTraceSourceCodes.Http, () => httpResponse.GetAsString());
				}
			}
			else
			{
				Tracer.TraceErrorEvent(AccountingTraceSourceCodes.Http, () => FormattableString.Invariant($"HMRC MTD Service does not support http {mtdRequest.Method} method"));
			}

			var mtdResponse = CreateResponse(httpResponse);
			var response = TryToProcessResponse<TResponseContent>(mtdResponse);
			return response;
		}

		public bool WasLastRequestUnauthorized { get; private set; }

		public MTDErrorInfo LastErrorInfo { get; private set; }

		public bool HasErrors()
		{
			return LastErrorInfo != null && !string.IsNullOrEmpty(LastErrorInfo.ToString());
		}

		public HttpStatusCode? LastHttpStatusCode { get; private set; }

		MTDHttpResponse CreateResponse(HttpResponseMessage httpResponse)
		{
			MTDHttpResponse result = null;

			if (httpResponse != null)
			{
				var responseContent = httpResponse.Content.ReadAsStringAsync().Result;
				result = new MTDHttpResponse(httpResponse.StatusCode, httpResponse.IsSuccessStatusCode, httpResponse.Headers, responseContent);
			}

			return result;
		}

		TContent TryToProcessResponse<TContent>(MTDHttpResponse response)
		{
			var content = default(TContent);

			if (response.IsSuccessStatusCode)
			{
				content = response.Content.FromJSON<TContent>();

				//I am not sure whether this is the best place
				if (content is MTDVATSubmitResponseContent resCnt)
				{
					resCnt.ReceiptID = response.GetReceiptNumber();
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(response.Content))
				{
					LastErrorInfo = response.Content.FromJSON<MTDErrorInfo>();
				}

				WasLastRequestUnauthorized = response.IsUnauthorizedStatusCode(LastErrorInfo?.IsAccessTokenRelatedError ?? false);
			}

			LastHttpStatusCode = response.StatusCode;
			return content;
		}

		public (bool IsSuccessful, string ErrorMessage) EnquireAccessAndRefreshTokens()
		{
			var result = false;
			var errorMessage = string.Empty;

			bool GotValidTokens((string AccessToken, string RefreshToken, string ErrorMessage) tokens) =>
				string.IsNullOrEmpty(tokens.ErrorMessage) &&
				!string.IsNullOrEmpty(tokens.AccessToken) &&
				!string.IsNullOrEmpty(tokens.RefreshToken);

			using (var mutex = GetMutex())
			{
				if (mutex.Lock())
				{
					var currentTokens = AccountingConfigurationRegistry.Instance.HMRCOAuthTokens.GetFallBackValueAtAllLevels(ComplianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty);
					var accessToken = string.Empty;
					var refreshToken = string.Empty;
					var tokens = (AccessToken: string.Empty, RefreshToken: string.Empty, ErrorMessage: string.Empty);

					var refreshTokenEnquireAttempted = false;
					if (!string.IsNullOrEmpty(currentTokens) && currentTokens.Contains(TokenSplitSymbol) && ShouldAttemptRefreshTokenEnquire)
					{
						tokens = RefreshTokens();
						refreshTokenEnquireAttempted = true;
					}

					if (!HasAlreadyAttemptedObtainTokenEnquire && (string.IsNullOrEmpty(currentTokens) || !GotValidTokens(tokens) || !ShouldAttemptRefreshTokenEnquire))
					{
						var authorisationCode = ComplianceReport.InvokeOAuthClientAuthorisationEvent();

						if (!string.IsNullOrEmpty(authorisationCode))
						{
							tokens = ObtainTokens(authorisationCode);
							RefreshTokenEnquireAttemptCount = 0;
							HasAlreadyAttemptedObtainTokenEnquire = true;
						}
						else
						{
							errorMessage = Res.GetString("44f551b2-340d-47bc-914f-58ff2db82ade", "Unable to obtain an HMRC MTD Authorization Code.");
						}
					}

					if (GotValidTokens(tokens))
					{
						accessToken = tokens.AccessToken;
						refreshToken = tokens.RefreshToken;
						result = true;
					}

					if (refreshTokenEnquireAttempted)
					{
						RefreshTokenEnquireAttemptCount++;
					}

					currentTokens = result ? accessToken + TokenSplitSymbol + refreshToken : string.Empty;
					AccountingConfigurationRegistry.Instance.HMRCOAuthTokens.SetValue(ComplianceReport.ACR_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, currentTokens);
				}
				else
				{
					errorMessage = Res.GetString("3a08e781-75fb-45b9-9e37-dde75b348d65", "Another user is already completing the authentication with HMRC. Please try again later.");
				}
			}

			return (result, errorMessage);
		}

		public static Uri OAuthUri => new Uri(FormattableString.Invariant($"{HostUrl}/oauth/authorize?response_type=code&client_id={ClientID}&scope=read:vat+write:vat&state=null&redirect_uri={RedirectUrl}"));
		public const string GrantPath = "/oauth/grantscope";
		public const string AuthCodePrefix = "Success code=";
		internal static string AuthorisationPart => FormattableString.Invariant($"client_secret={ClientSecret}&client_id={ClientID}");

		public const string RedirectUrl = "edient:Command=Acc.WebCallback?HMRC";
		static string ClientID => AccountingConfigurationRegistry.Instance.MTDClientID.Value;
		static string ClientSecret => AccountingConfigurationRegistry.Instance.MTDClientSecret.Value;

		(string AccessToken, string RefreshToken, string ErrorMessage) RefreshTokens()
		{
			var request = new MTDPostRefreshTokenRequest(this);
			return GetTokens(request);
		}

		(string AccessToken, string RefreshToken, string ErrorMessage) ObtainTokens(string authorisationCode)
		{
			var request = new MTDPostObtainTokenRequest(this, authorisationCode);
			return GetTokens(request);
		}

		(string AccessToken, string RefreshToken, string ErrorMessage) GetTokens(MTDRequestBase tokenRequest)
		{
			var result = (AccessToken: string.Empty, RefreshToken: string.Empty, ErrorMessage: string.Empty);

			var tokens = SendRequestCore<MTDTokensData>(tokenRequest);

			if (LastErrorInfo == null && tokens != null)
			{
				result.AccessToken = tokens.access_token;
				result.RefreshToken = tokens.refresh_token;
			}
			else
			{
				result.ErrorMessage = LastErrorInfo.ToString();
			}

			return result;
		}

		ZGlobalMutex GetMutex()
		{
			return new ZGlobalMutex(MutexIDs.HMRCOAuthAuthorisation, VATRegistrationNumber);
		}

		HttpClient Client => IsTestEnvironment ? HttpClientProvider.TestInstance.Client : HttpClientProvider.ProductionInstance.Client;

		static string HostUrl => IsTestEnvironment
			? AccountingConfigurationRegistry.Instance.MTDTestWebServiceUrl.Value
			: AccountingConfigurationRegistry.Instance.MTDProductionWebServiceUrl.Value;

		bool IsMTDApplicable => ComplianceReport.ReportCountryCode == Constants.CountryCodes.UnitedKingdom && ComplianceReport.ACR_ReportType == MakeTaxDigitalReportType;

		bool HasAlreadyAttemptedObtainTokenEnquire { get; set; }

		bool ShouldAttemptRefreshTokenEnquire => RefreshTokenEnquireAttemptCount < 1;

		int RefreshTokenEnquireAttemptCount { get; set; }

		public string VATRegistrationNumber => GetVATRegistrationNumber(ComplianceReport.Company, ComplianceReport.ReportCountryCode);

		public static bool IsTestEnvironment => !AccountingConfigurationRegistry.Instance.IsMTDProductionMode.Value;

		internal string AccessToken => GetAccessToken(ComplianceReport.ACR_GC_Company);

		internal string RefreshToken => GetRefreshToken(ComplianceReport.ACR_GC_Company);

		internal static string GetVATRegistrationNumber(GlbCompany company, ZString countryCode)
		{
			return company?.OrgProxy.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(RegistrationNumberCode, countryCode)?.OK_CustomsRegNo.Trim().Replace(" ", "") ?? string.Empty;
		}

		internal static string GetAccessToken(ZGuid companyPK)
		{
			var tokens = GetTokensFromRegistry(companyPK);
			return tokens.Contains(TokenSplitSymbol) ? tokens.Split(TokenSplitSymbol)[0] : string.Empty;
		}

		internal static string GetRefreshToken(ZGuid companyPK)
		{
			var tokens = GetTokensFromRegistry(companyPK);
			return tokens.Contains(TokenSplitSymbol) ? tokens.Split(TokenSplitSymbol)[1] : string.Empty;
		}

		static string GetTokensFromRegistry(ZGuid companyPK) => AccountingConfigurationRegistry.Instance.HMRCOAuthTokens.GetFallBackValueAtAllLevels(companyPK.ToGuid(), Guid.Empty, Guid.Empty);

		internal AccComplianceReport ComplianceReport { get; }

		const char TokenSplitSymbol = ':';
		const string MakeTaxDigitalReportType = "MTD";
		const string RegistrationNumberCode = "VAT";

		internal FraudPreventionDataProvider FraudPreventionData => fraudPreventionData ?? (fraudPreventionData = new FraudPreventionDataProvider());
		FraudPreventionDataProvider fraudPreventionData;

		readonly ITracer Tracer;

#if DEBUG
		public void SetFraudPreventionData_ForTestOnly(FraudPreventionDataProvider provider) => fraudPreventionData = provider;
#endif

		internal class HttpClientProvider
		{
			public static HttpClientProvider ProductionInstance => productionInstance ?? (productionInstance = new HttpClientProvider(false));
			[ThreadStatic]
			static HttpClientProvider productionInstance;

			public static HttpClientProvider TestInstance => testInstance ?? (testInstance = new HttpClientProvider(true));
			[ThreadStatic]
			static HttpClientProvider testInstance;

			HttpClientProvider(bool isTestEnvironment)
			{
				Client = new HttpClient();
				Client.BaseAddress = new Uri(isTestEnvironment ? AccountingConfigurationRegistry.Instance.MTDTestWebServiceUrl.Value : AccountingConfigurationRegistry.Instance.MTDProductionWebServiceUrl.Value);
			}

			public HttpClient Client
			{
				get;
#if DEBUG
				private set;
#endif
			}

#if DEBUG
			public void SetClient_ForTestOnly(HttpClient client) => Client = client;
#endif
		}
	}

	#endregion
}
