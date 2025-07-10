using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.UniversalDataBuss.ServiceTasks.Processing;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(UMIServiceTaskWorker.CODE, "UMQ Queued Service Task", typeof(QueuedStmQueueStateQueueProvider))]
namespace Enterprise.UniversalDataBuss.ServiceTasks.Processing
{
	public class QueuedStmQueueStateQueueProvider : StmQueueStateQueueProvider
	{
		public QueuedStmQueueStateQueueProvider() : base(QueueStatusCodes.Codes.Queued)
		{
		}
	}
}
