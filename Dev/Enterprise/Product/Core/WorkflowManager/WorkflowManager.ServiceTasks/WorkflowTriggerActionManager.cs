using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.WorkflowManager.ServiceTasks
{
	internal class WorkflowTriggerActionManager
	{
		public WorkflowTriggerActionManager(INotifications logsCollector)
		{
			this.logsCollector = logsCollector;
		}
		readonly INotifications logsCollector;

		public void Run(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowTriggerActionManager.Run"))
			using (WorkflowTriggerActionTracker.TrackTriggerActions(trigger.Factory))
			{
				var reasonForDoNotTriggerAction = job is ITriggerActionProvider provider ? provider.ReasonForDoNotTriggerAction : ZString.Empty;
				if (reasonForDoNotTriggerAction.IsEmpty)
				{
					RunActions(job, trigger, queuedLog);
				}
				else if (logsCollector != null)
				{
					logsCollector.AddInfo(Res.GetString("B87F18FB-51D1-4E7A-B057-B04010DBEA9F", "Trigger action can not be performed because {0}", reasonForDoNotTriggerAction));
				}
			}
		}

		void RunActions(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog)
		{
			var eventGetter = Lazy.Create<IStmALog>(() =>
			{
				TriggeringLogFinder.TryFindLog(new WorkflowTriggerEventData(queuedLog), trigger, job, out var result, wteFallbackLog: queuedLog);
				return result;
			});

			var actions = GetActions(job, trigger, queuedLog, logsCollector, eventGetter);
			if (logsCollector != null)
			{
				var types = string.Join(", ", actions.Where(a => !string.IsNullOrEmpty(a.Action.PQ_TriggerType)).Select(a => a.Action.PQ_TriggerType).ToArray());
				if (!string.IsNullOrEmpty(types))
				{
					logsCollector.AddVerboseInfo(() => Res.GetString("7df9cec0-cd04-441f-a70c-3bba04ea1756", "Action types in this batch: {0}", types));
				}
				else
				{
					logsCollector.AddVerboseInfo(() => Res.GetString("7dd20eaf-ae31-4b33-a24a-622013faa33d", "No actions with Trigger Type found to run"));
				}
			}

			var validators = ObjectFactory.Get<IEnumerable>("WorkflowTriggerActionValidators").Cast<IWorkflowTriggerActionValidator>();

			foreach (var action in actions)
			{
				if (validators.Any(v => v.ShouldValidate(action.Trigger, action.Action, action.QueuedLog, eventGetter.Value, action.Descriptor) &&
					!v.IsValid(action.Trigger, action.Action, action.QueuedLog, eventGetter.Value, action.Descriptor, job, logsCollector)))
				{
					continue;
				}

				var isLazy = action.Processor is IWorkflowSetFieldProcessor;
				if (!isLazy)
				{
					WorkflowTriggerActionRunnerLogs.FiringAction(action.Action, logsCollector);
				}

				RunAction(action);

				if (!isLazy)
				{
					WorkflowTriggerActionRunnerLogs.ActionComplete(action.Action, logsCollector);
				}
			}

			WorkflowTriggerActionTracker.OnAllActionsRun(trigger.Factory);
		}

		#region Implementation

		IList<IWorkflowTriggerAction> GetActions(BusinessObject job, IWorkflowTrigger trigger, IQueuedLog queuedLog, INotifications logsCollector, Lazy<IStmALog> eventGetter)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("WorkflowTriggerActionManager.GetDistinctActions"))
			{
				var results = new List<IWorkflowTriggerAction>();
				if (IsTriggerValid(job, trigger, logsCollector, out var workflowDescriptor))
				{
					var triggerActions = trigger.CompletionTriggerActionsCollection();
					foreach (var action in GetWorkflowTriggerActions(job, trigger, triggerActions, queuedLog, workflowDescriptor, eventGetter))
					{
						var runLoc = ((ITriggerAction)action.Action).RunLocation;
						if (runLoc == TriggerRunLocation.Server)
						{
							results.Add(action);
						}
					}
				}

				return results;
			}
		}

		IEnumerable<IWorkflowTriggerAction> GetWorkflowTriggerActions(BusinessObject job, IBaseTrigger trigger, IEnumerable<ProcessTaskNotification> triggerActions, IQueuedLog queuedLog, WorkflowDescriptor workflowDescriptor, Lazy<IStmALog> eventGetter)
		{
			var ungroupedActions = new List<IWorkflowTriggerAction>();

			foreach (var action in triggerActions)
			{
				var source = new WorkflowTriggerActionSource(job, trigger, action, queuedLog, eventGetter);
				var processor = workflowDescriptor.GetWorkflowTriggerAction(source, queuedLog);
				if (processor == null)
				{
					if (WorkflowTriggerActionTypeConstants.IsValidActionType(action.PQ_TriggerType))
					{
						ErrorReporter.ReportOnce(FormattableString.Invariant($"NoProcessorForWorkflowType{action.Parent.WorkflowProcessType}"), FormattableString.Invariant($"There wasn't a valid processor for this trigger action of type {action.PQ_TriggerType} on trigger with Workflow Type {action.Parent.WorkflowProcessType}. This trigger action has not been fired. Details about this trigger:\r\n{action.Parent.GetDiagnosticLogInfo()}"));
					}
					else
					{
						var errorReporterLog = FormattableString.Invariant($"There wasn't a valid processor for this trigger action of type {action.PQ_TriggerType} on trigger with Workflow Type {action.Parent?.WorkflowProcessType}. This trigger action has not been fired. Details about this trigger:\r\n{action.Parent?.GetDiagnosticLogInfo()}");
						logsCollector.AddError(errorReporterLog);
					}
				}
				else
				{
					ungroupedActions.Add(new WorkflowTriggerAction(trigger, action, queuedLog, workflowDescriptor, processor));
				}
			}

			(ProcessTaskNotification TriggerAction, IEnumerable<Guid?> DestinationPK) GetGroup(IWorkflowTriggerAction action)
			{
				return (action.Action, (action.Processor as IMessageProcessor)?.GetDestinations().Destinations.Select(destination => destination.Organisation?.PK.ToGuid()).Distinct());
			}

			var result = new List<IWorkflowTriggerAction>();
			foreach (var group in ungroupedActions.GroupBy(GetGroup, new ProcessTriggerComparer()))
			{
				if (UniversalTypes.Contains((string)group.Key.TriggerAction.PQ_TriggerType) && group.First().Processor is IUniversalXmlWorkflowProcessor processor)
				{
					var first = group.First();
					result.Add(first);
					foreach (var action in group.Skip(1))
					{
						processor.AddAdditionalTriggerParty(action.Action.PQ_TriggerParty, action.Action.PQ_TriggerPartyService);
						logsCollector.AddInfo(Res.GetString("32df3a0a-d554-4f1a-b4ab-e36b63234dbf", "Merging action [{0}] with identical destination", action.Action.PQ_TriggerType));
					}
				}
				else
				{
					result.AddRange(group);
				}
			}

			return result;
		}

		[ThreadSafe]
		readonly static string[] UniversalTypes = new[]
		{
			WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML,
			WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML,
			WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML
		};

#if DEBUG
		public
#endif
		class ProcessTriggerComparer : EqualityComparer<(ProcessTaskNotification TriggerAction, IEnumerable<Guid?> DestinationPK)>
		{
			public override bool Equals((ProcessTaskNotification TriggerAction, IEnumerable<Guid?> DestinationPK) x, (ProcessTaskNotification TriggerAction, IEnumerable<Guid?> DestinationPK) y)
			{
				return x.TriggerAction.PQ_TriggerType == y.TriggerAction.PQ_TriggerType
						&& x.TriggerAction.PQ_MessagePurpose == y.TriggerAction.PQ_MessagePurpose
						&& x.TriggerAction.PQ_ECS_MessageDeliveryContextSelector == y.TriggerAction.PQ_ECS_MessageDeliveryContextSelector
						&& (x.DestinationPK?.ContainsSameElementsInAnyOrder(y.DestinationPK) ?? false);
			}

			public override int GetHashCode((ProcessTaskNotification TriggerAction, IEnumerable<Guid?> DestinationPK) obj)
			{
				return unchecked(obj.TriggerAction.PQ_TriggerType.GetHashCode() + obj.TriggerAction.PQ_MessagePurpose.GetHashCode() + obj.TriggerAction.PQ_ECS_MessageDeliveryContextSelector.GetHashCode());
			}
		}

		void RunAction(IWorkflowTriggerAction action)
		{
			try
			{
				using (PerformanceStatisticsCollector.StartMonitoring("WorkflowTriggerActionManager.RunAction"))
				{
					IWorkflowTriggerActionRunner actionRunner = GetActionRunner(action.Action);
					if (actionRunner != null)
					{
						var maxTime = WorkflowDataRegistry.Instance.MillisecondsBeforeLoggingTriggerActions.Value;
						using (new WorkflowChainManager(action.QueuedLog))
						using (maxTime > 0 ? new OperationTimer(maxTime, (long timeElapsed) => logsCollector.AddInfo($"Trigger Action time: {timeElapsed}ms - Action: {action.Action.PQ_TriggerType}")) : null)
						{
							actionRunner.Run(action);
						}
					}
				}
			}
			catch (Exception ex)
			{
				if (WorkflowFailureHandler.IsEmailSenderAddressInvalidException(ex))
				{
					WorkflowFailureHandler.HandleTriggerActionFailure(action.Action, ex, logsCollector);
					return;
				}

				if (ex.IsCriticalException() || ExceptionVisibilityAttribute.Evaluate(ex) != ExceptionVisibility.User)
				{
					throw;
				}
				WorkflowFailureHandler.HandleTriggerActionFailure(action.Action, ex);
			}
		}

		protected virtual IWorkflowTriggerActionRunner GetActionRunner(ProcessTaskNotification triggerAction)
		{
			return new DefaultTriggerActionRunner { Notifications = logsCollector };
		}

		static bool IsTriggerValid(BusinessObject workflowProvider, IWorkflowTrigger trigger, INotifications logsCollector, out WorkflowDescriptor workflowDescriptor)
		{
			if (workflowProvider == null) // This is not necessarily an error as the parent may have been deleted. This is a race condition.
			{
				logsCollector.AddWarning(FormattableString.Invariant($"No parent workflow provider was found for this trigger action. This is needed in order to fire triggers. Details about this trigger:\r\n{trigger.GetDiagnosticLogInfo()}"));
				workflowDescriptor = null;
				return false;
			}

			workflowDescriptor = trigger.IsLineTrigger ? WorkflowDescriptors.Instance.TryGetValueSafe(((ILineTriggerSupport)trigger).LineTriggerType) : trigger.GetWorkflowDescriptor();

			if (workflowDescriptor == null)
			{
				var message = FormattableString.Invariant($"There wasn't a workflow descriptor for trigger with Workflow Type {trigger.WorkflowProcessType}. This is needed in order to fire triggers. Details about this trigger:\r\n{trigger.GetDiagnosticLogInfo()}");
				logsCollector.AddWarning(message);
				ErrorReporter.ReportOnce(message);
				return false;
			}

			if (!workflowDescriptor.WorkflowProviderType.IsAssignableFrom(workflowProvider.GetType()))
			{
				var message = FormattableString.Invariant($"Trigger action workflow provider was expected to be of type {workflowDescriptor.WorkflowProviderType.FullName} but was of incompatible type {workflowProvider.GetType().FullName}. Trigger will not fire. Details about this trigger:\r\n{trigger.GetDiagnosticLogInfo()}");
				logsCollector.AddWarning(message);
				ErrorReporter.ReportOnce(message);
				return false;
			}

			return true;
		}

		#endregion
	}
}
