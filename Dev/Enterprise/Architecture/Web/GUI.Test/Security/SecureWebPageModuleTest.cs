using System.Web;
using Enterprise.ZArchitecture.Web.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	[HttpContextEnabledTest]
	sealed class SecureWebPageModuleTest : TestCase
	{
		public void TestInit()
		{
			AssertNull("Settings", HttpContext.Current.Application["SecureWebPageSettings"]);

			Module.Init(HttpContext.Current.ApplicationInstance);
			AssertNull("There is no way to pass Settings :-(", HttpContext.Current.Application["SecureWebPageSettings"]);
		}

		#region Implementation

		SecureWebPageModule Module
		{
			get { return module ?? (module = new SecureWebPageModule()); }
		}
		SecureWebPageModule module;

		#endregion

	}
}
