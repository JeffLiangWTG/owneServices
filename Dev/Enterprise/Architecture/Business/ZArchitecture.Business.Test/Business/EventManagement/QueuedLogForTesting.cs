using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	sealed class QueuedLogForTesting : IQueuedLog
	{
		public QueuedLogForTesting(ZString reference)
		{
			SJ_SE_NKEvent = Events.WorkflowTriggerEventCode;
			SJ_Reference = reference;
		}

		public QueuedLogForTesting(StmALog wteEvent)
		{
			if (wteEvent.SL_SE_NKEvent != Events.WorkflowTriggerEventCode)
			{
				throw new InvalidOperationException("This should only be constructed from the WTE Event.");
			}

			Factory = wteEvent.Factory;
			PK = ZGuid.NewZGuid();
			SJ_ALogReference = wteEvent.PK;
			SJ_GS_NKUser = wteEvent.SL_GS_NKUser;
			SJ_IsEstimate = wteEvent.SL_IsEstimate;
			SJ_ParentID = wteEvent.SL_Parent;
			SJ_Reference = wteEvent.SL_Reference;
			SJ_SE_NKEvent = wteEvent.SL_SE_NKEvent;
			SJ_GE_NKDepartment = wteEvent.SL_GE_NKDepartment;
			SJ_GB_NKBranch = wteEvent.SL_GB_NKBranch;
		}
		#region IQueuedLog Members

		public BusinessObjectFactory Factory { get; private set; }
		public bool IsRetry { get; private set; }
		public ZGuid PK { get; private set; }
		public ZGuid SJ_ALogReference { get; private set; }
		public ZDateTime SJ_EventTime { get; private set; }
		public ZDateTime SJ_EventTimeUtc { get; private set; }
		public ZString SJ_GB_NKBranch { get; private set; }
		public ZString SJ_GE_NKDepartment { get; private set; }
		public ZString SJ_GS_NKUser { get; private set; }
		public ZBool SJ_IsEstimate { get; private set; }
		public ZGuid SJ_ParentID { get; private set; }
		public ZString SJ_ParentTableCode { get; private set; }
		public ZString SJ_Reference { get; private set; }
		public ZString SJ_SE_NKEvent { get; private set; }
		public ZGuid SJ_TargetID { get; private set; }
		public ZByte SJ_RetryCount { get; set; }
		public ZBool SJ_IsDelayFired { get; private set; }
		public ZDateTime SJ_PostedTimeUtc { get; private set; }
		public ZString SJ_Status { get; set; }

		public IEnumerable<IStmChangeLog> ChangeLogs => Enumerable.Empty<IStmChangeLog>();

		public ZString StaffCode => SJ_GS_NKUser;

		public IPropagationSettings PropagationSettings => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.DepartmentCode => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.BranchCode => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.CompanyCode => throw new NotImplementedException();

		ZGuid IWorkflowTriggerSource.ParentID => throw new NotImplementedException();

		ZDateTime IWorkflowTriggerSource.EventTime => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.SourceType => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.Reference => throw new NotImplementedException();

		ZBool IWorkflowTriggerSource.IsEstimate => throw new NotImplementedException();

		ZDateTime IWorkflowTriggerSource.PostedTimeUtc => throw new NotImplementedException();

		ZDateTimeOffset IWorkflowTriggerSource.EventTimeOffset => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.FriendlyTableName => throw new NotImplementedException();

		ZString IWorkflowTriggerSource.Source => throw new NotImplementedException();

		ZDateTime IWorkflowTriggerSource.EventTimeUtc => throw new NotImplementedException();

		ZBool IWorkflowTriggerSource.IsCancelled => throw new NotImplementedException();

		ZString IEventUserContextSource.UserCode => throw new NotImplementedException();

		ZGuid IIdentified.Identifier => throw new NotImplementedException();

		#endregion
	}
}
