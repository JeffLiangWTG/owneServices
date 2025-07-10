using System;
using System.IO;
using CargoWise.Loader.Client;

namespace Enterprise.Upgrades.Preinstall
{
	class EnterpriseUpgradePreinstallConfiguration : ClientConfiguration
	{
		protected override void InitializeCore(string[] args)
		{
			if (args == null || args.Length < 1)
			{
				throw new ArgumentException("Enterprise.Upgrades.Preinstall command line args must contain at least the package directory.", nameof(args));
			}

			this.tempDirectory = args[0];
			base.InitializeCore(args);
		}

		string tempDirectory;

		public override string CurrentPackage
		{
			get { return tempDirectory; }
		}

		public override string GetApplicationPathInCurrentPackage()
		{
			return Path.Combine(CurrentPackage, @"Distribution\Application");
		}

		public override string GetInstallPathInCurrentPackage()
		{
			return Path.Combine(CurrentPackage, @"Distribution\Install");
		}

		public override string ProgramFileName
		{
			get { throw new NotImplementedException(); }
		}
	}
}
