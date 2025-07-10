using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public abstract class LoggingMatchResult<T> : ISimpleLogger where T : IBusiness
	{
		protected LoggingMatchResult()
		{
			this.logs = new List<SimpleLog>();
		}

		readonly List<SimpleLog> logs;

		public bool Success
		{
			get;
			private set;
		}

		public T MatchFound
		{
			get;
			private set;
		}

		protected LoggingMatchResult<T> SetMatch(T matchFound)
		{
			this.Success = true;
			this.MatchFound = matchFound;
			return this;
		}

		public void Log(LogType type, string message)
		{
			logs.Add(new SimpleLog(type, message));
		}

		public void WriteLogs(ISimpleLogger logger)
		{
			logs.ForEach(log => logger.Log(log.Type, log.Message));
		}

		#region GetMatchingLogsForTesting
#if DEBUG
		public string[] GetMatchingLogsForTesting()
		{
			return logs
				.Select(log => (log.Type == LogType.Information ? "" : log.Type.ToString() + " - ") + log.Message)
				.ToArray();
		}
#endif
		#endregion

		#region ISimpleLogResult Members

		IEnumerable<ISimpleLog> ISimpleLogResult.Logs
		{
			get { return logs; }
		}

		#endregion
	}
}
