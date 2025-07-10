#if NETFRAMEWORK
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace Enterprise.ZArchitecture.Web.Business
{
	[SuppressMessage("CargoWiseOne", "CW1018:HttpApplicationRule", Justification = "ZEnterpriseGlobalBase is here to replace EnterpriseHttpApplication")]
	public abstract class ZEnterpriseGlobalBase : HttpApplication
	{
		public const string SiteUserSessionKey = "SiteUser";

		public virtual WebUser SiteUser => HttpContext.Current?.Session?[SiteUserSessionKey] as WebUser;

		public virtual WebUser GetNewSiteUser() => null;
	}
}
#endif
