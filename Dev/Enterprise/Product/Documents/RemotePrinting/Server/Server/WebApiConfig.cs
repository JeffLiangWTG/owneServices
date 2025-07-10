using System.Web.Http;

namespace Enterprise.RemotePrinting.Server
{
	public static class WebApiConfig
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static void Register(HttpConfiguration config)
		{
			config.MapHttpAttributeRoutes();
			GlobalConfiguration.Configure(x => x.Routes.MapHttpRoute(
				name: "DefaultApi",
				routeTemplate: "api/{controller}"
			));

			GlobalConfiguration.Configure(x => x.Routes.MapHttpRoute(
				name: "WtgApi",
				routeTemplate: "wtg/{controller}"
			));
		}
	}
}
