using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace AppDomainWrappers.Net
{
	public static class RunInProcessHelpers
	{
		const string HOST_LOCK_FILE_NAME = "host.lock";
		const string CLIENT_LOCK_FILE_NAME = "client.lock";

		static string GetBinFolder()
		{
			return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		public static string RunCodeInProcessNet(string codeToRun, string wrapperFileName, List<string> dllFiles = null, List<string> imports = null)
		{
			var config = new ProcessConfig();
			config.RunMode = ProcessConfig.AppDomainRunMode.RawCode;

			var binFolder = GetBinFolder();

			config.AssemblyDependancies = dllFiles?.Select(d => string.Join($"{binFolder}\\", d))?.ToList();
			config.UsingImports = imports;

			codeToRun = codeToRun.Replace("\"", "\\\"");
			var extraProcessArguments = $"--codeToRun \"{codeToRun}\"";

			return RunInProcess(config, wrapperFileName, extraProcessArguments);
		}

		public static string RunMethodInProcess(ProcessConfig processConfig, string wrapperFileName)
		{
			var config = (ProcessConfig)processConfig.Clone();
			config.RunMode = ProcessConfig.AppDomainRunMode.Method;

			return RunInProcess(config, wrapperFileName);
		}

		static string RunInProcess(ProcessConfig config, string wrapperFileName, string extraProcessArguments = null)
		{
			string result;
			config.TempConfigDirectoryPath = TemporaryWorkspace.GenerateTempDirectoryPathWithoutCreation();
			using (new TemporaryWorkspace(Path.GetFileName(config.TempConfigDirectoryPath)))
			{
				var heartbeat = HeartbeatSetUp(config);
				try
				{
					var configJson = JsonSerializer.Serialize(config);
					var configFilePath = Path.Combine(config.TempConfigDirectoryPath, "config.json");
					File.WriteAllText(configFilePath, configJson);

					var binFolder = GetBinFolder();
					if (!Directory.Exists(binFolder))
					{
						throw new DirectoryNotFoundException($"Directory not found: '{binFolder}'.");
					}

					var subFolder = wrapperFileName.Contains("Net48") ? Path.Combine("AppDW", "Net48") : "net8.0";
					var filePath = Path.Combine(binFolder, subFolder, wrapperFileName);

					var arguments = $"--config {configFilePath}";

					if (!string.IsNullOrWhiteSpace(extraProcessArguments))
					{
						arguments += " " + extraProcessArguments;
					}

					result = config.DoNotUseTempDirectory
					? RunProcessAndReturnOutput(filePath, arguments)
					: RunProcessInTemporaryDirectory(filePath, arguments, config.TempWorkingDirectoryPath);
				}
				finally
				{
					HeartbeatTearDown(heartbeat);
				}
			}

			return result;
		}

		static string RunProcessInTemporaryDirectory(string filePath, string arguments, string tempDirectoryPath)
		{
			using (new TemporaryWorkspace(tempDirectoryPath))
			{
				return RunProcessAndReturnOutput(filePath, arguments);
			}
		}

		#region Process Handling
		static string RunProcessAndReturnOutput(string filePath, string arguments)
		{
			var process = CreateProcess(filePath, arguments);

			var outputBuilder = HandleProcessOutput(process);

			if (!process.Start())
			{
				throw new InvalidOperationException("Process, with provided filePath and arguments, failed to start.");
			}

			return ReadAllOutput(process, outputBuilder);
		}

		static Process CreateProcess(string filePath, string arguments)
		{
			if (!File.Exists(filePath))
			{
				throw new FileNotFoundException("File not found", filePath);
			}

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = filePath,
					Arguments = arguments,
					WorkingDirectory = Directory.GetParent(filePath).Parent.FullName,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
				}
			};

			return process;
		}

		static StringBuilder HandleProcessOutput(Process process)
		{
			StringBuilder outputBuilder = new();
			process.OutputDataReceived += AppendLineToStringBuilder;
			process.ErrorDataReceived += AppendLineToStringBuilder;

			process.Exited += HasExited;

			return outputBuilder;

			void AppendLineToStringBuilder(object sender, DataReceivedEventArgs e)
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					_ = outputBuilder.AppendLine(e.Data);
				}
			}

			void HasExited(object sender, EventArgs e)
			{
				process.OutputDataReceived -= AppendLineToStringBuilder;
				process.ErrorDataReceived -= AppendLineToStringBuilder;
				process.Exited -= HasExited;
			}
		}

		static string ReadAllOutput(Process process, StringBuilder outputBuilder)
		{
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit();

			_ = process.ExitCode;
			process.Close();
			return outputBuilder.ToString();
		}
		#endregion

		#region Heartbeat
		static IHeartbeat HeartbeatSetUp(ProcessConfig config)
		{
			IHeartbeat heartbeat = new HeartbeatServer(HOST_LOCK_FILE_NAME);

			config.ClientLockFileName = CLIENT_LOCK_FILE_NAME;
			config.HostLockFileName = heartbeat.GetFileName();

			heartbeat.HostStateChanged += (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Host);
			_ = heartbeat.InitialiseMyLockFile(config.TempConfigDirectoryPath);
			_ = heartbeat.DoLockMyLockFile();

			heartbeat.ClientStateChanged += (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Client);
			_ = heartbeat.DoMonitorLockFile(
					Path.Combine(config.TempConfigDirectoryPath, config.ClientLockFileName),
					waitForConnection: true);   // Host is the starting point, so wait until the client creates and locks it's file.

			return heartbeat;
		}

		static void HeartbeatTearDown(IHeartbeat heartbeat)
		{
			_ = heartbeat.DoCancelMyLockFile();
			heartbeat.HostStateChanged -= (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Host);
			heartbeat.ClientStateChanged -= (s, e) => Heartbeat_StateChanged(e, HeartbeatExecutionMode.Client);
		}

		static void Heartbeat_StateChanged(LockFileStateChangedEventArgs e, HeartbeatExecutionMode mode)
		{
			if (e.State == LockFileState.Disconnected
					|| e.State == LockFileState.Failure)
			{
				if (!e.CancellationTokenSource.IsCancellationRequested)
				{
					// Failure
					if (mode == HeartbeatExecutionMode.Host)
					{
					}
					else
					{
					}
				}
			}
		}
		#endregion
	}
}

