using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.Integration.Licensing;
using Enterprise.ServiceManager.Shared;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManager.Shared.Abstractions;
using ServiceManagerProto;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Host
{
	class ProcessServiceRunner : ProcessServiceRunnerCore, IGrpcPortResolver
	{
		public ProcessServiceRunner(
			ITaskScheduler taskScheduler,
			IBackgroundThreadActionQueue actionQueue,
			IProcessFactory processFactory,
			IProcessRunnerRemotingServices serviceProcessRunnerRemotingServices,
			IHostLogger hostLogger,
			IGrpcClientSynchronizerFactory grpcClientSynchronizerFactory,
			IServiceTaskLocksCleaner serviceTaskLocksCleaner,
			IDateTimeProvider dateTimeProvider,
			IErrorReporterProxy errorReporterProxy,
			IHostRegistrySettings hostRegistry,
			string taskGroup,
			IProductRegistration productRegistration)
			: base(
				taskScheduler,
				actionQueue,
				processFactory,
				hostLogger,
				serviceTaskLocksCleaner,
				dateTimeProvider,
				errorReporterProxy,
				hostRegistry,
				taskGroup)
		{
			remotingServices = serviceProcessRunnerRemotingServices ?? throw new ArgumentNullException(nameof(serviceProcessRunnerRemotingServices));
			this.grpcClientSynchronizerFactory = grpcClientSynchronizerFactory ?? throw new ArgumentNullException(nameof(grpcClientSynchronizerFactory));
			responseTaskCancellationTokenSource = new CancellationTokenSource();
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.productRegistration = productRegistration;
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exceptions communicating with the runner are handled by runner termination and logging of the exception")]
		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public override bool TaskRunning
		{
			get
			{
				try
				{
					isRunning = CheckIsRunning();
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					HandleCommandException(ex);
				}

				return isRunning;
			}
		}

		bool isRunning;

		protected virtual bool CheckIsRunning()
		{
			if (!ProcessRunning)
			{
				return false;
			}

			return ExpectingTaskRunRequestCallback;
		}

		protected override bool RunCore(ITaskRunRequest runRequest)
		{
			switch (runRequest)
			{
				case IDirectTaskRunRequest directTaskRunRequest:
					return StartProcessAndSendCommand(
						directTaskRunRequest,
						runner => runner.Run(
							directTaskRunRequest.Task.Info.HostedServiceAttribute.TypeAssemblyName,
							directTaskRunRequest.Task.Info.HostedServiceAttribute.Code,
							directTaskRunRequest.Id,
							directTaskRunRequest.Task.ConfigString));

				case IScheduledTaskRunRequest scheduledTaskRunRequest:
					return StartProcessAndSendCommand(
						scheduledTaskRunRequest,
						runner => runner.Run(
							scheduledTaskRunRequest.Task.Info.HostedServiceAttribute.TypeAssemblyName,
							scheduledTaskRunRequest.Task.Info.HostedServiceAttribute.Code,
							scheduledTaskRunRequest.Id,
							scheduledTaskRunRequest.ExpectedNextRunTime.Value.UtcDateTime,
							scheduledTaskRunRequest.NextRunTime.Value.UtcDateTime,
							scheduledTaskRunRequest.Task.ConfigString));

				default:
					throw new ArgumentOutOfRangeException(nameof(runRequest));
			}
		}

		protected override void StopCore()
		{
			SendCommand(runner => runner.Stop());
			GrpcPort = null;
		}

		protected override ProcessStartInfo GetProcessStartInfo(IServiceTaskInfo info)
		{
			// cheeky hack to stop service runners opening then hanging
			// In the case where SQL is dead this will block - instead of starting 20 runners and having them all block
			// Profiled WiseGrid and found about 5 a minute get created so impact will be very low.
			Db.Connection.EnsureIsOpen();

			var fileName = hostRegistry.SwitchRunnerToNetCore
				? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "net8.0", "ServiceManager.Runner.CW.exe")
				: Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ServiceManager.Runner.CW.exe");
			var productKey = $"-ProductKey:{productRegistration.Key.EnterpriseCode}{productRegistration.Key.ServerCode}" ;
			var enableConnectionPooling = hostRegistry.ServiceTaskRunnerConnectionPoolingEnabled ? "-EnableConnectionPooling" : string.Empty;
			var arguments = Invariant($" -NoUI \"-grpc:{eventHandleNames.BaseEventWaitHandleName}\" {Db.ServerName} {Db.DatabaseName} {enableConnectionPooling} {productKey}");

			var sdir = System.Environment.GetCommandLineArgs().FirstOrDefault(x => x.StartsWith(HostCommandLineOptions.ServerDirectoryForEnterpriseArgumentPrefix, StringComparison.OrdinalIgnoreCase));
			if (sdir != null)
			{
				arguments += " " + CommandLineArgEncoder.EnquoteArgumentIfNeeded(sdir);
			}

			var result = new ProcessStartInfo(fileName, arguments)
			{
				WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
				UseShellExecute = false,
			};

			return result;
		}

		bool StartProcessAndSendCommand(ITaskRunRequest runRequest, Action<IRunnerCommandQueueProvider> command)
		{
			if (!ProcessRunning)
			{
				if (!StartProcessAndWaitForReadySignal(runRequest))
				{
					return false;
				}
			}

			if (ProcessRunning)
			{
				var commandResult = SendCommand(command, runRequest);
				if (!commandResult)
				{
					ReportProcessFailedToRun(runRequest.Task, "An exception occurred when sending the command, see service host log for details", LogLevel.Error);
				}

				return commandResult;
			}

			return false;
		}

		bool StartProcessAndWaitForReadySignal(ITaskRunRequest runRequest)
		{
			GrpcPort = null;
			var task = runRequest.Task;
			if (HasProcess)
			{
				ReportProcessFailedToRun(task, "The process is still exiting, cannot start new process yet", LogLevel.Debug);
				return false;
			}

			eventHandleNames = new GrpcEventHandleNames();
			using (var grpcClientSynchronizer = grpcClientSynchronizerFactory.Create(eventHandleNames))
			{
				var isProcessRunning = base.StartProcess(runRequest);
				if (!isProcessRunning)
				{
					ReportProcessFailedToRun(task, "The process did not start", LogLevel.Error);
					return false;
				}

				var timeout = hostRegistry.ServiceTaskUnloadTimeoutInSeconds * 2;
				HostLogger.Log(LogLevel.Debug, task?.Info?.HostedServiceAttribute, Invariant($"Waiting for runner ready signal for {timeout} ({task.Code}, {this})."));
				if (!grpcClientSynchronizer.WaitForServerReadySignal(task, this, process, TimeSpan.FromSeconds(timeout)))
				{
					if (!ProcessRunning)
					{
						ReportProcessFailedToRun(task, "The process terminated", LogLevel.Warning);
					}
					else
					{
						ReportProcessFailedToRun(task, $"No ready signal, killing runner ({task.Code}, {this}) in background.", LogLevel.Warning);
						KillInBackground();
					}

					return false;
				}

				CreateRunnerCommandQueue();
				return true;
			}
		}

		void CreateRunnerCommandQueue()
		{
			if (disposed || responseTaskCancellationTokenSource.Token.IsCancellationRequested)
			{
				return;
			}

			taskRunnerCommandQueueProvider = CreateRunnerCommandQueueProxy();
			taskRunnerCommandQueueProvider.StartResponseTask(ProcessResponseMessage, responseTaskCancellationTokenSource.Token);
		}

#if DEBUG
		protected virtual
#endif
			IRunnerCommandQueueProvider CreateRunnerCommandQueueProxy()
		{
			return remotingServices.CreateRunnerCommandQueueProxy(GrpcPort);
		}

		void ReportProcessFailedToRun(IRunnableServiceTask task, string message, LogLevel logLevel)
		{
			HostLogger.Log(logLevel, task?.Info?.HostedServiceAttribute, Invariant($"Failed to run {task?.Code} - {task?.Info?.HostedServiceAttribute.Description}, {ProcessInfo}. {message}."));
			HostLogger.Log(logLevel, task?.Info?.HostedServiceAttribute, "Please check that service task runs correctly, and restart Service Tasks Process Controller if needed.");
		}

		protected virtual bool SendCommand(Action<IRunnerCommandQueueProvider> command, ITaskRunRequest runRequest = null)
		{
			try
			{
				command(taskRunnerCommandQueueProvider);
				return true;
			}
			catch (HostGrpcIsClosedException)
			{
				runRequest?.OnUnableToRun(UnableToRunReason.RunnerIsInProcessOfShuttingDown, true, true);
				return true;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				HandleCommandException(ex, runRequest);
			}

			return false;
		}

		void HandleCommandException(Exception ex, ITaskRunRequest runRequest = null)
		{
			HostLogger.Log(LogLevel.Error, runRequest?.Task?.Info?.HostedServiceAttribute, "Command failed", ex);
			KillInBackground();
		}

		public int? GrpcPort { get; private set; }

		public bool PortOpened => GrpcPort != null;

		IRunnerCommandQueueProvider taskRunnerCommandQueueProvider;
		readonly IGrpcClientSynchronizerFactory grpcClientSynchronizerFactory;
		readonly IProcessRunnerRemotingServices remotingServices;
		public GrpcEventHandleNames eventHandleNames;
		bool disposed;
		readonly CancellationTokenSource responseTaskCancellationTokenSource;
		readonly IErrorReporterProxy errorReporterProxy;
		readonly IProductRegistration productRegistration;

		protected override void Dispose(bool disposing)
		{
			// Only do something if we're not already disposed
			if (!disposed)
			{
				if (disposing)
				{
					responseTaskCancellationTokenSource?.Cancel();
					taskRunnerCommandQueueProvider?.Dispose();
					responseTaskCancellationTokenSource?.Dispose();
				}
			}
			disposed = true;
			base.Dispose(disposing);
		}

		protected override void OutputDataReceived(object sender, DataReceivedEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Data))
			{
				return;
			}

			var information = e.Data.Split(':');
			var messageInfo = information.Length > 0 ? information[0] : string.Empty;
			if (messageInfo.Equals(ServiceManagerHelper.RunnerToHostGrpcPortLockAquiredCommandPrefix, StringComparison.OrdinalIgnoreCase))
			{
				GrpcPort = information.Length > 1
					? int.Parse(information[1])
					: throw new HostGrpcInitializationException();

				return;
			}

			ActionQueue.Enqueue(() =>
			{
				if (!string.IsNullOrEmpty(e.Data))
				{
					var taskCode = information.Length > 1 ? information[1] : string.Empty;
					if (messageInfo.Equals(ServiceManagerHelper.RunnerToHostCommunicationServiceTaskErrorCommandPrefix, StringComparison.OrdinalIgnoreCase))
					{
						TaskScheduler.TrackServiceTaskError(taskCode);
					}
				}

				base.OutputDataReceived(sender, e);
			});
		}

#if DEBUG
		protected virtual
#endif
		void ProcessResponseMessage(ServiceTaskRunResponse response)
		{
			LastSeenNotIdle = dateTimeProvider.CurrentDateTimeUtc;
			var capturedTaskRunRequest = TaskRunRequest;
			Task grpcCloseTask = null;
			if (response.Status == Status.RunnerExiting)
			{
				// procedure to close grpc stream must be done in a timely manner, we cannot wait for the background thread action queue
				// offload to task to not block further processing
				grpcCloseTask = Task.Run(() => taskRunnerCommandQueueProvider.Close());
			}

			switch (response.Status)
			{
				case Status.Queued:
					ActionQueue.Enqueue(() => capturedTaskRunRequest?.OnQueuedResponse(this));
					break;
				case Status.ProcessStarted:
					break;
				case Status.ProcessFinished:
					if (response.HasCommand)
					{
						ExpectingTaskRunRequestCallback = false;
						switch (response.Command)
						{
							case ResponseCommandType.Idle:
								ActionQueue.Enqueue(() => capturedTaskRunRequest?.OnSuccessfulRun());
								break;
							case ResponseCommandType.Reenqueue:
								ActionQueue.Enqueue(() => ReQueueRequest((x) => new DirectTaskRunRequest(x), true));
								break;
							case ResponseCommandType.ScheduleNextRunTime:
								ActionQueue.Enqueue(() => ReQueueRequest((x) => new ScheduledTaskRunRequest(x), false));
								break;
							case ResponseCommandType.RecordServiceTaskError:
								throw new NotImplementedException("Requires Changes to ServiceTaskErrorTracker and LoggerNLogWrapper");
						}

						InvokeTaskRunRequestCompleted();
					}

					break;
				case Status.RunnerExiting:
					if (response.HasFailureReason && ExpectingTaskRunRequestCallback)
					{
						ExpectingTaskRunRequestCallback = false;
						ActionQueue.Enqueue(() => capturedTaskRunRequest.OnUnableToRun(ConvertFromFailureReasonType(response.FailureReason), true, true));
						InvokeTaskRunRequestCompleted();
					}

					ActionQueue.Enqueue(() => grpcCloseTask?.Wait()); // Ensure grpc closure stream is returned to the thread pool
					break;
			}

			static UnableToRunReason ConvertFromFailureReasonType(FailureReasonType? failureReason)
			{
				switch (failureReason)
				{
					case FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock:
						return UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToServiceTaskLock;
					case FailureReasonType.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock:
						return UnableToRunReason.RunnerCouldNotObtainLockForSingleInstanceTaskDueToMutualGroupLock;
					case FailureReasonType.RunnerWasCancelled:
						return UnableToRunReason.RunnerWasCancelled;
					default:
						throw new ArgumentOutOfRangeException(nameof(failureReason));
				}
			}

			void ReQueueRequest<T>(Func<IRunnableServiceTask, T> reconstructRequestFunc, bool isNudgeRequest)
				where T : class, ITaskRunRequest
			{
				if (!response.TaskId.Equals(capturedTaskRunRequest?.Id.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					TaskScheduler
						.ReconstructRequest(reconstructRequestFunc, response.TaskCode)
						?.OnUnableToRun(ConvertFromFailureReasonType(response.FailureReason), true, true);
					errorReporterProxy.ReportOnce("ProcessServiceRunnerLostOriginalRequest", $"Task: {response.TaskCode}, IsNudgeRequest: {isNudgeRequest}");
				}
				else
				{
					capturedTaskRunRequest.OnUnableToRun(ConvertFromFailureReasonType(response.FailureReason), true, true);
				}
			}
		}
	}
}
