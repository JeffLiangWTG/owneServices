using System;
using System.Threading;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class RunningInstanceChecker : InstallationItem
	{
		readonly Mutex mutexObtainedByInstallation;
		readonly string upgraderName;

		public RunningInstanceChecker(Installation installation, Mutex mutexObtainedByInstallation, string upgraderName) : base(installation)
		{
			this.mutexObtainedByInstallation = mutexObtainedByInstallation;
			this.upgraderName = upgraderName;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				if (!mutexObtainedByInstallation.WaitOne(0))
				{
					return InstallationResult.Error($"Installation failed because another instance of <{upgraderName}> is still running.");
				}
				return InstallationResult.OK();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Error(ex.ToString());
			}
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}
	}
}
