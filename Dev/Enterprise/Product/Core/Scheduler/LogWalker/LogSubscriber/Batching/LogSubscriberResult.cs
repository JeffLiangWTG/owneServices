using System.Collections.Generic;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.LogWalker
{
	public sealed class LogSubscriberResult
	{
		#region Constructors and state

		public LogSubscriberResult(ICollection<IQueuedLog> processedLogs, ICollection<IQueuedLog> unprocessedLogs)
		{
			this.processedLogs = processedLogs;
			this.unprocessedLogs = unprocessedLogs;
		}

		readonly ICollection<IQueuedLog> processedLogs;
		readonly ICollection<IQueuedLog> unprocessedLogs;

		#endregion

		#region Properties

		public ICollection<IQueuedLog> ProcessedLogs => processedLogs;
		public ICollection<IQueuedLog> UnprocessedLogs => unprocessedLogs;

		#endregion
	}
}
