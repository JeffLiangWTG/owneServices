using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker
{
	class StmJobQueue : AutoStmJobQueue, IQueuedLog, IStmALog, IQueuedLogDelayer
	{
		public StmJobQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_ALogReference), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_EventTime), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_EventTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_FilterName), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_GB_NKBranch), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_GE_NKDepartment), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_GS_NKUser), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_IsCancelled), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_IsDelayFired), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_IsEstimate), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_JobConfirmed), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_ParentID), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_ParentTableCode), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_PostedTimeUtc), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_ProcessTaskParentID), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_Reference), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_SE_NKEvent), ConcurrencyPolicy.Ignore);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(SJ_TargetID), ConcurrencyPolicy.Ignore);
		}

		#region IQueuedLog Members

		bool IQueuedLog.IsRetry => SJ_RetryCount > 1; // > 1 as we journal SJ_RetryCount prior to processing

		public IEnumerable<IStmChangeLog> ChangeLogs => changeLogs ?? (changeLogs = GetChangeLogs());

		IEnumerable<IStmChangeLog> changeLogs;

		IEnumerable<IStmChangeLog> GetChangeLogs()
		{
			if (IsStmChangeLogQueue)
			{
				var pk = new WorkflowTriggerEventData(this).TriggeringLogPK;
				if (pk.IsValid)
				{
					var log = Factory.Load<StmChangeLog>(pk);
					if (log != null)
					{
						return new[] { log };
					}
				}
			}
			return Enumerable.Empty<IStmChangeLog>();
		}

		bool IsStmChangeLogQueue
		{
			get
			{
				return SJ_Reference.Contains(ZString.Format("{0}={1}", WorkflowTriggerEventData.TriggeringSource, StmChangeLogSchema.Constants.Prefix), StringComparison.OrdinalIgnoreCase);
			}
		}

		public new bool IsAutoLogged => base.IsAutoLogged;

		public ZString CompanyCode => Factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, SJ_GB_NKBranch)?.Company.GC_Code ?? ZString.Empty;

		public ZString StaffCode => SJ_GS_NKUser;

		ZString IWorkflowTriggerSource.DepartmentCode => SJ_GE_NKDepartment;

		ZString IWorkflowTriggerSource.BranchCode => SJ_GB_NKBranch;

		ZString IWorkflowTriggerSource.CompanyCode => CompanyCode;

		ZGuid IIdentified.Identifier => SJ_ALogReference;

		ZGuid IWorkflowTriggerSource.ParentID => SJ_ParentID;

		ZDateTime IWorkflowTriggerSource.EventTime => SJ_EventTime;

		ZString IWorkflowTriggerSource.SourceType => ZString.Empty;

		ZString IWorkflowTriggerSource.Reference => SJ_Reference;

		ZBool IWorkflowTriggerSource.IsEstimate => SJ_IsEstimate;

		ZDateTime IWorkflowTriggerSource.PostedTimeUtc => SJ_PostedTimeUtc;

		ZDateTimeOffset IWorkflowTriggerSource.EventTimeOffset => ((IStmALog)this).SL_EventTimeOffset;

		ZString IWorkflowTriggerSource.FriendlyTableName => ZString.Empty;

		IPropagationSettings IWorkflowTriggerSource.PropagationSettings => new DefaultPropagationSettings();

		ZDateTime IWorkflowTriggerSource.EventTimeUtc => SJ_EventTimeUtc;

		ZString IWorkflowTriggerSource.Source => ZString.Empty;

		ZBool IWorkflowTriggerSource.IsCancelled => SJ_IsCancelled;

		ZString IEventUserContextSource.UserCode => SJ_GS_NKUser;

		#endregion

		#region IStmALog

		ZGuid IStmALog.PK => SJ_ALogReference;

		public BusinessObject Master => master ?? (master = StmALog.LoadMaster(Factory, SJ_ParentID, SJ_ParentTableCode));
		BusinessObject master;

		ZDateTime IStmALog.SL_EventTime => SJ_EventTime;
		ZDateTime IStmALog.SL_EventTimeUtc => SJ_EventTimeUtc;

		ZDateTimeOffset IStmALog.SL_EventTimeOffset => StmALog.ToDateTimeOffset(Factory, SJ_EventTime, SJ_EventTimeUtc, SJ_GB_NKBranch);

		ZGuid IStmALog.SL_Parent => SJ_ParentID;

		ZString IStmALog.SL_GS_NKUser => SJ_GS_NKUser;

		ZString IStmALog.SL_Reference => SJ_Reference;

		ZString IStmALog.SL_ReferenceForBinding => StmALog.GetReferenceForBinding(Master, SJ_Reference);

		ZString IStmALog.SL_SE_NKEvent => SJ_SE_NKEvent;

		ZString IStmALog.SL_Table => ObjectFactory.Get<IApplicationSchemaResolver>().GetTableSchemaFromColumnNamePrefix(SJ_ParentTableCode).TableName;
		ZString IStmALog.SL_TableFriendlyName => StmALog.GetTableFriendlyName(this, Master, Factory);

		ZBool IStmALog.SL_IsEstimate => SJ_IsEstimate;

		ZBool IStmALog.SL_IsCancelled => SJ_IsCancelled;

		ZBool IStmALog.SL_FireWorkflow => SJ_IsDelayFired;

		ZString IStmALog.SL_GB_NKBranch => SJ_GB_NKBranch;

		ZString IStmALog.SL_GE_NKDepartment => SJ_GE_NKDepartment;

		ZString IStmALog.SL_UserNameAndInitials
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

		ZString IStmALog.DisplayEventReference => ((IStmALog)this).SL_TableFriendlyName;

		public ZDateTime PostedLocalBranchTime => ZDateTime.Empty;

		IDictionary<string, string> IStmALog.Parameters => parameters ?? (parameters = StmALog.GetParametersFromReference(SJ_Reference, StmALog.ParseReferenceError.None));
		IDictionary<string, string> parameters;

		void IStmALog.Cancel()
		{
			if (!SJ_IsCancelled)
			{
				var log = Factory.Load<StmALog>(GetStmALogQuery()).SingleOrDefault();
				if (log != null)
				{
					log.Cancel();
				}

				SJ_IsCancelled = true;
			}
		}

		IStaff IStmALog.User => (IStaff)Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, SJ_GS_NKUser);

		KeyDataPairCollection sourceInfoItems;
		public IKeyDataPairCollection SourceInfoItems
		{
			get { return sourceInfoItems ?? (sourceInfoItems = new KeyDataPairCollection(Factory)); }
		}

		IStmALog IStmALog.WeakCopy() => this; // StmJobQueue is already a copy.

		public ILogsParams Params => new LogsParams(SJ_Reference);
		ZQuery GetStmALogQuery()
		{
			return
				new ZQuery(StmALogSchema.SL_Parent, SJ_ParentID)
				.AddToFilter(StmALogSchema.SL_EventTime, SJ_EventTime)
				.AddToFilter(StmALogSchema.SL_SE_NKEvent, SJ_SE_NKEvent)
				.AddToFilter(StmALogSchema.SL_GS_NKUser, SJ_GS_NKUser)
				.AddToFilter(StmALogSchema.SL_IsEstimate, SJ_IsEstimate)
				.AddToFilter(StmALogSchema.PK, SJ_ALogReference);
		}

		ZDateTime IStmALog.PostedTimeLocal => PostedLocalBranchTime;

		#endregion

		#region IQueuedLogDelayer

		ZDateTime IQueuedLogDelayer.EventTime { set => SJ_EventTime = value; }

		ZBool IQueuedLogDelayer.IsDelayFired { get => SJ_IsDelayFired; set => SJ_IsDelayFired = value; }

		#endregion
	}
}
