using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Common;

namespace CargoWise.Data.SqlServer
{
	public class AttemptInGettingBacklog
	{
		public AttemptInGettingBacklog(bool success, string failureReason, BacklogResult backlogResult)
		{
			if (!(success && backlogResult != null || !success && !string.IsNullOrWhiteSpace(failureReason)))
			{
				throw new ArgumentException("Invalid argument.", nameof(success));
			}

			this.success = success;
			this.failureReason = failureReason;
			this.backlogResult = backlogResult;
		}

		readonly bool success;
		readonly string failureReason;
		readonly BacklogResult backlogResult;

		public bool Success { get { return success; } }
		public string FailureReason { get { return failureReason; } }
		public BacklogResult BacklogResult { get { return backlogResult; } }
	}

	public class BacklogResult
	{
		public Int64 BacklogSize;
		public string BacklogDescription;
	}

	public interface IBacklogWaiter
	{
		TimeSpan WaitUntilBacklogIsAcceptable(BacklogWaiter.OnWaitSingleItem callbackOnDelay = null);
		TimeSpan WaitUntilBacklogIsAcceptable(TimeSpan maxWaitTime, BacklogWaiter.OnWaitSingleItem callbackOnDelay = null, BacklogWaiter.OnTimeoutExpired callbackOnTimeoutExpired = null);
	}

	public class BacklogWaiter : IBacklogWaiter
	{
		public delegate void OnWaitSingleItem(TimeSpan elapsedTimeSpan, TimeSpan nextWaiTimeSpan, bool attemptSuccess, string attemptFailureReason, BacklogResult attemptBackupResult);
		public delegate void OnTimeoutExpired(TimeSpan elapsedTimeSpan, string timeoutMessage);

		readonly IBacklogInfoProvider[] strategies;

		public BacklogWaiter(IBacklogInfoProvider[] strategies)
		{
			Argument.NotNull(strategies, nameof(strategies));
			if (!strategies.All(s => s != null))
			{
				throw new ArgumentException("Invalid argument.", nameof(strategies));
			}

			this.strategies = strategies;
		}

		public TimeSpan WaitUntilBacklogIsAcceptable(OnWaitSingleItem callbackOnDelay = null)
		{
			return WaitUntilBacklogIsAcceptable(TimeSpan.MaxValue, callbackOnDelay);
		}

		public TimeSpan WaitUntilBacklogIsAcceptable(TimeSpan maxWaitTime, OnWaitSingleItem callbackOnDelay = null, OnTimeoutExpired callbackOnTimeoutExpired = null)
		{
			var totalWaitForAllItems = TimeSpan.Zero;
			var remainingWaitTime = maxWaitTime;
			TimeSpan lastRunWaitTime;
			var backlogMessages = new string[strategies.Length];

			do
			{
				lastRunWaitTime = TimeSpan.Zero;
				for (var i = 0; i < strategies.Length; i++)
				{
					var strategy = strategies[i];
					var elapsedTime = TimeSpan.Zero;
					AttemptInGettingBacklog lastBackLogResult = null;
					if (strategy.Timespans.Length > 0)
					{
						var waitResult = WaitOnSingleItem(strategy, callbackOnDelay, remainingWaitTime);
						elapsedTime = waitResult.ElapsedTime;
						lastBackLogResult = waitResult.LastBackLogResult;
						lastRunWaitTime += elapsedTime;

						totalWaitForAllItems += lastRunWaitTime;
						remainingWaitTime -= lastRunWaitTime;
					}

					if (lastBackLogResult == null || !lastBackLogResult.Success || lastBackLogResult.BacklogResult.BacklogSize > strategy.AcceptableBacklog)
					{
						backlogMessages[i] = GenerateProviderWaitResultMessage(lastBackLogResult, strategy);
					}

					if (remainingWaitTime <= TimeSpan.Zero)
					{
						var timeoutInfoMessage = string.Join("", backlogMessages.Where(s => !string.IsNullOrEmpty(s)));
						callbackOnTimeoutExpired?.Invoke(totalWaitForAllItems, timeoutInfoMessage);
						break;
					}
				}
			}
			while (lastRunWaitTime != TimeSpan.Zero && remainingWaitTime > TimeSpan.Zero);

			return totalWaitForAllItems;

			string GenerateProviderWaitResultMessage(AttemptInGettingBacklog backlogResult, IBacklogInfoProvider provider)
			{
				var waitResult = backlogResult != null ?
					backlogResult.Success ?
						"Success"
						: "Failure"
					: "No result";
				var backlogSizeInfo = backlogResult != null ? $"{backlogResult.BacklogResult.BacklogSize}" : "(unavailable)";
				return $"{provider.GetType().Name} check backlog result: {waitResult}, backlog size: {backlogSizeInfo}, acceptable level: {provider.AcceptableBacklog}, failed reason: {backlogResult?.FailureReason}";
			}
		}

		readonly ConcurrentDictionary<IBacklogInfoProvider, int> timespanPosition = new ConcurrentDictionary<IBacklogInfoProvider, int>();

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Not critical to overwrite the wait index. Just a smidge less efficient on the next wait pass.")]
		(TimeSpan ElapsedTime, AttemptInGettingBacklog LastBackLogResult) WaitOnSingleItem(IBacklogInfoProvider strategyItem, OnWaitSingleItem callbackOnDelay, TimeSpan maxWaitTime)
		{
			Argument.NotNull(strategyItem, nameof(strategyItem));

			var lastTimespan = timespanPosition.GetOrAdd(strategyItem, 0);
			var currentTimeSpan = ZMath.Clamp(lastTimespan, 0, strategyItem.Timespans.Length - 1);
			var sw = new Stopwatch();
			AttemptInGettingBacklog attempt = null;

			while (sw.Elapsed < maxWaitTime)
			{
				attempt = strategyItem.GetCurrentBacklog();

				if (attempt.Success && attempt.BacklogResult.BacklogSize <= strategyItem.AcceptableBacklog)
				{
					break; //backlog size has been brought down to an acceptable level
				}

				var waitTimespan = strategyItem.Timespans[currentTimeSpan];
				if (sw.Elapsed + waitTimespan > maxWaitTime)
				{
					waitTimespan = maxWaitTime - sw.Elapsed;
				}
				callbackOnDelay?.Invoke(sw.Elapsed, waitTimespan, attempt.Success, attempt.FailureReason, attempt.BacklogResult);

				sw.Start();
				Thread.Sleep(waitTimespan);
				currentTimeSpan = Math.Min(currentTimeSpan + 1, strategyItem.Timespans.Length - 1);
			}

			timespanPosition[strategyItem] = Math.Max(currentTimeSpan - 1, 0);

			return (sw.Elapsed, attempt);
		}
	}
}
