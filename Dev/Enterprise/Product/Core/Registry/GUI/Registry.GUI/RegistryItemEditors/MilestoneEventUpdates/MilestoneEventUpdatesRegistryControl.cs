using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class MilestoneEventUpdatesRegistryControl : RegistryZUserControl
	{
		public MilestoneEventUpdatesRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			MilestoneEventUpdatesGrid.ReadOnly = readOnly;
		}

#if DEBUG
		public ZGrid MilestoneEventUpdatesGridForTest
		{
			get { return MilestoneEventUpdatesGrid; }
		}
#endif
	}
}
