using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.UniversalDataBuss.ServiceTasks.Processing;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(UMIServiceTask.CODE, "UMI Blocked Service Task", typeof(BlockedStmQueueStateQueueProvider))]
namespace Enterprise.UniversalDataBuss.ServiceTasks.Processing
{
	public class BlockedStmQueueStateQueueProvider : StmQueueStateQueueProvider
	{
		public BlockedStmQueueStateQueueProvider() : base(QueueStatusCodes.Codes.Blocked)
		{
		}
	}
}
