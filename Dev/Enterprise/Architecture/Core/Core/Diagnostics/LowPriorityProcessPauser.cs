using System;
using CargoWise.Data.SqlServer;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.ZArchitecture.Core
{
	public class LowPriorityProcessPauserFactory : ILowPriorityProcessPauserFactory
	{
		public ILowPriorityProcessPauser Create() => new LowPriorityProcessPauser();
		public ILowPriorityProcessPauser Create(IBacklogInfoProvider[] providers) => new LowPriorityProcessPauser(providers);
	}

	public class LowPriorityProcessPauser : ILowPriorityProcessPauser
	{
		public LowPriorityProcessPauser(string databaseName = "")
			: this(
				new BacklogWaiter(new IBacklogInfoProvider[]
					{
						new CdcLatencyProvider(),
						new LogFullnessProvider(databaseName),
						new AlwaysOnDelayProvider(databaseName),
					})
			)
		{
		}

		public LowPriorityProcessPauser(IBacklogInfoProvider[] providers)
			: this(new BacklogWaiter(providers))
		{
		}

		internal LowPriorityProcessPauser(IBacklogWaiter waiter)
		{
			this.waiter = waiter;
		}

		readonly IBacklogWaiter waiter;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="logger"></param>
		/// <returns>The total time waited for the replica backlog to return to acceptable levels</returns>
		public TimeSpan Wait(ILogger logger = null, TimeSpan? maxWaitTime = null, Action callbackOnDelay = null)
		{
			return waiter.WaitUntilBacklogIsAcceptable(
				maxWaitTime ?? TimeSpan.MaxValue,
				(elapsedTime, nextWaitTime, success, failureReason, backlogDetail) =>
			{
				callbackOnDelay?.Invoke();

				if (elapsedTime.TotalSeconds > 5)
				{
					logger?.Log(
						elapsedTime.TotalSeconds > 10 ? LogType.Information : LogType.Debug,
						FormattableString.Invariant($"Waiting for {(success ? backlogDetail.BacklogDescription : failureReason)} backlog to clear. Total wait time = {elapsedTime}, next wait time = {nextWaitTime}.")
					);
				}
			},
			(elapsedTime, timeoutMessage) =>
			{
				var errorMessage = FormattableString.Invariant($@"Gave up waiting. Total wait time = {elapsedTime}");
				logger?.Log(LogType.Warning, errorMessage);
				throw new BacklogWaiterTimeoutException(timeoutMessage);
			});
		}
	}
}
