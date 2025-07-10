using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowCategoriesControl : RegistryZUserControl
	{
		ZGroupBox ParentGroupBox;
		ZGroupBox ChildGroupBox;
		public ZArchitecture.ZGrid ChildGrid;
		public ZArchitecture.ZGrid ParentGrid;

		public WorkflowCategoriesControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ParentGrid.ReadOnly = true;
			ChildGrid.ReadOnly = readOnly;
		}
	}
}
