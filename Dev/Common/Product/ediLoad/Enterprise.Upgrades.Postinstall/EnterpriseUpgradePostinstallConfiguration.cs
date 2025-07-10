using System;
using System.IO;
using CargoWise.Loader.Common;

namespace Enterprise.Upgrades.Postinstall
{
	class EnterpriseUpgradePostinstallConfiguration : Configuration
	{
		protected override void InitializeCore(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				throw new ArgumentException("Enterprise.Upgrades.Postinstall command line args must contain at least the installation path.", nameof(args));
			}

			string installationPath = args[0];
			BaseTargetPath = Path.GetDirectoryName(installationPath);
			TargetVersion = new Version(Path.GetFileName(installationPath));

			base.InitializeCore(args);
		}
	}
}
