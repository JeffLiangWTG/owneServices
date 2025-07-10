using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.Integration;
using Enterprise.ServiceManager.Tasks.OnlineDataTransformation;
using Enterprise.ZArchitecture.Core;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	OnlineDataTransformationTask.ServiceTaskCode, "Online Data Transformation Service", "SYS",
	typeof(OnlineDataTransformationTask),
	IsMandatory = true,
	MinimumPeriod = "1minute",
	MaximumPeriod = "1day",
	AlwaysRunAtStartup = true,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "1minute",
	ActiveByDefault = true)
]

namespace Enterprise.ServiceManager.Tasks.OnlineDataTransformation
{
	class OnlineDataTransformationTask : ServiceProviderImpl
	{
		public const string ServiceTaskCode = "ODT";

		public OnlineDataTransformationTask()
			: this(null)
		{
		}

		internal OnlineDataTransformationTask(IOnlineTransformationProvider provider)
			: this(provider, TimeSpan.FromMinutes(10))
		{
		}

		internal OnlineDataTransformationTask(IOnlineTransformationProvider provider, TimeSpan maximumTimeToRunSingleTask)
		{
			this.provider = provider ?? new OnlineTransformationProvider();
			this.maximumTimeToRunSingleTask = maximumTimeToRunSingleTask;
		}

		readonly IOnlineTransformationProvider provider;
		readonly TimeSpan maximumTimeToRunSingleTask;

		public override void RunTask(CancellationToken token)
		{
			var runningTasks = provider.GetRunningTasks().ToList();
			if (runningTasks.Count > 0)
			{
				using (Db.Connection.TemporarySetDeadlockPriority(DeadlockPriority.Min))
				{
#if DEBUG
					RunUserAction_ForTest();
#endif

					var backlogWaiter = new LowPriorityProcessPauser();
					var exceptions = new List<Exception>();

					while (runningTasks.Count > 0)
					{
						foreach (var task in runningTasks.ToArray())
						{
							backlogWaiter.Wait(ServiceLogger);

							using (var taskCTS = CancellationTokenSource.CreateLinkedTokenSource(token))
							{
								taskCTS.CancelAfter(maximumTimeToRunSingleTask);
								try
								{
									ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "[{0}] - Start", task.UserDescription));

									task.Run(text => ServiceLogger.Log(LogType.Information, text), taskCTS.Token);
									provider.OnTaskCompleted(task);
									runningTasks.Remove(task);

									ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "[{0}] - Finish", task.UserDescription));
									ResetTryCount(task);
								}
								catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.DeadlockError && CanRequeue(task))
								{
									ServiceLogger.Information(FormattableString.Invariant($"[{task.UserDescription}] - Requeued due to deadlock"));
								}
								catch (SqlException ex) when (new DbErrorMatch(ex).ExceptionType == DbErrorType.LockTimeoutExpired && CanRequeue(task))
								{
									ServiceLogger.Information(FormattableString.Invariant($"[{task.UserDescription}] - Requeued due to lock timeout"));
								}
								catch (OperationCanceledException)
								{
									if (token.IsCancellationRequested)
									{
										ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "[{0}] - Yielded as service host requested STOP", task.UserDescription));
										return;
									}
									else
									{
										ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "[{0}] - Yielded to allow other tasks to complete", task.UserDescription));
									}
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									ServiceLogger.Log(LogType.Information, string.Format(CultureInfo.CurrentCulture, "[{0}] - {1}", task.UserDescription, ex.Message));
									exceptions.Add(new OnlineDataTransformationException(task.UserDescription, ex));
								}
							}
						}

						if (exceptions.Count > 0)
						{
							throw new AggregateException(exceptions);
						}

						SetNextScheduledRunTime(TimeSpan.FromMinutes(1));
					}
				}
			}
			else
			{
				ServiceLogger.Log(LogType.Information, "No online transformations to run");
				SetNextScheduledRunTime(TimeSpan.FromDays(1));
			}
		}

		#region Implementation

		int tryCountDueToException = 10;
		readonly Dictionary<string, int> tryCountDueToExceptionPerTask = new Dictionary<string, int>();

		void SetNextScheduledRunTime(TimeSpan timeUntilNextRun)
		{
			var nextRunTime = ZDateTimeOffset.UtcNow.Add(timeUntilNextRun).ToDateTimeOffsetSafe();
			ObjectFactory.Get<IServiceManagerGovernor>().SetServiceTaskNextRuntime(ServiceTaskCode, nextRunTime);
		}

		void ResetTryCount(IOnlineTransformation task)
		{
			if (tryCountDueToExceptionPerTask.ContainsKey(task.UserDescription))
			{
				tryCountDueToExceptionPerTask[task.UserDescription] = 0;
			}
		}

		bool CanRequeue(IOnlineTransformation task)
		{
			var newTryCount = tryCountDueToExceptionPerTask.TryGetValue(task.UserDescription, out var tries) ? tries + 1 : 1;
			tryCountDueToExceptionPerTask[task.UserDescription] = newTryCount;

			return newTryCount <= tryCountDueToException;
		}

		#endregion // Implementation

		#region Test
#if DEBUG
		internal Action UserAction_ForTest { get; set; }

		void RunUserAction_ForTest()
		{
			UserAction_ForTest?.Invoke();
		}

		internal IDisposable TemporarySetTryCountDueToException_ForTest(int newValue)
		{
			var valueBeforeChange = tryCountDueToException;
			tryCountDueToException = newValue;

			return new DisposableAction(() => tryCountDueToException = valueBeforeChange);
		}
#endif
		#endregion
	}
}
