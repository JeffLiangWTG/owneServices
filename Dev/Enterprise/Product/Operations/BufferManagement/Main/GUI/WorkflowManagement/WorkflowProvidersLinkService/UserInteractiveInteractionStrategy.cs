using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.BufferManagement.Business.WorkflowProvidersLinkageService;

namespace Enterprise.BufferManagement.GUI
{
	class UserInteractiveInteractionStrategy : ILinkageServiceInteractionStrategy
	{
		bool ILinkageServiceInteractionStrategy.ShouldCommitProposedLinks(WorkflowProvidersLinkageService service, WorkflowProvidersLinkViewModel viewModel, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
		{
			if (viewModel.ProposedProcessHeaderLinks.Count > 0)
			{
				var context = service.CreateDialogContext(
					LinkOperationType.Addition,
					firstJobHeader, secondJobHeader,
					ResString.GetMultilingualString("8761962c-d0ee-4b3e-a751-188ea102f538", "Create Workflow Links {0}->{1}", firstJobHeader.Parent.WorkflowType, secondJobHeader.Parent.WorkflowType),
					Res.GetData("EC8AB8B0-3BA4-4D26-9030-0747D1E7BA25", "Apply to creation of all links between workflows in different jobs regardless job types"),
					ZMessageBoxIcon.None
					);

				return Globals.Message.ShowOrDefault(context, () => new WorkflowProvidersLinkageUserControl(viewModel)) == DialogResult.OK;
			}

			return false;
		}

		bool ILinkageServiceInteractionStrategy.ShouldRemoveExistingLinks(WorkflowProvidersLinkageService service, ProcessHeaderLink[] links, ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader)
		{
			var message = GetMessageForLinkRemoval(firstJobHeader, secondJobHeader, links);
			var dialogContext = service.CreateDialogContext(
				LinkOperationType.Deletion,
				firstJobHeader, secondJobHeader,
				ResString.GetMultilingualString("26c42308-ed24-4a06-9c52-f52ead2e602b", "Remove Workflow Links {0}->{1}", firstJobHeader.Parent.WorkflowType, secondJobHeader.Parent.WorkflowType),
				Res.GetData("96652183-72CA-4F3F-A80A-610D5AD0BF38", "Apply to removal of all links"),
				ZMessageBoxIcon.Information,
				ZMessageBoxButtons.YesNo
				);

			return Globals.Message.ShowOrDefault(dialogContext, message) == ZDialogResult.Yes;
		}

		static MultilingualString GetMessageForLinkRemoval(ProcessJobHeader firstJobHeader, ProcessJobHeader secondJobHeader, ProcessHeaderLink[] links)
		{
			if (links.Length == 1)
			{
				var link = links[0];
				return ResString.GetMultilingualString("9e5a90ed-43f4-4256-9e61-0f8f0444e1e7", "Would you like to remove the {0} workflow relationship between these two jobs?\r\n\r\nJob 1: {1}\r\nJob 2: {2}",
					/*0*/ link.FP_LinkType,
					/*1*/ link.HeaderFrom.Description,
					/*2*/ link.HeaderTo.Description
					);
			}
			else
			{
				return ResString.GetMultilingualString("63bc9c9c-a5cb-443e-8c16-6c99dd87364d", "There are multiple links between these two jobs. Would you like to remove all of them?\r\n\r\nJob 1: {0}\r\nJob 2: {1}",
					/*0*/ firstJobHeader.ParentJobDescription,
					/*1*/ secondJobHeader.ParentJobDescription
					);
			}
		}
	}
}
