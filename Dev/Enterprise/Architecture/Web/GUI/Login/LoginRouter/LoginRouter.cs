using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Login
{
	public abstract class LoginRouter
	{
		protected LoginRouter(Uri originalUrl, OrgContact contact, ZGlobal appInstance = null) : this(originalUrl)
		{
			appInstance?.SetupSession(null, EventArgs.Empty, false);
			var token = LoginRouterIdentityManager.GenerateToken(contact);
			InitialiseIdentityManager(token);
		}

		protected LoginRouter(Uri originalUrl, string token) : this(originalUrl)
		{
			InitialiseIdentityManager(token);
		}

		protected LoginRouter(Uri originalUrl)
		{
			OriginalUrl = originalUrl;
		}

		protected void InitialiseIdentityManager(string token)
		{
			IdentityManager = GetNewIdentityManager();
			IdentityManager.PopulatePropertiesFromToken(token);
		}

		protected virtual LoginRouterIdentityManager GetNewIdentityManager()
		{
			return new LoginRouterIdentityManager(Factory);
		}

		protected Uri OriginalUrl { get; }

		protected LoginRouterIdentityManager IdentityManager { get; private set; }

		protected BusinessObjectFactory Factory { get; } = new BusinessObjectFactory();

		protected OrgContact Contact => IdentityManager.Contact;

		public const string IdentityTokenQueryStringKey = "IdentityToken"; // Query string
		public const string OriginalUrlQueryStringKey = "OriginalUrl"; // Query string
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Query string")]
		public const string QueryStringKey = "udata";
		const string UserDefinedQueryParameterKeyPrefix = "UDF_"; // Query string

		public static Uri GetOriginalUrlFromRequest(HttpRequest request)
		{
			var queryStringData = request.QueryString[QueryStringKey];
			if (!string.IsNullOrEmpty(queryStringData))
			{
				SecureQueryString queryString = null;
				try
				{
					queryString = new SecureQueryString(WebUtility.UrlDecode(queryStringData));
				}
				catch (QueryStringException) { }

				if (queryString != null)
				{
					var originalRequestUrl = queryString[OriginalUrlQueryStringKey];
					if (!string.IsNullOrWhiteSpace(originalRequestUrl))
					{
						return new Uri(originalRequestUrl, UriKind.RelativeOrAbsolute);
					}
				}
			}

			return null;
		}

		public static string GetIdentityTokenFromRequest(HttpRequest request)
		{
			var queryStringData = request.QueryString[QueryStringKey];

			if (string.IsNullOrEmpty(queryStringData))
			{
				queryStringData = request.QueryString[SecureQueryString.QueryStringKey];
			}

			if (!string.IsNullOrEmpty(queryStringData))
			{
				SecureQueryString queryString = null;
				try
				{
					queryString = new SecureQueryString(WebUtility.UrlDecode(queryStringData));
				}
				catch (QueryStringException) { }

				if (queryString != null)
				{
					var token = queryString[IdentityTokenQueryStringKey];
					if (!string.IsNullOrWhiteSpace(token))
					{
						return token;
					}
				}
			}

			return string.Empty;
		}

		public static string GetUserDefinedQueryParameterValue(NameValueCollection requestQueryString, string key)
		{
			var queryStringData = requestQueryString[QueryStringKey];
			if (!string.IsNullOrEmpty(queryStringData))
			{
				SecureQueryString queryString = null;
				try
				{
					queryString = new SecureQueryString(WebUtility.UrlDecode(queryStringData));
				}
				catch (QueryStringException) { }

				if (queryString != null)
				{
					return queryString[$"{UserDefinedQueryParameterKeyPrefix}{key}"];
				}
			}

			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error message should not be translated, SecureQueryString uses DateTime.Now to determine validity of query string, URL Parameters, Uri string should not be translated")]
		public Uri GetRoutingUrl()
		{
			if (!IdentityManager.IsValidID())
			{
				var queryString = new SecureQueryString
				{
					["title"] = "Login Session Expired",
					["message"] = "Your login session was invalid or has expired. Please attempt to login again."
				};

				queryString.ExpireTime = TimeSpan.FromMinutes(10);
				return new Uri(GetNewGlobal().ErrorPage + "?data=" + WebUtility.UrlEncode(queryString.ToString()), UriKind.RelativeOrAbsolute);
			}

			foreach (var descriptor in RoutingDescriptors)
			{
				if (descriptor.IsRoutingRequired)
				{
					descriptor.RoutingAction();

					if (descriptor.RoutingUrl != null)
					{
						return BuildRoutingUrl(descriptor.RoutingUrl);
					}
				}
			}

			return BuildRoutingUrl(new Uri("~/Login/LoginComplete.aspx", UriKind.Relative));
		}

		protected abstract ZGlobal GetNewGlobal();

		Uri BuildRoutingUrl(Uri routingUrl)
		{
			var secureQueryString = new SecureQueryString
			{
				{ IdentityTokenQueryStringKey, IdentityManager.Token }
			};
			secureQueryString.ExpireTime = TimeSpan.FromMinutes(30);  // SecureQueryString uses DateTime.Now to determine validity of query string

			if (OriginalUrl != null)
			{
				secureQueryString.Add(OriginalUrlQueryStringKey, OriginalUrl.IsAbsoluteUri ? OriginalUrl.AbsoluteUri : OriginalUrl.OriginalString);
			}

			secureQueryString.Add(UserDefinedQueryParameters);

			var encodedQueryString = WebUtility.UrlEncode(secureQueryString.ToString());
			var uriDeconstructor = new UriDeconstructor(routingUrl);
			var query = HttpUtility.ParseQueryString(uriDeconstructor.Query);
			query.Add(QueryStringKey, encodedQueryString);

			return uriDeconstructor.GetUriWithNewQuery(query.ToString());
		}

		protected abstract Uri DefaultUrl { get; }

		protected virtual IEnumerable<ILoginRoutingDescriptor> RoutingDescriptors =>
			new List<ILoginRoutingDescriptor>
			{
				new SupersededLoginRoutingDescriptor(Contact),
				new PasswordRotationRoutingDescriptor(Contact),
			};

		public bool HasAnyRoutingRequired => RoutingDescriptors.Any(x => x.IsRoutingRequired);

		public void AddUserDefinedQueryParameter(string key, string value)
		{
			UserDefinedQueryParameters.Add($"{UserDefinedQueryParameterKeyPrefix}{key}", value);
		}

		NameValueCollection UserDefinedQueryParameters { get; } = new NameValueCollection();
	}
}
