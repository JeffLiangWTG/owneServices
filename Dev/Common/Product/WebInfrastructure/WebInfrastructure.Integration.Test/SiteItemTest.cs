using System.Linq;
using CargoWise.BuildTools;
using NUnit.Framework;

namespace CargoWiseOne.WebInfrastructure.Integration.Test
{
	class SiteItemTest : TestCase
	{
		public void TestSiteItemsList()
		{
			var glowFolders = new[] { "Glow", "GlowWebClient" };
			AssertContainsExactElementsInAnyOrder(
				BuildXml.Instance.GetWebDestFolders().Concat(glowFolders),
				InstallSiteItem.GetSitesToInstall().Select(s => s.FolderName).Distinct()
				);
		}
	}
}
