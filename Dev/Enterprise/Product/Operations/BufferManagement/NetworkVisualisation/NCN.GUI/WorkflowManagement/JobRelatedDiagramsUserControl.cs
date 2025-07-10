using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class JobRelatedDiagramsUserControl : RelatedDiagramsUserControlBase, IJobRelatedDiagramsUserControl
	{
		public JobRelatedDiagramsUserControl()
		{
			InitializeComponent();
			DiagramsGrid.ReadOnly = true;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var workflowProvider = dataSource as IWorkflowProviderCore;
			var bizo = dataSource as BusinessObject;

			if (workflowProvider == null || bizo == null)
			{
				return;
			}

			var jobHeader = ObjectFactory.Get<IProcessJobHeaderProvider>().GetForParent(workflowProvider, bizo.Factory) as ProcessJobHeader;

			if (jobHeader != null)
			{
				var viewModel = new JobRelatedDiagramsViewModel(jobHeader);
				base.SetDataBinding(viewModel, dataMember);
			}
		}

#if DEBUG
		public
#endif
		void DiagramsGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			DiagramsGrid_MouseDoubleClickBase(sender, e);
		}
	}
}
