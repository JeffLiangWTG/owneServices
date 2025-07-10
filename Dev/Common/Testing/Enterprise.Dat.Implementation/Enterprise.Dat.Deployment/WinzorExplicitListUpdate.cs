using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using CargoWise.BuildTools;
using Dat.Integration;
using WTG.DeploymentUtils.FileSystem;

namespace Enterprise.Dat.Implementation
{
	class WinzorExplicitListUpdate : IDeploymentProcess
	{
		public WinzorExplicitListUpdate(DeploymentConfiguration configuration)
		{
			Configuration = configuration;
		}

		public DeploymentConfiguration Configuration { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Creating process for TestFailureListGenerator.exe, not to open a file or url")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "Baseline")]
		public void Deploy(ITaskLogger logger)
		{
			var apiClient = new SubmissionsApiClient();
			var utPk = apiClient.GetLatestBuild("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", 99538);

			var startInfo = new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Winzor", "TestFailureListGenerator.exe"), $"/UpdateAll {utPk} \"{Configuration.SourcePath}\"");
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			using (var process = Process.Start(startInfo))
			{
				var output = new StringBuilder();
				var error = new StringBuilder();
				process.OutputDataReceived += (sender, e) => output.AppendLine(e.Data ?? string.Empty);
				process.ErrorDataReceived += (sender, e) => error.AppendLine(e.Data ?? string.Empty);
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				process.WaitForExit();
				if (process.ExitCode != 0)
				{
					throw new IOException(process.StartInfo.FileName + " " + process.StartInfo.Arguments + " exited with " + process.ExitCode + System.Environment.NewLine + output.ToString() + System.Environment.NewLine + error.ToString());
				}
			}

			using (var sourceControl = SourceControlFactory.Instance.GetSourceControl(Configuration.SourcePath))
			{
				var files = sourceControl.GetFilesWithPendingChanges();
				if (files.Length > 0)
				{
					((GitSourceControl)sourceControl).UseIdentity("DAT Service", "dat@wisetechglobal.com");
					sourceControl.SubmitChanges("dat", "Update Winzor Explicit Lists " + DateTime.Now.ToString("yyyyMMddHHmmss"), string.Empty, files);
				}
				else
				{
					logger?.RecordInfo("No pending changes in local repository");
				}
			}
		}
	}
}
