using System;
using System.Web;

namespace Enterprise.ZArchitecture.Web.Security
{
	public class ClickJackingProtectionModule : IHttpModule
	{
		public void Dispose()
		{
		}

		public void Init(HttpApplication application)
		{
			application.PostRequestHandlerExecute += Application_PostRequestHandlerExecute;
		}

		void Application_PostRequestHandlerExecute(object sender, EventArgs e)
		{
			var application = (HttpApplication)sender;
			OnPostRequestHandlerExecute(new HttpContextWrapper(application.Context));
		}

		public void OnPostRequestHandlerExecute(HttpContextBase context)
		{
			if (!IsClickJackingProtectionDisabed(context) && context.Response.BufferOutput)
			{
				AddXFrameOptionHeader(context);
			}
		}

		bool IsClickJackingProtectionDisabed(HttpContextBase context) => (bool?)context.Session?[SessionKeyClickJackingProtectionDisabled] ?? false;

		void AddXFrameOptionHeader(HttpContextBase context)
		{
			var header = context.Response.Headers[CustomizedXFrameOptionHeader.HeaderName];

			if (header != null)
			{
				context.Response.Headers.Remove(CustomizedXFrameOptionHeader.HeaderName);
				if (header != CustomizedXFrameOptionHeader.HeaderValue.None)
				{
					context.Response.AddHeader("x-frame-options", header);
				}
			}
			else
			{
				context.Response.AddHeader("x-frame-options", CustomizedXFrameOptionHeader.HeaderValue.SameOrigin);
			}
		}

		public static class CustomizedXFrameOptionHeader
		{
			public static readonly string HeaderName = "Customize-XFrameOption";
			public static class HeaderValue
			{
				public const string None = "NONE";
				public const string Deny = "DENY";
				public const string SameOrigin = "SAMEORIGIN";
				public const string AllowFrom = "ALLOWFROM";
			}
		}

		public const string SessionKeyClickJackingProtectionDisabled = "ClickJackingDisabled";
	}
}
