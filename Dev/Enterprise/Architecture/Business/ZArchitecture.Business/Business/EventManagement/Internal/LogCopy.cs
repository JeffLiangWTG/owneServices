using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	class LogCopy : NonPersistentBusinessObject, IStmALog
	{
		internal LogCopy(IStmALog log) : base(log.Factory)
		{
			var logBizo = log as BusinessObject;
			if (logBizo != null && !logBizo.IsSavedByFactory)
			{
				LogIdentifier = ZGuid.Invalid;
			}
			else
			{
				LogIdentifier = log.PK;
			}
			SL_PK = log.PK;
			SL_EventTime = log.SL_EventTime;
			SL_EventTimeUtc = log.SL_EventTimeUtc;
			SL_EventTimeOffset = log.SL_EventTimeOffset;
			SL_Parent = log.SL_Parent;
			SL_GS_NKUser = log.SL_GS_NKUser;
			SL_Reference = log.SL_Reference;
			SL_SE_NKEvent = log.SL_SE_NKEvent;
			SL_Table = log.SL_Table;
			SL_IsEstimate = log.SL_IsEstimate;
			SL_IsCancelled = log.SL_IsCancelled;
			SL_FireWorkflow = log.SL_FireWorkflow;
			SL_GB_NKBranch = log.SL_GB_NKBranch;
			SL_GE_NKDepartment = log.SL_GE_NKDepartment;
			Parameters = log.Parameters;
			ParentID = log.ParentID;
			EventTime = log.EventTime;
			SourceType = log.SourceType;
			Reference = log.Reference;
			PostedTimeUtc = log.PostedTimeUtc;
			FriendlyTableName = log.FriendlyTableName;
			Master = log.Master;
			PostedLocalBranchTime = log.PostedLocalBranchTime;
			Params = log.Params;

			this.PropagationSettings = log.PropagationSettings;
		}

		ZGuid IIdentified.Identifier => LogIdentifier;

		ZGuid LogIdentifier {  get; }
		public ZGuid SL_PK { get; }
		public BusinessObject Master { get; }

		public ZDateTime SL_EventTime { get; }
		public ZDateTime SL_EventTimeUtc { get; }

		public ZDateTimeOffset SL_EventTimeOffset { get; }
		public ZDateTimeOffset EventTimeOffset => SL_EventTimeOffset;

		public ZGuid SL_Parent { get; }

		public ZString SL_GS_NKUser { get; }

		public ZString SL_Reference { get; }

		public ZString SL_SE_NKEvent { get; }

		public ZString SL_Table { get; }
		public ZString SL_TableFriendlyName => StmALog.GetTableFriendlyName(this, Master, Factory);

		public ZBool SL_IsEstimate { get; }

		public ZBool SL_IsCancelled { get; }

		public ZBool SL_FireWorkflow { get; }

		public ZString SL_GB_NKBranch { get; }

		public ZString SL_GE_NKDepartment { get; }

		public IDictionary<string, string> Parameters { get; }

		public ZGuid ParentID { get; }

		public ZDateTime EventTime { get; }

		public ZString SourceType { get; }

		public ZString Reference { get; }

		public ZGuid Identifier => SL_PK;

		public ZBool IsEstimate => SL_IsEstimate;

		public ZDateTime PostedTimeUtc { get; }

		public ZString StaffCode => SL_GS_NKUser;

		public ZString DepartmentCode => SL_GE_NKDepartment;

		public ZString BranchCode => SL_GB_NKBranch;

		public ZString CompanyCode => Factory.LoadFromNaturalKey<IGlbBranch>(GlbBranchSchema.GB_Code, SL_GB_NKBranch)?.Company.GC_Code ?? ZString.Empty;

		public ZString FriendlyTableName { get; }
		public ZString SL_ReferenceForBinding => StmALog.GetReferenceForBinding(Master, SL_Reference);
		public IPropagationSettings PropagationSettings { get; }

		public IStaff User => (IStaff)Factory.LoadFromNaturalKey<IGlbStaff>(GlbStaffSchema.GS_Code, SL_GS_NKUser);

		IKeyDataPairCollection sourceInfoItems;
		public IKeyDataPairCollection SourceInfoItems => sourceInfoItems ?? (sourceInfoItems = new KeyDataPairCollection(Factory));

		public ZString SL_UserNameAndInitials => User == null ? ZString.Empty : StmALog.FormatStaffName(User.GS_FullName, User.GS_Code);

		public ZString DisplayEventReference => SL_Reference;

		public ZDateTime PostedLocalBranchTime { get; }

		public ZDateTime PostedTimeLocal => PostedLocalBranchTime;

		public ZDateTime EventTimeUtc => SL_EventTimeUtc;

		public ZString Source => FriendlyTableName;

		public ZBool IsCancelled => SL_IsCancelled;

		public ZString UserCode => StaffCode;

		public ILogsParams Params { get; }

		IStmALog IStmALog.WeakCopy()
		{
			return new LogCopy(this);
		}

		public void Cancel()
		{
			throw new InvalidOperationException();
		}
	}
}
