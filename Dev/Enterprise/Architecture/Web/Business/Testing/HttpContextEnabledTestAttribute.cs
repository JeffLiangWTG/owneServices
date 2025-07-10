#if DEBUG
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
#if NETFRAMEWORK
using System.Web.Hosting;
using Enterprise.MasterFiles.Business;
#elif NET
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	public interface IHttpContextEnabledTestWithAppInstance
	{
#if NETFRAMEWORK
		ZEnterpriseGlobalBase AppInstance { get; }
#endif
	}

	public interface IHttpContextEnabledTestWithHttps
	{
	}

	public interface IHttpContextEnabledTestWithRequestPath
	{
		string RequestPath { get; }
		string QueryString { get; }
	}

	#region DummyWorkerRequest

#if NETFRAMEWORK
	public class SecureDummyWorkerRequest : DummyWorkerRequest
	{
		public SecureDummyWorkerRequest(string page, string query, StringWriter output)
			: base(page, query, output)
		{
		}

		public override bool IsSecure() => true;
	}

	public class DummyWorkerRequest : SimpleWorkerRequest
	{
		public DummyWorkerRequest(string page, string query, StringWriter output)
			: base(page, query, output)
		{
		}

		readonly Dictionary<int, string> knownHeaders = new Dictionary<int, string>();

		public void SetUserLanguagesSeparatedByComma(string value)
		{
			knownHeaders.Add(0x17, value);
		}

		public void ClearUserLanguages()
		{
			knownHeaders.Remove(0x17);
		}

		public override string GetKnownRequestHeader(int index)
		{
			return knownHeaders.ContainsKey(index) ? knownHeaders[index] : base.GetKnownRequestHeader(index);
		}

		public override bool IsClientConnected()
		{
			return IsClientConnectedOverride;
		}

		public bool IsClientConnectedOverride { get; set; } = true;

		public NameValueCollection Headers
		{
			get
			{
				if (fHeaders == null)
				{
					fHeaders = new NameValueCollection();
				}

				return fHeaders;
			}
		}
		NameValueCollection fHeaders;

		public override void SendKnownResponseHeader(int index, string value)
		{
			Headers.Add(GetKnownRequestHeaderName(index), value);

			base.SendKnownResponseHeader(index, value);
		}

		public override void SendUnknownResponseHeader(string name, string value)
		{
			Headers.Add(name, value);

			base.SendUnknownResponseHeader(name, value);
		}
	}

	#region DummyHttpApplication

	public class DummyHttpApplication : ZEnterpriseGlobalBase
	{
		public DummyHttpApplication(DummyWorkerRequest workerRequest)
		{
			fWorkerRequest = workerRequest;
		}

		public DummyWorkerRequest WorkerRequest
		{
			get
			{
				return fWorkerRequest;
			}
		}
		readonly DummyWorkerRequest fWorkerRequest;

		public override WebUser SiteUser
		{
			get
			{
				if (siteUser == null)
				{
					siteUser = new OrgContactWebUser();
					siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				}
				return siteUser;
			}
		}

		public override WebUser GetNewSiteUser() => new OrgContactWebUser();

		WebUser siteUser;

		public void SetSiteUser(WebUser user)
		{
			siteUser = user;
		}
	}

	#endregion
#elif NET
	public interface ISimpleWorkerRequest
	{
		bool IsSecure();
		void SetUserLanguagesSeparatedByComma(string value);
		void ClearUserLanguages();
		string GetKnownRequestHeader(int index);
		bool IsClientConnected();
		void SendKnownResponseHeader(int index, string value);
		void SendUnknownResponseHeader(string name, string value);
	}
	public class SecureDummyWorkerRequest : DummyWorkerRequest
	{
		public SecureDummyWorkerRequest(string page, string query, StringWriter output, HttpContext context)
			: base(page, query, output, context)
		{
		}

		public override bool IsSecure() => true;
	}

	public class DummyWorkerRequest : ISimpleWorkerRequest
	{
		readonly HttpContext context;
		public DummyWorkerRequest(string page, string query, StringWriter output, HttpContext context)
		{
			this.context = context;
			context.Request.Scheme = IsSecure() ? "https" : "http";
		}

		readonly Dictionary<int, string> knownHeaders = new Dictionary<int, string>();

		public virtual bool IsSecure() => false;

		public void SetUserLanguagesSeparatedByComma(string value)
		{
			context.Request.Headers["Accept-Language"] = value;
			knownHeaders.Add(0x17, value);
		}

		public void ClearUserLanguages()
		{
			knownHeaders.Remove(0x17);
		}

		public virtual string GetKnownRequestHeader(int index)
		{
			return knownHeaders.ContainsKey(index) ? knownHeaders[index] : StandardHeaderNames[index];
		}

		public virtual bool IsClientConnected()
		{
			return IsClientConnectedOverride;
		}

		public bool IsClientConnectedOverride { get; set; } = true;

		public NameValueCollection Headers
		{
			get
			{
				if (fHeaders == null)
				{
					fHeaders = new NameValueCollection();
				}

				return fHeaders;
			}
		}
		NameValueCollection fHeaders;

		public virtual void SendKnownResponseHeader(int index, string value)
		{
			Headers.Add(StandardHeaderNames[index], value);
			context.Response.Headers[StandardHeaderNames[index]] = value;
		}

		public virtual void SendUnknownResponseHeader(string name, string value)
		{
			Headers.Add(name, value);
			context.Response.Headers[name] = value;
		}

		readonly Dictionary<int, string> StandardHeaderNames = new()
		{
			[0] = "Cache-Control",
			[1] = "Connection",
			[2] = "Date",
			[3] = "Keep-Alive",
			[4] = "Pragma",
			[5] = "Trailer",
			[6] = "Transfer-Encoding",
			[7] = "Upgrade",
			[8] = "Via",
			[9] = "Warning",
			[10] = "Allow",
			[11] = "Content-Length",
			[12] = "Content-Type",
			[13] = "Content-Encoding",
			[14] = "Content-Language",
			[15] = "Content-Location",
			[16] = "Content-MD5",
			[17] = "Content-Range",
			[18] = "Expires",
			[19] = "Last-Modified",
			[20] = "Accept-Ranges",
			[21] = "Age",
			[22] = "ETag",
			[23] = "Accept-Language",
			[24] = "Proxy-Authenticate",
			[25] = "Retry-After",
			[26] = "Server",
			[27] = "Set-Cookie",
			[28] = "Vary",
			[29] = "WWW-Authenticate",
			[30] = "Cookie",
			[31] = "Content-Disposition"
		};
	}
#endif

	#endregion
}
#endif
