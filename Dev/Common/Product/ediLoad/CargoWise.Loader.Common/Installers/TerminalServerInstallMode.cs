using System;
using System.IO;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public sealed class TerminalServerInstallMode : InstallationItem
	{
		public TerminalServerInstallMode(Installation installation) : base(installation)
		{
			Argument.NotNull(installation, nameof(installation));
			InitialiseWindows();
			InitialiseCitrix();
			if (installation.Configuration.Services.File.Exists(WindowsChange.FullPathOfProgramToRun))
			{
				AddDependency(WindowsChange);
			}
			else if (installation.Configuration.Services.File.Exists(CitrixChange.FullPathOfProgramToRun))
			{
				AddDependency(CitrixChange);
			}
		}

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		void InitialiseWindows()
		{
			WindowsChange = new InstallationProgram(Installation);
			WindowsChange.SetFullPathOfProgramToRun(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "CHANGE.EXE"));
			WindowsChange.Arguments = "USER /INSTALL";
			WindowsChange.WaitForExit = true;
			WindowsChange.CreateNoWindow = true;
		}

		void InitialiseCitrix()
		{
			CitrixChange = new InstallationProgram(Installation);
			CitrixChange.SetFullPathOfProgramToRun(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "CHGUSR.EXE"));
			CitrixChange.Arguments = "/INSTALL";
			CitrixChange.WaitForExit = true;
			CitrixChange.CreateNoWindow = true;
		}

		InstallationProgram WindowsChange;
		InstallationProgram CitrixChange;
	}
}

