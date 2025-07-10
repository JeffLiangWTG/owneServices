using Enterprise.RemoteDesktopServices;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public sealed class ZTerminalServiceTest : TestCase
	{
		public void TestMessages()
		{
			AssertEquals(GetExpectedMessageTemplate("Some functions will not be", ClientVersion.ProductName, ClientVersion.InstallerExeName), ZTerminalService.RDApplicationNotInstalledWarning.GetUnresolvedString());
			AssertEquals(GetExpectedMessageTemplate("This function is not", ClientVersion.ProductName, ClientVersion.InstallerExeName), ZTerminalService.RDApplicationNotInstalledError.GetUnresolvedString());
			AssertEquals(GetExpectedMessageTemplate("Some functions will not be", ClientCitrixVersion.ProductName, ClientCitrixVersion.InstallerExeName), ZTerminalService.CitrixApplicationNotInstalledWarning.GetUnresolvedString());
			AssertEquals(GetExpectedMessageTemplate("This function is not", ClientCitrixVersion.ProductName, ClientCitrixVersion.InstallerExeName), ZTerminalService.CitrixApplicationNotInstalledError.GetUnresolvedString());
		}

		static string GetExpectedMessageTemplate(string head, string productName, string installerName)
		{
			return $@"{head} available because {productName} is not installed or was not found.
Please download and run the {productName} installer to use this function. If you have already installed this, please restart current application.

You can download the latest version of the {productName} installer via the following link:
https://myaccount-portal.cargowise.com/myaccount/downloads/{installerName}";
		}
	}
}
