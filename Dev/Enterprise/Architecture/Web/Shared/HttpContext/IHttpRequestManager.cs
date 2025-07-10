#if NETFRAMEWORK
using System.Web;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace Enterprise.ZArchitecture.Web.Shared
{
	public interface IHttpRequestManager
	{
#if NETFRAMEWORK
		HttpContextBase GetHttpContextBase();
#else
		HttpContext GetHttpContext();
#endif
	}
}
