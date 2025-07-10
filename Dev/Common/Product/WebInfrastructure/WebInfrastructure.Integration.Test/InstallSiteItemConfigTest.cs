using System.Linq;
using NUnit.Framework;

namespace CargoWiseOne.WebInfrastructure.Integration.Test
{
	class InstallSiteItemConfigTest : TestCase
	{
		public void TestLoadConfig()
		{
			var configs = Strong.Test.InstallSites.LoadSitesFromConfig();
			AssertNotNull(configs);
			Assert(configs.InstallSiteItems.Any());
		}
	}
}
