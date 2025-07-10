using System.IO;
using System.Reflection;
using CargoWise.Loader.Common;
using CargoWise.Loader.Common.Installers;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class UpgradeInstallerAsync : AssemblyResourceInstaller
	{
		readonly string pluginProductName;
		readonly string installerName;

		protected UpgradeInstallerAsync()
			: base(null)
		{ }

		public UpgradeInstallerAsync(Installation installation, string pluginProductName, string installerName)
			: base(installation)
		{
			this.pluginProductName = pluginProductName;
			this.installerName = installerName;
		}

		protected override string NameOfComponentBeingInstalled
		{
			get { return pluginProductName; }
		}

		protected override string InstallerName
		{
			get { return installerName; }
		}

		protected override Stream ResourceStream()
		{
			// In general, it's unable to access the resource within current assembly because the resource is actually embedded in the executable file (.exe) instead.
			// At runtime, as this assembly will be merged into the executable file, GetType().Assembly returns the executable file and then the resource will be available eventually.
			// But at test time, this assembly is not contained in the test assembly, and GetType().Assembly returns this real assembly, so the resource is not available.
			return ResourceStream(GetType().Assembly);
		}

		Stream ResourceStream(Assembly assemblyContainingResource)
		{
			return assemblyContainingResource.GetManifestResourceStream(InstallerName);
		}

		protected override bool WaitForInstallerToComplete => false;

#if DEBUG
		public (string Name, Stream Stream) GetInstallerResourceForTest(Assembly assemblyContainingResource)
		{
			return (InstallerName, ResourceStream(assemblyContainingResource));
		}
#endif
	}
}
