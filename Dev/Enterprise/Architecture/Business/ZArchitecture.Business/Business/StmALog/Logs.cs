using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using IPropagationSettings = Enterprise.Integration.IPropagationSettings;

namespace Enterprise.ZArchitecture.Business
{
	public interface ILogsInternals
	{
		StmALog AddNew(Type logType);
		void ReloadFromDB();
	}

	public class Logs : ZLogsOrNotes, ILogsInternals
	{
		public Logs(IStmALogParent parent)
			: base((BusinessObject)parent)
		{
		}

		/// <summary>
		/// Sets SL_IsCancelled to true for all Logs. Note that this will first LOAD ALL LOGS for the parent business object (which may be slow).
		/// </summary>
		public void CancelAll()
		{
			((StmALogDependentCollection)ElementsInternal).CancelAll();
		}

		public IEnumerable<StmALog> Find(Func<StmALog, bool> predicate)
		{
			return ElementsInternal.Cast<StmALog>().Where(predicate);
		}

		/// <summary>
		/// Performs a search on the logs collection. Note that this will first LOAD ALL LOGS for the parent business object (which may be slow).
		/// </summary>
		public StmALog[] Find(ZQuery filter)
		{
			return (StmALog[])ElementsInternal.Find(filter);
		}

		/// <summary>
		/// Returns true if any StmALog records exist in the DATABASE for this business object matching the given filter (this may not be the same as the FACTORY!).
		/// </summary>
		public bool DatabaseHasLogs(ZQuery filter)
		{
			filter.AddToFilter(StmALogSchema.SL_Parent, Parent.LogsParentPK);

			return ElementsFactory.ExistsInDatabase(BusinessObjectFactory.GetTableNameFromType(ElementType), filter);
		}

		#region AddNew

		/// <summary>
		/// TODO: This method should be internal
		/// Adds a new StmALog to the collection.
		/// </summary>
		public new StmALog AddNew()
		{
			return (StmALog)base.AddNew();
		}

		public StmALog AddNew(Event eventType, ZBool isEstimate)
		{
			return AddNew(new EventValue(eventType, isEstimate: isEstimate));
		}

		public StmALog AddNew(Event eventType, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, parameters: parameters?.ToImmutableDictionary()));
		}

		public StmALog AddNew(Event eventType, ZDateTimeOffset dateTime, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, eventTime: dateTime, parameters: parameters?.ToImmutableDictionary()));
		}

		public StmALog AddNew(Event eventType, ZString reference, ZDateTimeOffset dateTime, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, eventTime: dateTime, reference: reference, parameters: parameters?.ToImmutableDictionary()));
		}

		public StmALog AddNew(Event eventType, ZDateTimeOffset dateTime, ZBool isEstimate, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, eventTime: dateTime, isEstimate: isEstimate, parameters: parameters?.ToImmutableDictionary()));
		}

		public StmALog AddNew(Event eventType, ZString reference, ZDateTimeOffset dateTime, ZBool isEstimate, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, reference: reference, eventTime: dateTime, isEstimate: isEstimate, parameters: parameters?.ToImmutableDictionary()));
		}

		public StmALog AddNew(Event eventType, ZString reference, ZDateTimeOffset dateTime, ZBool isEstimate, IPropagationSettings propagationSettings, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, reference: reference, eventTime: dateTime, isEstimate: isEstimate, propagationSettings: propagationSettings, parameters: parameters?.ToImmutableDictionary()));
		}

		/// <summary>
		/// Adds a new StmALog to the collection. Use Events.* to specify an Event type.
		/// </summary>
		public StmALog AddNew(Event eventType, ZString reference, params KeyValuePair<string, string>[] parameters)
		{
			return AddNew(new EventValue(eventType, reference: reference, parameters: parameters?.ToImmutableDictionary()));
		}

		/// <summary>
		/// Adds a new StmALog to the collection. Use Events.* to specify an Event type.
		/// </summary>
		public StmALog AddNew(EventValue eventValue)
		{
			if (!IsEventBeingAdded(eventValue))
			{
				return AddNewWithoutDuplicateCheck(eventValue);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// This Log will not cause Workflow to fire when it is added (if a trigger is added in the future it can fired)
		/// Example use case: archiving on old jobs (you probably want to SuppressTemplateApplication() in this case too)
		/// </summary>
		/// <returns></returns>
		public static IDisposable SuppressFiringWorkflow()
		{
			isFiringWorkflowSuppressed = true;
			return new DisposableAction(() => isFiringWorkflowSuppressed = false);
		}

		[ThreadStatic]
		static bool isFiringWorkflowSuppressed;

		/// <summary>
		/// Workflow will be fired in Log Walker service task instead of when the Log is added
		/// </summary>
		/// <param name="suppress"></param>
		/// <returns></returns>
		public static IDisposable DeferFiringWorkflow(bool defer)
		{
			isFiringWorkflowDeferred = defer;
			return new DisposableAction(() => isFiringWorkflowDeferred = false);
		}

		[ThreadStatic]
		static bool isFiringWorkflowDeferred;

		public static IDisposable FiringDelayedWorkflow()
		{
			if (!isFiringDelayedWorkflow)
			{
				isFiringDelayedWorkflow = true;
				return new DisposableAction(() => isFiringDelayedWorkflow = false);
			}
			return null;
		}

		public static ZBool IsFiringDelayedWorkflow => isFiringDelayedWorkflow;

		[ThreadStatic]
		static ZBool isFiringDelayedWorkflow;

		internal StmALog AddNewWithoutDuplicateCheck(EventValue eventValue)
		{
			var shouldBeNonPersistedLog = ShouldCreateNonPersistedStmALog(eventValue.Code);
			var log = shouldBeNonPersistedLog ? ElementsFactory.New<StmALogAddedToQueueOnly>() : ElementsFactory.New<StmALog>();
			log.SetPropagationConfig(eventValue.PropagationSettings);
			log.SetInMemoryIdentifier(eventValue.InMemoryIdentifier);

			bool pushed = false;
			try
			{
				using (log.LockForUpdatingKeyFields(!isFiringWorkflowSuppressed))
				{
					log.SL_SE_NKEvent = eventValue.Code;
					log.SL_IsEstimate = eventValue.IsEstimate;
					log.SL_Reference = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(eventValue.Reference, null);

					if (isFiringWorkflowSuppressed)
					{
						log.SL_FireWorkflow = false;
					}
					else
					{
						log.SL_FireWorkflow = isFiringWorkflowDeferred || eventValue.DeferFiringWorkflow || Parent.DeferFiringWorkflow;
					}

					if (eventValue.Parameters != null)
					{
						foreach (var param in eventValue.Parameters)
						{
							log.Parameters[param.Key] = param.Value;
						}
					}
					if (eventValue.EventTime.IsValid)
					{
						log.SL_EventTimeOffset = eventValue.EventTime;
					}

					if (log.Factory.IsInTransaction)
					{
						// Log record can be created after OnSaving() has already been run on logs factory in multi-factory transaction
						log.InitializePostedTime();
					}

					log.Master = Parent;
					Add(log);

					eventsInTheProcessOfBeingAdded.Push(eventValue.EventType.Code);
					pushed = true;
				}
			}
			finally
			{
				if (pushed)
				{
					eventsInTheProcessOfBeingAdded.Pop();
				}
			}

			return log;
		}

		#endregion

#if DEBUG
		public
#endif
		Stack<string> eventsInTheProcessOfBeingAdded = new Stack<string>();

		bool IsEventBeingAdded(EventValue eventInfo)
		{
			return eventsInTheProcessOfBeingAdded.Contains(eventInfo.EventType.Code, StringComparer.OrdinalIgnoreCase);
		}

		#region MostRecentLogByEventTime

		/// <summary>
		/// Loads the most recent log based on SL_EventTime for a given Event.
		/// Cancelled events are not retrieved by this method
		/// </summary>
		public StmALog MostRecentLogByEventTime(Event @event)
		{
			return MostRecentLogByEventTime(@event, (ZQuery)null);
		}

		/// <summary>
		/// Loads the most recent log based on SL_EventTime and SL_Reference for a given Event.
		/// Cancelled events are not retrieved by this method
		/// </summary>
		public StmALog MostRecentLogByEventTime(Event @event, ZString reference)
		{
			return MostRecentLogByEventTime(@event, new ZQuery(StmALogSchema.SL_Reference, reference));
		}

		/// <summary>
		/// Loads the most recent log based on SL_EventTime and ExtraQuery for a given Event.
		/// Cancelled events are not retrieved by this method
		/// </summary>
		public StmALog MostRecentLogByEventTime(Event @event, ZQuery extraQuery)
		{
			var filter = MostRecentLogByEventTimeQuery(@event, extraQuery);
			return FindWithFactoryIfNotLoaded(filter).FirstOrDefault();
		}

		public ZQuery MostRecentLogByEventTimeQuery(Event @event, ZQuery extraQuery)
		{
			var filter = GetParentRelatedLogsFilter();
			filter.MaximumRows = 1;
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);
			if (extraQuery != null && !extraQuery.IsEmpty)
			{
				filter.AddToFilter(extraQuery);
			}

			return filter;
		}

		public StmALog MostRecentLogByEventTime(Event @event, Func<StmALog, bool> queryFunc)
		{
			return LogByEventTime(@event, queryFunc, OrderDirection.Descending);
		}

		public StmALog EarliestLogByEventTime(Event @event, Func<StmALog, bool> queryFunc)
		{
			return LogByEventTime(@event, queryFunc, OrderDirection.Ascending);
		}

		StmALog LogByEventTime(Event @event, Func<StmALog, bool> queryFunc, OrderDirection direction, bool useFactoryIfNotLoaded = true)
		{
			var filter = GetParentRelatedLogsFilter(StmALogSchema.Constants.SL_EventTime, direction);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);
			filter.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;

			return FindWithFactoryIfNotLoaded(filter, useFactoryIfNotLoaded).FirstOrDefault(queryFunc);
		}

		IEnumerable<StmALog> FindWithFactoryIfNotLoaded(ZQuery filter, bool useFactoryIfNotLoaded = true)
		{
			if (IsElementsLoaded || !useFactoryIfNotLoaded)
			{
				return Find(filter);
			}

			try
			{
				return Factory.Load<StmALog>(filter);
			}
			catch (SqlException ex)
			{
				if (ex.IsInvalidObjectName())
				{
					return Find(filter);
				}

				throw;
			}
		}

		/// <summary>
		/// Loads the most recent log based on SL_EventTime and queryFunc for a given Event.
		/// Also searches children of the business object this Logs belongs to.
		/// Cancelled events are not retrieved by this method
		/// </summary>

		public StmALog MostRecentLogIncludingChildrenByEventTime(Event @event, Func<StmALog, bool> queryFunc)
		{
			return LogIncludingChildrenByEventTime(@event, queryFunc, OrderDirection.Descending);
		}

		public StmALog EarliestLogIncludingChildrenByEventTime(Event @event, Func<StmALog, bool> queryFunc)
		{
			return LogIncludingChildrenByEventTime(@event, queryFunc, OrderDirection.Ascending);
		}

		public StmALog LogIncludingChildrenByEventTime(Event @event, Func<StmALog, bool> queryFunc, OrderDirection direction)
		{
			var logList = new List<StmALog>();
			logList.Add(LogByEventTime(@event, queryFunc, direction, useFactoryIfNotLoaded: false));
			foreach (BusinessObject bo in GetBusinessObjectsWithRelatedElements())
			{
				var workflowTriggerEventSource = bo as IWorkflowTriggerEventSource;
				var boAsEnterpriseBizO = bo as EnterpriseBusinessObject;

				if (workflowTriggerEventSource != null && boAsEnterpriseBizO != null)
				{
					var parents = workflowTriggerEventSource.ParentWorkflowProviders;
					if (parents.Any(x => x.Equals(Parent)))
					{
						logList.Add(boAsEnterpriseBizO.Logs.LogByEventTime(@event, queryFunc, direction, useFactoryIfNotLoaded: false));
					}
				}
			}

			return direction == OrderDirection.Descending
					? logList.Where(x => x != null).MaxBySafe(x => x.SL_EventTime)
					: logList.Where(x => x != null).MinBySafe(x => x.SL_EventTime);
		}

		ZQuery GetParentRelatedLogsFilter(string orderby, OrderDirection direction = OrderDirection.Descending)
		{
			return GetParentRelatedLogsFilter(orderby, null, direction);
		}

		ZQuery GetParentRelatedLogsFilter(string orderField1, string orderField2, OrderDirection direction = OrderDirection.Descending)
		{
			ZQuery filter = new ZQuery(StmALogSchema.SL_Parent, this.Parent.LogsParentPK);
			filter.AddToFilter(StmALogSchema.SL_IsCancelled, "N");
			filter.OrderBy = orderField1 + OrderByClause.Get(direction);
			filter.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;
			if (orderField2 != null)
			{
				filter.OrderBy += $", {orderField2}{OrderByClause.Get(direction)}";
			}
			return filter;
		}

		ZQuery GetParentRelatedLogsFilter()
		{
			return GetParentRelatedLogsFilter(StmALogSchema.Constants.SL_EventTime);
		}

		public StmALog MostRecentLog
		{
			get
			{
				var filter = GetParentRelatedLogsFilter();
				filter.MaximumRows = 1;

				return FindWithFactoryIfNotLoaded(filter).FirstOrDefault();
			}
		}

		public StmALog MostRecentLogByPostedDate
		{
			get
			{
				var filter = GetParentRelatedLogsFilter(StmALogSchema.Constants.SL_PostedTimeUtc);
				filter.MaximumRows = 1;

				return FindWithFactoryIfNotLoaded(filter).FirstOrDefault();
			}
		}

		#endregion

		#region CreateOrRecreateEventLog
		/// <summary>
		/// - If eventTime is valid and an event log already exists in memory, it is updated instead of being re-created.
		/// - If eventTime is empty and there is an existing uncommitted (not in db) log, it is deleted.
		/// - Otherwise, it will create a new log regardless of whether an event with same type already exist.
		/// </summary>
		public void CreateOrRecreateEventLog(Event eventType, EstimateActual estimateActual, ZDateTimeOffset eventTime)
		{
			CreateOrRecreateEventLog(new EventValue(eventType,
				isEstimate: estimateActual == EstimateActual.Estimate,
				eventTime: eventTime));
		}

		public void CreateOrRecreateEventLog(Event eventType, EstimateActual estimateActual, ZDateTimeOffset eventTime, ZString logReference, params KeyValuePair<string, string>[] parameters)
		{
			CreateOrRecreateEventLog(new EventValue(eventType,
				isEstimate: estimateActual == EstimateActual.Estimate,
				eventTime: eventTime,
				reference: logReference,
				parameters: parameters.ToImmutableDictionary()));
		}

		public void CreateOrRecreateEventLog(EventValue eventInfo)
		{
			Argument.NotNull(eventInfo, "eventInfo");

			if (!IsEventBeingAdded(eventInfo))
			{
				FindCreateOrRecreateEventLog(eventInfo, false);
			}
		}

		#endregion

		#region CreateRecreateOrUpdateEventLog

		/// <summary>
		/// Creates a new event log and cancels the existing log (if found).
		/// - If eventTime is valid and an event log already exists in memory, it is updated instead of being re-created.
		/// - If eventTime is empty and there is an existing uncommitted (not in db) log, it is deleted.
		/// - If eventTime is empty, and there is an existing committed (saved) log, the existing log is cancelled.
		/// </summary>
		public StmALog CreateRecreateOrUpdateEventLog(Event eventType, EstimateActual estimateActual, ZDateTimeOffset eventTime)
		{
			return CreateRecreateOrUpdateEventLog(new EventValue(eventType,
				isEstimate: estimateActual == EstimateActual.Estimate,
				eventTime: eventTime
			));
		}

		public StmALog CreateRecreateOrUpdateEventLog(Event eventType, EstimateActual estimateActual, ZDateTimeOffset eventTime, ZString logReference, params KeyValuePair<string, string>[] parameters)
		{
			return CreateRecreateOrUpdateEventLog(eventType, estimateActual, eventTime, logReference, false, parameters).newLog;
		}

		public StmALog CreateRecreateOrUpdateEventLog(EventValue eventInfo, bool forceAdd = false)
		{
			Argument.NotNull(eventInfo, "eventInfo");
			if (forceAdd || !IsEventBeingAdded(eventInfo))
			{
				return FindCreateOrRecreateEventLog(eventInfo, true).newLog;
			}
			else
			{
				return null;
			}
		}

		public StmALog CreateRecreateOrUpdateEventLog(EventValue eventInfo, StmALog existingLog, bool duplicateCheck = true)
		{
			Argument.NotNull(eventInfo, "eventInfo");
			var logReferenceWithParameters = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(eventInfo.Reference, eventInfo.Parameters);
			return UpdateOrRecreateEventLog(eventInfo, existingLog, logReferenceWithParameters, duplicateCheck).newLog;
		}

		internal (StmALog newLog, StmALog replacedLog) CreateRecreateOrUpdateEventLog(Event eventType, EstimateActual estimateActual, ZDateTimeOffset eventTime, ZString logReference, bool deferFiringWorkflow, params KeyValuePair<string, string>[] parameters)
		{
			var eventValue = new EventValue(eventType,
				isEstimate: estimateActual == EstimateActual.Estimate,
				eventTime: eventTime,
				reference: StmALog.GetFreeTextFromReference(logReference),
				deferFiringWorkflow: deferFiringWorkflow,
				parameters: parameters.Concat(StmALog.GetParametersFromReference(logReference)).ToImmutableDictionary());

			if (!IsEventBeingAdded(eventValue))
			{
				return FindCreateOrRecreateEventLog(eventValue, true);
			}
			else
			{
				return (null, null);
			}
		}

		(StmALog newLog, StmALog replacedLog) FindCreateOrRecreateEventLog(EventValue eventInfo, bool checkLogsInDb)
		{
			var logReferenceWithParameters = StmALog.GenerateEventReferenceToFitInReferenceMaxLength(eventInfo.Reference, eventInfo.Parameters);
			var log = FindExistingLog(eventInfo, logReferenceWithParameters, checkLogsInDb);
			return UpdateOrRecreateEventLog(eventInfo, log, logReferenceWithParameters);
		}

		StmALog FindExistingLog(EventValue eventInfo, string logReferenceWithParameters, bool checkLogsInDb)
		{
			var log = FindActiveLogNotInDB(eventInfo);
			if (log == null && checkLogsInDb && ((BusinessObject)Parent).IsInDatabase)
			{
				log = FindActiveLog(eventInfo.Code, eventInfo.IsEstimate, logReferenceWithParameters);
			}
			return log;
		}

		(StmALog newLog, StmALog replacedLog) UpdateOrRecreateEventLog(EventValue eventInfo, StmALog logToReplace, string logReferenceWithParameters, bool duplicateCheck = true)
		{
			// This method is worth commenting because it contains some complex/confusing business logic.
			// We are supporting four use cases (Note: There is no official update note for these use cases, this is just how it works.)
			// 1) A user sets a date field for the first time which raises and event and fires workflow.
			// 2) A user modifies a date field that they just set, which should update workflow but not re-fire it.
			// 3) A user modifies a date field after previously saving, which should fire triggers but not milestones.
			// 4) A user clears a date field, so that they can set it again, causing workflow to fire again.

			if (logToReplace != null)
			{
				if (!logToReplace.IsInDatabase)
				{
					// As the event is not in the database, we assume at this point the user, is changing the time because they made a mistake.
					// This means we do not want to have two logs where one will do and we just update the existing log.
					if (eventInfo.EventTime.IsValid)
					{
						using (logToReplace.LockForUpdatingKeyFields(FireWorkflowMode.UpdateAll))
						{
							logToReplace.SL_Reference = logReferenceWithParameters;
							logToReplace.SL_EventTimeOffset = eventInfo.EventTime;
						}
					}
					else
					{
						// There is no point raising a cancelled log so we just delete it.
						logToReplace.Delete();
					}

					return (logToReplace, logToReplace);
				}
				else
				{
					// If the user has saved before updating a log, the best we can do is cancel the old log.
					// This means that there is a race condition in user space as the service task may already be processing/have processed the old log.
					if (!logToReplace.SL_IsCancelled)
					{
						if (logToReplace.SL_EventTimeOffset != eventInfo.EventTime)
						{
							if (eventInfo.EventTime.IsEmpty)
							{
								// If the user clears the date, it should clear milestones.
								logToReplace.Cancel();
							}
							else
							{
								// We do not modify the dates with this cancel because we do not want milestones to fire multiple times on updates.
								// (As we are about to raise a new log and the milestone would immediately fire otherwise).
								logToReplace.CancelWithoutChangingAnything();
							}
						}
						else
						{
							return (null, logToReplace);
						}
					}
				}
			}

			if (eventInfo.EventTime.IsValid)
			{
				return duplicateCheck ? (AddNew(eventInfo), logToReplace) : (AddNewWithoutDuplicateCheck(eventInfo), logToReplace);
			}
			else
			{
				return (null, logToReplace);
			}
		}

		StmALog FindActiveLog(ZString eventCode, bool isEstimate, ZString logReference)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmALog));
			query.AddToFilter(StmALogSchema.SL_Parent, Parent.LogsParentPK);
			query.AddToFilter(StmALogSchema.SL_Table, Parent.LogsParentTableName);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_IsEstimate, isEstimate);
			query.AddToFilter(StmALogSchema.SL_IsCancelled, ZBool.False);
			if (!logReference.IsEmpty)
			{
				query.AddToFilter(StmALogSchema.SL_Reference, logReference);
			}
			query.OrderBy = "SL_PostedTimeUtc desc";// this is a column name
			return Factory.LoadTop1<StmALog>(query);
		}

		StmALog FindActiveLogNotInDB(EventValue eventInfo)
		{
			Func<KeyValuePair<string, string>, bool> inNewParameters = pair =>
			{
				return eventInfo.Parameters != null && eventInfo.Parameters.Any(p => p.Key == pair.Key && p.Value == pair.Value || string.IsNullOrEmpty(p.Value));
			};

			foreach (StmALog log in LogsNotInDB)
			{
				if (log.SL_SE_NKEvent == eventInfo.Code &&
					log.SL_IsEstimate == eventInfo.IsEstimate &&
					(eventInfo.Reference.IsEmpty || log.ReferenceFreeText.IsEmpty || log.ReferenceFreeText == eventInfo.Reference || log.SL_Reference == eventInfo.Reference) &&
					log.Parameters.All(inNewParameters) &&
					!log.SL_IsCancelled)
				{
					return log;
				}
			}
			return null;
		}

		#endregion

		#region Auto Created Log

		public StmALog AutoCreatedLog
		{
			get { return autoCreatedLog != null && !autoCreatedLog.IsDeleted ? autoCreatedLog : null; }
		}

		public ZString AutoCreatedLogSL_ReferenceCache
		{
			get { return fAutoCreatedLogSL_ReferenceCache; }
			set { fAutoCreatedLogSL_ReferenceCache = value; }
		}

		/// <summary>
		/// Default SL_Reference to include on log entries. May be appended with CustomLogReference in BusinessObject
		/// </summary>
		public ZString AutoCreatedLogDefaultSL_Reference
		{
			get { return fAutoCreatedLogDefaultSL_Reference; }
			set { fAutoCreatedLogDefaultSL_Reference = value; }
		}

		internal StmALog CreateAutoAdminLog(bool forceModifiedLog = false)
		{
			var logTarget = Parent as IAutoAdminLogTarget;
			if (!IsCreateAutoAdminLogSuspended && logTarget != null)
			{
				var cancellable = Parent as ICancellable;
				if (!((BusinessObject)Parent).IsInDatabase && logTarget.IsAutoAdminBusinessObjectLoggerEnabled)
				{
					using (RaiseNonPersistedStmALogIfRequired())
					{
						return CreateAddLog();
					}
				}
				if (cancellable != null && cancellable.IsCancelledHasChanged && cancellable.IsCancelled)
				{
					return CreateRecordModifiedOrRemovedLog(Events.SetToInactive);
				}
				if (cancellable != null && cancellable.IsCancelledHasChanged && !cancellable.IsCancelled && IsLastLogAnInactiveLog)
				{
					return CreateRecordModifiedOrRemovedLog(Events.SetToActive);
				}
				if ((((BusinessObject)Parent).HasChanges || forceModifiedLog) && !HasAutoLogNotInDatabase(Events.EditedARecordCode) && logTarget.IsAutoAdminBusinessObjectLoggerEnabled)
				{
					using (RaiseNonPersistedStmALogIfRequired())
					{
						return CreateRecordModifiedOrRemovedLog(Events.EditedARecord);
					}
				}
			}
			return null;
		}

		bool HasAutoLogNotInDatabase(string eventCode)
		{
			ZString autoAddedReference = GetAutoCreatedLogReference();
			return LogsNotInDB.Any(x => !x.IsDeleted && x.SL_SE_NKEvent == eventCode && (string.IsNullOrWhiteSpace(x.SL_Reference) || x.SL_Reference.Trim().Equals(autoAddedReference)));
		}

		internal bool HasNonEmptyLogNotInDatabase(string eventCode)
		{
			return LogsNotInDB.Any(x => !x.IsDeleted && x.SL_SE_NKEvent == eventCode && !x.SL_Reference.IsEmpty);
		}

		internal ZBool IsLastLogAnInactiveLog
		{
			get
			{
				var result = ZBool.False;

				var mostRecentSetToInactiveLog = MostRecentLogByPostedTime(Events.SetToInactive);
				if (mostRecentSetToInactiveLog != null)
				{
					var mostRecentSetToActiveLog = MostRecentLogByPostedTime(Events.SetToActive);
					result = (mostRecentSetToActiveLog == null || mostRecentSetToInactiveLog.SL_PostedTimeUtc > mostRecentSetToActiveLog.SL_PostedTimeUtc);
				}

				return result;
			}
		}

		public StmALog MostRecentLogByPostedTime(Event @event)
		{
			var filter = GetRecentLogByPostedTimeQuery(@event);

			return FindWithFactoryIfNotLoaded(filter).FirstOrDefault();
		}

		public StmALog MostRecentLogByPostedTime(Event @event, Func<StmALog, bool> queryFunc)
		{
			return LogByPostedTime(@event, queryFunc, OrderDirection.Descending);
		}

		StmALog LogByPostedTime(Event @event, Func<StmALog, bool> queryFunc, OrderDirection orderDirection, bool useFactoryIfNotLoaded = true)
		{
			var filter = GetParentRelatedLogsFilter(StmALogSchema.Constants.SL_PostedTimeUtc, orderDirection);
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);
			filter.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;

			return FindWithFactoryIfNotLoaded(filter, useFactoryIfNotLoaded).FirstOrDefault(queryFunc);
		}

		ZQuery GetRecentLogByPostedTimeQuery(Event @event)
		{
			var filter = GetParentRelatedLogsFilter(StmALogSchema.Constants.SL_PostedTimeUtc);  // logs sorted by PostedTime DESC
			filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, @event.Code);
			filter.MaximumRows = 1;

			return filter;
		}

		ZString GetAutoCreatedLogReference()
		{
			ZString newReference;
			var logTarget = Parent as IAutoAdminLogTarget;
			if (Parent.IsDeleted || logTarget == null)
			{
				return ZString.Empty;
			}
			else if (!string.IsNullOrWhiteSpace(AutoCreatedLogSL_ReferenceCache))
			{
				newReference = AutoCreatedLogSL_ReferenceCache;
				AutoCreatedLogSL_ReferenceCache = string.Empty;
			}
			else
			{
				ZString separator = (AutoCreatedLogDefaultSL_Reference.IsEmpty || logTarget.CustomLogReferenceSuffix.IsEmpty) ? "" : " - ";
				newReference = new ZString(AutoCreatedLogDefaultSL_Reference + separator + logTarget.CustomLogReferenceSuffix).Trim();
				if (!newReference.IsEmpty)
				{
					newReference = Encoding.ASCII.GetString(Encoding.ASCII.GetBytes(newReference.Substring(0, StmALogSchema.SL_Reference.MaxLength)));
					AutoCreatedLogSL_ReferenceCache = newReference;
				}
			}
			return newReference;
		}

		internal void RemoveAutoAdminLog()
		{
			if (autoCreatedLog != null && !autoCreatedLog.IsInDatabase)
			{
				if (!autoCreatedLog.IsDeleted)
				{
					autoCreatedLog.Cancel(FireWorkflowMode.WithoutUpdatingRelatedFields);
				}
				RemoveAndDeleteWithoutLoading(autoCreatedLog);
				AutoCreatedLogSL_ReferenceCache = string.Empty;
			}
		}

		internal StmALog CreateAutoDeleteLog()
		{
			var logTarget = Parent as IAutoAdminLogTarget;
			if (logTarget != null && ((BusinessObject)Parent).IsInDatabase && logTarget.IsAutoAdminBusinessObjectLoggerEnabled)
			{
				using (RaiseNonPersistedStmALogIfRequired())
				{
					return CreateRecordModifiedOrRemovedLog(Events.DeletedARecordInTheSystem);
				}
			}
			return null;
		}

		internal void RemoveAutoDeleteLog()
		{
			RemoveAutoAdminLog();
		}

		StmALog CreateAddLog()
		{
			return autoCreatedLog = AddedLog;
		}

		StmALog CreateRecordModifiedOrRemovedLog(Event eventType)
		{
			return autoCreatedLog = AddNew(eventType, GetAutoCreatedLogReference());
		}

		internal IDisposable SuspendCreateAutoAdminLog()
		{
			return new Semaphore(this);
		}

		class Semaphore : IDisposable
		{
			public Semaphore(Logs parent)
			{
				this.parent = parent;
				parent.createAutoAdminLogSemaphore++;
			}
			readonly Logs parent;

			public void Dispose()
			{
				if (!isDisposed)
				{
					parent.createAutoAdminLogSemaphore--;
					isDisposed = true;
				}
			}
			bool isDisposed;
		}

		bool IsCreateAutoAdminLogSuspended
		{
			get { return createAutoAdminLogSemaphore > 0; }
		}

		int createAutoAdminLogSemaphore;
		StmALog autoCreatedLog;
		ZString fAutoCreatedLogDefaultSL_Reference;
		ZString fAutoCreatedLogSL_ReferenceCache;

		#endregion

		#region Events that Cannot be Added / Cancelled

		public EventCollection EventsThatCannotBeAdded
		{
			get
			{
				if (fEventsThatCannotBeAdded == null)
				{
					fEventsThatCannotBeAdded = new EventCollection();
				}
				return fEventsThatCannotBeAdded;
			}
		}

		public EventCollection EventsThatCannotBeCancelled
		{
			get
			{
				if (fEventsThatCannotBeCancelled == null)
				{
					fEventsThatCannotBeCancelled = new EventCollection();
				}
				return fEventsThatCannotBeCancelled;
			}
		}

		EventCollection fEventsThatCannotBeAdded;
		EventCollection fEventsThatCannotBeCancelled;

		#endregion

		#region ZLogsOrNotes Overrides

		protected override BusinessObject[] GetBusinessObjectsWithRelatedElements()
		{
			return Parent.BusinessObjectsWithRelatedEvents;
		}

		protected override SchemaColumn ElementForeignKeyColumn
		{
			get { return StmALogSchema.SL_Parent; }
		}

		protected override Type ElementType
		{
			get { return typeof(StmALog); }
		}

		protected override BusinessObjectFactory ElementsFactory
		{
			get { return Parent.LogsFactory; }
		}

		protected override ZQuery GetRelatedElementsFilter(BusinessObject relatedBizO)
		{
			return null;
		}

		protected override BusinessObjectCollection GetNewElementsCollection()
		{
			return new StmALogDependentCollection(Parent, ElementsFactory);
		}

		protected override BusinessObjectCollection GetNewAllElementsCollection()
		{
			return new StmALogCollectionWithRelatedElements(Parent);
		}

		protected override IBusinessObjectCollectionView GetNewVisibleElementsCollectionView()
		{
			return new StmALogCollectionView(Parent);
		}

		protected override BusinessObjectCollection GetElementsCollectionFromRelatedBizObject(BusinessObject relatedBizObject)
		{
			// TODO : this should *not* load hte elements as it is loaded in the base class!
			return ((IStmALogParent)relatedBizObject).Logs.ElementsInternal;
		}

		#endregion

		#region Created by User Initial

		public ZString CreatedByUserInitials
		{
			get { return (AddedLog == null) ? ZString.Empty : AddedLog.SL_GS_NKUser; }
		}

		public ZString CreatedByUserName
		{
			get { return (AddedLog == null || AddedLog.User == null) ? ZString.Empty : AddedLog.User.GS_FullName; }
		}

		#endregion

		#region Created Date Utc

		public ZDateTime CreatedDateUtc
		{
			get { return (AddedLog == null || AddedLog.SL_IsCancelled == ZBool.True) ? ZDateTime.Empty : AddedLog.SL_PostedTimeUtc; }
		}

		#endregion

		#region Last Modified by User  + Last Modified Date

		public ZString LastModifiedByUserName
		{
			get { return (LastEditLog == null) ? ZString.Empty : LastEditLog.SL_GS_NKUser; }
		}

		public ZString LastModifiedByUserFullName
		{
			get { return (LastEditLog == null || LastEditLog.User == null) ? ZString.Empty : LastEditLog.User.GS_FullName; }
		}

		public ZDateTime LastModifiedDateLocal
		{
			get { return LastModifiedDate.IsValid ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(LastModifiedDate.ToDateTime()) : ZDateTime.Empty; }
		}

		public ZDateTime LastModifiedDate
		{
			get { return (LastEditLog == null) ? ZDateTime.Empty : LastEditLog.SL_PostedTimeUtc; }
		}

		public ZPropertyInfo LastModifiedDateInfo
		{
			get { return GetZPropertyInfo(nameof(LastModifiedDate)); }
		}

		protected StmALog LastEditLog
		{
			get
			{
				var filter = new ZQuery(StmALogSchema.SL_Parent, Parent.LogsParentPK);
				filter.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;
				filter.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.EditedARecord.Code);
				filter.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
				filter.TableIndexHints.Add(new TableIndexHint(BaseStmALog.Schema.Indexes.NR_RX__SL_Parent_SL_SE_NKEvent_SL_EventTime));
				filter.IsForceSeek = true;

				return (StmALog)ElementsFactory.LoadTop1(ElementType, filter);
			}
		}

		#endregion

		#region User has/has not read related notes

		public bool HasUserReadNotes()
		{
			return HasUserReadNotes(StaticCurrentFetcher.Instance.CurrentUser);
		}

		public void MarkNotesAsRead()
		{
			MarkNotesAsRead(StaticCurrentFetcher.Instance.CurrentUser);
		}

		public void MarkNotesAsRead(IGlbStaff user)
		{
			var parent = (BusinessObject)Parent;
			if (parent.IsInDatabase && !ParentIsTemplateRecord)
			{
				// As we are calling Factory.Save, as usual it is necessary to wrap this operation with concurrency handling.
				ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
				{
					var factory = ElementsFactory.CreateNewFactory();
					var entry = factory.New<StmALog>();
					using (entry.LockForUpdatingKeyFields(true))
					{
						entry.SL_Parent = Parent.LogsParentPK;
						entry.SL_Table = Parent.LogsParentTableName;
						entry.SL_SE_NKEvent = Events.ReadRelatedNotes.Code;
						entry.SL_GS_NKUser = user.GS_Code;
					}
					factory.Save();
				}, () => { });
			}
			else
			{
				var entry = AddNew(Events.ReadRelatedNotes);
				entry.SL_GS_NKUser = user.GS_Code;
			}
		}

		public bool HasUserReadNotes(IGlbStaff user)
		{
			ZQuery query = new ZQuery(StmALogSchema.SL_Parent, Parent.LogsParentPK);

			query.FetchOnlyFromLocalCache = !((BusinessObject)Parent).IsInDatabase;
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.ReadRelatedNotes.Code);
			query.AddToFilter(StmALogSchema.SL_GS_NKUser, user.GS_Code);

			return ElementsFactory.LoadTop1(ElementType, query) != null;
		}

		bool ParentIsTemplateRecord
		{
			get { return Parent is ITemplateRecordProvider templateRecordProvider && templateRecordProvider.IsTemplateRecord; }
		}

		#endregion

		#region The Added Log

		public StmALog AddedLog
		{
			get
			{
				if (ObjectFactory.Get<IAuditStmALogDecider>().AuditLogConfigNonPersistedForEventCode(Parent, AutoEvents.AddedARecordToTheSystemCode))
				{
					if (Parent.IsInDatabase)
					{
						addedLog = null;
					}
					else if (Parent is IAutoAdminLogTarget logTarget && logTarget.IsAutoAdminBusinessObjectLoggerEnabled && (addedLog == null || addedLog.IsDeleted))
					{
						addedLog = NonPersistedAddedLogCreate();
					}
				}
				else if (addedLog == null || addedLog.IsDeleted)
				{
					addedLog = PersistedAddedLog_LoadOrCreate();
				}

				return addedLog;
			}
		}
		StmALog addedLog;

		#endregion

		StmALog NonPersistedAddedLogCreate()
		{
			using (RaiseNonPersistedStmALogIfRequired())
			{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				return AddNew(new EventValue(Events.AddedARecordToTheSystem, reference: GetAutoCreatedLogReference(), eventTime: new ZDateTimeOffset(((BusinessObject)Parent).InstantiationTime, DateTimeKind.Local)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		StmALog PersistedAddedLog_LoadOrCreate()
		{
			StmALog addedLog = null;
			if (Parent.IsInDatabase || Parent.Factory.GetBizOsForPK(Parent.LogsParentPK.ToGuid()).Length > 1)
			{
				var filter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
				filter.FetchOnlyFromLocalCache = !Parent.IsInDatabase;
				filter.AddToFilter(StmALogSchema.SL_Parent, Parent.LogsParentPK);
				addedLog = (StmALog)ElementsFactory.LoadTop1(ElementType, filter);
			}
			var logTarget = Parent as IAutoAdminLogTarget;
			if (logTarget != null && logTarget.IsAutoAdminBusinessObjectLoggerEnabled && addedLog == null)
			{
				if (!(Parent is StmNote stmNote && stmNote.IsParentTemplateRecord) && !((Parent as ITemplateRecordProvider)?.IsTemplateRecord ?? false))
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					addedLog = AddNew(new EventValue(Events.AddedARecordToTheSystem, reference: GetAutoCreatedLogReference(), eventTime: new ZDateTimeOffset(((BusinessObject)Parent).InstantiationTime, DateTimeKind.Local)));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
			return addedLog;
		}

		#region Change AddedLog

		public void ChangeAddedLog(ZDateTime eventTime)
		{
			ChangeAddedLog(eventTime, ZString.Empty);
		}

		public void ChangeAddedLog(ZString reference)
		{
			ChangeAddedLog(ZDateTime.Empty, reference);
		}

		public void ChangeAddedLog(ZDateTime eventTime, ZString reference)
		{
			using (AddedLog.LockForUpdatingKeyFields(true))
			{
				if (eventTime.IsValid)
				{
					this.AddedLog.SL_EventTime = eventTime;
				}

				if (reference.IsValid)
				{
					this.AddedLog.SL_Reference = reference;
				}
			}
		}

		#endregion

		#region Logs not in DB

		public StmALog[] LogsNotInDB
		{
			get { return (StmALog[])ElementsNotInDB.ToArray(typeof(StmALog)); }
		}

		#endregion

		#region Getting / Searching for Logs

		public StmALogDependentCollection GetAllLogs()
		{
			return (StmALogDependentCollection)ElementsInternal;
		}

		#endregion

		#region GetAllLogsByEvent

		/// <summary>
		/// Loads all logs for a given Event order by SL_PostedTimeUtc DESC.
		/// Cancelled events are not retrieved by this method
		/// </summary>
		public IEnumerable<StmALog> GetAllLogsByEventOrderByPostedTimeUtcDESC(params Event[] @events)
		{
			var filter = GetParentRelatedLogsFilter(StmALogSchema.Constants.SL_PostedTimeUtc);
			filter.AddToFilter(JoinCondition.And, StmALogSchema.SL_SE_NKEvent, @events.Select(e => e.Code));
			return FindWithFactoryIfNotLoaded(filter);
		}

		#endregion

		#region ILogsInternals

		StmALog ILogsInternals.AddNew(Type logType)
		{
			return (StmALog)AddNew(logType);
		}

		void ILogsInternals.ReloadFromDB()
		{
			ReloadFromDB();
		}

		#endregion

		#region Implementation

		new IStmALogParent Parent
		{
			get { return (IStmALogParent)base.Parent; }
		}

		#endregion

		public bool HasLogWith(SchemaColumn column, object value)
		{
			var query = new ZQuery(column, value);
			return HasLogWith(query);
		}

		public bool HasLogWith(ZQuery filter)
		{
			return Find(filter).Length > 0;
		}

		public bool HasLogWith(Func<StmALog, bool> filter)
		{
			return Find(filter).Any();
		}

		IDisposable RaiseNonPersistedStmALogIfRequired()
		{
			if (checkForNonPersistedStmALogs)
			{
				return null;
			}

			checkForNonPersistedStmALogs = true;
			return new DisposableAction(() => checkForNonPersistedStmALogs = false);
		}
		bool checkForNonPersistedStmALogs;

		bool ShouldCreateNonPersistedStmALog(string eventCode)
		{
			return checkForNonPersistedStmALogs && ObjectFactory.Get<IAuditStmALogDecider>().AuditLogConfigNonPersistedForEventCode(base.Parent, eventCode);
		}
	}
}
