using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class QualityIterationHelper
	{
		public QualityIterationHelper(QualityIterationInfo qualityIterationInfo, BusinessObjectFactory factory, Action<string> logAction)
		{
			qcbTask = qualityIterationInfo?.QcbTask;
			qualityIterationType = qualityIterationInfo?.QualityIterationType;
			QualityIterationReasonCode = qualityIterationInfo?.QualityIterationReasonCode;
			resourceUnderReviewNk = qualityIterationInfo?.ResourceUnderReviewNk;
			this.factory = factory;
			this.logAction = logAction;
		}

		#region Fields

		public IEnumerable<string> EligibleIterateFromTaskTypes => eligibleIterateFromTaskTypes ?? (eligibleIterateFromTaskTypes = GetEligibleIterateFromTaskTypesFromRegistry());
		IEnumerable<string> eligibleIterateFromTaskTypes;

		public static IEnumerable<string> GetEligibleIterateFromTaskTypesFromRegistry()
		{
			return EDIDataRegistry.Instance.CodingTasks.Value.GetAllCodes();
		}

		public const string FailedQualityIterationRegistryCategoryCode = "WKI";
		public const string FailedQualityIterationUserCode = "DAT";

		readonly WorkItemProcessTask qcbTask;
		readonly BusinessObjectFactory factory;
		readonly Action<string> logAction;
		readonly string qualityIterationType;
		readonly string resourceUnderReviewNk;

		#endregion

		public string QualityIterationReasonCode { get; }

		#region Implementation

		bool ShouldCreateQualityIteration(ILogger logger)
		{
			if (!qcbTask.IsQualityContainmentBarrierTask())
			{
				logger?.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Task Type [{0}] is not configured as a Containment Barrier in the Task Types registry item.", qcbTask.P9_Type));
				return false;
			}

			var workflow = qcbTask.ProcessHeader;
			var releaseGroupCode = workflow?.ReleaseGroup?.GG_Code ?? ZString.Empty;

			var registryItemValue = EDIDataRegistry.Instance.QualityIterationAssignments.Value;
			var isEnabledInRegistry = registryItemValue.IsQiEnabledForReleaseGroupOrNotSpecifiedButDefaultEnabled(releaseGroupCode);

			if (!isEnabledInRegistry)
			{
				var isReleaseGroupSpecified = registryItemValue.IsReleaseGroupSpecified(releaseGroupCode);
				var isReleaseGroupEnabled = isReleaseGroupSpecified && registryItemValue.IsQiEnabledForReleaseGroup(releaseGroupCode);
				var isDefaultOptionSelected = registryItemValue.IsDefaultOptionSelected;

				logger?.Log(LogType.Warning, string.Format(CultureInfo.InvariantCulture, "Quality iterations for release group {0} are not enabled in the registry. Release Group Specified: {1}, Release Group Enabled: {2}, Default Option Selected: {3}", releaseGroupCode, isReleaseGroupSpecified, isReleaseGroupEnabled, isDefaultOptionSelected));
			}

			return isEnabledInRegistry;
		}

		public bool CreateQualityIteration(bool shouldStartFromCurrentQcbTask, Action<IProcessHeader> adjustTasksAfterCopyTasks = null, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks = null, ILogger logger = null)
		{
			if (!ShouldCreateQualityIteration(logger))
			{
				return false;
			}

			var creator = ObjectFactory.Get<IContainmentBarrierCreator>(nameof(IContainmentBarrierCreator), new[] { logger });
			var checkInTasks = ReleaseRingsLookup.CheckInTaskTypes.ToArray();
			var doNotRepeatList = checkInTasks
				.Append(EDIDataRegistry.Instance.CompetencyLearningTask.Value)
				.Append(WorkItemProcessTask.AspectReviewTaskType);
			creator.TaskTypesToNotRepeat = doNotRepeatList;
			creator.TaskTypesToSuspendOnRepeatIfQcbTask = checkInTasks;
			creator.IterationType = qualityIterationType;
			creator.QcbTaskNewStatus = ProcessTaskStatusCodeList.Codes.Cancelled;
			creator.QcbCreatingUserLoginName = GetQualityIterationLoginName();
			creator.ResourceUnderReviewNk = resourceUnderReviewNk;

			var iterateFromTaskPk = shouldStartFromCurrentQcbTask ? qcbTask.PK : creator.FindBestIterateFromTask(qcbTask.PK, ContainmentBarrierIterateFromTaskSelectionMode.ExcludeDifferentResourceAsQcbTask | ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks, EligibleIterateFromTaskTypes);
			var reasonPk = GetReasonPkForQualityIteration();

			if (iterateFromTaskPk == ZGuid.Empty || reasonPk == ZGuid.Empty || string.IsNullOrEmpty(creator.QcbCreatingUserLoginName))
			{
				if (iterateFromTaskPk == ZGuid.Empty)
				{
					logger?.Log(LogType.Warning, "Could not find a task to iterate to when creating a quality iteration for " + qcbTask.P9_TaskID);
				}

				return false;
			}

			creator.CreateQualityIteration(qcbTask.PK, iterateFromTaskPk, reasonPk, adjustTasksAfterCopyTasks, customQualityIterationTasks);
			return true;
		}

		public static QualityIterationTaskDescriptor ResolveSubmissionFailureTaskDescriptor(ZString assignedStaffCode)
		{
			const int LowEstimatedDurationForResolveSubmissionFailureTasksInMinutes = 20;

			return new QualityIterationTaskDescriptor
			{
				Type = "COD",
				Description = "Resolve submission failure",
				AssignedStaffCode = assignedStaffCode,
				EstDuration = TimeSpan.FromMinutes(LowEstimatedDurationForResolveSubmissionFailureTasksInMinutes),
			};
		}

		public ZGuid GetReasonPkForQualityIteration()
		{
			var reasons = WorkflowDataRegistry.Instance.IterationReasons.Value;
			var reason = reasons.GetIterationReason(FailedQualityIterationRegistryCategoryCode, QualityIterationReasonCode);

			if (reason == null)
			{
				logAction?.Invoke(string.Format(CultureInfo.InvariantCulture, "Please add a {0} Quality Iteration Reasons registry item for workflow type {1}.", QualityIterationReasonCode, FailedQualityIterationRegistryCategoryCode));
				return ZGuid.Empty;
			}

			return reason.PK;
		}

		public string GetQualityIterationLoginName()
		{
			var query = new ZQuery(GlbStaffSchema.GS_Code, SQLComparisonOperator.Equal, FailedQualityIterationUserCode);
			var user = factory.LoadTop1<GlbStaff>(query);

			if (user == null)
			{
				logAction?.Invoke(string.Format(CultureInfo.InvariantCulture, "Could not find a user with GS_Code value of {0}, which is required for quality iterations to be created for failed shelves.", FailedQualityIterationUserCode));
				return null;
			}

			return user.GS_LoginName.ToString();
		}

		#endregion
	}
}
