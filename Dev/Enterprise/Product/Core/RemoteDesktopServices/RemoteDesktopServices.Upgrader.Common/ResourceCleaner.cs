using System;
using CargoWise.Loader.Common;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class ResourceCleaner : InstallationItem
	{
		readonly Action cleanUpAction;

		public ResourceCleaner(Installation installation, Action cleanUpAction) : base(installation)
		{
			this.cleanUpAction = cleanUpAction;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			cleanUpAction.Invoke();
			return InstallationResult.OK();
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}
	}
}
