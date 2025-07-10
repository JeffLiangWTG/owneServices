using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class GrpcRunner : TaskRunner
	{
		public GrpcRunner(
			IServiceTaskRunnerQueueService serviceTaskRunnerQueueService,
			IRunnerLogger logger,
			string grpcGuid,
			IClientHostedServiceAttributeProvider hostedServiceAttributeProvider,
			IHostCommunicationStrategy hostCommunicationStrategy,
			ICommandExecutionStrategy commandExecutionStrategy,
			IRunnerRegistrySettings runnerRegistry,
			INudgingController nudgingController)
			: base(
				logger,
				hostedServiceAttributeProvider)
		{
			GrpcGuid = grpcGuid;
			this.serviceTaskRunnerQueueService = serviceTaskRunnerQueueService ?? throw new ArgumentNullException(nameof(serviceTaskRunnerQueueService));
			this.hostCommunicationStrategy = hostCommunicationStrategy ?? throw new ArgumentNullException(nameof(hostCommunicationStrategy));
			this.commandExecutionStrategy = commandExecutionStrategy ?? throw new ArgumentNullException(nameof(commandExecutionStrategy));
			this.runnerRegistry = runnerRegistry ?? throw new ArgumentNullException(nameof(runnerRegistry));
			this.nudgingController = nudgingController ?? throw new ArgumentNullException(nameof(nudgingController));
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Expected until refactored")]
		[SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "This is logging so want real machine time.")]
		protected override RunnerExitCode RunInternal(bool singleRun)
		{
			using (var queue = serviceTaskRunnerQueueService.Start(GrpcGuid))
			using (var cancellationTokenSource = new CancellationTokenSource(runnerRegistry.ServiceTaskUnloadTimeout))
			{
				RegisterNudgeSubscriptions();
				try
				{
					// de-initialize proxy, we don't need it yet and proxy initialization will occur once later on by first serviceTask run initialization
					var quit = false;
					var cancelled = false;
					WebRequest.DefaultWebProxy = null;
					var initialConnectionPoolingPoolingRegistryValue = runnerRegistry.ServiceTaskRunnerConnectionPoolingEnabled;
					var initialPriorityValue = runnerRegistry.RunnerProcessPriorityValue;

					while (!quit)
					{
						var commandInfo = queue.GetNextCommand();
						if (commandInfo != null)
						{
							System.Environment.SetEnvironmentVariable("ServiceTaskStatus", $"{commandInfo.GetType().Name} requested at {DateTime.Now.ToLongTimeString()}");
							Logger.Log(LogLevel.Debug, commandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.PreparingExecution));

							var executionResult = commandExecutionStrategy.Execute(commandInfo);

							switch (executionResult)
							{
								case ServiceTaskRunResult.Success:
									hostCommunicationStrategy.Completed(queue);
									Logger.Log(LogLevel.Debug, commandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.CompletedCommand));
									break;
								case ServiceTaskRunResult.Cancelled:
									cancelled = true;
									quit = true;
									break;
								case ServiceTaskRunResult.UnhandledException:
									Logger.Log(LogLevel.Debug, "Terminating runner due to unhandled exception in task", commandInfo);
									hostCommunicationStrategy.Completed(queue);
									queue.CloseStream(cancelled, cancellationTokenSource.Token);
									return RunnerExitCode.ServiceTaskUnhandledException;
								case ServiceTaskRunResult.ServiceTaskLockNotAcquired:
									hostCommunicationStrategy.ServiceTaskLockNotAcquired(queue, commandInfo);
									break;
								case ServiceTaskRunResult.GroupLockNotAcquired:
									hostCommunicationStrategy.GroupLockNotAcquired(queue, commandInfo);
									break;
								case ServiceTaskRunResult.LockNotReleased:
									Logger.Log(LogLevel.Warning, commandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.ServiceTaskLockNotReleased, GetTaskCode(commandInfo), commandInfo.Id));
									queue.CloseStream(cancelled, cancellationTokenSource.Token);
									return RunnerExitCode.ServiceTaskLockNotReleased;
								default:
									hostCommunicationStrategy.Completed(queue);
									Logger.Log(LogLevel.Warning, commandInfo.FormatRequestToLogMessage(RunnerLogMessageStage.CorruptedEnvironment));
									queue.CloseStream(cancelled, cancellationTokenSource.Token);
									return RunnerExitCode.ServiceTaskCorruptedTheEnvironment;
							}
						}
						else if (singleRun) // in case of singlerun we run all the commands in the queue but dont wait for new ones.
						{
							Logger.Log(LogLevel.Debug, "Quit: single run completed.");
							quit = true;
						}
						else
						{
							Logger.Log(LogLevel.Error, "Quit: no commands were received for twice the unload timeout.");
							quit = true;
						}

						if (initialConnectionPoolingPoolingRegistryValue != runnerRegistry.ServiceTaskRunnerConnectionPoolingEnabled)
						{
							Logger.Log(LogLevel.Debug, $"Quit: connection pooling enabled value changed to [{runnerRegistry.ServiceTaskRunnerConnectionPoolingEnabled}].");
							quit = true;
						}

						if (initialPriorityValue != runnerRegistry.RunnerProcessPriorityValue)
						{
							Logger.Log(LogLevel.Debug, $"Quit: Process priority value changed from [{initialPriorityValue}] to [{runnerRegistry.RunnerProcessPriorityValue}].");
							quit = true;
						}
					}
					queue.CloseStream(cancelled, cancellationTokenSource.Token);

					return RunnerExitCode.NoIssues;
				}
				finally
				{
					UnregisterNudgeSubscriptions();
				}

				static string GetTaskCode(ICommandInfo commandInfo)
				{
					if (commandInfo is DirectRunCommandInfo dInfo) { return dInfo.Code; }
					if (commandInfo is ScheduledRunCommandInfo sInfo) { return sInfo.Code; }
					throw new InvalidOperationException($"[{commandInfo.GetType()}] with id [{commandInfo.Id}] should not claim a service task lock but is failing to release one");
				}
			}
		}

		void OnNudgeEvent(object? o, NudgeEventArgs e) => Logger.Log(LogLevel.Debug, e.ToString());

		void RegisterNudgeSubscriptions()
		{
			nudgingController.NudgeFailedEvent += OnNudgeEvent;
			nudgingController.NudgeTrackingEvent += OnNudgeEvent;
		}

		void UnregisterNudgeSubscriptions()
		{
			nudgingController.NudgeFailedEvent -= OnNudgeEvent;
			nudgingController.NudgeTrackingEvent -= OnNudgeEvent;
		}

		private protected readonly IHostCommunicationStrategy hostCommunicationStrategy;
		readonly ICommandExecutionStrategy commandExecutionStrategy;
		public string GrpcGuid { get; }

		readonly IServiceTaskRunnerQueueService serviceTaskRunnerQueueService;
		readonly IRunnerRegistrySettings runnerRegistry;
		readonly INudgingController nudgingController;
	}
}
