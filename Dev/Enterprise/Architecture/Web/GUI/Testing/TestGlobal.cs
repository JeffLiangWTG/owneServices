#if DEBUG
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZTestGlobal : ZGlobal
	{
		public override string DefaultPage => "/DefaultPage.aspx";

		public override string LoginPage => "/Login.aspx";

		public override string ApplicationRoot => "/";

		public override WebUser SiteUser => siteUser ?? (siteUser = new OrgContactWebUser());

		public void SetSiteUser(WebUser user) => siteUser = user;

		WebUser siteUser;

		public override string HomePage => @"http://www.test.cargowise.com/";

		protected override ZGlobalConfig GetNewGlobalConfig() => new ZTestGlobalConfig();

		public override string CompanyName => "TestCompany";
	}
}
#endif
