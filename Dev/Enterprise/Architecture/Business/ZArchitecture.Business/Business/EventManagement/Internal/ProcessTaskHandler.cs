using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public partial class ProcessTaskHandler : IProcessTaskHandler
	{
		internal ProcessTaskHandler(IStmALogParent targetBusinessObject, IStmALog logBeingAdded)
		{
			this.log = Argument.NotNull(logBeingAdded, nameof(logBeingAdded));
			this.targetBusinessObject = Argument.NotNull(targetBusinessObject, nameof(targetBusinessObject));

			if (targetBusinessObject is IProcessHandlingInfoProvider)
			{
				processHandlingInfo = ((IProcessHandlingInfoProvider)targetBusinessObject).ProcessHandlingInfo;
			}
		}

		public ProcessTaskHandler(ProcessHandlingInfo processHandlingInfo, IStmALog log, IStmALogParent target)
		{
			this.log = log ?? throw new ArgumentNullException(nameof(log));
			this.processHandlingInfo = processHandlingInfo;
			this.targetBusinessObject = target;
		}

		readonly IStmALogParent targetBusinessObject;
		readonly ProcessHandlingInfo processHandlingInfo;
		IStmALog log;

		#region IProcessTaskHandler Members

		void IProcessTaskHandler.Fire()
		{
			if (ShouldUpdateTriggersWithLog(log))
			{
				var eventTime = log.SL_EventTimeOffset;
				FireTriggers(eventTime);
				if (processHandlingInfo != null && !IsAnythingImportantDeleted())
				{
					CascadeTriggerEventDates(ref eventTime);
					if (!IsAnythingImportantDeleted())
					{
						new PropagationHandler(log.Factory).Propagate(processHandlingInfo, log);
					}
				}
			}
		}

		void IProcessTaskHandler.Withdraw()
		{
			if (ShouldUpdateTriggersWithLog(log))
			{
				var eventTime = ZDateTimeOffset.Empty;
				FireTriggers(eventTime);
				if (processHandlingInfo != null && !IsAnythingImportantDeleted())
				{
					CascadeTriggerEventDates(ref eventTime);
					if (!IsAnythingImportantDeleted())
					{
						new PropagationHandler(log.Factory).UnPropagate(processHandlingInfo, log);
					}
				}
			}
		}

		#endregion

		#region Implementation

		void CascadeTriggerEventDates(ref ZDateTimeOffset dateToCascade)
		{
			if (!processHandlingInfo.IsEventExcludedFromCascadingOrPropagation(log.SL_SE_NKEvent))
			{
				var targets = processHandlingInfo.GetCascadingTargets(log);
				if (targets != null)
				{
					foreach (var target in targets)
					{
						if (IsAnythingImportantDeleted())
						{
							return;
						}
						UpdateTriggerEventDatesForCascading(target, ref dateToCascade);
					}
				}
			}
		}

		void FireTriggers(ZDateTimeOffset eventTime)
		{
			var recursionHandler = EventRecursionHandler.Instance;
			if (log.Factory.OnSaveDelayer.IsDelaying)
			{
				log = log.WeakCopy();
			}

			HookFireForUnitTests("FireTriggers");  // Key for test method

			log.Factory.OnSaveDelayer.Do(() =>
			{
				if (!IsAnythingImportantDeleted())
				{
					using (EventRecursionHandler.WithEventRecursionDetection(recursionHandler))
					{
						UpdateTriggerEventDatesForLocal(ref eventTime);
						UpdateParentTriggers(ref eventTime);
					}
				}
			});
		}

		#region For Test Purposes

		partial void HookFireForUnitTests(string methodName);

		#endregion

		bool IsAnythingImportantDeleted()
		{
			return (log is BusinessObject bizo && (bizo.IsDeleted || bizo.IsDeleting))
				|| targetBusinessObject == null
				|| targetBusinessObject.IsDeleted;
		}

		void UpdateTriggerEventDatesForLocal(ref ZDateTimeOffset value)
		{
			var triggers = ObjectFactory.Get<ITriggerProvider>().LoadAllMilestonesAndTriggersForEvent(targetBusinessObject, log, value);
			UpdateTriggerEventDates(targetBusinessObject, triggers, ref value);
		}

		void UpdateParentTriggers(ref ZDateTimeOffset value)
		{
			if (!IsAnythingImportantDeleted() && processHandlingInfo != null && !log.SL_IsEstimate)
			{
				var triggers = processHandlingInfo.GetParentTriggers(log);
				if (triggers != null)
				{
					LineProcessTaskHandler.FireLineTriggers(value, triggers, log, targetBusinessObject);
				}
			}
		}

		void UpdateTriggerEventDatesForCascading(CascadingLink cascadingLink, ref ZDateTimeOffset value)
		{
			if (!EventRecursionHandler.Instance.IsDuplicateCascade(cascadingLink.Parent.Identifier, log.SL_SE_NKEvent))
			{
				var filteredTriggers = cascadingLink.Triggers.Where(trigger => trigger.AreTriggerConditionsMet(log, cascadingLink.Parent))
				.Select(t => ((IBaseTrigger)t, (IBusiness)cascadingLink.Parent)).ToList();
				UpdateTriggerEventDates(cascadingLink.Parent, filteredTriggers, ref value);
			}
		}

		void UpdateTriggerEventDates(IStmALogParent workflowParent, ICollection<(IBaseTrigger trigger, IBusiness parent)> triggers, ref ZDateTimeOffset value)
		{
			var logInternals = log as IBusinessObjectInternals;

			using (logInternals != null ? logInternals.SuppressReportRowDeletedError() : null)
			{
				var isEstimate = log.SL_IsEstimate;

				foreach (var tuple in triggers)
				{
					var trigger = tuple.trigger;
					if (!WorkflowChainManager.HasWTEEventFromCurrentChain(trigger) && !EventRecursionHandler.Instance.IsBlockingDuplicateFire(trigger.Identifier, value) && !IsAnythingImportantDeleted())
					{
						if (isEstimate)
						{
							trigger.SetEstimateTime(log, value);
						}

						if (trigger.ShouldTriggerOnEstimateEvents == isEstimate)
						{
							trigger.SetEventTime(log, workflowParent, value);
						}

						UpdateEstimateOnSubsequentMilestones(workflowParent, trigger, isEstimate);
					}

					HookFireForUnitTests("UpdateTriggerEventDates"); // Key for test method
				}
			}
		}

		bool ShouldUpdateTriggersWithLog(IStmALog log)
		{
			if (this.log is BusinessObject bizo && bizo.IsDeleted)
			{
				return false;
			}

			if (EnvProxy.Instance.CurrentUser == null)
			{
				return false;
			}

			if (!Logs.IsFiringDelayedWorkflow && log.SL_SE_NKEvent == Events.EditedARecordCode)
			{
				return !EnvProxy.Instance.CurrentUser.IsBatchProcessor && ObjectFactory.Get<IEnv>().Instance.ServiceTaskCode.IsNullOrEmpty();
			}

			return true;
		}

		void UpdateEstimateOnSubsequentMilestones(IStmALogParent workflowParent, IBaseTrigger trigger, bool isEstimate)
		{
			if (ObjectFactory.Get<ITriggerProvider>().TryGetQueryForAllTriggersIncludingThoseOnParentObjects(workflowParent, ZString.Empty, out ZQuery relatedQuery))
			{
				string defaultFrom = isEstimate ? "" : "ACT";
				relatedQuery.AddToFilter(ProcessTasksSchema.P9_EstimatedDefaultedFrom, defaultFrom);
				relatedQuery.AddToFilter(ProcessTasksSchema.P9_EstimatedDefaultFromPredecessor, trigger.Sequence);
				relatedQuery.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, System.DBNull.Value);

				foreach (var relatedTask in workflowParent.Factory.Load<IProcessTask>(relatedQuery))
				{
					relatedTask.DefaultEstimateIfRequired();
				}
			}
		}

		#endregion
	}

#if DEBUG
	public partial class ProcessTaskHandler
	{
		static readonly Overridable<Action<IStmALog, string>> onFire = new Overridable<Action<IStmALog, string>>(null);

		public static void SetOnFireHookForTest(Action<IStmALog, string> onFireHook)
		{
			onFire.Value = onFireHook;
		}

		partial void HookFireForUnitTests(string methodName)
		{
			onFire.Value?.Invoke(log, methodName);
		}

		public void UpdateParentTriggersExposedForTest(ref ZDateTimeOffset value) => UpdateParentTriggers(ref value);
	}
#endif
}
