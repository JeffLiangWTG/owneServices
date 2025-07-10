using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class WorkflowManagerIterationReasonsControl : RegistryZUserControl
	{
		internal ZArchitecture.ZGrid ChildGridInternal => ChildGrid;
		internal ZArchitecture.ZGrid ParentGridInternal => ParentGrid;
		internal ZDropEdit ValidationDropEditInternal => ValidationDropEdit;

		public WorkflowManagerIterationReasonsControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ParentGrid.ReadOnly = true;
			ChildGrid.ReadOnly = readOnly;
			ValidationDropEdit.ReadOnly = readOnly;
		}
	}
}
