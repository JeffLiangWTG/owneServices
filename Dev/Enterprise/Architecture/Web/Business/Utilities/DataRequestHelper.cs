using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
#if NETFRAMEWORK
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
#endif
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Business.Utilities
{
	public abstract class DataRequestHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public const string DataKey = "Data";

		public string GetHandlerUrl(ZGuid pK)
		{
			Dictionary<string, ZGuid> keys = new Dictionary<string, ZGuid>();
			keys.Add(DataKey, pK);

			return GetHandlerUrl(keys);
		}

		public string GetHandlerUrl(ZGuid[] pKs)
		{
			Dictionary<string, ZGuid[]> keys = new Dictionary<string, ZGuid[]>();
			keys.Add(DataKey, pKs);

			return GetHandlerUrl(keys);
		}

		protected internal string GetHandlerUrl(Dictionary<string, ZGuid> keys1)
		{
			Dictionary<string, ZGuid[]> keys = new Dictionary<string, ZGuid[]>();

			foreach (string key in keys1.Keys)
			{
				keys.Add(key, new ZGuid[] { keys1[key] });
			}

			return GetHandlerUrl(keys);
		}

		protected internal string GetHandlerUrl(Dictionary<string, ZGuid[]> queryStringValues)
		{
			return GetHandlerUrl(ConvertToStringNameValuePairs(queryStringValues));
		}

		/// <summary>
		/// To inject additional parameters into the URL that are not Guids
		/// please overwrite this method and do not call base.
		/// </summary>
		/// <param name="queryStringValues">
		/// The query string values containing any Guids already added
		/// </param>
		/// <returns>
		/// The query string including the previous Guids added, with NONE
		/// removed, and with new values added for non-guids.
		/// </returns>
		protected virtual IEnumerable<KeyValuePair<string, string>> InsertAdditionalParameters(IEnumerable<KeyValuePair<string, string>> queryStringValues)
		{
			return queryStringValues;
		}

		protected string GetHandlerUrl(IEnumerable<KeyValuePair<string, string>> queryStringValues)
		{
			queryStringValues = InsertAdditionalParameters(queryStringValues);

			if (UseSecureQueryString)
			{
				SecureQueryString queryString = new SecureQueryString();
				foreach (KeyValuePair<string, string> queryStringValue in queryStringValues)
				{
					queryString.Add(queryStringValue.Key, queryStringValue.Value);
				}
				return String.Format("{0}?{1}={2}", BaseUrl, SecureQueryString.QueryStringKey, WebUtility.UrlEncode(queryString.ToString()));
			}
			else
			{
				StringBuilder queryString = new StringBuilder(BaseUrl + "?");
				foreach (KeyValuePair<string, string> queryStringValue in queryStringValues)
				{
					queryString.AppendFormat("{0}={1}", queryStringValue.Key, queryStringValue.Value);
					queryString.Append("&");
				}
				return queryString.ToString().TrimEnd('&');
			}
		}

		static IEnumerable<KeyValuePair<string, string>> ConvertToStringNameValuePairs(Dictionary<string, ZGuid[]> queryStringValues)
		{
			foreach (KeyValuePair<string, ZGuid[]> queryStringValue in queryStringValues)
			{
				yield return new KeyValuePair<string, string>(queryStringValue.Key, GetGuidsAsString(queryStringValue.Value));
			}
		}

		protected internal static string GetGuidsAsString(params ZGuid[] guids)
		{
			StringBuilder result = new StringBuilder();
			foreach (ZGuid guid in guids)
			{
				result.Append(guid.ToString() + ",");
			}
			return result.ToString().TrimEnd(',');
		}

#if NETFRAMEWORK
		public virtual string GetCacheKey(params ZGuid[] pKs)
		{
			return GetDefaultCacheKey(pKs);
		}

		static string GetDefaultCacheKey(params ZGuid[] pKs)
		{
			string sessionKey = HttpContext.Current.Session.SessionID + GetGuidsAsString(pKs);
			return sessionKey;
		}
#elif NET
		public virtual string GetCacheKey(HttpContext httpContext, params ZGuid[] pKs)
		{
			return GetDefaultCacheKey(httpContext, pKs);
		}

		static string GetDefaultCacheKey(HttpContext httpContext, params ZGuid[] pKs)
		{
			string sessionKey = httpContext?.Session.Id + GetGuidsAsString(pKs);
			return sessionKey;
		}
#endif

		#region Abstract Members

		public abstract string BaseUrl
		{
			get;
		}

		public abstract bool EnableCache
		{
			get;
		}

		public abstract bool UseSecureQueryString
		{
			get;
		}

		#endregion
	}
}
