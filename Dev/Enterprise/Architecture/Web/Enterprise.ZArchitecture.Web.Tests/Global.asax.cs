using System;
using System.Web.Http;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.Utilities.Environment;

namespace Enterprise.ZArchitecture.Web.Tests
{
	public class Global : ZGlobal
	{
		public override string DefaultPage => "ServerPath.aspx";

		protected override void Application_Start(object sender, EventArgs e)
		{
			base.Application_Start(sender, e);

			GlobalConfiguration.Configure(x => x.MapHttpAttributeRoutes());
			WebInitialiser.Initialise(enableErrorReport: false, new WebEnvironmentProvider());
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = Enterprise.Core.SharedConstants.Languages.English;
		}
	}
}
