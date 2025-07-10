using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;

[assembly: AssemblyTitle("CargoWise.exe Placeholder")]

// This exists for release build version number management purposes

namespace CargoWisePlaceholder
{
	static class Program
	{
		static EventHandler<bool> processExitedEventHandler;

#pragma warning disable CW1021
		static Process process;
#pragma warning restore CW1021

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWise", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Starting up the current application exe process not opening a file or url")]
		static int Main(string[] args)
		{
			var installationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var cw1Exe = Path.Combine(installationPath, ExeFileNames.CargoWiseWindowsDesktopExe);

			var psi = new ProcessStartInfo(cw1Exe, string.Join(" ", args.Select(arg => CommandLineArgEncoder.EnquoteArgumentIfNeeded(arg))))
			{
				UseShellExecute = false
			};

			using (process = new Process())
			{
				try
				{
					process.StartInfo = psi;
					process.EnableRaisingEvents = true;

					process.Exited += (sender, eventArgs) =>
					{
						processExitedEventHandler.Invoke(process, true);
					};

					process.Start();
				}
				catch (Exception)
				{
					return -1;
				}

				processExitedEventHandler = ProcessExitedCallback;

				// Wait for processes UI to be created and go idle. Note this will also occur when the process is closing.
				process.WaitForInputIdle();

				// Wait until Application.Exit is called in ProcessExitedCallback. Critically this will also fire an WaitForInputIdle event, emulating the behavior of the CargoWise Main exec.
				Application.Run();

				// Clean up shell process, if the temp form is closed first then the process will not have closed yet.
				if (!process.HasExited)
				{
					process.CloseMainWindow();
					process.WaitForExit();
				}

				return process.ExitCode;
			}
		}

		static void ProcessExitedCallback(object sender, bool success)
		{
			Application.Exit();
		}
	}
}
