using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.BufferManagement.Business
{
	static class BMSRegistryServiceTaskHelper
	{
		#region EnableAndDisableServiceTasksWhenWorkflowModeChanges

		public static void EnableAndDisableServiceTasksWhenWorkflowModeChanges(string mode)
		{
			var governor = ObjectFactory.Get<IServiceManagerGovernor>();
			var allTasks = ObjectFactory.Get<IServiceManagerQuerier>()
				.GetServiceTasksByCategory(BMSServiceTaskBase.Category);

			var enableAll = mode.Equals(WorkflowManagementModes.Codes.PlanningManagement);
			var disableAll = mode.Equals(WorkflowManagementModes.Codes.BasicWorkflow);

			if (enableAll || disableAll)
			{
				foreach (var task in allTasks)
				{
					governor.SetServiceTaskIsActive(task, enableAll);
				}
			}
			else
			{
				var specificTasksToDisable = GetTasksToDisable(mode);

				foreach (var task in allTasks)
				{
					var enable = !specificTasksToDisable.Contains(task, StringComparer.InvariantCultureIgnoreCase);

					governor.SetServiceTaskIsActive(task, enable);
				}
			}
		}

		static string[] GetTasksToDisable(string mode)
		{
			switch (mode)
			{
				case WorkflowManagementModes.Codes.IncludesBufferManagement:
					return TaskCodesThatNeedPlanningManagement;
				case WorkflowManagementModes.Codes.EnhancedWorkflow:
					return TaskCodesThatNeedPlanningManagement.Concat(
						TaskCodesThatNeedBufferManagementWorkflowModeOrHigher).ToArray();
				default:
					throw new InvalidOperationException("PLN should just enable everything and BWF disable everything, no need to call this method for those modes.");
			}
		}

		static string[] TaskCodesThatNeedPlanningManagement => new[]
		{
			BMConstants.BufferPenetrationUpdaterServiceTaskCode,
		};

		static string[] TaskCodesThatNeedBufferManagementWorkflowModeOrHigher => new[]
		{
			BMConstants.AgedScoresServiceTaskCode,
			BMConstants.MENTDataPurgeServiceTaskCode,
			TagServiceTask.Code,
			TagMonitorServiceTask.Code,
			ReleaseGateRunnerServiceTask.Code,
			CapacityConstrainedResourceStatusServiceTask.Code,
			StaggeredReleaseDelayCalculatorTask.Code,
			CapabilityTaskAutoAssignmentServiceTask.Code,
		};

		#endregion

		#region DisableCapacityDependentServiceTasks

		public static void DisableCapacityDependentServiceTasks()
		{
			var serviceTaskCodes = new[]
			{
				ReleaseGateRunnerServiceTask.Code,
				CapacityConstrainedResourceStatusServiceTask.Code,
				StaggeredReleaseDelayCalculatorTask.Code,
				CapabilityTaskAutoAssignmentServiceTask.Code
			};

			var governor = ObjectFactory.Get<IServiceManagerGovernor>();
			var tasks = ObjectFactory.Get<IServiceManagerQuerier>()
				.GetServiceTasksByCategory(BMSServiceTaskBase.Category)
				.Where(t => serviceTaskCodes.Contains(t));

			foreach (var task in tasks)
			{
				governor.SetServiceTaskIsActive(task, isActive: false);
			}
		}

		#endregion
	}
}
