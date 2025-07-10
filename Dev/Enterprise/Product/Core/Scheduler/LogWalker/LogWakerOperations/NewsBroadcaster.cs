using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.LogWalker.Internals;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.LogWalker
{
	class NewsBroadcaster : ILogWalkerOperation
	{
		void ILogWalkerOperation.Execute(SubscriberParameters subscriberParameters, LogSubscriber[] allSubscribers, CancellationToken token)
		{
			ProcessLogs(subscriberParameters, allSubscribers, token);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		LogSubscriber[] GetSubscribersWithQueuedLogs(LogSubscriber[] allLogSubscribers)
		{
			using (var command = Db.Connection.Command("GetSubscribersWithQueuedLogs"))
			{
				command.CommandType = CommandType.StoredProcedure;
				var queryResult = (string)(command.ExecuteScalar());
				var validSubscribers = new HashSet<string>(queryResult.Split('|'), StringComparer.OrdinalIgnoreCase);
				var result = allLogSubscribers.Where(s => validSubscribers.Contains(s.Name)).ToArray();
				return result;
			}
		}

		public void ProcessLogs(SubscriberParameters subscriberParameters, LogSubscriber[] allSubscribers, CancellationToken token)
		{
			var subscribersWithQueuedLogs = GetSubscribersWithQueuedLogs(allSubscribers);
			allSubscribers.ForEach((subscriber) => subscriber.SetDefaultLogger(subscriberParameters.Logger));

			var subscriberQueue = new Queue<LogSubscriber>(subscribersWithQueuedLogs);
			var stopWatch = new LogWalkerStopWatch();
			while (stopWatch.CanRunNextSubscriber(subscriberParameters.Logger) && subscriberQueue.Any())
			{
				token.ThrowIfCancellationRequested();

				var subscriber = subscriberQueue.Dequeue();
				var logResult = CallQueuedLogsProcessor(subscriber, allSubscribers, subscriberParameters);

				if (int.TryParse(Db.Connection.ExecuteScalar("SELECT @@TRANCOUNT")?.ToString(), out var transactionCount) && transactionCount > (Globals.IsTest ? 1 : 0))
				{
					ErrorReporter.ReportOnce("LogWalker.SubscriberTransactionLeak", $"Subscriber {subscriber.FriendlyName}, [{subscriber.Name}] has leaked an open transaction.");
					((IDbReconnectionHandling)Db.Connection).CloseAndReopenConnection(); // We need to carry on processing subscribers or progress could be indefinitely blocked.
				}

				switch (logResult.Request)
				{
					case LogSubscriberProcessRequest.Requeue:
						subscriberQueue.Enqueue(subscriber); // The subscriber hasn't done enough work yet, but it is temporarily yielding to other subscribers.
						break;
					case LogSubscriberProcessRequest.Yield:
						break; // There is no work left to do here.
					case LogSubscriberProcessRequest.Abort:
						return; // Something terrible has happened and it is time to close the service task.
					default:
						throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognized: {0}", logResult.Request));
				}
			}
		}

		protected virtual LogSubscriberProcessResult CallQueuedLogsProcessor(LogSubscriber subscriber, LogSubscriber[] allSubscribers, SubscriberParameters subscriberParameters)
		{
			return GetNewNewsTransmitter(subscriber,  allSubscribers, subscriberParameters).ProcessLogQueueBatch(SystemDataRegistry.Instance.LogWalkerBatchSize.Value);
		}

		protected virtual NewsTransmitter GetNewNewsTransmitter(LogSubscriber subscriber, LogSubscriber[] allSubscribers, SubscriberParameters subscriberParameters)
		{
			return new NewsTransmitter(subscriber, allSubscribers, subscriberParameters);
		}
	}

	[DebuggerDisplay("ItemsFound = {ItemsFound}")]
	public class LogSubscriberProcessResult
	{
		public int ItemsFound { get; set; }

		public LogSubscriberProcessRequest Request { get; set; }
	}

	public enum LogSubscriberProcessRequest
	{
		Yield = 0,
		Requeue,
		Abort,
	}
}
