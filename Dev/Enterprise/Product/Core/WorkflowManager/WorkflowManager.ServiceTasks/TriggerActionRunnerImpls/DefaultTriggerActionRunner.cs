using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	class DefaultTriggerActionRunner : IWorkflowTriggerActionRunner
	{
		public INotifications Notifications { get; set; }

		public void Run(IWorkflowTriggerAction workflowTriggerAction)
		{
			var action = workflowTriggerAction.Action;
			var trigger = workflowTriggerAction.Trigger;
			var processor = workflowTriggerAction.Processor;
			if (processor is IMessageProcessor messageProcessor)
			{
				var triggerName = trigger.WorkflowItemType.Equals("MIL") ? (NoResString)"milestone" : (NoResString)"trigger";
				var destinations = messageProcessor.GetDestinations();
				if (destinations == null)
				{
					ErrorReporter.ReportOnce(FormattableString.Invariant($"Destinations for {action.PQ_TriggerType} was null."));
				}
				else if (!destinations.Destinations.Any())
				{
					var warning = FormattableString.Invariant($"Action [Type={action.PQ_TriggerType},Recipient={action.PQ_Calc_TriggerParty},Purpose={action.PQ_MessagePurpose}] failed for {triggerName} [{trigger.TriggerEventCode}-{trigger.Description}] because {destinations.ConfigurationLogging.ToString("EN")}");
					Notifications.AddWarning(warning);
				}
				else
				{
					if (destinations.ConfigurationLogging != null)
					{
						var information = FormattableString.Invariant($"Action [Type={action.PQ_TriggerType},Recipient={action.PQ_Calc_TriggerParty},Purpose={action.PQ_MessagePurpose}] starting for {triggerName} [{trigger.TriggerEventCode}-{trigger.Description}] with {destinations.ConfigurationLogging.ToString("EN")}");
						Notifications.Add(new InfoNotification(information));
					}
					processor.Process(Notifications, CancellationToken.None);
				}
			}
			else
			{
				processor.Process(Notifications, CancellationToken.None);
			}
		}
	}
}
