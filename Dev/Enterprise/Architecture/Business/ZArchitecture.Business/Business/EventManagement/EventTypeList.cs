using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business
{
	public static class EventTypeListProvider
	{
		public static CodeDescriptionPairList CreateDefaultEventTypeList()
		{
			var defaultTypes = new CodeDescriptionPairList();

			var set = Events.All.Cast<Event>()
				.Where(e => AllowedTriggerEvents.IsAllowedEventType(e))
				.Select(e => new CodeDescriptionPair(e.Code, e.MultilingualDescription)).ToArray();
			defaultTypes.AddRange(set);
			defaultTypes.SortByDescription();

			return defaultTypes;
		}

		public static CodeDescriptionPairList CreateMilestoneEventTypeList(IWorkflowItem workflowItem)
		{
			return CreateMilestoneEventTypeList(workflowItem.WorkflowItemType, (workflowItem as IBaseTrigger)?.TriggerEventCode, workflowItem.HasTriggerFired(), workflowItem.Factory);
		}

		public static CodeDescriptionPairList CreateMilestoneEventTypeList(string workflowItemType, string existingMilestoneType, bool hasTriggerFired, BusinessObjectFactory factory = null)
		{
			var result = CreateDefaultEventTypeList();

			var isMilestone = workflowItemType == Constants.Workflow.MilestoneType;
			var isWorkflowTrigger = workflowItemType == Constants.Workflow.WorkflowTriggerType;
			var isException = workflowItemType == Constants.Workflow.ExceptionType;

			if (isMilestone || isWorkflowTrigger || isException)
			{
				InsertInSortOrderByDescription(result, isWorkflowTrigger);

				var inactiveMilestoneEvent = Events.InactiveEvents.FirstOrDefault(e => e.Code == existingMilestoneType);
				if (hasTriggerFired && inactiveMilestoneEvent != null)
				{
					result.InsertInSortOrderByDescription(inactiveMilestoneEvent);
				}
			}

			// Customizable events should be at the end of the list
			var customizables = factory != null
				? factory.GetCachedValue(nameof(StmCustomizableEventCodeDescriptionPairList), () => new StmCustomizableEventCodeDescriptionPairList(factory))
				: new StmCustomizableEventCodeDescriptionPairList(new BusinessObjectFactory { NameForDebugging = nameof(EventTypeListProvider) });

			result.AddRange(customizables);

			return result;
		}

		public static void InsertInSortOrderByDescription(CodeDescriptionPairList result, bool isEditedARecordAndExceptionRaised)
		{
			result.InsertInSortOrderByDescription(Events.AddedARecordToTheSystem);

			if (isEditedARecordAndExceptionRaised)
			{
				result.InsertInSortOrderByDescription(Events.EditedARecord);
			}
		}
	}
}
