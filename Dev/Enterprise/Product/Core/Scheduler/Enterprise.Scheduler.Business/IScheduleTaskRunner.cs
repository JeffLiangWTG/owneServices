using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Types;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.Scheduler.Business
{
	public interface IScheduleTaskRunner
	{
		void Process(ZString parentTableCode, bool useStmReportRun, INotifications notifications, CancellationToken token);
		void ProcessWithoutLock(ZString parentTableCode, bool useStmReportRun, INotifications notifications, CancellationToken token);
		QueueResult GetPendingJobsQueue(ZString parentTableCode);
	}
}
