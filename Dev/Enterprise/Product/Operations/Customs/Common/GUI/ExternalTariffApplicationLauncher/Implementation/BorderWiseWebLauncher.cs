using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Environment;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.GUI
{
	internal sealed class BorderWiseWebLauncher : IExternalTariffApplicationLauncher
	{
		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "We use ConcurrentDictionary so it is thread safe")]
		public static ConcurrentDictionary<string, RedirectUrlResponse> AutoLoginURLs = new ConcurrentDictionary<string, RedirectUrlResponse>();

		public void LaunchExternalApplication(BorderWiseFilters filters, ZGuid businessEntityPk, ZGuid? webSocketClientId = null, string jobPk = null)
		{
			if (filters != null)
			{
				filters.ReturnUri = new Uri(ShowEditFormUrlHandler.Instance.Create(ControllerIDs.BorderWiseWebReturnHook, businessEntityPk) + ArgsUriFullParameter);
			}

			var mode = string.IsNullOrEmpty(jobPk) ? TariffClassificationMode.Single : TariffClassificationMode.Batch;

			ShowBorderWiseWebApp(filters, mode, webSocketClientId.ToString(), jobPk);
		}

		public void LaunchExternalApplication(BorderWiseFilters filters)
		{
			ShowBorderWiseWebApp(filters, mode: TariffClassificationMode.Single);
		}

		void ShowBorderWiseWebApp(BorderWiseFilters filters, string mode, string webSocketClientId = "", string jobPk = null)
		{
			var url = string.Empty;

			if (!string.IsNullOrWhiteSpace(BaseBorderWiseUmpApiUrl))
			{
				url = GetAutoLoginUrlFromUserManagementPortal();
			}

			if (string.IsNullOrEmpty(url))
			{
				url = BaseBorderWiseAppPath;
			}

			var parameters = GetUrlParameters(filters, webSocketClientId, mode, jobPk);

			if (!string.IsNullOrEmpty(parameters))
			{
				var concatCharacter = url.Contains("?") ? '&' : '?';

				url += concatCharacter + parameters;
			}

			if (url.Length > 2000)
			{
				ErrorReporter.ReportOnce("URL is too long for many browsers: " + url);
			}

			WebUrlLauncher.Launch(url);
		}

		string GetUrlParameters(BorderWiseFilters filters, string webSocketClientId, string mode, string jobPk)
		{
			var result = new StringBuilder();

			void Append(string key, string value)
			{
				if (!string.IsNullOrEmpty(value))
				{
					if (result.Length > 0)
					{
						result.Append("&");
					}

					result.Append(key);
					result.Append("=");
					result.Append(WebUtility.UrlEncode(value));
				}
			}

			if (filters != null)
			{
				if (!string.IsNullOrEmpty(filters.Tariff))
				{
					Append(TariffParameterName, filters.Tariff);
					Append(StatParameterName, filters.Stat);
				}

				if (filters.ImpExp != null)
				{
					Append(ImportExportFlagParameterName, filters.ImpExp);
				}

				if (filters.AddCountryParameter)
				{
					Append(CountryParameterName, string.IsNullOrEmpty(filters.CountryCodeOverride) ? (string)GlbCompany.CurrentCompany?.GC_RN_NKCountryCode : filters.CountryCodeOverride);
				}

				if (filters.RequestId != null)
				{
					Append(RequestIdParameterName, filters.RequestId);
				}

				if (filters.FocusedCommodity != null)
				{
					Append(FocusedCommodityParameterName, filters.FocusedCommodity);
				}

				if (filters.ReturnUri != null && mode == TariffClassificationMode.Single)
				{
					Append(ReturnUrlParameterName, filters.ReturnUri.OriginalString);
				}
			}

			if (!string.IsNullOrWhiteSpace(webSocketClientId))
			{
				Append(Version, BorderWiseTariffModelVersion);
				Append(Mode, mode);
				Append(WebSocketClientIdParameterName, webSocketClientId);
				Append(WebSocketTimestampParameterName, ZDateTime.UtcNow.ToString(DateFormat));
				if (!string.IsNullOrEmpty(jobPk))
				{
					Append(JobPkParameterName, jobPk);
				}
			}

			return result.ToString();
		}

		#region User Management Portal

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Second access is ok in this case.")]
		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		string GetAutoLoginUrlFromUserManagementPortal()
		{
			var emailAddress = Env.CurrentUser?.EmailAddress;
			var loggedInOrganisationPK = Env.CurrentCompany?.OrganisationPK;
			var staffCode = default(string);
			var result = string.Empty;
			var currentUserPk = Env.CurrentUser?.PK;
			var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-");
			var versionNumber = ReleaseInfo.Instance.VersionNumber.ToString();
			var releaseRing = ReleaseInfo.Instance.ReleaseRing;

			if (currentUserPk != null)
			{
				var factory = new BusinessObjectFactory();
				var glbStaff = factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.PK, currentUserPk));
				staffCode = glbStaff?.GS_Code;
			}
			var databaseNumber = ObjectFactory.Get<IProductRegistration>().Key.DatabaseNumber;

			if (!string.IsNullOrEmpty(emailAddress) && loggedInOrganisationPK != null)
			{
				var loggedInOrgProxy = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseWebLauncher) }.Load<OrgHeader>(loggedInOrganisationPK.Value);

				if (!string.IsNullOrEmpty(loggedInOrgProxy?.OH_Code))
				{
					var secureQueryString = new SecureQueryString
					{
						{ UserEmailKey, emailAddress },
						{ OrgCodeKey, loggedInOrgProxy.OH_Code },
						{ StaffCodeKey, staffCode },
						{ DatabaseNumberKey, databaseNumber.ToString(CultureInfo.InvariantCulture) },
						{ LicenseCodeKey, licenseCode },
						{ ReleaseRingKey , releaseRing },
						{ CWVersionNumberKey, versionNumber },
					};

					var cacheKey = secureQueryString.ToString();
					if (AutoLoginURLs.TryGetValue(cacheKey, out var cachedResponse) && cachedResponse.TokenExpiry > DateTime.UtcNow)
					{
						result = cachedResponse.Url;
					}

					if (string.IsNullOrEmpty(result))
					{
						var redirectUrlResponse = GetAutoLoginUrl(secureQueryString);
						if (redirectUrlResponse != null)
						{
							if (!string.IsNullOrEmpty(redirectUrlResponse.Url) &&
								redirectUrlResponse.Url.IndexOf(UmpUnAuthorized, StringComparison.OrdinalIgnoreCase) < 0)
							{
								AutoLoginURLs.AddOrUpdate(cacheKey, redirectUrlResponse, (key, current) => redirectUrlResponse);
							}
							result = redirectUrlResponse.Url;
						}
					}
				}
			}
			return result;
		}

		RedirectUrlResponse GetAutoLoginUrl(SecureQueryString secureQueryString)
		{
#if DEBUG
			if (OverriddenResponse_ForTest.IsOverriden)
			{
				return OverriddenResponse_ForTest.Value;
			}
#endif

			try
			{
				if (Uri.TryCreate(BaseBorderWiseUmpApiUrl.TrimEnd('/') + GetAutoLoginApiUrl + WebUtility.UrlEncode(secureQueryString.ToString()), UriKind.Absolute, out var requestUri))
				{
					using (var client = HttpMessageHandler == null ? new HttpClient() : new HttpClient(HttpMessageHandler))
					{
						var task = client.GetAsync(requestUri, CancellationToken.None);

						if (!task.Wait(20000, CancellationToken.None))
						{
							return null;
						}
						if (!task.Result.IsSuccessStatusCode)
						{
							return null;
						}

						var content = task.Result.Content.ReadAsStringAsync().Result;
						var response = Utilities.DeserializeFromJson<RedirectUrlResponse>(content);

						return response;
					}
				}
				else
				{
					UserNotification.Instance.ShowError(Res.GetString("b9118cbc-eea3-4559-9406-2ef4f4ee6d8a", "Invalid URI settings in Registry '{0}'.", RawDataRegistry.Instance.BorderWiseUmpApiBaseAddress.HumanReadableRegistryPath()));
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				ErrorReporter.ReportOnce("Logging into BorderWise Web App failed with: " + ex.Message, ex);
			}

			return null;
		}

		#endregion

		#region Constants

		internal static string BaseBorderWiseAppPath => ZArchitecture.Environment.DataRegistry.Instance.BorderWiseWebAddress;
		internal static string BaseBorderWiseUmpApiUrl => ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress;

		public const string UserEmailKey = "UserEmail";
		public const string OrgCodeKey = "OrgCode";
		public const string StaffCodeKey = "StaffCode";
		public const string LicenseCodeKey = "LicenseCode";
		public const string DatabaseNumberKey = "DatabaseNumber";
		public const string ReleaseRingKey = "ReleaseRing";
		public const string CWVersionNumberKey = "CWVersionNumber";

		const string DateFormat = "yyyyMMddHHmmssfff";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Web address")]
		const string GetAutoLoginApiUrl = "/v1/auth/autoLogin/url?securedQueryString=";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string ArgsUriFullParameter = "&Args=";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string TariffParameterName = "tariff";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string StatParameterName = "stat";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string ImportExportFlagParameterName = "impexp";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string CountryParameterName = "c";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string RequestIdParameterName = "requestId";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string FocusedCommodityParameterName = "focusedCommodity";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string ReturnUrlParameterName = "ret";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string WebSocketClientIdParameterName = "WebSocketClientId";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string JobPkParameterName = "JobPk";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string WebSocketTimestampParameterName = "WebSocketTimestamp";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string Version = "version";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string BorderWiseTariffModelVersion = "2";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query parameter name")]
		const string Mode = "mode";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Parameter value in Url")]
		const string UmpUnAuthorized = "NotAuthorized";

		public HttpMessageHandler HttpMessageHandler { get; set; }

		#endregion

#if DEBUG
		internal readonly Overridable<RedirectUrlResponse> OverriddenResponse_ForTest = new Overridable<RedirectUrlResponse>();
#endif
	}
}
