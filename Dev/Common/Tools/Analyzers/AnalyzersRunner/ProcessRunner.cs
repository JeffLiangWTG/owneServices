using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace AnalyzersRunner
{
	[SuppressMessage("CargoWiseOne", "CW1051:Do not use BaseSourcePath", Justification = "Runs against the projects")]
	[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Developer only tool")]
	static class ProcessRunner
	{
		[SuppressMessage("CargoWiseOne", "CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.", Justification = "Uses ProcessStartInfo")]
		public static string RunAnalysisOnProject(string projectFileRelativePath)
		{
			var baseSourcePath = TestCase.BaseSourcePath;

			RestorePackages(baseSourcePath, "Build", projectFileRelativePath);

			var arguments = $"\"{baseSourcePath.TrimEnd(Path.DirectorySeparatorChar)}\" \"{projectFileRelativePath}\" \"False\" \"{TestingState.IsRunningOnDAT}\"";

			var process = Process.Start(new ProcessStartInfo()
			{
				FileName = Assembly.GetExecutingAssembly().Location,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			});

			var output = new StringBuilder();
			process.OutputDataReceived += (sender, e) =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					output.AppendLine(e.Data);
				}
			};
			process.BeginOutputReadLine();

			process.ErrorDataReceived += (sender, e) =>
			{
				if (e.Data != null)
				{
					output.AppendLine(e.Data);
				}
			};
			process.BeginErrorReadLine();

			process.WaitForExit();

			return output.ToString();
		}

		public static void AssertNoIssuesOnProject(string projectFileRelativePath)
		{
			var output = RunAnalysisOnProject(projectFileRelativePath);

			if (output.Length > 0)
			{
				AssertionWithHtml.HtmlFail(output);
			}
			else
			{
				Assertion.Assert(true);
			}
		}

		static void RestorePackages(string workingDirectory, string paketGroup, string projectFileRelativePath)
		{
			var output = new StringBuilder();

			if (!TryExecuteProcess("dotnet", "tool restore", workingDirectory, output))
			{
				throw new InvalidOperationException($"'dotnet tool restore' failed: {output}");
			}

			output.Clear();

			//WI00586783 investigated any potential speed benefit from removing this paket restore step.
			//it was found that paket only took ~3 seconds to run, and the speed benefit was not
			//worth the cost of bundling the analyzer assemblies into the build.
			if (!TryExecuteProcess("dotnet", "paket restore --group " + paketGroup, workingDirectory, output))
			{
				throw new InvalidOperationException($"'dotnet paket restore' failed: {output}");
			}

			//restore nuget packages
			output.Clear();

			var projectDirectoryFullPath = Path.GetDirectoryName(Path.Combine(workingDirectory, projectFileRelativePath));
			if (!TryExecuteProcess("dotnet", "restore " + Path.GetFileName(projectFileRelativePath), projectDirectoryFullPath, output))
			{
				throw new InvalidOperationException($"'dotnet restore' failed: {output}");
			}
		}

		[SuppressMessage(
			"CargoWiseOne",
			"CW1078:Do not use Process.Start to open a file or url, use WebUrlLauncher or FileOpener for proper integration with Remote Desktop Services. False alarm if you are running a process for a reason other than to open a file or url.",
			Justification = "Test setup, RDP integration is not a concern")]
		static bool TryExecuteProcess(string path, string arguments, string workingDirectory, StringBuilder output)
		{
			var processStartInfo = new ProcessStartInfo
			{
				FileName = path,
				Arguments = arguments,
				CreateNoWindow = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				WorkingDirectory = workingDirectory,
			};

			using (var process = Process.Start(processStartInfo))
			{
				process.OutputDataReceived += (sender, e) =>
				{
					if (e.Data != null)
					{
						output.AppendLine(e.Data);
					}
				};
				process.BeginOutputReadLine();

				process.ErrorDataReceived += (sender, e) =>
				{
					if (e.Data != null)
					{
						output.AppendLine(e.Data);
					}
				};
				process.BeginErrorReadLine();

				process.WaitForExit();

				return process.ExitCode == 0;
			}
		}
	}
}
