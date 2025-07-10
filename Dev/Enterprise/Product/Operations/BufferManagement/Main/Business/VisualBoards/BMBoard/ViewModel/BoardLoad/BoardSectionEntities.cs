using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionEntities
	{
		public static BoardSectionEntities ForStandardRefresh(ICollection<ProcessTask> tasksShownOnSection, ICollection<ProcessHeader> workflowsShownOnSection, ICollection<ProcessHeader> workflowsShownOnSectionAndTheirAncestors)
		{
			Argument.NotNull(tasksShownOnSection, nameof(tasksShownOnSection));
			Argument.NotNull(workflowsShownOnSection, nameof(workflowsShownOnSection));
			Argument.NotNull(workflowsShownOnSectionAndTheirAncestors, nameof(workflowsShownOnSectionAndTheirAncestors));

			return new BoardSectionEntities(tasksShownOnSection, workflowsShownOnSection, workflowsShownOnSectionAndTheirAncestors);
		}

		public static BoardSectionEntities ForPartialRefresh(ICollection<ProcessTask> tasksShownOnSection, ICollection<ProcessHeader> workflowsShownOnSection)
		{
			Argument.NotNull(workflowsShownOnSection, nameof(workflowsShownOnSection));

			return new BoardSectionEntities(tasksShownOnSection, workflowsShownOnSection, GetWorkflowsWithAncestors(workflowsShownOnSection));
		}

#if DEBUG
		public static BoardSectionEntities ForTest(ICollection<ProcessHeader> workflowsShownOnSection)
		{
			Argument.NotNull(workflowsShownOnSection, nameof(workflowsShownOnSection));

			var distinctWorkflows = workflowsShownOnSection.Distinct().ToArray();
			var tasks = distinctWorkflows.SelectMany(x => x.GetTasksWithoutAccessingWorkflowParent()).ToArray();

			return new BoardSectionEntities(tasks, distinctWorkflows, GetWorkflowsWithAncestors(workflowsShownOnSection));
		}
#endif

		internal static BoardSectionEntities Empty => new BoardSectionEntities(System.Array.Empty<ProcessTask>(), System.Array.Empty<ProcessHeader>(), System.Array.Empty<ProcessHeader>());

		BoardSectionEntities(ICollection<ProcessTask> tasksShownOnSection, ICollection<ProcessHeader> workflowsShownOnSection, ICollection<ProcessHeader> workflowsShownOnSectionAndTheirAncestors)
		{
			TasksShownOnSection = tasksShownOnSection;
			WorkflowsShownOnSection = workflowsShownOnSection;
			WorkflowsShownOnSectionAndTheirAncestors = workflowsShownOnSectionAndTheirAncestors;
		}

		internal ICollection<ProcessTask> TasksShownOnSection { get; }
		internal ICollection<ProcessHeader> WorkflowsShownOnSection { get; }
		internal ICollection<ProcessHeader> WorkflowsShownOnSectionAndTheirAncestors { get; }

		static ICollection<ProcessHeader> GetWorkflowsWithAncestors(ICollection<ProcessHeader> nonDistinctWorkflows)
		{
			var distinctWorkflows = nonDistinctWorkflows.Distinct().ToArray();
			return distinctWorkflows.SelectDistinctRecursive(w => w.ParentHeaders.ToArray()).Concat(distinctWorkflows).ToArray();
		}
	}
}
