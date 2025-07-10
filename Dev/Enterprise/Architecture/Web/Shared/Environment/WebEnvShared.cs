using System.Globalization;
using System.Threading;
#if NETFRAMEWORK
using System.Web;
#else
using System.Linq;
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared
{
	public static class WebEnvShared
	{
#if NET
		public static HttpContext HttpContext { get; set; }
#endif

		public static CultureInfo ClientCulture
		{
			get
			{
				try
				{
#if NETFRAMEWORK
					return HttpContext.Current != null &&
						HttpContext.Current.Request != null &&
						HttpContext.Current.Request.UserLanguages != null &&
						HttpContext.Current.Request.UserLanguages.Length > 0
						? CultureInfo.CreateSpecificCulture(HttpContext.Current.Request.UserLanguages[0])
						: Thread.CurrentThread.CurrentCulture;
#else
					var userLanguages = HttpContext.Request
						.GetTypedHeaders()?.AcceptLanguage
						.FirstOrDefault()?.Value.ToString();

					return string.IsNullOrEmpty(userLanguages)
						? Thread.CurrentThread.CurrentCulture
						: CultureInfo.CreateSpecificCulture(userLanguages);
#endif
				}
				catch (CultureNotFoundException)
				{
					return Thread.CurrentThread.CurrentCulture;
				}
			}
		}
	}
}
