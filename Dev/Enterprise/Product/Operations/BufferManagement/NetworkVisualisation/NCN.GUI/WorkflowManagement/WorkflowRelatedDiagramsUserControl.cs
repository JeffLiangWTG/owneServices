using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class WorkflowRelatedDiagramsUserControl : RelatedDiagramsUserControlBase
	{
		public WorkflowRelatedDiagramsUserControl(ProcessHeader workflow)
		{
			this.SetDataBinding(workflow, "");
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var selectedWorkflow = dataSource as ProcessHeader;
			if (selectedWorkflow != null)
			{
				viewModel = new WorkflowRelatedDiagramsViewModel(selectedWorkflow);
				base.SetDataBinding(viewModel, dataMember);
			}
		}

		WorkflowRelatedDiagramsViewModel viewModel;

#if DEBUG
		public
#endif
		void DiagramsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			DiagramsGrid_MouseDoubleClickBase(sender, e);
		}
	}
}
