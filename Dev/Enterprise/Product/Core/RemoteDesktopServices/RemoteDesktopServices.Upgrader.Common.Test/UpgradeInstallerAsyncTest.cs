using CargoWise.Loader.Common.Installers;
using NUnit.Framework;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common.Test
{
	sealed class UpgradeInstallerAsyncTest : TestCase
	{
		public void TestUpgradeInstallerAsyncShouldInheritFromAssemblyResourceInstaller()
		{
			Assert($"UpgradeInstallerAsync should be typeof {nameof(AssemblyResourceInstaller)}", typeof(AssemblyResourceInstaller).IsAssignableFrom(typeof(UpgradeInstallerAsync)));
		}
	}
}
