#if NETFRAMEWORK
using System;
using System.Web;
using Enterprise.ZArchitecture.Web.Shared;

namespace Enterprise.ZArchitecture.Web.Business
{
	public abstract class ZGlobalBase : EnterpriseHttpApplication
	{
		public virtual WebUser SiteUser => HttpContext.Current?.Session?[SessionIndexerForSiteUser] as WebUser;

		protected virtual void OnCustomSessionStart(Object sender, EventArgs e)
		{
			HttpContext.Current.Session.Add(SessionIndexerForSiteUser, GetNewSiteUser());
		}

		public virtual WebUser GetNewSiteUser()
		{
			return new OrgContactWebUser();
		}

		public const string SessionIndexerForSiteUser = "SiteUser";
	}
}
#endif

