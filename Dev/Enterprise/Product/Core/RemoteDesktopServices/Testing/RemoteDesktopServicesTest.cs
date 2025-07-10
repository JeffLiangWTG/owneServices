using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	public abstract class RemoteDesktopServicesTest : TestCaseWithFactory
	{
		public const string ControlChars = "?/*&";
		int clientProcessId;
		StreamWriter clientProcessStandardInput;

		[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Inherited classes have SOURCE_CODE")]
		protected override void SetUp()
		{
			SetupMock();
			InitializeServer();
		}

		[SuppressMessage("CargoWiseOne", "CW1051:DoNotUseBaseSourcePath", Justification = "Inherited classes have SOURCE_CODE")]
		protected void SetupMock()
		{
			clientProcessCancellationTokenSource = new CancellationTokenSource();
			clientProcessErrors = new StringBuilder();
			clientProcessOutput = new StringBuilder();

			var (process, clientProcessStandardInput) = StartClientProcess(Path.Combine(AssemblyLoader.GetBinPath(), TestClientHostExeName), BaseSourcePath + " " + GetChannelNumber(), clientProcessErrors, clientProcessOutput, clientProcessCancellationTokenSource);
			this.clientProcessStandardInput = clientProcessStandardInput;
			clientProcess = process;

			if (process != null && !process.HasExited && string.IsNullOrEmpty(clientProcessErrors.ToString()))
			{
				clientProcessId = process.Id;
				clientProcess = process;

				clientProcessStandardInput.WriteLine("Ready");

				ClientProcessId = clientProcess.Id;

				WtsApi.Instance = new MockWtsApi(clientProcess, ControlChars, clientProcessStandardInput);
			}
			else
			{
				clientProcess = null;
				clientProcessErrors.AppendLine("Failed to start up client process on SetUp with errors");
				FailWithFailureDetails(null);
			}

			instanceDetailsDisposable = InstanceDetails.SetUpCurrentForTest(new InstanceDetails("TestDomain", "TestInstance", Db.ServerName, Db.DatabaseName.ToUpper()));
		}
		IDisposable instanceDetailsDisposable;

		private protected static (Process Process, StreamWriter clientProcessStandardInput) StartClientProcess(string fileName, string arguments, StringBuilder errors, StringBuilder consoleOutput, CancellationTokenSource cancellationTokenSource)
		{
			var clientProcessStart = new ProcessStartInfo
			{
				FileName = fileName,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true,
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};

			var process = new Process { StartInfo = clientProcessStart };
			process.ErrorDataReceived += (sender, args) =>
			{
				if (!cancellationTokenSource.IsCancellationRequested && !string.IsNullOrEmpty(args.Data))
				{
					errors.AppendLine(args.Data);
				}
			};

			process.OutputDataReceived += (sender, args) =>
			{
				if (!cancellationTokenSource.IsCancellationRequested && !string.IsNullOrEmpty(args.Data))
				{
					consoleOutput.AppendLine(args.Data.EndsWith(ControlChars) ? args.Data.Replace(ControlChars, string.Empty) : args.Data);
				}
			};

			try
			{
				if (!process.Start())
				{
					errors.AppendLine($"Failed to start the process: {fileName} {arguments}.");
					return (null, null);
				}

				consoleOutput.AppendLine($"ClientProcess StartedTime: {process.StartTime}");
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
			}
			catch (Exception exception) when (!exception.IsCriticalException())
			{
				errors.AppendLine($"Failed to start the process: {fileName} {arguments}, with error: {exception}.");
				return (null, null);
			}

			var started = false;
			var initialisationTimeout = TimeSpan.FromSeconds(30);
			var tokenSource = new CancellationTokenSource(initialisationTimeout);

			while (!started)
			{
				try
				{
					started = process.StartTime <= DateTime.Now;
				}
				catch (InvalidOperationException)
				{
					if (tokenSource.IsCancellationRequested)
					{
						cancellationTokenSource.Cancel();
						errors.AppendLine($"TestClientHost failed to start after {initialisationTimeout.TotalSeconds} seconds.");
					}

					Thread.Sleep(100);
				}
			}

			var clientProcessStandardInput = new StreamWriter(process.StandardInput.BaseStream, new UTF8Encoding(false), bufferSize: 4096, leaveOpen: true)
			{
				AutoFlush = true
			};

			return (process, clientProcessStandardInput);
		}

		protected virtual int GetChannelNumber()
		{
			return 1;
		}

		protected virtual void InitializeServer(bool waitForInitialization = true)
		{
			try
			{
				EnterpriseChannel.Initialize();
				EnterpriseChannel.InitializeUI("EDIEDIDAT", "DEMHQ");
				startDropHandler = new StartDropHandlerForTest();
				MessageHandlers.Register(EnterpriseChannelMessageTypes.StartDrop, startDropHandler);

				if (waitForInitialization)
				{
					var initialisationTimeout = TimeSpan.FromSeconds(90);
					var tokenSource = new CancellationTokenSource(initialisationTimeout);

					while (InitializationMessageHandler.RegisteredRemoteMessageTypes.Length == 0
							|| !IsRemotingServerRegistered())
					{
						Thread.Sleep(10);
						Application.DoEvents();
						if (tokenSource.IsCancellationRequested)
						{
							FailTestForInitialisationTimeout(initialisationTimeout);
						}
					}

					initializationEvent.Set();
				}
			}
			catch (Exception ex) when (ex is not BreakableException)
			{
				FailWithFailureDetails(ex);
			}
		}

		bool IsRemotingServerRegistered()
		{
			using (var key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"EdiEnterprise.edient\shell\open\command\EDIEDIDAT"))
			{
				return key != null && (int)key.GetValue("PID") == clientProcess.Id;
			}
		}

		void FailTestForInitialisationTimeout(TimeSpan timeoutDuration)
		{
			var channelClassName = nameof(EnterpriseChannel);
			var channelInstancePropertyName = $"{channelClassName}.{nameof(EnterpriseChannel.Instance)}";
			clientProcessOutput.AppendLine($"{channelClassName} failed to initialise after {timeoutDuration.TotalSeconds} seconds.");
			clientProcessOutput
				.Append($"{channelInstancePropertyName} is")
				.Append(EnterpriseChannel.Instance == null ? "" : " not")
				.AppendLine(" null.");

			clientProcessOutput
				.Append($"{channelClassName} is")
				.Append(EnterpriseChannel.Instance.IsConnected ? "" : " not")
				.AppendLine(" connected.");

			clientProcessOutput
				.Append("Initialisation message has")
				.Append(InitializationMessageHandler.InitializationCompleted.WaitOne(0) ? "" : " not")
				.AppendLine(" been handled.");

			clientProcessOutput
				.AppendLine("----------------------------------");

			clientProcessOutput
				.AppendLine($"Channel has connected {EnterpriseChannel.Instance.ConnectedCount} times.");

			if (InitializationMessageHandler.RegisteredRemoteMessageTypes.Length == 0)
			{
				clientProcessOutput.AppendLine("No remote message types have been registered.");
			}
			else
			{
				clientProcessOutput.AppendLine("The following remote message types have been registered:");
				foreach (var messageType in InitializationMessageHandler.RegisteredRemoteMessageTypes)
				{
					clientProcessOutput.AppendLine($"    {messageType}");
				}
			}

			clientProcessOutput
				.Append("The remoting server is")
				.Append(IsRemotingServerRegistered() ? "" : " not")
				.AppendLine(" registered.");

#pragma warning disable CS0618 // Type or member is obsolete
			try
			{
				if (EnterpriseChannel.Instance.ReadThreadForTest != null)
				{
					EnterpriseChannel.Instance.ReadThreadForTest?.Suspend();
#if NETFRAMEWORK
					var stack = new StackTrace(EnterpriseChannel.Instance.ReadThreadForTest, true);
#else
					// In .NET 8, capturing the stack trace of another thread is not supported
					var stack = new StackTrace(fNeedFileInfo: true);
#endif

					clientProcessOutput
						.AppendLine("ReadThread Stack :")
						.AppendLine(stack.ToString());
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				EnterpriseChannel.Instance.ReadThreadForTest?.Resume();
			}
#pragma warning restore CS0618 // Type or member is obsolete

			clientProcessOutput.AppendLine("----------------------------------");
			FailWithFailureDetails(null);
		}

		protected override void TearDown()
		{
			try
			{
				instanceDetailsDisposable?.Dispose();
				InitializationMessageHandler.InitializationCompleted.Reset();
				InitializationMessageHandler.RemoteInitializationMessage = null;
				EnterpriseChannel.Instance.Close();
				EnterpriseChannel.Instance.ActivityLogger.Clear();
				EnterpriseChannel.Reset();
				startDropHandler?.Dispose();
				WtsApi.Instance = null;
			}
			catch (Exception ex)
			{
				FailWithFailureDetails(ex);
			}
			finally
			{
				try
				{
					if (IsClientProcessRunning())
					{
						KillProcess();
					}
					else
					{
						clientProcessErrors.AppendLine("Client host process is not running");
					}
				}
				catch (Exception ex)
				{
					FailWithFailureDetails(ex);
				}
			}

			AssertNoClientProcessError();
		}

		protected virtual void AssertNoClientProcessError()
		{
			if (clientProcessErrors.Length > 0)
			{
				FailWithFailureDetails(null);
			}
		}

		void KillProcess()
		{
			if (clientProcess is null)
			{
				return;
			}

			using (clientProcess)
			{
				clientProcessStandardInput.WriteLine("END");
				clientProcessStandardInput.Dispose();

				var waitTimeForNaturalExit = TimeSpan.FromSeconds(2d); // Time to wait for the client process to exit naturally after sending END message
				var timeout = TimeSpan.FromSeconds(10d);
				var stopwatch = new Stopwatch();
				stopwatch.Start();

				while (IsClientProcessRunning() && stopwatch.Elapsed < timeout && !clientProcess.WaitForExit(10))
				{
					if (stopwatch.Elapsed > waitTimeForNaturalExit)
					{
						clientProcess.Kill();
					}
					Thread.Sleep(10);
				}

				clientProcessCancellationTokenSource.Cancel(); // Stop listening to client output and error streams

				if (IsClientProcessRunning())
				{
					clientProcessErrors.AppendLine("Client process failed to exit");
				}
				else
				{
					clientProcessOutput.AppendLine($"Client process exit ExitedTime: {ObtainProcessInfo(x => x.ExitTime, DateTime.Now)}");
					clientProcessOutput.AppendLine($"Client process exit ExitCode: {ObtainProcessInfo(x => x.ExitCode, -1000)}");
				}

				T ObtainProcessInfo<T>(Func<Process, T> processInfoFunc, T defaultResult)
				{
					try
					{
						return processInfoFunc(clientProcess);
					}
					catch
					{
						return defaultResult;
					}
				}
			}
		}

		void FailWithFailureDetails(Exception ex)
		{
			var sb = new StringBuilder();

			if (ex is not null)
			{
				sb.AppendLine($"Exception: {ex}");
			}

			sb.AppendLine($"Errors from test client process: {clientProcessErrors}");
			sb.AppendLine($"Output from test client process: {clientProcessOutput}");

			Fail(sb.ToString());
		}

		[SuppressMessage("CargoWiseOne", "CW1055:DoNotUseProcessGetProcesses", Justification = "Baseline")]
		bool IsClientProcessRunning()
		{
			return Process.GetProcesses().Any(p => p.Id == clientProcessId);
		}

		[SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Outside of Enterprise")]
		protected string GetRemoteFilePath(string localFilePath)
		{
			return Path.Combine(Path.GetTempPath(), Path.GetFileName(localFilePath));
		}

		protected const string TestClientHostExeName = "Enterprise.RemoteDesktopServices.TestClientHost.exe";
		protected int ClientProcessId;
		protected AutoResetEvent initializationEvent = new AutoResetEvent(false);
		CancellationTokenSource clientProcessCancellationTokenSource;
		Process clientProcess;
		protected StringBuilder clientProcessErrors;
		protected StringBuilder clientProcessOutput;
		StartDropHandlerForTest startDropHandler;
	}

	class StartDropHandlerForTest : StartDropHandler, IDisposable
	{
		public override DragDropHelper GetDragDropHelperInstance()
		{
			if (helperInstance == null)
			{
				helperInstance = new DragDropHelperForTest();
			}
			return helperInstance;
		}
		DragDropHelper helperInstance;

		public void Dispose()
		{
			Application.UseWaitCursor = false;
		}
	}

	class DragDropHelperForTest : DragDropHelper
	{
		protected override void SelectionForm_ShownCore(Button okButton)
		{
			SelectionFormPopCount++;
			okButton.PerformClick();
		}

		protected override Form GetCurrentActiveForm()
		{
			return null;
		}
		protected override bool HasActionOnSelectionFormShown => true;
		public int SelectionFormPopCount { get; set; }
	}
}
