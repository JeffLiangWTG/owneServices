using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowRelationshipsForm : ZEditForm
	{
		public WorkflowRelationshipsForm(ProcessHeader workflow)
			: base(new ProcessHeaderRelationshipsViewModel(workflow.Factory))
		{
			InitializeComponent();
			var viewModel = (ProcessHeaderRelationshipsViewModel)base.DataSource;
			workflowRelationshipsUserControl.SetDataBinding(workflow, viewModel);
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override string FormVerb => string.Empty;
	}
}
