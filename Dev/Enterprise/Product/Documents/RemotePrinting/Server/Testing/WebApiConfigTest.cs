using System.Web.Http;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	sealed class WebApiConfigTest : TestCase
	{
		public void TestRegister()
		{
			var config = new HttpConfiguration();
			WebApiConfig.Register(config);

			AssertNotNull(config.Routes["MS_attributerouteWebApi"]);

			var defaultApiRoute = GlobalConfiguration.Configuration.Routes["DefaultApi"];
			AssertEquals("api/{controller}", defaultApiRoute.RouteTemplate);
		}

		public void TestRegisterWhenApiRouteIsWtg()
		{
			var config = new HttpConfiguration();
			WebApiConfig.Register(config);

			AssertNotNull(config.Routes["MS_attributerouteWebApi"]);

			var defaultApiRoute = GlobalConfiguration.Configuration.Routes["WtgApi"];
			AssertEquals("wtg/{controller}", defaultApiRoute.RouteTemplate);
		}

		protected override void TearDown()
		{
			GlobalConfiguration.Configuration.Routes.Clear();

			base.TearDown();
		}
	}
}
