using System;
using System.Threading;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.BufferManagement.Business
{
	[NeedsDataRefresh]
	public abstract class BMSServiceTaskBase : ServiceProviderImpl
	{
		public const string Category = "BMS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Logging")]
		public sealed override void RunTask(CancellationToken token)
		{
			if (!IsSufficientWorkflowManagementModeEnabled)
			{
				var location = BMSRegistry.Instance.WorkflowManagementMode.GetLocation();
				ServiceLogger.Log(LogType.Information, $"The registry item [{location}] must be set to a higher level to use {TaskDescription}.");
				return;
			}

			if (ShouldNotRunIfCapacityCalculationsDisabled && BMSRegistry.Instance.DisableCapacityCalculations.Value)
			{
				ServiceLogger.Debug("Service task not run because the [Disable Capacity Calculations] registry item is enabled.");
				return;
			}

			RunTaskCore(token);
		}

		protected virtual bool IsSufficientWorkflowManagementModeEnabled => BMSRegistryProvider.IsBufferManagementEnabled;

		protected int ProcessInGenericBatches(CancellationToken token, Func<int> processOneBatch, Func<int, bool> shouldProcessNextBatch)
		{
			var totalProcessedCount = 0;
			int lastBatchProcessedCount;

			do
			{
				token.ThrowIfCancellationRequested();

				lastBatchProcessedCount = processOneBatch.Invoke();
				OnBatchProcessed(lastBatchProcessedCount);
				totalProcessedCount += lastBatchProcessedCount;
			}
			while (shouldProcessNextBatch(lastBatchProcessedCount));

			return totalProcessedCount;
		}

		protected virtual void OnBatchProcessed(int lastBatchProcessedCount)
		{
		}

		protected abstract void RunTaskCore(CancellationToken token);
		protected abstract string TaskDescription { get; }

		#region Running requirements

		protected virtual bool ShouldNotRunIfCapacityCalculationsDisabled => false;

		protected static string CheckCapacityCalculationsNotDisabledCore()
		{
			return HostedServiceRequirementAttribute.CheckValueIsNotEqualTo(BMSRegistry.Instance.DisableCapacityCalculations, true);
		}

		protected static string CheckPlanningManagementEnabled()
		{
			return HostedServiceRequirementAttribute.CheckValueIsEqualTo(BMSRegistry.Instance.WorkflowManagementMode, WorkflowManagementModes.Codes.PlanningManagement);
		}

		protected static string CheckBufferManagementWorkflowModeOrBetterEnabledCore()
		{
			return HostedServiceRequirementAttribute.CheckValueIsAmong(
				BMSRegistry.Instance.WorkflowManagementMode,
				WorkflowManagementModes.Codes.IncludesBufferManagement,
				WorkflowManagementModes.Codes.PlanningManagement);
		}

		protected static string CheckEnhancedWorkflowModeOrBetterEnabledCore()
		{
			return HostedServiceRequirementAttribute.CheckValueIsAmong(
				BMSRegistry.Instance.WorkflowManagementMode,
				WorkflowManagementModes.Codes.EnhancedWorkflow,
				WorkflowManagementModes.Codes.IncludesBufferManagement,
				WorkflowManagementModes.Codes.PlanningManagement);
		}

		#endregion
	}
}
