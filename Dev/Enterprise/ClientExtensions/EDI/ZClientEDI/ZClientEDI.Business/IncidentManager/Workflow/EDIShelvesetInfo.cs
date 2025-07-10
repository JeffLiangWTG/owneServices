using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.ZArchitecture.Core;
using WTG.DevTools.SourceControl;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIShelvesetInfo : ITaskCompetencyRequirementsProvider
	{
		public EDIShelvesetInfo(string owner, string name, string actionType, WorkItemProcessTask processTask)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			Owner = owner;
			Name = name ?? string.Empty;
			ActionType = actionType;
			RelatedProcessTask = processTask;
			AspectReviews = new ();
			DatGitPullRequests = new ();
		}

		public string DisplayName
		{
			get
			{
				var result = Name;
				if (string.IsNullOrEmpty(result))
				{
					result = DatGitPullRequests.FirstOrDefault()?.Title;
				}
				if (string.IsNullOrEmpty(result))
				{
					result =  "Your submission to DAT";
				}
				return result;
			}
		}

		public string Name { get; private set; }
		public string Owner { get; private set; }
		public string ActionType { get; private set; }
		public WorkItemProcessTask RelatedProcessTask { get; set; }

		WorkItemProcessTask nextCodeReviewTask;
		bool triedFindingCodeReview;
		public WorkItemProcessTask NextCodeReviewTask
		{
			get
			{
				if (nextCodeReviewTask == null && !triedFindingCodeReview)
				{
					nextCodeReviewTask = FindNextTask(WorkItemProcessTask.CodeReviewTaskType);
					triedFindingCodeReview = true;
				}

				return nextCodeReviewTask;
			}
		}

		public Guid UserHeaderPK { get; set; }
		public string PrimaryBranch { get; set; }
		public string Status { get; set; }
		public string NotificationEmail { get; set; }
		public List<AspectReviewSummary> AspectReviews { get; private set; }
		public List<DatGitPullRequest> DatGitPullRequests { get; private set; }

		public bool IsTestRun => ActionType == ActionTypes.ShelfsetTest;
		public bool IsCheckin => ActionType == ActionTypes.ShelfCheckin;

		public bool IsRejectedStatus => Status == ShelfStatuses.Rejected || Status == ShelfStatuses.RejectedForPendingAspectData;

		public void Schedule(ICrikeyDataAccess crikeyDataAccess)
		{
			if (RelatedProcessTask != null)
			{
				if (crikeyDataAccess.IsShelfScheduled(this))
				{
					crikeyDataAccess.RemoveFromSchedule(this);
				}

				crikeyDataAccess.AddToSchedule(this);
			}
		}

		public bool HasBeenBuilt => !string.IsNullOrEmpty(Status) && Status != ShelfStatuses.Queued && Status != ShelfStatuses.QueuedForBranchDetection;

		public string GetNotifiedEquivalentOfCurrentStatus()
		{
			return Status switch
			{
				ShelfStatuses.Rejected => ShelfStatuses.RejectedAndNotified,
				ShelfStatuses.RejectedForPendingAspectData => ShelfStatuses.RejectedForPendingAspectDataAndNotified,
				ShelfStatuses.DeploymentJobFailed => ShelfStatuses.DeploymentJobFailedAndNotified,
				ShelfStatuses.CheckedIn => ShelfStatuses.CheckedInAndNotified,
				ShelfStatuses.Passed => ShelfStatuses.PassedAndNotified,
				_ => null
			};
		}

		public string Comments
		{
			get
			{
				if (RelatedProcessTask == null)
				{
					return string.Empty;
				}

				var replacedText = taskNotesPullRequestUrlRegex.Replace(RelatedProcessTask.P9_NotesAsString, m => m.Value.Replace(m.Groups["fldrslt"].Value, @"\fldrslt{" + m.Groups["url"].Value + "}"));
				var notes = ORtfTextUtil.RtfToText(replacedText);

				var workItem = RelatedProcessTask.Parent;
				if (workItem != null)
				{
					notes = $"TestRigOrigin: {workItem.WKI_WorkItemNumber}\r\n{notes}";
				}

				var options = GetRegistryOptions();

				return options == null ? notes : GetOptionsFromRegistryAndNotes(options, notes);
			}
		}

		static readonly Regex taskNotesPullRequestUrlRegex = new Regex(@"\\field{\\\*\\fldinst{HYPERLINK ""?(?<url>http(s)?\://.+?)""?( .*?)?}}{(?<fldrslt>\\fldrslt{.*?})}}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		TestRigRegistryOptions GetRegistryOptions()
		{
			var workItem = RelatedProcessTask.Parent;

			if (workItem == null)
			{
				return null;
			}

			var workItemType = workItem.WKI_WorkItemType;
			var workItemArea = workItem.WKI_WorkItemArea;
			var activityType = workItem.WKI_ActivityType;
			var activitySubtype = workItem.WKI_ActivitySubtype;
			var options = TestRigRegistryHelper.GetTestRigOptions();

			return TestRigRegistryHelper.GetOptionsForWorkItemCombination(workItemType, workItemArea, activityType, activitySubtype, options) ??
				   TestRigRegistryHelper.GetOptionsForWorkItemCombination(workItemType, workItemArea, activityType, string.Empty, options) ??
				   TestRigRegistryHelper.GetOptionsForWorkItemCombination(workItemType, workItemArea, string.Empty, string.Empty, options) ??
				   TestRigRegistryHelper.GetOptionsForWorkItemCombination(workItemType, string.Empty, string.Empty, string.Empty, options);
		}

		static string GetOptionsFromRegistryAndNotes(TestRigRegistryOptions options, string notes)
		{
			const string backupFileKey = "TestRigRestoreFromBackup:";
			const string keyPrefix = "TestRig";
			var result = new StringBuilder(notes);

			if (!notes.Contains(backupFileKey))
			{
				result.AppendLine();
				result.Append(string.Format(CultureInfo.InvariantCulture, "{0} <{1}>", backupFileKey, options.BackupFile));
			}

			var regex = new Regex(string.Format(CultureInfo.InvariantCulture, @"^\s*{0}(?<value>.*):", keyPrefix), RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
			var matches = regex.Matches(notes).Cast<Match>();
			var keys = matches.Where(x => x.Success).Select(x => keyPrefix + x.Groups["value"].Value).ToArray();

			using (var reader = new StringReader(options.AdditionalOptions))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					if (!string.IsNullOrWhiteSpace(line) && !keys.Any(x => line.Contains(x)))
					{
						result.AppendLine();
						result.Append(line);
					}
				}
			}

			return result.ToString();
		}

		WorkItemProcessTask FindNextTask(string taskType)
		{
			if (RelatedProcessTask == null)
			{
				return null;
			}

			WorkItemProcessTask nextTask = null;
			var alltasksInOrder = RelatedProcessTask.GetAllTasksInWorkflowInOrder().Where(t => t is WorkItemProcessTask).ToArray();
			var tasksAfterThis = alltasksInOrder
				.SkipWhile(t => !(t.P9_Sequence == RelatedProcessTask.P9_Sequence
				&& t.P9_FH_ProcessHeader == RelatedProcessTask.P9_FH_ProcessHeader)).Where(t => !t.IsClosed).ToArray();

			foreach (WorkItemProcessTask t in tasksAfterThis)
			{
				if (t.P9_Type.EqualsIgnoringCase(taskType))
				{
					nextTask = t;
					break;
				}
			}

			return nextTask;
		}

		IEnumerable<CompetencyRequirement> ITaskCompetencyRequirementsProvider.GetPendingCompetencyRequirements()
		{
			return AspectReviews.Where(r => r.IsAssess).Select(r => new CompetencyRequirement(r.AspectPK)).Distinct();
		}

		public static class ActionTypes
		{
			public const string ShelfCheckin = "SCH";
			public const string ShelfsetTest = "SHV";
			public const string UATBuild = "UAB";
			public const string AspectOnlyBuild = "ASB";
			public const string UATCombinedBuild = "UCB";
			public const string UATCombinedChild = "UCC";
			public const string ExperimentalTest = "JPR";
		}
	}
}
