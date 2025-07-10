using System;
using System.Collections.Generic;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMSRegistry
	{
		string WorkflowManagementMode
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool BufferManagementEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool IsEnhancedWorkflowManagementOrBetterEnabled
		{
			get;
		}

		bool IsBufferManagementWorkflowModeOrBetterEnabled
		{
			get;
		}

		bool IsPlanningManagementEnabled
		{
			get;
		}

		bool RequireResourceToCloseTask
		{
			get;
#if DEBUG
			set;
#endif
		}

		int MaxNumberOfItemsAllowedToCacheBoardInSlideShow
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool NCNRibbonEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool PAVEOnTheWeb
		{
			get;
#if DEBUG
			set;
#endif
		}

		int SectionProgressStatisticsHistoryRange
		{
			get;
#if DEBUG
			set;
#endif
		}

		int MaxNumberOfBoardsAllowedToCacheInSlideShow
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool BoardOnSecondaryServer
		{
			get;
		}

#if DEBUG
		bool DisallowDBHitsOnBoardGUIThread
		{
			get;
			set;
		}

		ICollection<string> AllowedTablesDBHitsOnBoardGUIThread
		{
			get;
			set;
		}
#endif

		bool DisplayResponsiveReleaseGateUiSettings
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool ReleaseSequencesModuleEnabled
		{
			get;
#if DEBUG
			set;
#endif
		}

		Guid DefectTagPK
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool AlwaysViewWorkflowManagementTab
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool EnableMENTSections
		{
			get;
#if DEBUG
			set;
#endif
		}

		int TimeBeforeDeletingOldScheduledTasks
		{
			get;
#if DEBUG
			set;
#endif
		}

		int TaskActionSchedulerBatchSize
		{
			get;
#if DEBUG
			set;
#endif
		}

		#region Responsive PAVE Data Processing

		bool EnableResponsivePAVEDataProcessing
		{
			get;
#if DEBUG
			set;
#endif
		}

		#region Transfer

		bool TransferWorkflowComponentOnChanges
		{
			get;
#if DEBUG
			set;
#endif
		}

		int TransferWorkflowComponentMaximumNumberOfCDCChanges
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool DynamicallyFilterTransferRules
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool ProcessAllTransferRulesLinksOnNextBMSRun
		{
			get;
			set;
		}

		bool EnableNudgingBMSServiceTaskByThePVEServiceTask
		{
			get;
#if DEBUG
			set;
#endif
		}

		int DelayForNudgingTheBMSServiceTaskByThePVEServiceTask
		{
			get;
#if DEBUG
			set;
#endif
		}

		#endregion

		#region Auto Assignment

		bool AutoAssignCapabilityTasksOnChanges
		{
			get;
#if DEBUG
			set;
#endif
		}

		int AutoAssignCapabilityTasksMaxNumberOfCDCChanges
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool EnableNudgingBMTServiceTaskByThePVEServiceTask
		{
			get;
#if DEBUG
			set;
#endif
		}

		int DelayForNudgingTheBMTServiceTaskByThePVEServiceTask
		{
			get;
#if DEBUG
			set;
#endif
		}

		#endregion

		#region Responsive Workflow Updates On BMS Changes

		bool EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get;
#if DEBUG
			set;
#endif
		}

		int ResponsiveWorkflowUpdatesBatchSize
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get;
#if DEBUG
			set;
#endif
		}

		#endregion

		#endregion

		#region EXPERIMENTAL

		bool EnablePaveExperimentalFeatures
		{
			get;
#if DEBUG
			set;
#endif
		}

		bool IgnoreIterationsWhenCalculatingStartability
		{
			get;
#if DEBUG
			set;
#endif
		}

		#endregion
	}
}
