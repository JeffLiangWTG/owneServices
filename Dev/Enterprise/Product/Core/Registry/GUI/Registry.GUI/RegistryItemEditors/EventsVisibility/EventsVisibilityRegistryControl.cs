using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class EventsVisibilityRegistryControl : RegistryZUserControl
	{
		public EventsVisibilityRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			EventsVisibilityGrid.ReadOnly = readOnly;
		}

#if DEBUG
		public ZGrid EventsVisibilityGridForTest
		{
			get { return EventsVisibilityGrid; }
		}
#endif
	}
}
