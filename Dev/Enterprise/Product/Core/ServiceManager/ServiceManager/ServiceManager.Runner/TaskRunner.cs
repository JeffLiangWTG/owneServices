using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Data;
using ServiceManager.Common.Abstractions;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	public abstract class TaskRunner : ITaskRunner
	{
		protected TaskRunner(IRunnerLogger logger, IClientHostedServiceAttributeProvider hostedServiceAttributeProvider)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			HostedServiceAttributeProvider = hostedServiceAttributeProvider ?? throw new ArgumentNullException(nameof(hostedServiceAttributeProvider));
		}

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public RunnerExitCode Run(bool singleRun, IApplicationExceptionHandler exceptionHandler)
		{
			// Run the task runner in a separate thread to avoid the finalizer for COM objects getting stuck due to it being allocated on the main thread.
			var resultCode = RunnerExitCode.NoIssues;
			var runThread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					try
					{
						resultCode = RunInternal(singleRun);
					}
					catch (Exception ex)
					{
						exceptionHandler.HandleFromTask(ex, () => Logger);
						resultCode = RunnerExitCode.RunnerFailure;
					}
				}
			});
			runThread.Start();
			runThread.Join();
			return resultCode;
		}

		protected IRunnerLogger Logger { get; }
		protected IClientHostedServiceAttributeProvider HostedServiceAttributeProvider { get; }
		protected abstract RunnerExitCode RunInternal(bool singleRun);
	}
}
