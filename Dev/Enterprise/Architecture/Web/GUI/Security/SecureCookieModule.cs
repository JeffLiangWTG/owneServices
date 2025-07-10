using System;
using System.Web;

namespace Enterprise.ZArchitecture.Web.Security
{
	public class SecureCookieModule : IHttpModule
	{
		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			if (application != null)
			{
				application.EndRequest += Application_EndRequest;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "standardized constant")]
		const string Https = "https";
		const string XForwardedProto = "X-Forwarded-Proto";

		protected void Application_EndRequest(object sender, EventArgs e)
		{
			var application = sender as HttpApplication;
			var request = application?.Request;
			if (request?.IsSecureConnection == true || request?.Headers.Get(XForwardedProto) == Https)
			{
				for (var i = 0; i < application.Response.Cookies.Count; i++)
				{
					application.Response.Cookies[i].Secure = true;
				}
			}
		}
	}
}
