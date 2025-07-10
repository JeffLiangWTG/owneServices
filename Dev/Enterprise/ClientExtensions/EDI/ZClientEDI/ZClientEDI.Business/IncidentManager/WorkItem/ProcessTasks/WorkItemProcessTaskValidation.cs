using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Environment;
using WTG.DevTools.Definitions;
using WTG.DevTools.SourceControl;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class WorkItemProcessTaskValidation : ProcessTaskValidation
	{
		public WorkItemProcessTaskValidation(WorkItemProcessTask parent)
			: base(parent)
		{
		}

		protected new WorkItemProcessTask Parent
		{
			get { return (WorkItemProcessTask)base.Parent; }
		}

		protected override void CheckP9_Notes()
		{
			Parent.MinutesElapsedForShelfCreationTimeLog = 0m;
			if (Parent.IsShelfsetTask && Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
			{
				var hasGitPullRequest = PullRequestUrl.ParseMany(Parent.P9_NotesAsString).Length > 0;
				if (!hasGitPullRequest)
				{
					AddPullRequestMissingForUserError();
				}
			}

			if (Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Working)
			{
				base.CheckP9_Notes();
			}
		}

		void AddPullRequestMissingForUserError()
		{
			Parent.P9_NotesInfo.AddError("This DAT submission contains no Pull Requests. Please add a Pull Request URL, or change the task type or status.");
		}

		protected override void CheckP9_GS_NKAssignedStaffMember()
		{
			if (!IsShelf)
			{
				base.CheckP9_GS_NKAssignedStaffMember();

				if (Parent.IsShelfsetTask && Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
				{
					if (Parent.AssignedStaffMember == null)
					{
						Parent.P9_GS_NKAssignedStaffMemberInfo.AddError("Please choose a valid user or change the task type.");
					}
				}

				if (Parent.P9_GS_NKAssignedStaffMember.IsEmpty &&
					Parent.IsShelfsetTask &&
					!Parent.ShelfOwnerName.IsEmpty)
				{
					Parent.P9_GS_NKAssignedStaffMemberInfo.AddError("Staff member for TFS login '" + Parent.ShelfOwnerName + "' has not been found.");
				}

				if (Parent.IsInDatabase && Parent.P9_GS_NKAssignedStaffMemberInfo.HasChanges && Parent.P9_GS_NKAssignedStaffMember == "DAT"
					 && Parent.IsCheckInTaskEvenWhenAssignedToDAT && !Parent.P9_GS_NKAssignedStaffMemberInfo.OriginalValue.IsEmpty)
				{
					Parent.P9_GS_NKAssignedStaffMemberInfo.AddWarning("Please assign a staff member for the task, so that notification emails can be received.");
				}
			}
		}

		protected override void CheckP9_Status()
		{
			if (!IsShelf)
			{
				base.CheckP9_Status();
			}

			ValidateTaskForWorkingShelfStatus(Parent.P9_StatusInfo);
			ValidateTasksForShelfCreationForDefect(Parent.P9_StatusInfo);
		}

		void ValidateTaskForWorkingShelfStatus(ZPropertyInfo propertyInfo)
		{
			if (Parent.IsShelfsetTaskEvenWhenAssignedToDAT && Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Working)
			{
				propertyInfo.AddError(ShelfTaskCannotBeSetToWorking);
			}
		}

		public const string ShelfTaskCannotBeSetToWorking =
				"You cannot set a shelf task to working, please use the assigned (ASN) status to submit a shelf to DAT.";

		protected override void CheckP9_Type()
		{
			if (!IsShelf)
			{
				base.CheckP9_Type();

				if (ReleaseRingsLookup.CheckInTaskTypes.Any(t => t == Parent.P9_Type) && Parent.P9_GS_NKAssignedStaffMember.IsEmpty)
				{
					Parent.P9_TypeInfo.AddError("You cannot have a checkin task assigned to no-one as this will cause build errors with DAT.");
				}

				ValidateTaskForWorkingShelfStatus(Parent.P9_TypeInfo);
				ValidateCheckinTaskForDefect(Parent.P9_TypeInfo);
				ValidateCheckinTaskForCodeReviews(Parent.P9_TypeInfo);
				ValidateCheckinTaskForPrecedingTasks(Parent.P9_TypeInfo);
			}
		}

		bool ShouldValidateDefectOnConditions => Parent.Parent != null &&
			Parent.Parent.IsDefect;

		bool ValidDefectCausedByWorkItem => !Parent.Parent.DefectCausedByWorkItemPK.IsEmpty &&
			Parent.Parent.DefectCausedByWorkItemPK.IsValid && !(Parent.Parent.Lookups.DefectCausedByTasks.Count == 0);

		bool ValidDefectCausedByTask => !Parent.Parent.WKI_P9_DefectCausedByTask.IsEmpty && Parent.Parent.WKI_P9_DefectCausedByTask.IsValid &&
			Parent.Parent.IsTargetTaskLinkedToTargetDefectCausedWI() && (Parent.Parent.DefectCausedByTask.P9_Status == ProcessTaskStatusCodeList.Codes.Closed);

		bool ValidDefectFirstMissedInTask => !Parent.Parent.WKI_P9_DefectFirstMissedInTask.IsEmpty && Parent.Parent.WKI_P9_DefectFirstMissedInTask.IsValid;

		void ValidateCheckinTaskForDefect(ZPropertyInfo propertyInfo)
		{
			if (ShouldValidateDefectOnConditions)
			{
				var isCompletedNonDATCheckin = Parent.P9_Type == EDITaskTypes.TaskCheckin && Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Closed;

				if (IsQueuedAlphaCheckin || isCompletedNonDATCheckin)
				{
					if (!ValidDefectCausedByWorkItem)
					{
						propertyInfo.AddError(DefectIntroducedWorkItemMustBeEntered);
					}
					else if (!ValidDefectCausedByTask)
					{
						propertyInfo.AddError(DefectIntroducedTaskMustBeEntered);
					}
					else if (!ValidDefectFirstMissedInTask)
					{
						propertyInfo.AddError(FirstCBThatMissedDefectMustBeEntered);
					}
				}
			}
		}

		void ValidateCheckinTaskForCodeReviews(ZPropertyInfo propertyInfo)
		{
			if (!ShouldCheckReviewOrPrecedingTasks)
			{
				return;
			}

			var reviewTasks = WorkItemTaskHelper.GetReviewTasks(Parent, new ProcessTaskStatusCodeList().GetAllCodes());
			if (reviewTasks.IsEmpty)
			{
				propertyInfo.AddError(NoCompletedReviewTaskMessage);
				return;
			}
			var closedReviewFound = false;
			foreach (var task in reviewTasks)
			{
				if (task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
				{
					closedReviewFound = true;
				}
				else if (!task.IsClosed) // Excludes cancelled tasks because IsClosed includes cancelled status for some reason...
				{
					propertyInfo.AddError(IncompleteReviewTaskMessage);
					break;
				}
			}
			if (!closedReviewFound)
			{
				propertyInfo.AddError(NoCompletedReviewTaskMessage);
			}
		}

		void ValidateCheckinTaskForPrecedingTasks(ZPropertyInfo propertyInfo)
		{
			if (!ShouldCheckReviewOrPrecedingTasks)
			{
				return;
			}

			var isModernizationWorkItem = Parent.Parent != null
				&& Parent.Parent.WKI_WorkItemType == EnterpriseCode
				&& Parent.Parent.WKI_WorkItemArea == ModernizationCode;

			if (isModernizationWorkItem)
			{
				var hasIncompletePrecedingTasks = Parent.GetAllTasksInWorkflowInOrder()
					.Any(x => x.P9_Sequence < Parent.P9_Sequence
						&& x.P9_Status != ProcessTaskStatusCodeList.Codes.Closed
						&& x.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled);

				if (hasIncompletePrecedingTasks)
				{
					propertyInfo.AddError(IncompletePrecedingTaskMessage);
				}
			}
		}

		public const string NoCompletedReviewTaskMessage = "Cannot queue for checkin since no completed code review task is present. It must be lower in sequence than this task in the same workflow, or in a prerequisite workflow.";
		public const string IncompleteReviewTaskMessage = "Cannot queue for checkin since there is at least one incomplete code review task present. All code reviews related to this checkin task must be closed.";
		public const string IncompletePrecedingTaskMessage = "Cannot queue for checkin since there is at least one incomplete task present. All preceding tasks in this workflow must be completed.";

		bool ShouldCheckReviewOrPrecedingTasks => EDIDataRegistry.Instance.RequireCompletedCodeReviewForCheckin.Value
				&& IsQueuedAlphaCheckin
				&& (!Parent.IsInDatabase || Parent.P9_TypeInfo.HasChanges); // Task was newly created or type was changed on an existing task

		bool IsQueuedAlphaCheckin => IsAlphaCheckinTaskType && Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned;

		bool IsAlphaCheckinTaskType => Parent.P9_Type == ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;

		public static string DefectIntroducedWorkItemMustBeEntered { get { return "There is no nominated cause for this defect. A " + ProcessManagementRegistry.Instance.DefectIntroducedInWorkItemLabel.Value.ToString() + " value must be entered before this change can be queued for check-in."; } }

		public static string DefectIntroducedTaskMustBeEntered { get { return "There is no nominated task for this defect. A " + ProcessManagementRegistry.Instance.DefectIntroducedInTaskLabel.Value.ToString() + " value must be entered before this change can be queued for check-in."; } }

		public static string FirstCBThatMissedDefectMustBeEntered { get { return "There is no nominated containment barrier for this defect. A " + ProcessManagementRegistry.Instance.FirstCBThatMissedDefectLabel.Value.ToString() + " value must be entered before this change can be queued for check-in."; } }

		void ValidateTasksForShelfCreationForDefect(ZPropertyInfo propertyInfo)
		{
			if (ShouldValidateDefectOnConditions &&
				Parent.IsTaskMandatoryForShelfQueue &&
				Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
			{
				if (!ValidDefectCausedByWorkItem)
				{
					propertyInfo.AddWarning(DefectIntroducedWorkItemMustBeEntered);
				}
				else if (!ValidDefectCausedByTask)
				{
					propertyInfo.AddWarning(DefectIntroducedTaskMustBeEntered);
				}
				else if (!ValidDefectFirstMissedInTask)
				{
					propertyInfo.AddWarning(FirstCBThatMissedDefectMustBeEntered);
				}
			}
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckForReusedCheckInOrShelfsetTestTask();
			if (!Globals.IsTest)
			{
				ValidateDefectCount();
			}
		}

		public bool IsShelf => Parent.IsShelfCheckedIn || Parent.IsShelfsetTestClosed;

		void CheckForReusedCheckInOrShelfsetTestTask()
		{
			if (IsShelf)
			{
				ZPropertyInfo[] propertiesThatShouldNotBeChanged = new[]
				{
					Parent.P9_DescriptionInfo,
					Parent.P9_GS_NKAssignedStaffMemberInfo,
					Parent.P9_TypeInfo
				};
				if (propertiesThatShouldNotBeChanged.All(p => !p.HasChanges) && Parent.P9_StatusInfo.HasChanges)
				{
					if (Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled || Parent.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
					{
						return;
					}
				}
				if (propertiesThatShouldNotBeChanged.Any(p => p.HasChanges) || Parent.P9_StatusInfo.HasChanges)
				{
					if (Parent.IsShelfCheckedIn)
					{
						Parent.AddRowError("This task has previously been checked in. Please do not reuse.");
					}
					else
					{
						Parent.AddRowError("This task has previously been shelf tested. Please do not reuse.");
					}
				}
			}
		}

		protected override void CheckP9_Description()
		{
			if (!IsShelf)
			{
				base.CheckP9_Description();
			}
		}

		protected override void CheckP9_GG_AssignedGroup()
		{
			if (!IsShelf)
			{
				base.CheckP9_GG_AssignedGroup();
			}
		}

		public void ValidateDefectCount()
		{
			ValidateCalculatedProperty(Parent.DefectCountInfo);
		}

		protected void CheckDefectCount()
		{
			MandatoryValidation.CheckNotNegative(Parent.DefectCountInfo);
			CompareValidation.CheckLessThanOrEqualTo(Parent.DefectCountInfo, 999m);
		}

		public const string EnterpriseCode = "ENT";
		public const string ModernizationCode = "MDN";
	}
}
