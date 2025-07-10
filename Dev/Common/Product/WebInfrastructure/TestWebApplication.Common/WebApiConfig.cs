using System.Web.Http;
using System.Web.Http.ExceptionHandling;

namespace Enterprise.Web.TestWebApplication.Common
{
	public static class WebApiConfig
	{
		public static void Register(HttpConfiguration config)
		{
			config.Services.Replace(typeof(IExceptionHandler), new WebExceptionHandler());
		}
	}
}
