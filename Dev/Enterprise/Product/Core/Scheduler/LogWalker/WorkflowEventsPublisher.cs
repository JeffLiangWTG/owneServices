using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	[Serializable]
	public class WorkflowEventsPublisher : LogSubscriber
	{
		public override string Name
		{
			get { return "WorkflowEventPublish"; }
		}

		public override string[] EventTypes => Array.Empty<string>();

		public override string[] TableNames => Array.Empty<string>();

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			var subscriptionsQuery = new ZQuery(StmEventSubscriptionSchema.PK, queuedLogs.Select(q => q.SJ_TargetID));
			var subscriptions = queuedLogs[0].Factory.Load<StmEventSubscription>(subscriptionsQuery);
			var eventsToFire = new List<EventWithLogParentsToFire>();

			foreach (var log in queuedLogs)
			{
				var subscription = subscriptions.FirstOrDefault(s => s.PK == log.SJ_TargetID);
				if (subscription == null)
				{
					continue;
				}

				var agent = WorkflowDescriptors.Instance.TryGetValueSafe(subscription.SES_AgentDescriptor) as IEventSubscriptionAgent;
				if (agent == null)
				{
					continue;
				}

				var evnt = PublishedEvent.Create(log);
				var logParentsToFireWorkflow = agent.GetLogParentsToFireWorkflowForEvent(subscription, evnt).ToArray();
				var eventToFire = new EventWithLogParentsToFire(evnt, logParentsToFireWorkflow);

				AddProcessTasksFetchHints(eventToFire);
				eventsToFire.Add(eventToFire);
			}

			foreach (var eventToFire in eventsToFire)
			{
				var evnt = eventToFire.Event;

				using (EventRecursionHandler.WithEventRecursionDetection())
				{
					foreach (var logParent in eventToFire.LogParents)
					{
						new WorkflowGun(evnt, FireWorkflowMode.UpdateAll).TriggerWorkflow(logParent);
					}
				}
			}
		}

		static void AddProcessTasksFetchHints(EventWithLogParentsToFire eventWithLogParents)
		{
			var evnt = eventWithLogParents.Event;

			foreach (var logParent in eventWithLogParents.LogParents)
			{
				var milestoneOrTriggerQuery = new ZQuery(ProcessTasksSchema.P9_Type, new[] { Core.Constants.Workflow.MilestoneType, Core.Constants.Workflow.WorkflowTriggerType });

				var processTasksQuery = new ZQuery();
				processTasksQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, logParent.LogsParentPK);
				processTasksQuery.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, evnt.SL_SE_NKEvent);
				processTasksQuery.AddToFilter(milestoneOrTriggerQuery);
				processTasksQuery.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, ZString.Empty);

				var activeProcessTasksQuery = new ZQuery();

				var activeProcessTasksSubquery = new ZQuery();
				activeProcessTasksSubquery.AddToFilter(ProcessTasksSchema.P9_ParentID, logParent.LogsParentPK);
				activeProcessTasksSubquery.AddToFilter(milestoneOrTriggerQuery);
				activeProcessTasksSubquery.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, ZString.Empty);
				activeProcessTasksQuery.AddToFilter(activeProcessTasksSubquery);

				activeProcessTasksQuery.AddToFilter(ProcessTasksSchema.P9_EstimatedDefaultedFrom, Events.SetToActiveCode);
				activeProcessTasksQuery.AddToFilter(ProcessTasksSchema.P9_EstimatedDefaultFromPredecessor, 1);
				activeProcessTasksQuery.AddToFilter(ProcessTasksSchema.P9_ScheduledDateUtc, null);

				evnt.Factory.AddFetchHint(typeof(ProcessTask), processTasksQuery);
				evnt.Factory.AddFetchHint(typeof(ProcessTask), activeProcessTasksQuery);
			}
		}

		protected internal override ZQuery GetFeedbackLogFilter(IQueuedLog[] queuedLogs)
		{
			var result = base.GetFeedbackLogFilter(queuedLogs);
			result.AddToFilter(StmALogSchema.SL_Parent, queuedLogs.Select(o => o.SJ_ParentID).Distinct());

			return result;
		}

		#region Types

		public class PublishedEvent : NonPersistentBusinessObject, IStmALog
		{
			PublishedEvent(BusinessObjectFactory factory, ZGuid pk) : base(factory)
			{
				SL_PK = pk;
			}

			public ZGuid SL_PK { get; }
			public IDictionary<string, string> Parameters { get; private set; }
			public ZDateTime SL_EventTime { get; private set; }
			public ZDateTime SL_EventTimeUtc { get; private set; }
			public ZDateTime SL_PostedTimeUtc { get; private set; }

			public ZDateTimeOffset SL_EventTimeOffset => StmALog.ToDateTimeOffset(Factory, SL_EventTime, SL_EventTimeUtc, SL_GB_NKBranch);
			public ZString SL_GS_NKUser { get; private set; }
			public ZBool SL_IsEstimate { get; private set; }
			public ZBool SL_IsCancelled { get; private set; }
			public ZGuid SL_Parent { get; private set; }
			public ZString SL_Reference { get; private set; }
			public ZString SL_ReferenceForBinding => StmALog.GetReferenceForBinding(Master, SL_Reference);
			public ZString SL_SE_NKEvent { get; private set; }
			public ZString SL_Table { get; private set; }
			public ZString SL_TableFriendlyName => StmALog.GetTableFriendlyName(this, Master, Factory);
			public ZBool SL_FireWorkflow { get; private set; }
			public ZString SL_GB_NKBranch => GlbBranch.CurrentBranch?.GB_Code ?? ZString.Empty;
			public ZString SL_GE_NKDepartment => GlbDepartment.CurrentDepartment?.GE_Code ?? ZString.Empty;
			public BusinessObject Master => (BusinessObject)StmALog.GetMaster(SL_Parent, SL_Table, Factory);

			ZGuid IIdentified.Identifier => SL_PK;
			ZDateTime IWorkflowTriggerSource.EventTime => SL_EventTime;
			ZGuid IWorkflowTriggerSource.ParentID => SL_Parent;
			ZString IWorkflowTriggerSource.SourceType => ZString.Empty;
			ZString IWorkflowTriggerSource.Reference => SL_Reference;

			ZBool IWorkflowTriggerSource.IsEstimate => SL_IsEstimate;

			ZDateTime IWorkflowTriggerSource.PostedTimeUtc => SL_PostedTimeUtc;
			ZDateTimeOffset IWorkflowTriggerSource.EventTimeOffset => SL_EventTimeOffset;

			ZString IWorkflowTriggerSource.DepartmentCode => SL_GE_NKDepartment;

			ZString IWorkflowTriggerSource.BranchCode => SL_GB_NKBranch;

			ZString IWorkflowTriggerSource.CompanyCode => Factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, SL_GB_NKBranch)?.Company.GC_Code ?? ZString.Empty;

			ZString IWorkflowTriggerSource.FriendlyTableName => ZString.Empty;

			IPropagationSettings IWorkflowTriggerSource.PropagationSettings => new DefaultPropagationSettings();

			ZDateTime IWorkflowTriggerSource.EventTimeUtc => SL_EventTimeUtc;

			ZString IWorkflowTriggerSource.Source => ZString.Empty;

			ZBool IWorkflowTriggerSource.IsCancelled => SL_IsCancelled;

			ZString IEventUserContextSource.UserCode => SL_GS_NKUser;

			ZString IEventUserContextSource.StaffCode => SL_GS_NKUser;

			void IStmALog.Cancel()
			{
				var message = string.Format(CultureInfo.InvariantCulture, "Trying to cancel an uncancellable event: {0} {1} {2}.", SL_SE_NKEvent, SL_GS_NKUser, SL_Reference);
				ErrorReporter.ReportOnce("CancellingAnUncancellableEvent", message);
			}
			IStaff IStmALog.User => (IStaff)Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, SL_GS_NKUser);

			ZDateTime IStmALog.PostedTimeLocal => PostedLocalBranchTime;

			KeyDataPairCollection sourceInfoItems;
			public IKeyDataPairCollection SourceInfoItems
			{
				get { return sourceInfoItems ?? (sourceInfoItems = new KeyDataPairCollection(Factory)); }
			}

			public ZString SL_UserNameAndInitials
			{
				get
				{
					var user = ((IStmALog)this).User;

					if (user == null)
					{
						return ZString.Empty;
					}

					return StmALog.FormatStaffName(user.GS_FullName, user.GS_Code);
				}
			}

			public ZString DisplayEventReference => SL_TableFriendlyName;

			public ZDateTime PostedLocalBranchTime => (SL_PostedTimeUtc.IsValid) ? EnvProxy.Instance.Time.GetLocalTimeFromUtc(SL_PostedTimeUtc.ToDateTime()) : ZDateTime.Empty;

			IStmALog IStmALog.WeakCopy()
			{
				return (IStmALog)this.MemberwiseClone();
			}

			public ILogsParams Params => new LogsParams(SL_Reference);

			public static PublishedEvent Create(IQueuedLog log)
			{
				return new PublishedEvent(log.Factory, log.PK)
				{
					SL_EventTime = log.SJ_EventTime,
					SL_EventTimeUtc = log.SJ_EventTimeUtc,
					SL_GS_NKUser = log.SJ_GS_NKUser,
					SL_IsEstimate = log.SJ_IsEstimate,
					SL_Parent = log.SJ_ParentID,
					SL_Table = log.SJ_ParentTableCode,
					SL_Reference = log.SJ_Reference,
					SL_SE_NKEvent = log.SJ_SE_NKEvent,
					SL_FireWorkflow = log.SJ_IsDelayFired,
					SL_PostedTimeUtc = log.SJ_PostedTimeUtc,
					Parameters = StmALog.GetParametersFromReference(log.SJ_Reference)
				};
			}
		}

		class EventWithLogParentsToFire
		{
			public EventWithLogParentsToFire(PublishedEvent evnt, IStmALogParent[] logParents)
			{
				Event = evnt;
				LogParents = logParents;
			}

			public PublishedEvent Event { get; }
			public IStmALogParent[] LogParents { get; }
		}

		#endregion
	}
}
