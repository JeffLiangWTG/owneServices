using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	public class WorkflowTriggerAction : IWorkflowTriggerAction
	{
		public WorkflowTriggerAction(IBaseTrigger trigger, ProcessTaskNotification action, IQueuedLog log, WorkflowDescriptor descriptor, IProcessor processor)
		{
			Trigger = Argument.NotNull(trigger, nameof(trigger));
			Action = Argument.NotNull(action, nameof(action));
			QueuedLog = Argument.NotNull(log, nameof(log));
			Descriptor = Argument.NotNull(descriptor, nameof(descriptor));
			Processor = Argument.NotNull(processor, nameof(processor));
		}

		public IBaseTrigger Trigger { get; }

		public ProcessTaskNotification Action { get; }

		public IQueuedLog QueuedLog { get; }

		public WorkflowDescriptor Descriptor { get; }

		public IProcessor Processor { get; }
	}
}
