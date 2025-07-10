using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.NGenInstallerProgram.Exceptions;
using Enterprise.Upgrades;
using Microsoft.Extensions.Logging;

namespace CargoWise.NGenInstallerProgram
{
	class NGenCaller
	{
		public NGenCaller(ILogger logger, string nGenExecutablePath = null, TimeSpan? timeout = null)
		{
			this.logger = logger;
			NGenExecutablePath = nGenExecutablePath ?? NGenPathHelper.GetNGenExecutablePath(use32Bit: false);
			ExecutionTimeout = timeout ?? TimeSpan.FromMinutes(25);

			logger.LogInformation("Using NGen.exe located: {path}", NGenExecutablePath);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "No Justification - Needs to be reviewed")]
		void Call(string arguments, Func<string, int, string, bool> isSucceeded)
		{
			logger.LogInformation("Executing command [{arguments}]...", arguments);

			var stringBuilder = new StringBuilder();
			void ReadOutputData(object sender, DataReceivedEventArgs e)
			{
				if (e?.Data != null)
				{
					stringBuilder.AppendLine(e.Data);
				}
			}

			var startInfo = new ProcessStartInfo(NGenExecutablePath)
			{
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = false,
				Arguments = arguments,
			};

			using (var process = new Process())
			{
				try
				{
					process.StartInfo = startInfo;
					process.OutputDataReceived += ReadOutputData;

					if (!process.Start())
					{
						throw new NGenCallException(-1, arguments, $"Failed to start the process: {startInfo.FileName}.");
					}

					process.BeginOutputReadLine();

					if (!process.WaitForExit((int)ExecutionTimeout.TotalMilliseconds))
					{
						process.Kill();
						throw new NGenTimeoutException(ExecutionTimeout);
					}

					var output = stringBuilder.ToString();
					if (!isSucceeded(arguments, process.ExitCode, output))
					{
						throw new NGenCallException(process.ExitCode, arguments, output);
					}

					logger.LogInformation("Command executed.");
				}
				finally
				{
					process.OutputDataReceived -= ReadOutputData;
				}
			}
		}

		public void QueuePause()
		{
			Call("queue pause", (argument, exitCode, output) => exitCode == 0);
		}

		public void QueueContinue()
		{
			Call("queue continue", (argument, exitCode, output) => exitCode == 0);
		}

		public void InstallOrUninstall(NGenAction action, string path)
		{
			var isSucceeded = new Func<string, int, string, bool>((arguments, exitCode, output) =>
			{
				return exitCode == 0 || (action == NGenAction.Uninstall && output.Contains("The specified assembly is not installed"));
			});

#if DEBUG
			//Always raise the error in debug builds, as it is required for OldVersionsRemoverTest.TestDeleteOrphanedVersionManyTimes
			isSucceeded = (arguments, exitCode, output) => exitCode == 0;
#endif

			Call(GetArguments(), isSucceeded);

			string GetArguments()
			{
				return action == NGenAction.Uninstall
					? $"Uninstall \"{path}\""
					: $"Install \"{path}\"{GetExeConfigOption()}";

				string GetExeConfigOption()
				{
					var mainExeFilePath = FindMainExeFilePath();
					return string.IsNullOrEmpty(mainExeFilePath)
						? string.Empty
						: $" /ExeConfig:\"{mainExeFilePath}\"";
				}

				string FindMainExeFilePath()
				{
					var rootDirectory = Path.GetDirectoryName(path);
					var exeFilePath = new[]
						{
							ExeFileNames.CargoWiseWindowsDesktopExe,
							ExeFileNames.CargoWiseOneExeForVersionInfo,
						}
						.Select(fileName => Path.Combine(rootDirectory, fileName))
						.FirstOrDefault(File.Exists);

					if (exeFilePath == null)
					{
						logger.LogWarning("No valid exe file exists in this folder - {rootDirectory}", rootDirectory);
					}

					return exeFilePath;
				}
			}
		}

		internal string NGenExecutablePath { get; }
		internal TimeSpan ExecutionTimeout { get; }

		readonly ILogger logger;
	}
}
