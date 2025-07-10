using System.Collections.Generic;
using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.BufferManagement.Business
{
	sealed class TicketReassignmentParameters
	{
		internal TicketReassignmentParameters(BMBoardSectionViewModel destinationSectionViewModel, ICardContent cardContent, ProcessHeader workflow, ProcessTask task, CellContent sourceCell, CellContent destinationCell, IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> userNotification, BMBoardSectionViewModel sourceSectionViewModel)
		{
			DestinationSectionViewModel = destinationSectionViewModel;
			CardContent = cardContent;
			Workflow = workflow;
			Task = task;
			SourceCell = sourceCell;
			DestinationCell = destinationCell;
			UserNotification = userNotification;
			SourceSectionViewModel = sourceSectionViewModel;
		}

		internal BMBoardSectionViewModel SourceSectionViewModel { get; }
		internal BMBoardSectionViewModel DestinationSectionViewModel { get; }
		internal ICardContent CardContent { get; }
		internal ProcessHeader Workflow { get; }
		internal ProcessTask Task { get; }
		internal IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> UserNotification { get; }

		internal CellContent SourceCell { get; }
		internal CellContent DestinationCell { get; }

		internal IVisualBoardChannel SourceChannel => SourceCell.Channel;
		internal IVisualBoardChannel DestinationChannel => DestinationCell.Channel;

		internal bool IsMovingWithinChannelToDifferentTimeSlot
		{
			get
			{
				return DestinationSectionViewModel.ComponentPK == SourceSectionViewModel.ComponentPK &&
					SourceChannel == DestinationChannel
					&& SourceCell.Coordinates != DestinationCell.Coordinates
					&& ValidTimeProgressionFieldsForDragDrop.Contains(DestinationSectionViewModel.TimeProgressionField);
			}
		}

		static IEnumerable<string> ValidTimeProgressionFieldsForDragDrop
		{
			get
			{
				yield return TimeProgressionFieldList.Codes.AgreedDeliveryDate;
				yield return TimeProgressionFieldList.Codes.DoNotStartBeforeDate;
				yield return TimeProgressionFieldList.Codes.TransferTime;
			}
		}

		internal bool ShowJobCards
		{
			get { return DestinationSectionViewModel.ShowJobCards || SourceSectionViewModel.ShowJobCards; }
		}

		internal IEnumerable<ProcessHeader> WorkflowsAffectedBySuccessfulReassignment
		{
			get
			{
				if (Workflow != null)
				{
					yield return Workflow;

					foreach (var childWorkflow in Workflow.GetChildWorkflowsDownTheHierarchy())
					{
						if (childWorkflow.FH_FC_CurrentComponent == Workflow.FH_FC_CurrentComponent && childWorkflow.IsInSameJob(Workflow))
						{
							yield return childWorkflow;
						}
					}
				}
			}
		}
	}
}
