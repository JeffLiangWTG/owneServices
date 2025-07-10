using System;
using System.Web.Http;
using Enterprise.Web.TestWebApplication.Common;
using Enterprise.ZArchitecture.Web.GUI;

namespace TestApplicationBasedOnZGlobal
{
	public class Global : ZGlobal
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			GlobalConfiguration.Configure(WebApiConfig.Register);
		}

		public override string DefaultPage => ApplicationRoot + "Default.aspx";
		public override string LogoImage => ApplicationRoot + "Images/Logo.png";
	}
}
