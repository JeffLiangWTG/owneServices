using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using Enterprise.Integration;

namespace Enterprise.Client.EDI.Escrow.Interfaces
{
	sealed class GitAdapter : IGitAdapter
	{
		void RunCommand(string arguments, ILogger logger)
		{
			_ = GitDirectory ?? throw new InvalidOperationException($"{nameof(GitDirectory)} is not set.");

#pragma warning disable CW1078 // Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
			using (var process = new Process())
#pragma warning restore CW1078 // Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
			{
				var workingDirectory = Path.Combine(GitDirectory.DirectoryName, "cmd");
				process.StartInfo.FileName = Path.Combine(workingDirectory, "git.exe");
				process.StartInfo.Arguments = arguments;
				process.StartInfo.WorkingDirectory = workingDirectory;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.RedirectStandardOutput = true;

				process.StartInfo.RedirectStandardOutput = true;
				process.OutputDataReceived += (o, e) => LogIfNotNull(e);

				var stdErr = new StringBuilder();
				process.StartInfo.RedirectStandardError = true;
				process.ErrorDataReceived += (o, e) =>
				{
					stdErr.AppendLine(e?.Data ?? string.Empty);
					LogIfNotNull(e);
				};

				try
				{
#pragma warning disable CW1078 // Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
					if (!process.Start())
#pragma warning restore CW1078 // Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.
					{
						throw new GitAdapterException(-1, arguments,
							$"Failed to start the process: {process.StartInfo.FileName}.");
					}
				}
				catch (Win32Exception exception)
				{
					throw new GitExecutableFileException(exception);
				}

				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new GitAdapterException(process.ExitCode, arguments, stdErr.ToString());
				}
			}

			void LogIfNotNull(DataReceivedEventArgs e)
			{
				if (e?.Data != null)
				{
					logger.Log(LogType.Debug, e.Data);
				}
			}
		}

		public void Clone(string repository, string folder, ILogger logger, string extraArgs)
		{
			var cmd = string.IsNullOrEmpty(extraArgs)
				? $"-C \"{folder}\" clone --depth=1 --progress --verbose \"{repository}\" -c core.longpaths=true"
				: $"-C \"{folder}\" clone --depth=1 --progress --verbose \"{repository}\" -c core.longpaths=true {extraArgs}";

			RunCommand(cmd, logger ?? throw new ArgumentNullException(nameof(logger)));
		}

		public IWorkingDirectory GitDirectory { get; set; }
	}
}
