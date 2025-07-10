using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	public partial class DeliveryModeRegistryControl : RegistryZUserControl
	{
		public DeliveryModeRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			DeliveryModeGrid.ReadOnly = readOnly;
		}

#if DEBUG
		internal ZGrid DeliveryModeGridForTest
		{
			get { return DeliveryModeGrid; }
		}
#endif
	}
}
