using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeader : ITagable
	{
		ZGuid FH_FH_ParentHeader { get; set; }
		ZGuid FH_FC_CurrentComponent { get; set; }
		ZGuid FH_FC_DedicatedBuffer { get; set; }

		[List("Lookups.AllReleaseGroups")]
		ZGuid FH_GG_ReleaseGroup { get; set; }
		ZGuid FH_P0_Template { get; set; }
		ZGuid FH_ParentTemplateId { get; set; }

		ZDateTime FH_ReleaseDateTime { get; set; }
		ZDateTime FH_AgreedDeliveryDate { get; set; }
		ZDateTime FH_DoNotStartBeforeDate { get; set; }
		ZDateTime PlannedDuration { get; }

		ZString FH_CompletionStatement { get; set; }
		[List("Lookups.DateAcceptabilityList")]
		ZString FH_DateAcceptability { get; set; }

		[List("Lookups.DeadlineTypeList")]
		ZString FH_DeadlineType { get; set; }

		ZString Code { get; }
		new ZString Description { get; }
		ZString FH_Status { get; }

		ZDecimal FH_TimeDelayFactor { get; set; }
		ZInt FH_TimeDelayMinutes { get; set; }
		ZDateTime StaggeredReleaseDelayExpiryLocal { get; }
		ZDateTime FH_StaggeredReleaseDelayExpiry { get; }
		ZShort FH_VoteUpDownAmount { get; set; }

		ZBool FH_IsActive { get; set; }
		ZBool FH_IsCriticalHandover { get; set; }
		ZBool ApplicableIsCriticalHandover { get; }
		ZBool FH_AllowTaskAutoAssignment { get; set; }
		ZBool FH_IsStandby { get; set; }
		ZBool FH_IsApproved { get; set; }
		ZGuid FH_GB_Branch { get; set; }
		ZGuid FH_GE_Department { get; set; }
		ZDecimal FH_EffectiveNudge { get; set; }
		ZGuid FH_BMT_BufferTimespan { get; set; }
		TimeSpan EffectiveBufferDuration {  get; }
		ZInt EffectiveBufferDurationMinutes { get; }
		ZString EffectiveBufferDurationString { get; }
		ZBool IsOpen { get; }
		ZBool SynchroniseBufferPenetration { get; set; }

		ZGuid FH_ParentId { get; set; }
		ZString FH_ParentTableCode { get; set; }

		ZString FH_WorkflowType { get; set; }

		ZString FH_LastTransferType { get; }
		ZString FH_MilestoneCompletionPivotKey { get; set; }

		ZString LastTransferTypeDescription { get; }

		ZDateTime FH_SystemCreateTimeUtc { get; set; }
		ZString FH_SystemCreateUser { get; set; }
		ZDateTime FH_SystemLastEditTimeUtc { get; set; }
		ZString FH_SystemLastEditUser { get; set; }

		ZString IterationReason { get; set; }
		ZString ProcessHeaderType { get; }

		ZString FH_AgreedDeliveryDateDefaultsFrom { get; set; }
		ZDateTime FH_AgreedDeliveryDateDefaultHoursOffset { get; set; }
		ZString FH_EarliestStartDateDefaultsFrom { get; set; }
		ZDateTime FH_EarliestStartDefaultHoursOffset { get; set; }

		ZDateTime FH_TaskPenetrationResetDateTimeUtc { get; }

		ZString FH_Category { get; set; }
		ZString CategoryDescription { get; }

		ZString ReleaseSequenceName { get; }
		ZInt ReleaseSequencePosition { get; }
		ZInt ReleaseSequenceInvestment { get; }
		ZInt ReleaseSequenceValue { get; }
		ZGuid ReleaseSequencePK { get; }
		ZString ReleaseSequenceWorkflow { get; }
		ZString ReleaseSequenceParentJob { get; }
		IBMReleaseSequence HighestReleaseSequence { get; }

		IProcessJobHeader JobHeader { get; }
		IProcessHeaderValidation Validation { get; }
		IProcessHeaderLookups Lookups { get; }
		IEnumerable<IProcessTask> Tasks { get; }
		IBMComponent CurrentComponent { get; }
		IBMSystem BMSystem { get; }
		new IWorkflowProviderCore Parent { get; }
		IGlbGroup ReleaseGroup { get; }

		IEnumerable<IProcessHeaderLink> Links { get; }
		IProcessHeaderLinkCollection LinksFromMeToOthers { get; }
		IProcessHeaderLinkCollection LinksFromOthersToMe { get; }
		IEnumerable<IProcessHeaderLink> PrerequisiteLinks { get; }

		IEnumerable<IProcessHeaderLink> ParentLinks { get; }

		IEnumerable<IProcessHeader> GetPrerequisitesUpTheTree();
		IEnumerable<IProcessHeader> GetParentWorkflowsUpTheHierarchy();
		IEnumerable<IProcessHeader> GetChildWorkflowsDownTheHierarchy();
		IEnumerable<IProcessHeader> GetAncestors();

		IProcessHeaderLink GetOrCreateLinkToParent(IProcessHeader parent);
		IProcessHeaderLink GetOrCreateDependencyLink(IProcessHeader postrequisite);

		bool IsParentOf(IProcessHeader other);
		bool IsWorkflow { get; }
		IProcessHeader ParentHeader { get; }

		ITagLinkCollection TagLinks_ForBinding { get; }

		ZString Sequence { get; }
		bool IsDeleted { get; }
		bool IsReleased { get; }
		bool IsCurrent(IProcessTask task);

		IDisposable SuspendSettingHasChanges();
		void UpdateLastEditTimeIfNotDeleting();
		void NudgeUp();
		void NudgeDown();
		IProcessHeader Clone();

		void SetPenetrationResetDate();

		void UpdateEffectiveNudge();

		void UpdateEffectiveBranch();
		void UpdateEffectiveDepartment();

		IBranchDepartmentProvider OverrideContextProvider { get; set; }
		TimeSpan GetWorkingTimeSinceTaskBecameStartable(ZDateTime timeTaskBecameStartable);

		IBindingListView TaskCollectionIncludingChildWorkflowTasksBindingListView { get; }

		void UpdateJobProperties();

		#region Fetch Hints

		void AddDeepFetchHintForParentType(BusinessObjectFactory specificFactory, Type parentType);

		void AddTagLinksFetchHint();

		#endregion

		#region For customised controls

		ZDateTime AgreedDeliveryDateLocal { get; set; }
		ZString CompletionCriteria { get; }
		ZString CurrentStatusOnSingleLine { get; }
		ZString CurrentTaskResourceCode { get; }
		ZString CurrentTaskResourceName { get; }
		ZDateTime DoNotStartBeforeDateLocal { get; set; }
		ZBool HasOpenPrerequisites { get; }
		ZString PrerequisiteStatusDescription { get; }
		ZString PrerequisiteStatusShortDescription { get; }
		ZDateTime LastTransferDateLocal { get; }
		ZDecimal OverallEstimateFactor { get; }
		ZString ParentJobDescription { get; }
		ZString ProviderJobDescription { get; }
		ZString ProviderJobNumber { get; }

		#region Total Actual Hours

		ZString TotalActualHoursSummary { get; }
		ZString TotalActualHoursLabel { get; }
		ZDecimal TotalActualHours { get; }
		ZString TotalActualHoursIncludingChildrenSummary { get; }
		ZString TotalActualHoursIncludingChildrenLabel { get; }
		ZDecimal TotalActualHoursIncludingChildren { get; }

		#endregion

		#region Total Estimated Hours

		ZString TotalEstimatedHoursSummary { get; }

		ZDecimal TotalLowEstimate { get; }
		ZDecimal TotalHighEstimate { get; }
		ZDecimal TotalStandardEstimate { get; }

		ZDecimal PreviousTotalLowEstimate { get; set; }
		ZDecimal PreviousTotalHighEstimate { get; set; }

		#endregion

		#region Total Relevant Estimated Hours

		ZString TotalRelevantEstimatedHoursSummary { get; }
		ZString TotalRelevantEstimatedHoursLabel { get; }
		ZDecimal TotalRelevantEstimatedHours { get; }
		ZString TotalRelevantEstimatedHoursIncludingChildrenSummary { get; }
		ZString TotalRelevantEstimatedHoursIncludingChildrenLabel { get; }
		ZDecimal TotalRelevantEstimatedHoursIncludingChildren { get; }

		#endregion

		#region Remaining Estimate Hours

		ZString RemainingEstimateHoursSummary { get; }
		ZString RemainingEstimateHoursLabel { get; }
		ZDecimal RemainingEstimateHours { get; }
		ZString RemainingEstimateHoursIncludingChildrenSummary { get; }
		ZString RemainingEstimateHoursIncludingChildrenLabel { get; }
		ZDecimal RemainingEstimateHoursIncludingChildren { get; }

		#endregion

		ZDecimal ImplicitDurationHours { get; }
		ZString CurrentTasksStatus { get; }
		ZString FH_StatusDescription { get; }
		ZDecimal EffectiveNudge { get; }
		ZInt NumberOfOpenPrerequisitesUpTheTree { get; }

		ZDateTime ApplicableAgreedDeliveryDateUtc { get; }

		ZDateTime ReleaseSequenceSortDate { get; }

		ZString ReleaseSequence { get; }

		#region Approved Schedule

		ZDateTime ApprovedShapeScheduledStartTimeLocal { get; }
		ZDateTime ApprovedShapeScheduledFinishTimeLocal { get; }

		ZDecimal ApprovedShapeExplicitDurationHours { get; }

		ZBool ApprovedShapeIsCriticalPath { get; }
		ZString ApprovedShapeRootDiagramName { get; }

		#endregion

		#region Required ZPropertyInfos

		ZPropertyInfo FH_FH_ParentHeaderInfo { get; }
		ZPropertyInfo FH_FC_CurrentComponentInfo { get; }
		ZPropertyInfo FH_FC_DedicatedBufferInfo { get; }
		ZPropertyInfo FH_GG_ReleaseGroupInfo { get; }
		ZPropertyInfo FH_P0_TemplateInfo { get; }
		ZPropertyInfo FH_ParentTemplateIdInfo { get; }
		ZPropertyInfo FH_ReleaseDateTimeInfo { get; }
		ZPropertyInfo FH_AgreedDeliveryDateInfo { get; }
		ZPropertyInfo FH_DoNotStartBeforeDateInfo { get; }
		ZPropertyInfo FH_CompletionStatementInfo { get; }
		ZPropertyInfo FH_DateAcceptabilityInfo { get; }
		ZPropertyInfo FH_DeadlineTypeInfo { get; }
		ZPropertyInfo FH_TimeDelayFactorInfo { get; }
		ZPropertyInfo FH_TimeDelayMinutesInfo { get; }
		ZPropertyInfo FH_VoteUpDownAmountInfo { get; }
		ZPropertyInfo FH_IsActiveInfo { get; }
		ZPropertyInfo FH_IsCriticalHandoverInfo { get; }
		ZPropertyInfo FH_AllowTaskAutoAssignmentInfo { get; }
		ZPropertyInfo FH_IsStandbyInfo { get; }
		ZPropertyInfo FH_IsApprovedInfo { get; }
		ZPropertyInfo FH_GB_BranchInfo { get; }
		ZPropertyInfo FH_GE_DepartmentInfo { get; }
		ZPropertyInfo FH_EffectiveNudgeInfo { get; }
		ZPropertyInfo FH_BMT_BufferTimespanInfo { get; }
		ZPropertyInfo SynchroniseBufferPenetrationInfo { get; }
		ZPropertyInfo FH_MilestoneCompletionPivotKeyInfo { get; }
		ZPropertyInfo FH_ParentIdInfo { get; }
		ZPropertyInfo FH_ParentTableCodeInfo { get; }
		ZPropertyInfo FH_WorkflowTypeInfo { get; }
		ZPropertyInfo FH_SystemCreateTimeUtcInfo { get; }
		ZPropertyInfo FH_SystemCreateUserInfo { get; }
		ZPropertyInfo FH_SystemLastEditTimeUtcInfo { get; }
		ZPropertyInfo FH_SystemLastEditUserInfo { get; }
		ZPropertyInfo IterationReasonInfo { get; }
		ZPropertyInfo AgreedDeliveryDateLocalInfo { get; }
		ZPropertyInfo DoNotStartBeforeDateLocalInfo { get; }

		ZPropertyInfo PreviousTotalLowEstimateInfo { get; }
		ZPropertyInfo PreviousTotalHighEstimateInfo { get; }

		ZPropertyInfo FH_AgreedDeliveryDateDefaultsFromInfo { get; }
		ZPropertyInfo FH_AgreedDeliveryDateDefaultHoursOffsetInfo { get; }
		ZPropertyInfo FH_EarliestStartDateDefaultsFromInfo { get; }
		ZPropertyInfo FH_EarliestStartDefaultHoursOffsetInfo { get; }

		ZPropertyInfo FH_CategoryInfo { get; }

		#endregion

		#endregion
	}
}
