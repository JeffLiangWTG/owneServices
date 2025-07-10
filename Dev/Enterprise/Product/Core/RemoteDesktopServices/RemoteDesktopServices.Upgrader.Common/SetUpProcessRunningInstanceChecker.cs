using System;
using System.Diagnostics;
using System.Linq;
using CargoWise.ApplicationManager.Common;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace CargoWise.RemoteDesktopServices.Upgrader.Common
{
	public class SetUpProcessRunningInstanceChecker : AppManagerInvoker, IAppManagerInvocable
	{
		internal readonly string processName;

		SetUpProcessRunningInstanceChecker() : this(null, null)
		{ }

		public SetUpProcessRunningInstanceChecker(Installation installation, string processName) : base(installation)
		{
			this.processName = processName;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				var result = InvokeAppManager<SetUpProcessRunningInstanceChecker>(this.processName, null);
				switch (result.Status)
				{
					case InstallationResultStatus.OK:
						return InstallationResult.OK();
					default:
						return InstallationResult.Error(result.Message);
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Error(ex.ToString());
			}
		}

		protected override bool NeedsToInstallCore()
		{
			return AssemblyResolver.IsApplicationManagerInstalled();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Baseline")]
		AppManagerResult IAppManagerInvocable.Invoke(bool waitedForMutex, object state)
		{
			var processesName = (string)state;

			if (!Process.GetProcessesByName(processesName).Any())
			{
				return new AppManagerResult(AppManagerResultStatus.Success);
			}
			else
			{
				return new AppManagerResult(AppManagerResultStatus.Error, $"Installation failed because another instance of <{processesName}> is still running.");
			}
		}
	}
}
