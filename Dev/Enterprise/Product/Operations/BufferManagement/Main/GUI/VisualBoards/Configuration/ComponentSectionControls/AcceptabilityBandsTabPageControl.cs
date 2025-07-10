using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AcceptabilityBandsTabPageControl : ZUserControl
	{
		public AcceptabilityBandsTabPageControl()
		{
			InitializeComponent();

			Grid.Visible = ObjectFactory.Get<IBMSRegistry>().IsBufferManagementWorkflowModeOrBetterEnabled;
			BandsHintLabel.Visible = Grid.Visible;
			NotAvailableLabel.Visible = !Grid.Visible;
			NotAvailableLabel.Text = Res.GetString("ada0eb71-4064-4d8c-b29e-81e5eb77a106", "Acceptability bands are not available when the registry item [{0}] is set to {1} - {2}.",
				BMSRegistry.Instance.WorkflowManagementMode.GetLocation(), WorkflowManagementModes.Codes.EnhancedWorkflow, WorkflowManagementModes.Descriptions.EnhancedWorkflow);
		}
	}
}
