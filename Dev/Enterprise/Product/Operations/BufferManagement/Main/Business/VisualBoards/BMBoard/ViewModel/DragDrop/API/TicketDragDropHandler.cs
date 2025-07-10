using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class TicketDragDropHandler
	{
		public TicketDragDropHandler(BusinessObjectFactory factory, BMBoardSectionViewModel sectionViewModel, CellContent sourceCell, CellContent destinationCell, ICardContent cardContent, IMultiActionButtonDialogWrapper<CrossChannelTaskAssignments> userNotification, BMBoardSectionViewModel sourceSectionViewModel)
		{
			var task = cardContent.GetTask(factory);
			var workflow = GetWorkflowToDragDrop(task?.GetProcessHeader());

			parameters = new TicketReassignmentParameters(sectionViewModel, cardContent, workflow, task, sourceCell, destinationCell, userNotification, sourceSectionViewModel);
		}

		readonly TicketReassignmentParameters parameters;

		public ReadOnlyCollection<ProcessHeader> WorkflowsAffected { get; private set; } = new ReadOnlyCollection<ProcessHeader>(System.Array.Empty<ProcessHeader>());

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "This is a view model, there is going to be a GUI attached")]
		public bool TryMoveToDestinationCell()
		{
			var result = false;
			var workflow = parameters.Workflow;

			if (workflow != null)
			{
				var destinationComponent = workflow.Factory.Load<BMComponent>(parameters.DestinationSectionViewModel.ComponentPK);

				if (parameters.ShowJobCards)
				{
					if (destinationComponent != null && parameters.SourceCell != parameters.DestinationCell && !parameters.IsMovingWithinChannelToDifferentTimeSlot)
					{
						if (parameters.SourceSectionViewModel == parameters.DestinationSectionViewModel && parameters.SourceChannel != parameters.DestinationChannel)
						{
							var message = Res.GetString("73ef2a8d-851a-4fb7-9d17-73ead4d95dcb", "Drag/drop between channels is not supported for job-level workflow tickets.");
							Globals.Message.Show(message); // This is a view model, there is going to be a GUI attached
						}
						else
						{
							var jobHeader = workflow.JobHeader;
							var moveResult = jobHeader.MoveJobToComponent(parameters.SourceSectionViewModel.AllShownComponentPKs.ToArray(), destinationComponent);
							result = moveResult.Item1 > 0;

							WorkflowsAffected = new ReadOnlyCollection<ProcessHeader>(moveResult.Item2.Cast<ProcessHeader>().ToList());
						}
					}
				}
				else
				{
					var draggedIntoAnotherComponent = !parameters.DestinationSectionViewModel.AllShownComponentPKs.Contains(workflow.FH_FC_CurrentComponent);

					if (draggedIntoAnotherComponent && destinationComponent != null && workflow.MoveToComponent(destinationComponent))
					{
						result = true;
					}
				}

				if (!result && parameters.Task != null && workflow != null)
				{
					result = TicketReassignmentStrategy.GetStrategy(parameters.DestinationChannel, parameters).TryReassign();
				}

				if (result)
				{
					WorkflowsAffected = new ReadOnlyCollection<ProcessHeader>(parameters.WorkflowsAffectedBySuccessfulReassignment.ToList());
				}
			}

			return result;
		}

		static ProcessHeader GetWorkflowToDragDrop(ProcessHeader proposedWorkflow)
		{
			if (proposedWorkflow == null)
			{
				return null;
			}
			else if (proposedWorkflow.WorkflowParentLink == null)
			{
				return proposedWorkflow;
			}
			else
			{
				var resultWorkflow = proposedWorkflow;
				ProcessHeaderLink linkToParent = null;

				while ((linkToParent = resultWorkflow.WorkflowParentLink) != null)
				{
					if (linkToParent.FP_SynchroniseBufferPenetration && linkToParent.HeaderTo != null)
					{
						resultWorkflow = linkToParent.HeaderTo;
					}
					else
					{
						break;
					}
				}

				return resultWorkflow;
			}
		}
	}
}
