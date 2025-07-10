using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.TfsRest;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.Definitions;
using WTG.DevTools.ServiceClient.Assess;
using static System.FormattableString;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[SystemDefinedValues]
	public class WorkItemProcessTask : ProcessManagement.Business.WorkItemProcessTask
	{
		public WorkItemProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public override ZString P9_Description
		{
			get { return base.P9_Description; }
			set
			{
				base.P9_Description = value;
				MarkAsNeedingValidation();
			}
		}

		public override ZString P9_Status
		{
			get { return base.P9_Status; }
			set
			{
				if (base.P9_Status != value)
				{
					base.P9_Status = value;

					MarkAsNeedingValidation();
					if (!IsValidationSuspended && HasChanges)
					{
						Validation.ValidateP9_Description();
						Validation.ValidateP9_Type();
					}
				}
			}
		}

		public bool HasAssignedStaffNotCompletedAnyAssessRequirements()
		{
			if (AssignedStaffMember?.GS_PER is { IsValid: true } personPK)
			{
				foreach (ProcessTaskRequiredSkill pivot in SkillsPivots)
				{
					if (pivot.P9S_Aspect is { IsValid: true } aspectPK)
					{
						var hasPassedResult = AssessServiceClient.HasPassedLearningUnitAsync(aspectPK.ToGuid(), personPK.ToGuid()).GetAwaiter().GetResult();
						if (hasPassedResult.IsSuccess && !hasPassedResult.Content)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		IAssessServiceClient AssessServiceClient => assessServiceClient ??= ObjectFactory.Get<IAssessServiceClient>();
		IAssessServiceClient assessServiceClient;

		public override ZString P9_Type
		{
			get { return base.P9_Type; }
			set
			{
				base.P9_Type = value;
				MarkAsNeedingValidation();
				if (!IsValidationSuspended && HasChanges)
				{
					Validation.ValidateP9_Description();
				}
			}
		}

		protected override void SetDescriptionFromType()
		{
			if (!IsShelfsetTask)
			{
				base.SetDescriptionFromType();
			}
		}

		#region DescriptionFieldType (for ZMultiCombinationControl)

		public ZString DescriptionFieldType
		{
			get
			{
				ZString result = nameof(FieldType.Text);
				if (IsShelfsetTask)
				{
					result = nameof(FieldType.TextCodeFindBox);
				}

				return result;
			}
		}

		#endregion

		public ZBool IsCheckInTask
		{
			get { return ReleaseRingsLookup.CheckInTaskTypes.Any(t => t == P9_Type) && P9_GS_NKAssignedStaffMember != "DAT"; }
		}

		public ZBool WasCheckInTask
		{
			get { return ReleaseRingsLookup.CheckInTaskTypes.Any(t => t == P9_TypeInfo.OriginalValue.ToString()) && P9_GS_NKAssignedStaffMember != "DAT"; }
		}

		public bool IsCheckInTaskEvenWhenAssignedToDAT
		{
			get { return ReleaseRingsLookup.CheckInTaskTypes.Any(t => t == P9_Type); }
		}

		public ZPropertyInfo IsCheckInTaskInfo
		{
			get { return GetZPropertyInfo(nameof(IsCheckInTask)); }
		}

		public const string UATBuildShelfTask = "UA0";
		public const string AspectOnlyBuildShelfTask = "AS0";
		public const string ExperimentalPullRequestTask = "JP0";

		public ZBool IsShelfsetTask
		{
			get { return IsCheckInTask || P9_Type == ShelfsetTestTask || P9_Type == UATBuildShelfTask || P9_Type == AspectOnlyBuildShelfTask || P9_Type == ExperimentalPullRequestTask; }
		}

		public ZBool WasShelfsetTask
		{
			get { return WasCheckInTask || P9_TypeInfo.OriginalValue.ToString() == ShelfsetTestTask || P9_TypeInfo.OriginalValue.ToString() == UATBuildShelfTask || P9_TypeInfo.OriginalValue.ToString() == AspectOnlyBuildShelfTask || P9_TypeInfo.OriginalValue.ToString() == ExperimentalPullRequestTask; }
		}

		public bool IsShelfsetTaskEvenWhenAssignedToDAT
		{
			get { return IsCheckInTaskEvenWhenAssignedToDAT || P9_Type == ShelfsetTestTask || P9_Type == UATBuildShelfTask || P9_Type == AspectOnlyBuildShelfTask || P9_Type == ExperimentalPullRequestTask; }
		}

		public ZPropertyInfo IsShelfsetTaskInfo
		{
			get { return GetZPropertyInfo(nameof(IsShelfsetTask)); }
		}

		public const string ShelfsetTestTask = "SH0";

		public ZBool IsShelfsetTestTask
		{
			get { return P9_Type == ShelfsetTestTask || P9_Type == ExperimentalPullRequestTask; }
		}

		public ZBool IsShelfsetTestClosed
		{
			get
			{
				if (!IsInDatabase)
				{
					return false;
				}

				var originalStatus = P9_StatusInfo.OriginalValue.ToString();
				if (originalStatus != ProcessTaskStatusCodeList.Codes.Closed && originalStatus != ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					return false;
				}

				var originalType = P9_TypeInfo.OriginalValue.ToString();
				if (originalType != ShelfsetTestTask && originalType != ExperimentalPullRequestTask)
				{
					return false;
				}

				return true;
			}
		}

		public ZBool HasShelfToAdd
		{
			get
			{
				return IsShelfsetTask
					&& P9_Status == ProcessTaskStatusCodeList.Codes.Assigned
					&& !P9_GS_NKAssignedStaffMember.IsEmpty
					&& HasChanges;
			}
		}

		public ZBool IsShelfCheckedIn
		{
			get
			{
				return IsInDatabase &&
					ReleaseRingsLookup.CheckInTaskTypes.Any(t => t == P9_TypeInfo.OriginalValue.ToString())
					&& (P9_StatusInfo.OriginalValue.ToString() == ProcessTaskStatusCodeList.Codes.Closed || P9_StatusInfo.OriginalValue.ToString() == ProcessTaskStatusCodeList.Codes.Cancelled);
			}
		}

		public ZPropertyInfo IsShelfCheckedInInfo
		{
			get { return GetZPropertyInfo(nameof(IsShelfCheckedIn)); }
		}

		public ZBool IsTaskMandatoryForShelfQueue
		{
			get
			{
				return EDIDataRegistry.Instance.TasksMandatoryInShelfCreation.Value.ContainsCode(P9_Type);
			}
		}

		public ZBool IsReviewTask
		{
			get { return Parent != null && Parent.Lookups.ActiveReviewTasks.ContainsCode(P9_Type); }
		}

		public ZBool IsClosedOrCancelled
		{
			get
			{
				return P9_Status == ProcessTaskStatusCodeList.Codes.Closed ||
					P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled;
			}
		}

		public override TaskPatchType TaskPatchType
		{
			get
			{
				if (!ReleaseRingsLookup.CheckInTaskTypesExcludingAlpha.Any(t => t == P9_Type))
				{
					return TaskPatchType.None;
				}
				else if (Iteration.IsEmpty)
				{
					return TaskPatchType.Auto;
				}
				else
				{
					return TaskPatchType.Manual;
				}
			}
		}

		#region ReleaseInfo

		[ResourceStringData("07d6f758-5b44-4058-9fc4-f20c1f72098a", Caption = "Release Info")]
		public ZString Release
		{
			get => release;
			set
			{
				release = value;
				ReleaseInfo.RefreshBinding();
			}
		}

		ZString release;

		public ZPropertyInfo ReleaseInfo => GetZPropertyInfo(nameof(Release));

		[ResourceStringData("45498583-b3c1-49b7-ab54-3bfb4fca006e", Caption = "Version No")]
		public ZString VersionNumber
		{
			get => versionNumber;
			set
			{
				versionNumber = value;
				VersionNumberInfo.RefreshBinding();
			}
		}

		ZString versionNumber;

		public ZPropertyInfo VersionNumberInfo => GetZPropertyInfo(nameof(VersionNumber));

		#endregion

		#region Shelf Management

		public override ZString P9_GS_NKAssignedStaffMember
		{
			get { return base.P9_GS_NKAssignedStaffMember; }
			set
			{
				base.P9_GS_NKAssignedStaffMember = value;
				MarkAsNeedingValidation();
				if (!IsValidationSuspended && HasChanges)
				{
					Validation.ValidateP9_Description();
				}
			}
		}

		public ZString ShelfOwnerName => AssignedStaffMember == null ? ZString.Empty : (ZString)(TfsRestContext.DomainName + "\\" + AssignedStaffMember.GS_LoginName);

		internal ZBool IsShelfUpdateSuspended;

		class ShelfUpdateSuspender : IDisposable
		{
			public ShelfUpdateSuspender(WorkItemProcessTask processTask)
			{
				this.ProcessTask = processTask;
			}

			readonly WorkItemProcessTask ProcessTask;

			public void Dispose()
			{
				ProcessTask.IsShelfUpdateSuspended = false;
			}
		}

		public IDisposable BeginShelfUpdateSuspender()
		{
			IsShelfUpdateSuspended = true;
			return new ShelfUpdateSuspender(this);
		}

		public void UpdateAssignedStaff(ZString ownerName)
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, SQLComparisonOperator.EndsWith, GetUserNameWithoutDomainName(ownerName)));
			if (staff != null)
			{
				P9_GS_NKAssignedStaffMember = staff.GS_Code;
			}
			else
			{
				P9_GS_NKAssignedStaffMember = "";
			}
		}

		public void AppendTextToTaskNote(string extraText)
		{
			string extraTextRTF = ORtfTextUtil.TextToRtf("\r\n\r\n" + extraText);
			extraTextRTF = ORtfTextUtil.AppendRtfStrings(P9_Notes.ToUTF8(), extraTextRTF);
			P9_Notes = ZBlob.FromUTF8(extraTextRTF);
		}

		public static IProcessTask CreateSkillLearningTask(IProcessTask referenceTask)
		{
			return CreateSkillLearningTask(referenceTask, referenceTask.P9_FH_ProcessHeader);
		}

		public static IProcessTask CreateSkillLearningTask(IProcessTask referenceTask, ZGuid parentHeader, int? sequence = null, string description = null, bool shouldHaveSkills = true)
		{
			var referenceTaskBizO = (ProcessTask)referenceTask;
			var newSequence = sequence ?? referenceTaskBizO.P9_Sequence + 1;
			if (string.IsNullOrEmpty(description))
			{
				description = Invariant($"Learning: {referenceTaskBizO.P9_Description}");
			}
			description = description.Substring(0, Math.Min(ProcessTasksSchema.P9_Description.MaxLength, description.Length));

			if (referenceTaskBizO.SkillsPivots.Count > 0 || !shouldHaveSkills)
			{
				var learningTaskType = EDIDataRegistry.Instance.CompetencyLearningTask.Value;
				var task = referenceTaskBizO
							.Parent
							.WorkflowItems
							.Where(t =>
								t.P9_GS_NKAssignedStaffMember == referenceTaskBizO.P9_GS_NKAssignedStaffMember &&
								t.P9_Sequence == newSequence &&
								t.P9_Type == learningTaskType &&
								t.P9_Description == description &&
								!t.IsClosed &&
								t.P9_FH_ProcessHeader == referenceTaskBizO.P9_FH_ProcessHeader)
							.FirstOrDefault();

				if (task == null)
				{
					task = referenceTaskBizO.Parent.WorkflowItems.AddNew();
					task.P9_GS_NKAssignedStaffMember = referenceTaskBizO.P9_GS_NKAssignedStaffMember;
					task.P9_G4_RequiredCapability = referenceTaskBizO.P9_G4_RequiredCapability;
					task.P9_Type = learningTaskType;
					task.P9_Description = description;
					task.P9_Sequence = newSequence;
					task.P9_FH_ProcessHeader = parentHeader;
					task.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();
					task.P9_NotesAsString = Invariant($"Complete the required competency tests in the Competency Requirements tab of task '{referenceTaskBizO.P9_Description}'.");
				}

				return task;
			}
			else
			{
				return null;
			}
		}

		public WorkItemProcessTask EnsureAssessReviewExists(AssessRequirementsState assessRequirements, IAssessServiceClient apiClient)
		{
			var task = GetOpenAssessReview();
			if (task == null)
			{
				var lastReviewToCopyCapabilities = WorkItemTaskHelper.GetCompletedOrCancelledReviewTasks(this).FirstOrDefault();
				if (lastReviewToCopyCapabilities != null)
				{
					task = CreateNewAssessReview(lastReviewToCopyCapabilities, assessRequirements, apiClient);
				}
			}

			return task;
		}

		public bool EnsureRequiredAspectReviewExists(string capabilityCode, string aspectName, string taskNotes)
		{
			if (string.IsNullOrEmpty(capabilityCode))
			{
				return false;
			}

			var capability = Factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.G4_Code, capabilityCode)).FirstOrDefault();

			if (capability == null)
			{
				CreateTasksToFixAspectCapability(capabilityCode, taskNotes);
				return true;
			}
			else
			{
				return EnsureRequiredAspectReviewExists(capability, aspectName, taskNotes);
			}
		}

		WorkItemProcessTask GetOpenAssessReview()
		{
			if (WorkItemProcessTask.IsCheckinTypeTask(P9_Type))
			{
				return GetAllTasksInWorkflowInOrder().Where(t => !t.IsClosed && t.P9_Sequence < P9_Sequence && t.P9_Type == CodeReviewTaskType).Reverse().FirstOrDefault();
			}
			else
			{
				return GetAllTasksInWorkflowInOrder().FirstOrDefault(t => !t.IsClosed && t.P9_Sequence >= P9_Sequence && t.P9_Type == CodeReviewTaskType);
			}
		}

		WorkItemProcessTask CreateNewAssessReview(ProcessTask lastReview, AssessRequirementsState competencyRequirements, IAssessServiceClient apiClient)
		{
			var notes = new StringBuilder(FormattableString.Invariant($"In order to proceed with the check-in, the submission needs to be reviewed by someone who has completed the following ASSESS "));
			if (competencyRequirements.AllRequirements.CompetencyRequirements.Length == 1)
			{
				notes.Append("course:");
			}
			else
			{
				notes.Append("courses:");
			}

			var task = Parent.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = lastReview.P9_GS_NKAssignedStaffMember;
			task.P9_G4_RequiredCapability = lastReview.P9_G4_RequiredCapability;
			task.P9_Type = CodeReviewTaskType;
			task.P9_Description = AssessReviewDescription;
			task.P9_Sequence = P9_Sequence == 0 ? 0 : P9_Sequence - 1;
			task.P9_FH_ProcessHeader = P9_FH_ProcessHeader;
			task.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();

			foreach (var requirement in competencyRequirements.AllRequirements.CompetencyRequirements)
			{
				task.SkillsPivots.AddAspect(requirement.AspectPK);
			}
			AddNotes(competencyRequirements.UnmetRequirements, isComplete: false);
			AddNotes(competencyRequirements.CompletedRequirements, isComplete: true);

			task.P9_NotesAsString = notes.ToString();
			EnrollReviewer(task.AssignedStaffMember, competencyRequirements.UnmetRequirements, apiClient);
			return task;

			void AddNotes(TaskCompetencyRequirements requirements, bool isComplete)
			{
				foreach (var text in GetHumanReadableText(requirements, isComplete))
				{
					notes.AppendLine();
					notes.Append(text);
					if (isComplete)
					{
						notes.Append(" [complete]");
					}
				}
			}

			IEnumerable<string> GetHumanReadableText(TaskCompetencyRequirements requirements, bool isComplete)
			{
				foreach (var requirement in requirements.CompetencyRequirements)
				{
					var text = WorkItemTaskHelper.GetHumanReadableText(requirement, apiClient);
					if (isComplete)
					{
						yield return text;
					}
					else
					{
						var urlResult = apiClient.GetLearningUnitUrlAsync(requirement.AspectPK).GetAwaiter().GetResult();
						if (urlResult.IsSuccess)
						{
							yield return $"{text} [{urlResult.Content}]";
						}
						else
						{
							yield return text;
						}
					}
				}
			}
		}

		static void EnrollReviewer(GlbStaff reviewer, TaskCompetencyRequirements competencyRequirements, IAssessServiceClient apiClient)
		{
			var reviewerPersonPK = reviewer?.GS_PER;
			if (!reviewerPersonPK.HasValue || reviewerPersonPK.Value.IsEmpty)
			{
				return;
			}

			foreach (var requirement in competencyRequirements.CompetencyRequirements.Distinct())
			{
				var parameters = new EnrolmentParameters(reviewer.GS_FullName, reviewer.GS_EmailAddress);
				apiClient.EnsureEnrolledAsync(requirement.AspectPK, reviewerPersonPK.Value.ToGuid(), parameters).GetAwaiter().GetResult();
			}
		}

		void EnsureReviewerEnrolledInCompetencyRequirements()
		{
			var requirements = new List<CompetencyRequirement>();
			foreach (ProcessTaskRequiredSkill pivot in SkillsPivots)
			{
				if (pivot.P9S_Aspect is { IsValid: true } guid)
				{
					requirements.Add(new CompetencyRequirement(guid.ToGuid()));
				}
			}
			if (requirements.Count == 0)
			{
				return;
			}

			var apiClient = ObjectFactory.Get<IAssessServiceClient>();
			var competencyRequirements = new TaskCompetencyRequirements(requirements.ToImmutableArray());
			EnrollReviewer(AssignedStaffMember, competencyRequirements, apiClient);
		}

		bool EnsureRequiredAspectReviewExists(GlbCapability capability, string aspectName, string taskNotes)
		{
			if (capability == null)
			{
				throw new ArgumentNullException(nameof(capability));
			}
			WorkItemProcessTask requiredAspectReview = null;

			var alltasksInOrder = GetAllTasksInWorkflowInOrder().Where(t => !t.IsClosed).ToArray();
			if (!alltasksInOrder.Contains(this))
			{
				if (IsClosed)
				{
					return false;
				}
				else
				{
					alltasksInOrder = new[] { this };
				}
			}
			var tasksAfterThis = alltasksInOrder.SkipWhile(t => !(t.P9_Sequence == P9_Sequence && t.P9_FH_ProcessHeader == P9_FH_ProcessHeader)).ToArray();

			WorkItemProcessTask nextCheckinOrLastTask = null;
			var reviewTasks = new List<WorkItemProcessTask>();

			foreach (WorkItemProcessTask t in tasksAfterThis)
			{
				if (IsReviewTypeTask(t.P9_Type.ToString()))
				{
					reviewTasks.Add(t);
				}
				if (IsCheckinTypeTask(t.P9_Type.ToString()))
				{
					nextCheckinOrLastTask = t;
					break;
				}
			}

			if (nextCheckinOrLastTask == null)
			{
				nextCheckinOrLastTask = tasksAfterThis.Length > 0 ? tasksAfterThis[tasksAfterThis.Length - 1] : this;
			}

			if (!reviewTasks.Any() && IsCheckinTypeTask(P9_Type.ToString()))
			{
				//edge case where checkin has aspect review as previous task and still open
				reviewTasks.AddRange(alltasksInOrder.Where(t => IsReviewTypeTask(t.P9_Type.ToString()) && t.P9_Sequence <= P9_Sequence));
			}

			if (reviewTasks.Any())
			{
				var capableAspectReviewTasks = FilterCapableAspectReviews(reviewTasks, aspectName, capability);
				if (capableAspectReviewTasks.Any())
				{
					requiredAspectReview = capableAspectReviewTasks.First();
				}
				else
				{
					var lastReviewInFirstReviewBlock = FindLastReviewInFirstReviewBlock(alltasksInOrder, reviewTasks);
					var newSeq = lastReviewInFirstReviewBlock.P9_Sequence + 1;
					if (IsCheckinTypeTask(nextCheckinOrLastTask.P9_Type.ToString()))
					{
						if (newSeq >= nextCheckinOrLastTask.P9_Sequence)
						{
							newSeq = nextCheckinOrLastTask.P9_Sequence - 1;
						}
						if (newSeq < 0)
						{
							newSeq = 0;
						}
					}

					requiredAspectReview = CreateOrUseAspectReviewTaskFromShelf(newSeq, capability, aspectName, lastReviewInFirstReviewBlock.P9_FH_ProcessHeader);
				}
			}
			else
			{
				var newSeq = IsCheckinTypeTask(nextCheckinOrLastTask.P9_Type.ToString())
						? nextCheckinOrLastTask.P9_Sequence > 0
							? nextCheckinOrLastTask.P9_Sequence - 1
							: 0
						: nextCheckinOrLastTask.P9_Sequence + 1;

				requiredAspectReview = CreateOrUseAspectReviewTaskFromShelf(newSeq, capability, aspectName, P9_FH_ProcessHeader);
			}

			var datTimestamp = Invariant($"DAT {Env.Time.FormatDateTime(Env.Time.CurrentLocalDateTime)}: ");
			if (!requiredAspectReview.P9_NotesAsString.Contains(taskNotes, StringComparison.Ordinal))
			{
				requiredAspectReview.AppendTextToTaskNote(datTimestamp + taskNotes);
			}
			return true;
		}

		WorkItemProcessTask FindLastReviewInFirstReviewBlock(WorkItemProcessTask[] alltasksInOrder, List<WorkItemProcessTask> reviewTasks)
		{
			WorkItemProcessTask lastReviewInFirstReviewBlock = null;
			foreach (var task in alltasksInOrder)
			{
				if (task == reviewTasks[0])
				{
					lastReviewInFirstReviewBlock = task;
				}
				else if (lastReviewInFirstReviewBlock != null)
				{
					if (IsReviewTypeTask(task.P9_Type.ToString()))
					{
						lastReviewInFirstReviewBlock = task;
					}
					else
					{
						break;
					}
				}
			}

			return lastReviewInFirstReviewBlock;
		}

		static IEnumerable<WorkItemProcessTask> FilterCapableAspectReviews(IEnumerable<WorkItemProcessTask> reviewTasks, string aspectName, GlbCapability capability)
		{
			var aspectReviewTasks = reviewTasks.Where(t => t.P9_Description == GetAspectReviewTaskDesctiption(aspectName));
			var capableAspectReviewTasks = aspectReviewTasks.Where(t => (t.RequiredCapability == capability && t.AssignedStaffMember == null) || (t.AssignedStaffMember != null && t.AssignedStaffMember.Capabilities.Contains(capability)));
			return capableAspectReviewTasks;
		}

		public IEnumerable<WorkItemProcessTask> GetAllTasksInWorkflowInOrder()
		{
			var topWorkflow = GetTopWorkflow();
			return GetOrderedChildren(topWorkflow, new HashSet<ZGuid>());
		}

		public IProcessHeader GetTopWorkflow()
		{
			var topWorkflows = ProcessHeader?.GetParentWorkflowsUpTheHierarchy()
				.Where(p =>
					p.IsWorkflow &&
					((!p.ParentLinks.Any() && p.JobHeader.PK == ProcessHeader.JobHeader.PK) ||
					(p.ParentLinks.Any() && p.ParentLinks.All(pl => !IsLinkToWorkflowWithinJob(pl, ProcessHeader.JobHeader)))))
				.Distinct()
				.ToList();

			IProcessHeader topWorkflow;
			if (topWorkflows.IsNullOrEmpty())
			{
				topWorkflow = ProcessHeader;
			}
			else
			{
				if (topWorkflows.Count > 1)
				{
					ErrorReporter.ReportOnce(
						FormattableString.Invariant($"Task {P9_TaskID} found more than one top level Workflows in {Parent.Number}"),
						FormattableString.Invariant($"There should be only one top level Workflows from task {P9_TaskID} in {Parent.Number} but there was: {System.Environment.NewLine + string.Join(System.Environment.NewLine, topWorkflows)}"));
				}

				topWorkflow = topWorkflows.FirstOrDefault();
			}

			return topWorkflow;
		}

		IEnumerable<WorkItemProcessTask> GetOrderedChildren(IProcessHeader rootParent, HashSet<ZGuid> seen)
		{
			if (rootParent != null)
			{
				foreach (var task in rootParent.Tasks.OrderBy(t => t.P9_Sequence).ThenBy(t => OrderByTaskType(t)))
				{
					if (task is WorkItemProcessTask workItemProcessTask)
					{
						if (seen.Add(task.PK))
						{
							yield return workItemProcessTask;

							foreach (IProcessTaskIterationLink iterationLink in workItemProcessTask.IterationLinks)
							{
								var iterationWorkflow = Factory.Load<IProcessHeader>(iterationLink.P9I_FH_IterationWorkflow);
								if (iterationWorkflow != null)
								{
									foreach (var child in GetOrderedChildren(iterationWorkflow, seen))
									{
										yield return child;
									}
								}
							}
						}
					}
				}
			}
		}

		static int OrderByTaskType(IProcessTask t)
		{
			if (IsReviewTypeTask(t.P9_Type.ToString()))
			{
				return 1;
			}
			else if (IsCheckinTypeTask(t.P9_Type.ToString()))
			{
				return 2;
			}
			else
			{
				return 0;
			}
		}

		static bool IsLinkToWorkflowWithinJob(IProcessHeaderLink link, IProcessJobHeader jobHeader)
		{
			return link.FP_FH_HeaderTo != jobHeader.PK && link.HeaderTo != null && link.HeaderTo.FH_ParentId == jobHeader.FH_ParentId;
		}

		WorkItemProcessTask CreateOrUseAspectReviewTaskFromShelf(int taskSeqNum, GlbCapability capability, string aspectName, ZGuid processHeader)
		{
			if (capability == null)
			{
				throw new ArgumentNullException(nameof(capability));
			}

			var task = Parent
				.WorkflowItems
				.Where(t =>
					t.P9_Sequence == taskSeqNum &&
					t.P9_G4_RequiredCapability == capability.PK &&
					t.P9_Type == AspectReviewTaskType &&
					t.P9_Description == GetAspectReviewTaskDesctiption(aspectName) &&
					!t.IsClosed &&
					t.P9_FH_ProcessHeader == processHeader)
				.FirstOrDefault() as WorkItemProcessTask;

			if (task == null)
			{
				task = Parent.WorkflowItems.AddNew();
				task.P9_G4_RequiredCapability = capability.PK;
				task.P9_Type = AspectReviewTaskType;
				task.P9_Description = GetAspectReviewTaskDesctiption(aspectName);
				task.P9_Sequence = taskSeqNum;
				task.P9_GS_NKAssignedStaffMember = "";
				task.P9_FH_ProcessHeader = processHeader;
				task.P9_EstDuration = new ZInt(10).GetDateTimeFromMinutes();
			}

			return task;
		}

		void CreateTasksToFixAspectCapability(string capabilityCode, string aspectReviewNote)
		{
			var datAdmins = Factory.Load<GlbCapability>(new ZQuery(GlbCapabilitySchema.G4_Code, "DAT")).FirstOrDefault();

			var invTask = CreateNewTask("INV");
			invTask.P9_GS_NKAssignedStaffMember = P9_GS_NKAssignedStaffMember;
			invTask.AppendTextToTaskNote(
				"Please determine the correct capability for this aspect review," +
				" and communicate with DAT admins to get it fixed in DAT.");

			var astTask = CreateNewTask("AST");
			astTask.P9_G4_RequiredCapability = datAdmins.PK;
			astTask.P9_GS_NKAssignedStaffMember = null;
			astTask.AppendTextToTaskNote(
				Invariant($"Please assist {P9_GS_NKAssignedStaffMember} to fix DAT's aspect review capability assignment."));

			WorkItemProcessTask CreateNewTask(string taskType)
			{
				var task = Parent.WorkflowItems.AddNew();
				task.P9_Sequence = P9_Sequence;
				task.P9_FH_ProcessHeader = P9_FH_ProcessHeader;
				task.P9_Type = taskType;
				task.P9_Description = "Fix aspect capability assignment in DAT";

				task.AppendTextToTaskNote($"The aspect capability '{capabilityCode}' could not be found.");
				task.AppendTextToTaskNote(aspectReviewNote.Replace("Please review", "See"));

				return task;
			}
		}

		public static bool IsCheckinTypeTask(string type) => type.StartsWith("CH", StringComparison.Ordinal);

		public const string AspectReviewTaskType = "CBS";
		public const string CodeReviewTaskType = "CBC";
		public const string GeneralReviewTaskType = "CBR";
		public const string CodingTaskType = "CDF";
		public const string AssessReviewDescription = "ASSESS Review";

		public static bool IsReviewTypeTask(string type) => type.StartsWith("CB", StringComparison.Ordinal);

		static string GetAspectReviewTaskDesctiption(string aspectName) => Invariant($"Aspect Review - {aspectName.Substring(0, Math.Min(24, aspectName.Length))} (with CB)");

		#endregion

		#region Defect Count

		const string DefectCountFieldName = "P9_WI_DefectCount";

		public ZInt DefectCount
		{
			get { return this.GetSystemDefinedValue<ZInt>(DefectCountFieldName); }
			set
			{
				this.SetSystemDefinedValue(DefectCountFieldName, value);
				DefectCountInfo.RefreshBinding();
				if (!IsValidationSuspended)
				{
					((WorkItemProcessTaskValidation)Validation).ValidateDefectCount();
				}
			}
		}

		public ZPropertyInfo DefectCountInfo
		{
			get { return GetZPropertyInfo(nameof(DefectCount)); }
		}

		#endregion

		#endregion

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			LogShelfReviewWarningTime();
			ProcessShelf();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				EnsureReviewerEnrolledInCompetencyRequirements();
			}
		}

		void LogShelfReviewWarningTime()
		{
			if (ReleaseRingsLookup.GetRingCodeByCheckInType(P9_Type) == ReleaseRings.Codes.ALP && P9_Status == ProcessTaskStatusCodeList.Codes.Assigned
				&& MinutesElapsedForShelfCreationTimeLog > 0)
			{
				ZQuery logFilter = new ZQuery(StmALogSchema.SL_Parent, PK);
				logFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, shelfReviewTimeWarningLogReference);
				if (!Logs.HasLogWith(logFilter))
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					Logs.AddNew(Events.EditedARecord, shelfReviewTimeWarningLogReference + MinutesElapsedForShelfCreationTimeLog.Round(0));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		const string shelfReviewTimeWarningLogReference = "Shelf Review Warning:";

		#endregion

		#region Related Business Objects

		public new NewWorkItem Parent
		{
			get { return (NewWorkItem)base.Parent; }
		}

		protected override Type ParentType
		{
			get { return typeof(NewWorkItem); }
		}

		#endregion

		public ZDecimal MinutesElapsedForShelfCreationTimeLog { get; set; }

		protected override ZQuery GetSuspendedTasksQuery()
		{
			ZQuery query = base.GetSuspendedTasksQuery();
			query.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, ReleaseRingsLookup.CheckInTaskTypes.ToArray());
			return query;
		}

		string GetUserNameWithoutDomainName(string userName)
		{
			Argument.NotNull(userName, nameof(userName)); // Suggested By ReviewBot
			string result = userName;
			int index = result.IndexOf('\\');
			if (index != -1)
			{
				result = result.Substring(index + 1);
			}
			return result;
		}

		#region Delete

		public override void Delete()
		{
			if (IsShelfsetTask)
			{
				EDIShelvesetInfo shelf = ShelvesetTools.LoadShelfByProcessTask(this);
				if (shelf != null && !shelf.HasBeenBuilt && Parent != null)
				{
					ShelfUpdaterService.GetInstance(Factory).SubmissionsToRemove.Add(shelf);
				}
			}

			base.Delete();
		}

		#endregion

		#region Shelf Operations

		void ProcessShelf()
		{
			if (!IsShelfUpdateSuspended && (!IsInDatabase || HasShelfAlteringChanges))
			{
				var shelfUpdaterService = ShelfUpdaterService.GetInstance(Factory);
				if (HasShelfToAdd)
				{
					var shelfInDeleteList = shelfUpdaterService.SubmissionsToRemove.SingleOrDefault(s => s.RelatedProcessTask.PK == this.PK);
					if (shelfInDeleteList != null)
					{
						shelfUpdaterService.SubmissionsToRemove.Remove(shelfInDeleteList);
					}
					else
					{
						var shelfToAdd = new EDIShelvesetInfo(ShelfOwnerName, null, NewWorkItem.GetActionByShelfsetTaskType(P9_Type), this);
						shelfUpdaterService.SubmissionsToSchedule.Add(shelfToAdd);
					}
				}
				else if (WasShelfsetTask && P9_StatusInfo.OriginalValue.ToString() == ProcessTaskStatusCodeList.Codes.Assigned
					|| (IsShelfsetTask && (P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled || P9_Status == ProcessTaskStatusCodeList.Codes.Suspended)))
				{
					var shelfInAddList = shelfUpdaterService.SubmissionsToSchedule.SingleOrDefault(s => s.RelatedProcessTask.PK == this.PK);

					if (shelfInAddList != null)
					{
						shelfUpdaterService.SubmissionsToSchedule.Remove(shelfInAddList);
					}
					else
					{
						var shelfToCancel = new EDIShelvesetInfo(ShelfOwnerName, P9_Description, NewWorkItem.GetActionByShelfsetTaskType(P9_Type), this);
						shelfUpdaterService.SubmissionsToRemove.Add(shelfToCancel);
					}
				}
			}
		}

		bool HasShelfAlteringChanges => P9_StatusInfo.HasChanges || P9_TypeInfo.HasChanges || P9_GS_NKAssignedStaffMemberInfo.HasChanges || P9_DescriptionInfo.HasChanges || NotesTextChanged;

		bool NotesTextChanged
		{
			get
			{
				if (P9_NotesInfo.HasChanges)
				{
					return !ORtfTextUtil.RtfToText((ZBlob)P9_NotesInfo.OriginalValue).Trim().Equals(ORtfTextUtil.RtfToText(P9_Notes).Trim());
				}
				return false;
			}
		}

		#endregion

		#region Validation

		protected override ProcessTasksValidation GetNewValidation()
		{
			ProcessTasksValidation result = null;
			if (IsTask)
			{
				result = new WorkItemProcessTaskValidation(this);
			}
			else
			{
				result = base.GetNewValidation();
			}
			return result;
		}

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var newProcessTask = (ProcessTask)base.CloneInternal(args);

			switch (P9_Type)
			{
				case EDITaskTypes.TaskActiveShelfTest:
					newProcessTask.P9_Description = ZString.Empty;
					newProcessTask.P9_Type = EDITaskTypes.TaskShelfTest;
					break;
				case EDITaskTypes.TaskActiveCheckin:
					newProcessTask.P9_Description = ZString.Empty;
					newProcessTask.P9_Type = EDITaskTypes.TaskCheckin;
					break;
				case EDITaskTypes.TaskActiveUATBuild:
					newProcessTask.P9_Description = ZString.Empty;
					newProcessTask.P9_Type = EDITaskTypes.TaskUATBuild;
					break;
				case EDITaskTypes.TaskActiveAspectOnlyBuild:
					newProcessTask.P9_Description = ZString.Empty;
					newProcessTask.P9_Type = EDITaskTypes.TaskAspectOnlyBuild;
					break;
				case EDITaskTypes.TaskActiveExperimentalPullRequest:
					newProcessTask.P9_Description = ZString.Empty;
					newProcessTask.P9_Type = EDITaskTypes.TaskExperimentalPullRequest;
					break;
			}

			return newProcessTask;
		}

		#endregion
	}
}
