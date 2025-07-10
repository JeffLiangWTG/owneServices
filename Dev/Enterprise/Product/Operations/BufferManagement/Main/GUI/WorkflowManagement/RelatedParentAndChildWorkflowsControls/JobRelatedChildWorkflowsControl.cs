using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.GUI
{
	public partial class JobRelatedChildWorkflowsControl : JobRelatedWorkflowsControlBase, IJobRelatedParentChildWorkflowsControl
	{
		public JobRelatedChildWorkflowsControl()
		{
			InitializeComponent();
			ChildWorkflowGrid.ReadOnly = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			SetDataBindingCore(dataSource, RelationshipDirection.To);
		}

		void ChildWorkflowGrid_DoubleClick(object sender, System.EventArgs e)
		{
			GridMouseDoubleClickCore(sender, link => link.HeaderFrom);
		}

		void AddButton_Click(object sender, System.EventArgs e)
		{
			ChooseProcessHeadersAndAddLinks(selectedProcessHeaders => viewModel.AddLinks(selectedProcessHeaders));
		}

		void RemoveButton_Click(object sender, System.EventArgs e)
		{
			var selectedLinks = ChildWorkflowGrid.SelectedElements.Cast<ProcessHeaderLink>().ToArray();
			viewModel.RemoveLinks(selectedLinks);
		}
	}
}
