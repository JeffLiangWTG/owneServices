using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	public partial class JobRelatedParentWorkflowsControl : JobRelatedWorkflowsControlBase, IJobRelatedParentChildWorkflowsControl
	{
		public JobRelatedParentWorkflowsControl()
		{
			InitializeComponent();
			ParentWorkflowGrid.ReadOnly = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			SetDataBindingCore(dataSource, RelationshipDirection.From);
		}

		void ParentWorkflowGrid_DoubleClick(object sender, System.EventArgs e)
		{
			GridMouseDoubleClickCore(sender, link => link.HeaderTo);
		}

		void AddButton_Click(object sender, System.EventArgs e)
		{
			ChooseProcessHeadersAndAddLinks(selectedProcessHeaders => viewModel.AddLinks(selectedProcessHeaders));
		}

		void RemoveButton_Click(object sender, System.EventArgs e)
		{
			var selectedLinks = ParentWorkflowGrid.SelectedElements.Cast<ProcessHeaderLink>().ToArray();
			viewModel.RemoveLinks(selectedLinks);
		}
	}
}
