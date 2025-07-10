using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.DebuggerDisplay("Sequence: {TGL_Sequence}, Parent: {Parent}")]
	public class WorkQueueMembershipLink : TagLink, ISequenceNumber
	{
		public WorkQueueMembershipLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region BusinessObject Overrides

		protected override TagLinkValidation GetNewValidation()
		{
			return new WorkQueueMembershipLinkValidation(this);
		}

		#endregion

		#region Related Business Objects

		public new ProcessHeader Parent
		{
			get { return Factory.Load<ProcessHeader>(TGL_ParentId); }
		}

		public WorkQueue WorkQueue
		{
			get { return Factory.Load<WorkQueue>(TGL_TGM_Magnitude); }
		}

		#endregion

		#region Properties

		[ReadOnlyMember(nameof(CheckResequenceSecurity))]
		public override ZShort TGL_Sequence
		{
			get { return base.TGL_Sequence; }
			set { base.TGL_Sequence = value; }
		}

		protected bool CheckResequenceSecurity
		{
			get { return !WorkQueueSecurity.CheckResequenceQueueSecurity(WorkQueue); }
		}

		public bool ShouldValidateSequenceUniqueness { get; set; }

		#endregion

		#region New Properties

		[ReadOnly(true)]
		[ResourceStringData("WorkQueueMembershipLink.RunningTotal", Caption = "Running Total", FullDescription = "Running Total is a sum of the planned duration up to and including this item.")]
		public ZDateTime RunningTotal
		{
			get { return RunningTotalInMinutes.GetDateTimeFromMinutes(); }
		}

		public ZInt RunningTotalInMinutes
		{
			get { return WorkQueue.GetRunningTotal(this); }
		}

		[ReadOnly(true)]
		[ResourceStringData("WorkQueueMembershipLink.WorkflowDescription", Caption = "Workflow Description")]
		public ZString WorkflowDescription
		{
			get
			{
				var workflow = Parent;
				return workflow != null ? workflow.FH_CompletionStatement : ZString.Empty;
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("WorkQueueMembershipLink.TimeAddedToQueue", Caption = "Time Added to Queue (Local)")]
		public ZDateTime LocalTimeAddedToQueue => ToLocalTime(TGL_SystemCreateTimeUtc);

		[ReadOnly(true)]
		[ResourceStringData("WorkQueueMembershipLink.AddedByName", Caption = "Added By")]
		public ZString AddedByName => StaffFullName(TGL_SystemCreateUser);

		public ZString QueueStatus
		{
			get
			{
				return Factory.GetCachedValue("WorkQueueMembershipLink.QueueStatus" + PK.ToString(), () =>
				{
					var queue = WorkQueue;

					if (queue != null)
					{
						var earlierMembers =
							from link in queue.Members
							where link.TGL_Sequence < TGL_Sequence
							let processHeader = link.Parent
							where processHeader != null
							select processHeader;

						if (earlierMembers.All(m => m.IsClosed || IsFullyReleased(m)))
						{
							return QueueStatusList.Codes.ReadyToRelease;
						}
						else
						{
							return QueueStatusList.Codes.Blocked;
						}
					}

					return string.Empty;
				}, CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		[ResourceStringData("WorkQueueMembershipLink.QueueStatusDescription", Caption = "Queue Status", FullDescription = "Indicates whether this item is blocked by earlier items in the queue. An item is considered ready to release when all preceding items are complete or released to a buffer.")]
		public ZString QueueStatusDescription
		{
			get { return Lookups.QueueStatusList.GetDescriptionFromCode(QueueStatus); }
		}

		static bool IsFullyReleased(ProcessHeader processHeader)
		{
			return processHeader.IsWorkflow ? (bool)processHeader.IsReleased : processHeader.JobHeader.ProcessHeaders.All(w => w.IsClosed || w.IsReleased);
		}

		#endregion

		#region ISequenceNumber Members

		ZShort ISequenceNumber.SequenceNumber { get => TGL_Sequence; set => TGL_Sequence = value; }
		ProcessTask CurrentProcessTask => Parent.CurrentTasks.FirstOrDefault();

		[ResourceStringData("WorkQueueMembershipLink.StaffAssignedToNextStartableTask", Caption = "Staff Assigned to Next Startable Task")]
		public ZString StaffAssignedToNextStartableTask => CurrentProcessTask?.StaffName ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.CapabilityAssignedToNextStartableTask", Caption = "Capability Assigned to Next Startable Task")]
		public ZString CapabilityAssignedToNextStartableTask => CurrentProcessTask?.CapabilityName ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.JobCreatedBy", Caption = "Job Created By")]
		public ZString JobCreatedBy => StaffFullName(WorkQueueMembersProvider?.JobCreatedBy ?? ZString.Empty);

		[ResourceStringData("WorkQueueMembershipLink.JobCreatedDate", Caption = "Job Created Date (Local)")]
		public ZDateTime JobCreatedDate => ToLocalTime(WorkQueueMembersProvider?.JobCreatedDate ?? ZDateTime.Empty);

		[ResourceStringData("WorkQueueMembershipLink.JobCriteria1", Caption = "Job Criteria 1")]
		public ZString JobCriteria1 => WorkQueueMembersProvider?.JobCriteria1 ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.JobCriteria2", Caption = "Job Criteria 2")]
		public ZString JobCriteria2 => WorkQueueMembersProvider?.JobCriteria2 ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.JobCriteria3", Caption = "Job Criteria 3")]
		public ZString JobCriteria3 => WorkQueueMembersProvider?.JobCriteria3 ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.JobCriteria4", Caption = "Job Criteria 4")]
		public ZString JobCriteria4 => WorkQueueMembersProvider?.JobCriteria4 ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.JobCriteria5", Caption = "Job Criteria 5")]
		public ZString JobCriteria5 => WorkQueueMembersProvider?.JobCriteria5 ?? ZString.Empty;

		IWorkQueueMembersProvider WorkQueueMembersProvider => Parent?.Parent as IWorkQueueMembersProvider;

		ProcessHeader CurrentWorkflow
		{
			get
			{
				var isWorkflow = Parent?.IsWorkflow ?? ZBool.False;
				var processJobHeader = isWorkflow ? Parent?.JobHeader : Parent as ProcessJobHeader;
				return processJobHeader?.ProcessHeaders?.FirstOpenWorkFlow;
			}
		}

		[ResourceStringData("WorkQueueMembershipLink.DescriptionOfTheFirstOpenWorkflowInTheJob", Caption = "Description of the first open workflow in the job")]
		public ZString DescriptionOfTheFirstOpenWorkflowInTheJob => CurrentWorkflow?.FH_CompletionStatement ?? ZString.Empty;

		[ResourceStringData("WorkQueueMembershipLink.CurrentComponentForTheFirstOpenWorkflowInTheJob", Caption = "Current Component for the first open workflow in the job")]
		public ZString CurrentComponentForTheFirstOpenWorkflowInTheJob => CurrentWorkflow?.CurrentComponent?.FC_Name ?? ZString.Empty;

		ZDateTime ToLocalTime(ZDateTime dateTime)
		{
			var localTime = ZDateTime.Empty;
			if (dateTime.IsValid)
			{
				localTime = Factory.GetCachedValue(dateTime.ToString(), () => { return WorkingTimeContext.Create(Factory).ToLocalTime(dateTime, Factory); });
			}
			return localTime;
		}
		ZString StaffFullName(ZString staffCode)
		{
			var staffFullName = ZString.Empty;
			if (!staffCode.IsEmpty)
			{
				var glbStaff = Factory.GetCachedValue(staffCode, () => { return Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode); });
				staffFullName = glbStaff?.GS_FullName ?? ZString.Empty;
			}
			return staffFullName;
		}

		#endregion

		#region Object Overrides

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, (NoResString)"Sequence: {0}, Parent: {1}", TGL_Sequence, Parent); // Developer diagnostic text
		}

		#endregion
	}
}
