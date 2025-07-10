using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.EventManagement
{
	public sealed class PropagationHandler : IPropagationHandler
	{
		public PropagationHandler(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, nameof(factory));
			PropagationDeferrer = PropagationDeferrer.GetInstance(factory);
		}

		readonly Dictionary<ZGuid, LinkedPropagationState> linkedPropagationStates = new Dictionary<ZGuid, LinkedPropagationState>();
		PropagationDeferrer PropagationDeferrer { get; }

		public void Propagate(ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			Argument.NotNull(processHandlingInfo, nameof(processHandlingInfo));
			Argument.NotNull(logBeingAdded, nameof(logBeingAdded));

			if (!processHandlingInfo.IsEventExcludedFromCascadingOrPropagation(logBeingAdded.SL_SE_NKEvent))
			{
				if (!PropagationDeferrer.IsDeferred)
				{
					PropagateCore(processHandlingInfo, logBeingAdded);
				}
				else
				{
					PropagationDeferrer.AddLogToPropagate(processHandlingInfo, logBeingAdded);
				}
			}
		}

		void PropagateCore(ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			var propagationTargets = processHandlingInfo.GetPropagationTargets(logBeingAdded);
			if (propagationTargets != null)
			{
				foreach (var propagationTargetInfo in propagationTargets.Where(p => p.Target != null && !PropagationTargetHasMatchedEvent(p, processHandlingInfo, logBeingAdded)))
				{
					var propagationHandled = TryPropagateSpecialEvent(propagationTargetInfo, processHandlingInfo, logBeingAdded);
					if (!propagationHandled)
					{
						TryPropagateNormalEvent(propagationTargetInfo, processHandlingInfo, logBeingAdded);
					}
				}
			}
		}

		public void UnPropagate(ProcessHandlingInfo processHandlingInfo, IStmALog logBeingRemoved)
		{
			if (!processHandlingInfo.IsEventExcludedFromCascadingOrPropagation(logBeingRemoved.SL_SE_NKEvent))
			{
				var propagationTargets = processHandlingInfo.GetPropagationTargets(logBeingRemoved);
				if (propagationTargets != null)
				{
					foreach (var propagationTargetInfo in propagationTargets.Where(p => p.Target != null).ToList())
					{
						if (GetLinkedPropagationState(logBeingRemoved).InMemoryPropagatedLogs.Any())
						{
							TryUnpropagateEvent(propagationTargetInfo, processHandlingInfo, logBeingRemoved, GetLinkedPropagationState(logBeingRemoved).InMemoryPropagatedLogs);
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Key used for Propagated Event Identification")]
		public static bool IsPropagatedEventLog(IStmALog eventLog)
		{
			return eventLog.SL_Reference.StartsWith("Propagated:", StringComparison.OrdinalIgnoreCase);
		}

		#region Implementation

		internal static class PropagatedReferencePrefix
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Key used for Propagated Event Identification")]
			public const string NormalEvent = "Propagated: All ";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Key used for Propagated Event Identification")]
			public const string StartingEvent = "Propagated: First ";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Key used for Propagated Event Identification")]
			public const string FinishingEvent = "Propagated: Last ";
		}

		LinkedPropagationState GetLinkedPropagationState(IStmALog logBeingAddedOrRemoved)
		{
			if (!linkedPropagationStates.TryGetValue(logBeingAddedOrRemoved.Identifier, out var linkedPropagationState))
			{
				linkedPropagationState = new LinkedPropagationState(logBeingAddedOrRemoved.Factory, logBeingAddedOrRemoved);
				linkedPropagationStates.Add(logBeingAddedOrRemoved.Identifier, linkedPropagationState);
			}

			return linkedPropagationState;
		}

		bool TryPropagateSpecialEvent(PropagationLink propagationLink, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			var eventType = Events.All[logBeingAdded.SL_SE_NKEvent];

			if (processHandlingInfo.FinishingEventTypes.Contains(eventType))
			{
				return TryPropagateFinishingEvent(propagationLink, processHandlingInfo, logBeingAdded);
			}

			if (processHandlingInfo.StartingEventTypes.Contains(eventType))
			{
				return TryPropagateStartingEvent(propagationLink, processHandlingInfo, logBeingAdded);
			}

			return false;
		}

		bool TryPropagateStartingEvent(PropagationLink propagationLink, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			var firstFromSiblingCollection = propagationLink.GetFirstWhereThereIsAnOrder();
			if (firstFromSiblingCollection != null)
			{
				string referencePrefix = PropagatedReferencePrefix.StartingEvent + firstFromSiblingCollection.HumanReadableName;

				if (processHandlingInfo.logParent == firstFromSiblingCollection)
				{
					return TryAddPropagatedEvent(propagationLink, referencePrefix, logBeingAdded.Parameters, logBeingAdded).couldAdd;
				}

				var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, logBeingAdded.SL_SE_NKEvent);
				filter.AddToFilter(StmALogSchema.SL_IsEstimate, logBeingAdded.SL_IsEstimate);

				if (!propagationLink.Target.Logs.HasLogWith(filter))
				{
					return TryAddPropagatedEvent(propagationLink, referencePrefix, logBeingAdded.Parameters, logBeingAdded).couldAdd;
				}
			}

			return false;
		}

		bool TryPropagateFinishingEvent(PropagationLink propagationLink, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			var lastFromSiblingCollection = propagationLink.GetLastWhereThereIsAnOrder();
			if (lastFromSiblingCollection != null && processHandlingInfo.logParent == lastFromSiblingCollection)
			{
				string referencePrefix = PropagatedReferencePrefix.FinishingEvent + lastFromSiblingCollection.HumanReadableName;
				return TryAddPropagatedEvent(propagationLink, referencePrefix, logBeingAdded.Parameters, logBeingAdded).couldAdd;
			}

			return false;
		}

		void TryPropagateNormalEvent(PropagationLink propagationLink, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAdded)
		{
			var sublings = propagationLink.Siblings
				.Where(sibling => sibling.PK != processHandlingInfo.logParent.PK || sibling.TableName != processHandlingInfo.logParent.TableName)
				.ToArray();
			var sublingsEvents = new List<IEnumerable<StmALog>>();

			foreach (var subling in sublings)
			{
				AddFetchHintsForSiblingsEvents(subling as IStmALogParent, logBeingAdded);
			}

			foreach (var subling in sublings)
			{
				var events = FindMatchingEvents(subling as IStmALogParent, processHandlingInfo, logBeingAdded);
				if (events.Any())
				{
					sublingsEvents.Add(events);
				}
				else
				{
					return;
				}
			}

			var freeText = PropagatedReferencePrefix.NormalEvent + propagationLink.SiblingsDescription;
			var parameters = new Dictionary<string, string>();

			foreach (var parameter in logBeingAdded.Parameters)
			{
				if (sublingsEvents.All(evnts => evnts.Any(evnt => evnt.Parameters.TryGetValue(parameter.Key, out var foundValue) && object.Equals(parameter.Value, foundValue))))
				{
					parameters.Add(parameter.Key, parameter.Value);
				}
			}

			StmALog previouslyPropagatedLog = null;

			// If the log being added is set to PropagateOnParameterChange, we rely on the default behaviour of CreateRecreateOrUpdateEventLog
			// which will update if matching parameters match, or create a new event if not.
			if (!logBeingAdded.PropagationSettings.PropagateOnParameterChange)
			{
				previouslyPropagatedLog =
					GetLinkedPropagationState(logBeingAdded).InMemoryPropagatedLogs
						.FirstOrDefault(inMemoryPropagatedLog =>
					   inMemoryPropagatedLog.propagatedLog != null
					   && !inMemoryPropagatedLog.propagatedLog.IsDeleted
					   && !inMemoryPropagatedLog.propagatedLog.IsCancelled
					   && inMemoryPropagatedLog.propagatedLog.SL_SE_NKEvent == logBeingAdded.SL_SE_NKEvent
					   && inMemoryPropagatedLog.propagationTarget.LogsParentPK == propagationLink.Target.LogsParentPK).propagatedLog;
			}

			var addResult = TryAddPropagatedEvent(propagationLink, freeText, parameters, logBeingAdded, previouslyPropagatedLog);

			if (addResult.couldAdd)
			{
				GetLinkedPropagationState(logBeingAdded).TryRemovePropagatedLog(propagationLink.Target, addResult.propagatedLog);
				GetLinkedPropagationState(logBeingAdded).AddPropagatedLog(propagationLink.Target, addResult.propagatedLog, addResult.replacedLog);
			}
		}

		void TryUnpropagateEvent(PropagationLink propagationLink, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingRemoved, IEnumerable<(IStmALogParent propagationTarget, StmALog propagatedLog, StmALog replacedLog)> propagatedLogs)
		{
			var childObjectsOfPropagationTarget = propagationLink.Siblings
					.Where(sibling => sibling.PK != processHandlingInfo.logParent.PK || sibling.TableName != processHandlingInfo.logParent.TableName)
					.Append(processHandlingInfo.LogParent)
					.ToArray();

			foreach (var childObject in childObjectsOfPropagationTarget)
			{
				AddFetchHintsForSiblingsEvents(childObject as IStmALogParent, logBeingRemoved);
			}

			foreach (var childObject in childObjectsOfPropagationTarget)
			{
				var events = FindMatchingEvents(childObject as IStmALogParent, processHandlingInfo, logBeingRemoved);

				if (childObject.Equals(processHandlingInfo.LogParent))
				{
					if (events.Any(log => !log.IsInDatabase && !log.IsCancelled && !log.Equals(logBeingRemoved)))
					{
						continue;
					}
					else
					{
						UndoLogPropagation();
						break;
					}
				}
				else
				{
					if (events.Any())
					{
						continue;
					}
					else
					{
						UndoLogPropagation();
						break;
					}
				}
			}

			void UndoLogPropagation()
			{
				foreach (var propagatedLog in propagatedLogs.Where(log => log.propagationTarget.LogsParentPK.Equals(propagationLink.Target.LogsParentPK)).ToList())
				{
					if (!propagatedLog.propagatedLog.IsInDatabase)
					{
						propagatedLog.propagatedLog.Delete();

						if (propagatedLog.propagatedLog != propagatedLog.replacedLog)
						{
							propagatedLog.replacedLog?.Reactivate();
						}
					}
				}
			}
		}

		ZQuery GetSiblingsEventsQuery(IStmALogParent parent, IStmALog logBeingAddedOrRemoved)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, logBeingAddedOrRemoved.SL_SE_NKEvent);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, logBeingAddedOrRemoved.SL_IsEstimate);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			query.AddToFilter(StmALogSchema.SL_Parent, parent.LogsParentPK);
			return query;
		}

		void AddFetchHintsForSiblingsEvents(IStmALogParent parent, IStmALog logBeingAddedOrRemoved)
		{
			var businessObj = parent as BusinessObject;
			if (businessObj != null && businessObj.IsInDatabase && !parent.Logs.IsElementsLoaded)
			{
				var query = GetSiblingsEventsQuery(parent, logBeingAddedOrRemoved);
				parent.LogsFactory.AddFetchHint(typeof(StmALog), query);
			}
		}

		(bool couldAdd, StmALog propagatedLog, StmALog replacedLog) TryAddPropagatedEvent(PropagationLink propagationLink, string referenceFreeText, IEnumerable<KeyValuePair<string, string>> parameters, IStmALog logBeingAdded, StmALog exisitingLog = null)
		{
			if (!EventRecursionHandler.Instance.IsDuplicatePropagation(propagationLink.Target.Identifier, logBeingAdded.SL_SE_NKEvent))
			{
				var estimateActual = logBeingAdded.SL_IsEstimate ? EstimateActual.Estimate : EstimateActual.Actual;

				var eventType = Events.All[logBeingAdded.SL_SE_NKEvent];
				if (eventType == null)
				{
					var errorMessage = string.Format(CultureInfo.InvariantCulture, "SL_Table is {0}, SL_Parent is {1}, SL_Reference is {2}, SL_SE_NKEvent is {3}.",
					logBeingAdded.SL_Table, logBeingAdded.SL_Parent, logBeingAdded.SL_Reference, logBeingAdded.SL_SE_NKEvent);
					ErrorReporter.ReportOnce("LogsEventTypeIsNULL", errorMessage);
					return (false, null, null);
				}

				StmALog propagatedLog;
				StmALog replacedLog;
				if (exisitingLog != null)
				{
					var eventValue = new EventValue(
						eventType,
						isEstimate: estimateActual == EstimateActual.Estimate,
						eventTime: logBeingAdded.SL_EventTimeOffset,
						reference: referenceFreeText,
						deferFiringWorkflow: logBeingAdded.SL_FireWorkflow,
						parameters: parameters.ToImmutableDictionary());

					propagatedLog = propagationLink.Target.Logs.CreateRecreateOrUpdateEventLog(eventValue, exisitingLog, false);
					replacedLog = propagatedLog;
				}
				else
				{
					var result = propagationLink.Target.Logs.CreateRecreateOrUpdateEventLog(eventType, estimateActual, logBeingAdded.SL_EventTimeOffset, referenceFreeText, logBeingAdded.SL_FireWorkflow, parameters.ToArray());
					propagatedLog = result.newLog;
					replacedLog = result.replacedLog;
				}

				if (propagatedLog != null)
				{
					return (true, propagatedLog, replacedLog);
				}
			}

			return (false, null, null);
		}

		bool PropagationTargetHasMatchedEvent(PropagationLink propagationLink, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAddedOrRemoved)
		{
			return FindMatchingEvents(propagationLink.Target, processHandlingInfo, logBeingAddedOrRemoved)
				.Any(matchedEvent => matchedEvent.SL_EventTime == logBeingAddedOrRemoved.SL_EventTime);
		}

		IEnumerable<StmALog> FindMatchingEvents(IStmALogParent parent, ProcessHandlingInfo processHandlingInfo, IStmALog logBeingAddedOrRemoved)
		{
			var parametersToMatch = processHandlingInfo.GetEventParametersToMatchDuringPropagation(logBeingAddedOrRemoved.SL_SE_NKEvent);
			var businessObj = parent as BusinessObject;
			if (businessObj == null || !businessObj.IsInDatabase || parent.Logs.IsElementsLoaded)
			{
				return parent.Logs.Find(log => HasSameEventTypeAndMatchingParameters(parametersToMatch, log, logBeingAddedOrRemoved));
			}

			var query = GetSiblingsEventsQuery(parent, logBeingAddedOrRemoved);
			var logsInDb = parent.LogsFactory.Load<StmALog>(query);
			return logsInDb.Concat(parent.Logs.LogsNotInDB).Where(log => HasSameEventTypeAndMatchingParameters(parametersToMatch, log, logBeingAddedOrRemoved)).ToArray();
		}

		bool HasSameEventTypeAndMatchingParameters(IEnumerable<string> parametersToMatch, StmALog log, IStmALog logBeingAddedOrRemoved)
		{
			return log.SL_SE_NKEvent == logBeingAddedOrRemoved.SL_SE_NKEvent
				&& log.SL_IsEstimate == logBeingAddedOrRemoved.SL_IsEstimate
				&& !log.SL_IsCancelled
				&& AreParametersMatched(parametersToMatch, log, logBeingAddedOrRemoved);
		}

		static bool AreParametersMatched(IEnumerable<string> parametersToMatch, IStmALog log1, IStmALog log2)
		{
			var parameters1 = log1.Parameters.Where(p => parametersToMatch.Contains(p.Key)).ToArray();
			var parameters2 = log2.Parameters.Where(p => parametersToMatch.Contains(p.Key)).ToArray();

			if (parameters1.Length != parameters2.Length)
			{
				return false;
			}

			foreach (var pair in parameters1)
			{
				var hasMatch = parameters2.Any(p => p.Key == pair.Key && StringComparer.InvariantCultureIgnoreCase.Compare(p.Value, pair.Value) == 0);

				if (!hasMatch)
				{
					return false;
				}
			}

			return true;
		}

		#endregion
	}
}
