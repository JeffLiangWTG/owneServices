using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace Enterprise.Loader
{
	class CurrentVersionFileRemover : InstallationItem
	{
		public CurrentVersionFileRemover(Installation installation)
			: base(installation)
		{
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		protected override InstallationResult InstallExcludingDependencies()
		{
			try
			{
				File.Delete(Path.Combine(Installation.Configuration.BaseTargetPath, "CurrentVersion"));
				return InstallationResult.OK();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return InstallationResult.Error(ex.Message);
			}
		}
	}
}
