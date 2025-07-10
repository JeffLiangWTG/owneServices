using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public interface IBizoCardContent : ICardContent
	{
		ProcessTask Task { get; }
		ProcessHeader Workflow { get; }
	}

	public interface ICardContent : ICardContentBase
	{
		SizedButtonBorderStyle BorderStyle { get; }
		TagDefinitionCache Definitions { get; }
		ImmutableHashSet<ZGuid> ApplicableTagMagnitudes { get; }
		TValue GetCustomAttribute<TValue>(StaticControlProperty key);

		ZString WorkflowType { get; }

		object Bindable { get; }

		ITaskOrderable TaskOrderable { get; }
		ICardCapacityDto CapacityDto { get; }
	}

	public static class ICardContentExtensions
	{
		public static ProcessHeader GetWorkflow(this ICardContent content, BusinessObjectFactory factory)
		{
			return factory.Load<ProcessHeader>(content.WorkflowIdentifier);
		}

		public static ProcessTask GetTask(this ICardContent content, BusinessObjectFactory factory)
		{
			return factory.Load<ProcessTask>(content.TaskIdentifier);
		}

		public static void FetchForLoad(this IEnumerable<ICardContent> cards, BusinessObjectFactory factory)
		{
			cards.Select(c => c.WorkflowIdentifier).Distinct().ForEach(pk => factory.AddFetchHint(ProcessHeaderSchema.PK, pk));
			cards.Select(c => c.TaskIdentifier).Distinct().ForEach(pk => factory.AddFetchHint(ProcessTasksSchema.PK, pk));
		}

		public static IEnumerable<ZString> GetAssignedStaff(this ProcessHeader workflow, PropertyCache cache)
		{
			return cache.GetCachedValue(workflow.PK, BMBoardSectionViewModel.CacheConstants.AssignedStaff, () => GetAssignedStaff(workflow));
		}

		static IEnumerable<ZString> GetAssignedStaff(ProcessHeader workflow)
		{
			return workflow.GetTasksWithoutAccessingWorkflowParent()
				.Select(t => t.P9_GS_NKAssignedStaffMember)
				.Where(c => !c.IsEmpty)
				.ToArray();
		}

		[Conditional("DEBUG")]
		public static void SetDebugInformation(ICardContentBase cardContent, ProcessTask task, ProcessHeader workflow)
		{
#if DEBUG
			cardContent.DisplayTextForDebugging = string.Format(CultureInfo.InvariantCulture, "Task: [{0}]; Workflow: [{1}]", task.P9_Description, workflow?.FH_CompletionStatement ?? string.Empty);
#endif
		}
	}
}
