using Enterprise.Registry.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	public partial class MexicoNotificationRemainingFolioConfigurationControl : RegistryZUserControl
	{
		public MexicoNotificationRemainingFolioConfigurationControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			FoliosQuantityCalcEdit.ReadOnly = readOnly;
			IntervalCalcEdit.ReadOnly = readOnly;
		}
	}
}
