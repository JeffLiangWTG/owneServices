#if NETFRAMEWORK
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
#endif
using Enterprise.MasterFiles.Business;

namespace Enterprise.ZArchitecture.Web.Business
{
#if NETFRAMEWORK
	public static class WebEnv
	{
		public static ZEnterpriseGlobalBase AppInstance => HttpContext.Current?.ApplicationInstance as ZEnterpriseGlobalBase;

		public static IContactable CurrentUser => AppInstance?.SiteUser?.LoggedInUser;
	}
#elif NET
	public static class WebEnv
	{
		public static IHttpContextAccessor HttpContextAccessor { get; set; }

		public static WebUser SiteUser => HttpContextAccessor?.HttpContext?.Session?.GetObject<WebUser>("SiteUser");

		public static IContactable CurrentUser => SiteUser?.LoggedInUser;
	}
#endif
}
