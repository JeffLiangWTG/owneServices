using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ServiceTask;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.DevTools.Definitions;
using WTG.DevTools.ServiceClient.Assess;
using static System.FormattableString;

[assembly: HostedService(
	Enterprise.Client.EDI.ProcessedShelfsServiceTask.Code,
	"Processed Shelves Service Task",
	"CSP",
	typeof(Enterprise.Client.EDI.ProcessedShelfsServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "30Minutes",
	DefaultScheduleRunEvery = "30minutes",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI
{
	[NeedsDataRefresh]
	public class ProcessedShelfsServiceTask : ServiceProviderImpl
	{
		public const string Code = "HPS";

		public override void RunTask(CancellationToken token)
		{
			try
			{
				var branch = ServiceTaskHelper.GetBranchForEDIServiceTasks(new BusinessObjectFactory { NameForDebugging = nameof(ProcessedShelfsServiceTask) });
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				using (var submissionsProvider = CreateDatSubmissionsProvider())
				{
					if (!submissionsProvider.CanProvideSubmissions)
					{
						ServiceLogger.Error("Crikey db server not configured, this is probably not the production instance of ediProd");
						return;
					}

					while (true)
					{
						token.ThrowIfCancellationRequested();
						var submissionsBatch = submissionsProvider.GetNextBatch();
						if (submissionsBatch.Submissions.Count == 0)
						{
							break;
						}

						try
						{
							ProcessBatch(submissionsBatch, submissionsProvider, token);
						}
						catch (ZSaveConcurrencyException)
						{
							ServiceLogger.Log(LogType.Information, "Concurrency error encountered; retrying batch.");
							continue;
						}
						catch (SubmissionCannotBeProcessedException)
						{
							ServiceLogger.Log(LogType.Information, "Error encountered when processing a submission; retrying batch (excluding that submission).");
							continue;
						}

						if (submissionsBatch.IsLastBatch)
						{
							break;
						}
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				var aggregateEx = ex as AggregateException;
				var isTaskCanceled = ex is TaskCanceledException
					|| ex is OperationCanceledException
					|| (aggregateEx != null && (aggregateEx.InnerException is TaskCanceledException || aggregateEx.InnerException is OperationCanceledException));

				if (!isTaskCanceled)
				{
					const string errorMessage = "RunTask Failed in ProcessedShelfsServiceTask";
					ErrorReporter.ReportOnce(errorMessage, ex);
					ServiceLogger.Error(errorMessage, ex);
				}
				throw;
			}
		}

		void ProcessBatch(DatSubmissionBatch submissionsBatch, IDatSubmissionsProvider submissionsProvider, CancellationToken token)
		{
			foreach (var shelf in submissionsBatch.Submissions)
			{
				token.ThrowIfCancellationRequested();
				try
				{
					if (!HasDatAlreadyProcessedShelf(shelf))
					{
						ServiceLogger.Log(LogType.Information, $"Processing {shelf.ActionType} submission '{shelf.DisplayName}' from task: {shelf.RelatedProcessTask?.P9_TaskID ?? "Unknown RelatedProcessTask"}");
						UpdateRelatedProcessTask(shelf);

						var assessRequirementsForReviewer = GetAssessRequirementsStateForReviewer(shelf);
						var assessRequirementsForCodeAuthor = GetAssessRequirementsStateForCodeAuthor(shelf, assessRequirementsForReviewer);

						if (!IsSuccessfulCheckin(shelf) && !assessRequirementsForReviewer.IsEmpty)
						{
							UpdateReviewTask(shelf, assessRequirementsForReviewer);
							if (shelf.IsTestRun)
							{
								AddLearningTasksForCodeAuthor(shelf, assessRequirementsForCodeAuthor);
							}
							ServiceLogger.Log(LogType.Debug, $"Processed ASSESS info for submission '{shelf.DisplayName}'");
						}

						SendNotificationEmailToOwner(submissionsBatch.BatchFactory, shelf, assessRequirementsForCodeAuthor, assessRequirementsForReviewer);
						if (shelf.Status == ShelfStatuses.CheckedIn)
						{
							MarkRelatedIncidentsAsAwaitingAutoDeploy(shelf);
							ServiceLogger.Log(LogType.Debug, $"Marked related incidents as awaiting auto deploy for submission '{shelf.DisplayName}'");
						}

						ServiceLogger.Log(LogType.Debug, $"Saving batch factory");
						submissionsBatch.BatchFactory.Save();
					}
					else
					{
						ServiceLogger.Log(LogType.Information, $"Already Processed {shelf.ActionType} submission '{shelf.DisplayName}' from task: {shelf.RelatedProcessTask?.P9_TaskID ?? "Unknown RelatedProcessTask"}");
					}

					submissionsProvider.UpdateStatusToNotified(shelf);
					ServiceLogger.Log(LogType.Information, $"Updated Dat Status for submission '{shelf.DisplayName}' from task: {shelf.RelatedProcessTask?.P9_TaskID ?? "Unknown RelatedProcessTask"}");
				}
				catch (Exception ex) when (!ex.IsCriticalException() && ex is not ZSaveConcurrencyException)
				{
					var errorMessage = FormattableString.Invariant($"Unable to process submission '{shelf.DisplayName}' from task: {shelf.RelatedProcessTask?.P9_TaskID ?? "Unknown RelatedProcessTask"}");
					ErrorReporter.ReportOnce(errorMessage, ex);
					ServiceLogger.Error(errorMessage, ex);
					submissionsProvider.NotifySubmissionCouldNotBeProcessed(shelf);
					throw new SubmissionCannotBeProcessedException();
				}
			}
		}

		AssessRequirementsState GetAssessRequirementsStateForReviewer(EDIShelvesetInfo shelf)
		{
			var taskInfo = shelf.RelatedProcessTask != null
				? $"TaskID:'{shelf.RelatedProcessTask.P9_TaskID}', Pk:{shelf.RelatedProcessTask.PK}"
				: "Task Not Found";
			if (shelf.AspectReviews == null || !shelf.AspectReviews.Any(r => r.IsAssess))
			{
				ServiceLogger.Debug(Invariant($"ASSESS review requirements not found for submission: '{shelf.DisplayName}', Related task: {taskInfo}"));
				return AssessRequirementsState.Empty;
			}

			if (shelf.RelatedProcessTask == null)
			{
				return AssessRequirementsState.Empty;
			}

			return WorkItemTaskHelper.GetAssessRequirementsCompletionState(shelf.RelatedProcessTask, shelf, AssessServiceClient);
		}

		AssessRequirementsState GetAssessRequirementsStateForCodeAuthor(EDIShelvesetInfo shelf, AssessRequirementsState assessRequirementsForReviewer)
		{
			var author = shelf?.RelatedProcessTask?.AssignedStaffMember;
			if (author == null)
			{
				return AssessRequirementsState.AllRequirementsUnmet(assessRequirementsForReviewer.AllRequirements, ImmutableArray<GlbStaff>.Empty);
			}
			return WorkItemTaskHelper.GetAssessRequirementsCompletionState(assessRequirementsForReviewer.AllRequirements, AssessServiceClient, author);
		}

		void UpdateReviewTask(EDIShelvesetInfo shelf, AssessRequirementsState assessRequirements)
		{
			var shelfTask = shelf?.RelatedProcessTask;
			if (shelfTask == null)
			{
				return;
			}

			var reviewTask = shelf.NextCodeReviewTask;
			var isCheckinTask = WorkItemProcessTask.IsCheckinTypeTask(shelfTask.P9_Type);
			if (reviewTask == null && isCheckinTask && !assessRequirements.UnmetRequirements.IsEmpty)
			{
				reviewTask = shelfTask.EnsureAssessReviewExists(assessRequirements, AssessServiceClient);
			}
			if (reviewTask == null)
			{
				ServiceLogger.Debug(Invariant($"Next Code Review Task not found for submission: {shelf.DisplayName}, Related task: {shelf.RelatedProcessTask?.PK ?? ZGuid.Empty}"));
				return;
			}

			reviewTask.SkillsPivots.RemoveAndDeleteAll();
			foreach (var requirement in assessRequirements.AllRequirements.CompetencyRequirements)
			{
				reviewTask.SkillsPivots.AddAspect(requirement.AspectPK);
				ServiceLogger.Debug(Invariant($"Added aspect {requirement.AspectPK} to Code Review Task {reviewTask.PK}."));
			}
		}

		void AddLearningTasksForCodeAuthor(EDIShelvesetInfo shelf, AssessRequirementsState assessRequirements)
		{
			var shelfTask = shelf?.RelatedProcessTask;
			if (shelfTask != null)
			{
				var (refTask, newSequence) = FindReferenceTaskAndNextSequence(shelf, shelfTask);

				foreach (var requirement in assessRequirements.UnmetRequirements.CompetencyRequirements)
				{
					var urlResult = AssessServiceClient.GetLearningUnitUrlAsync(requirement.AspectPK).GetAwaiter().GetResult();
					if (!urlResult.IsSuccess)
					{
						continue;
					}
					var courseDetails = (WorkItemTaskHelper.GetHumanReadableText(requirement, AssessServiceClient), urlResult.Content);
					CreateLearningTask(refTask, newSequence, courseDetails);
				}
			}

			void CreateLearningTask(ProcessTask referenceTask, int sequence, (string name, string url) requirementDetails)
			{
				var taskDescription = Invariant($"(Optional) {requirementDetails.name}");
				var learningTask = (ProcessTask)WorkItemProcessTask.CreateSkillLearningTask(shelfTask, referenceTask.P9_FH_ProcessHeader, sequence, taskDescription, false);
				var notes = Invariant($@"DAT has detected an ASSESS course relevant to your submission. Please consider taking this course using the link below to help improve your knowledge in this area. It is optional, and you may cancel this task if you do not wish to complete the course now.
DAT requires that code reviewers complete all ASSESS courses relevant to the submission, so taking the course will allow you to review code like this in the future.

{requirementDetails.name}
{requirementDetails.url}");
				learningTask.P9_NotesAsString = notes;
				ServiceLogger.Debug(Invariant($"Added task: {learningTask.P9_Description}, submission: {shelf.DisplayName}, Related task: {shelfTask?.PK ?? ZGuid.Empty}"));
			}
		}

		(WorkItemProcessTask refTask, ZInt newSequence) FindReferenceTaskAndNextSequence(EDIShelvesetInfo shelf, WorkItemProcessTask shelfTask)
		{
			WorkItemProcessTask refTask = null;
			ZInt newSequence;
			if (shelf.NextCodeReviewTask == null)
			{
				ServiceLogger.Debug(Invariant($"Next Code Review Task not found. submission: {shelf.DisplayName}, Related task: {shelfTask?.PK ?? ZGuid.Empty}"));
			}
			if (shelf.Status == ShelfStatuses.Passed)
			{
				refTask = shelf.NextCodeReviewTask ?? shelfTask;
				newSequence = refTask.P9_Sequence;
			}
			else
			{
				if (shelfTask.IterationLinks.Count == 0)
				{
					var alltasksInOrder = shelfTask.GetAllTasksInWorkflowInOrder().Where(t => t is WorkItemProcessTask).ToArray();
					var currentTaskInWorkflow = alltasksInOrder.SkipWhile(t => !t.IsCurrent).FirstOrDefault();

					refTask = currentTaskInWorkflow ?? shelfTask;
					newSequence = refTask.P9_Sequence == 0 ? 0 : refTask.P9_Sequence - 1;
				}
				else
				{
					foreach (IProcessTaskIterationLink iterationLink in shelfTask.IterationLinks)
					{
						var iterationWorkflow = shelfTask.Factory.Load<IProcessHeader>(iterationLink.P9I_FH_IterationWorkflow);
						if (iterationWorkflow != null)
						{
							refTask = iterationWorkflow.Tasks.OrderBy(t => t.P9_Sequence).FirstOrDefault(t => t is WorkItemProcessTask) as WorkItemProcessTask;
							if (refTask != null)
							{
								break;
							}
						}
					}

					refTask = refTask ?? shelfTask;
					newSequence = refTask.P9_Sequence == 0 ? 0 : refTask.P9_Sequence - 1;
				}
			}
			return (refTask, newSequence);
		}

		bool HasDatAlreadyProcessedShelf(EDIShelvesetInfo shelf)
		{
			return shelf?.RelatedProcessTask?.P9_NotesAsString.Contains(shelf.UserHeaderPK.ToString(), StringComparison.Ordinal) ?? false;
		}

		void MarkRelatedIncidentsAsAwaitingAutoDeploy(EDIShelvesetInfo shelf)
		{
			var workItem = shelf?.RelatedProcessTask?.Parent;
			if (workItem != null)
			{
				foreach (BusinessObject relatedItem in workItem.RelatedItems)
				{
					if (relatedItem is SupportIncident incident)
					{
						var releaseRing = ReleaseRingsLookup.GetRingCodeByCheckInType(shelf.RelatedProcessTask.P9_Type);
						bool shouldMark = false;
						if (incident.ClientReleaseRing == releaseRing)
						{
							shouldMark = !(from NewWorkItem incidentWorkItem in incident.RelatedWorkItems
										   where incidentWorkItem.HasOpenedShelfCheckInTasksForReleaseRing(releaseRing)
										   select incidentWorkItem).Any();
						}
						else if ((releaseRing == ReleaseRings.Codes.GPR) && (incident.ClientReleaseRing == ReleaseRings.Codes.GPC) && !Builds.GpcReleaseBuildIsAvailable)
						{
							shouldMark = true;
							foreach (NewWorkItem incidentWorkItem in incident.RelatedWorkItems)
							{
								if (incidentWorkItem.HasOpenedShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.GPR))
								{
									shouldMark = false;
									break;
								}
								else if (incidentWorkItem.HasShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.GPC))
								{
									if (incidentWorkItem.HasOpenedShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.GPC)
											|| !incidentWorkItem.HasCancelledShelfCheckInTasksForReleaseRing(ReleaseRings.Codes.GPC))
									{
										shouldMark = false;
										break;
									}
								}
							}
						}

						if (shouldMark)
						{
							if (incident.IM_Category == SupportIncidentCategoriesList.Codes.Support)
							{
								incident.IM_Status = IncidentMainLookups.Status.Closed; // IncidentConstants.IncidentStatus.Closed;
							}
							else if (incident.NeedUpgrade)
							{
								incident.WaitForUpgrade();
							}
						}
					}
				}
			}
		}

		#region Emailing

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		internal (string subject, string message) GetSubjectAndTestSummary(EDIShelvesetInfo submission, AssessRequirementsState assessRequirementsForReviewer)
		{
			var subjectStatus = StatusToResultDictionary[submission.Status];
			var subjectPrefix = GetPrefixFromActionType(submission.ActionType);
			var subjectAction = GetSubjectActionResult(submission);
			var emailSubject = Invariant($"{subjectStatus}: {subjectPrefix}{submission.DisplayName} {subjectAction}");

			var emailMessage = new ZStringBuilder();
			if (submission.Status == ShelfStatuses.RejectedForPendingAspectData)
			{
				emailMessage.Append(GetReviewRequirementMessage(submission, assessRequirementsForReviewer));
			}
			emailMessage.Append("<br />");
			if (submission.Status == ShelfStatuses.DeploymentJobFailed)
			{
				emailMessage.Append("A deployment job has failed.<br />");
			}

			foreach (var gitPull in submission.DatGitPullRequests)
			{
				var prefix = GetPrefixFromActionType(submission.ActionType);
				emailMessage.Append(Invariant($@"{prefix}""{gitPull.Title}"" {TranslateTestResult(gitPull.Status, submission.ActionType)}: <a href=""{gitPull.OverviewUri}"">{gitPull.OverviewUri}</a><br />"));
			}

			return (emailSubject, emailMessage.ToString());
		}

		static string GetReviewRequirementMessage(EDIShelvesetInfo submission, AssessRequirementsState assessRequirementsForReviewer)
		{
			var hasAspects = submission.AspectReviews.Any(ar => !string.IsNullOrEmpty(ar.Capability));
			var hasAssess = submission.AspectReviews.Any(ar => ar.IsAssess);
			var hasOneReviewer = assessRequirementsForReviewer.StaffForWhomAssessIsRequired.Length == 1;
			var hasMultipleReviewers = assessRequirementsForReviewer.StaffForWhomAssessIsRequired.Length > 1;

			if (!hasAspects && !hasAssess)
			{
				return string.Empty;
			}
			if (hasAspects && !hasAssess)
			{
				return "Your submission was rejected because there are pending Aspect reviews.";
			}

			var reviewRequirementsDescription = hasAspects && hasAssess ? "pending Aspect reviews and ASSESS requirements" : "ASSESS requirements";

			if (hasOneReviewer)
			{
				return $"Your submission was rejected because there are {reviewRequirementsDescription}, and the code reviewer ({GetReviewerDetails()}) has not passed the corresponding ASSESS courses.";
			}
			else if (hasMultipleReviewers)
			{
				return $"Your submission was rejected because there are {reviewRequirementsDescription}, and the code reviewers ({GetReviewerDetails()}) have not collectively passed the corresponding ASSESS courses.";
			}
			else
			{
				return $"Your submission was rejected because there are {reviewRequirementsDescription}, but DAT could not find a completed code review task for this submission. This task must be present in the same workflow as the submission task, or in a prerequisite workflow.";
			}

			string GetReviewerDetails()
			{
				return string.Join("; ", assessRequirementsForReviewer.StaffForWhomAssessIsRequired.Select(s => $"{s.GS_Code} - {s.GS_FullName}"));
			}
		}

		static string GetPrefixFromActionType(string actionType)
		{
			var prefix = string.Empty;
			if (actionType == EDIShelvesetInfo.ActionTypes.AspectOnlyBuild)
			{
				prefix = "Aspect Build ";
			}
			else if (actionType == EDIShelvesetInfo.ActionTypes.UATBuild)
			{
				prefix = "UAT Build ";
			}
			else if (actionType == EDIShelvesetInfo.ActionTypes.UATCombinedBuild)
			{
				prefix = "UAT Combined Build ";
			}
			else if (actionType == EDIShelvesetInfo.ActionTypes.UATCombinedChild)
			{
				prefix = "UAT Combined Child ";
			}
			return prefix;
		}

		static string GetSubjectActionResult(EDIShelvesetInfo shelf)
		{
			var result = TranslateTestResult(shelf.Status, shelf.ActionType);
			if (shelf.Status == ShelfStatuses.DeploymentJobFailed)
			{
				result += " but has a failed deployment job";
			}
			return result;
		}

		internal static string TranslateTestResult(string status, string actionType)
		{
			var testResult = string.Empty;
			if (status == ShelfStatuses.DeploymentJobFailed)
			{
				status = actionType == EDIShelvesetInfo.ActionTypes.ShelfCheckin ? ShelfStatuses.CheckedIn : ShelfStatuses.Passed;
			}

			if (!string.IsNullOrEmpty(status))
			{
				switch (status)
				{
					case ShelfStatuses.Passed:
						switch (actionType)
						{
							case EDIShelvesetInfo.ActionTypes.AspectOnlyBuild:
								testResult = "has been aspected";
								break;
							case EDIShelvesetInfo.ActionTypes.UATBuild:
							case EDIShelvesetInfo.ActionTypes.UATCombinedBuild:
								testResult = "has been built";
								break;
							default:
								testResult = "has passed";
								break;
						}

						break;
					case ShelfStatuses.CheckedIn:
						testResult = "has completed";
						break;
					case ShelfStatuses.Rejected:
						testResult = "has been rejected";
						break;
					case ShelfStatuses.RejectedForPendingAspectData:
						testResult = "has been rejected due to pending review requirements";
						break;
					default:
						testResult = "has " + status;
						break;
				}
			}

			return testResult;
		}

		void SendNotificationEmailToOwner(BusinessObjectFactory factory, EDIShelvesetInfo shelf, AssessRequirementsState assessRequirementsForCodeAuthor, AssessRequirementsState assessRequirementsForReviewer)
		{
			if (shelf == null)
			{
				return;
			}

			var (shouldNotify, recipient) = GetNotificationInfo(factory, shelf);
			if (recipient.IsEmpty || !shouldNotify)
			{
				ServiceLogger.Log(LogType.Debug, $"No email sent, recipient: '{recipient}', shouldNotify: '{shouldNotify}' for submission '{shelf.DisplayName}'");
				return;
			}

			var email = new HtmlEmailDef();
			var (subject, message) = GetSubjectAndTestSummary(shelf, assessRequirementsForReviewer);

			email.Subject = subject;
			email.Body = message;

			var testResultsUrl = CrikeyWeb.GetTestResultsUrl(shelf.UserHeaderPK);
			email.Body += Invariant($@"<br />See <a href=""{testResultsUrl}"">{testResultsUrl}</a> for details.<br />");

			if (shelf.RelatedProcessTask?.Parent != null)
			{
				var workItem = shelf.RelatedProcessTask.Parent;
				var workItemURL = ShowEditFormUrlHandler.Instance.Create(ControllerIDs.WorkItem, workItem.PK);
				email.Body += Invariant($"<br /><a href=\"{workItemURL}\">{WebUtility.HtmlEncode(workItem.Number.ToString())} - {workItem.WKI_Summary}</a><br />");

				var assignedStaff = shelf.RelatedProcessTask.AssignedStaffMember;
				if (!assessRequirementsForCodeAuthor.UnmetRequirements.IsEmpty && assignedStaff != null)
				{
					email.Body += Invariant($@"<br />We have detected the following areas of knowledge relevant to your submission. We suggest you complete these courses by following the links below:<br />");

					foreach (var (humanReadableDescription, url) in GetUnmetRequirementsDetails(assessRequirementsForCodeAuthor))
					{
						email.Body += Invariant($@"<a href=""{url}"">{humanReadableDescription}</a><br />");
					}
				}
			}

			email.AddRecipientForUserCommunication(recipient);

			Env.OutgoingMailManager.Create(factory, email);
			ServiceLogger.Log(LogType.Debug, $"Email sent to {recipient} for submission '{shelf.DisplayName}'");
		}

		IEnumerable<(string humanReadableDescription, string url)> GetUnmetRequirementsDetails(AssessRequirementsState assessRequirements)
		{
			foreach (var requirement in assessRequirements.UnmetRequirements.CompetencyRequirements)
			{
				var description = WorkItemTaskHelper.GetHumanReadableText(requirement, AssessServiceClient);
				var urlResult = AssessServiceClient.GetLearningUnitUrlAsync(requirement.AspectPK).GetAwaiter().GetResult();
				if (urlResult.IsSuccess)
				{
					yield return (description, urlResult.Content);
				}
			}
		}

		(bool shouldNotify, ZString recipient) GetNotificationInfo(BusinessObjectFactory factory, EDIShelvesetInfo shelf)
		{
			var shouldNotify = shelf.Status == ShelfStatuses.CheckedIn || shelf.IsRejectedStatus || (shelf.Status == ShelfStatuses.Passed && shelf.ActionType != EDIShelvesetInfo.ActionTypes.UATCombinedChild) || shelf.Status == ShelfStatuses.DeploymentJobFailed;
			var recipient = ZString.Empty;
			if (shelf.RelatedProcessTask?.AssignedStaffMember != null)
			{
				recipient = shelf.RelatedProcessTask.AssignedStaffMember.GS_EmailAddress;
			}
			else if (!string.IsNullOrEmpty(shelf.NotificationEmail))
			{
				recipient = shelf.NotificationEmail;
			}
			else
			{
				var staff = factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_LoginName, UserNameWithoutDomain(shelf.Owner));
				if (staff != null)
				{
					recipient = staff.GS_EmailAddress;
				}
			}
			return (shouldNotify, recipient);
		}

		static string UserNameWithoutDomain(string username)
		{
			return username.Split('\\').Last();
		}

		#endregion

		[SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		void UpdateRelatedProcessTask(EDIShelvesetInfo shelf)
		{
			ZDateTime AddMinutes(ZDateTime date, int minutes) => date.IsValid ? date.AddMinutes(minutes) : TimeSpan.FromMinutes(minutes);
			var task = shelf?.RelatedProcessTask;
			if (task != null)
			{
				var taskNoteMessage = GetNotesTestSummary(shelf);

				if (IsSuccessfulCheckin(shelf))
				{
					task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
					task.P9_ActualDuration = AddMinutes(task.P9_ActualDuration, 10);
					ServiceLogger.Log(LogType.Information, "Marking task as checked in : " + task.P9_TaskID);
				}

				if (IsSuccessfulShelfTest(shelf))
				{
					if (shelf.Status == ShelfStatuses.Passed)
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
						task.P9_ActualDuration = AddMinutes(task.P9_ActualDuration, 10);

						if (IsExperimentalPullRequest())
						{
							task.P9_Outcome = EDITaskOutcomes.Success;
						}

						ServiceLogger.Log(LogType.Information, "Marking task as passed : " + task.P9_TaskID);
					}

					if (shelf.Status == ShelfStatuses.DeploymentJobFailed)
					{
						if (IsExperimentalPullRequest())
						{
							task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
							task.P9_Outcome = EDITaskOutcomes.Failed;
							task.P9_ActualDuration = AddMinutes(task.P9_ActualDuration, 10);
							ServiceLogger.Log(LogType.Information, "Marking task as completed but failed (due to deployment) : " + task.P9_TaskID);
						}
						else
						{
							task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
							ServiceLogger.Log(LogType.Information, "Marking task as suspended : " + task.P9_TaskID);
						}
					}
				}

				task.Logs.AddNew(Events.JobClose, CrikeyWeb.GetTestResultsUrl(shelf.UserHeaderPK).ToString());

				if (shelf.IsRejectedStatus && task.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned)
				{
					if (WorkItemProcessTask.IsCheckinTypeTask(task.P9_Type) && shelf.AspectReviews != null && shelf.AspectReviews.Count > 0 && shelf.Status == ShelfStatuses.RejectedForPendingAspectData)
					{
						foreach (var aspectReview in shelf.AspectReviews.Where(a => !string.IsNullOrEmpty(a.Capability)))
						{
							task.EnsureRequiredAspectReviewExists(aspectReview.Capability, aspectReview.AspectName, aspectReview.GetTaskNotes());
						}
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
						ServiceLogger.Log(LogType.Information, "Marking task as suspended : " + task.P9_TaskID);
						taskNoteMessage.Append($@"Task suspended due to pending aspects. ");
					}
					else if (IsExperimentalPullRequest())
					{
						task.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
						task.P9_Outcome = EDITaskOutcomes.Failed;
						task.P9_ActualDuration = AddMinutes(task.P9_ActualDuration, 10);
						ServiceLogger.Log(LogType.Information, "Marking task as completed but failed : " + task.P9_TaskID);
					}
					else
					{
						if (TryCreateQualityIteration(task))
						{
							ServiceLogger.Log(LogType.Information, "Marking task as cancelled : " + task.P9_TaskID);
						}
						else
						{
							task.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
							ServiceLogger.Log(LogType.Information, "Marking task as suspended : " + task.P9_TaskID);
						}
					}
				}

				taskNoteMessage.Append($"See {CrikeyWeb.GetTestResultsUrl(shelf.UserHeaderPK)} for details.");
				task.AppendTextToTaskNote(taskNoteMessage.ToString());
			}

			bool IsExperimentalPullRequest() => shelf.RelatedProcessTask.P9_Type == EDITaskTypes.TaskActiveExperimentalPullRequest;
		}

		bool TryCreateQualityIteration(WorkItemProcessTask submissionTask)
		{
			var qualityIterationInfo = new QualityIterationInfo(submissionTask, @"Shelf Iteration", "SHV", submissionTask.P9_GS_NKAssignedStaffMember);
			var qualityIterationHelper = new QualityIterationHelper(qualityIterationInfo, submissionTask.Factory, logMessage => ServiceLogger.Log(LogType.Error, logMessage));

			static void AdjustTasksAfterCopyTasksDelegate(IProcessHeader qualityIterationWorkflow)
			{
				foreach (var task in qualityIterationWorkflow.Tasks.Cast<ProcessTask>().ToArray())
				{
					if (!task.IsInDatabase && task.P9_Description == WorkItemProcessTask.AssessReviewDescription && task.P9_Type == WorkItemProcessTask.CodeReviewTaskType)
					{
						task.Delete();
					}
				}
			}

			IEnumerable<QualityIterationTaskDescriptor> CustomQualityIterationTasks()
			{
				yield return QualityIterationHelper.ResolveSubmissionFailureTaskDescriptor(submissionTask.P9_GS_NKAssignedStaffMember);
			}

			var isCheckInSubmission = WorkItemProcessTask.IsCheckinTypeTask(submissionTask.P9_Type);

			return qualityIterationHelper.CreateQualityIteration(
				shouldStartFromCurrentQcbTask: !isCheckInSubmission,
				logger: ServiceLogger,
				adjustTasksAfterCopyTasks: isCheckInSubmission ? AdjustTasksAfterCopyTasksDelegate : null,
				customQualityIterationTasks: isCheckInSubmission ? null : CustomQualityIterationTasks());
		}

		internal ZStringBuilder GetNotesTestSummary(EDIShelvesetInfo shelf)
		{
			var datTimestamp = Invariant($"DAT {Env.Time.FormatDateTime(Env.Time.CurrentLocalDateTime)}: ");
			var taskNoteMessage = new ZStringBuilder(datTimestamp);

			if (shelf.DatGitPullRequests.Count == 0 || (shelf.IsRejectedStatus && !shelf.DatGitPullRequests.Any(pr => pr.Status == ShelfStatuses.Rejected)))
			{
				taskNoteMessage.Append(Invariant($@"Your submission to DAT {TranslateTestResult(shelf.Status, shelf.ActionType)}. "));
			}

			foreach (var gitPull in shelf.DatGitPullRequests)
			{
				taskNoteMessage.Append(Invariant($@"{GetPrefixFromActionType(shelf.ActionType)}""{gitPull.Title}"" {TranslateTestResult(gitPull.Status, shelf.ActionType)}. "));
			}

			if (shelf.Status == ShelfStatuses.DeploymentJobFailed)
			{
				taskNoteMessage.Append("A deployment job has failed. ");
			}

			return taskNoteMessage;
		}

		bool IsSuccessfulCheckin(EDIShelvesetInfo shelf)
		{
			return shelf.Status == ShelfStatuses.CheckedIn || (shelf.Status == ShelfStatuses.DeploymentJobFailed && shelf.ActionType == EDIShelvesetInfo.ActionTypes.ShelfCheckin);
		}

		bool IsSuccessfulShelfTest(EDIShelvesetInfo shelf)
		{
			return shelf.Status == ShelfStatuses.Passed || (shelf.Status == ShelfStatuses.DeploymentJobFailed && shelf.ActionType != EDIShelvesetInfo.ActionTypes.ShelfCheckin);
		}

		LatestReleaseBuildsDictionary builds;
		protected LatestReleaseBuildsDictionary Builds
		{
			get
			{
				if (builds == null)
				{
					builds = new LatestReleaseBuildsDictionary(new BusinessObjectFactory { NameForDebugging = $"{nameof(ProcessedShelfsServiceTask)}.{nameof(Builds)}" });
					builds.Load();
				}
				return builds;
			}
		}

		#region ASSESS

		IAssessServiceClient AssessServiceClient => assessServiceClient ??= ObjectFactory.Get<IAssessServiceClient>();
		IAssessServiceClient assessServiceClient;

		#endregion

		#region Dictionaries

		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		readonly static Lazy<Dictionary<string, string>> LazyStatusToResultDictionary = new Lazy<Dictionary<string, string>>(() =>
		new Dictionary<string, string>
		{
			{ ShelfStatuses.Rejected, "Failure" },
			{ ShelfStatuses.RejectedForPendingAspectData, "Failure" },
			{ ShelfStatuses.DeploymentJobFailed, "Failure" },
			{ ShelfStatuses.CheckedIn, "Success" },
			{ ShelfStatuses.Passed, "Success" },
		});

		internal static Dictionary<string, string> StatusToResultDictionary { get { return LazyStatusToResultDictionary.Value; } }

		#endregion

		protected virtual IDatSubmissionsProvider CreateDatSubmissionsProvider() => new DatSubmissionsProvider();
	}
}
