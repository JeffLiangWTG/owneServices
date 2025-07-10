using System;
using System.Web.Http;
using CargoWiseOne.WebInfrastructure;
using Enterprise.Web.TestWebApplication.Common;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Shared;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace TestApplicationBasedOnZGlobalBase
{
	/// <summary>
	/// Inherits from <see cref="ZGlobalBase"/> to:
	/// <list type="ol">
	/// <item>
	/// A new WebUser is created a added onto HttpContext.Current.Session;
	/// </item>
	/// <item>
	/// Inherits from root base <see cref="EnterpriseHttpApplication"/> so that a new <see cref="WebUpgradeManager"/>
	/// is created and started on Application_Start, disposed on Application_End.
	/// </item>
	/// </list>
	/// </summary>
	public class Global : ZGlobalBase
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			GlobalConfiguration.Configure(WebApiConfig.Register);
			WebInitialiser.Initialise(enableErrorReport: true, new WebEnvProvider());
		}
	}
}
