using System.Net.NetworkInformation;
using static System.FormattableString;

namespace CargoWiseOne.WebInfrastructure.Integration.Strong.Test.Helpers
{
	static class ExtensionsHelper
	{
		public static void Install(this InstallSiteItem siteItem, string siteName)
		{
			siteItem.Install = true;
			siteItem.WebAddress = $"{siteName.GetHostName()}/{siteItem.DefaultApplicationPath}";
		}

		public static string GetHostName(this string siteName)
		{
			return Invariant($"{(string.IsNullOrEmpty(siteName) ? "localhost" : siteName)}.{IPGlobalProperties.GetIPGlobalProperties().DomainName}");
		}
	}
}
