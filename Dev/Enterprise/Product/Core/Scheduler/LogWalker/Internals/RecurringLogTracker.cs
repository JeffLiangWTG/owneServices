using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.LogWalker
{
	class RecurringLogTracker
	{
		internal RecurringLogTracker()
		{
			logsThatHaveBeenSeenBeforeInThisRun = new Dictionary<EventLogKey, int>();
		}

		internal RecurringLogTracker(RecurringLogTracker outer)
		{
			logsThatHaveBeenSeenBeforeInThisRun = new Dictionary<EventLogKey, int>(outer.logsThatHaveBeenSeenBeforeInThisRun);
		}

		readonly Dictionary<EventLogKey, int> logsThatHaveBeenSeenBeforeInThisRun;

		public void TrackLogs(IEnumerable<IQueuedLog> queuedLogs) => TrackLogs(queuedLogs.Select(q => new EventLogKey(q)));

		void TrackLogs(IEnumerable<EventLogKey> logs)
		{
			foreach (var key in logs)
			{
				if (!logsThatHaveBeenSeenBeforeInThisRun.ContainsKey(key))
				{
					logsThatHaveBeenSeenBeforeInThisRun[key] = 0;
				}
				else
				{
					logsThatHaveBeenSeenBeforeInThisRun[key]++;
				}
			}
		}

		internal RecurringLogsResult GetRecurringLogs(BusinessObjectFactory factory)
		{
			var newLogCandidates = GetNewLogs(factory);
			var duplicateLogs = new List<StmALog>();
			var newLogs = new List<StmALog>();

			foreach (var logsByKey in newLogCandidates.GroupBy(l => new EventLogKey(l)))
			{
				var key = logsByKey.Key;
				if (logsThatHaveBeenSeenBeforeInThisRun.ContainsKey(key))
				{
					duplicateLogs.AddRange(logsByKey);
				}
				else
				{
					newLogs.AddRange(logsByKey);
				}
			}

			return new RecurringLogsResult(newLogs.AsReadOnly(), duplicateLogs.AsReadOnly());
		}

		StmALog[] GetNewLogs(BusinessObjectFactory factory)
		{
			return factory.Load<StmALog>(new ZQuery { FetchOnlyFromLocalCache = true }).Where(s => !s.IsInDatabase && !s.IsDeleted).ToArray();
		}

		internal class RecurringLogsResult
		{
			public RecurringLogsResult(IReadOnlyCollection<StmALog> newLogs, IReadOnlyCollection<StmALog> duplicateLogs)
			{
				NewLogs = newLogs;
				DuplicateLogs = duplicateLogs;
			}

			public IReadOnlyCollection<StmALog> NewLogs { get; }
			public IReadOnlyCollection<StmALog> DuplicateLogs { get; }
		}
	}
}
