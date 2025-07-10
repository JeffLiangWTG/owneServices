using System;
using System.IO;
using System.Net;
using CargoWise.Common;

namespace Enterprise.MailManager.FileDownload
{
	public abstract class WebProtocolSupport
	{
		public WebProtocolSupport(string url)
		{
			fUrl = url;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used as key")]
		public const string UserQueryStringKey = "user";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "used as key")]
		public const string PasswordQueryStringKey = "pw";

		public virtual WebRequest GetWebRequestForDownload()
		{
			Uri uri = new Uri(Url);
			QueryString qs = null;
			if (uri.Query.Length > 0)
			{
				qs = new QueryString(uri.Query.Substring(1));
				string secureValue = qs[SecureQueryString.QueryStringKey];
				if (!string.IsNullOrEmpty(secureValue))
				{
					qs = new SecureQueryString(secureValue);
				}

				// chop off the query string
				uri = new Uri(uri.GetLeftPart(UriPartial.Path));
			}

#pragma warning disable SYSLIB0014 // WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			WebRequest request = WebRequest.Create(uri);
#pragma warning restore SYSLIB0014
			request.Proxy = Proxy;
			if (qs != null)
			{
				string user = qs[UserQueryStringKey];
				string password = qs[PasswordQueryStringKey];
				if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
				{
					request.Credentials = new NetworkCredential(user, password);
				}
			}
			return request;
		}

		public abstract WebRequest GetWebRequestForDownload(int startPosition);

		public virtual WebRequest GetWebRequestForFileInfo()
		{
			WebRequest request = GetWebRequestForDownload();
			request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.BypassCache);

			return request;
		}

		public abstract bool ResponseHasValidStatus(WebResponse response, RequestMode mode);

		public string GetFileName(WebResponse response)
		{
			return GetFileNameFromUri(response.ResponseUri);
		}

		public long GetFileSize(WebResponse response)
		{
			return response.ContentLength;
		}

		public abstract DateTime GetFileLastModified(WebResponse response);

		public string GetFileNameFromUri(Uri uri)
		{
			return Path.GetFileName(uri.LocalPath);
		}

		public string Url
		{
			get { return fUrl; }
		}

		public virtual bool IsResumeSupported
		{
			get { return false; }
		}

		protected IWebProxy Proxy
		{
			get { return proxy ?? WebRequest.DefaultWebProxy; }
			set { proxy = value; }
		}
		IWebProxy proxy;

		readonly string fUrl;

		public enum RequestMode
		{
			StartDownload,
			ResumeDownload,
			GetFileInfo
		}
	}
}
