using System;
using System.IO;
using CargoWise.Common;
using CargoWise.Loader.Common;

namespace CargoWise.Loader.Client
{
	public class InstallationProgramFromFile : InstallationProgramBase
	{
		public InstallationProgramFromFile(ClientInstallation installation)
			: base(installation)
		{
			Argument.NotNull(installation, nameof(installation));
			WaitForInputIdle = Environment.UserInteractive && installation.Configuration.UILevel != UILevel.AutomatedWithNoUI;
		}

		public override string Arguments
		{
			get
			{
				return argumentOverride ?? Installation.Configuration.ProgramArguments;
			}
			set
			{
				argumentOverride = value;
			}
		}
		string argumentOverride;

		public new ClientInstallation Installation
		{
			get
			{
				return (ClientInstallation)base.Installation;
			}
		}

		public override string FullPathOfProgramToRun
		{
			get
			{
				return Path.Combine(Installation.Configuration.BaseTargetPath, Installation.Configuration.ProgramFileName);
			}
		}
	}
}
