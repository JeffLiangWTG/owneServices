using System;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class TaskRunner : ITaskRunRequestProcessor
	{
		public TaskRunner(
			IProcessRunnerPool processRunnerPool,
			IHostLogger hostLogger)
		{
			this.processRunnerPool = processRunnerPool;
			this.hostLogger = hostLogger;
		}

		public TaskRunRequestResult ProcessRunRequest(ITaskRunRequest request, IServiceRunner runner)
		{
			_ = request ?? throw new ArgumentNullException(nameof(request));
			_ = runner ?? throw new ArgumentNullException(nameof(runner));

			var task = request.Task;

			if (task.Info != null)
			{
				task.Info.ErrorOnLastRun = false;
			}

			// ready to run, let's do some basic checks
			if (string.IsNullOrEmpty(task.Info.HostedServiceAttribute.TypeName))
			{
				task.RecordLastRunError();
				hostLogger.Log(LogLevel.Error, request.FormatRequestToLogMessage(LogMessageStage.TypeNameIsEmpty));
				return TaskRunRequestResult.ConfigurationError;
			}

			if (!task.IsActive)
			{
				hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.StoppingAccociatedRunners));
				processRunnerPool.StopAllRunners(task);
			}

			if (request is IDirectTaskRunRequest directRequest && !directRequest.HasRunsRemaining)
			{
				hostLogger.Log(LogLevel.Debug, request.FormatRequestToLogMessage(LogMessageStage.NoRunsAttemptsRemaining));
				return TaskRunRequestResult.TaskAlreadyCompleted;
			}

			using (hostLogger.LogSection(
				LogLevel.Debug,
				request.FormatRequestToLogMessage(LogMessageStage.ValidatingRequest),
				request.FormatRequestToLogMessage(LogMessageStage.ValidatedRequest)))
			{
				var validateForRunResult = task.ValidateForRun();
				if (validateForRunResult != TaskRunRequestResult.Success)
				{
					return validateForRunResult;
				}
			}

			task.TimeSinceLastStarted.Restart();
			if (processRunnerPool.RunningCount(task) == 0)
			{
				task.TimeRunning.Restart();
			}

			return runner.Run(request) ? TaskRunRequestResult.Success : TaskRunRequestResult.ProcessDidNotStart;
		}

		readonly IProcessRunnerPool processRunnerPool;
		readonly IHostLogger hostLogger;
	}
}

