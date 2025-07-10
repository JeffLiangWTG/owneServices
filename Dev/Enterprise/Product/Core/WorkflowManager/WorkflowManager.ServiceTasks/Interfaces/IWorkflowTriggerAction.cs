using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public interface IWorkflowTriggerAction
	{
		IBaseTrigger Trigger { get; }
		ProcessTaskNotification Action { get; }
		IQueuedLog QueuedLog { get; }
		WorkflowDescriptor Descriptor { get; }
		IProcessor Processor { get; }
	}
}
