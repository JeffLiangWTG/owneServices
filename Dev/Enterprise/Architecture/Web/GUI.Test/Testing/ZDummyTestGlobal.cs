using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	class ZDummyTestGlobal : ZTestGlobal
	{
		public override WebUser SiteUser
		{
			get { return SiteUserOverride; }
		}

		public WebUser SiteUserOverride;
	}
}
