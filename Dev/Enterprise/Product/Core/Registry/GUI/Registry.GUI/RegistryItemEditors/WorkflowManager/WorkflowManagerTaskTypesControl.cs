namespace Enterprise.Registry.GUI
{
	partial class WorkflowManagerTaskTypesControl : RegistryZUserControl
	{
		internal ZArchitecture.ZGrid ChildGridInternal => ChildGrid;
		internal ZArchitecture.ZGrid ParentGridInternal => ParentGrid;

		public WorkflowManagerTaskTypesControl()
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
