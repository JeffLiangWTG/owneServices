using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace CargoWise.Loader.Common
{
	public abstract class InstallationProgramBase : InstallationItem
	{
		protected InstallationProgramBase(Installation installation) : base(installation)
		{
		}

		public abstract string FullPathOfProgramToRun
		{
			get;
		}

		public virtual string Arguments
		{
			get;
			set;
		}

		protected Process Process;
		public bool WaitForInputIdle;
		public bool WaitForExit;
		public bool WaitForExitInBackground;
		public bool CreateNoWindow;
		protected Thread WaitThread;

		protected override bool NeedsToInstallCore()
		{
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		protected override InstallationResult InstallExcludingDependencies()
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(FullPathOfProgramToRun, Arguments);
			startInfo.CreateNoWindow = CreateNoWindow;
			startInfo.WorkingDirectory = Path.GetDirectoryName(FullPathOfProgramToRun);
			try
			{
				Process = Process.Start(startInfo);
				if (Process == null)
				{
					return InstallationResult.Error(Installation.Configuration.ApplicationName + " failed to launch a program. The program was: " + FullPathOfProgramToRun);
				}
			}
			catch (Win32Exception ex)
			{
				if (ex.NativeErrorCode == ERROR_CANCELLED)
				{
					return InstallationResult.Error((Installation == null ? "(null)" : Installation.Configuration.ApplicationName) + " needed to launch a program, but it was cancelled. If Windows shows you a security warning for " + FullPathOfProgramToRun + ", you should allow the program to run. Also, check for any anti-spyware software which may be blocking this program from running.");
				}
				else
				{
					return InstallationResult.Error((Installation == null ? "(null)" : Installation.Configuration.ApplicationName) + " failed to launch a program: " + ex.Message + ". The program was: " + FullPathOfProgramToRun);
				}
			}
			if (WaitForInputIdle)
			{
				ChangeCurrentTaskDescription("Launching ...");
				Process.WaitForInputIdle();
			}

			int exitCode = 0;
			if (WaitForExit)
			{
				exitCode = DoWaitForExit();
			}
			else if (WaitForExitInBackground)
			{
				(WaitThread = new Thread(new ThreadStart(() => DoWaitForExit()))).Start();
			}
			return ResultForExitCode(exitCode);
		}

		int DoWaitForExit()
		{
			ChangeCurrentTaskDescription("Waiting for " + Path.GetFileNameWithoutExtension(FullPathOfProgramToRun));
			Process.WaitForExit();
			return Process.ExitCode;
		}

		// May be subclassed for programs that provide known exit codes
		protected virtual InstallationResult ResultForExitCode(int exitCode)
		{
			return InstallationResult.OK();
		}

		const int ERROR_CANCELLED = 1223;
	}
}

