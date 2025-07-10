using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class IProcessTaskExtensions
	{
		#region ProcessTask

		[DebuggerStepThrough]
		public static ProcessHeader GetProcessHeader(this IProcessTask processTask)
		{
			return (ProcessHeader)((ProcessTask)processTask).ProcessHeader;
		}

		public static ProcessHeader GetProcessHeaderForCardType(this IProcessTask processTask, bool showJobCards)
		{
			var processHeader = processTask.GetProcessHeader();

			return showJobCards ? processHeader.JobHeader : processHeader;
		}

		public static bool IsStartable(this IProcessTask task, PropertyCache cache)
		{
			return cache.GetCachedValue(task.PK, TaskJobWorkflowCacheHelper.CacheConstants.IsCurrent, task.IsStartable);
		}

		public static bool IsStartable(this IProcessTask processTask)
		{
			var service = ((IBusiness)processTask).Factory.ServiceContainer.GetService<TaskStartabilityService>();

			bool CalculateIsStartable(IProcessTask task) => IsStartable_ExcludingReplenishment(task);

			if (service != null)
			{
				return service.IsStartable(processTask, CalculateIsStartable);
			}

			return CalculateIsStartable(processTask);
		}

		public static bool IsStartable_ExcludingReplenishment(this IProcessTask processTask)
		{
			return processTask.IsStartable_ExcludingReplenishment(null);
		}

		internal static bool IsStartable_ExcludingReplenishment(this IProcessTask processTask, ICollection<IProcessTaskIterationLink> openIterations)
		{
			var task = (ProcessTask)processTask;
			var workflow = task.GetProcessHeader();

			if (workflow != null)
			{
				var allTasksInWorkflow = workflow.GetTasksWithoutAccessingWorkflowParent();
				var isTaskCurrent = ProcessTaskCollection.GetCurrentTasks(workflow, allTasksInWorkflow).Contains(task) && !workflow.HasOpenPrerequisites;

				if (isTaskCurrent)
				{
					return !PrerequisiteQualityIterationWorkflowsExist(task, allTasksInWorkflow, openIterations);
				}
			}

			return false;
		}

		static bool PrerequisiteQualityIterationWorkflowsExist(ProcessTask task, IEnumerable<ProcessTask> allTasksInWorkflow, ICollection<IProcessTaskIterationLink> openIterations)
		{
			if (BMSRegistry.Instance.IgnoreIterationsWhenCalculatingStartability.Value)
			{
				return false;
			}

			if (openIterations != null)
			{
				var tasksWithOpenIteration =
					from earlierTask in allTasksInWorkflow
					where earlierTask.P9_Sequence < task.P9_Sequence
					where openIterations.Any(l => l.P9I_P9_ContainmentBarrierTask == earlierTask.PK && l.P9I_FH_IterationWorkflow != earlierTask.P9_FH_ProcessHeader)
					select earlierTask;

				return tasksWithOpenIteration.Any();
			}
			else
			{
				var pivotQuery = GetOpenIterationLinksQuery(task, allTasksInWorkflow);
				var containmentBarrierPivots = task.Factory.Load<IProcessTaskIterationLink>(pivotQuery);
				var workflowCodes = containmentBarrierPivots.Select(p => p.P9I_FH_IterationWorkflow).Where(p => p.IsValid).ToArray();

				if (workflowCodes.Length > 0)
				{
					var workflowQuery = new ZQuery(ProcessHeaderSchema.PK, workflowCodes);
					workflowQuery.AddToFilter(ProcessHeaderSchema.FH_Status, SQLComparisonOperator.NotEqual, WorkflowStatusList.Codes.Closed);

					return task.Factory.Exists(typeof(ProcessHeader), workflowQuery, mergeDbAndCacheResult: false);
				}

				return false;
			}
		}

		internal static ZQuery GetOpenIterationLinksQuery(ProcessTask task, IEnumerable<ProcessTask> allTasksInWorkflow)
		{
			var previousTasks = allTasksInWorkflow.Where(t => t.P9_Sequence < task.P9_Sequence);
			var pivotQuery = new ZQuery { AllowTableValuedParameters = true };
			pivotQuery.AddToFilter(ProcessTaskIterationLinkSchema.P9I_P9_ContainmentBarrierTask, previousTasks.Select(t => t.PK));
			pivotQuery.AddToFilter(ProcessTaskIterationLinkSchema.P9I_LinkType, IterationLinkTypeList.Codes.QualityIterationTask);
			pivotQuery.AddToFilter(ProcessTaskIterationLinkSchema.P9I_FH_IterationWorkflow, SQLComparisonOperator.NotEqual, task.P9_FH_ProcessHeader);

			return pivotQuery;
		}

		public static bool IsInWorkflowWithCCR(this IProcessTask processTask, CCRConstraintFilter ccrConstraintFilter = CCRConstraintFilter.None)
		{
			var originalTask = (ProcessTask)processTask;
			var workflow = originalTask.GetProcessHeader();
			var currentComponent = workflow.CurrentComponent;

			var isCCRWorkflow = false;
			if (currentComponent != null && currentComponent.IsBuffer)
			{
				var tasks = GetTasksNotAccessingWorkflowParent(processTask, ccrConstraintFilter);
				var followingTasksStaff =
					from t in tasks
					let staff = t.AssignedStaffMember
					where staff != null && staff != originalTask.AssignedStaffMember
					select staff;

				isCCRWorkflow = followingTasksStaff.Any(s => ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(s, currentComponent))
					|| (ccrConstraintFilter == CCRConstraintFilter.None && originalTask.AssignedStaffMember != null && ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(originalTask.AssignedStaffMember, currentComponent));
			}

			return isCCRWorkflow;
		}

		public static int GetPrimaryAxis(this BMComponentSectionConfiguration sectionConfiguration, int row, int col)
		{
			return sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical ? col : row;
		}

		public static int GetSecondaryAxis(this BMComponentSectionConfiguration sectionConfiguration, int row, int col)
		{
			return sectionConfiguration.OrientationValue == BMBoardSectionOrientation.Vertical ? row : col;
		}

		public static IEnumerable<ZGuid> IsInStaffsWorkflow(this IProcessTask processTask, ZGuid[] staffPKs, CCRConstraintFilter ccrConstraintFilter = CCRConstraintFilter.None)
		{
			var originalTask = (ProcessTask)processTask;
			var currentComponent = originalTask.GetProcessHeader().CurrentComponent;

			IEnumerable<ZGuid> followingTasksStaff = null;
			if (currentComponent != null && currentComponent.IsBuffer)
			{
				var tasks = GetTasksNotAccessingWorkflowParent(processTask, ccrConstraintFilter);
				followingTasksStaff =
					from t in tasks
					let staff = t.AssignedStaffMember
					where staff != null && staffPKs.Contains(staff.PK)
					select staff.PK;
			}

			return followingTasksStaff;
		}

		static IEnumerable<ProcessTask> GetTasksNotAccessingWorkflowParent(IProcessTask processTask, CCRConstraintFilter ccrConstraintFilter)
		{
			var task = (ProcessTask)processTask;
			var workflow = task.GetProcessHeader();

			return
				from t in workflow.GetTasksWithoutAccessingWorkflowParent()
				orderby t.P9_Sequence
				where (ccrConstraintFilter == CCRConstraintFilter.None) ||
					(ccrConstraintFilter == CCRConstraintFilter.Preconstraint && t.P9_Sequence >= task.P9_Sequence) ||
					(ccrConstraintFilter == CCRConstraintFilter.Postconstraint && t.P9_Sequence < task.P9_Sequence)
				select t;
		}

		public static ZDecimal GetEffectiveNudge(this IProcessTask task, bool showJobCards)
		{
			var processHeader = showJobCards ? task.GetProcessHeader()?.JobHeader : task.GetProcessHeader();
			return processHeader != null ? processHeader.EffectiveNudge : ZDecimal.Zero;
		}

		public static ZDecimal GetEffectiveNudge(this IProcessTask task, BMBoardSectionViewModel viewModel)
		{
			return GetEffectiveNudge(task, viewModel.ShowJobCards);
		}

		public static bool CanTaskBeShownInResourceChannel(this ProcessTask task, GlbStaff resource)
		{
			if (string.Equals(task.P9_GS_NKAssignedStaffMember, resource.GS_Code, StringComparison.CurrentCultureIgnoreCase))
			{
				return true;
			}

			return task.RequiresResourceWithCapability && resource.Capabilities.Any(c => c.PK == task.P9_G4_RequiredCapability);
		}

		public static IEnumerable<BMComponent> GetBuffers(this BMComponent parent, ConstraintStatus constraint)
		{
			return parent.ChildComponents.Where(c => c != parent && c.FC_Type == BMComponentTypeList.Codes.Buffer && ConstrainedModeHelper.GetConstraintStatus(c) == constraint);
		}

		#endregion
	}
}
