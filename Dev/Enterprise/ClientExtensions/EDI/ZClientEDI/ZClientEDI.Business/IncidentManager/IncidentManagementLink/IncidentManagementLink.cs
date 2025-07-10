using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EventReference;
using CargoWise.Types;
using CargoWise.Workflow;
using Enterprise.BufferManagement.Business;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Client.EDI.IncidentManager.Business.SupportIncidentLookups;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementLink : AutoIncidentManagementLink, IPivotBusinessObject
	{
		public IncidentManagementLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IncidentManagementGroup IncidentManagementGroup
		{
			get
			{
				if (incidentManagementGroup == null && !INL_ING_Group.IsEmpty)
				{
					incidentManagementGroup = Factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, INL_ING_Group));
				}
				return incidentManagementGroup;
			}

			private set
			{
				incidentManagementGroup = value;
			}
		}
		IncidentManagementGroup incidentManagementGroup;

		public SupportIncident SupportIncident
		{
			get
			{
				if (supportIncident == null && !INL_IM_Incident.IsEmpty)
				{
					supportIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, INL_IM_Incident));
				}
				return supportIncident;
			}

			private set
			{
				supportIncident = value;
			}
		}
		SupportIncident supportIncident;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new IncidentManagementLinkFetchStrategy(this);
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!IsInDatabase)
			{
				CascadeGroupParameters();
				CascadeTriage();
			}

			if ((!IsInDatabase || INL_IsGroupControlledInfo.HasChanges) && IncidentManagementGroup != null && SupportIncident != null && IsControlled)
			{
				IncidentManagementGroup.SetWorkItemsCascadeToIncident(this);
			}
		}

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				AttachLog();
			}

			if (INL_IsGroupControlled && (!IsInDatabase || INL_IsGroupControlledInfo.HasChanges))
			{
				CancelTasksAddLogsAndUpdateStatusOnIncident();
			}

			BroadcastMessage();

			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				LastCommunicationInfo.RefreshBinding();
				HasCancelledTasksAndUpdatedIncidentStatus = false;
			}
		}

		void AttachLog()
		{
			var paramList = new List<KeyValuePair<string, string>>();

			var desc = FormattableString.Invariant(
						$"Incident {SupportIncident.Number} added to Incident Group {IncidentManagementGroup.Number}"
					   );
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Description, desc));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, SupportIncident.Number));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, IncidentManagementGroup.Number));
			paramList.Add(new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, IncidentManagementGroup.Number));
			IncidentManagementGroup.Logs.AddNew(
				AutoEvents.Attached,
				paramList.ToArray()
			);

			SupportIncident.Logs.AddNew(
				AutoEvents.Attached,
				paramList.ToArray()
			);
		}

		#region BroadcastMessage

		void BroadcastMessage()
		{
			if (IsInDatabase)
			{
				return;
			}

			if (!CanReceiveBroadcastMessages)
			{
				return;
			}

			IncidentManagementGroup.SendBroadcastMessage(this, ignoreInterimMessage: true);
		}

		public bool CanReceiveBroadcastMessages
		{
			get
			{
				return !IncidentManagementGroup.ING_IsBroadcastToControlledOnly || IsControlled;
			}
		}
		#endregion

		public void CascadeGroupParameters()
		{
			if (IncidentManagementGroup.CascadeProductDetails)
			{
				StoreStatusChanges();
				SupportIncident.Reload();

				using (SupportIncident.DisableIncidentEventFactory())
				{
					SupportIncident.SetCriticalityWithoutLoggingReason(IncidentManagementGroup.ING_Priority);
					SupportIncident.IM_Product = IncidentManagementGroup.ING_Product;
					SupportIncident.IM_ProgramArea = IncidentManagementGroup.ING_ProductArea;
					SupportIncident.IM_Module = IncidentManagementGroup.ING_Module;
					SupportIncident.IM_ServiceType = IncidentManagementGroup.ING_ServiceType;
					SupportIncident.IM_SourceModuleId = IncidentManagementGroup.ING_SourceModuleId;
				}

				RestoreStatusChanges();
			}
		}

		public void CascadeTriage()
		{
			if (IncidentManagementGroup.ING_IMT_Triage != Guid.Empty &&
				IncidentManagementGroup.ING_IMT_Triage != SupportIncident.IM_IMT_Triage)
			{
				SupportIncident.IM_IMT_Triage = IncidentManagementGroup.ING_IMT_Triage;
			}
		}

		void StoreStatusChanges()
		{
			if (SupportIncident.IM_StatusInfo.HasChanges)
			{
				storedStatus = SupportIncident.IM_Status;
			}

			if (SupportIncident.IM_CategoryInfo.HasChanges)
			{
				storedCategory = SupportIncident.IM_Category;
			}

			if (SupportIncident.IM_ResolutionCodeInfo.HasChanges)
			{
				storedResolutionCode = SupportIncident.IM_ResolutionCode;
			}
		}

		ZString storedStatus;
		ZString storedCategory;
		ZString storedResolutionCode;

		void RestoreStatusChanges()
		{
			if (!string.IsNullOrEmpty(storedStatus))
			{
				SupportIncident.IM_Status = storedStatus;
			}

			if (!string.IsNullOrEmpty(storedCategory))
			{
				SupportIncident.IM_Category = storedCategory;
			}

			if (!string.IsNullOrEmpty(storedResolutionCode))
			{
				SupportIncident.IM_ResolutionCode = storedResolutionCode;
			}
		}

		#region Table Properties

		[RelatedBusinessObject(nameof(Business.IncidentManagementGroup))]
		public override ZGuid INL_ING_Group
		{
			get => base.INL_ING_Group;
			set
			{
				base.INL_ING_Group = value;
				IncidentManagementGroup = Factory.LoadTop1<IncidentManagementGroup>(new ZQuery(IncidentManagementGroupSchema.PK, value));
			}
		}

		[RelatedBusinessObject(nameof(Business.SupportIncident))]
		public override ZGuid INL_IM_Incident
		{
			get => base.INL_IM_Incident;
			set
			{
				base.INL_IM_Incident = value;
				SupportIncident = Factory.LoadTop1<SupportIncident>(new ZQuery(IncidentMainSchema.PK, value));
			}
		}

		public ZGuid Relation1ID { get => INL_ING_Group; set => INL_ING_Group = value; }

		public BusinessObject Relation1Object => IncidentManagementGroup;

		public ZGuid Relation2ID { get => INL_IM_Incident; set => INL_IM_Incident = value; }

		public BusinessObject Relation2Object => SupportIncident;

		public ZDateTime SystemCreateTimeLocal => INL_SystemCreateTimeUtc.ToLocalBranchTime(Factory);

		public ZPropertyInfo SystemCreateTimeLocalInfo
		{
			get { return GetZPropertyInfo(nameof(SystemCreateTimeLocal)); }
		}

		#endregion

		public JobConversationMessage LastCustomerMessageReceived
		{
			get
			{
				var conversation = SupportIncident?.EConversation.ExistingConversation;
				if (conversation != null)
				{
					return conversation.Messages
						.Where(x => !x.JCM_JCP_Participant.IsEmpty && x.Sender.JCP_ParticipantTableCode == OrgContactSchema.Constants.Prefix)
						.OrderByDescending(x => x.JCM_PostedTimeUtc).FirstOrDefault();
				}
				return null;
			}
		}

		ZString LastCustomerMessage => LastCustomerMessageReceived == null ? ZString.Empty : LastCustomerMessageReceived.Body;

		#region Flagged

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(Flagged_ReadOnly))]
		public ZBool Flagged
		{
			get
			{
				if (LastCustomerMessageReceived != null)
				{
					return LastCustomerMessageReceived.JCM_PostedTimeUtc > LastUnflaggedEventDateTime;
				}
				else
				{
					return false;
				}
			}
			set
			{
				if (value != Flagged)
				{
					if (value)
					{
						var unFlaggedEvents = IncidentManagementGroup.Logs.Find(UnflaggedEventSearchQuery);
						foreach (var nowEvent in unFlaggedEvents)
						{
							CancelOrDeleteUnflaggedEvent(IncidentManagementGroup, nowEvent);
						}

						var unFlaggedEventsInLink = this.Logs.Find(UnflaggedEventSearchQuery);
						foreach (var nowEvent in unFlaggedEventsInLink)
						{
							CancelOrDeleteUnflaggedEvent(this, nowEvent);
						}
						ClearLastUnflaggedEvent();
					}
					else
					{
						CreateOrUpdateUnflaggedEvent();
						UpdateStatusOnReopenedIncidentIfRequired();
					}
				}
			}
		}

		public void UpdateStatusOnReopenedIncidentIfRequired()
		{
			if (StatusAndResolutionCodeChanged && IsControlled && SupportIncident.RelatedWorkItems.Cast<WorkItem>().All(x => x.IsClosedOrCancelled))
			{
				SupportIncident.IM_Status = PreviousStatus;
				SupportIncident.IM_ResolutionCode = PreviousResolutionCode;
				var invalidEvents = SupportIncident.Logs.Find(InvalidEventSearchQuery);
				foreach (var nowEvent in invalidEvents)
				{
					CancelOrDeleteUnflaggedEvent(this, nowEvent);
				}
			}
		}

		public ZPropertyInfo FlaggedInfo => GetZPropertyInfo(nameof(Flagged));

		StmALog PreviousStatusLog => SupportIncident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousStatus: "));
		StmALog PreviousResolutionCodeLog => SupportIncident.Logs.GetAllLogs().Cast<StmALog>().FirstOrDefault(l => !l.SL_IsCancelled && l.SL_SE_NKEvent == AutoEvents.StatusChange.Code && l.SL_Reference.StartsWith("previousResolutionCode: "));
		public bool StatusAndResolutionCodeChanged => PreviousStatusLog != null && PreviousResolutionCodeLog != null;
		ZString PreviousStatus => PreviousStatusLog.SL_Reference.Substring(PreviousStatusLog.SL_Reference.Length - 3, 3);
		ZString PreviousResolutionCode => PreviousResolutionCodeLog.SL_Reference.Substring(PreviousResolutionCodeLog.SL_Reference.Length - 3, 3);

		bool lastUnflaggedEventLoaded;
		protected StmALog LastUnflaggedEvent
		{
			get
			{
				if (lastUnflaggedEvent == null && !lastUnflaggedEventLoaded)
				{
					var query = UnflaggedEventSearchQuery;
					query.MaximumRows = 1;
					lastUnflaggedEvent = IncidentManagementGroup.Logs.Find(query).FirstOrDefault();
					lastUnflaggedEventLoaded = true;
				}
				return lastUnflaggedEvent;
			}
			set
			{
				lastUnflaggedEvent = value;
			}
		}
		StmALog lastUnflaggedEvent;

		internal ZQuery UnflaggedEventSearchQuery
		{
			get
			{
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChange.Code);
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
				query.AddToFilter(StmALogSchema.SL_Reference, GetUnFlaggedEventReferenceText(SupportIncident));
				query.OrderBy = $"{StmALogSchema.SL_PostedTimeUtc.Name} DESC";
				return query;
			}
		}

		internal ZQuery InvalidEventSearchQuery
		{
			get
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, SupportIncident.PK);
				query.AddToFilter(StmALogSchema.SL_SE_NKEvent, AutoEvents.StatusChange.Code);

				var subQuery = new ZQuery();
				subQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "previousStatus: ");
				subQuery.AddToFilter(JoinCondition.Or, StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, "previousResolutionCode: ");

				query.AddToFilter(subQuery);
				return query;
			}
		}

		public ZDateTime LastUnflaggedEventDateTime => LastUnflaggedEvent == null ? ZDateTime.MinSmallDateTimeValue : LastUnflaggedEvent.SL_PostedTimeUtc;

		internal void CreateOrUpdateUnflaggedEvent()
		{
			var unflaggedEventValue = new EventValue(Events.MessageStatusChange, reference: GetUnFlaggedEventReferenceText(SupportIncident), eventTime: ZDateTimeOffset.Now);

			LastUnflaggedEvent = IncidentManagementGroup.Logs.CreateRecreateOrUpdateEventLog(unflaggedEventValue, LastUnflaggedEvent);
			var linkEvents = this.Logs.CreateRecreateOrUpdateEventLog(unflaggedEventValue, this.Logs.Find(UnflaggedEventSearchQuery).FirstOrDefault());

			LastUnflaggedEvent.IsCancelled = false;
			linkEvents.IsCancelled = false;
		}

		public void ClearLastUnflaggedEvent()
		{
			LastUnflaggedEvent = null;
			lastUnflaggedEventLoaded = false;
		}

		internal static ZString GetUnFlaggedEventReferenceText(SupportIncident incident)
		{
			var unflaggedLogBuilder = EventLogReferenceBuilder.New()
				.AddMandatory(Constants.EventReferenceParameters.Codes.ReferenceNumber, incident.Number)
				.AddMandatory(Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Message unflagged on {incident.Number}"));
			return unflaggedLogBuilder.Build();
		}

		void CancelOrDeleteUnflaggedEvent(EnterpriseBusinessObject parent, StmALog eventLog)
		{
			if (!eventLog.IsInDatabase)
			{
				parent.Logs.Factory?.EnqueueDelete(typeof(StmALog), eventLog.PK);
			}
			else
			{
				eventLog.IsCancelled = true;
			}
		}

		public ZBool Flagged_ReadOnly => LastCustomerMessageReceived == null;

		public ZString CustomerWaitingTime
		{
			get
			{
				if (!IsCustomerWaitingForResponse)
				{
					return string.Empty;
				}

				var currentWaitingTime = ZDateTime.Now - LastCustomerMessageReceived.SendLocalDateTime;

				return FormattableString.Invariant($"{currentWaitingTime.Days}Day(s) {currentWaitingTime.Hours}Hour(s) {currentWaitingTime.Minutes}Min");
			}
		}

		public ZPropertyInfo CustomerWaitingTimeInfo => GetZPropertyInfo(nameof(CustomerWaitingTime));

		public bool IsCustomerWaitingForResponse => LastCustomerMessageReceived != null && (LastUnflaggedEvent == null || LastCustomerMessageReceived.SystemCreateTimeInUtc > LastUnflaggedEvent.SL_EventTimeUtc);

		#endregion

		public void CancelTasksAddLogsAndUpdateStatusOnIncident()
		{
			if (IsControlled && SupportIncident != null && !HasCancelledTasksAndUpdatedIncidentStatus)
			{
				var currentTaskList = new List<Tuple<SupportIncidentProcessTask, string>>();
				var cancellationKey = Guid.NewGuid();

				foreach (var incidentTask in SupportIncident.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>())
				{
					if (!incidentTask.IsClosed)
					{
						if (incidentTask.AssignedStaffMember != null && incidentTask.IsCurrent)
						{
							currentTaskList.Add(new Tuple<SupportIncidentProcessTask, string>(incidentTask, incidentTask.P9_Status));
						}

						var incidentLogBuilder = EventLogReferenceBuilder.New()
							.AddMandatory(Constants.EventReferenceParameters.Codes.JobNumber, IncidentManagementGroup.ING_IncidentGroupNumber)
							.AddMandatory(Constants.EventReferenceParameters.Codes.Assigned, incidentTask.P9_GS_NKAssignedStaffMember)
							.AddMandatory(Constants.EventReferenceParameters.Codes.TaskCode, incidentTask.P9_TaskID)
							.AddMandatory(Constants.EventReferenceParameters.Codes.Status, incidentTask.P9_Status)
							.AddMandatory(Constants.EventReferenceParameters.Codes.Type, "Task")
							.AddMandatory(Constants.EventReferenceParameters.Codes.Argument, cancellationKey.ToString())
							.AddMandatory(Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Task cancelled by {IncidentManagementGroup.ING_IncidentGroupNumber}"));

						SupportIncident.Logs.AddLog(Events.Cancelled, incidentLogBuilder.Build());

						if (incidentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Working || incidentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)
						{
							incidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
						}
						else if (incidentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Open || incidentTask.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
						{
							incidentTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
						}
					}
				}

				if (currentTaskList.Any())
				{
					var earliestSequenceCurrentTaskStatusPairs = currentTaskList.GroupBy((taskStatusPair) => taskStatusPair.Item1.P9_Sequence).FirstOrDefault();
					foreach (var taskStatusPair in earliestSequenceCurrentTaskStatusPairs)
					{
						var groupLogBuilder = EventLogReferenceBuilder.New()
								.AddMandatory(Constants.EventReferenceParameters.Codes.ReferenceNumber, SupportIncident.IM_IncidentNumber)
								.AddMandatory(Constants.EventReferenceParameters.Codes.Assigned, taskStatusPair.Item1.P9_GS_NKAssignedStaffMember)
								.AddMandatory(Constants.EventReferenceParameters.Codes.TaskCode, taskStatusPair.Item1.P9_TaskID)
								.AddMandatory(Constants.EventReferenceParameters.Codes.Status, taskStatusPair.Item2)
								.AddMandatory(Constants.EventReferenceParameters.Codes.Argument, cancellationKey.ToString())
								.AddMandatory(Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Control enabled for incident {SupportIncident.IM_IncidentNumber}"));
						IncidentManagementGroup.Logs.AddNew(Events.LockForEdit, groupLogBuilder.Build());
						SupportIncident.Logs.AddNew(Events.LockForEdit, groupLogBuilder.Build());
					}
				}
				else
				{
					var logBuilder = EventLogReferenceBuilder.New()
						.AddMandatory(Constants.EventReferenceParameters.Codes.ReferenceNumber, SupportIncident.IM_IncidentNumber)
						.AddMandatory(Constants.EventReferenceParameters.Codes.Description, FormattableString.Invariant($"Control enabled for incident {SupportIncident.IM_IncidentNumber}"));
					IncidentManagementGroup.Logs.AddNew(Events.LockForEdit, logBuilder.Build());
					SupportIncident.Logs.AddNew(Events.LockForEdit, logBuilder.Build());
				}

				UpdateStatusOnIncidentIfRequired();
				HasCancelledTasksAndUpdatedIncidentStatus = true;
			}
		}

		public void UpdateStatusOnIncidentIfRequired()
		{
			var incidentHasAttachedWorkItem = SupportIncident.RelatedWorkItems.Any();
			var hasAnyWorkItemsInProgress = SupportIncident.RelatedWorkItems.Cast<WorkItem>().Any(x => IsWorkItemInProgress(x.WKI_Status));
			var areAllWorkItemsClosed = !hasAnyWorkItemsInProgress && SupportIncident.RelatedWorkItems.Cast<WorkItem>().All(x => x.WKI_Status == ProcessTaskStatusCodeList.Codes.Closed);
			var isIncidentCompleted = IncidentManagementGroup.IncidentCompleted;

			if (!incidentHasAttachedWorkItem && !isIncidentCompleted)
			{
				SupportIncident.IM_Status = IncidentMainLookups.Status.Working;
				SupportIncident.IM_ResolutionCode = DispositionList.Constants.Working.WorkInProgress;
			}
			else if (incidentHasAttachedWorkItem && areAllWorkItemsClosed && !isIncidentCompleted)
			{
				SupportIncident.IM_Status = IncidentMainLookups.Status.Working;
			}
			else if (!incidentHasAttachedWorkItem && isIncidentCompleted)
			{
				SupportIncident.IM_Status = IncidentMainLookups.Status.Closed;
				SupportIncident.IM_ResolutionCode = DispositionList.Constants.Closed.ClosedByIncidentGroup;
				SupportIncident.SetIncidentClosedDate();
			}
			else if (incidentHasAttachedWorkItem && areAllWorkItemsClosed && isIncidentCompleted)
			{
				SupportIncident.IM_Status = IncidentMainLookups.Status.Working;
			}

			if (incidentHasAttachedWorkItem)
			{
				SupportIncident.SetIncidentStageWithoutReason(SupportIncidentCategoriesList.Codes.Defect);
			}
			else
			{
				SupportIncident.SetIncidentStageWithoutReason(SupportIncidentCategoriesList.Codes.Support);
			}
		}

		public void ReopenIncidentTasksClosedByGroup(string appendNote = "")
		{
			var lckSearchQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.LockForEdit.Code).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, SupportIncident.IM_IncidentNumber);
			lckSearchQuery.OrderBy = $"{StmALogSchema.SL_PostedTimeUtc.Name} DESC";

			var lckEvents = IncidentManagementGroup?.Logs.Find(lckSearchQuery);
			var lckCancellationKey = ZGuid.Invalid;
			if (lckEvents?.FirstOrDefault() != null && (!lckEvents?.FirstOrDefault().SL_IsCancelled ?? false))
			{
				if (StmALog.GetParametersFromReference(lckEvents.FirstOrDefault().SL_Reference).GetValueSafe(Constants.EventReferenceParameters.Codes.Argument) is string key)
				{
					try
					{
						lckCancellationKey = new ZGuid(key);
					}
					catch (Exception ex)
					{
						lckCancellationKey = ZGuid.Invalid;
						ErrorReporter.ReportOnce("The cancellation key can not be loaded from LCK event.", $"Event PK:{lckEvents.FirstOrDefault().PK}\r\nEvent Reference:\r\n{lckEvents.FirstOrDefault().SL_Reference}", ex);
					}
				}
			}
			else
			{
				lckEvents?.ForEach(x => x.Cancel());
			}

			var cncSearchQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.Cancelled.Code).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, IncidentManagementGroup.Number).
				AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, $"{Constants.EventReferenceParameters.Codes.Type}=Task");
			cncSearchQuery.OrderBy = $"{StmALogSchema.SL_PostedTimeUtc.Name} DESC";
			if (lckCancellationKey.IsValid)
			{
				cncSearchQuery.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, lckCancellationKey.ToString());
			}

			var cncEvents = SupportIncident?.Logs.Find(cncSearchQuery);
			if (!cncEvents?.FirstOrDefault()?.SL_IsCancelled ?? false)
			{
				if (cncEvents != null && cncEvents.Length != 0)
				{
					foreach (var cncEvent in cncEvents.Where(x => !x.SL_IsCancelled))
					{
						var informations = StmALog.GetParametersFromReference(cncEvent.SL_Reference);
						var taskID = informations.GetValueSafe(Constants.EventReferenceParameters.Codes.TaskCode) ?? string.Empty;
						var taskStatus = informations.GetValueSafe(Constants.EventReferenceParameters.Codes.Status) ?? string.Empty;
						var assignedStaff = informations.GetValueSafe(Constants.EventReferenceParameters.Codes.Assigned) ?? string.Empty;
						var expectedTask = (!string.IsNullOrEmpty(taskID) && !string.IsNullOrEmpty(taskStatus)) ? SupportIncident?.WorkflowItems.Tasks.Cast<SupportIncidentProcessTask>().FirstOrDefault(x => x.P9_TaskID == taskID) : null;
						EvaluateTask(taskID, taskStatus, expectedTask, appendNote, assignedStaff, cncEvent);
					}
				}
			}

			lckEvents?.ForEach(x => x.Cancel());
			cncEvents?.ForEach(x => x.Cancel());
		}

		void EvaluateTask(String taskID, String taskStatus, SupportIncidentProcessTask expectedTask, string appendNote, string assignedStaff, StmALog cncEvent)
		{
			if (!string.IsNullOrEmpty(taskID) && !string.IsNullOrEmpty(taskStatus))
			{
				if (expectedTask != null)
				{
					expectedTask.P9_Status = taskStatus;
					expectedTask.AppendNote(string.IsNullOrEmpty(appendNote) ? "Task reopened as Incident is no longer controlled by Incident Group" : appendNote);
					if (!string.IsNullOrEmpty(assignedStaff) && Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, assignedStaff) != null)
					{
						expectedTask.P9_GS_NKAssignedStaffMember = assignedStaff;
					}
				}
				else
				{
					var ex = new ArgumentException($"{nameof(expectedTask)} can not be null", nameof(expectedTask));
					ErrorReporter.ReportOnce("The expected task can not be loaded according to task ID", $"Event PK:{cncEvent.PK}\r\nEvent Reference\r\n{cncEvent.SL_Reference}", ex);
				}
			}
			else
			{
				var ex = new ArgumentException((string.IsNullOrEmpty(taskID) ? nameof(taskID) : nameof(taskStatus)) + " can not be empty", nameof(taskID));
				ErrorReporter.ReportOnce("The expected task or task status can not be loaded from CNC events", $"Task ID is empty: {string.IsNullOrEmpty(taskID)}\r\nTask status is empty: {string.IsNullOrEmpty(taskID)}\r\nEvent PK:{cncEvent.PK}\r\nEvent Reference\r\n{cncEvent.SL_Reference}", ex);
			}
		}

		bool HasCancelledTasksAndUpdatedIncidentStatus { get; set; }

		bool IsWorkItemInProgress(string workItemStatus)
		{
			return workItemStatus == ProcessTaskStatusCodeList.Codes.Working
				|| workItemStatus == ProcessTaskStatusCodeList.Codes.Assigned
				|| workItemStatus == ProcessTaskStatusCodeList.Codes.Open;
		}

		public ZBool IsControlled
		{
			get { return INL_IsGroupControlled && (IncidentManagementGroup?.ControlIncidents ?? false); }
		}

		public ZPropertyInfo IsControlledInfo
		{
			get { return GetZPropertyInfo(nameof(IsControlled)); }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			INL_IM_Incident = Factory.NewWithValidTestData<SupportIncident>().PK;
			INL_ING_Group = Factory.NewWithValidTestData<IncidentManagementGroup>().PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
		}
#endif

		public ZString LastCommunication
		{
			get
			{
				var latestCommunicationLog = IncidentManagementGroup.Logs.Find(x =>
						(
							x.SL_SE_NKEvent == AutoEvents.MessageReceivedCode
							|| x.SL_SE_NKEvent == AutoEvents.MessageSentCode
							|| x.SL_SE_NKEvent == AutoEvents.BroadcastMessageCode
						)
						&& x.Parameters.Contains(new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, SupportIncident.IM_IncidentNumber)))
					.OrderByDescending(x => x.SL_PostedTimeUtc).FirstOrDefault();

				if (latestCommunicationLog == null)
				{
					return string.Empty;
				}

				var eventDescription = ((Event)latestCommunicationLog.Event).Description;
				return latestCommunicationLog.Parameters.ContainsKey(EventReferenceParameters.Codes.MessageType)
					? FormattableString.Invariant($"{eventDescription} - {latestCommunicationLog.Parameters[EventReferenceParameters.Codes.MessageType]}")
					: eventDescription;
			}
		}

		public ZPropertyInfo LastCommunicationInfo
		{
			get { return GetZPropertyInfo(nameof(LastCommunication)); }
		}

		public void TrySendGroupAutoReply()
		{
			if (IncidentManagementGroup != null && IncidentManagementGroup.ING_IsAutoReply && SupportIncident.IsGroupControlled && this.IsControlled)
			{
				var message = IncidentManagementGroup.IncidentManagementGroupMessages.Cast<IncidentManagementGroupMessage>()
								.FirstOrDefault(m => m.IGM_IsPublished && m.IGM_Type == IncidentManagementGroupMessageTypePairList.Codes.AutoReply);

				if (message != null && !SupportIncident.EConversation.AnyLocalMessageContains(message.IGM_Message))
				{
					SupportIncident.AddStaffMessageToCustomer(message.IGM_Message, false);
					AddAutoReplyEvent();
				}
			}
		}

		internal void AddAutoReplyEvent()
		{
			IncidentManagementGroup.AddIncidentMessageSentEvent(SupportIncident, IncidentManagementGroupMessageTypePairList.Codes.AutoReply, FormattableString.Invariant($"Replied on {SupportIncident.IM_IncidentNumber} via AUTO"));
		}

		public new class Schema : AutoIncidentManagementLink.Schema
		{
			public const string CustomerWaitingTime = nameof(IncidentManagementLink.CustomerWaitingTime);
		}

		public override GlbStaff Responder
		{
			get
			{
				if (base.Responder != null)
				{
					return base.Responder;
				}

				var groupOwner = Factory.Load<IncidentManagementGroup>(INL_ING_Group)?.ING_GS_NKGroupOwner ?? ZString.Empty;
				return string.IsNullOrEmpty(groupOwner) ? null : Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, groupOwner);
			}
		}
	}
}
