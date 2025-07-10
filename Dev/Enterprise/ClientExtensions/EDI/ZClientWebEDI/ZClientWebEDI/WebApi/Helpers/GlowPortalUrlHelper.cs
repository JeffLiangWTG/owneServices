using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Http;
using CargoWise.Authentication.Primitives;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.CustomerService.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public class GlowPortalUrlHelper
	{
		public GlowPortalUrlHelper(ITokenizedAccessControl accessControl)
		{
			AccessControl = accessControl;
		}
		readonly ITokenizedAccessControl AccessControl;

		public static class ServerOnlyQueryStringKeys
		{
			public const string SSOKey = "sso_otp";
			public const string Product = "Product";
			public const string Criticality = "Criticality";
		}

		public Uri GetNewRequestUrl(Guid contactPK, Guid userAccountPK, string licenseCode, string product, string module, string subModule, string criticality, string referenceId)
		{
			var portalPageUri = EDIDataRegistry.Instance.GlowNewERequestPageUri.Value;
			var token = CreateToken(contactPK);
			var builder = CreatePortalUriBuilder(portalPageUri, token);
			var queryParameters = UriExtensions.ParseQueryString(builder.Uri);

			var sourceModule = EDIDataRegistry.Instance.SourceModules.Value.GetSourceModule(subModule, ModuleListType.DetectedMenuItem, product);
			if (sourceModule != null && !string.IsNullOrEmpty(sourceModule.DefaultModule))
			{
				module = sourceModule.DefaultModule;
			}

			AddOrReplaceKeyValue(queryParameters, StaffContactValueObjectHelper.QueryStringKeys.LicenseCode, licenseCode);
			AddOrReplaceKeyValue(queryParameters, ServerOnlyQueryStringKeys.Product, product);
			AddOrReplaceKeyValue(queryParameters, StaffContactValueObjectHelper.QueryStringKeys.Module, module);
			AddOrReplaceKeyValue(queryParameters, StaffContactValueObjectHelper.QueryStringKeys.SubModule, subModule);
			AddOrReplaceKeyValue(queryParameters, ServerOnlyQueryStringKeys.Criticality, criticality);
			AddOrReplaceKeyValue(queryParameters, StaffContactValueObjectHelper.QueryStringKeys.ReferenceId, referenceId);
			builder.Query = queryParameters.ToString();
			var url = builder.Uri;
			return GetAutoLoginUrl(url, contactPK, userAccountPK);
		}

		public Uri GetRequestPortalUrl()
		{
			var portalPageUri = EDIDataRegistry.Instance.GlowERequestPortalUri.Value;
			var builder = new UriBuilder(new Uri(EDIDataRegistry.Instance.GlowPortalRootUrl.Value.TrimEnd('/') + "/" + portalPageUri.TrimStart('/')));
			return builder.Uri;
		}

		public Uri GetRequestPortalUrl(Guid contactPK, Guid userAccountPK)
		{
			var portalPageUri = EDIDataRegistry.Instance.GlowERequestPortalUri.Value;
			var token = CreateToken(contactPK);
			var builder = CreatePortalUriBuilder(portalPageUri, token);
			return GetAutoLoginUrl(builder.Uri, contactPK, userAccountPK);
		}

		public Uri GetEditRequestUrl(Guid contactPK, Guid userAccountPK, Guid incidentPK)
		{
			var portalPageUri = EDIDataRegistry.Instance.GlowEditERequestPageUri.Value;
			portalPageUri = portalPageUri.Replace(EDIDataRegistry.GlowEditERequestPageUri_PkMacro, incidentPK.ToString());
			var token = CreateToken(contactPK);
			var builder = CreatePortalUriBuilder(portalPageUri, token);
			return GetAutoLoginUrl(builder.Uri, contactPK, userAccountPK);
		}

		public Uri GetAccreditationPortalUrl(Guid contactPK, Guid userAccountPK)
		{
			var portalPageUri = EDIDataRegistry.Instance.GlowAccreditationPortalUri.Value;
			var token = CreateToken(contactPK);
			var builder = CreatePortalUriBuilder(portalPageUri, token);
			return GetAutoLoginUrl(builder.Uri, contactPK, userAccountPK);
		}

		public Uri GetGenericPortalWithAutoLogin(string baseUrl, Guid contactPK, Guid userAccountPK)
		{
			var token = CreateToken(contactPK);
			var builder = CreateGenericUriBuilder(baseUrl, token);
			return GetAutoLoginUrl(builder.Uri, contactPK, userAccountPK);
		}

		public bool IsOwnGlowPortalUrl(string url)
		{
			var ownGlowPortalUrl = EDIDataRegistry.Instance.GlowPortalRootUrl.Value;
			return url.StartsWith(ownGlowPortalUrl);
		}

		Uri GetAutoLoginUrl(Uri glowUrl, Guid contactPK, Guid userAccountPK)
		{
			var scope = FormattableString.Invariant($"{userAccountPK}");
			var myAccountAccessToken = AccessControl.CreateLimitedToken(AccessTokenTypes.GlowAutoLogin, new AccessTokenInfo(scope, contactPK, OrgContactSchema.Constants.Prefix), TimeSpan.FromMinutes(5), maxUses: 1);

			var secureQueryString = new SecureQueryString
			{
				["glowUrl"] = glowUrl.ToString(),
				["token"] = myAccountAccessToken
			};

			var basePath = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value;
			var queryString = FormattableString.Invariant($"?{SecureQueryString.QueryStringKey}={WebUtility.UrlEncode(secureQueryString.ToString())}");
			var path = basePath.TrimEnd('/') + "/Login/GlowPortalAutoLogin.aspx" + queryString;
			var builder = new UriBuilder(path) { Port = -1 };
			return builder.Uri;
		}

		public static ZGuid IncidentRequestPkFromNumber(string incidentNumber)
		{
			if (string.IsNullOrEmpty(incidentNumber))
			{
				return ZGuid.Empty;
			}

			const string sql = "select INC_PK from dbo.IncidentRequest where INC_IncidentNumber = @Incident";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.AddParameterBasedOnDbColumn("@Incident", incidentNumber, IncidentRequestSchema.INC_IncidentNumber);
				var val = cmd.ExecuteScalar();
				return val == DBNull.Value ? ZGuid.Empty : new ZGuid((Guid)val);
			}
		}

		string CreateToken(Guid contactPK)
		{
			var scope = new LocalIdentityTokenScope()
			{
				BranchKey = Env.CurrentBranchPK,
				DepartmentKey = Env.CurrentDepartmentPK
			};

			var scopeObject = JsonConvert.SerializeObject(scope);
			var tokenInfo = new AccessTokenInfo(scopeObject, contactPK, OrgContactSchema.Constants.Prefix);

			return AccessControl.CreateLimitedToken(AccessTokenTypes.LocalIdentity, tokenInfo, TimeSpan.FromMinutes(5), 1, TokenizedAccessControlExtensions.DefaultCharacterSet, 8);
		}

		UriBuilder CreatePortalUriBuilder(string portalPageUri, string token) => CreateGenericUriBuilder(EDIDataRegistry.Instance.GlowPortalRootUrl.Value.TrimEnd('/') + "/" + portalPageUri.TrimStart('/'), token);

		UriBuilder CreateGenericUriBuilder(string baseUri, string token)
		{
			var builder = new UriBuilder(baseUri);
			var queryParameters = UriExtensions.ParseQueryString(builder.Uri);
			AddOrReplaceKeyValue(queryParameters, ServerOnlyQueryStringKeys.SSOKey, token);
			builder.Query = queryParameters.ToString();
			return builder;
		}

		void AddOrReplaceKeyValue(NameValueCollection queryParameters, string key, string value)
		{
			if (!string.IsNullOrEmpty(value))
			{
				queryParameters.Remove(key);
				queryParameters.Add(key, Uri.EscapeDataString(value));
			}
		}
	}
}
