using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	class BMSRegistryProvider : IBMSRegistry
	{
		string IBMSRegistry.WorkflowManagementMode
		{
			get => BMSRegistry.Instance.WorkflowManagementMode.Value;
#if DEBUG
			set
			{
				BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
#endif
		}

		bool IBMSRegistry.BufferManagementEnabled
		{
			get { return IsBufferManagementEnabled; }
#if DEBUG
			set
			{
				var mode = value
					? WorkflowManagementModes.Codes.PlanningManagement
					: WorkflowManagementModes.Codes.BasicWorkflow;
				BMSRegistry.Instance.WorkflowManagementMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mode);
			}
#endif
		}

		public static bool IsBufferManagementEnabled
			=> BMSRegistry.Instance.WorkflowManagementMode.Value != WorkflowManagementModes.Codes.BasicWorkflow;

		bool IBMSRegistry.IsEnhancedWorkflowManagementOrBetterEnabled
			=> IsEnhancedWorkflowManagementOrBetterEnabled;

		public static bool IsEnhancedWorkflowManagementOrBetterEnabled
		{
			get
			{
				return IsWorkflowModeSelected(
					WorkflowManagementModes.Codes.EnhancedWorkflow,
					WorkflowManagementModes.Codes.IncludesBufferManagement,
					WorkflowManagementModes.Codes.PlanningManagement);
			}
		}

		bool IBMSRegistry.IsBufferManagementWorkflowModeOrBetterEnabled
			=> IsBufferManagementWorkflowModeOrBetterEnabled;

		public static bool IsBufferManagementWorkflowModeOrBetterEnabled
			=> IsWorkflowModeSelected(WorkflowManagementModes.Codes.IncludesBufferManagement, WorkflowManagementModes.Codes.PlanningManagement);

		bool IBMSRegistry.IsPlanningManagementEnabled
			=> IsPlanningManagementEnabled;

		public static bool IsPlanningManagementEnabled
			=> IsWorkflowModeSelected(WorkflowManagementModes.Codes.PlanningManagement);

		static bool IsWorkflowModeSelected(params string[] matchingModes)
		{
			var mode = BMSRegistry.Instance.WorkflowManagementMode.Value;

			return matchingModes.Contains(mode, StringComparer.InvariantCultureIgnoreCase);
		}

		public bool RequireResourceToCloseTask
		{
			get { return BMSRegistry.Instance.RequireResourceToCloseTask.Value; }
#if DEBUG
			set { BMSRegistry.Instance.RequireResourceToCloseTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int MaxNumberOfItemsAllowedToCacheBoardInSlideShow
		{
			get { return BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.Value; }
#if DEBUG
			set { BMSRegistry.Instance.MaxNumberOfItemsAllowedToCacheBoardInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		bool IBMSRegistry.NCNRibbonEnabled
		{
			get { return BMSRegistry.Instance.NCNRibbonEnabled.Value; }
#if DEBUG
			set { BMSRegistry.Instance.NCNRibbonEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		bool IBMSRegistry.PAVEOnTheWeb
		{
			get { return BMSRegistry.Instance.PAVEOnTheWeb.Value; }
#if DEBUG
			set { BMSRegistry.Instance.PAVEOnTheWeb.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int SectionProgressStatisticsHistoryRange
		{
			get { return BMSRegistry.Instance.SectionProgressStatisticsHistoryRange.Value; }
#if DEBUG
			set { BMSRegistry.Instance.SectionProgressStatisticsHistoryRange.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int MaxNumberOfBoardsAllowedToCacheInSlideShow
		{
			get { return BMSRegistry.Instance.MaxNumberOfBoardsAllowedToCacheInSlideShow.Value; }
#if DEBUG
			set { BMSRegistry.Instance.MaxNumberOfBoardsAllowedToCacheInSlideShow.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool BoardOnSecondaryServer
		{
			get { return BMSRegistry.Instance.BoardOnSecondaryServer.Value; }
#if DEBUG
			set { BMSRegistry.Instance.BoardOnSecondaryServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

#if DEBUG
		bool IBMSRegistry.DisallowDBHitsOnBoardGUIThread
		{
			get { return BMSRegistry.Instance.DisallowDBHitsOnBoardGUIThread.Value; }
			set { BMSRegistry.Instance.DisallowDBHitsOnBoardGUIThread.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		ICollection<string> IBMSRegistry.AllowedTablesDBHitsOnBoardGUIThread
		{
			get { return BMSRegistry.Instance.AllowedTablesDBHitsOnBoardGUIThread.Value; }
			set { BMSRegistry.Instance.AllowedTablesDBHitsOnBoardGUIThread.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value.ToArray()); }
		}
#endif
		bool IBMSRegistry.DisplayResponsiveReleaseGateUiSettings
		{
			get { return BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.Value; }
#if DEBUG
			set { BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		bool IBMSRegistry.ReleaseSequencesModuleEnabled
		{
			get { return BMSRegistry.Instance.ReleaseSequencesModuleEnabled.Value; }
#if DEBUG
			set { BMSRegistry.Instance.ReleaseSequencesModuleEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		Guid IBMSRegistry.DefectTagPK
		{
			get => BMSRegistry.Instance.DefectTagPK.Value;
#if DEBUG
			set => BMSRegistry.Instance.DefectTagPK.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public bool AlwaysViewWorkflowManagementTab
		{
			get => BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.Value;
#if DEBUG
			set => BMSRegistry.Instance.AlwaysViewWorkflowManagementTab.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public int TimeBeforeDeletingOldScheduledTasks
		{
			get => BMSRegistry.Instance.TimeBeforeDeletingOldScheduledTasks.Value;
#if DEBUG
			set => BMSRegistry.Instance.TimeBeforeDeletingOldScheduledTasks.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public int TaskActionSchedulerBatchSize
		{
			get => BMSRegistry.Instance.TaskActionSchedulerBatchSize.Value;
#if DEBUG
			set => BMSRegistry.Instance.TaskActionSchedulerBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		public bool EnableMENTSections
		{
			get => BMSRegistry.Instance.EnableMENTSections.Value;
#if DEBUG
			set => BMSRegistry.Instance.EnableMENTSections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
#endif
		}

		#region Responsive PAVE Data Processing

		public bool EnableResponsivePAVEDataProcessing
		{
			get { return BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.Value; }
#if DEBUG
			set { BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#region Transfer

		public bool TransferWorkflowComponentOnChanges
		{
			get { return BMSRegistry.Instance.TransferWorkflowComponentOnChanges.Value; }
#if DEBUG
			set { BMSRegistry.Instance.TransferWorkflowComponentOnChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int TransferWorkflowComponentMaximumNumberOfCDCChanges
		{
			get { return BMSRegistry.Instance.TransferWorkflowComponentMaximumNumberOfCDCChanges.Value; }
#if DEBUG
			set { BMSRegistry.Instance.TransferWorkflowComponentMaximumNumberOfCDCChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool DynamicallyFilterTransferRules
		{
			get { return BMSRegistry.Instance.DynamicallyFilterTransferRules.Value; }
#if DEBUG
			set { BMSRegistry.Instance.DynamicallyFilterTransferRules.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool ProcessAllTransferRulesLinksOnNextBMSRun
		{
			get { return BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.Value; }
			set { BMSRegistry.Instance.ProcessAllTransferRulesLinksOnNextBMSRun.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
		}

		public bool EnableNudgingBMSServiceTaskByThePVEServiceTask
		{
			get { return BMSRegistry.Instance.EnableNudgingBMSServiceTaskByThePVEServiceTask.Value; }
#if DEBUG
			set { BMSRegistry.Instance.EnableNudgingBMSServiceTaskByThePVEServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int DelayForNudgingTheBMSServiceTaskByThePVEServiceTask
		{
			get { return BMSRegistry.Instance.DelayForNudgingTheBMSServiceTaskByThePVEServiceTask.Value; }
#if DEBUG
			set { BMSRegistry.Instance.DelayForNudgingTheBMSServiceTaskByThePVEServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Auto Assignment

		public bool AutoAssignCapabilityTasksOnChanges
		{
			get { return BMSRegistry.Instance.AutoAssignCapabilityTasksOnChanges.Value; }
#if DEBUG
			set { BMSRegistry.Instance.AutoAssignCapabilityTasksOnChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int AutoAssignCapabilityTasksMaxNumberOfCDCChanges
		{
			get { return BMSRegistry.Instance.AutoAssignCapabilityTasksMaxNumberOfCDCChanges.Value; }
#if DEBUG
			set { BMSRegistry.Instance.AutoAssignCapabilityTasksMaxNumberOfCDCChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool EnableNudgingBMTServiceTaskByThePVEServiceTask
		{
			get { return BMSRegistry.Instance.EnableNudgingBMTServiceTaskByThePVEServiceTask.Value; }
#if DEBUG
			set { BMSRegistry.Instance.EnableNudgingBMTServiceTaskByThePVEServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int DelayForNudgingTheBMTServiceTaskByThePVEServiceTask
		{
			get { return BMSRegistry.Instance.DelayForNudgingTheBMTServiceTaskByThePVEServiceTask.Value; }
#if DEBUG
			set { BMSRegistry.Instance.DelayForNudgingTheBMTServiceTaskByThePVEServiceTask.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#region Responsive Workflow Updates On BMS Changes

		public bool EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get { return BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value; }
#if DEBUG
			set { BMSRegistry.Instance.EnableResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public int ResponsiveWorkflowUpdatesBatchSize
		{
			get { return BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.Value; }
#if DEBUG
			set { BMSRegistry.Instance.ResponsiveWorkflowUpdatesBatchSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges
		{
			get { return BMSRegistry.Instance.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges.Value; }
#if DEBUG
			set { BMSRegistry.Instance.LogWorkflowInfoOnResponsiveWorkflowUpdatesOnRelatedObjectChanges.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion

		#endregion

		#region EXPERIMENTAL

		public bool EnablePaveExperimentalFeatures
		{
			get { return BMSRegistry.Instance.EnablePaveExperimentalFeatures.Value; }
#if DEBUG
			set { BMSRegistry.Instance.EnablePaveExperimentalFeatures.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		public bool IgnoreIterationsWhenCalculatingStartability
		{
			get { return BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.Value; }
#if DEBUG
			set { BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value); }
#endif
		}

		#endregion
	}
}
