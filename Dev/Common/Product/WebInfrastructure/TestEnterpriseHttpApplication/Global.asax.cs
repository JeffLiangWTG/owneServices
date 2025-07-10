using System;
using System.Web.Http;
using Enterprise.Web.TestWebApplication.Common;
using Enterprise.ZArchitecture.Web.Shared;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace TestEnterpriseHttpApplication
{
	public class Global : EnterpriseHttpApplication
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			GlobalConfiguration.Configure(WebApiConfig.Register);
			WebInitialiser.Initialise(enableErrorReport: true, new WebEnvProvider());
		}
	}
}
