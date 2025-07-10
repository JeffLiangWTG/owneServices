namespace Enterprise.Registry.GUI
{
	partial class EventsVisibilityOverrideRegistryControl : RegistryZUserControl
	{
		internal ZArchitecture.ZGrid ChildGrid { get; private set; }
		internal ZArchitecture.ZGrid ParentGrid { get; private set; }

		public EventsVisibilityOverrideRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			ParentGrid.ReadOnly = readOnly;
			ChildGrid.ReadOnly = readOnly;
		}
	}
}
