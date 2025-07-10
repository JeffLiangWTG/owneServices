using Enterprise.Scheduler.GraphEngine;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.UniversalDataBuss.ServiceTasks.Processing;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceQueueProvider(UMIServiceTask.CODE, "UMI PreKey Service Task", typeof(PreKeyStmQueueStateQueueProvider))]
namespace Enterprise.UniversalDataBuss.ServiceTasks.Processing
{
	public class PreKeyStmQueueStateQueueProvider : StmQueueStateQueueProvider
	{
		public PreKeyStmQueueStateQueueProvider() : base(QueueStatusCodes.Codes.PreKey)
		{
		}
	}
}

