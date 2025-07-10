using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Integration.Abstractions;
using static System.FormattableString;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;

namespace Enterprise.ServiceManager.Host
{
	abstract class ProcessServiceRunnerCore : IServiceRunner
	{
		readonly Stopwatch stopwatch = new Stopwatch();
		readonly IProcessFactory processFactory;
		readonly IHostLogger hostLogger;

		internal ProcessServiceRunnerCore(
			ITaskScheduler taskScheduler,
			IBackgroundThreadActionQueue actionQueue,
			IProcessFactory processFactory,
			IHostLogger hostLogger,
			IServiceTaskLocksCleaner serviceTaskLocksCleaner,
			IDateTimeProvider dateTimeProvider,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			string taskGroup)
		{
			TaskScheduler = taskScheduler;
			ActionQueue = actionQueue;
			this.processFactory = processFactory;
			this.hostLogger = hostLogger;
			this.serviceTaskLocksCleaner = serviceTaskLocksCleaner ?? throw new ArgumentNullException(nameof(serviceTaskLocksCleaner));
			this.dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
			LastSeenNotIdle = dateTimeProvider.CurrentDateTimeUtc;
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.hostRegistry = hostRegistry ?? throw new ArgumentNullException(nameof(hostRegistry));
			TaskGroup = taskGroup;
		}

		protected ITaskScheduler TaskScheduler { get; private set; }
		protected IBackgroundThreadActionQueue ActionQueue { get; private set; }
		public DateTime LastSeenNotIdle { get; protected set; }

		protected bool ProcessRunning
		{
			get
			{
				try
				{
					var currentProcess = process;
					return currentProcess != null && !currentProcess.HasExited;
				}
				catch (ObjectDisposedException)
				{
					return false;
				}
			}
		}

		bool isStopping;

		string State
		{
			get
			{
				if (TaskRunning)
				{
					return "Running";
				}
				if (IsExiting)
				{
					return "Exiting";
				}
				if (isStopping)
				{
					return "Stopping";
				}

				return "Idle";
			}
		}

		bool IsExiting => HasProcess && !ProcessRunning;
		public bool IsIdle => !TaskRunning && !IsExiting && !isStopping;
		public abstract bool TaskRunning { get; }
		public bool IsAllocatingTask { get; set; }

		public TimeSpan ElapsedFromLastRun
		{
			get { return stopwatch.Elapsed; }
		}

		public event EventHandler<ProcessStartedEventArgs> ProcessStarted;
		public event EventHandler Exited;
		public event EventHandler TaskRunRequestCompleted;

		protected abstract void StopCore();
		protected abstract bool RunCore(ITaskRunRequest runRequest);

		protected void InvokeTaskRunRequestCompleted()
		{
			TaskRunRequestCompleted?.Invoke(this, null);
		}

		public void Stop()
		{
			if (!HasProcess)
			{
				Exited?.Invoke(this, null);
				return;
			}

			if (IsExiting || isStopping)
			{
				return;
			}

			StopCore();
			isStopping = true;
		}

		public bool Run(ITaskRunRequest runRequest)
		{
			if (TaskRunning)
			{
				throw new InvalidOperationException("Attempted to run whilst already running");
			}

			LastSeenNotIdle = dateTimeProvider.CurrentDateTimeUtc;
			TaskRunRequest = runRequest;
			ExpectingTaskRunRequestCallback = true;
			stopwatch.Restart();
			var runCoreResult = RunCore(runRequest);
			if (runCoreResult)
			{
				hostLogger.Log(LogLevel.Debug, runRequest.FormatRequestToLogMessage(LogMessageStage.RequestIsSentToRunner, this));
			}

			IsAllocatingTask = false;
			return runCoreResult;
		}

		public void CheckIdleStatus(IHostLogger logger, TimeSpan idleProcessExpiry)
		{
			if (IsIdle && dateTimeProvider.CurrentDateTimeUtc - LastSeenNotIdle > idleProcessExpiry)
			{
				logger.Log(LogLevel.Debug, Invariant($"Runner [{ToString()}] idle timeout has expired, was last used {new ZDateTime(LastSeenNotIdle)}.  Stopping runner."));
				Stop();
			}
			else
			{
				logger.Log(LogLevel.Debug, Invariant($"Runner [{ToString()}] retained, was last used {new ZDateTime(LastSeenNotIdle)}."));
			}
		}

		protected abstract ProcessStartInfo GetProcessStartInfo(IServiceTaskInfo info);

		protected bool StartProcess(ITaskRunRequest runRequest)
		{
			if (process != null)
			{
				errorReporterProxy.ReportOnce($"Attempted to start a process with a process already running, PID:[{process.Id}]");
				return false;
			}

			var info = runRequest.Task.Info;
			bool result;
			try
			{
				process = null;

				var startInfo = GetProcessStartInfo(info);
				if (startInfo != null)
				{
					HostLogger.Log(LogLevel.Debug, Invariant($"{info.Code} Starting a new Runner process with priority {hostRegistry.RunnerProcessPriorityValue} for initial use ({info.Description}). {startInfo.FileName} {startInfo.Arguments}"));
					startInfo.RedirectStandardOutput = true;
					startInfo.RedirectStandardError = true;
					startInfo.UseShellExecute = false;
					var temp = processFactory.Create(startInfo, true, hostRegistry.RunnerProcessPriorityValue);
					InitHooks(temp);
					result = temp.Start();
					if (result)
					{
						try
						{
							temp.BeginErrorReadLine();
							temp.BeginOutputReadLine();
						}
						catch (Exception ex) when (!ex.IsCriticalException())
						{
							// Ignore race condition. (Process could have already closed ;p)
						}
						process = temp;
						HostLogger.Log(LogLevel.Information, $"{info.Code} PID={temp.Id}: Started {Path.GetFileName(startInfo.FileName)} for initial use ({info.Description})");
						ProcessStarted?.Invoke(this, new ProcessStartedEventArgs(temp));
					}
					else
					{
						temp.Close();
					}
				}
				else
				{
					result = false;
				}
			}
			catch (Win32Exception ex)
			{
				HostLogger.Log(LogLevel.Error , "Could not start service runner process", ex);
				result = false;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				result = false;
				HostLogger.Log(LogLevel.Warning, Invariant($"Hosted service failed to start. Failed service code: {info.HostedServiceAttribute.Code}. Exception Detail: {ex.ToString()}"));
			}

			return result;
		}

		private protected void InitHooks(IProcess newProcess)
		{
			// There should never be more than one process for each ProcessRunner.
			newProcess.Exited += OnExited;
			newProcess.ErrorDataReceived += ErrorDataReceived;
			newProcess.OutputDataReceived += OutputDataReceived;
		}

		void TeardownHooks(IProcess exitedProcess)
		{
			exitedProcess.Exited -= OnExited;
			exitedProcess.OutputDataReceived -= OutputDataReceived;
			exitedProcess.ErrorDataReceived -= ErrorDataReceived;
		}

		protected virtual void OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			// does nothing
		}

		void ErrorDataReceived(object sender, DataReceivedEventArgs e)
		{
			var capturedTaskRunRequest = TaskRunRequest;

			ActionQueue.Enqueue(() =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					capturedTaskRunRequest?.Task.Log(LogLevel.Warning, Invariant($"Runner StdErr [{e.Data}]"));
				}
			});
		}

		void OnExited(object sender, EventArgs e)
		{
			ITaskRunRequest capturedTaskRunRequest = null;
			var capturedExpectingCallback = true;

			if (sender is IProcess finishedProcess)
			{
				TeardownHooks(finishedProcess);
				capturedTaskRunRequest = TaskRunRequest;
				capturedExpectingCallback = ExpectingTaskRunRequestCallback;

				if (finishedProcess != process)
				{
					errorReporterProxy.ReportOnce(nameof(ProcessServiceRunnerCore), new InvalidOperationException($"Process id of exited runner [PID={finishedProcess.Id}] does not match expected process id [PID={process?.Id}]"));
				}

				TaskRunRequest = null;
				ExpectingTaskRunRequestCallback = false;
				process = null;
			}

			ActionQueue.Enqueue(() =>
			{
				// This can be called on the main thread during a call to Process.HasExited
				// or on the RPC thread when the process handle is signaled.
				// If we are on the RPC thread and the main thread calls HasExited, which returns true,
				// then the main thread will be wanting to call RunTask and start a new process simultaneously.
				if (sender is IProcess p)
				{
					var exitCode = RunnerExitCode.NoIssues;
					var id = 0;
					try
					{
						exitCode = p.ExitCode;
						id = p.Id;
					}
					catch (InvalidOperationException)
					{
						// either exitCode or Id can throw this exception
					}

					var serviceTask = capturedTaskRunRequest?.Task;
					var exitTaskInfo = serviceTask?.Info;
					HostLogger.Log(
						exitCode == RunnerExitCode.NoIssues ? LogLevel.Debug : LogLevel.Warning,
						Invariant($"PID={id}: Terminated with ExitCode={exitCode}. {(exitTaskInfo != null ? exitTaskInfo.Code + " - " + exitTaskInfo.Description : "taskInfo==null ")}"));

					if (capturedExpectingCallback && exitCode != RunnerExitCode.ServiceTaskLockNotReleased)
					{
						capturedTaskRunRequest?.OnUnableToRun(UnableToRunReason.HostDidNotReceiveRunnerProcessingFinishedCallback, true, failedPostScheduleUpdate: true);
					}
					else if (!disposed && exitCode != RunnerExitCode.NoIssues)
					{
						if (exitCode == RunnerExitCode.ServiceTaskUnhandledException)
						{
							AddExceptionToLimiter(serviceTask);
						}

						if (exitCode != RunnerExitCode.ServiceTaskLockNotReleased)
						{
							SetExceptionStatusOnLastRun(exitTaskInfo);
							capturedTaskRunRequest?.OnUnableToRun(UnableToRunReason.RunnerProcessExited, true, failedPostScheduleUpdate: true);
						}
					}

					try
					{
						HostLogger.Log(LogLevel.Debug, Invariant($"PID={id}: {serviceTask?.Code}, Clean locks from service tasks"));
						serviceTaskLocksCleaner.ReleaseLocksFromServiceTask(id, (exitTaskInfo
							?.HostedServiceAttribute
							?.MutuallyExclusiveTaskGroup ?? MutuallyExclusiveServiceTaskGroups.NoGroup).ToString());

						serviceTaskLocksCleaner.ReleaseLocksFromServiceTask(id, serviceTask?.Code);
					}
					catch (Exception exception) when (!exception.IsCriticalException())
					{
						HostLogger.Log(LogLevel.Debug, exception, Invariant($"PID={id}: {serviceTask?.Code}, CleanupServiceTaskLocks error"));
					}
					p.Dispose();
				}
				else
				{
					errorReporterProxy.ReportOnce(nameof(ProcessServiceRunnerCore), new InvalidOperationException($"Exit raised with sender of type [{sender?.GetType()}] must be [{typeof(IProcess)}]"));
				}

				Exited?.Invoke(this, null);
			});
		}

		public int ProcessId
		{
			get
			{
				var currentProcess = process;
				try
				{
					return currentProcess?.Id ?? 0;
				}
				catch (InvalidOperationException)
				{
					return 0;
				}
			}
		}

		string ProcessIdString
		{
			get
			{
				var processId = ProcessId;
				return (processId != 0) ? processId.ToString(CultureInfo.InvariantCulture) : "None";
			}
		}

		protected bool HasProcess => process != null;

		protected string ProcessInfo => Invariant($"PID={ProcessIdString}, State={State}");

		public sealed override string ToString()
		{
			var taskInfo = TaskRunning ? TaskRunRequest?.Task.Info : null;
			if (taskInfo != null)
			{
				return Invariant($"{ProcessInfo}, Task={taskInfo.Code} - {taskInfo.Description}");
			}

			return ProcessInfo;
		}

		protected virtual IHostLogger HostLogger => hostLogger;

		static void SetExceptionStatusOnLastRun(IServiceTaskInfo taskWithException)
		{
			if (taskWithException != null)
			{
				taskWithException.ErrorOnLastRun = true;
			}
		}

		void AddExceptionToLimiter(IRunnableServiceTask runnableServiceTask)
		{
			var taskInfo = runnableServiceTask?.Info;
			if (taskInfo != null)
			{
				runnableServiceTask.RecordLastRunError();
				HostLogger.Log(LogLevel.Debug, Invariant($"{taskInfo.HostedServiceAttribute.Code}:{taskInfo.HostedServiceAttribute.Description} exception added."));
			}
		}

		public virtual void Kill(bool withLogging = true)
		{
			var processToKill = process;
			if (processToKill != null)
			{
				var logger = withLogging ? HostLogger : null;

				try
				{
					logger?.Log(LogLevel.Information, "PID=" + processToKill.Id + ": Terminating");
				}
				catch (InvalidOperationException)
				{
					// Id can throw this exception
				}

				var tryToKill = 3;
				while (tryToKill-- > 0 && !SafeHasExited(processToKill))
				{
					SafeSuspendAllThreads(processToKill);

					if (KillAndWaitForExit(processToKill, logger))
					{
						break;
					}
				}

				// This is to work around the 4.5 bug where not closing a connection leaks
				Db.Connection.Dispose();
			}
		}

		protected internal void KillInBackground()
		{
			var processToKill = process;
			if (processToKill != null)
			{
				System.Threading.Tasks.Task.Factory.StartNew(() =>
				{
					try
					{
						processToKill.Kill();
					}
					catch (Exception ex) when (!ex.IsCriticalException())
					{
					}
				});
			}
			TaskRunRequest = null;
		}

		protected IProcess process;
		readonly IServiceTaskLocksCleaner serviceTaskLocksCleaner;
		protected readonly IDateTimeProvider dateTimeProvider;
		readonly IErrorReporterProxy errorReporterProxy;
		protected ITaskRunRequest TaskRunRequest { get; private set; }
		public bool ExpectingTaskRunRequestCallback { get; private protected set; }

		public string TaskGroup { get; }

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
			// This object will be cleaned up by the Dispose method.
			// Therefore, you should call GC.SupressFinalize to
			// take this object off the finalization queue
			// and prevent finalization code for this object
			// from executing a second time.
			GC.SuppressFinalize(this);
		}

		static bool SafeHasExited(IProcess process)
		{
			var result = true;
			try
			{
				result = process == null || process.HasExited;
			}
			catch (InvalidOperationException)
			{
			}
			return result;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				DisposeCore(disposing);
				Kill(withLogging: false);
			}
			disposed = true;
		}

		static bool KillAndWaitForExit(IProcess processToKill, IHostLogger hostLogger)
		{
			var done = true;

			try
			{
				processToKill.Kill();
				done = processToKill.WaitForExit(10000);
			}
			catch (Win32Exception ex)
			{
				done = false;
				if (ex.NativeErrorCode != 5) // If the call to the Kill method is made while the process is currently terminating, a Win32Exception is thrown for Access Denied (NativeErrorCode = 5).
				{
					done = true;
					hostLogger?.Log(LogLevel.Error, "An error occurred when attempting to terminate runner process", ex);
				}
			}
			// The process has already exited.
			// -or-
			// There is no process associated with this Process object.
			catch (InvalidOperationException)
			{
				done = true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				done = false;
				hostLogger?.Log(LogLevel.Error, "An error occurred when attempting to terminate runner process", ex);
			}

			return done;
		}

		[DllImport("kernel32.dll")]
		static extern IntPtr OpenThread(int dwDesiredAccess, bool bInheritHandle, uint dwThreadId);
		[DllImport("kernel32.dll")]
		static extern uint SuspendThread(IntPtr hThread);
		[DllImport("kernel32.dll")]
		static extern bool CloseHandle(IntPtr hThread);

		[SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", Justification = "Best attempt to suspend, nothing can be done if it fails.")]
		static void SafeSuspendAllThreads(IProcess processToKill)
		{
			try
			{
				foreach (ProcessThread processThread in processToKill.Threads)
				{
					var pOpenThread = OpenThread(0x0002, false, (uint)processThread.Id); // THREAD_SUSPEND_RESUME (0x0002)	Required to suspend or resume a thread (see SuspendThread and ResumeThread).
					if (pOpenThread != IntPtr.Zero)
					{
						SuspendThread(pOpenThread);
						CloseHandle(pOpenThread);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
			}
		}

		protected virtual void DisposeCore(bool disposing)
		{
		}

		bool disposed;
		protected readonly IHostRegistrySettings hostRegistry;

		#endregion
	}
}
