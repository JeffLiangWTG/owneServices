using System.Linq;

namespace Enterprise.BufferManagement.Business
{
	public class ReleaseGateLoggerWithFailureServices : ReleaseGateLogger
	{
		public ReleaseGateLoggerWithFailureServices(ReleaseGateFailureLogService failureService)
		{
			this.failureService = failureService;
		}

		readonly ReleaseGateFailureLogService failureService;

		protected override void CommitReleaseLogsCore()
		{
			base.CommitReleaseLogsCore();

			CommitFailureReleaseLogs();
		}

		void CommitFailureReleaseLogs()
		{
			foreach (var logCache in ReleaseLogs.Where(kvp => kvp.Key.NoteType == BMConstants.LastReleaseFailureNoteType).ToArray())
			{
				failureService.QueueFailureLog(logCache.Key.WorkflowPK.ToGuid(), logCache.Value.GetLogs());
			}

			failureService.SubmitQueueToRemoteCache();
		}
	}
}
