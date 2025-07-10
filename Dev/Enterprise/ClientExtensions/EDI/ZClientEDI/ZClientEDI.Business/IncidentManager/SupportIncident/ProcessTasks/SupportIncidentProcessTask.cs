using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentProcessTask : CRMProcessTask, IIncidentEventProcessTask
	{
		public SupportIncidentProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ClientControllerRegistration.SupportIncident; }
		}

		protected override Type ParentType
		{
			get { return typeof(SupportIncident); }
		}

		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				if (base.P9_Status != value)
				{
					var originalStatus = base.P9_Status;
					base.P9_Status = value;
					if (IsRowCommittedAndHasParent)
					{
						var parentStatusBeforeCalculation = Parent.IM_Status;
						Parent.CalculateWorkflowDependentProperties();
						if (parentStatusBeforeCalculation != Parent.IM_Status
							&& (P9_Status == ProcessTaskStatusCodeList.Codes.Closed || P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled))
						{
							Parent.CloseIncidentByWorkflowTask(parentStatusBeforeCalculation, this, new SupportIncident.TaskStatusChangeEventArgs() { OriginalTaskStatus = originalStatus });
						}

						Parent.TriggerTaskStatusChangeEvent();
					}
				}
			}
		}

		public override ZString P9_GS_NKAssignedStaffMember
		{
			get { return base.P9_GS_NKAssignedStaffMember; }
			set
			{
				if (base.P9_GS_NKAssignedStaffMember != value)
				{
					base.P9_GS_NKAssignedStaffMember = value;
					if (IsRowCommittedAndHasParent)
					{
						Parent.CalculateWorkflowDependentProperties();
					}
				}
			}
		}

		protected override void OnSetScheduledDateCore(ZDateTimeOffset value)
		{
			base.OnSetScheduledDateCore(value);
			if (IsRowCommittedAndHasParent)
			{
				Parent.CurrentTaskEstimatedDateAsTextInfo.RefreshBinding();
			}
		}

		protected override bool ShouldDefaultEstimate
		{
			get
			{
				var result = base.ShouldDefaultEstimate;
				if (P9_EstimatedDefaultedFrom == SupportIncidentEstimateDefaultedFromList.Codes.TimeOfTaskCopiedFromTemplate)
				{
					result &= !IsInDatabase;
				}
				return result;
			}
		}

		[ReadOnly(true)]
		public override ZGuid P9_OA
		{
			get { return base.P9_OA; }
			set { base.P9_OA = value; }
		}

		[ReadOnly(true)]
		public override ZGuid P9_OC
		{
			get { return base.P9_OC; }
			set { base.P9_OC = value; }
		}

		public new SupportIncident Parent
		{
			get { return (SupportIncident)base.Parent; }
		}

		bool IsRowCommittedAndHasParent
		{
			get { return ((INeedRow)this).Row.RowState != DataRowState.Detached && Parent != null; }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (P9_StatusInfo.HasChanges
				&& (P9_Status == ProcessTaskStatusCodeList.Codes.Closed || P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled)
				&& Parent.IM_Status == SupportIncidentLookups.Status.Closed
				&& (Parent.IM_ResolutionCode != SupportIncidentLookups.DispositionList.Constants.Closed.ClosedAwaitingClientResponse && !Parent.ShouldKeepCurrentDispositionWhileAwaitingResponse))
			{
				foreach (StmALog log in Logs.LogsNotInDB)
				{
					if (log.SL_Reference == AwaitingClientResponseLogReference)
					{
						log.Delete();
					}
				}
			}
		}

		public override void Delete()
		{
			if (IsLastOpenTask())
			{
				Parent.WorkflowItems.Remove(this);
				var previousStatus = P9_Status;
				var parentPreviousStatus = Parent.IM_Status;
				Parent.IM_Status = SupportIncidentLookups.Status.Closed;
				base.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
				Parent.CloseIncidentByWorkflowTask(parentPreviousStatus, this, new SupportIncident.TaskStatusChangeEventArgs() { OriginalTaskStatus = previousStatus });
				if (P9_Status != ProcessTaskStatusCodeList.Codes.Closed && P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					Parent.WorkflowItems.Add(this);
					Parent.Factory.Save();
					return;
				}
			}

			base.Delete();
		}

		bool IsLastOpenTask()
		{
			if (P9_Status == ProcessTaskStatusCodeList.Codes.Closed || P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled)
			{
				return false;
			}

			if (Parent == null || !Parent.IsInDatabase)
			{
				return false;
			}

			return !Parent.WorkflowItems.Cast<SupportIncidentProcessTask>()
				.Any(task =>
					task.PK != this.PK
					&& task.P9_Status != ProcessTaskStatusCodeList.Codes.Closed
					&& task.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled);
		}

		ZString IIncidentEventProcessTask.EventCode
		{
			get;
			set;
		}

		internal const string AwaitingClientResponseLogReference = "AWAITING CLIENT";

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			Parent.TriggerTaskStatusChangeEvent();
		}
	}
}

